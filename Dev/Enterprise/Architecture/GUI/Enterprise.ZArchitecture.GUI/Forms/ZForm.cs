#pragma warning disable 0809
// ReSharper disable DoNotCallOverridableMethodsInConstructor

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business;
using CargoWise.Integration;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using CargoWise.Windows.UI.Interop;
using CargoWise.Windows.UI.Layout;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.DevTools;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Business.Design;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Design;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Scanning;
using Enterprise.ZArchitecture.GUI.Support;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Tools;
using Constants = Enterprise.Core.Constants;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

#if DEBUG
using UnsafeNativeMethods = CargoWise.Interop.UnsafeNativeMethods;
#endif

#if !WINZOR
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
#endif

namespace Enterprise.ZArchitecture.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	[TestExcludeZWinFormHasTypedConstructor]
#if DEBUG
	[DesignerSerializer(typeof(ControlCodeDomSerializerWithDelayedTabCreate), typeof(CodeDomSerializer))]
#endif
	[SuppressFormDesignerAnalysis]
	[ContainerControlBaseClass]
	[UserEventDiagnosticReference("Caption = \"{Text}\"")]
	public partial class ZForm : KForm,
		IZForm,
		IDialogKeyDown,
		IUpdateStatusBar,
		IHaveTooltipsForMigration,
		IDisplayModeAware,
		ICaptionRenderingSupport,
		IDesignTimeDataSourceType,
		IShowPreSaveDialog,
		ISaveInitiator,
		IStatusBarProvider,
		IPositionSaveProvider,
		IFormPlugInsProvider,
		IPostingButtonsProvider,
		IPreviousNextControlProvider,
		IBusinessForm,
		IPasteSupport,
		IFileMenuItemsProvider,
		IDisposeStackProvider,
		IResCaptionedControl,
		IHotkeyProvider,
		ISplitterLayoutProvider,
		IRecentItemCaptionFormatter
	{
		#region Initialisation

		public ZForm()
		{
			Initialise();
			InitialiseForm();
		}

		public ZForm(object dataSource)
		{
			constructedDataSource = dataSource;
			Initialise();
			InitialiseForm();
			constructedDataSource = null;

			if (!this.IsDesignMode() && dataSource != null)
			{
				SetDataBinding(dataSource, "");
			}
		}

		void Initialise()
		{
			if (DesignModeFinder.IsDesigning)
			{
				DesignTimeEnvironment.InitializeDesignTimeEarlyWithoutServiceProvider();
			}

			AddAdornments();

			DisplayErrorsInMessagePanel = true;
			RememberSplitterLayout = true;
			BindingContext = new ZBindingContext();
			designTimeDataSourceTypeHelper = new TopLevelDataSourceTypeHelper(this);

			InitializeExtenderProviders();
			UpdateMaximizeBox();

			ZFormMenuStrategy.AddActionsMenuItem(this);
			if (!DesignModeFinder.IsDesigning)
			{
				BackColor = ObjectFactory.Get<ISystemDataRegistry>().ColorTheme.FormBackgroundColor;
			}

			if (ShouldAddFormActivityLog)
			{
				ZFormActivityLoggingStrategy.AddAdornments(this);
			}

			if (!DesignModeFinder.IsDesigning && BindingSource.DataSourceType == null)
			{
				BindingSource.DataSourceType = typeof(object);
			}

			RegisterHotkeys();
		}

		#region ShouldAddFormActivityLog

		protected virtual bool ShouldAddFormActivityLog => true;

		#endregion

		[Browsable(true)]
		[DefaultValue(true)]
		public bool RememberSplitterLayout { get; set; }

		[Browsable(false)]
		public Dictionary<string, (ISplitterLayoutSaveProvider Splitter, int SplitterPosition)> Splitters
		{
			get
			{
				return splitters ?? (splitters = new Dictionary<string, (ISplitterLayoutSaveProvider Splitter, int SplitterPosition)>());
			}
		}
		Dictionary<string, (ISplitterLayoutSaveProvider Splitter, int SplitterPosition)> splitters;

		protected virtual void InitialiseForm()
		{
			SuspendLayout();
			ClientSize = ControlDpiScalingHelper.NewScaledSize(Math.Max(ControlDpiScalingHelper.ScaleToCurrentDpiX(292), Width), Math.Max(ControlDpiScalingHelper.ScaleToCurrentDpiY(138), Height), false);
			StartPosition = FormStartPosition.CenterScreen;
			Name = "ZForm";
			ResumeLayout(false);
			InitializeComponent();
		}

		void RegisterHotkeys()
		{
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Enter, HandleCtrlEnterKey, Res.GetString("04559c08-d8b2-4d3e-bad4-7f29c79440e7", "Click Apply or Post Button"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.D, HandleCtrlAltDAndCtrlShiftDKey, Res.GetString("8eebfaea-f5ec-4c0e-8963-40d3986f5580", "Show Developer Diagnostics Form(for developer use only)"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Alt | Keys.D, HandleCtrlAltDAndCtrlShiftDKey, Res.GetString("8eebfaea-f5ec-4c0e-8963-40d3986f5580", "Show Developer Diagnostics Form(for developer use only)"));
			Hotkeys.RegisterHotKey(Keys.Control | Keys.Shift | Keys.R, HandleCtrlShiftR, Res.GetString("393174d8-473c-4eae-a556-13c7129d62ad", "Show Control Information Form"));
		}

		void HandleCtrlEnterKey()
		{
			if (CommandButtonApply != null && CommandButtonApply.Enabled)
			{
				CommandButtonApply.Focus();
				CommandButtonApply.PerformClick();
			}
			else if (CommandButtonPost != null && CommandButtonPost.Enabled)
			{
				CommandButtonPost.Focus();
				CommandButtonPost.PerformClick();
			}
		}

		void HandleCtrlAltDAndCtrlShiftDKey()
		{
			if (DeveloperLoginForm.TryAuthenticate())
			{
				var tools = new List<IDevTool>();
				PopulateDevToolsInternal(tools);

				var host = new DeveloperDiagnosticsForm(this, tools);
				host.Show();
			}
		}

		void HandleCtrlShiftR()
		{
			using (var debugForm = new DebugControlInfoForm())
			{
				debugForm.ShowFormInfo(this);
			}
		}

		internal List<int> IndexesOfFocusedControl()
		{
			var result = new List<int>();
			helper(this, result);
			return result;

			void helper(Control parentControl, List<int> list)
			{
				for (var i = 0; i < parentControl.Controls.Count; ++i)
				{
					var control = parentControl.Controls[i];
					if (control.ContainsFocus)
					{
						result.Add(i);
						helper(control, result);
					}
				}
			}
		}

		internal static Control SelectControlBasedOnIndexes(Control parentControl, List<int> indexes)
		{
			for (var i = 0; i < indexes.Count; ++i)
			{
				try
				{
					parentControl = parentControl.Controls[indexes[i]];
				}
				catch (ArgumentOutOfRangeException)
				{
					//number of controls changed, e.g. if we used to be on a grid but now aren't.
					break;
				}
			}
			return parentControl;
		}

		internal static string SelectedTabs(ZForm form)
		{
			if (form.TopLevelTabControl?.SelectedTab == null)
			{
				return string.Empty;
			}

			var tabTopLevelTabControlText = string.IsNullOrEmpty(form.TopLevelTabControl.SelectedTab.Name) ? form.TopLevelTabControl.SelectedTab.Text : form.TopLevelTabControl.SelectedTab.Name;
			var result = new List<string> { tabTopLevelTabControlText };

			var currentTabControl = form.TopLevelTabControl;
			while (true)
			{
				//tab could have multiple sub tab pages -> just return first interesting (non-zero index) one, since function we're passing it to can't handle branches anyway.
				var nextTabControl = currentTabControl.SelectedTab.FindAll<ZTabControl>(x => x.SelectedIndex != 0).FirstOrDefault();
				if (nextTabControl?.SelectedTab == null)
				{
					break;
				}

				var selectedTabText = string.IsNullOrEmpty(nextTabControl.SelectedTab.Name) ? nextTabControl.SelectedTab.Text : nextTabControl.SelectedTab.Name;
				if (string.IsNullOrEmpty(selectedTabText))
				{
					break;
				}

				result.Add(selectedTabText);
				currentTabControl = nextTabControl;
			}

			return string.Join("+", result);
		}

		public ZForm ReloadForm(bool shouldCancel = false)
		{
			var controller = ZControllerFactory.Create(this.ControllerID);
			var businessEntity = this.BusinessEntity as BusinessObject;
			var oldFormDisplay = this.DisplayMode;
			var tabName = SelectedTabs(this);
			var indexesOfFocusedControl = IndexesOfFocusedControl();

			if (oldFormDisplay != ODisplayMode.Undefined)
			{
				if (controller != null)
				{
					this.Close();
#if WINZOR
					controller.InitialTabPageNameToSelectWhenAFormIsShown = tabName;
#endif
					var newForm = (ZForm)controller.ShowFormOfGivenDisplayType(businessEntity, oldFormDisplay);
					if (newForm != null)
					{
						newForm.InitialTabPageNameToSelectOnLoaded = tabName;
						newForm.InitialControlIndexesToFocus = indexesOfFocusedControl;

						var newBusinessEntity = newForm.BusinessEntity as BusinessObject;
						MakeCancellable(shouldCancel, newBusinessEntity);

#if !WINZOR
						newForm.Show();
#endif
						return newForm;
					}
					else
					{
						Globals.Message.ShowError(Res.GetString("5e686f5a-7c13-46d6-859d-3998a0ad580e", "Failed to reload the form. (Possibly this record no longer exists in the database?)"), Res.GetString("f99b1507-740f-493c-8c59-ff86a2531dfb", "Cannot Reload"));
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("bbf3b113-f57d-4bd8-b02f-ca15a9cc7759", "Failed to reload the form. (Could not create a controller for this Controller Id.)"), Res.GetString("f99b1507-740f-493c-8c59-ff86a2531dfb", "Cannot Reload"));
				}
			}
			else
			{
				// why are you reloading an undefined form?! How did you do this?!
				ErrorReporter.ReportOnce("We tried to perform ReloadForm on a form that has DisplayMode set to Undefined.");
			}
			return null;
		}

		static void MakeCancellable(bool shouldCancel, BusinessObject businessEntity)
		{
			if (shouldCancel && businessEntity is ICancellable cancellable && string.IsNullOrEmpty(cancellable.CanCancel()))
			{
				cancellable.IsCancelled = true;
			}
		}

		protected virtual void SaveToRecentItems()
		{
			var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
			var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(this);
			if (linkWrapper != null)
			{
				favoriteProvider.AddToRecentItems(linkWrapper);
			}
		}

		protected virtual void RemoveFromRecentItems()
		{
			var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
			var linkWrapper = ZFormUtilities.GetLinkWrapperForFavoriteOrRecent(this);
			if (linkWrapper != null)
			{
				favoriteProvider.RemoveFromRecentItems(linkWrapper);
			}
		}

		protected virtual void AddAdornments()
		{
			ZFormStrategy.AddAdornments(this);
		}

		#endregion

		#region ExtenderProviders

		protected readonly ControlVisibilityConfigurationProvider VisibilityConfigurationProvider = new ControlVisibilityConfigurationProvider();
		protected readonly ControlVisibilityRelationshipProvider VisibilityRelationshipProvider = new ControlVisibilityRelationshipProvider();
		protected readonly LabelCaptionRenderProvider LabelCaptionRenderProvider = new LabelCaptionRenderProvider();

		void InitializeExtenderProviders()
		{
#if DEBUG
			if (DesignModeFinder.IsDesigning)
			{
				TypeDescriptor.AddAttributes(DesignerActionExtenderProvider, DesignTimeVisibleAttribute.No);
				TypeDescriptor.AddAttributes(VisibilityConfigurationProvider, DesignTimeVisibleAttribute.No);
				TypeDescriptor.AddAttributes(VisibilityRelationshipProvider, DesignTimeVisibleAttribute.No);
				TypeDescriptor.AddAttributes(LabelCaptionRenderProvider, DesignTimeVisibleAttribute.No);
			}
#endif
		}

		#endregion

		#region Properties

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), DefaultValue(Constants.DataContext.None)]
		public
#if NET8_0_OR_GREATER && !WINZOR
			new
#endif
			Constants.DataContext DataContext
		{
			get { return fDataContext; }
			set { fDataContext = value; }
		}
		Constants.DataContext fDataContext = Constants.DataContext.None;

		[Browsable(true)]
		public bool RememberFormPosition
		{
			get { return fRememberFormPosition; }
			set { fRememberFormPosition = value; }
		}
		protected bool fRememberFormPosition = true;

		[Browsable(true)]
		public bool RememberFormSize
		{
			get { return fRememberFormSize; }
			set { fRememberFormSize = value; }
		}
		protected bool fRememberFormSize = true;

		public virtual bool AllowExportNativeXml
		{
			get { return true; }
		}

		public virtual bool IsResizableByTabPageAllowed
		{
			get { return false; }
		}

		internal protected virtual bool AllowActionDataMenuItem
		{
			get { return false; }
		}

		internal SizeF AutoScaleFactorForInternalUse
		{
			get { return AutoScaleFactor; }
		}

		internal Rectangle FormPositionRectangle { get; set; }

		Rectangle IPositionSaveProvider.FormPositionRectangle
		{
			get { return FormPositionRectangle; }
			set { FormPositionRectangle = value; }
		}

		internal WindowSystemMenu SystemMenu
		{
			get { return systemMenu ?? (systemMenu = new WindowSystemMenu(this)); }
		}
		WindowSystemMenu systemMenu;

		public new object DataSource
		{
			get { return base.DataSource ?? constructedDataSource; }
		}
		object constructedDataSource;

		public virtual IBusiness BusinessEntity
		{
			get { return DataSource as IBusiness; }
		}

		public virtual Guid IdentifierForPersistingForm
		{
			get
			{
				if (identifierForPersistingForm != Guid.Empty)
				{
					return identifierForPersistingForm;
				}

				var bizo =
					BusinessEntityForPersistingForm != null && BusinessEntityForPersistingForm.Identifier != ZGuid.Empty
						? BusinessEntityForPersistingForm
						: BusinessEntity != null && BusinessEntity.Identifier != ZGuid.Empty
							? BusinessEntity
							: null;

				if (bizo is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord && templateRecordProvider.TemplateRecord is IBusiness templateRecord)
				{
					bizo = templateRecord;
				}

				return bizo != null ? bizo.Identifier.ToGuid() : Guid.Empty;
			}
			protected internal set
			{
				identifierForPersistingForm = value;
			}
		}
		Guid identifierForPersistingForm;

		public virtual IBusiness BusinessEntityForPersistingForm
		{
			get
			{
				return businessEntityForPersistingForm ?? BusinessEntity;
			}
			protected internal set
			{
				businessEntityForPersistingForm = value;
			}
		}
		IBusiness businessEntityForPersistingForm;

		IBusiness IZForm.BusinessEntityForPersistingForm
		{
			get { return BusinessEntityForPersistingForm; }
			set { BusinessEntityForPersistingForm = value; }
		}

		Guid IZForm.IdentifierForPersistingForm
		{
			get { return IdentifierForPersistingForm; }
			set { IdentifierForPersistingForm = value; }
		}

		bool IZForm.IsActivityLogFinished { get; set; }

		public bool IsEditToDelete { get; set; }

		#region Form Caption

		/// <summary>
		/// The Caption part in the FormHeading
		/// </summary>
		public virtual string FormCaption
		{
			get
			{
				var formCaption = string.Empty;
				if (this.CaptionRenderingEnabled.HasValue && this.CaptionRenderingEnabled.Value)
				{
					var captions = ZLabelCaptionCache.Instance.GetCaptions(this);
					foreach (var caption in captions)
					{
						if (caption.Length > formCaption.Length)
						{
							formCaption = caption;
						}
					}
				}
				return formCaption;
			}
		}

		/// <summary>
		/// The heading shown on top of the form, a combination of a verb and a caption
		/// </summary>
		public virtual string FormHeading
		{
			get
			{
				var result = "";
				var formCaption = this.FormCaption;
				if (!string.IsNullOrEmpty(formCaption))
				{
					result = (FormVerb + " " + formCaption).Trim();
				}
				return result;
			}
		}

		/// <summary>
		/// The Verb part in the FormHeading
		/// </summary>
		public virtual string FormVerb
		{
			get
			{
				var verb = "";

				if (BusinessEntityForHasChanges != null)
				{
					var isInDB = (BusinessEntityForHasChanges is BusinessObject && ((BusinessObject)BusinessEntityForHasChanges).IsInDatabase)
						|| BusinessEntityForHasChanges.IsInDatabaseIncludingChildren;
					var isDelete = DisplayMode == ODisplayMode.Delete;
					var isView = DisplayMode == ODisplayMode.ReadOnly;

					if (isDelete)
					{
						var cancellable = GetICancellable(BusinessEntity);

						if (cancellable != null && PreventDeleteAttribute.IsTrue(BusinessEntity.GetType()))
						{
							verb = cancellable.IsCancelled ? FormVerbs.Deactivate : FormVerbs.Activate;
						}
						else
						{
							verb = FormVerbs.Delete;
						}
					}
					else if (isView)
					{
						verb = FormVerbs.View;
					}
					else if (isInDB)
					{
						verb = FormVerbs.Edit;
					}
					else
					{
						verb = FormVerbs.New;
					}
				}

				return verb;
			}
		}

		protected static class FormVerbs
		{
			public static string Activate { get { return Res.GetString("a6277c09-6767-4815-af4a-6a30a656758d", "Activate"); } }
			public static string Deactivate { get { return Res.GetString("9bf00ae2-e4a2-41b2-af8a-d9eca74d4e69", "Deactivate"); } }
			public static string Delete { get { return Res.GetString("FormVerb|Delete", "Delete"); } }
			public static string View { get { return Res.GetString("8bda2cdd-52d9-4a70-83a1-5927216ab6a9", "View"); } }
			public static string Edit { get { return Res.GetString("FormVerb|Edit", "Edit"); } }
			public static string New { get { return Res.GetString("FormVerb|New", "New"); } }
		}

		#endregion

		#region FormBorderStyle & MaximizeBox

		public new FormBorderStyle FormBorderStyle
		{
			get { return base.FormBorderStyle; }
			set
			{
				base.FormBorderStyle = value;
				UpdateMaximizeBox();
			}
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool MaximizeBox
		{
			get { return base.MaximizeBox; }
			set { }
		}

		void UpdateMaximizeBox()
		{
			base.MaximizeBox = (FormBorderStyle == FormBorderStyle.Sizable || FormBorderStyle == FormBorderStyle.SizableToolWindow);
		}

		#endregion

		#region Command Buttons

		internal IButton CommandButtonApply
		{
			get { return fApplyButton; }
		}
		protected IButton fApplyButton;

		IButton IPostingButtonsProvider.CommandButtonApply
		{
			get { return CommandButtonApply; }
		}

		internal IButton CommandButtonPost
		{
			get { return fPostButton; }
		}
		protected IButton fPostButton;

		IButton IPostingButtonsProvider.CommandButtonPost
		{
			get { return CommandButtonPost; }
		}

		internal IButton CommandButtonCancel
		{
			get { return fCancelButton; }
		}
		protected IButton fCancelButton;

		IButton IPostingButtonsProvider.CommandButtonCancel
		{
			get { return CommandButtonCancel; }
		}

		[DefaultValue(false), Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool IsPostOnly
		{
			get { return fIsPostOnly; }
			set
			{
				fIsPostOnly = value;
				if (fPostButton != null)
				{
					fPostButton.Text = ZFormPostingButtonsStrategy.PostButtonText(this).Text;
				}

				if (fApplyButton != null)
				{
					fApplyButton.Visible = !fIsPostOnly;
				}

				if (FileMenuItem != null)
				{
					var fileSaveAndCloseMenuItem = FileMenuItem.MenuItems.FindByName(ZFormMenuStrategy.FileSaveAndCloseMenuItemName);
					if (fileSaveAndCloseMenuItem != null)
					{
						fileSaveAndCloseMenuItem.Text = ZFormPostingButtonsStrategy.PostButtonText(this).Text;
					}
				}
			}
		}
		bool fIsPostOnly;

		bool IPostingButtonsProvider.AllowNew
		{
			get { return AllowNew; }
		}

		protected virtual bool AllowNew
		{
			get { return true; }
		}

		#endregion

		#region ShouldSerializeTabPageMethods

		[Category(ZGUIConstants.DesignerCategory)]
		public bool ShouldSerializeTabPageMethods
		{
			get { return shouldSerializeTabPageMethods ?? false; }
			set { shouldSerializeTabPageMethods = value; }
		}
		bool? shouldSerializeTabPageMethods;

		protected bool ShouldSerializeShouldSerializeTabPageMethods()
		{
			return shouldSerializeTabPageMethods != null;
		}

		[Browsable(false)]
		public bool IsShouldSerializeTabPageMethodsSpecified
		{
			get { return shouldSerializeTabPageMethods != null; }
		}

		#endregion

		#region Previous & Next Button Support

		public bool AutoAddPreviousNextButtons
		{
			get { return fAutoAddPreviousNextButtons; }
			set { fAutoAddPreviousNextButtons = value; }
		}
		bool fAutoAddPreviousNextButtons = true;

#if DEBUG
		ZPreviousNextControl IPreviousNextControlProvider.PreviousNextControlForTesting
		{
			get { return PreviousNextControlForTesting; }
			set { PreviousNextControlForTesting = value; }
		}
		protected internal ZPreviousNextControl PreviousNextControlForTesting;
#endif

		#endregion

		#region Security

		[Category("(K-Architecture)")]
		public string SecurityToken
		{
			get { return !string.IsNullOrEmpty(securityToken) ? securityToken : GetType().FullName; }
			set { securityToken = value; }
		}
		string securityToken;

		#endregion

		#endregion

		protected internal virtual bool ShouldRememberPositionAndSize => !Db.DatabaseUpgradedExceptionHasBeenThrownInConnection;

		bool hasRestoredSize;

#if DEBUG
		internal bool ForceRememberPositionAndSize { get; set; }
#endif

		#region Loading

		public void FormInitialSize()
		{
			if (!this.IsDesignMode())
			{
				SplitterLayoutStrategy.RestoreSplittersLayout(this);
				if (ShouldRememberPositionAndSize)
				{
					EnterpriseFormLookStrategy.RestorePositionAndSize(this);
					hasRestoredSize = true;
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Logging only")]
		protected override void OnLoad(EventArgs e)
		{
			if (ShouldRememberPositionAndSize)
			{
				EnterpriseFormLookStrategy.RestoreJustPositionAndSize(this, justPosition: hasRestoredSize);
				hasRestoredSize = true;
			}

			if (!this.IsDesignMode())
			{
				ZFormPostingButtonsStrategy.AddAdornments(this);
			}

			base.OnLoad(e);

			if (!this.IsDesignMode())
			{
				ZFormPlugInStrategy.SetupPluginsAfterLoad(this);
				SelectInitialTabPage();
				RefreshCaption();
				PostSetCaptionAgainWhenRemoteApp();
				ZFormMenuStrategy.SetMainMenuItemsPopupEventHandler(this);

#if DEBUG
				if (!(this is ZChildForm) && !SetupPostingCalled && !Globals.IsTest)
				{
					throw new ZException(
						"You must call SetupPosting() to use a ZWinForm or ZForm (for form " + GetType().FullName + "." +
						"Consider using ZChildForm instead if your form is not data-bound.");
				}
#endif
			}

			if (IsViewOrDeleteMode)
			{
				SetAllControlsReadOnly();
			}

			if (BusinessEntity != null && BusinessEntity.Factory != null && string.IsNullOrEmpty(BusinessEntity.Factory.NameForDebugging))
			{
				BusinessEntity.Factory.NameForDebugging = "Form: " + Text + (string.IsNullOrEmpty(Text) ? ", Type=" + GetType().FullName : "");
			}

			if (DisplayMode == ODisplayMode.Delete)
			{
				AcceptButton = fPostButton;
			}
		}

		public new IButtonControl AcceptButton
		{
			get => base.AcceptButton;
			set
			{
				acceptButtonWrapper?.Dispose();
				if (value != null && !(value is Control))
				{
					acceptButtonWrapper = new KButton();
					acceptButtonWrapper.Click += (object sender, EventArgs e2) => value.PerformClick();
					base.AcceptButton = acceptButtonWrapper;
				}
				else
				{
					base.AcceptButton = value;
				}
			}
		}
		KButton acceptButtonWrapper;

		void SelectInitialTabPage()
		{
			if (!InitialTabPageNameToSelectOnLoaded.IsEmpty && TopLevelTabControl != null)
			{
				var pageNames = InitialTabPageNameToSelectOnLoaded.Split('+');
				var page = TopLevelTabControl.GetTabPageByNameOrText(pageNames[0]);

				if (page != null)
				{
					TopLevelTabControl.SelectedTab = page;
					for (var i = 1; i < pageNames.Length && page != null; ++i)
					{
						page = page.FindAll<ZTabPage>(x => x.Name == pageNames[i] || x.Text == pageNames[i]).FirstOrDefault();
						if (page != null)
						{
							page.ParentTabControl.SelectedTab = page;
						}
					}
				}

				if (InitialControlIndexesToFocus != null)
				{
					SelectControlBasedOnIndexes(this, InitialControlIndexesToFocus)?.Focus();
				}
			}
		}

		#endregion

		#region Show Form

		public static void QuietlyShowForm(object form)
		{
			if (form is ZForm zForm)
			{
				try
				{
					zForm.FormInitialSize(); //has to be before layout is suspended, or maximized Organization form opens wrong.
											 //+ size has to not happen a second time during form.show or restored organization form overlaps posting buttons and main status bar
					_ = zForm.Handle; //make handle now so when SuspendDrawing checks before continuing, it exists
					zForm.SuspendDrawing(); //has to be before SuspendLayout, or terrible visual gunky things happen when opening forms maximized
					zForm.FormInitialSize(); //has to be called again, or the form size will recalled incorrectly (height offset a bit)
					zForm.SuspendLayout();
					zForm.Show();
				}
				finally
				{
					zForm.ResumeLayout();
					zForm.ResumeDrawing();
				}
			}
			else if (form is Form fForm)
			{
				fForm.Show();
			}
			else if (form is IZForm iZForm)
			{
				iZForm.Show();
			}
		}

		protected override void SetVisibleCore(bool value)
		{
			if (this.IsDisposed)
			{
				return;
			}

			if (value && !base.Visible)
			{
				ZFormPlugInStrategy.SetupPlugInsBeforeLoad(this);
				JustBeforeFirstTimeVisible();
			}
#if !WINZOR
			if (IsRemoteAppSession && value)
			{
				var cachedTopMost = TopMost;
				SetTopMost(true);
				try
				{
#if DEBUG
					if (Globals.IsTest)
					{
						ThrowExceptionForTestIfNeeded();
					}
#endif
					base.SetVisibleCore(value);
				}
				finally
				{
					SetTopMost(cachedTopMost);
				}
			}
			else
#endif
			{
				base.SetVisibleCore(value);
#if WINZOR
				if (!value)
				{
					this.DisposePendingUserAction();
				}
#endif
			}
		}

		protected virtual void SetTopMost(bool value)
		{
			TopMost = value;
		}

		protected virtual void JustBeforeFirstTimeVisible()
		{
			ZFormMenuStrategy.AddActionsMenuAfterFormIsLoaded(this);
		}

#if DEBUG
		protected virtual void ThrowExceptionForTestIfNeeded()
		{ }
#endif

		protected override void OnActivated(EventArgs e)
		{
			if (OpenedFormCache.LastActiveForm.Target != this)
			{
				OpenedFormCache.LastActiveForm.Target = this;
			}

			base.OnActivated(e);
		}

		protected override void OnShown(EventArgs e)
		{
			if (!this.IsDesignMode() && ShowStatusBar)
			{
				MainStatusBar.SendToBack();
			}

			base.OnShown(e);

			if (!this.IsDesignMode())
			{
				SelectInitialTabPage();
				ShowOtherUsersCurrentlyAccessingThisEntity();
				SaveToRecentItems();
				if (BusinessEntity is { } entity)
				{
					ProcessTemplateValidationManager.InjectFieldRules(entity);
				}
			}
		}

		public virtual void ShowOtherUsersCurrentlyAccessingThisEntity()
		{
			if (DisplayMode != ODisplayMode.ReadOnly)
			{
				if (BusinessEntity != null)
				{
					var beingEdited = ObjectsBeingEdited();
					if (beingEdited.Count > 0)
					{
						var editing = string.Empty;
						foreach (var entry in beingEdited)
						{
							if (editing.Length > 0)
							{
								editing += "\r\n";
							}

							editing += entry.Key + ":\r\n" + entry.Value;
						}
						var message = Res.GetString("ZForm|UserActionHeader", "These users are currently modifying {0} or one of its dependent objects:\r\n{1}\r\nYour modifications may not be able to be saved if the other user saves first (times shown in your local time zone).", FormCaption, editing) + "\r\n\r\n";
						Globals.Message.ShowInformation(message, Res.GetString("ZForm|UserActionCaption", "Another session is editing the same information"));
					}
					else if (CurrentUserEditingMainObject)
					{
						Globals.Message.ShowInformation(
							Res.GetString("8913fab9-6747-428c-ac8c-5aba1326b412", "You are currently modifying {0} in another form. You may be able to save modifications from only one form.", FormCaption),
							Res.GetString("b52dc895-5890-456a-bb4e-972e6a1a0cd8", "You are editing the same information"));
					}
				}
			}
		}

		public Dictionary<string, string> ObjectsBeingEdited(bool createNewSemaphore = true)
		{
			var objectsBeingEdited = new Dictionary<string, string>();
			var objects = GetListOfBizObjectsToCheckEditing();
			CurrentUserEditingMainObject = false;
			foreach (var bizObj in objects)
			{
				ISemaphoreType semaphore = new PendingUserActionSemaphore(bizObj.Key.Identifier.ToString());
				var pendingUserActions = SemaphoreProvider.GetActiveSemaphoreHandles(semaphore);
				if (pendingUserActions.Length > 0)
				{
					var userList = new StringBuilder();
					foreach (var userAction in pendingUserActions)
					{
						if (IsAnotherUserEditing(userAction))
						{
							userList.AppendLine("\t" + Res.GetString("ZForm|UserAction", "{0} - {1} since {2}",
								userAction.OwnerSession.FullUserName,
								userAction.OwnerSession.HostName,
								EnvProxy.Instance.Time.GetLocalTimeFromUtc(userAction.CreateTimeUtc)));
						}
						else if (bizObj.Key == BusinessEntity && !IsEditToDelete)
						{
							var parentZForm = ZFormModaliser.GetParentFormForModalForm(this) as ZForm;
							CurrentUserEditingMainObject = parentZForm == null || !HasSameSemaphore(parentZForm, semaphore);
						}
					}

					if (userList.Length > 0)
					{
						objectsBeingEdited.Add(bizObj.Value, userList.ToString());
					}
				}

				if (createNewSemaphore && bizObj.Key == BusinessEntity && !IsEditToDelete)
				{
					pendingUserActionSemaphoreHandle = SemaphoreProvider.CreateSemaphoreHandle(semaphore);

					if (!pendingUserActionSemaphoreHandle.Success)
					{
						if (EnvProxy.Instance.SemaphoreProvider.InternalHeartbeat.RegisterIfUserPkNotAlreadyInDatabase())
						{
							pendingUserActionSemaphoreHandle = SemaphoreProvider.CreateSemaphoreHandle(semaphore);
						}
						else
						{
							if (!Globals.IsTest)
							{
								//can still happen in the case where the user is logged in a second time and the old record just got purged due to staleness.
								//because of the new record, another old record will not be made (in RegisterIfUserPkNotAlreadyInDatabase() ) and this message will be reached.
								//maybe other circumstances under which this can happen too?
								Globals.Message.ShowDeveloperExceptionOnce("EnterprisePendingUserActionSemaphore", "Cannot create PendingUserAction semaphore.", pendingUserActionSemaphoreHandle.CreateException); // Localising diagnostic information is counterproductive.
							}
						}
					}
				}
			}

			return objectsBeingEdited;
		}

		bool CurrentUserEditingMainObject;

		/// <summary>
		/// Handle common exceptions that occur during Save.
		/// Unhandled exceptions are rethrown.
		/// Override HandleSaveException for custom handling.
		/// </summary>
		public void TryHandleSaveException(Exception ex) => HandleSaveException(ex);

		ISemaphoreProvider SemaphoreProvider
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && SemaphoreProvider_Exposed != null)
				{
					return SemaphoreProvider_Exposed;
				}
#endif
				return EnvProxy.Instance.SemaphoreProvider;
			}
		}

#if DEBUG
		internal ISemaphoreProvider SemaphoreProvider_Exposed;
#endif

		protected virtual bool IsAnotherUserEditing(ISemaphoreInfo userAction)
		{
			return !userAction.OwnerSession.HostName.StartsWith(System.Environment.MachineName) ||
				userAction.OwnerSession.ProcessId != Process.GetCurrentProcess().Id ||
				userAction.OwnerSession.UserPk != EnvProxy.Instance.CurrentUser.PK ||
				userAction.OwnerSession.FullUserName != EnvProxy.Instance.CurrentUser.FullName;
		}

		bool HasSameSemaphore(ZForm form, ISemaphoreType semaphore)
		{
			return form.pendingUserActionSemaphoreHandle != null &&
				form.pendingUserActionSemaphoreHandle.Success &&
				form.pendingUserActionSemaphoreHandle.Semaphore.Category == semaphore.Category &&
				form.pendingUserActionSemaphoreHandle.Semaphore.LockInfo == semaphore.LockInfo;
		}

		protected virtual Dictionary<IBusiness, ZString> GetListOfBizObjectsToCheckEditing()
		{
			var result = new Dictionary<IBusiness, ZString>();
			result.Add(BusinessEntity, FormCaption);
			return result;
		}

		/// <summary>
		/// Called by the "New" button.
		/// </summary>
		protected virtual void ShowNewForm()
		{
			if (ControllerID == null)
			{
				Globals.Message.ShowDeveloperErrorOnce("MissingController" + Name + GetType().FullName,
					"Form " + Name + " (" + GetType().FullName + ") cannot find its controller. Please create your form using a controller.", ""); // Localising diagnostic information is counterproductive.

				Globals.Message.ShowInformation(Res.GetString("7169a3c3-bd2d-4090-809e-573bb2bef9ea", "Cannot show a new form at this time."));
			}
			else
			{
				ZControllerFactory.Create(ControllerID).ShowNewForm();
			}

			if (NewButtonClick != null)
			{
				NewButtonClick(this, new NewButtonEventArgs(Location));
			}
		}

		public void LoadPersistedFormArgs(IEnumerable<string> args)
		{
			FormLoadedWithArgs?.Invoke(this, new FormLoadedWithArgsEventArgs(args));
		}

		protected internal event EventHandler<FormLoadedWithArgsEventArgs> FormLoadedWithArgs;

		public class FormLoadedWithArgsEventArgs : EventArgs
		{
			public IEnumerable<string> Args { get; }

			public FormLoadedWithArgsEventArgs(IEnumerable<string> args)
			{
				Args = args;
			}
		}

		protected internal virtual IEnumerable<string> GetFormArgsToPersistOnClose()
		{
			return null;
		}

		#endregion

		#region Closing

		internal int LastSaveCount;
		bool isClosing;
		bool forceClose;
#if DEBUG
		internal
#endif
		ISemaphoreHandle pendingUserActionSemaphoreHandle;

		protected override void OnClosing(CancelEventArgs e)
		{
			if (!forceClose)
			{
				if (!isClosing)
				{
					isClosing = true;
					try
					{
						ZForm_Closing(this, e);
						base.OnClosing(e);
						if (ShouldRememberPositionAndSize)
						{
							EnterpriseFormLookStrategy.SavePositionAndSize(this);
						}
						SplitterLayoutStrategy.SaveSplittersLayout(this);
					}
					finally
					{
						isClosing = false;
					}
				}
				else
				{
					e.Cancel = true;
				}
			}
		}

		public void ForceClose()
		{
			forceClose = true;
			Close();
		}

		protected virtual void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (!IsSavingInProgress &&
				(DisplayMode == ODisplayMode.Edit || DisplayMode == ODisplayMode.New ||
					DisplayMode == ODisplayMode.NewSaved && BusinessEntity != null && BusinessEntity.HasChanges))
			{
				e.Cancel = false;

				var cancelOptions = ShowSaveChangesDialog(e);

				switch (cancelOptions)
				{
					case DialogResult.Yes:
						HandleSaveWhileClosing(e);
						break;

					case DialogResult.No:
						if (CommandButtonCancel != null)
						{
							CommandButtonCancel.Focus();
						}
						break;

					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
		}

		protected virtual DialogResult ShowSaveChangesDialog(CancelEventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				WindowState = FormWindowState.Normal;
			}

			BringToFront();
			return Globals.Message.Show(FormClosingQuestion, Res.GetString("ZForm|UnsavedChangesCaption", "Warning"), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
		}

		public bool IsSavingInProgress
		{
			get { return savingInProgressIndex > 0; }
		}

		int savingInProgressIndex;

		IDisposable SetSavingInProgress()
		{
			return new SavingInProgress(this);
		}

#if DEBUG

		public IDisposable SetSavingInProgressExposedForTest() => SetSavingInProgress();

#endif

		class SavingInProgress : IDisposable
		{
			public SavingInProgress(ZForm form)
			{
				this.form = form;
				form.savingInProgressIndex++;
			}
			readonly ZForm form;
			#region IDisposable Members

			public void Dispose()
			{
				form.savingInProgressIndex--;
			}

			#endregion
		}

		protected virtual string FormClosingQuestion
		{
			get { return Res.GetString("ZForm|UnsavedChangesMessage", "This record has been modified.\r\nWould you like to save the changes?"); }
		}

		#endregion

		#region Saving and Deleting

		public enum ContinueWithDelete
		{
			Yes,
			No
		}

		public event EventHandler Saved;

		protected void FireSaved()
		{
			if (Saved != null)
			{
				Saved(this, EventArgs.Empty);
			}
		}

#if DEBUG
		internal
#endif
		protected virtual bool ExecuteAllFetchHintsBeforeValidateAll
		{
			get { return true; }
		}

		protected internal virtual IBusiness BusinessEntityForValidation
		{
			get { return BusinessEntity; }
		}

#if DEBUG

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		public void FireValidateAllForTest()
		{
			var fileMenuItem = Menu.MenuItems.FindByText("File");
			var validateAllMenu = fileMenuItem.MenuItems.FindByText("Validate All");

			validateAllMenu.PerformClick();
		}

#endif

		protected internal void ValidateAll(ValidationType type)
		{
			if (IsDisposed)
			{
				return;
			}

			if (BusinessEntity.Factory?.IsValidationSuspended ?? false)
			{
				return;
			}

			using (DeferUpdateSaveButtonsBasedOnHasChangesIfNeeded())
			{
				using (GetCellNotificationSuspender())
				using (BusinessEntity.Factory != null ? ActiveBusinessObjectCollection.DelayListChangedEvents(BusinessEntity.Factory) : null)
				{
					using (PerformanceStatisticsCollector.StartMonitoring("ZWinForm.SynchronisePlugInsOnSave", GetType().FullName))
					{
						UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|UpdatePlugins", "Updating plug-ins..."), 8);
						PlugIns.SynchronisePlugInsOnSave();
					}

					using (PerformanceStatisticsCollector.StartMonitoring("ZWinForm.RunPreSaveValidation()", GetType().FullName))
					{
						UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|ValidatingData", "Validating data..."), 15);
						if (type == ValidationType.Full)
						{
							BusinessEntityForValidation.MarkAsNeedingValidationIncludingChildren();
						}
						BusinessEntityForValidation.RunPreSaveValidationFetch(ExecuteAllFetchHintsBeforeValidateAll);
						if (BusinessEntityForValidation is IProgressReporterValidation progressReporterValidation)
						{
							using (progressReporterValidation.ReportProgress((message) => UpdateSaveProgressBoxIfEnabled(message, 18)))
							{
								BusinessEntityForValidation.RunPreSaveValidation();
							}
						}
						else
						{
							BusinessEntityForValidation.RunPreSaveValidation();
						}
						ProcessTemplateValidationManager.ValidateOnValidateAll(BusinessEntityForValidation);
					}

					UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|DisplayingNotifications", "Calculating errors, warnings and message errors..."), 50);
					if (BusinessEntityForValidation.HasErrors() || BusinessEntityForValidation.HasMessageErrors())
					{
						TabPageNotificationsExposer.ExposeTabPageNotificationsOnIdle(this, BusinessEntityForValidation);
					}
				}
				Refresh(); // some ZWrappedPropertyInfos in the grid don't refresh until a repaint
			}
		}

		SaveProgressMediator saveProgressMediator;

		internal void ShowProgressBoxIfEnabled(string loadingText)
		{
			if (ShowSaveProgressBox)
			{
				SetHideProgressDialogDelegateInModaliser();
				saveProgressMediator = new SaveProgressMediator();
				saveProgressMediator.ShowModalProgressForm(Bounds, loadingText, 5);
			}
		}

		internal protected static string LoadingValidationCodeText
		{
			get { return Res.GetString("ZForm|SaveProgress|LoadValidation", "Loading validation code..."); }
		}

		bool ShowSaveProgressBox
		{
			get
			{
				if (showSaveProgressBox == null)
				{
					showSaveProgressBox = EnvProxy.Instance.Registry.ShowSaveProgressBox && OSFeature.Feature.GetVersionPresent(OSFeature.LayeredWindows) != null;
				}
				return showSaveProgressBox.Value;
			}
		}
		bool? showSaveProgressBox;

		internal void DisposeProgressBoxIfEnabled()
		{
			ClearHideProgressDialogDelegateInModaliser();
			if (saveProgressMediator != null)
			{
				saveProgressMediator.Dispose();
				saveProgressMediator = null;
			}
		}

		void SetHideProgressDialogDelegateInModaliser()
		{
			if (!modaliserPreShowDelegateSet)
			{
				modaliserPreShowDelegateSet = true;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(delegate
				{
					if (saveProgressMediator != null)
					{
						saveProgressMediator.HideForm();

						if (IsHandleCreated)
						{
							BeginInvoke(new MethodInvoker(ZFormModaliser.ActivateMessageBoxIfShowing));
						}
					}
				});
			}
		}
		bool modaliserPreShowDelegateSet;

		internal void ClearHideProgressDialogDelegateInModaliser()
		{
			if (modaliserPreShowDelegateSet)
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				modaliserPreShowDelegateSet = false;
			}
		}

		void UpdateSaveProgressBoxIfEnabled(string status, int percentComplete)
		{
			if (saveProgressMediator != null && ShowSaveProgressBox)
			{
				SetHideProgressDialogDelegateInModaliser();
				saveProgressMediator.SetStatusAndPercentCompleteShowingFormIfItIsInvisible(status, percentComplete);
			}
		}

		protected internal DialogResult ShowErrorsDialog(bool includeIgnoreOption = false)
		{
			// ShowErrorsDialogCore should be called always, do not put it inside ?:
			var result = ShowErrorsDialogCore(includeIgnoreOption);
			return includeIgnoreOption ? result : DialogResult.Abort;
		}

		protected internal virtual DialogResult ShowErrorsDialogCore(bool includeIgnoreOption)
		{
			return ShowErrorsDialog(BusinessEntityForValidation, includeIgnoreOption);
		}

		internal DialogResult ShowErrorsDialog(IBusiness businessEntity, bool includeIgnoreOption = false)
		{
			var result = DialogResult.Abort;

#if DEBUG
			if (Globals.IsTest)
			{
				Globals.Message.ShowError(Res.GetString("6d1f056d-c44c-49c9-877a-375c12973236", "There are errors - can't save."), Res.GetString("38fb4d4a-deb0-489d-bfcf-7fe7bb345bbe", "Errors!"));
				result = ShowErrorsDialogCloseDialogResultForTest;
			}
			else
#endif
			{
				var messageBox = CreateErrorMessageBox(businessEntity, includeIgnoreOption);
				if (messageBox != null)
				{
					result = ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox);
					messageBox.Dispose();
				}
				else
				{
					ErrorReporter.ReportOnce("ZWinForm.ShowErrorsDialogCore." + businessEntity.GetType().FullName, "You must return valid, non-null ZError Message Box.");
				}
			}

			return result;
		}

		#region Test stuff
#if DEBUG
		public DialogResult ShowErrorsDialogCloseDialogResultForTest { get; set; } = DialogResult.OK;
		public bool IsFormToBash_ForTestOnly;
#endif
		#endregion

		protected virtual ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption = false)
		{
			return new ZErrorMessageBox(businessEntityForValidation, includeIgnoreOption);
		}

		IDisposable GetCellNotificationSuspender()
		{
			return new CellNotificationSuspender(this);
		}

		#region Save

		/// <summary>
		/// Override this to save extra factories / change factories being saved.
		/// Factories[] param is factories from PlugIns + Factory from top level
		/// business object.
		/// </summary>
		protected virtual void Save(ITransactionParticipant[] factories)
		{
			BusinessObjectFactory.SaveTogether(factories);
		}
#if DEBUG
		internal
#endif
		protected virtual void SaveInternal()
		{
			using (DeferUpdateSaveButtonsBasedOnHasChangesIfNeeded())
			using (GetCellNotificationSuspender())
			{
				if (BusinessEntity != null)
				{
					var factories = new List<ITransactionParticipant> { BusinessEntity.Factory };

					foreach (var factory in PlugIns.FactoriesToBeSaved)
					{
						if (!factories.Contains(factory))
						{
							factories.Add(factory);
						}
					}

					if (DisplayMode == ODisplayMode.Delete && factories.IndexOf(BusinessEntity.Factory) < factories.Count - 1)
					{
						factories.Remove(BusinessEntity.Factory);
						factories.Add(BusinessEntity.Factory);
					}

					Save(factories.ToArray());
				}
			}
		}

		IDisposable DeferUpdateSaveButtonsBasedOnHasChangesIfNeeded()
		{
			return isPostingButtonsAssigned ? ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(this) : new DisposableObject();
		}

		/// <summary>
		/// Override this function to show message boxes/dialogs
		/// before saving. If returned value is ContinueWithSave.No,
		/// the save process will be halted.
		/// </summary>
		protected
#if DEBUG
		internal
#endif
		virtual ContinueWithSave ShowPreSaveDialogs()
		{
			var result = PlugIns.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && BusinessEntity != null)
			{
				result = BusinessEntity.CanContinueWithSave ? ContinueWithSave.Yes : ContinueWithSave.No;
			}
			return result;
		}

		ContinueWithSave IShowPreSaveDialog.ShowPreSaveDialogs()
		{
			return ShowPreSaveDialogs();
		}

		/// <summary>
		/// Same as clicking the Save button in every case; this will never act as if it's clicking the New or Delete buttons!
		/// This function is ONLY virtual for .NET mock usage.
		/// Do not override it.
		/// </summary>
		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
#if DEBUG
		virtual // for .NET mocking
#endif
		public ContinueWithSave FireSaveButton(object sender = null)
		{
			var continueWithSave = ContinueWithSave.No;
			try
			{
				SaveFormProper(saveOnlyMode: true, sender: sender);
				if (this.LastSaveSucceeded)
				{
					continueWithSave = ContinueWithSave.Yes;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleSaveException(ex);
			}
			return continueWithSave;
		}

		IProcessTemplateValidationManager ProcessTemplateValidationManager => processTemplateValidationManager ??= ObjectFactory.Get<IProcessTemplateValidationManager>();
		IProcessTemplateValidationManager processTemplateValidationManager;

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		ContinueWithSave SaveFormCore(bool closeOnSave = false)
		{
			var continueWithSave = ContinueWithSave.No;
			ProcessTemplateValidationManager.ValidateOnSave(BusinessEntity, () => continueWithSave = ValidateAndSave());
			RefreshCaption();

			if (!closeOnSave && continueWithSave == ContinueWithSave.Yes)
			{
				DisplayMode = fDefaultDisplayMode;
			}

			LastSaveSucceeded = (continueWithSave == ContinueWithSave.Yes);
			return continueWithSave;
		}

		protected virtual INotificationHandler FormNotificationHandler
		{
			get { return NotificationHandler.Instance; }
		}

		#region Handle Save

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Testing")]
		void SaveFormProper(bool closeOnSave = false, object sender = null, bool saveOnlyMode = false)
		{
			if (!IsSavingInProgress && !IsDisposed)
			{
				using (closeOnSave ? null : GetCellNotificationSuspender())
				using (SetSavingInProgress())
				{
					FocusButton(sender);
					HandleApplyPostingButtonClick(closeOnSave: closeOnSave, saveOnlyMode: saveOnlyMode);

					if (BusinessEntity != null && !closeOnSave && !saveOnlyMode)
					{
						using (BusinessEntity.Factory != null ? ActiveBusinessObjectCollection.DelayListChangedEvents(BusinessEntity.Factory) : null)
						{
							PullBindingsFromBizo(this);
						}
					}
#if DEBUG
					if (Globals.IsTest && AllowToSimulateAnotherButtonClickInside_ForTestOnly)
					{
						Application.DoEvents();
					}
#endif
				}
			}
		}

		void PullBindingsFromBizo(Control control)
		{
			foreach (var binding in control.DataBindings.Cast<Binding>().ToArray())
			{
				try
				{
					binding.ReadValue();
				}
				catch (IndexOutOfRangeException ex)
				{
					var error = FormattableString.Invariant($@"Control: {control.GetType().FullName}
Binding Control: {binding.Control.GetType().FullName}
DataSource: {binding.DataSource}
Binding Field: {binding.BindingMemberInfo.BindingField}
IsBinding:{binding.IsBinding}"); // Error message
					throw new IndexOutOfRangeException(error, ex);
				}
			}

			foreach (Control child in control.Controls)
			{
				PullBindingsFromBizo(child);
			}
		}

		protected virtual void HandleSaveException(Exception ex)
		{
			var handleError = BusinessEntity as IHandleDeleteError;
			var disableForm =
				DisplayMode == ODisplayMode.Delete
				&& ex is ZSaveConcurrencyException
				&& handleError != null
				&& handleError.DisableFormOnDeleteConcurrencyError;

			try
			{
				ZExceptionReporting.HandleSaveException(ex, FormNotificationHandler, this);
			}
			catch (Exception e) when (e.Find<ZSaveConcurrencyException>() != null) // This should never happen since it's handles in HandleSaveException
			{
				Globals.Message.ShowError(
					Res.GetString(
						"b28ea99d-e365-4f57-8d97-f2f269990057"
						, "There was an unresolved concurrency error preventing saving this {0}. Please try to reopen this {0}, re-enter your changes, and try to Save again."
						, BusinessEntity.HumanReadableName)
					+ "\r\n"
					+ e.Message);
			}
			catch (Exception e) when (e.Find<ZSaveErrorAfterCommitInDbException>() != null)
			{
				Globals.Message.ShowError(
					Res.GetString(
						"61eafe8e-1119-4214-9d67-eae76ea17df9"
						, "There was an error during saving this {0}, but some or all data has been saved to database. Please close this form, check if your changes were saved, and repeat if required."
						, BusinessEntity.HumanReadableName)
					+ "\r\n"
					+ e.Message);
				disableForm = true;
			}
			catch (Exception e) when (e.Find<SqlLockLostException>() != null)
			{
				Globals.Message.ShowInformation(
					Res.GetString("82408150-b456-44e1-b298-efec89d2292b",
						"Connection was reset during saving. Please try to Save again."));
			}
			catch (Exception e) when (e.Find<System.Data.Common.DbException>() != null)
			{
				var sqlException = e.GetFirstOccurrenceOfException<System.Data.Common.DbException>(matchExactType: false);
				var dbErrorMatch = new DbErrorMatch(sqlException);
				if (dbErrorMatch.ExceptionType == DbErrorType.LoopbackLinkedServerDoesNotExist)
				{
					Globals.Message.ShowError(dbErrorMatch.GetUserFriendlyMessage(Db.Connection));
				}
				else
				{
					var message = sqlException.Message;

					if (SystemDataRegistry.Instance.EDocsStorageProvider.Value != Enterprise.Core.Constants.EDocsStorageProviders.Code.DB
						&& message.Contains("_SD") && message.Contains(".dbo.StorageDocs")) // programmatic constant
					{
						if (SystemDataRegistry.Instance.EDocsStorageProvider.Value == Enterprise.Core.Constants.EDocsStorageProviders.Code.S3)
						{
							Globals.Message.ShowInformation(
								Res.GetString("4c14fe24-e54f-4195-9287-3a52577e2282",
							"Error occurred trying to save to the database. eDocs Storage registry setting is set to S3 compatible storage and it is incorrectly configured. Contact your system administrator."));
						}
						else
						{
							Globals.Message.ShowInformation(
								Res.GetString("a10ab6d7-f193-437b-80f9-d7165ed75773",
							"Error occurred trying to save to the database. eDocs Storage registry setting is set to {0} and it is incorrectly configured. Contact your system administrator.",
							SystemDataRegistry.Instance.EDocsStorageProvider.Value));
						}
					}
					else
					{
						Globals.Message.ShowInformation(
							Res.GetString("60BA36C4-D652-4EA1-AE95-1FE626DB7447",
								"Error occurred trying to save to the database. Please try to Save again or contact your systems administrator to check the database for errors. ") + message);
					}
				}
			}
			catch (Exception e) when (e.GetBaseException() is TransactionException transactionException && transactionException.ErrorType == OdysseyDataErrorType.TransactionRolledBack)
			{
				Globals.Message.ShowInformation(
						Res.GetString("D7C8B5F3-FBF4-4B35-96B0-94214F34B7E6",
							"There was transaction related error on DB server. Please try to Save again. If error repeats, please reopen form and try again."));
			}
			catch (Exception e) when (e.Find<EmptyContentEDocsException>() != null)
			{
				Globals.Message.ShowError(
					Res.GetString(
						"BABAC943-23B1-401B-87DF-FF956E1FDAE9",
						"Error occurred trying to save to the database. Error: '{0}'. Please try to Save again. If error repeats, please reopen form and try again.",
						e.Message));
			}
			finally
			{
				if (disableForm)
				{
					DisplayMode = ODisplayMode.ReadOnly;
					SetAllControlsReadOnly();
				}
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal bool NoValidationOnSave { get; set; }

		protected virtual void HandleSaveWhileClosing(CancelEventArgs e)
		{
			//to force zgrid to commit most recently added row, if any
			if (CommandButtonCancel != null)
			{
				CommandButtonCancel.Focus();
			}

			Focus();
			HandleApplyPostingButtonClick(false);
			if (!LastSaveSucceeded)
			{
				e.Cancel = true;
			}
		}

		internal ICancellable GetICancellable(IBusiness businessEntity) => CancellableHelper.GetICancellable(businessEntity);

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "This is as simple as it can get; any reduction in complexity impedes on code functionality.")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Only enable in ShipmentForm, this should be a temp change, log for AppTransactionCount issue")]
		void HandleApplyPostingButtonClick(bool closeOnSave, bool saveOnlyMode = false)
		{
			if (IsDisposed || isSaving)
			{
				return;
			}

			var appTransactionCount = -1;
			string lastExceptionMessage = null;
			isSaving = true;

			try
			{
				var connection = ((IDbConnected)BusinessEntity?.Factory)?.Connection;
				if (connection != null)
				{
					var maxAppTransactionCount = 0;
#if DEBUG
					if (Globals.IsTest)
					{
						maxAppTransactionCount = 1;
					}
#endif
					if (connection.AppTransactionCount > maxAppTransactionCount)
					{
						// Maybe something is being saved in multiple threads using same connection
						for (var repeats = 3; (repeats > 0) && (connection.AppTransactionCount > maxAppTransactionCount); repeats--)
						{
							Thread.Sleep(100);
						}

						if (connection.AppTransactionCount > maxAppTransactionCount)
						{
							ErrorReporter.ReportOnce("ZForm_HandleSaveButton_AlreadyInTransaction",
								"Connection.AppTransactionCount was more than " + maxAppTransactionCount + " when ZForm.HandleSaveButton() was called." +
								"\r\nCurrent value = " + connection.AppTransactionCount);
						}
					}

					appTransactionCount = connection.AppTransactionCount;

					if (GetType().FullName == "Enterprise.Freight.Forwarding.GUI.ShipmentForm")
					{
						connection.EnableAppTransactionCountLog();
					}
				}

				var bizO = BusinessEntity as BusinessObject;
				using (new ZWaitCursorChanger(this))
				using (bizO != null && !NoValidationOnSave ? ((IBusinessObjectInternals)bizO).ResumeValidationForAllDescendantsTemporarily() : null)
				{
					try
					{
						LastSaveSucceeded = false;
						if (saveOnlyMode)
						{
							SaveFormCore();
						}
						else
						{
							HandleApplyPostingButtonClickUnsafe(closeOnSave);
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						lastExceptionMessage = ex.Message;

						try
						{
							if (DisplayMode == ODisplayMode.Delete)
							{
								var isFKViolationException = IsFKViolationException(ex);

								if (!isFKViolationException)
								{
									HandleSaveException(ex);
								}

								var handleError = BusinessEntity as IHandleDeleteError;
								// rollback must be after the concurrency resolver, but before cancelling
								if (BusinessEntity != null && (handleError == null || handleError.RollbackAfterDeleteError))
								{
									var wasCancelled = !bizO.IsDeleted && bizO is ICancellable cancellable && cancellable.IsCancelled;

									((IBusinessObjectFactoryInternals)BusinessEntity.Factory)?.Rollback();

									if (handleError != null && !handleError.RebindAfterDeleteError)
									{
										Close();
									}
									else if (isFKViolationException)
									{
										TryCancelInstead(ex);
									}
									else
									{
										RefreshAfterDeleteFailure(wasCancelled, bizO.IsDeleted);
									}
								}
							}
							else
							{
								HandleSaveException(ex);

								var bizo = BusinessEntity as BusinessObject;
								if (bizo != null && bizo.IsDeleted)
								{
									Globals.Message.ShowError(Res.GetString("856eeffa-7492-42b5-a978-d6f81d940b6d", "Because this {0} has been deleted while you were editing it, this form will be closed.", BusinessEntity.HumanReadableName));
									this.ForceClose();
								}
							}
						}
						catch (Exception innerEx) when (!innerEx.IsCriticalException())
						{
							var innermostException = innerEx.GetInnermostException();
							var message = innermostException.Message;
							var exMsg = string.Join(
								"|"
								, new[] { "HandleSaveButton_ExceptionHandle_Exception", message == null ? string.Empty : message.Substring(0, Math.Min(message.Length, 150)) }.
									Concat(innermostException.StackTrace?.Split('\n').Take(6) ?? new List<string>()).
									Concat(new[] { innerEx.TargetSite?.Name }).
									Concat(new[] { "HandleSaveButton_Exception", ex.Message?.Substring(0, Math.Min(ex.Message.Length, 150)) ?? string.Empty }).
									Concat(ex.StackTrace?.Split('\n').Take(6) ?? new List<string>()).
									Concat(new[] { ex.TargetSite?.Name }).
									Where(s => !string.IsNullOrEmpty(s)));

							ExceptionReporter.Instance.ReportException(exMsg, innerEx);
						}
					}
				}
			}
			finally
			{
				isSaving = false;

				var connection = ((IDbConnected)BusinessEntity?.Factory)?.Connection;
				if (connection != null && appTransactionCount >= 0 && connection.AppTransactionCount != appTransactionCount)
				{
					// Maybe something is being saved in multiple threads using same connection
					for (var repeats = 3; (repeats > 0) && (connection.AppTransactionCount != appTransactionCount); repeats--)
					{
						Thread.Sleep(100);
					}

					if (connection.AppTransactionCount != appTransactionCount)
					{
						var lastException = !string.IsNullOrEmpty(lastExceptionMessage)
							? "\r\nSave exception: " + lastExceptionMessage
							: string.Empty;

						var appTransactionCountChangedLog = connection.GetAppTransactionCountChangedLog() is string appTransactionCountLog
							? "\r\nAppTransactionCountLog:" + appTransactionCountLog
							: string.Empty;

						ErrorReporter.ReportOnce("ZForm_HandleSaveException_StillInTransaction",
							"Connection.AppTransactionCount is different after ZForm.HandleSaveButton()." +
							"\r\nOriginal value = " + appTransactionCount + ", new value = " + connection.AppTransactionCount +
							lastException + appTransactionCountChangedLog);
					}
				}

				connection?.DisableAppTransactionCountLog();
			}
		}

		bool isSaving;

		DialogResult ShowDialogWithHyperlink(string caption, string topMessage, string detailMessage)
		{
			var hyperlinkMessages = new HyperlinkAlertBusinessObject(new ZString(topMessage), new ZString(detailMessage));
			using (var hyperlinkAlertForm = new HyperlinkAlertForm(hyperlinkMessages))
			{
				hyperlinkAlertForm.Text = caption;
				var result = ZFormModaliser.ShowDialogWithoutDispose(hyperlinkAlertForm);
				return result;
			}
		}

		void TryCancelInstead(Exception ex)
		{
			var cancellable = GetICancellable(BusinessEntityForValidation);
			if (cancellable == null)
			{
				var message = GetMessageForCannotDeleteRecordInUseException(ex);
				var caption = Res.GetString("ZForm|DeleteErrorCaption", "Cannot Delete...");
				if (!string.IsNullOrEmpty(message) && message.Length > 200)
				{
					var summary = Res.GetString("8C940B6C-C45B-4AD3-BE6A-DD1086029787", @"This record is in use by one or more record(s) with the following descriptions, and thus cannot be deleted. Please click View Details below to see full list.");
					ShowDialogWithHyperlink(caption, summary, message);
				}
				else
				{
					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			else if (!((BusinessObject)cancellable).IsDeleted)
			{
				var reasonWeCantCancel = cancellable.CanCancel();
				if (string.IsNullOrEmpty(reasonWeCantCancel))
				{
					if (!cancellable.IsCancelled)
					{
						var message = Res.GetString("ZForm|DeleteErrorMessage", "This record is in use by other records in the system. Would you like to mark this record as Inactive?");
						var caption = Res.GetString("ZForm|DeleteErrorCaption", "Cannot Delete...");
						var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (result == DialogResult.Yes)
						{
							ReloadAndDeactivateInsteadOfDelete();
						}
						else
						{
							HandleSaveException(ex);
						}
					}
					else
					{
						ShowDeleteErrorInactiveMessage();
					}
				}
				else
				{
					Globals.Message.ShowInformation(reasonWeCantCancel);
				}
			}
		}

		protected virtual ZForm ReloadAndDeactivateInsteadOfDelete()
		{
			DisplayMode = ODisplayMode.ReadOnly;
			var newForm = ReloadForm(true);
			if (newForm != null)
			{
				newForm.DisplayMode = ODisplayMode.Edit;
				newForm.RefreshCaption();
				ZFormPostingButtonsStrategy.UpdateSaveButtonsBasedOnHasChanges(newForm);
			}
			return newForm;
		}

		protected virtual void ShowDeleteErrorInactiveMessage()
		{
			Globals.Message.ShowInformation(Res.GetString("ZForm|DeleteErrorInactiveMessage", "This record is in use by other records in the system and can't be deleted. It's marked as Inactive already."));
		}

		protected virtual string GetMessageForCannotDeleteRecordInUseException(Exception ex)
		{
			return Res.GetString("e583390d-75c8-4d6c-bb5c-998c83c446dd", "This record is in use by other records in the system and cannot be deleted.");
		}

		void RefreshAfterDeleteFailure(bool wasCancelled, bool isDeleted)
		{
			var entity = BusinessEntity as BusinessObject;

			if (isDeleted)
			{
				Globals.Message.ShowInformation(Res.GetString("ZForm|DeleteErrorAlreadyDeleted", "This record has already been deleted. The form will now be closed."));
				Close();
			}
			else if (entity != null && ControllerID != null)
			{
				Globals.Message.ShowInformation(Res.GetString("8169be69-3707-49dd-982f-518964e45a7d", "This form has been modified by another user. It will now be reloaded."));
				ReloadForm(wasCancelled);
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("97090b90-a634-4495-b30e-b867f103a081", "This record has been modified by another user. Please reload this form."));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "sql error message")]
		protected bool IsFKViolationException(Exception ex)
		{
			return (ex.InnerException != null && ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"));
		}

#if DEBUG
		protected internal virtual
#endif
		void HandleApplyPostingButtonClickUnsafe(bool closeOnSave)
		{
			if (DisplayMode == ODisplayMode.Delete)
			{
				var cancellable = GetICancellable(BusinessEntityForValidation);
				if (cancellable != null && PreventDeleteAttribute.IsTrue(cancellable.GetType()))
				{
					var deactivating = cancellable.IsCancelled && string.IsNullOrEmpty(cancellable.CanCancel());
					var activating = !cancellable.IsCancelled && string.IsNullOrEmpty(cancellable.CanReactivate());
					if (deactivating || activating)
					{
						if (deactivating || (activating && !BusinessEntityForValidation.HasErrors()))
						{
							try
							{
								ShowProgressBoxIfEnabled(LoadingValidationCodeText);
								UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|SavingToTheDatabase", "Saving to the database..."), 50);
								SaveInternal();
								UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|SaveSuccessfull", "Saved successfully."), 100);
								FireSaved();
							}
							finally
							{
								DisposeProgressBoxIfEnabled();
							}

							LastSaveSucceeded = true;
							RefreshCaption();

							if (closeOnSave)
							{
								Close();
							}
						}
						else
						{
							Globals.Message.Show(Res.GetString("E4165321-DD6B-4890-A8F1-5DF352AE4AA7", "Please fix errors on this entity before Activating"));
						}
					}
					else
					{
						var message = cancellable.IsCancelled ? cancellable.CanCancel() : cancellable.CanReactivate();
						Globals.Message.Show(message);
					}
				}
				else if (ShowPreDeleteDialogs() == ContinueWithDelete.Yes)
				{
					Delete();
					Close();
				}
			}
			else if (DisplayMode == ODisplayMode.Edit || DisplayMode == ODisplayMode.New || DisplayMode == ODisplayMode.NewSaved)
			{
				var @continue = SaveFormCore(closeOnSave: closeOnSave);

				if (@continue == ContinueWithSave.Yes)
				{
					if (closeOnSave)
					{
						Close();
					}
					else if (OriginalDisplayMode == ODisplayMode.New || OriginalDisplayMode == ODisplayMode.NewSaved)
					{
						OriginalDisplayMode = fDefaultDisplayMode;
					}
				}
			}
			else if (DisplayMode == ODisplayMode.Browse)
			{
				ShowNewForm();
				Close();
			}
		}

		#endregion

		#region HasChanges support

		public virtual IBusiness BusinessEntityForHasChanges
		{
			get { return DataSource as IBusiness; }
		}

		internal ODisplayMode OriginalDisplayMode
		{
			get { return fOriginalDisplayMode; }
			set { fOriginalDisplayMode = value; }
		}
		ODisplayMode fOriginalDisplayMode = ODisplayMode.Undefined;

		#endregion

		#region RegisterControlToBeBoundOnPreSaveValidation

		public void RegisterControlToBeBoundOnPreSaveValidation(IBoundOnPreSaveValidation controlToBeBoundOnSave)
		{
			if (ControlsToBeBoundOnSave == null)
			{
				ControlsToBeBoundOnSave = new HashSet<IBoundOnPreSaveValidation>();
			}

			if (!ControlsToBeBoundOnSave.Contains(controlToBeBoundOnSave))
			{
				ControlsToBeBoundOnSave.Add(controlToBeBoundOnSave);
				controlToBeBoundOnSave.AfterFirstBinding += ControlToBeBoundOnSave_AfterFirstBinding;
			}
		}

		void ControlToBeBoundOnSave_AfterFirstBinding(object sender, EventArgs e)
		{
			var controlToBeBoundOnSave = (IBoundOnPreSaveValidation)sender;
			UnRegisterControlForBindingOnPreSaveValidation(controlToBeBoundOnSave);
		}

		void UnRegisterControlForBindingOnPreSaveValidation(IBoundOnPreSaveValidation controlToBeBoundOnSave)
		{
			ControlsToBeBoundOnSave.Remove(controlToBeBoundOnSave);
			controlToBeBoundOnSave.AfterFirstBinding -= ControlToBeBoundOnSave_AfterFirstBinding;
		}

		HashSet<IBoundOnPreSaveValidation> ControlsToBeBoundOnSave
		{
			get { return controlsToBeBoundOnSave; }
			set
			{
				if (value == null && controlsToBeBoundOnSave != null)
				{
					foreach (var control in controlsToBeBoundOnSave)
					{
						if (control != null)
						{
							control.AfterFirstBinding -= ControlToBeBoundOnSave_AfterFirstBinding;
						}
					}
				}
				controlsToBeBoundOnSave = value;
			}
		}
		HashSet<IBoundOnPreSaveValidation> controlsToBeBoundOnSave;

		#endregion

		#region ValidateAndSave

		protected
#if DEBUG
		internal
#endif
		virtual ContinueWithSave ValidateAndSave()
		{
			if (BusinessEntityForValidation == null)
			{
				Globals.Message.ShowDeveloperErrorOnce("NullBusinessEntityForValidation", "BusinessEntityForValidation does not exist. ValidateAndSave() method is aborted.", "ValidateAndSave");
				return ContinueWithSave.No;
			}
#if WINZOR
			//In Winzor sometimes Disposing is true which can lead to Dispose of BussinessEntity making it null
			if (IsDisposed || Disposing)
#else
			if (IsDisposed)
#endif
			{
				if (disposeStack != null)
				{
					ErrorReporter.ReportOnce("FormAlreadyDisposedInValidateAndSave", "The form was disposed during ValidateAndSave() method.\r\n\r\nLocation:\r\n" + disposeStack.ToString());
				}
				return ContinueWithSave.No;
			}

			var result = ContinueWithSave.No;

			using (PerformanceStatisticsCollector.StartMonitoring("ZWinForm.ValidateAndSave()", GetType().FullName))
			using (new ZFormModaliser.ActiveFormOverride(this))
			using (DeferUpdateSaveButtonsBasedOnHasChangesIfNeeded())
			using (GetCellNotificationSuspender())
			{
				try
				{
					ShowProgressBoxIfEnabled(LoadingValidationCodeText);

					ZFormModaliser.EnableForm(this, false);

					BindControlsForPreSaveValidation();
					if (BeforePerformValidation != null)
					{
						BeforePerformValidation(this, EventArgs.Empty);
					}

					PerformValidation();

					UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|CheckingData", "Checking data..."), 60);

					var includeIgnoreOption = BusinessEntityForValidation is ITemplateRecord templateRecord &&
						DataRegistry.Instance.TemplateRecordValidation == RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave;

					var hasErrors = BusinessEntityForValidation.HasErrors();
					var shouldIgnoreErrors = hasErrors && ShowErrorsDialog(includeIgnoreOption) == DialogResult.Ignore;

					if ((!hasErrors || shouldIgnoreErrors) &&
						ShowPreSaveDialogs() == ContinueWithSave.Yes)
					{
						result = OnValidatingForSave();
					}

					if (result == ContinueWithSave.Yes &&
						!shouldIgnoreErrors && BusinessEntityForValidation.HasErrors() && // Check for new errors as ShowPreSaveDialogs may change objects
						ShowErrorsDialog(includeIgnoreOption) != DialogResult.Ignore)
					{
						result = ContinueWithSave.No;
					}

					if (result == ContinueWithSave.Yes)
					{
						PerformSave();
					}
				}
				finally
				{
					PlugIns.SynchronisePlugInsOnSaveCompletedOrAborted(result == ContinueWithSave.Yes);
					ZFormModaliser.EnableForm(this, true);
					DisposeProgressBoxIfEnabled();
				}
			}

			return result;
		}

		public event EventHandler BeforePerformValidation;

		protected internal virtual void PerformValidation()
		{
			ValidateAll(ValidationType.Light);
		}

		void BindControlsForPreSaveValidation()
		{
			if (ControlsToBeBoundOnSave != null)
			{
				foreach (Control control in ControlsToBeBoundOnSave.ToArray())
				{
					if (!control.IsDisposed)
					{
						control.ForceBindingIncludingParents();
					}
				}

				ControlsToBeBoundOnSave = null;
			}
		}

		void PerformSave()
		{
			UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|SavingToTheDatabase", "Saving to the database..."), 80);
			SaveInternal();
			UpdateSaveProgressBoxIfEnabled(Res.GetString("ZForm|SaveProgress|SaveSuccessfull", "Saved successfully."), 100);
			FireSaved();
			((ISaveInitiator)this).SaveExceptionCaughtAlready = false;
			Activate();
			SaveToRecentItems();
		}

		ContinueWithSave OnValidatingForSave()
		{
			var e = new ValidatingForSaveEventArgs();
			if (ValidatingForSave != null)
			{
				ValidatingForSave(this, e);
			}
			return e.ContinueWithSave;
		}

		public event ValidatingForSaveEventHandler ValidatingForSave;

		#endregion

		#endregion

		#region Enable/Disable core

		int disableCount;

		internal bool IsEnabledCore
		{
			get { return disableCount <= 0; }
		}

		internal void UpdateDisableCount()
		{
			disableCount++;
		}

		internal void UpdateEnableCount()
		{
			disableCount--;
		}

		#endregion

		#region Delete

		internal protected virtual void Delete()
		{
			PlugIns.Delete();
			DeleteCore();
			RemoveFromRecentItems();
			SaveInternal();
		}

		protected virtual void DeleteCore()
		{
			BusinessEntity.Delete();
		}

		protected virtual ContinueWithDelete ShowPreDeleteDialogs()
		{
			var result = PlugIns.ShowPreDeleteDialogs();

			if (result == ContinueWithDelete.Yes && ShowConfirmationForDelete() == DialogResult.No)
			{
				result = ContinueWithDelete.No;
			}

			return result;
		}

		#endregion

		#endregion

		#region Dispose

		protected void DisposePendingUserAction()
		{
			if (pendingUserActionSemaphoreHandle != null)
			{
				pendingUserActionSemaphoreHandle.Dispose();
			}
		}

		[DefaultValue(false)]
		[Browsable(false)]
		public bool TrackDisposedAccess
		{
			get { return trackDisposedAccess; }
			set { trackDisposedAccess = value; }
		}
		bool trackDisposedAccess = true;

		public StackTrace DisposeStack
		{
			get { return disposeStack; }
		}
		StackTrace disposeStack;

		public string DisposeControlPath
		{
			get { return disposeControlPath; }
		}
		string disposeControlPath;

		public bool IsDisposing { get; private set; }

		DisposableManager disposableManager;

#if DEBUG
		public string OwnerName_ForTest;
#endif

#if WINZOR
		/// <summary>
		/// Reimplement the WM_CLOSE logic from ZForm.WndProc in Winzor
		/// </summary>
		protected override void WMClose()
		{
			if (IsEnabledCore)
			{
				base.WMClose();
			}
		}
#endif

		protected virtual bool AllowDisposeError_ForTest => false;

		protected override void Dispose(bool disposing)
		{
			if (!disposing && (!Globals.IsTest || AllowDisposeError_ForTest))
			{
				ErrorReporter.ReportDeveloperExceptionOnce("Form disposed by finalizer", new Exception($"Form {GetType().FullName} disposed by finalizer"));
			}

			if (IsDisposing)
			{
				base.Dispose(disposing);
				return;
			}

			Form owner = null;
			try
			{
				this.SuspendDrawing();
				this.SuspendLayout();
				try
				{
					owner = Owner;
					this.Visible = false;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					//might have tried to have a side-effect like accessing BusinessEntity - that's fine then, we're disposing, just ignore it
				}
				if (disposing)
				{
					IsDisposing = true;
					DisposeCore();
				}
			}
			finally
			{
				try
				{
#if DEBUG
					if (Globals.IsTest)
					{
						OwnerName_ForTest = owner?.GetType()?.Name;
					}
#endif
					base.Dispose(disposing);
#if !WINZOR
					MaybeNukeAllEvents();
#endif
					this.FactoryChanged = null;

					if (TrackDisposedAccess)
					{
						disposeStack = new StackTrace();
						disposeControlPath = ControlDescription.GetControlPath(this);
					}

#if !WINZOR
					TryToActivateOwnerFormIfIsRemoteAppSession(new[] { owner, ParentFormInZFormModaliser });
#endif
				}
				finally
				{
					IsDisposing = false;
				}
			}
		}

#if !WINZOR
		// Only deal with the parent form cannot be focused if this form opened by ZFormModaliser.ShowInternal
		// Just don't change this property to public
		internal Form ParentFormInZFormModaliser { get; set; }

		void TryToActivateOwnerFormIfIsRemoteAppSession(Form[] forms)
		{
			if (IsRemoteAppSession)
			{
				foreach (var form in forms)
				{
					if (form != this &&
						form is { IsDisposed: false, IsHandleCreated: true } and not ZForm { IsDisposing: true })
					{
						form.Activate();
					}
				}
			}
		}
#endif

#if !WINZOR
		protected virtual void MaybeNukeAllEvents()
		{
			NukeAllEvents(this);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "private field name is not a resource string")]
		internal static void NukeAllEvents(object nukee)
		{
#if NET
			const string fieldName = "_events";
#else
			const string fieldName = "events";
#endif
			var pi = typeof(Component).GetField(fieldName,
				BindingFlags.NonPublic | BindingFlags.Instance);
			//pi will always be null for Winzor, hence why we skip it there
			//(I doubt this kind of form leaking even happens in Winzor)
			if (pi == null)
			{
				ErrorReporter.ReportDeveloperExceptionOnce($"Component.{fieldName} field could not be found using reflection", null);
			}
			else
			{
				pi.SetValue(nukee, null);
			}
		}
#endif

#if !WINZOR
		protected virtual bool IsRemoteAppSession => ObjectFactory.Get<TerminalService>().IsRemoteAppSession;
#endif

		void DisposeCore()
		{
			try
			{
				if (accessibleObjects.Any())
				{
					//for some reason ClearOwnerControlInternal can't be reflected on DAT?
					//But since all it does for a non-subclass is set ownerControl field to null then we may as well just do that directly.
#if NETFRAMEWORK
					var ownerControl = typeof(ControlAccessibleObject).GetField("ownerControl", BindingFlags.Instance | BindingFlags.NonPublic);
#elif NET
					var ownerControl = typeof(ControlAccessibleObject).GetField("_ownerControl", BindingFlags.Instance | BindingFlags.NonPublic);
#else
#error Unexpected target platform
#endif
					if (ownerControl != null)
					{
						foreach (var cao in accessibleObjects)
						{
							ownerControl.SetValue(cao, null);
						}
					}
					else
					{
						ErrorReporter.ReportOnce(typeof(ControlAccessibleObject).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Select(x => x.Name).Aggregate((x, y) => x + System.Environment.NewLine + y));
					}
				}
				accessibleObjects.Clear();
			}
			catch (Exception e)
			{
				ErrorReporter.ReportOnce("ClearOwnerControlInternal", e);
			}

			UnhookPreSaveValidationBoundControls();
			try
			{
				DisposePendingUserAction();
			}
			finally
			{
				if (BusinessEntity != null && BusinessEntity.Factory != null)
				{
					ModuleResultsBusinessObject = null;
				}

				if (fPlugIns != null)
				{
					((IDisposable)PlugIns).Dispose();
				}

				UnbindControls();
				DataBindings.Clear();

				DisposeIfNotNull(HelpClientSpecificMenuItem);
				DisposeIfNotNull(Components);

				DisposeIfNotNull(MainMenu);
				DisposeIfNotNull(fLicensedComponentManager);

				DisposeIfNotNull(acceptButtonWrapper);
				DisposeIfNotNull(module);
				module = null;
				disposableManager?.Dispose();

				SetDataBinding(null, "");
				BindingSource.DataSource = null;
				constructedDataSource = null;
				DisposeIfNotNull(BindingSource);
				fController = null;
			}
		}

		readonly List<ControlAccessibleObject> accessibleObjects = new List<ControlAccessibleObject>();

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			if (IsDisposed || IsDisposing)
			{
				return null;
			}
			var result = base.CreateAccessibilityInstance();
			if (result is ControlAccessibleObject cao)
			{
				accessibleObjects.Add(cao);
			}
			return result;
		}

		void UnhookPreSaveValidationBoundControls()
		{
			if (ControlsToBeBoundOnSave != null)
			{
				ControlsToBeBoundOnSave = null;
			}
		}

		static void DisposeIfNotNull(IDisposable objectToDispose)
		{
			if (objectToDispose != null)
			{
				objectToDispose.Dispose();
			}
		}

		protected virtual void UnbindControls()
		{
			UnbindControls(this);
			CurrencyManagerRelease();
		}

		static void UnbindControls(Control aControl)
		{
			foreach (Control childControl in aControl.Controls)
			{
				UnbindControls(childControl);
			}
			aControl.DataBindings.Clear();
		}

		void CurrencyManagerRelease()
		{
			// .NET does not call CurrencyManager.Release() to unhook an event between the currency manager & biz object so we reflect it out and call it here
			if (BindingContext != null)
			{
				var releaseMethod = typeof(CurrencyManager).GetMethod("Release", BindingFlags.NonPublic | BindingFlags.Instance);
				foreach (DictionaryEntry entry in BindingContext)
				{
					var bindingManager = ((WeakReference)entry.Value).Target as CurrencyManager;
					if (bindingManager != null)
					{
						releaseMethod.Invoke(bindingManager, Array.Empty<object>());
					}
				}
			}
		}

#endregion

		#region Display Mode

		/// <summary>
		/// Gets the current display mode of the form, or sets the controls' readonly state and changes the PostButton and CancelButton.
		/// </summary>
		public virtual ODisplayMode DisplayMode
		{
			get { return fDisplayMode; }
			set
			{
				var fromMode = fDisplayMode;
				if (fDisplayMode != value)
				{
					if (fPostButton != null && fCancelButton != null)
					{
						switch (value)
						{
							case ODisplayMode.Browse:
								ZFormStrategy.DoDisplayModeBrowse(this);
								break;
							case ODisplayMode.Edit:
								ZFormStrategy.DoDisplayModeEdit(this);
								break;
							case ODisplayMode.Delete:
								ZFormStrategy.DoDisplayModeDelete(this);
								break;
							case ODisplayMode.ReadOnly:
								ZFormStrategy.DoDisplayModeReadOnly(this);
								break;
							case ODisplayMode.New:
								ZFormStrategy.DoDisplayModeNew(this);
								break;
							case ODisplayMode.NewSaved:
								ZFormStrategy.DoDisplayModeNewSaved(this);
								break;
						}
					}
				}

				fDisplayMode = value;
				RaiseDisplayModeChanged(fromMode, value);

				if (fPlugIns != null)
				{
					fPlugIns.ParentFormDisplayMode = DisplayMode;
				}
			}
		}
		ODisplayMode fDisplayMode = ODisplayMode.Undefined;

		void RaiseDisplayModeChanged(ODisplayMode fromMode, ODisplayMode toMode)
		{
			if (DisplayModeChanged != null)
			{
				DisplayModeChanged(this, new DisplayModeChangedEventArgs(fromMode, toMode));
			}
		}

		public event DisplayModeChangedEventHandler DisplayModeChanged;

		protected bool IsViewOrDeleteMode
		{
			get { return (DisplayMode == ODisplayMode.ReadOnly || DisplayMode == ODisplayMode.Delete); }
		}

		#endregion

		#region Buttons

		void AssignButtonsInternal(IButton saveAndCloseButtonControl, IButton cancelButtonControl, IButton saveButtonControl)
		{
			fPostButton = saveAndCloseButtonControl;
			fCancelButton = cancelButtonControl;
			fApplyButton = saveButtonControl;
			if (fPostButton is ZButton zPostButton)
			{
				zPostButton.EditableInViewMode = true;
			}
			if (fCancelButton is ZButton zCancelButton)
			{
				zCancelButton.EditableInViewMode = true;
			}
			if (fApplyButton is ZButton zApplyButton)
			{
				zApplyButton.EditableInViewMode = true;
			}

			if (fPostButton != null)
			{
				fPostButton.Click += OnPostButtonClick;
			}

			if (fCancelButton != null)
			{
				fCancelButton.Click += OnCancelButtonClick;
			}

			if (fApplyButton != null)
			{
				fApplyButton.Click += OnApplyButtonClick;
			}

			DisplayMode = ODisplayMode.Browse;

			CancelButton = fCancelButton;
		}

		void IPostingButtonsProvider.AssignButtonsInternal(IButton saveAndCloseButtonControl, IButton cancelButtonControl, IButton saveButtonControl)
		{
			AssignButtonsInternal(saveAndCloseButtonControl, cancelButtonControl, saveButtonControl);
			isPostingButtonsAssigned = true;
		}
		bool isPostingButtonsAssigned;

		bool IPostingButtonsProvider.SetupPostingCalled
		{
			get { return SetupPostingCalled; }
			set { SetupPostingCalled = value; }
		}
		internal protected bool SetupPostingCalled;

		/// <summary>
		///	Happens when you click the save/new button. Do NOT override this.
		/// </summary>
		//[Obsolete("This is an O-Arch function that is not called reliably. Please override Save() or ShowPreSaveDialogs() instead.")]
		protected virtual void OnPostButtonClick(object sender, EventArgs e)
		{
			SaveFormProper(closeOnSave: true, sender: sender);
		}

		#region For Test
#if DEBUG
		internal bool AllowToSimulateAnotherButtonClickInside_ForTestOnly;
#endif
#endregion

		/// <summary>
		///	Happens when you click the save/new button. Do NOT override this.
		/// </summary>
		//[Obsolete("This is an O-Arch function that is not called reliably. Please override Save() or ShowPreSaveDialogs() instead.")]
		protected virtual void OnApplyButtonClick(object sender, EventArgs e)
		{
			SaveFormProper(sender: sender);
		}

		void OnCancelButtonClick(object sender, EventArgs e)
		{
			if (!IsSavingInProgress && !IsDisposed)
			{
				Close();
			}
		}

		static protected void FocusButton(object sender)
		{
			var button = sender as Button;
			if (button != null)
			{
				button.Focus();
			}
			else
			{
				Control parentControl = (sender as ToolStripButton)?.GetCurrentParent();
				if (parentControl != null)
				{
					parentControl.Focus();
				}
			}
		}

		public bool LastSaveSucceeded { get; private set; }

		/// <summary>
		/// If form startup takes a sufficently long time for the UI to become responsive (more than about 10 seconds)
		/// then the RemoteApp framework gives up trying to get the form caption and creates a client window without a caption.
		/// The correct caption is needed by drag-drop for matching the client and server windows.
		/// Workaround this by setting the caption again asynchronously after the form has loaded.
		/// </summary>
		void PostSetCaptionAgainWhenRemoteApp()
		{
#if !WINZOR
			if (IsRemoteAppSession)
			{
				BeginInvoke(new Action(() =>
				{
					if (IsHandleCreated)
					{
						var caption = TextIncludingSuffix;
						// Using Win32 directly since the text isn't being changed, but we still want to set it.
						UnsafeNativeMethods.SetWindowText(new HandleRef(this, Handle), caption);
					}
				}));
			}
#endif
		}

		public void RefreshCaption()
		{
			if (!string.IsNullOrEmpty(FormHeading))
			{
				base.Text = FormHeading;
			}
		}

		#endregion

		#region Status Bar

		void IUpdateStatusBar.UpdateStatusBar(string notification, INotificationType state)
		{
			UpdateStatusBar(notification, state);
		}

		protected virtual void UpdateStatusBar(string notification, INotificationType state)
		{
			MainStatusBar.Font = state == null ? OFont.GetFont() : OFont.GetFontBold();
#if WINZOR
			MessageStatusBarPanel.ForeColor = GetStatusBarColor(state);
#endif
			statusBarINotificationType = state;
			MessageStatusBarPanel.Text = notification;
		}

#if WINZOR
		Color GetStatusBarColor(INotificationType state)
		{
			if (state == CargoWise.EntityFramework.NotificationType.Error)
			{
				return Color.Red;
			}
			if (state == CargoWise.EntityFramework.NotificationType.MessageError)
			{
				return Color.Blue;
			}
			if (state == CargoWise.ComponentModel.NotificationType.Warning)
			{
				return Color.Olive;
			}

			return SystemColors.WindowText;
		}
#endif

		internal INotificationType statusBarINotificationType;

#if DEBUG
		public string StatusBarTextForTesting
		{
			get { return MessageStatusBarPanel.Text; }
		}
#endif

		[DefaultValue(true)]
		public bool DisplayErrorsInMessagePanel { get; set; }

		#region IStatusBarProvider Members

		ZStatusBar IStatusBarProvider.MainStatusBar
		{
			get { return MainStatusBar; }
			set { MainStatusBar = value; }
		}

		ZStatusBarPanel IStatusBarProvider.MessageStatusBarPanel
		{
			get { return MessageStatusBarPanel; }
			set { MessageStatusBarPanel = value; }
		}

		ZStatusBarPanel IStatusBarProvider.ErrorStatusBarPanel
		{
			get { return ErrorStatusBarPanel; }
			set { ErrorStatusBarPanel = value; }
		}

		INotificationType IStatusBarProvider.StatusBarINotificationType
		{
			get { return statusBarINotificationType; }
		}

		bool IStatusBarProvider.ShowStatusBar
		{
			get { return ShowStatusBar; }
		}

		protected virtual bool ShowStatusBar
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region Disabling New Action for Form

		public void DisableNewAction()
		{
			if (DisplayMode == ODisplayMode.Browse || DisplayMode == ODisplayMode.Edit || DisplayMode == ODisplayMode.New || DisplayMode == ODisplayMode.NewSaved)
			{
				fDefaultDisplayMode = ODisplayMode.NewSaved;
				if (DisplayMode == ODisplayMode.Browse)
				{
					DisplayMode = ODisplayMode.NewSaved;
				}
			}
		}

		ODisplayMode fDefaultDisplayMode = ODisplayMode.Browse;

		#endregion

		#region Tab Skipping ReadOnly Fields

#if !WINZOR

		protected override bool ProcessTabKeyCore(bool forward)
		{
			var active = this.GetFrontMostActiveControl();
			if (active.GetReadOnly())
			{
				return base.ProcessTabKeyCore(forward);
			}
			else
			{
				if (this.SelectNextControlNonTabStopNonReadOnly(ActiveControl, forward, true, true))
				{
					return true;
				}
				if ((this.IsMdiChild || (ParentForm == null)) && this.SelectNextControlNonTabStopNonReadOnly(null, forward, true, false))
				{
					return true;
				}
				return false;
			}
		}

#endif

		#endregion

		#region Drag & Drop

		internal void HandleDragOver(DragEventArgs e)
		{
			OnDragOver(e);
		}

		internal void HandleDragDrop(DragEventArgs e)
		{
			OnDragDrop(e);
		}

		protected void OnDataObjectPasted(DataObjectPastedEventArgs args)
		{
			if (DataObjectPasted != null)
			{
				DataObjectPasted(this, args);
			}
		}

		public event DataObjectPastedEventHandler DataObjectPasted;

		void IPasteSupport.HandleDataObjectPasted(DataObjectPastedEventArgs args)
		{
			OnDataObjectPasted(args);
		}

#if !WINZOR

		protected override void OnDragEnter(DragEventArgs e)
		{
			base.OnDragEnter(e);
			TrackingInfoLogger.Instance?.LogDragDropEvents("ZForm_DragEnter", GetType().Name, Name);
		}

		protected override void OnDragLeave(EventArgs e)
		{
			base.OnDragLeave(e);
			TrackingInfoLogger.Instance?.LogDragDropEvents("ZForm_DragLeave", GetType().Name, Name);
		}

		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			base.OnDragDrop(drgevent);
			TrackingInfoLogger.Instance?.LogDragDropEvents("ZForm_DragDrop", GetType().Name, Name);
		}
#endif

		#endregion

		#region Scanning

		/// <summary>
		/// Enable Scanning via a Wedge Scanner.
		/// </summary>
		/// <param name="barCodes">A collection of BarCodes that this form should support.</param>
		/// <param name="isOkToScan">This delegate can be used to disable scanning as desired. An example might
		/// be when a given tab is not current/visible. If not supplied Scanning is always enabled.</param>
		/// <param name="errorBoxSite">The parent panel that you want to display any scan error in. (the error box will
		/// be docked to the bottom of said panel). If not provided then ShowScanningError(msg) cannot be used.</param>
		public void EnableScanning(BarcodeManager barCodes, Func<bool> isOkToScan = null, Panel errorBoxSite = null)
		{
			if (Scanner != null)
			{
				throw new NotSupportedException("Multiple barcode managers are not supported. If support is needed, the arc can be changed to store a consolidation of all barcodes.");
			}

			Func<bool> isOkToScanWithViewModeCheck = delegate
			{
				var isEditable = (DisplayMode != ODisplayMode.ReadOnly && DisplayMode != ODisplayMode.Delete);
				return isEditable && (isOkToScan == null || isOkToScan());
			};

			Scanner = new ScanningManager(this, barCodes, isOkToScanWithViewModeCheck);

			if (errorBoxSite != null)
			{
				ScanMessageControl = new ScanMessageUserControl();
				errorBoxSite.Controls.Add(ScanMessageControl);

				Scanner.BarcodeScan += (sender, args) => ScanMessageControl.HideMessage();
			}
		}

		internal ScanningManager Scanner
		{
			get;
			private set;
		}

		public void ShowScanningMessage(ZString message, NotificationTypes notifyType, bool showOkButton = true)
		{
			if (!message.IsEmpty)
			{
				ThrowExceptionIfScanningNotEnabled();
				ScanMessageControl.ShowMessage(message, notifyType, showOkButton);
			}
		}

		public void HideScanningMessage()
		{
			ThrowExceptionIfScanningNotEnabled();
			ScanMessageControl.HideMessage();
		}

		void ThrowExceptionIfScanningNotEnabled()
		{
			if (ScanMessageControl == null)
			{
				throw new NotSupportedException("No Scanning Message Control exists, if you need to add/remove scanning errors supply a parent control in EnableScanning().");
			}
		}

		ScanMessageUserControl ScanMessageControl;

		#endregion

		#region IZForm Members

		public ControllerID ControllerID
		{
			get { return controllerID; }
			set
			{
				if (controllerID != value)
				{
					needRetrieveModule = true;
					controllerID = value;
				}
			}
		}
		ControllerID controllerID;
		ZController fController;
		ZModule module;
		bool needRetrieveModule = true;

		public ZModule GetModule()
		{
			if (module == null && needRetrieveModule)
			{
				try
				{
					if (ControllerID != null)
					{
						fController = ZControllerFactory.Create(ControllerID);
					}
					if (fController != null)
					{
						module = ZModuleFactory.Instance.Create(fController.ModuleID);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					//oh well, we tried
				}
				needRetrieveModule = false;
			}

			return module;
		}

		ModuleResultsBusinessObject IZForm.ModuleResultsBusinessObject
		{
			set { ModuleResultsBusinessObject = value; }
		}
		ModuleResultsBusinessObject IBusinessForm.ModuleResultsBusinessObject
		{
			get { return ModuleResultsBusinessObject; }
		}
		protected internal ModuleResultsBusinessObject ModuleResultsBusinessObject;

		#endregion

		#region IDialogKeyDown Members

		event KeyEventHandler IDialogKeyDown.DialogKeyDown
		{
			add { dialogKeyDown += value; }
			remove { dialogKeyDown -= value; }
		}
		event KeyEventHandler dialogKeyDown;

		#endregion

		#region IHaveTooltipsForMigration Members

		[Browsable(false)]
		public ToolTipCollectorForMigration FormToolTip
		{
			get { return ((IHaveTooltipsForMigration)this).ToolTipForMigration; }
		}

		ToolTipCollectorForMigration IHaveTooltipsForMigration.ToolTipForMigration
		{
			get { return toolTipCollectorForMigration ?? (toolTipCollectorForMigration = new ToolTipCollectorForMigration()); }
		}
		ToolTipCollectorForMigration toolTipCollectorForMigration;

		#endregion

		#region ZPlugIns

		public PlugIns PlugIns
		{
			get
			{
				if (fPlugIns == null)
				{
					fPlugIns = new PlugIns(GetTopLevelBusinessEntityForPlugIn(), this);
					fPlugIns.ParentFormDisplayMode = DisplayMode;
				}
				return fPlugIns;
			}
		}
		PlugIns fPlugIns;

		ControllerID IFormPlugInsProvider.PlugInIDToSelectOnLoaded
		{
			get { return PlugInIDToSelectOnLoaded; }
		}
		public ControllerID PlugInIDToSelectOnLoaded;

		internal ZString InitialTabPageNameToSelectOnLoaded = ZString.Empty;
		internal List<int> InitialControlIndexesToFocus;

		/// <summary>
		/// Returns the BusinessEntity to give to PlugIns. By default, this
		/// is the form's BusinessEntity.
		/// </summary>
		protected internal virtual IBusiness GetTopLevelBusinessEntityForPlugIn()
		{
			return BusinessEntity;
		}

		protected internal virtual ZTabControl TopLevelTabControl
		{
			get
			{
				ZTabControl result = null;
				foreach (Control control in Controls)
				{
					if (control is ZTabControl)
					{
						result = (ZTabControl)control;
						break;
					}
				}
				return result;
			}
		}

		ZTabControl IFormPlugInsProvider.TopLevelTabControl
		{
			get { return TopLevelTabControl; }
		}

		Menu IFormPlugInsProvider.GetMenuForPlugIn(ControllerID controllerID)
		{
			return GetMenuForPlugInCore(controllerID);
		}

		protected virtual Menu GetMenuForPlugInCore(ControllerID controllerID)
		{
			return MainMenu;
		}

		protected internal virtual void CustomisePluginTab(ZTabPagePlugIn tabPage, ControllerID pluginControllerID)
		{
		}

		#endregion

		#region Events

		public event EventHandler ResizeComplete;

		#region New Button Click

		public class NewButtonEventArgs : EventArgs
		{
			public NewButtonEventArgs(Point location)
			{
				this.Location = location;
			}

			public Point Location;
		}

		internal delegate void NewButtonEventHandler(object sender, NewButtonEventArgs e);

		internal event NewButtonEventHandler NewButtonClick;

		#endregion

		#region Handle KeyDown

		internal bool IsProcessingCutCopyOrPaste;

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			IsProcessingCutCopyOrPaste = (keyData == (Keys.Control | Keys.X) ||
											keyData == (Keys.Control | Keys.C) ||
											keyData == (Keys.Control | Keys.V));

			try
			{
				if (keyData == (Keys.Control | Keys.Tab))
				{
					if (HandleTabPageNavigation(true))
					{
						return true;
					}
				}
				else if (keyData == (Keys.Control | Keys.Shift | Keys.Tab))
				{
					if (HandleTabPageNavigation(false))
					{
						return true;
					}
				}

				return base.ProcessCmdKey(ref msg, keyData);
			}
			finally
			{
				IsProcessingCutCopyOrPaste = false;
			}
		}

		bool HandleTabPageNavigation(bool isForward)
		{
			var keyHandled = false;

			var innermostControl = GetInnermostActiveControl(ActiveControl);
			var currentTabControl = GetClosestParentTabControl(innermostControl);

			if (currentTabControl != null)
			{
				if (isForward)
				{
					currentTabControl.SelectNextTabPage();
				}
				else
				{
					currentTabControl.SelectPreviousTabPage();
				}
				keyHandled = true;
			}

			return keyHandled;
		}

		/// <summary>
		/// Navigate towards innermost active control
		/// </summary>
		static Control GetInnermostActiveControl(Control initialControl)
		{
			var result = initialControl;
			while (result is IContainerControl)
			{
				result = ((IContainerControl)result).ActiveControl;
			}
			return result;
		}

		/// <summary>
		/// Navigate backwards until find a TabControl
		/// </summary>
		ZTabControl GetClosestParentTabControl(Control initialControl)
		{
			var result = initialControl;
			while (!(result is ZTabControl))
			{
				if (result == null || result.Parent == null || result.Parent == this)
				{
					return null;
				}
				result = result.Parent;
			}
			return result as ZTabControl;
		}

		#endregion

		#region FactoryChanged

		protected void OnFactoryChanged()
		{
			if (FactoryChanged != null)
			{
				FactoryChanged(this, new EventArgs());
			}
		}

		internal event EventHandler FactoryChanged;

		#endregion

		#endregion

		#region Recent Item Caption

		protected virtual string FormatRecentItemCaption(string caption) => caption;

		string IRecentItemCaptionFormatter.FormatRecentItemCaption(string caption) => FormatRecentItemCaption(caption);

		#endregion

		#region Windows Form Designer generated code

		readonly IContainer Components = new Container();
		public ZStatusBar MainStatusBar;
		public ZStatusBarPanel MessageStatusBarPanel;
		public ZStatusBarPanel ErrorStatusBarPanel;

		protected MainMenu MainMenu;
		protected MenuItem FileMenuItem;
		protected MenuItem EditMenuItem;
		protected MenuItem ActionsMenuItem;
		protected MenuItem HelpClientSpecificMenuItem;
		protected MenuItem HelpMenuItem;

		MainMenu IFileMenuItemsProvider.MainMenu
		{
			get { return MainMenu; }
			set { MainMenu = value; }
		}

		MenuItem IFileMenuItemsProvider.FileMenuItem
		{
			get { return FileMenuItem; }
			set { FileMenuItem = value; }
		}

		MenuItem IFileMenuItemsProvider.EditMenuItem
		{
			get { return EditMenuItem; }
			set { EditMenuItem = value; }
		}

		MenuItem IFileMenuItemsProvider.ActionsMenuItem
		{
			get { return ActionsMenuItem; }
			set { ActionsMenuItem = value; }
		}

		MenuItem IFileMenuItemsProvider.HelpClientSpecificMenuItem
		{
			get { return HelpClientSpecificMenuItem; }
			set { HelpClientSpecificMenuItem = value; }
		}

		MenuItem IFileMenuItemsProvider.HelpMenuItem
		{
			get { return HelpMenuItem; }
			set { HelpMenuItem = value; }
		}

		#endregion

		#region IDesignTimeDataSourceType

		TopLevelDataSourceTypeHelper designTimeDataSourceTypeHelper;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DataSourceAssemblyName
		{
			get { return designTimeDataSourceTypeHelper.DataSourceAssemblyName; }
			set { designTimeDataSourceTypeHelper.DataSourceAssemblyName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public string DataSourceTypeName
		{
			get { return designTimeDataSourceTypeHelper.DataSourceTypeName; }
			set { designTimeDataSourceTypeHelper.DataSourceTypeName = value; }
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override Type DataSourceType
		{
			get { return designTimeDataSourceTypeHelper.DataSourceType; }
			set { base.DataSourceType = value; }
		}

		#endregion

		#region ICaptionRenderingSupport Members

		[Category(ZGUIConstants.DesignerCategory)]
		[Browsable(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public bool? CaptionRenderingEnabled
		{
			get { return captionRenderingEnabled; }
			set
			{
				if (captionRenderingEnabled != value)
				{
					captionRenderingEnabled = value;
					OnCaptionRenderingEnabledChanged(EventArgs.Empty);
				}
			}
		}
		bool? captionRenderingEnabled;

		public event EventHandler CaptionRenderingEnabledChanged;

		void OnCaptionRenderingEnabledChanged(EventArgs e)
		{
			if (CaptionRenderingEnabledChanged != null)
			{
				CaptionRenderingEnabledChanged(this, e);
			}
		}

		#endregion

		#region ILicensedComponent Members

		public LicensedComponentManager LicensedComponentManager
		{
			get { return ((ILicensedComponent)this).LicensedComponentManager as LicensedComponentManager; }
		}

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return fLicensedComponentManager ?? (fLicensedComponentManager = new LicensedComponentManager(this)); }
		}

		LicensedComponentManager fLicensedComponentManager;

		#endregion

		#region ISaveInitiator Memebers

		bool ISaveInitiator.SaveExceptionCaughtAlready { get; set; }

		IBusiness ISaveInitiator.BusinessEntityForValidation
		{
			get { return BusinessEntityForValidation; }
		}

		void ISaveInitiator.ShowErrorsDialog()
		{
			ShowErrorsDialog();
		}

		#endregion

		public sealed override string Text
		{
			get
			{
				var result = base.Text;
				if (string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(lastManualText))
				{
					result = lastManualText;
				}
				return result;
			}
			set
			{
				base.Text = value;
				lastManualText = value;
				IsTextSetManually = !string.IsNullOrEmpty(value);
			}
		}
		string lastManualText;

		protected bool ShouldSerializeText()
		{
			return !string.IsNullOrEmpty(Text) && IsTextSetManually;
		}

		bool IsTextSetManually;

		#region handling ReadOnly/Disabled

		protected void RecursivelyEnableParent(Control ctrl)
		{
			if (ctrl.Parent != null)
			{
				ctrl.Parent.Enabled = true;
				RecursivelyEnableParent(ctrl.Parent);
			}
		}

		protected void RecursivelySetControls(Control ctrl, bool controlsEnabled)
		{
			if (ctrl is IShouldntBeReadOnly notReadOnly)
			{
				if (!controlsEnabled)
				{
					RecursivelyEnableParent(ctrl);
				}

				if (!notReadOnly.ReadOnly)
				{
					notReadOnly.ReadOnly = !controlsEnabled;
				}
			}
			else if (ctrl is IEditableInViewMode editable && editable.EditableInViewMode || ctrl is ISkipSettingControlEnabled skipSetting && skipSetting.SkipSettingControlEnabled)
			{
				if (!controlsEnabled)
				{
					RecursivelyEnableParent(ctrl);
				}
			}
			else
			{
				ctrl.Enabled = controlsEnabled;
				foreach (Control childCtrl in ctrl.Controls)
				{
					RecursivelySetControls(childCtrl, controlsEnabled);
				}
			}
		}

		#endregion

		#region Implementation

		protected override KBindingSource NewBindingSource()
		{
			return new ZBindingSource();
		}

		/// <summary>
		/// Show a message box to ask the user to confirm that they want to delete this record.
		/// </summary>
		/// <returns>Returns Yes or No.</returns>
		protected virtual DialogResult ShowConfirmationForDelete()
		{
			var message = Res.GetString("ZForm|DeleteConfirmation|Message", "You are about to delete this record permanently from the system. Do you want to proceed?");
			var caption = Res.GetString("ZForm|DeleteConfirmation|Caption", "Delete Confirmation");
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
		}

		protected virtual ZGrid GetGridFromControl(Control controlToCheck)
		{
			ZGrid result;
			if (controlToCheck is IButtonGrid)
			{
				result = ((IButtonGrid)controlToCheck).InnerGrid;
			}
			else
			{
				result = controlToCheck as ZGrid;
			}
			return result;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var previousBizo = DataSource as BusinessObject;
			if (previousBizo != null)
			{
				previousBizo.IsTopLevel = boundBizObjIsTopLevelOriginalValue;
				previousBizo.HasChangesChanged -= HasChangesChangedHandler;
			}

			var topLevelBusinessObject = dataSource as BusinessObject;
			if (topLevelBusinessObject != null)
			{
				boundBizObjIsTopLevelOriginalValue = topLevelBusinessObject.IsTopLevel;
				topLevelBusinessObject.IsTopLevel = true;
				topLevelBusinessObject.HasChangesChanged += HasChangesChangedHandler;

				var factory = topLevelBusinessObject.Factory;
				if (factory != null && !factory.TryGetDisposableManager(out _))
				{
					if (disposableManager == null)
					{
						disposableManager = new DisposableManager();
					}
					disposableManager.Subscribe(factory.AddDisposableService(disposableManager));
				}
			}

			base.SetDataBinding(dataSource, dataMember);

			if (BusinessEntity is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord &&
				DataRegistry.Instance.TemplateRecordValidation == RawDataRegistry.TemplateRecordValidationCodes.NoValidation)
			{
				NoValidationOnSave = true;
			}

#if DEBUG
			if (dataSource != null)
			{
				LastDataSourceForTest = dataSource;
			}
#endif
		}

		bool boundBizObjIsTopLevelOriginalValue;

#if DEBUG
		public object LastDataSourceForTest { get; private set; }
#endif

		void HasChangesChangedHandler(object sender, EventArgs e)
		{
			var bizo = DataSource as BusinessObject;

			if (bizo != null && bizo.IsDeleted && bizo.IsRefreshingByDataRefreshBus)
			{
				bizo.HasChangesChanged -= HasChangesChangedHandler;

				if (Focused)
				{
					HandleDeletedBusinessEntity();
				}
				else
				{
					Activated += HandleDeletedBusinessEntityDelayed;
				}
			}
		}

		void HandleDeletedBusinessEntity()
		{
			Globals.Message.ShowInformation(Res.GetString("f72bf87a-8343-41f8-919b-82c0f41d2cf7", "This record has been deleted in other form and cannot be used further. This form will be closed."));

			var bizo = DataSource as IBusinessObjectInternals;
			using (bizo != null ? bizo.SuppressReportRowDeletedError() : null)
			{
				if (InvokeRequired)
				{
					Invoke(new MethodInvoker(ForceClose));
				}
				else
				{
					ForceClose();
				}
			}
		}

		void HandleDeletedBusinessEntityDelayed(object sender, EventArgs e)
		{
			if (CanFocus)
			{
				Activated -= HandleDeletedBusinessEntityDelayed;
				HandleDeletedBusinessEntity();
			}
		}

		protected virtual bool CheckForErrorsInBinding
		{
			get { return true; }
		}

		void SetAllControlsReadOnly()
		{
			BusinessEntity.IncrementReadOnlyIncludingChildren();
			SetReadOnlyIncludingChildren();
		}

		protected virtual void SetReadOnlyIncludingChildren()
		{
			(this as Control).SetReadOnlyIncludingChildren();
		}

		protected internal bool IsChangingWithoutFocus { get; set; }

		#region PropertyDescriptors

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<ZForm>()
				.Property("DataSourceTypeName", "", false)
				.Property("DataSourceAssemblyName", "", false)
				.Result;
		}

		#endregion

		#region Developer Tools

		protected virtual void PopulateDevTools(List<IDevTool> tools)
		{
#if !DEBUG // On ediProd we want to withold some access
			if (Enterprise.ZArchitecture.Modules.ClientHookLoader.Instance.Client == CargoWise.Definitions.Clients.EDI
				&& ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Production)
			{
				tools.Add(new DeveloperInformationTool());
				tools.Add(new NotificationTool());
				tools.Add(new BindingManagerTool());
				tools.Add(new QueryAnalyserTool());
				return;
			}
#endif

			tools.Add(new DeveloperInformationTool());
			tools.Add(new DataMagicTool());
			tools.Add(new NotificationTool());
			tools.Add(new BindingManagerTool());
			tools.Add(new FactoryXmlTool());
			tools.Add(new BusinessObjectsStackTraceXmlTool());
			tools.Add(new QueryAnalyserTool());
			tools.Add(new AutoRatingExplorerTool());
			tools.Add(new UserIdleWorkerExplorerTool());
			tools.Add(new SqlProfilerTool());
			tools.Add(new MacroEvaluationDevTool());
			tools.Add(new BizoDiffTool());
#if DEBUG
			tools.Add(new SuspendValidationTool());
#endif
		}

		internal void PopulateDevToolsInternal(List<IDevTool> tools)
		{
			PopulateDevTools(tools);
		}

		public void ShowZNotificationForm()
		{
			if (BusinessEntity != null)
			{
				new ZNotificationsForm(BusinessEntity).Show();
			}
			else
			{
				ShowBusinessEntityNullError();
			}
		}

		public void ShowBizoDiffForm()
		{
			if (BusinessEntity != null)
			{
				new BizoDiffForm(BusinessEntity).Show();
			}
			else
			{
				ShowBusinessEntityNullError();
			}
		}

		public virtual void ShowDataMagicForm() // OWinForm override, not reference to DataSet
		{
			if (BusinessEntity != null)
			{
				DataSet data = null;

				var businessEntityAsDataProvider = BusinessEntity as INeedDataSet;
				if (businessEntityAsDataProvider != null && !(businessEntityAsDataProvider is NonPersistentBusinessObject || businessEntityAsDataProvider is INonPersistentBusinessObjectCollection))
				{
					data = businessEntityAsDataProvider.Data;
				}

				if (data == null && BusinessEntity.Factory != null)
				{
					data = ((INeedDataSet)BusinessEntity.Factory).Data;
				}

				ShowDataMagicForm(data);
			}
			else
			{
				ShowBusinessEntityNullError();
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic message.")]
		static void ShowBusinessEntityNullError()
		{
			Globals.Message.ShowError("Cannot show Developer form, BusinessEntity is null.");
		}

		#region Implementation

		protected virtual void ShowDataMagicForm(object data)
		{
			var form = new ODiscoverMagicForm { WindowState = FormWindowState.Maximized, DataSource = data };
			form.Show();
		}

		static protected Control GetActiveControl(Control currentControl)
		{
			if (currentControl.Parent is ZGrid)
			{
				return currentControl.Parent;
			}
			else if (currentControl is IContainerControl)
			{
				if (currentControl.DataBindings.Count > 0)
				{
					return currentControl;
				}
				return GetActiveControl(((IContainerControl)currentControl).ActiveControl);
			}
			else
			{
				return currentControl;
			}
		}

		protected DataSet GetCurrentControlDataSource() // This is legacy architecture that will be removed
		{
			var currentControl = GetActiveControl(ActiveControl);

			if (currentControl is ZGrid)
			{
				return ((ZGrid)currentControl).DataSource as DataSet; // This is legacy architecture that will be removed
			}

			foreach (Binding b in currentControl.DataBindings)
			{
				if (b.DataSource is DataSet) // This is legacy architecture that will be removed
				{
					return (DataSet)b.DataSource; // This is legacy architecture that will be removed
				}
			}
			return null;
		}

		#endregion

		#endregion

		#region Site

		public override ISite Site
		{
			get { return base.Site; }
			set
			{
				base.Site = value;
				DesignTimeEnvironment.InitializeDesignTimeWithServiceProvider(value);
#if DEBUG
				DesignerInheritedFormsSizeAndLocationFixer.Fix(this);
				VerifyInitializeFormOrComponentNotOverridden();
#endif
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Development message")]
		void VerifyInitializeFormOrComponentNotOverridden()
		{
			if (this.IsDesignMode())
			{
				var uiservice = (IUIService)GetService(typeof(IUIService));
				var typeDeclaration = (CodeTypeDeclaration)GetService(typeof(CodeTypeDeclaration));
				if (typeDeclaration != null && uiservice != null)
				{
					foreach (CodeTypeMember member in typeDeclaration.Members)
					{
						if (member.Attributes != MemberAttributes.Private)
						{
							if (member.Name == "InitializeForm")
							{
								uiservice.ShowMessage("It is not recommended you override InitializeForm in your form. Consider using the constructor instead.");
								break;
							}
							if (member.Name == "InitializeComponent")
							{
								uiservice.ShowMessage(@"
It is recommended you make InitializeComponent private and non-virtual, and call InitializeComponent from each of your constructors:

new void InitializeComponent()
{
}

(note: the new keyword will be removed after all ZForm.InitializeComponent methods are removed)");
								break;
							}
						}
					}
				}
			}
		}

		#endregion

		#region WndProc

#if !WINZOR

		Size tempOriginalFormSize = Size.Empty;

		protected override void WndProc(ref Message m)
		{
			if (!this.IsDesignMode())
			{
				WndProcRobotCheck.CheckForRPAOnNonRobotUser((uint)m.Msg);
				SystemMenu.ProcessWndProc(ref m);

				switch (m.Msg)
				{
					case WindowsMessage.WM_CLOSE:
						if (!IsEnabledCore)
						{
							m.Result = IntPtr.Zero;
							return;
						}
						break;
					case WindowsMessage.WM_ENTERSIZEMOVE:
						tempOriginalFormSize = Size;
						break;

					case WindowsMessage.WM_EXITSIZEMOVE:
						if (tempOriginalFormSize != Size)
						{
							if (ResizeComplete != null)
							{
								ResizeComplete(this, EventArgs.Empty);
							}
						}
						break;

					case WindowsMessage.WM_DISPLAYCHANGE:
						if (ResizeComplete != null)
						{
							ResizeComplete(this, EventArgs.Empty);
						}
						break;

					case WindowsMessage.WM_SYSCOMMAND:
						if (ResizeComplete != null)
						{
							int wParamInt;
							unchecked
							{
								wParamInt = m.WParam.ToInt32();
							}

							if ((wParamInt == NativeMethods.SC_MAXIMIZE) || (wParamInt == NativeMethods.SC_RESTORE) ||
								(wParamInt == (NativeMethods.SC_MAXIMIZE + 2)) || (wParamInt == (NativeMethods.SC_RESTORE + 2)))
							{
								BeginInvoke(new EventHandler(ResizeComplete), new object[] { this, EventArgs.Empty });
							}
						}
						break;

					case WindowsMessage.WM_NCMOUSEMOVE:
						unchecked
						{
							if (m.WParam.ToInt32() == HitTestCodes.HTCAPTION && TranslationFeedbackManager.InTranslationFeedbackMode())
							{
								CaptionHighlightForm.Show(this);
							}
						}
						break;
				}
			}
			menuFeedback.WndProc(this, ref m);
			base.WndProc(ref m);
		}

		readonly MenuFeedback menuFeedback = new MenuFeedback();

#endif

		#endregion

		#region WindowState

		protected override void OnClientSizeChanged(EventArgs e)
		{
			if (WindowState != FormWindowState.Minimized && lastWindowState != WindowState)
			{
				lastWindowState = WindowState;
			}
			base.OnClientSizeChanged(e);
		}

		FormWindowState lastWindowState;

		public void RestoreLastWindowStateFromMinimized()
		{
			if (WindowState == FormWindowState.Minimized)
			{
				WindowState = lastWindowState;
			}
		}

		#endregion

		#region ProcessDialogKey

		protected override bool ProcessDialogKey(Keys keyData)
		{
			var result = false;
			if (dialogKeyDown != null)
			{
				var args = new KeyEventArgs(keyData);
				dialogKeyDown(this, args);
				result = args.Handled;
			}

			if (!result)
			{
				result = base.ProcessDialogKey(keyData);
			}

			return result;
		}

		#endregion

		#endregion

		[Browsable(true)]
		public ResourceStringData CaptionResourceString
		{
			get { return captionResourceString ?? ResourceStringData.Empty; }
			set
			{
#if DEBUG
				if (DesignMode && value == null)
				{
					return;
				}
#endif
				captionResourceString = value;
			}
		}
		ResourceStringData captionResourceString;

		bool ShouldSerializeCaptionResourceString()
		{
			return !CaptionResourceString.IsEmpty();
		}

		internal protected void RunActionWithProcessBox(string loadingText, Action action)
		{
			using (new ZWaitCursorChanger(this))
			{
				try
				{
					this.ShowProgressBoxIfEnabled(loadingText);
					action();
					this.Activate();
				}
				finally
				{
					this.ClearHideProgressDialogDelegateInModaliser();
					this.DisposeProgressBoxIfEnabled();
				}
			}
		}

		#region ShowWithoutActivation

		protected override bool ShowWithoutActivation
		{
			get { return ShowWithoutActivationOverride; }
		}

		internal bool ShowWithoutActivationOverride { get; set; }

		#endregion

		#region HotkeyProvider

		HotkeyRegister IHotkeyProvider.Hotkeys => Hotkeys;
		string IHotkeyProvider.TypeNameForDisplay => Res.GetString("HotkeyTypeName|KForm", "Form");

		#endregion
	}

	#region IZForm Interface

	public interface IZForm : IWin32Window, IDisposable, ILicensedComponent
	{
		ControllerID ControllerID { get; set; }
		ODisplayMode DisplayMode { get; set; }
		ModuleResultsBusinessObject ModuleResultsBusinessObject { set; }
		void Show();
		event EventHandler Closed;
		IBusiness BusinessEntityForPersistingForm { get; set; }
		Guid IdentifierForPersistingForm { get; set; }
		bool IsActivityLogFinished { get; set; }
		void FormInitialSize();
	}

	public interface IBusinessForm
	{
		string FormCaption { get; }
		ModuleResultsBusinessObject ModuleResultsBusinessObject { get; }
	}

	#endregion

	#region IShowPreSaveDialog Interface

	public interface IShowPreSaveDialog : IDisposable
	{
		ContinueWithSave ShowPreSaveDialogs();
	}

	#endregion

	#region Validating for Save Event

	public delegate void ValidatingForSaveEventHandler(object sender, ValidatingForSaveEventArgs e);

	public class ValidatingForSaveEventArgs : EventArgs
	{
		public ContinueWithSave ContinueWithSave = ContinueWithSave.Yes;
	}

	#endregion

	#region Display Mode Changed Event

	public class DisplayModeChangedEventArgs : EventArgs
	{
		public DisplayModeChangedEventArgs(ODisplayMode fromMode, ODisplayMode toMode)
		{
			this.FromMode = fromMode;
			this.ToMode = toMode;
		}

		public readonly ODisplayMode FromMode;
		public readonly ODisplayMode ToMode;
	}

	public delegate void DisplayModeChangedEventHandler(object sender, DisplayModeChangedEventArgs e);

	#endregion
}
// ReSharper restore DoNotCallOverridableMethodsInConstructor
#pragma warning restore 0809
