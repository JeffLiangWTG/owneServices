using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	[SuppressFormDesignerAnalysis]
	public class ZFilterCollectionFindBox : ZCodeFindBox, INotificationDataMembers
	{
		#region Bare

		[CodeAlive("For VS toolbox")]
		[ToolboxItem(false)]
		public new class Bare : ZFilterCollectionFindBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

		#region Binding Mappings

		[BindingMetaDataProperty(MetaDataTypes.MaxLength, "MaxLength", Enabled = false)] // disable MaxLength binding
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		#endregion

		public ZFilterCollectionFindBox()
			: this(null)
		{
		}

		public ZFilterCollectionFindBox(IModuleFilterWithSelectedFilters moduleFilter, bool isPopupButtonEnabledWhenReadOnly = false, Type parentType = null, ModuleIdentifier parentModuleID = null)
		{
			CodeBox.Visible = false;
			ModuleFilter = moduleFilter;
			ParentModuleID = parentModuleID;
			ParentType = parentType;
			SetModuleId(moduleFilter.ModuleId);
			UpdatePopupButtonStateForSelectedModule();

			if (isPopupButtonEnabledWhenReadOnly)
			{
				PopupButtonReadonlyCanBeDifferent = true;
			}
		}

		protected override void OnModuleIdChanged()
		{
			base.OnModuleIdChanged();

			UpdatePopupButtonStateForSelectedModule();
		}

		bool HasModuleSelected => !Equals(ModuleID, ModuleIDs.NotAssigned);

		#region ReadOnly

		protected override bool GetPopupButtonReadonlyCanBeDifferentCore()
		{
			return base.GetPopupButtonReadonlyCanBeDifferentCore() || !HasModuleSelected;
		}

		void UpdatePopupButtonStateForSelectedModule()
		{
			PopupButton.ReadOnly = !HasModuleSelected;
		}

		public void SetReadOnlyWithEnabledButton()
		{
			DescriptionBox.Enabled = false;
			shouldPopupFilterStripsBeReadOnly = true;
			PopupButton.Enabled = true;
		}

		#endregion

		string[] INotificationDataMembers.NotificationDataMembers
		{
			get { return new string[] { BindToForDescription }; }
		}

		protected IModuleFilterWithSelectedFilters ModuleFilter { get; }

		#region SelectedLayout

		void OnLayoutChanged()
		{
			if (FiltersSelected != null && selectedFiltersChangedFiringSuspendedCount <= 0)
			{
				FiltersSelected(this, new FiltersSelectedEventArgs(ModuleFilter.SelectedFilters));
			}
		}

		public event EventHandler<FiltersSelectedEventArgs> FiltersSelected;

		public class FiltersSelectedEventArgs : EventArgs
		{
			public FilterStripBusinessObject SelectedFilters { get; }

			public FiltersSelectedEventArgs(FilterStripBusinessObject selectedFilters)
			{
				SelectedFilters = selectedFilters;
			}
		}

		public IDisposable SuspendSelectedFiltersChangedFiring()
		{
			selectedFiltersChangedFiringSuspendedCount++;
			return new DisposableAction(() => selectedFiltersChangedFiringSuspendedCount--);
		}

		int selectedFiltersChangedFiringSuspendedCount;

		#endregion

		#region Popup

		bool shouldPopupFilterStripsBeReadOnly;

		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			try
			{
				module.FilterBusinessObject.CopyDetailsAndFiltersFrom(ModuleFilter.SelectedFilters);
				var userDefinedFilter = ModuleFilter as ModuleUserDefinedFilter;
				FilterCollectionEmbeddedModulePopup popup;

				if (userDefinedFilter != null)
				{
					var shouldPopupBeReadOnly = shouldPopupFilterStripsBeReadOnly
						|| (userDefinedFilter.IsPublished && !EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed)
						|| (!EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed && userDefinedFilter.CreatorOrEarlyestUser != null && userDefinedFilter.CreatorOrEarlyestUser.PK != Env.CurrentUserPK);
					popup = new UserDefinedFilterEmbeddedModulePopup(module, userDefinedFilter, shouldLoadLayoutEvenWhenUnsaved: true, makeReadOnly: shouldPopupBeReadOnly);
				}
				else
				{
					var shouldPopupBeReadOnly = shouldPopupFilterStripsBeReadOnly || !RunPopupSecurityCheck();
					popup = new FilterCollectionEmbeddedModulePopup(module, shouldLoadLayoutEvenWhenUnsaved: true, makeReadOnly: shouldPopupBeReadOnly);
				}

				popup.EmbeddedModulePopupOKButtonStrategy = module.ModuleDecisionProvider;
				popup.FiltersSelected += OnFiltersSelected;

				return popup;
			}
			catch (NullReferenceException ex)
			{
				var message = string.Format(Culture.Invariant, "ModuleId: {0}\r\nFilterBusinessObject: {1}\r\nModuleFilter: {2}", // internal log, no need to translate
						module.ID,
						module.FilterBusinessObject == null ? "null" : module.FilterBusinessObject.GetType().FullName,
						ModuleFilter == null ? "null" : ModuleFilter.GetType().FullName + "\r\nSelectedFilters: " + ModuleFilter.SelectedFilters == null ? "null" : ModuleFilter.SelectedFilters.GetType().FullName); // internal log, no need to translate
				throw new InvalidOperationException(message, ex);
			}
		}

#if DEBUG
		internal
#endif
		protected override ZFilterModule NewModuleFromModuleID()
		{
			var module = base.NewModuleFromModuleID();
			if (ModuleFilter is ModuleUserDefinedFilter moduleUserDefined)
			{
				module.ShouldLoadFilterBizOIndexSearchFilter = moduleUserDefined.IsIndexSearch;
				module.AllowToggleIndexSearchFilterMenuItemVisibility = false;
			}
			else if (ModuleFilter is ModuleGuidFilter moduleGuidForeignCollectionFilter)
			{
				module.AllowToggleIndexSearchFilterMenuItemVisibility = false;
			}
			return module;
		}
		protected override IModuleDecisionProvider GetModuleDecisionProvider(ZFilterModule module)
		{
			return new FilterSelectionPopupModuleDecisionProvider(this);
		}

		protected void OnFiltersSelected(object sender, EmbeddedModulePopup.FiltersSelectedEventArgs e)
		{
			UpdateLayout(e.Filters);
		}

		void UpdateLayout(FilterStripBusinessObject filters)
		{
			ModuleFilter.UpdateSelectedFilters(filters);
			OnLayoutChanged();
		}

		#endregion

		#region IDataBoundControl Members

		public override Type DataSourceType => typeof(StmModuleFilter);

		#endregion

		protected override bool CanReferenceByDescription => false;
	}
}
