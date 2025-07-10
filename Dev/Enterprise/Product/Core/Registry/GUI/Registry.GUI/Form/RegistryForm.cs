using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using Enterprise.ZArchitecture.Environment.Registry;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	interface IRegistryForm
	{
		void UpdateHasChanges();
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Reverting Code")]
	public partial class RegistryForm : ZChildForm, IRegistryForm, IRegistryItemVisibility
	{
		#region Fields

		protected ZMenuItem NewEditMenuItem;

		protected Hashtable SelectedFallbacksHash;
		protected FallbackTreeNode LastSelectedNodeKey;

		protected string DefaultHint => Res.GetString("5f2176a4-cdca-4a9a-ac21-5ce41a464bbe", "Please select a Registry item.");
		protected string VisibleToSupportOnlyText => (NoResString)"Visible only to Support";
		protected string EditableByHostedSupportOnlyText => (NoResString)"This registry is only editable by Support on Hosted Systems";
		protected string VisibleToDevelopersOnlyText => (NoResString)"Visible only to Developers";
		protected string ApplicableOnlyTo_eAdaptorNextText => (NoResString)"Applicable only when using eAdaptor Next.";

		protected RegistryFormTreeViewBuilder Builder;
		protected IList<RegistryItemTag> ChangedItems;
		RegistryItemTag CurrentRegistryItem;
		bool isReadOnly;
		protected Hashtable ColouredNodes;
		protected bool PluginControlEnteredButLeaveEventNotFired;
		GlbCompanyCollection companiesForFilteringRegistryItems;
		DynamicBusinessObjectCollection joinedCountryCompanyBranchCollection;

		readonly IRegistry registry;

		public bool HasChanges
		{
			get { return hasChanges; }
			protected set
			{
				hasChanges = value;
				this.SaveButton.Enabled = hasChanges;

				if (PluginBusinessObject != null && PluginBusinessObject.HasChanges != value)
				{
					PluginBusinessObject.HasChanges = value;
				}
			}
		}
		bool hasChanges;

		#endregion

		public RegistryForm()
			: this(ObjectFactory.Get<IRegistryProvider>())
		{
		}

		internal RegistryForm(IRegistryProvider provider)
			: base(new RegistryFormManager())
		{
			this.isReadOnly = !Env.Security.SystemRegistryEdit.IsAllowed;
			registry = provider.CreateRegistry(this);

			InitializeComponent();
			InitialiseProperties();
			SetupForm();
			Hotkeys.RegisterHotKey(Keys.Control | Keys.S, SaveButton.PerformClick, Res.GetString("B14EF4EE-0F4F-4184-B3AA-2A91C519D13F", "Save"));
			this.SaveButton.Enabled = false;

#if DEBUG
			TypeDescriptor.AddAttributes(NewFileMenuItem, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(HideMenuItem, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(CloseMenuItem, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(NewEditMenuItem, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(SupportLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			Manager.OverrideDefaultInfo.ValueChanged += OverrideCheckBox_CheckedChanged;
		}

		internal RegistryFormManager Manager
		{
			get { return (RegistryFormManager)BusinessEntity; }
		}

		public bool IsFormReadOnly
		{
			get { return isReadOnly; }
		}

		#region ZForm Overrides

		protected override bool ShouldRememberPositionAndSize => false;

		protected override void OnClosing(CancelEventArgs e)
		{
			if (HasChanges)
			{
				switch (GetCloseDialogResult())
				{
					case DialogResult.Cancel:
						e.Cancel = true;
						break;

					case DialogResult.Yes:
						bool noErrors = CheckForErrors(true);
						if (noErrors)
						{
							PerformSave();
						}
						else
						{
							e.Cancel = true;
						}

						break;

					case DialogResult.No:
						ClearChangedItems(true);
						break;
				}
			}
		}

		public override string FormVerb
		{
			get { return Res.GetString("bbb611b6-13c3-440b-a5c9-dc944f30f144", "Edit"); }
		}

		public override string FormCaption
		{
			get { return Res.GetString("c076127f-80b4-47d9-9831-898d527b1f7e", "System Registry"); }
		}

		#endregion

		#region Implementation

		#region Suspend And Resume

		int UpdateOverrideCheckBoxSemaphore;

		bool IsUpdateOverrideCheckBoxSuspended
		{
			get { return UpdateOverrideCheckBoxSemaphore > 0; }
		}

		protected void SuspendUpdateOverrideCheckBox()
		{
			++UpdateOverrideCheckBoxSemaphore;
		}

		protected void ResumeUpdateOverrideCheckBox()
		{
			UpdateOverrideCheckBoxSemaphore = Math.Max(UpdateOverrideCheckBoxSemaphore - 1, 0);
		}

		int SetSelectedFallbackSemaphore;

		bool IsSetSelectedFallbackSuspended
		{
			get { return SetSelectedFallbackSemaphore > 0; }
		}

		protected void SuspendSetSelectedFallback()
		{
			++SetSelectedFallbackSemaphore;
		}

		protected void ResumeSetSelectedFallback()
		{
			SetSelectedFallbackSemaphore = Math.Max(SetSelectedFallbackSemaphore - 1, 0);
		}

		#endregion

		bool IRegistryItemVisibility.IsVisible(IRegistryItem registryItem)
		{
			return new RegistryItemTag(registryItem).CheckItemIsVisible(companiesForFilteringRegistryItems, this);
		}

		bool IRegistryItemVisibility.IsVisible(Guid companyPk, Guid branchPk, IEnumerable<Guid> countryFilterPKs)
		{
			return RegistryLoadingHelper.IsVisible(companyPk, branchPk, countryFilterPKs, joinedCountryCompanyBranchCollection);
		}

		void InitialiseProperties()
		{
			Factory = new BusinessObjectFactory();
			companiesForFilteringRegistryItems = RegistryItemTreeViewBuilder.GetCompanies(Factory, false);
			joinedCountryCompanyBranchCollection = RegistryLoadingHelper.GetJoinedCountryCompanyBranchCollection(Factory);
			Builder = new RegistryFormTreeViewBuilder(Factory, companiesForFilteringRegistryItems, true);
			LastSelectedNodeKey = new FallbackTreeNode("DoesNotExistInRegistry/LastSelectedNode");
			SelectedFallbacksHash = new Hashtable();
			ChangedItems = new List<RegistryItemTag>();
			ColouredNodes = new Hashtable();
			CancelButton = CloseButton;

			if (Env.CurrentUser != null && Env.CurrentUser.IsDeveloperLogin)
			{
				NameOnDbTextBox.Visible = true;
			}
		}

		void SetupForm()
		{
			SaveButton.Visible = !isReadOnly;
			findOverridesMenuItem.Enabled = !isReadOnly;
			importOverridesMenuItem.Enabled = !isReadOnly;
			HideMenuItem.Checked = true;
			HintLabel.Text = DefaultHint;
			HintLabel.Font = OFont.GetFont();
			FillRegistriesTreeView();
			UpdatePluginPanel(false);
			AddEditMenuItems();
			ServiceRequestMenuItem.AddServiceRequestMenuItem(NewHelpMenuItem, this);
		}

		void AddEditMenuItems()
		{
			NewEditMenuItem = new ZEditMenuItem(this);

			var findMenuItem = new ZMenuItem();
			findMenuItem.Shortcut = Shortcut.CtrlF;
			findMenuItem.Text = ResString.GetMultilingualString("cbce3ab6-2b70-4be4-9854-90d08ffa4ecb", "&Find");
			findMenuItem.Click += FindMenuItem_Click;
			NewEditMenuItem.MenuItems.Add(findMenuItem);

			this.NewMainMenu.MenuItems.Add(1, NewEditMenuItem);
		}

		protected void FillFallbackTreeView(TreeNode registryNode)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("FillFallbackTreeView"))
			{
				if (registryNode != null)
				{
					PerformFillFallbackTreeView(registryNode);
				}
			}
		}

		protected virtual void PerformFillFallbackTreeView(TreeNode registryNode)
		{
			SuspendSetSelectedFallback();
			try
			{
				FallbackTreeView.BeginUpdate();
				FallbackTreeView.Nodes.Clear();
				FallbackTreeView.Enabled = Builder.UpdateFallbackTree(FallbackTreeView, registryNode);
				FallbackTreeView.EndUpdate();
				HighlightFallbackNodes(false);
			}
			finally
			{
				ResumeSetSelectedFallback();
			}
		}

		protected virtual void FillRegistriesTreeView()
		{
			RegistriesTreeView.Initialise(registry, companiesForFilteringRegistryItems, this);
		}

		protected void SetSelectedFallback()
		{
			if (!IsSetSelectedFallbackSuspended &&
				FallbackTreeView.SelectedNode != null)
			{
				var keys = new TreeNode[2] { LastSelectedNodeKey, RegistriesTreeView.SelectedNode };

				foreach (var key in keys.Where(k => k != null))
				{
					if (SelectedFallbacksHash.Contains(key))
					{
						SelectedFallbacksHash[key] = FallbackTreeView.SelectedNode;
					}
					else
					{
						SelectedFallbacksHash.Add(key, FallbackTreeView.SelectedNode);
					}
				}
			}
		}

		/// <summary>
		/// Select a node in the fallback tree in the following order:
		/// 1. If a fallback was previously selected for the current Registry item, select that fallback.
		/// 2. Otherwise, if a fallback was previously selected for any other Registry item, select that fallback if it exists.
		/// 3. If the above two does not exist, select the System/Enterprise fallback if it exists.
		/// </summary>
		protected void GetSelectedFallback()
		{
			SuspendSetSelectedFallback();
			try
			{
				if (CurrentRegistryItem != null)
				{
					FallbackTreeNode nodeToSelect = null;

					if (SelectedFallbacksHash.Count > 0 && LastSelectedNodeKey != null)
					{
						nodeToSelect = FindNode((FallbackTreeNode)SelectedFallbacksHash[LastSelectedNodeKey]);

						if (nodeToSelect == null && RegistriesTreeView.SelectedNode != null)
						{
							nodeToSelect = FindNode((FallbackTreeNode)SelectedFallbacksHash[RegistriesTreeView.SelectedNode]);
						}
					}

					if (nodeToSelect == null &&
						((FallbackTreeNode)FallbackTreeView.Nodes[0]).Status != FallbackStatus.NotAFallback)
					{
						nodeToSelect = (FallbackTreeNode)FallbackTreeView.Nodes[0];
					}

					if (nodeToSelect != null)
					{
						FallbackTreeView.SelectedNode = nodeToSelect;
					}
					else
					{
						FallbackTreeView.SelectedNode = null;
						FallbackTreeView.CollapseAll();
					}
				}
			}
			finally
			{
				ResumeSetSelectedFallback();
			}
		}

		RegistryItemEditor GetEditor()
		{
			var fallbackNode = (FallbackTreeNode)FallbackTreeView.SelectedNode;
			var fallback = fallbackNode.GetFallbackLevel();
			return GetEditor(CurrentRegistryItem, fallback, Factory);
		}

		internal static RegistryItemEditor GetEditor(RegistryItemTag registryItemTag, FallbackLevel fallback, BusinessObjectFactory factory)
		{
			var item = registryItemTag.RegistryItem as ILicencedRegistryItem;
			if (item != null && !item.IsLicensed)
			{
				return new NotInterfaceConnectorLicencedRegistryItemEditor();
			}
			else
			{
				return RegistryItemEditorFactory.NewEditor(registryItemTag.RegistryItem, fallback, registryItemTag.DataType, registryItemTag.EditorInfo, factory);
			}
		}

		bool IsValidatingRegistryItem { get; set; }

		protected BusinessObjectFactory Factory;

		#region Handle Changed Items

		void CommitValue(Control editControl)
		{
			if (editControl != null && editControl.BindingContext != null)
			{
				foreach (var entry in editControl.BindingContext.Cast<DictionaryEntry>().ToArray())
				{
					WeakReference reference = (WeakReference)entry.Value;
					if (reference != null)
					{
						BindingManagerBase bind = (BindingManagerBase)reference.Target;
						if (bind != null)
						{
							typeof(BindingManagerBase).GetMethod("PullData", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(bind, null);
							typeof(BindingManagerBase).GetMethod("PushData", BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null).Invoke(bind, null);//Will set the new notifications to control
						}
					}
				}
			}
		}

		protected void UpdateChangedItems(bool hasValue, object newValue)
		{
			HasChanges = true;
			CurrentRegistryItem.HasValue = hasValue;
			CurrentRegistryItem.NewValue = hasValue ? newValue : GetFallbackAccessor().GetCurrentValue().Value;
			CurrentRegistryItem.IsChanged = true;
			if (!ChangedItems.Contains(CurrentRegistryItem))
			{
				ChangedItems.Add(CurrentRegistryItem);
			}
		}

		void ValidateRegistryItem(RegistryItemTag registryItem, bool suspendErrorProvider, bool isPreSaveValidation)
		{
			IsValidatingRegistryItem = true;
			var registryItemWithOtherChangedItems = registryItem?.RegistryItem as IRegistryItemWithOtherChangedItems;

			try
			{
				if (registryItem != null && !(registryItem.RegistryItem is LinkRegistryItem))
				{
					if (registryItemWithOtherChangedItems != null)
					{
						registryItemWithOtherChangedItems.OtherChangedItems = GetOtherChangedItems(registryItem);
					}
					string errorMessage = "";
					bool initialErrorState = registryItem.CurrentFallbackIsInError;

					registryItem.CurrentFallbackIsInError = false;
#if DEBUG
					CurrentOverrideDefault = Manager.OverrideDefault;
#endif
					if (!isReadOnly && registryItem.ShouldValidate(Manager.OverrideDefault))
					{
						try
						{
							registryItem.ValidateItem(isPreSaveValidation);
						}
						catch (RegistryValidationException exception)
						{
							errorMessage = exception.Message;
						}

						if (errorMessage.IsNullOrEmpty() && FallbackTreeView.SelectedNode != null && registryItem == CurrentRegistryItem)
						{
							errorMessage = GetEditor().GetCustomValidation(PluginControl);
						}

						if (!errorMessage.IsNullOrEmpty())
						{
							registryItem.CurrentFallbackIsInError = true;
							registryItem.LastValidationErrorMessage = errorMessage;

							if (!suspendErrorProvider)
							{
								Manager.SetValidationErrorMessage(errorMessage);
							}
						}
					}

					if (initialErrorState != registryItem.CurrentFallbackIsInError)
					{
						UpdateColouredFallbacks();
					}
				}
			}
			finally
			{
				if (registryItemWithOtherChangedItems != null)
				{
					registryItemWithOtherChangedItems.OtherChangedItems = null;
				}
				IsValidatingRegistryItem = false;
			}
		}

#if DEBUG
		internal bool CurrentOverrideDefault;
#endif

		IEnumerable<IRegistryItemInternals> GetOtherChangedItems(RegistryItemTag registryItem)
		{
			return ChangedItems.Where(x => x != registryItem).Select(x => x.RegistryItem).OfType<IRegistryItemInternals>();
		}

		protected virtual void ValidateRegistryItem(bool suspendErrorProvider, bool isPreSaveValidation = false, bool validateAllRegistryItems = false)
		{
			Manager.RemoveValidationErrorMessage();

			if (validateAllRegistryItems)
			{
				foreach (var registryItem in ChangedItems)
				{
					ValidateRegistryItem(registryItem, suspendErrorProvider, isPreSaveValidation);
				}
			}
			else
			{
				ValidateRegistryItem(CurrentRegistryItem, suspendErrorProvider, isPreSaveValidation);
			}
		}
		protected void UpdateRegistryItemValueFromPluginControl()
		{
			if (PluginControlEnteredButLeaveEventNotFired)
			{
				PerformUpdateRegistryItemValueFromPluginControl();
			}
		}

		protected virtual void PerformUpdateRegistryItemValueFromPluginControl()
		{
			PluginControlEnteredButLeaveEventNotFired = false;

			if (!isReadOnly)
			{
				CommitValue(PluginControl);

				var currentValue = CurrentRegistryItem.GetValue();
				var defaultValue = CurrentRegistryItem.IsReadOnly ? currentValue : CurrentRegistryItem.DefaultValue;
				var newValue = OverrideCheckBox.Checked ? GetEditor().GetValueFromEditorPane(PluginControl) : defaultValue;

				if (!CurrentRegistryItem.DataType.ValuesAreEqual(newValue, currentValue))
				{
					UpdateChangedItems(OverrideCheckBox.Checked, newValue);
					ValidateRegistryItem(false);
					UpdateColouredFallbacks();
				}
			}
		}

		#endregion

		#region Saving the Form

		void PerformSave()
		{
			UnhighlightNodes();
			INotifyRegistryFormSave notifyRegistryFormSave = PluginControl as INotifyRegistryFormSave;

			try
			{
#if DEBUG
				if (isThrowRegistryValidationException)
				{
					throw new RegistryValidationException("RegistryValidationException throws.");
				}
#endif
				SaveChangedItems();
			}
			catch (RegistryValidationException ex)
			{
				Globals.Message.ShowError(ex.Message);
				return;
			}

			notifyRegistryFormSave?.OnRegistryFormSave();

			RefreshLogsTabPage();
			RefreshNotesTabPage();
			ClearChangedItems(false);
		}

		protected void SaveChangedItems()
		{
			try
			{
				foreach (RegistryItemTag item in ChangedItems)
				{
					item.SaveAllValues();
				}
				Factory.Save();
				HasChanges = false;
			}
			catch (RegistryValidationException exception)
			{
				Globals.Message.ShowError(exception.Message, Res.GetString("E8A48884-86C7-4E80-AE6B-53CFD322DFD2", "Cannot Save Registry"));
			}

			if (ChangedItems.Count > 0)
			{
				int registryUpdateVersion = Env.Registry.RawRegistry.RegistryUserUpdateVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				registryUpdateVersion = registryUpdateVersion == int.MaxValue ? 0 : registryUpdateVersion + 1;
				Env.Registry.RawRegistry.RegistryUserUpdateVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryUpdateVersion);
			}
		}

		void ClearChangedItems(bool clearRegistryItemTags)
		{
			if (clearRegistryItemTags)
			{
				foreach (RegistryItemTag item in ChangedItems)
				{
					item.ClearAll();
				}
			}
			ChangedItems.Clear();
			HasChanges = false;
			RegistryItemDictionary.Instance.PurgeAll();
		}

		bool CheckForErrors(bool isPreSaveValidation = false)
		{
			ValidateRegistryItem(false, isPreSaveValidation, true);

			string errorMessage = "";
			foreach (RegistryItemTag item in ChangedItems)
			{
				if (item.AnyFallbackHasError)
				{
					errorMessage += string.Format((NoResString)"\r\n• {0}/{1}", item.Category, item.Caption);
					if ((item.DataType is IntRegistryDataType || item.DataType is DecimalRegistryDataType) && !string.IsNullOrEmpty(item.LastValidationErrorMessage))
					{
						errorMessage += " - " + item.LastValidationErrorMessage;
					}
				}
			}
			if (!string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = Res.GetString("4dc86b3b-7e43-4cdc-b273-3ff8362c89e0", "The Registry cannot be saved because errors were found in the following items:\r\n{0}", errorMessage);
				Globals.Message.ShowError(errorMessage, Res.GetString("3b90f6aa-ca27-4b0e-b963-63ff3ffb59cb", "Cannot Save Registry"));
			}
			return string.IsNullOrEmpty(errorMessage);
		}

		protected virtual DialogResult GetSaveDialogResult()
		{
			return Globals.Message.Show(Res.GetString("f525a5ed-2b84-404b-aac6-052775958d17", "All changes will be saved. Do you want to proceed?"), Res.GetString("9081805a-1355-42a3-9562-8ac2a755ca92", "Save All Changes"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		protected virtual DialogResult GetCloseDialogResult()
		{
			return Globals.Message.Show(Res.GetString("b29e61c3-96d8-49b0-b755-59c5424dd73f", "The Registry has been modified.\r\nWould you like to save the changes?"),
				Res.GetString("4de7dd51-c48c-4211-b994-aa604cd0d897", "Registry Has Been Modified"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
		}

		#endregion

		#region Updating the GUI

		void UpdateHintLabel(bool changeHint)
		{
			var hint = HintLabel.Text;

			if (changeHint)
			{
				hint = (CurrentRegistryItem != null) ? CurrentRegistryItem.Hint : DefaultHint;
				HintLabel.Text = string.IsNullOrEmpty(hint) ? Res.GetString("d8aa53dc-ce37-4540-987a-353caa70eff1", "There is no hint available for this Registry item.") : hint;

				bool isSupportLabelVisible = false;

				if (CurrentRegistryItem != null && CurrentRegistryItem.RegistryItem != null)
				{
					if (CurrentRegistryItem.RegistryItem.HasOption(RegistryOptions.IsOnlyForSupport))
					{
						isSupportLabelVisible = true;
						SupportLabel.Text = VisibleToSupportOnlyText;
					}
					else if ((EnvProxy.IsHostedWithCargowise && CurrentRegistryItem.RegistryItem.HasOption(RegistryOptions.IsOnlyEditableBySupportIfHosted) && GlbStaff.CurrentUser.IsSupportUser) || CurrentRegistryItem.IsLockedDown)
					{
						isSupportLabelVisible = true;
						SupportLabel.Text = EditableByHostedSupportOnlyText;
					}
					else if (CurrentRegistryItem.RegistryItem.HasOption(RegistryOptions.IsOnlyForDevelopers))
					{
						isSupportLabelVisible = true;
						SupportLabel.Text = VisibleToDevelopersOnlyText;
					}

					if (CurrentRegistryItem.RegistryItem.HasOption(RegistryOptions.IsOnlyFor_eAdaptorNext))
					{
						if (isSupportLabelVisible)
						{
							SupportLabel.Text = $"{ApplicableOnlyTo_eAdaptorNextText} {SupportLabel.Text}";
						}
						else
						{
							isSupportLabelVisible = true;
							SupportLabel.Text = ApplicableOnlyTo_eAdaptorNextText;
						}
					}
				}

				SupportLabel.Visible = isSupportLabelVisible;

				using (var graphic = Graphics.FromHwnd(HintLabel.Handle))
				{
					var stringSize = TextRenderer.MeasureText(HintLabel.Text, HintLabel.Font, ControlDpiScalingHelper.NewScaledSize(HintLabel.Width, Height, false), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
					ControlDpiScalingHelper.SetHeight(ref HintLabel, (int)Math.Ceiling((decimal)stringSize.Height), false);
				}
			}
		}

		void ResizeLabel(bool isInitialisingLabel, ZLabel label)
		{
			if (PluginPanel.Height > 0) // Otherwise it will have problems if the form is minimised
			{
				if (label != null &&
					!label.IsDisposed &&
					PluginControl != null &&
					!PluginControl.IsDisposed)
				{
					int originalHeight = label.Height;
					Graphics graphic = Graphics.FromHwnd(label.Handle);
					SizeF stringSize = graphic.MeasureString(label.Text, label.Font, label.Width);
					ControlDpiScalingHelper.SetHeight(ref label, (int)stringSize.Height, false);

					if (isInitialisingLabel)
					{
						ControlDpiScalingHelper.SetTop(ref OverrideCheckBox, label.Top + label.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
					}
					else
					{
						int difference = label.Height - originalHeight;
						ControlDpiScalingHelper.SetTop(ref OverrideCheckBox, OverrideCheckBox.Top + difference, false);
						ControlDpiScalingHelper.SetTop(ref PluginControl, PluginControl.Top + difference, false);
						ControlDpiScalingHelper.SetHeight(ref PluginControl, PluginControl.Height - difference, false);
					}
				}
			}
		}

		void UpdatePluginPanel(bool suspendErrorProvider)
		{
			if (!IsSetSelectedFallbackSuspended)
			{
				SuspendUpdateOverrideCheckBox();
				try
				{
					if (PluginPanel.Contains(PluginControl))
					{
						UpdateRegistryItemValueFromPluginControl();
						RemoveNotificationsChangedEventHandler();
						PluginControl.Dispose();
					}
					if (PluginPanel.Contains(FallbackMessageLabel))
					{
						FallbackMessageLabel.Dispose();
					}
					if (PluginPanel.Contains(SecurityDeniedLabel))
					{
						SecurityDeniedLabel.Dispose();
					}
					if (PluginPanel.Contains(LinkButton))
					{
						LinkButton.Dispose();
					}
					DisposeLogsTabPage();
					DisposeNotesTabPage();
					DisposeDefaultNotesTabPage();

					OverrideCheckBox.Visible = false;
					ValueLabel.Visible = false;
					FallbackMessageLabel.Visible = false;
					SecurityDeniedLabel.Visible = false;

					var registryNodeForFallbackNode = RegistriesTreeView.SelectedNode;
					var isSelectedNodeNull = registryNodeForFallbackNode == null;

					if (isSelectedNodeNull && CurrentRegistryItem != null && FallbackTreeView.SelectedNode != null && ((FallbackTreeNode)FallbackTreeView.SelectedNode).Status.Equals(FallbackStatus.Active))
					{
						registryNodeForFallbackNode = previousTreeNode;
					}

					if (registryNodeForFallbackNode != null && CurrentRegistryItem != null)
					{
						if (!isSelectedNodeNull)
						{
							previousTreeNode = RegistriesTreeView.SelectedNode;
						}

						if (FallbackTreeView.SelectedNode == null || !((FallbackTreeNode)FallbackTreeView.SelectedNode).Status.Equals(FallbackStatus.Active))
						{
							HandleInvalidFallback();
						}
						else
						{
							var checkpoint = Env.Security.GetRegistryCheckPoint(CurrentRegistryItem.RegistryItem.Name, CurrentRegistryItem.RegistryItem.Caption);
							var itemPK = CurrentRegistryItem.GetRegistryItemPK();
							HandleSecurityDenied(checkpoint);
							HandleValidFallback();
							ValidateRegistryItem(suspendErrorProvider);
							SetupLogsTabPage(itemPK);
							SetupNotesTabPage(itemPK);
						}
					}

					translateButton.Visible = CurrentRegistryItem != null &&
																		CurrentRegistryItem.RegistryItem is IRegistryItemCaptionSource &&
																		((IRegistryItemCaptionSource)CurrentRegistryItem.RegistryItem).IsTranslatable;
					translateButton.Enabled = translateButton.Visible && FallbackTreeView.SelectedNode != null && !((FallbackTreeNode)FallbackTreeView.SelectedNode).Status.Equals(FallbackStatus.NotAFallback);
				}
				finally
				{
					ResumeUpdateOverrideCheckBox();
				}
			}
		}

		TreeNode previousTreeNode;

		void UpdatePluginBusinessObjectRegistryName()
		{
			var registryBizo = PluginBusinessObject as RegistryBusinessObjectCollectionTemplate;
			if (registryBizo != null)
			{
				registryBizo.RegistryName = (CurrentRegistryItem == null) ? ZString.Empty : CurrentRegistryItem.Name;
			}
		}

		void SetupLogsTabPage(ZGuid registryItemPK)
		{
			DisposeLogsTabPage();

			if (registryItemPK.IsEmpty)
			{
				registryItemPK = ZGuid.Invalid; // Do not use empty Guid as it can have some records
			}

			tabPageLogs = new ZStmALogTabPage();
			tabControlRegistryItem.Controls.Add(tabPageLogs);
			tabPageLogs.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryForm|33f27687-47f6-4c86-8d6d-ee855b1f12f9", "Change Log");
			tabPageLogs.LogsToShow = LogsToShow.All;
			tabPageLogs.Name = "tabPageLogs";
			tabPageLogs.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3);
			tabPageLogs.TabIndex = 1;
			tabPageLogs.UseVisualStyleBackColor = true;
			tabPageLogs.TabVisible = true;

			var stmData = Factory.Load<StmData>(registryItemPK);
			if (stmData != null)
			{
				tabPageLogs.SetDataBinding(stmData, "");
				stmData.HasChangesChanged -= RegistryForm_HasChangesChanged;
				stmData.HasChangesChanged += RegistryForm_HasChangesChanged;
			}
			else
			{
				tabPageLogs.SetDataBinding(new RegistryItemLogs(registryItemPK, Factory), "");
			}
		}

		void DisposeLogsTabPage()
		{
			if (tabPageLogs != null)
			{
				tabControlRegistryItem.SelectedIndex = 0;
				tabControlRegistryItem.TabPages.Remove(tabPageLogs);
				tabPageLogs.Dispose();
				tabPageLogs = null;
			}
		}

		void RefreshLogsTabPage()
		{
			if (tabPageLogs != null)
			{
				DisposeLogsTabPage();

				if (CurrentRegistryItem != null)
				{
					SetupLogsTabPage(CurrentRegistryItem.GetRegistryItemPK());
				}
			}
		}

		void SetupNotesTabPage(ZGuid registryItemPK)
		{
			DisposeNotesTabPage();
			if (registryItemPK.IsEmpty)
			{
				registryItemPK = ZGuid.Invalid;
			}

			var stmData = Factory.Load<StmData>(registryItemPK);
			if (stmData == null)
			{
				SetupDefaultNotesTabPage();
				return;
			}
			tabPageNotes = new ZStmNoteForRegistryItemTabPage();
			tabControlRegistryItem.Controls.Add(tabPageNotes);
			tabPageNotes.CaptionResourceString =
				Res.GetData("RegistryForm|0C3BFA39-18F3-48C6-86A9-4D24BFF09A2D", "Notes");
			tabPageNotes.Name = "tabPageNotes";
			tabPageNotes.Padding = ControlDpiScalingHelper.NewScaledPadding(3);
			tabPageNotes.UseVisualStyleBackColor = true;
			tabPageNotes.TabVisible = true;
			tabPageNotes.SetDataBinding(stmData, "");
			stmData.HasChangesChanged -= RegistryForm_HasChangesChanged;
			stmData.HasChangesChanged += RegistryForm_HasChangesChanged;
		}

		void DisposeNotesTabPage()
		{
			if (tabPageNotes != null)
			{
				tabControlRegistryItem.SelectedIndex = 0;
				tabControlRegistryItem.TabPages.Remove(tabPageNotes);
				tabPageNotes.Dispose();
				tabPageNotes = null;
			}
		}

		void RefreshNotesTabPage()
		{
			DisposeNotesTabPage();
			DisposeDefaultNotesTabPage();
			if (CurrentRegistryItem != null)
			{
				SetupNotesTabPage(CurrentRegistryItem.GetRegistryItemPK());
			}
			else
			{
				SetupDefaultNotesTabPage();
			}
		}

		void DisposeDefaultNotesTabPage()
		{
			if (tabPageDefaultNotes != null)
			{
				tabControlRegistryItem.SelectedIndex = 0;
				tabControlRegistryItem.TabPages.Remove(tabPageDefaultNotes);
				tabPageDefaultNotes.Dispose();
				tabPageDefaultNotes = null;
			}
		}

		void SetupDefaultNotesTabPage()
		{
			DisposeDefaultNotesTabPage();
			tabPageDefaultNotes = new ZTabPage();
			tabPageDefaultNotes.Controls.Add(SetupDefaultNotesTabLabel());
			tabControlRegistryItem.Controls.Add(tabPageDefaultNotes);
			tabPageDefaultNotes.CaptionResourceString = Res.GetData("RegistryForm|518AA94E-AC78-49DC-9D0D-6B9F4424663D", "", "Notes");
			tabPageDefaultNotes.Name = "defaultNotesTabPage";
			tabPageDefaultNotes.Padding = ControlDpiScalingHelper.NewScaledPadding(3);
			tabPageDefaultNotes.UseVisualStyleBackColor = true;
			tabPageDefaultNotes.TabVisible = true;
		}

		ZLabel SetupDefaultNotesTabLabel()
		{
			var defaultNotesTabLabel = new ZLabel();
			defaultNotesTabLabel.FontType = OFontTypes.Normal | OFontTypes.SansSerif;
			defaultNotesTabLabel.Location = ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			defaultNotesTabLabel.Name = "defaultNotesTabLabel";
			defaultNotesTabLabel.Size = ControlDpiScalingHelper.NewScaledSize(443, 69, true);
			defaultNotesTabLabel.TextAlign = ContentAlignment.TopLeft;
			defaultNotesTabLabel.UseMnemonic = false;
			defaultNotesTabLabel.CaptionResourceString = Res.GetData("RegistryForm|7E206619-3D92-4074-B0CE-08B44C6675FE", "", "To use the Notes tab, this Registry Value must be modified.");
			defaultNotesTabLabel.Dock = DockStyle.Fill;
			return defaultNotesTabLabel;
		}

		ZStmALogTabPage tabPageLogs;
		ZStmNoteTabPage tabPageNotes;
		ZTabPage tabPageDefaultNotes;

		void HandleValidFallback()
		{
			LinkRegistryItem item = CurrentRegistryItem.RegistryItem as LinkRegistryItem;

			if (item == null)
			{
				RegistryItemEditor editor = GetEditor();
				if (editor != null)
				{
					ValueLabel.Visible = true;

					if (!isReadOnly && !CurrentRegistryItem.IsReadOnly)
					{
						OverrideCheckBox.Visible = true;

						if (CurrentRegistryItem.MustOverrideDefaultValue)
						{
							OverrideCheckBox.Checked = true;
							OverrideCheckBox.Enabled = false;
						}
						else
						{
							OverrideCheckBox.Checked = CurrentRegistryItem.HasValue;
							OverrideCheckBox.Enabled = true;
						}
					}
					else
					{
						OverrideCheckBox.Visible = false;
						OverrideCheckBox.Checked = false;
					}

					RegistryItemProposedValueAccessor retriever = GetFallbackAccessor();
					FallbackValue currentFallbackValue = retriever.GetCurrentValue();

					PluginControl = editor.NewWinFormsEditorPane();

					SetupDefaultValueMessage(currentFallbackValue.Level, retriever.IsDefaultValueForThisLevelAmbiguous(), currentFallbackValue.IsMergedWithDefaultValue);

					int pluginControlTop = (OverrideCheckBox.Visible) ? OverrideCheckBox.Top + OverrideCheckBox.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(4) : OverrideCheckBox.Top;
					ControlDpiScalingHelper.SetTop(ref PluginControl, pluginControlTop, false);
					ControlDpiScalingHelper.SetLeft(ref PluginControl, OverrideCheckBox.Left, false);
					editor.SetEditorPaneLayout(PluginControl, PluginPanel.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(10), PluginPanel.Height - pluginControlTop - (SecurityDeniedLabel.Visible ? SecurityDeniedLabel.Height : 0));
					BindingSource.SetBindingMember(PluginControl, "");
					PluginPanel.Controls.Add(PluginControl);

					editor.EnableEditorPane(PluginControl, OverrideCheckBox.Checked);
					editor.SetValueFromEditorPane(PluginControl, currentFallbackValue.Value);
					AddPluginControlEventHandlers();

					if (editor is NotInterfaceConnectorLicencedRegistryItemEditor)
					{
						OverrideCheckBox.Visible = false;
						ValueLabel.Visible = false;
						FallbackMessageLabel.Visible = false;
						SecurityDeniedLabel.Visible = false;
					}
				}
			}
			else
			{
				HandleLinkRegistryItem(item);
			}
		}

		void HandleLinkRegistryItem(LinkRegistryItem item)
		{
			LinkButton = new LinkButton(item.ModuleName, item.ModuleID);
			LinkButton.Location = ValueLabel.Location;
			ValueLabel.Visible = false;
			OverrideCheckBox.Visible = false;
			PluginPanel.Controls.Add(LinkButton);
			LinkButton.BringToFront();
		}

		void AddPluginControlEventHandlers()
		{
			PluginControl.Enter += new EventHandler(PluginControl_Enter);
			AddPluginControlLeaveEventHandler();
			AddNotificationsChangedEventHandler();
		}

		IBusiness PluginBusinessObject => (PluginControl as ZUserControl)?.BindingSource.DataSource as IBusiness;

		void AddNotificationsChangedEventHandler()
		{
			var editorBoundBusinessObject = PluginBusinessObject;

			if (editorBoundBusinessObject != null)
			{
				editorBoundBusinessObject.NotificationsChanged -= new EventHandler<NotificationsChangedEventArgs>(RegistryForm_NotificationsChanged);   //Avoid having multiple handlers to same registry item
				editorBoundBusinessObject.NotificationsChanged += new EventHandler<NotificationsChangedEventArgs>(RegistryForm_NotificationsChanged);
				editorBoundBusinessObject.HasChangesChanged += new EventHandler<HasChangesChangedEventArgs>(RegistryForm_HasChangesChanged);
			}
			else if (PluginControl is ICustomHasChangesControl c)
			{
				c.HasChangesChanged += RegistryForm_HasChangesChanged;
			}
		}

		void RemoveNotificationsChangedEventHandler()
		{
			var editorBoundBusinessObject = PluginBusinessObject;
			if (editorBoundBusinessObject != null)
			{
				editorBoundBusinessObject.NotificationsChanged -= new EventHandler<NotificationsChangedEventArgs>(RegistryForm_NotificationsChanged);
				editorBoundBusinessObject.HasChangesChanged -= new EventHandler<HasChangesChangedEventArgs>(RegistryForm_HasChangesChanged);
			}
		}

		bool settingHasChanges;
		void RegistryForm_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (!settingHasChanges)
			{
				try
				{
					settingHasChanges = true;
					HasChanges |= e.ObjectJustWasChanged;
				}
				finally
				{
					settingHasChanges = false;
				}
			}
		}

		void RegistryForm_NotificationsChanged(object sender, NotificationsChangedEventArgs e)
		{
			if (OverrideCheckBox.Checked)
			{
				UpdateChangedItems(true, GetEditor().GetValueFromEditorPane(PluginControl));
			}
			if (!IsValidatingRegistryItem)
			{
				ValidateRegistryItem(false);
			}
		}

		protected virtual void AddPluginControlLeaveEventHandler()
		{
			PluginControl.Leave += new EventHandler(PluginControl_Leave);
		}

		RegistryItemProposedValueAccessor GetFallbackAccessor()
		{
			FallbackTreeNode node = (FallbackTreeNode)FallbackTreeView.SelectedNode;
			return node.GetFallbackAccessor(CurrentRegistryItem);
		}

		void HandleInvalidFallback()
		{
			string message = Res.GetString("47f79db5-b807-452a-9783-73cfd6cd078e", "Please select a valid fallback level.");
			if (FallbackTreeView.SelectedNode != null &&
				((FallbackTreeNode)FallbackTreeView.SelectedNode).Status.Equals(FallbackStatus.NotActive))
			{
				message = Res.GetString("d5021b80-7274-44be-820b-2cd124a7ef0c", "The selected fallback level does not apply to this Registry item.");
			}
			SetupFallbackMessageLabel(false, message);
		}

		#region Setting Up FallbackMessageLabel

		void SetupFallbackMessageLabel(bool isValidFallback, string message)
		{
			FallbackMessageLabel = new ZLabel();

			if (isValidFallback)
			{
				FallbackMessageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				ControlDpiScalingHelper.SetTop(ref FallbackMessageLabel, ValueLabel.Top + ValueLabel.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				ControlDpiScalingHelper.SetLeft(ref FallbackMessageLabel, ValueLabel.Left, false);
				FallbackMessageLabel.ForeColor = Color.Purple;
			}
			else
			{
				FallbackMessageLabel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
				ControlDpiScalingHelper.SetLeft(ref FallbackMessageLabel, PluginPanel.Left, false);
				FallbackMessageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
				ControlDpiScalingHelper.SetHeight(ref FallbackMessageLabel, PluginPanel.Height, false);
			}

			ControlDpiScalingHelper.SetWidth(ref FallbackMessageLabel, PluginPanel.Width, false);
			FallbackMessageLabel.Text = message;

			PluginPanel.Controls.Add(FallbackMessageLabel);
			ResizeLabel(true, FallbackMessageLabel);
		}

		void HandleSecurityDenied(SecurityCheckpoint checkpoint)
		{
			var deniedMessage = "";
			if (checkpoint.IsAllowed)
			{
				isReadOnly = !Env.Security.SystemRegistryEdit.IsAllowed;
				if (isReadOnly)
				{
					deniedMessage = Env.Security.GetErrorMessageForNotAllowed(Env.Security.SystemRegistryEdit);
				}
			}
			else
			{
				isReadOnly = true;
				deniedMessage = Env.Security.GetErrorMessageForNotAllowed(checkpoint);
			}

			if (!string.IsNullOrEmpty(deniedMessage))
			{
				SecurityDeniedLabel = new ZLabel();
				SecurityDeniedLabel.Text = deniedMessage;
				SecurityDeniedLabel.Anchor = AnchorStyles.Bottom; //(AnchorStyles)AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
				SecurityDeniedLabel.Dock = DockStyle.Bottom;
				SecurityDeniedLabel.ForeColor = Color.Red;
				ControlDpiScalingHelper.SetLeft(ref SecurityDeniedLabel, PluginPanel.Left, false);
				SecurityDeniedLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
				ControlDpiScalingHelper.SetWidth(ref SecurityDeniedLabel, PluginPanel.Width, false);

				PluginPanel.Controls.Add(SecurityDeniedLabel);

				var graphic = Graphics.FromHwnd(SecurityDeniedLabel.Handle);
				//add an extra line to compensate for higher DPIs being slightly too narrow vertically
				var stringSize = graphic.MeasureString(SecurityDeniedLabel.Text + System.Environment.NewLine + "W1", SecurityDeniedLabel.Font, SecurityDeniedLabel.Width);
				ControlDpiScalingHelper.SetHeight(ref SecurityDeniedLabel, (int)stringSize.Height, false);
				ControlDpiScalingHelper.SetTop(ref SecurityDeniedLabel, PluginPanel.Bottom - SecurityDeniedLabel.Height - ExtraPanel.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
				SecurityDeniedLabel.BringToFront();
			}
		}

		protected void SetupDefaultValueMessage(RegistryStorageFlags fallbackLevel, bool isAmbiguous, bool isMergedWithDefaultValue)
		{
			string message = Res.GetString("3a994f99-6138-408a-a5b8-f6d9cdd2a6f6", "Default value overridden.");

			if (CurrentRegistryItem.HasReadOnlyOption || fallbackLevel != ((FallbackTreeNode)FallbackTreeView.SelectedNode).GetFallbackLevel().Level)
			{
				message = (isAmbiguous) ?
									Res.GetString("bcd326d7-238f-4d9f-8eb9-137e90188c37", "Default value is ambiguous and may not be accurate - please refer to parent fall backs.") :
									GetFallbackLevelString(fallbackLevel, isMergedWithDefaultValue);
			}

			SetupFallbackMessageLabel(true, message);
		}

		string GetFallbackLevelString(RegistryStorageFlags fallbackLevel, bool isMergedWithDefaultValue)
		{
			string[] fallbackLevels = Array.Empty<string>();

			if (fallbackLevel == RegistryStorageFlags.All)
			{
				return Res.GetString("a471f49e-3edc-42e5-b0d7-4d734855d3a4", "Default value obtained from Registry item default.");
			}
			else
			{
				fallbackLevels = GetFallbackLevelsList(fallbackLevel);
			}

			string result = "";

			bool isMergedValue = !(fallbackLevels.Length == 1 && !isMergedWithDefaultValue);

			if (!isMergedValue)
			{
				result = Res.GetString("879c3274-c93e-4729-b7ca-ee05c30a9de1", "Default value obtained from {0} level.", fallbackLevels[0]);
			}
			else
			{
				result = (isMergedWithDefaultValue) ? Res.GetString("d816e329-e6be-4a81-aa74-3d84bea27707", "Default value obtained by merging values from Registry item default") : Res.GetString("a5ba7204-5b56-492c-bd06-6bf922a97c84", "Default value obtained by merging values from") + " ";

				if (fallbackLevels.Length > 1)
				{
					result = (isMergedWithDefaultValue) ? result + ", " : result;
					result += fallbackLevels[0];

					for (int x = 1; x < fallbackLevels.Length; ++x)
					{
						if (x != fallbackLevels.Length - 1)
						{
							result += ", " + fallbackLevels[x];
						}
						else
						{
							result += " " + Res.GetString("8e20b315-3fb8-417b-87b5-bab98f1787d5", "and {0} levels.", fallbackLevels[x]);
						}
					}
				}
				else if (isMergedWithDefaultValue)
				{
					result += " " + Res.GetString("8e20b315-3fb8-417b-87b5-bab98f1787d5", "and {0} levels.", fallbackLevels[0]);
				}
			}

			return result;
		}

		string[] GetFallbackLevelsList(RegistryStorageFlags fallbackLevel)
		{
			ArrayList fallbackLevels = new ArrayList();

			if ((fallbackLevel & RegistryStorageFlags.System) == RegistryStorageFlags.System)
			{
				fallbackLevels.Add(Res.GetString("4d730b27-5dd5-496b-af3c-af9407e361a1", "System"));
			}
			if ((fallbackLevel & RegistryStorageFlags.SystemDepartment) == RegistryStorageFlags.SystemDepartment)
			{
				fallbackLevels.Add(Res.GetString("9f10365a-203b-414c-a085-f8634dee3be1", "System Department"));
			}
			if ((fallbackLevel & RegistryStorageFlags.Company) == RegistryStorageFlags.Company)
			{
				fallbackLevels.Add(Res.GetString("f1e74302-2824-439e-b8df-778564b62b03", "Company"));
			}
			if ((fallbackLevel & RegistryStorageFlags.CompanyDepartment) == RegistryStorageFlags.CompanyDepartment)
			{
				fallbackLevels.Add(Res.GetString("27c0681a-2588-4743-ba35-ce2619359c2e", "Company Department"));
			}
			if ((fallbackLevel & RegistryStorageFlags.Branch) == RegistryStorageFlags.Branch)
			{
				fallbackLevels.Add(Res.GetString("91e096db-cd75-4ffe-826d-a58636ea7bbf", "Branch"));
			}
			if ((fallbackLevel & RegistryStorageFlags.BranchDepartment) == RegistryStorageFlags.BranchDepartment)
			{
				fallbackLevels.Add(Res.GetString("017cfa74-4518-4434-b012-6a493121aace", "Branch Department"));
			}

			return (string[])fallbackLevels.ToArray(typeof(string));
		}

		#endregion

		#endregion

		#region Highlight/Unhighlight Changed Nodes

		protected void UpdateColouredFallbacks()
		{
			if (CurrentRegistryItem == null || RegistriesTreeView.SelectedNode == null || FallbackTreeView.SelectedNode == null)
			{
				return;
			}

			RegistriesTreeView.SelectedNode.ForeColor = (CurrentRegistryItem.AnyFallbackHasError) ? Color.Red : Color.Blue;
			FallbackTreeView.SelectedNode.ForeColor = (CurrentRegistryItem.CurrentFallbackIsInError) ? Color.Red : Color.Blue;

			if (ColouredNodes.Contains(RegistriesTreeView.SelectedNode))
			{
				var fallbackNodes = (ArrayList)ColouredNodes[RegistriesTreeView.SelectedNode];

				bool foundExisting = false;
				foreach (FallbackTreeNode fallbackNode in fallbackNodes)
				{
					if (fallbackNode.Comparator == ((FallbackTreeNode)FallbackTreeView.SelectedNode).Comparator)
					{
						foundExisting = true;
						fallbackNodes[fallbackNodes.IndexOf(fallbackNode)] = FallbackTreeView.SelectedNode;
						break;
					}
				}
				if (!foundExisting)
				{
					fallbackNodes.Add(FallbackTreeView.SelectedNode);
				}
			}
			else
			{
				var value = new ArrayList();
				value.Add(FallbackTreeView.SelectedNode);
				ColouredNodes.Add(RegistriesTreeView.SelectedNode, value);
			}
		}

		protected void UnhighlightNodes()
		{
			foreach (TreeNode registryNode in ColouredNodes.Keys)
			{
				registryNode.ForeColor = RegistriesTreeView.ForeColor;
			}
			HighlightFallbackNodes(true);
			ColouredNodes.Clear();
		}

		void HighlightFallbackNodes(bool resetToDefault)
		{
			if (RegistriesTreeView.SelectedNode != null && ColouredNodes.ContainsKey(RegistriesTreeView.SelectedNode))
			{
				foreach (FallbackTreeNode fallbackNode in (ArrayList)ColouredNodes[RegistriesTreeView.SelectedNode])
				{
					FallbackTreeNode colouredNode = FindNode(fallbackNode);

					if (colouredNode != null)
					{
						if (resetToDefault)
						{
							colouredNode.ForeColor = FallbackTreeView.ForeColor;
						}
						else
						{
							colouredNode.ForeColor = ((RegistryItemTag)RegistriesTreeView.SelectedNode.Tag).GetIsInErrorCustomPK(colouredNode.GetFallbackLevel()) ?
								Color.Red : Color.Blue;
						}
						colouredNode.EnsureVisible();
					}
				}
			}
		}

		#endregion

		#region Recursively Find Node

		FallbackTreeNode FindNode(FallbackTreeNode nodeToLookFor)
		{
			FallbackTreeNode result = null;

			if (nodeToLookFor != null)
			{
				foreach (FallbackTreeNode node in FallbackTreeView.Nodes)
				{
					if (node.Comparator == nodeToLookFor.Comparator)
					{
						result = node;
					}
					else
					{
						result = FindNode(node, nodeToLookFor);
					}

					if (result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		FallbackTreeNode FindNode(FallbackTreeNode parentNode, FallbackTreeNode nodeToLookFor)
		{
			FallbackTreeNode result = null;
			foreach (FallbackTreeNode node in parentNode.Nodes)
			{
				if (node.Comparator == nodeToLookFor.Comparator)
				{
					result = node;
				}
				else
				{
					result = FindNode(node, nodeToLookFor);
				}

				if (result != null)
				{
					break;
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region Event Handlers

		#region TreeView Events

		void RegistriesTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			UpdateRegistryItemValueFromPluginControl();
		}

		void FallbackTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
		{
			UpdateRegistryItemValueFromPluginControl();
		}

		protected virtual void FallbackTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (!PluginControlEnteredButLeaveEventNotFired)
			{
				SetSelectedFallback();

				if (CurrentRegistryItem != null && e.Node != null && ((FallbackTreeNode)FallbackTreeView.SelectedNode).Status.Equals(FallbackStatus.Active))
				{
					CurrentRegistryItem.SetFallback(((FallbackTreeNode)e.Node).GetFallbackLevel());
				}
				UpdatePluginPanel(false);
			}
			else
			{
				throw new ZException("PluginControl was entered, but its value was not retrieved and updated to the previous registry item before a different fallback was selected.");
			}
		}

		protected virtual void RegistriesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (!PluginControlEnteredButLeaveEventNotFired)
			{
				CurrentRegistryItem = e.Node.Tag as RegistryItemTag;
				FillFallbackTreeView(e.Node);
				FallbackTreeView.SelectedNode = null;
				GetSelectedFallback();
				PluginGroupBox.Text = e.Node.Text.Replace("&", "&&");
				UpdatePluginPanel(false);
				UpdatePluginBusinessObjectRegistryName();
				UpdateHintLabel(true);
				NameOnDbTextBox.Text = (CurrentRegistryItem == null) ? ZString.Empty : CurrentRegistryItem.Name;
			}
			else
			{
				throw new ZException("PluginControl was entered, but its value was not retrieved and updated to the previous registry item before a different registry item was selected.");
			}
		}

		#endregion

		#region Button and Menu Item Events

		void SaveButton_Click(object sender, EventArgs e)
		{
			SaveInternal();
		}

		protected override void SaveInternal()
		{
			if (HasChanges)
			{
				UpdateRegistryItemValueFromPluginControl();
				bool noErrors = CheckForErrors(true);

				if (noErrors)
				{
					if (GetSaveDialogResult() == DialogResult.Yes)
					{
						PerformSave();
					}
				}
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("d6078c5b-a355-4662-881f-7940ebd3f2e7", "No changes have been made to the Registry."), Res.GetString("6cf920b2-bc98-4fa1-93a4-6b9f197241a2", "No Changes To Save"));
			}
		}

		void CloseMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void HideMenuItem_Click(object sender, EventArgs e)
		{
			UpdateRegistryItemValueFromPluginControl();
			HideMenuItem.Checked = !HideMenuItem.Checked;
			Builder.SetHideInactiveFallbacks(HideMenuItem.Checked);
			FillFallbackTreeView(RegistriesTreeView.SelectedNode);
			GetSelectedFallback();
			UpdatePluginPanel(false);
		}

		void FindMenuItem_Click(object sender, EventArgs e)
		{
			RegistriesTreeView.ShowFindForm();
		}

		#endregion

		#region Plugin-related Events

		void OverrideCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!IsUpdateOverrideCheckBoxSuspended && PluginPanel.Controls.Contains(PluginControl))
			{
				Manager.ValidationErrorMessage = null;
				if ((CurrentRegistryItem.Name.Equals("SystemFont") || CurrentRegistryItem.Name.Equals("NavigationMenuFont") || CurrentRegistryItem.Name.Equals("NewsAnnouncementFont")) && OverrideCheckBox.Checked)
				{
					string warningMessage = Res.GetString("563CFC91-54D0-42E3-905D-AC332EB72B68", "Warning: Changing {0} might cause text to be displayed incorrectly.\r\nThe font will not go into effect until {1} is closed and relaunched.", CurrentRegistryItem.Caption, BrandingFactory.Instance.ProductName);
					Globals.Message.ShowWarning(warningMessage, Res.GetString("17ECA415-3AC7-4CBA-A8B6-E25A87E334A1", "System wide custom Font"));
				}

				UpdateChangedItems(Manager.OverrideDefault, GetEditor().GetValueFromEditorPane(PluginControl));
				UpdatePluginPanel(Manager.OverrideDefault);
				UpdateColouredFallbacks();
			}
		}

		protected virtual void PluginControl_Enter(object sender, EventArgs e)
		{
			if (!PluginControlEnteredButLeaveEventNotFired)
			{
				PluginControlEnteredButLeaveEventNotFired = true;
			}
		}

		void PluginControl_Leave(object sender, EventArgs e)
		{
			UpdateRegistryItemValueFromPluginControl();
		}

		#endregion

		#region Filter Change Logs related events

		void filterChangeLogsMenuItem_Click(object sender, EventArgs e)
		{
			if (this.filterChangeLogsMenuItem.Checked)
			{
				//Re-initialize the tree view
				RegistriesTreeView.DynamicUpdateOnExpandEnabled = true;
				FillRegistriesTreeView();
				this.filterChangeLogsMenuItem.Checked = false;
			}
			else
			{
				var filteredItemsToShow = GetChangeLogFilteredRegistryItems();
				if (filteredItemsToShow != null)
				{
					RegistriesTreeView.BeginUpdate();
					RegistriesTreeView.DynamicUpdateOnExpandEnabled = false;
					FilterRegistryItemTree(filteredItemsToShow);
					RegistriesTreeView.EndUpdate();
					this.filterChangeLogsMenuItem.Checked = true;
				}
			}
		}

		public void FilterRegistryItemTree(IEnumerable<IRegistryItem> registryItems)
		{
			var regItemTags = registryItems.Select(ri => new RegistryItemTag(ri)).ToList();
			var validRegItemCategories = new List<string>();
			foreach (var cats in regItemTags.Select(regItemTag => regItemTag.Category.Split('/')))
			{
				validRegItemCategories.AddRange(cats);
			}

			//Remove invalid categories first
			RemoveCategoriesFromTree(RegistriesTreeView.Nodes, validRegItemCategories);

			//DFS search and removal of all items in tree
			DfsRemoveNodes(RegistriesTreeView.Nodes, regItemTags);
		}

		void DfsRemoveNodes(TreeNodeCollection nodes, IList<RegistryItemTag> validTags)
		{
			var nodesToRemove = new List<int>();

			for (var i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];

				if (node.Tag is RegistryCategoryRef category)
				{
					//As the registry is dynamically loaded we need to force an update of the node here.
					var content = registry.GetSortedContent(category.Key);
					if (content.Items != null && content.Items.Any())
					{
						RegistriesTreeView.AddItems(node.Nodes, content.Items);
					}
					if (content.Categories != null && content.Categories.Any())
					{
						RegistriesTreeView.AddCategories(node.Nodes, content.Categories);
					}
					DfsRemoveNodes(node.Nodes, validTags);
					if (node.Nodes.Count == 0)
					{
						nodesToRemove.Add(i - nodesToRemove.Count);
					}
				}
				else if (node.Tag == null)
				{
					DfsRemoveNodes(node.Nodes, validTags);
					if (node.Nodes.Count == 0)
					{
						nodesToRemove.Add(i - nodesToRemove.Count);
					}
				}
				else
				{
					if (node.Tag is not RegistryItemTag nodeTag)
					{
						continue;
					}
					if (validTags.All(vt => vt.Name != nodeTag.Name))
					{
						nodesToRemove.Add(i - nodesToRemove.Count);
					}
				}
			}

			foreach (var index in nodesToRemove)
			{
				nodes.RemoveAt(index);
			}
		}

		void RemoveCategoriesFromTree(TreeNodeCollection nodes, IList<string> validCategories)
		{
			var nodesToRemove = new List<int>();

			for (var i = 0; i < nodes.Count; i++)
			{
				var node = nodes[i];
				if (node.Tag != null && node.Tag is not RegistryCategoryRef)
				{
					continue;
				}
				if (!validCategories.Contains(node.Text))
				{
					nodesToRemove.Add(i - nodesToRemove.Count);
				}
			}

			foreach (var index in nodesToRemove)
			{
				nodes.RemoveAt(index);
			}
		}

		public IEnumerable<IRegistryItem> GetChangeLogFilteredRegistryItems()
		{
			using (var dateForm = new SelectChangeLogFilterDatesForm())
			{
				var result = ZFormModaliser.ShowDialogWithoutDispose(dateForm);
				if (result == DialogResult.OK)
				{
#if DEBUG
					//For unit testing purposes, if the form dates are empty, fill them.
					if (dateForm.fromDate.DateTimeValue == ZDateTime.Empty)
					{
						dateForm.fromDate.DateTimeValue = ZDateTime.Today;
					}
					if (dateForm.toDate.DateTimeValue == ZDateTime.Empty)
					{
						dateForm.toDate.DateTimeValue = ZDateTime.Today;
					}
#endif
					var changeLogFilter = new RegistryItemChangeLogFilter(this.Factory);

					using (var progressForm = new ProgressForm())
					{
						var progressManager = new RegistryDiffFinderProgress(progressForm);
						progressForm.Show();
						progressForm.Invalidate();
						progressForm.Update();
						try
						{
#if DEBUG
							var registryItems = changeLogFilter
								.FilterRegistryItemsWithEvents(Builder.RegistryItemsToDisplay.ToList(),
									dateForm.FilterParameters, progressManager).ToList();
							registryItems.Add(new StringRegistryItem("TestItem1", (NoResString)"Cat-1/Sub-Category 12", (NoResString)"ZZ-Item 1", null, RegistryStorageFlags.System));
							return registryItems;
#else
							return changeLogFilter.FilterRegistryItemsWithEvents(Builder.RegistryItemsToDisplay.ToList(), dateForm.FilterParameters, progressManager).ToList();
#endif
						}
						catch (OperationCanceledException)
						{
							return null;
						}
					}
				}
				return null;
			}
		}

		#endregion

		bool HideInactiveChildren => HideMenuItem.Checked;

		void RegistryForm_Resize(object sender, EventArgs e)
		{
			UpdateHintLabel(false);
			ResizeLabel(false, FallbackMessageLabel);
		}

		void translateButton_Click(object sender, EventArgs e)
		{
			var captionSource = new TranslatableRegistryItemValueCaptionSource((ITranslatableRegistryItemCaptionSource)CurrentRegistryItem.RegistryItem, GetFallbackAccessor().GetCurrentValue().Value);
			ObjectFactory.Get<ICustomizableDataTranslationEditor>().EditTranslations(new CustomizableDataResourceStrings(captionSource), (ResourceString)captionSource.GetRuntimeCaptions().FirstOrDefault(), null);
		}

		void findOverridesMenuItem_Click(object sender, EventArgs e)
		{
			var difBizo = GetDiffBusinessObject();
			if (difBizo != null)
			{
				if (difBizo.ItemsThatDifferBetweenBaseAndOverride.Any())
				{
					ShowComparisonFormForExport(difBizo);
				}
				else
				{
					Globals.Message.ShowInformation(Res.GetString("E934ECDF-F59C-41FF-AD81-E0AAC57F38F2", "No difference was found between the two levels you selected."));
				}
			}
		}

		Tuple<IOverrideLevel, IOverrideLevel> GetLevelsToDiff()
		{
			using (var form = new FindOverridesLevelForm(HideInactiveChildren))
			{
				var result = ZFormModaliser.ShowDialogWithoutDispose(form);

				return result == DialogResult.OK ? form.SelectedValues : null;
			}
		}

		void ShowComparisonFormForExport(RegistryComparisonBusinessObject differenceObject)
		{
			var form = new RegistryExportForm(differenceObject, new RegistryItemSaveHandler(), HideInactiveChildren);
			form.Show();
		}

		IRegistryItemSaveHandler SaveHandler { get { return new RegistryItemSaveHandler(); } }

#if DEBUG
		internal RegistryComparisonBusinessObject diffBusinessObjectForTest;
#endif

		RegistryComparisonBusinessObject GetDiffBusinessObject()
		{
#if DEBUG
			if (diffBusinessObjectForTest != null)
			{
				return diffBusinessObjectForTest;
			}
#endif
			var levels = GetLevelsToDiff();

			if (levels != null)
			{
				using (var progressForm = new ProgressForm())
				{
					var progressManager = new RegistryDiffFinderProgress(progressForm);
					progressForm.Show();

					try
					{
						return new RegistryComparisonBusinessObject(GetApplicableItemsForComparison(), levels.Item1, levels.Item2, progressManager);
					}
					catch (OperationCanceledException)
					{
						return null;
					}
				}
			}

			return null;
		}

		internal IEnumerable<IRegistryItem> GetApplicableItemsForComparison() => Builder.RegistryItemsToDisplay.Where(item => item.CanBeExported);

		internal class RegistryDiffFinderProgress : IProgress<int>
		{
			readonly ProgressForm progressForm;
			readonly string progressStatus;

			volatile bool hasBeenCancelled;
			int itemsChecked;

			// Maybe replace this with a non-guess? Like based on the exact number of registry items or something like that.
			// No it shouldn't, see TestRoughGuessIsntTooFarOut for justification
			public const int guessAtHowManyItemsWillBeChecked = 3803;

			public RegistryDiffFinderProgress(ProgressForm progressForm)
			{
				this.progressForm = progressForm;
				progressStatus = Res.GetString("49E57A70-9431-49F7-94A8-1DD39C708EC3", "Finding overrides...");

				progressForm.ShowCancelButton = true;
				progressForm.Cancelled += (o, e) => hasBeenCancelled = true;
				progressForm.SetStatusAndPercentComplete(progressStatus, 0);
			}

			int previousPercentComplete;

			public void Report(int numberOfItemsThatHaveBeenProcessedSinceLastCall)
			{
				itemsChecked += numberOfItemsThatHaveBeenProcessedSinceLastCall;
				var percentComplete = Math.Min(100 * itemsChecked / guessAtHowManyItemsWillBeChecked, 97);

				if (percentComplete - previousPercentComplete >= 2)
				{
					progressForm.PercentComplete = percentComplete;
					previousPercentComplete = percentComplete;
				}

				if (hasBeenCancelled)
				{
					throw new OperationCanceledException("The process has been cancelled");
				}
			}
		}

#endregion

#if DEBUG
		internal bool isThrowRegistryValidationException;
#endif

		void importOverridesMenuItem_Click(object sender, EventArgs e)
		{
			var comparison = GetImportedValues();

			if (comparison != null)
			{
				var form = new RegistryApplyForm(comparison);
				form.Show();
			}
		}

		RegistryComparisonBusinessObject GetImportedValues()
		{
			using (var openFileDialog = new ZOpenFileDialog())
			{
				var bizo = new RegistryImportBusinessObject(GetApplicableItemsForComparison(), openFileDialog, SaveHandler);
				using (var form = new ImportOverridesForm(bizo, HideInactiveChildren))
				{
					return ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK ? bizo.Comparison : null;
				}
			}
		}

		void exportListOfPreservedRegistryItemsMenuItem_Click(object sender, EventArgs e)
		{
			var allItems = new RegistryItemSetLocator().GetAllRegistryItems();
			var itemsToExport = GetPerservedRegistryItems(allItems);

			if (itemsToExport.Any())
			{
				ExportToCsv(itemsToExport, new RegistryItemSaveCsvHandler());
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("5b70f4b7-eaa6-4ea2-b7cf-dda0362aea56", "No Preserved Registry Item to Export!"));
			}
		}

		public IEnumerable<string> OldRegistryValues
		{
			get
			{
				if (oldRegistryValues == null)
				{
					var assembly = Assembly.Load("Enterprise.DbBackupAndRestore.Business");
					string oldRegistryValuesText;
					const string oldRegItemListResourceName = "Enterprise.DataTools.DbBackupAndRestore.Business.Restore.CopyProductionToTest.DataToPreserve.OldRegistryToPreserveList.txt";

					using (var oldRegItemListStream = assembly.GetManifestResourceStream(oldRegItemListResourceName))
					using (var oldRegItemListReader = new StreamReader(oldRegItemListStream))
					{
						oldRegistryValuesText = oldRegItemListReader.ReadToEnd();
					}
					oldRegistryValues = oldRegistryValuesText.SplitByLine().Select(line => line.Trim(',', '\''));
				}
				return oldRegistryValues;
			}

			internal set => oldRegistryValues = value;
		}
		IEnumerable<string> oldRegistryValues;

		protected IEnumerable<IRegistryItem> GetPerservedRegistryItems(IEnumerable<IRegistryItem> registryItems)
		{
			var itemsToExport = registryItems
				.Where(item =>
					item.CanBeExported
					&& !item.HasOption(RegistryOptions.IsHidden)
					&& (item.HasOption(RegistryOptions.PreserveTestValue) || OldRegistryValues.Any(old => old.Equals(item.Name, StringComparison.OrdinalIgnoreCase))))
				.OrderBy(item => item.Category).ThenBy(item => item.Caption);

			return itemsToExport;
		}

		void ExportToCsv(IEnumerable<IRegistryItem> itemsToExport, IRegistryItemSaveHandler saveHandler)
		{
			using (var saveFileDialog = new ZSaveFileDialog() { DefaultExt = ".csv", Filter = (NoResString)"CSV Files (*.csv)|*.csv", AddExtension = true })
			{
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(saveFileDialog) == DialogResult.OK)
				{
					try
					{
						using (var stream = saveFileDialog.OpenFile())
						{
							var level = new DefaultOverrideLevel();
							saveHandler.SaveItems(itemsToExport, level, stream);
							Globals.Message.Show(Res.GetString("a1d0971c-bb50-4b77-ae69-7a71dae70f0d",
@"List has exported to:
{0}", saveFileDialog.UnmappedFileName));
						}
					}
					catch (OperationCanceledException)
					{
						DeleteCreatedFiles(saveFileDialog);
					}
					catch (IOException)
					{
						DeleteCreatedFiles(saveFileDialog);

						Globals.Message.ShowError(Res.GetString("0BDFBC8D-EF33-447D-A328-158D2F2747D6", "A problem was encountered while trying to write to the file. Please try writing to a different Path."));
					}
				}
			}
		}

		void DeleteCreatedFiles(ZSaveFileDialog saveDialog)
		{
			if (File.Exists(saveDialog.UnmappedFileName))
			{
				File.Delete(saveDialog.UnmappedFileName);
			}
		}

		public void UpdateHasChanges()
		{
			HasChanges = true;
		}
	}
}
