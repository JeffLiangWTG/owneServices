using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Controls;
using Enterprise.RemoteDesktopServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;
#if DEBUG
using CargoWise.Windows.UI.Testing;
#endif

namespace Enterprise.ZArchitecture.GUI.Internal
{
#if DEBUG
	[SuppressFormDesignerAnalysis]
#endif
	public abstract class ZPopupFindBox : ZListProviderFindBox, IShowEditOrViewForm
	{
		public ZPopupFindBox()
		{
			PopupCaption = "";
			ShowNewFormWhenEmpty = true;
			AllowNewForm = true;
			this.PopupButton.EditableInViewMode = true;
		}

		#region Bare

		public abstract class Bare : ZPopupFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Properties

		[SmartTagVisible]
		[Category(ZGUIConstants.DesignerCategory)]
		[Editor(typeof(ModuleIDEditor), typeof(UITypeEditor))]
		public virtual ModuleIdentifier ModuleID
		{
			get
			{
				var result = moduleID;
				if (result == ModuleIDs.NotAssigned)
				{
					result = GetListRelatedModuleID();
				}
				return result;
			}
			set
			{
				SetModuleId(value);
			}
		}
		ModuleIdentifier moduleID = ModuleIDs.NotAssigned;

		public ModuleIdentifier ParentModuleID
		{
			get
			{
				return parentModuleID;
			}
			set
			{
				if (!Equals(value, ModuleIDs.NotAssigned))
				{
					parentModuleID = value;
				}
			}
		}
		ModuleIdentifier parentModuleID = ModuleIDs.NotAssigned;

		public Type ParentType { get; set; }

		protected virtual void SetModuleId(ModuleIdentifier moduleId)
		{
			if (!Equals(moduleId, ModuleIDs.NotAssigned))
			{
				BindToListAndModuleIdWarner.ShowOnChangedModuleIDWarning(this);
			}

			var didChange = !Equals(moduleID, moduleId);
			moduleID = moduleId;

			if (didChange)
			{
				OnModuleIdChanged();
			}
		}

		protected virtual void OnModuleIdChanged()
		{
		}

		internal void SetPopupButtonReadOnlyForNewlySelectedModule(IModuleFilterWithSelectedFilters filter)
		{
			var shouldBeReadOnly = !filter.HasValidModuleInCurrentContext;
			ReadOnly = shouldBeReadOnly;
			PopupButton.ReadOnly = shouldBeReadOnly;
		}

		protected ModuleIdentifier ModuleIDDoNotLookInList
		{
			get { return moduleID; }
		}

		bool ShouldSerializeModuleID()
		{
			return ModuleID != ModuleIDs.NotAssigned;
		}

		[DefaultValue("")]
		public string PopupCaption { get; set; }

		public Func<string> GetCountryCode;

		[DefaultValue(true)]
		public bool ShowNewFormWhenEmpty { get; set; }

		[DefaultValue(true)]
		public bool AllowNewForm { get; set; }

		IDisposable SwitchModuleIdIfNeeded()
		{
			var oldModuleID = moduleID;
			if (CurrentItem is IModuleIDProvider moduleIDProvider && Equals(oldModuleID, ModuleIDs.NotAssigned))
			{
				moduleID = moduleIDProvider.ModuleID;
			}

			return new DisposableAction(() =>
			{
				moduleID = oldModuleID;
			});
		}

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			using (SwitchModuleIdIfNeeded())
			{
				if (!(this is ICustomizableFindBoxPopup))
				{
					ActiveControl = CodeBox;
				}

				var oldCodeValue = CodeBox.Text;
				var shouldShowPopupForm = true;
				var shouldAutoSearchAndFocus = !string.IsNullOrEmpty(Code) && AutoRunSearchFromFindBox;
#if WINZOR
			if (IsPopupFormVisible())
			{
				return;
			}
#endif
				var embeddedPopup = PopupForm as EmbeddedModulePopup;
				if (embeddedPopup != null && shouldAutoSearchAndFocus && autoSelect)
				{
					SetRunSearchOnEnteringAModule(embeddedPopup);
					shouldShowPopupForm = !embeddedPopup.AutoSearchAndSelectIfOnlyOneRecord(this) || oldCodeValue == CodeBox.Text;
					embeddedPopup.Close();
				}

				if (shouldShowPopupForm)
				{
					embeddedPopup = PopupForm as EmbeddedModulePopup;
					if (embeddedPopup != null && shouldAutoSearchAndFocus)
					{
						SetRunSearchOnEnteringAModule(embeddedPopup);
					}

					PopupForm.ShowModal(this, FindForm());

					if (embeddedPopup != null && shouldAutoSearchAndFocus)
					{
						embeddedPopup.FocusFirstRecord();
					}
				}
			}
		}
#if DEBUG
		internal
#endif
		protected bool RunPopupSecurityCheck()
		{
			var result = true;
			using (var filterModule = ZFilterModule.GetZFilterModule(ModuleID, GetCountryCode?.Invoke()))
			{
				if (filterModule == null)
				{
					return result;
				}

				filterModule.ParentModuleID = ParentModuleID;
				var securityCheckpointForPopups = filterModule.GetSecurityCheckpointForPopups();

				if (securityCheckpointForPopups != null)
				{
					var securityCheckpointDenied = securityCheckpointForPopups.Where(x => !x.IsAllowed).ToArray();
					if (securityCheckpointDenied.Length > 0)
					{
						result = false;
					}
				}
			}
			return result;
		}

		bool AutoRunSearchFromFindBox
		{
			get { return EnvProxy.Instance.Registry.AutoRunSearchFromFindBox; }
		}

		void SetRunSearchOnEnteringAModule(EmbeddedModulePopup embeddedPopup)
		{
			var filterControl = (ZFilterStripCommonControl)embeddedPopup.Module.EmbeddedControl;
			if (filterControl != null)
			{
				filterControl.RunSearchOnEnteringAModuleOverride = true;
			}
		}

		public override SilentSelectResult SelectFromPopupFormWithoutDisplaying()
		{
			EmbeddedModulePopup modulePopup = null;
			try
			{
				modulePopup = (EmbeddedModulePopup)GetNewPopupForm();
				return modulePopup.SelectFromPopupWithoutDisplaying(this, modulePopup);
			}
			finally
			{
				if (modulePopup != null)
				{
					modulePopup.Dispose();
				}
			}
		}

		#endregion

		#region Edit Form

		protected override void ShowEditOrViewForm()
		{
			var showForm = new ShowSelectedInEditOrViewForm(this);
			showForm.ShowEditOrViewForm();
		}
#if DEBUG
		internal
#endif
		protected virtual void ShowEditForm(ZFilterModule module)
		{
			var showForm = new ShowSelectedInEditOrViewForm(this);
			showForm.ShowEditForm(module);
		}

		protected virtual void ShowViewForm(ZFilterModule module)
		{
			var showForm = new ShowSelectedInEditOrViewForm(this);
			showForm.ShowViewForm(module);
		}

		ZFilterModule IShowEditOrViewForm.NewModuleFromModuleID()
		{
			return NewModuleFromModuleID();
		}

		public ShowingEditOrViewFormEventArgs InitialiseForm(ZFilterModule module)
		{
			module.OverrideModuleDecisionProvider(module.GetModuleDecisionProviderForFindBox(this));
			module.SetFormsModalTo(FindForm());

			var showingEditOrViewFormEventArgs = new ShowingEditOrViewFormEventArgs(module, module.AllowEdit && AllowShowEditForm, module.AllowView);
			OnShowingEditOrViewForm(showingEditOrViewFormEventArgs);
			return showingEditOrViewFormEventArgs;
		}

		protected void OnShowingEditOrViewForm(ShowingEditOrViewFormEventArgs args)
		{
			if (ShowingEditOrViewForm != null)
			{
				ShowingEditOrViewForm(this, args);
			}
		}

		public event EventHandler<ShowingEditOrViewFormEventArgs> ShowingEditOrViewForm;

		public string SearchCode => IFindBox.Code;

		IEnumerable<BusinessObject> IShowEditOrViewForm.GetBizObjsToEditOrView()
		{
			return GetBizObjsToEditOrView();
		}

		public void ShowMoreThanOneSelectedMessageAndPopup(bool isEdit)
		{
			Globals.Message.Show(Res.GetString("f1835297-ec9e-4462-ac9e-14822fcb6599", "More than one entity has this code. Select and edit the one you're after."));
			SelectFromPopupForm();
		}

		public DialogResult ShowDefaultMessageWhenCreatingANewBizObjFromFindBox(ZFilterModule module)
		{
			var message = module.DefaultMessageWhenCreatingANewBizObjFromFindBox(IFindBox);
			return Globals.Message.Show(message, Res.GetString("d53fcf04-f570-4550-8382-ba54fbb04470", "Code does not exist"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		}

		void IShowEditOrViewForm.SetCodePropertyFromText(IZForm form)
		{
			SetCodePropertyFromText(form);
		}
#if DEBUG
		internal
#endif
		protected virtual IEnumerable<BusinessObject> GetBizObjsToEditOrView()
		{
			return OpenOnlyFilteredBusinessObjects
				? IFindBox.ListProvider.GetBusinessObjectsFromCode(CodeForFinding)
				: IFindBox.ListProvider.GetBusinessObjectsFromCodeWithoutFilter(CodeForFinding);
		}

		protected virtual bool AllowShowEditForm
		{
			get { return true; }
		}

		object IShowEditOrViewForm.DataSource => DataSource;

		bool OpenOnlyFilteredBusinessObjects
		{
			get
			{
				var type = IFindBox.ListProvider.List?.TypeOfElements;
				return
					type != null &&
					type.GetCustomAttributes(typeof(RestrictedFilteredItemAttribute), true).Any();
			}
		}

		#endregion

		#region Popup Form

#if DEBUG
		internal
#endif
		protected override IFindBoxPopup PopupForm
		{
			get
			{
				if (popupForm == null)
				{
					popupForm = GetNewPopupForm();
					popupForm.Closed += new EventHandler(PopupForm_Closed);

					var embeddedPopup = popupForm as EmbeddedModulePopup;
					if (embeddedPopup != null)
					{
						embeddedPopup.Selected += Popup_Selected;
					}
				}
				return popupForm;
			}
		}
		IFindBoxPopup popupForm;

		void PopupForm_Closed(object sender, EventArgs e)
		{
			if (popupForm != null)
			{
				popupForm.Closed -= new EventHandler(PopupForm_Closed);
				OnPopupFormClosed(popupForm);
				popupForm.Dispose();
				popupForm = null;

#if !WINZOR
				if (ObjectFactory.Get<TerminalService>().IsRemoteAppSession)
				{
					ParentForm?.Activate();
				}
#endif
			}
		}

		protected void ClosePopupForm()
		{
			PopupForm_Closed(this, EventArgs.Empty);
		}

		protected virtual void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
		}

		void Popup_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			OnPopupSelected(popupForm, e.SelectedBusinessObjects);

			if (PopupSelected != null)
			{
				PopupSelected(sender, e);
			}
		}

		protected virtual void OnPopupSelected(IFindBoxPopup popup, BusinessObject[] selectedBusinessObjects)
		{
		}

		public event EmbeddedModulePopup.SelectedEventHandler PopupSelected;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal log, no need to translate")]
		protected virtual IFindBoxPopup GetNewPopupForm()
		{
			IFindBoxPopup popup = null;
			var module = NewModuleFromModuleID();
			if (module != null)
			{
				try
				{
					//ZLimitedColumnsProvider won't work with anything that isn't a BusinessObject type, like LocationCollection is backed by ILocation and uses custom logic to load.
					if (!RunPopupSecurityCheck())
					{
						var listProvider = IFindBox.ListProvider.List;
						if (listProvider != null && listProvider.TypeOfElements.IsSubclassOf(typeof(BusinessObject)))
						{
							module.LimitedColumns = new ZLimitedColumnsProvider(IFindBox.ListProvider.List.TypeOfElements);
						}
					}
				}
				catch (NullReferenceException ex)
				{
					var message = string.Format(Culture.Invariant, "{0}\r\nModuleId: {1}\r\nControl path: {2}\r\nDataSource: {3}\r\nCurrentItem: {4}\r\nDataMember: {5}",
						ex.Message,
						module.ID,
						ControlDescription.GetControlPath(this),
						DataSource == null ? "null" : DataSource.GetType().FullName + ": " + DataSource + ": " + ((DataSource as BusinessObject)?.PK.ToString() ?? "No PK"),
						CurrentItem == null ? "null" : CurrentItem.GetType().FullName + ": " + CurrentItem + ": " + ((CurrentItem as BusinessObject)?.PK.ToString() ?? "No PK"),
						DataMember);
					throw new InvalidOperationException(message);
				}

				module.OverrideModuleDecisionProvider(GetModuleDecisionProvider(module));
				module.FilterBusinessObject.ParentModuleID = ParentModuleID;
				module.FilterBusinessObject.ParentType = ParentType;
				popup = CreateEmbeddedPopup(module);
			}
			else
			{
				ErrorReporter.ReportOnce(string.Format(Culture.Invariant, "InvalidModuleIDForControl:{0}", Name),
					string.Format(Culture.Invariant, "A valid ModuleID could not be found for: {0}\r\nList.GetType() is {1}\r\nDataSource is {2}\r\nCurrentItem is {3}\r\nDataMember is {4}\r\nDataMember is {5}",
					ControlDescription.GetControlPath(this),
					List == null ? "null" : List.GetType().FullName,
					DataSource == null ? "null" : DataSource.GetType().FullName + ": " + DataSource + ": " + ((DataSource as BusinessObject)?.PK.ToString() ?? "No PK"),
					CurrentItem == null ? "null" : CurrentItem.GetType().FullName + ": " + CurrentItem + ": " + ((CurrentItem as BusinessObject)?.PK.ToString() ?? "No PK"),
					DataMember,
					DataPropertyName));
				popup = new ZCodeFindBoxPopup(PopupCaption ?? String.Empty);
			}

			return popup;
		}

		protected virtual IModuleDecisionProvider GetModuleDecisionProvider(ZFilterModule module)
		{
			return module.GetModuleDecisionProviderForFindBoxPopup(this);
		}

		protected virtual EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			return new EmbeddedModulePopup(module);
		}

#if WINZOR
		bool IsPopupFormVisible()
		{
			if (popupForm != null)
			{
				var ePopup = popupForm as EmbeddedModulePopup;
				if (ePopup != null && ePopup.Visible)
				{
					return true;
				}
			}

			return false;
		}
#endif
		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			PopupForm_Closed(this, EventArgs.Empty);
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Implementation

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override IList List
		{
			get
			{
				return base.List;
			}
			set
			{
				base.List = value;
				GetListRelatedModuleID();
			}
		}

		protected override void InvalidateListCore()
		{
			base.InvalidateListCore();
			if (ListNoPullIfNull != null)
			{
				listRelatedModuleID = ModuleIDs.NotAssigned;
			}
		}

		ModuleIdentifier GetListRelatedModuleID()
		{
			if (listRelatedModuleID == ModuleIDs.NotAssigned && !this.IsDesignMode() && RequiresList && List != null)
			{
				listRelatedModuleID = ZMetaData.GetModuleId(List) ?? ModuleIDs.NotAssigned;
			}
			return listRelatedModuleID;
		}
		ModuleIdentifier listRelatedModuleID = ModuleIDs.NotAssigned;

		void SetCodePropertyFromText(IZForm form)
		{
			var zForm = form as ZForm;
			if (zForm != null)
			{
				var bizObj = zForm.BusinessEntity as BusinessObject;
				if (bizObj != null)
				{
					SetCodeProperty(bizObj, IFindBox.Code);
				}
			}
		}
#if DEBUG
		internal
#endif
		void SetCodeProperty(BusinessObject bizObj, string code)
		{
			var type = bizObj.GetType();
			var propertyName = CodePropertyAttribute.CodePropertyNameFromType(type);
			var attrib = Attribute.GetCustomAttribute(type.GetProperty(propertyName), typeof(NotDefaultingPropertyValueAttribute)) as NotDefaultingPropertyValueAttribute;

			if (attrib == null || (!attrib.MatchFilterValue(code)))
			{
				var descriptor = (KPropertyDescriptor)ZCustomTypeDescriptor.GetProperties(type)[propertyName];
				if (descriptor.HasSetter())
				{
					var info = bizObj.ZPropertyInfoHash.GetPropertySafe(propertyName);
					var maxLength = info?.MaxLength ?? int.MaxValue;
					var trimmedValue = (maxLength > -1) ? code.Substring(0, Math.Min(code.Length, maxLength)) : code;
					bizObj[propertyName] = trimmedValue;
				}
			}
		}

		public override bool PrefixExistsInModule(string prefix)
		{
			using (var module = NewModuleFromModuleID())
			{
				return module != null &&
					module.FilterBusinessObject != null &&
					module.FilterBusinessObject.GetModuleFilters().Any(filter => filter.Prefix == prefix);
			}
		}

		/// <summary>
		/// Remember to dispose the module this creates!
		/// </summary>
#if DEBUG
		internal
#endif
		protected virtual ZFilterModule NewModuleFromModuleID()
		{
			var moduleID = ModuleID;
			var e = new ModuleShowingEventArgs(moduleID);
			OnModuleShowing(e);

			moduleID = e.ModuleID;
			return ZFilterModule.GetZFilterModule(moduleID, GetCountryCode?.Invoke());
		}

		void OnModuleShowing(ModuleShowingEventArgs e)
		{
			if (ModuleShowing != null)
			{
				ModuleShowing(this, e);
			}
		}

		public event EventHandler<ModuleShowingEventArgs> ModuleShowing;

		#endregion
	}
}
