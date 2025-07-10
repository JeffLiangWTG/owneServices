using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class UserDefinedFilterEmbeddedModulePopup : FilterCollectionEmbeddedModulePopup
	{
		public UserDefinedFilterEmbeddedModulePopup(ZFilterModule module, ModuleUserDefinedFilter filter, bool shouldLoadLayoutEvenWhenUnsaved = false, bool makeReadOnly = false)
			: base(module, shouldLoadLayoutEvenWhenUnsaved, makeReadOnly)
		{
			this.userDefinedFilterName = filter.MultilingualDescription;
			this.filter = filter;
			isMadeReadOnly = makeReadOnly;

			var control = module.DisplayGrid.GetParentFilterControl();
			control.HideManageLayoutsButton();
			control.OnFilterStripLoaded += (sender, args) =>
			{
				FilterStripsLoaded?.Invoke(this, EventArgs.Empty);
			};

			control.LayoutSaved += OnLayoutSaved;

			filter.ShouldSelectedFilterLayoutBeReloaded = true;

			control.GetDefaultValuesForSaveLayoutFunc = GetSaveLayoutBusinessObject;
			OK_Button.Text = Res.GetString("498c06c6-c8e6-44e0-9bc5-ff04ae03bba4", "Save");
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (isMadeReadOnly)
			{
				var message = string.Empty;
				if (!Env.Security.EditUserDefinedFilters.IsAllowed)
				{
					message = Env.Instance.Security.EditUserDefinedFilters.ErrorMessageForNotAllowed;
				}
				else if (!Env.Security.PublishUserDefinedFilters.IsAllowed && filter.IsPublished)
				{
					message = Env.Security.PublishUserDefinedFilters.ErrorMessageForNotAllowed;
				}
				if (!string.IsNullOrEmpty(message))
				{
					Globals.Message.Show(message, Res.GetString("9580B38E-F22F-4D23-9171-85970514F860", "Edit User-Defined Filter"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
				}
			}
		}

		public override string FormCaption => isMadeReadOnly ? Res.GetString("acccd6d5-a054-4d68-aee6-a04c2ca506d3", "View {0}", userDefinedFilterName)
			: Res.GetString("f5acc15d-c1ab-431f-988e-77efc8b9607e", "Edit {0}", userDefinedFilterName);

		readonly ModuleUserDefinedFilter filter;
		readonly string userDefinedFilterName;
		readonly bool isMadeReadOnly;
		internal event EventHandler FilterStripsLoaded;

		protected override void OnOkButtonClicked()
		{
			var existingLayout = GetExistingLayout();
			var saveLayoutBusinessObject = GetSaveLayoutBusinessObject(existingLayout);

			var caption = Res.GetString("3bbec130-9567-4263-b065-9cd391f61d3a", "Cannot Save User-Defined Filter");
			if (!EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed && existingLayout.CreatorOrEarlyestUser?.PK != EnvProxy.Instance.CurrentUser.PK)
			{
				Globals.Message.ShowError(EnvProxy.Instance.Security.EditUserDefinedFilters.ErrorMessageForNotAllowed, caption);
			}
			else if (saveLayoutBusinessObject.HasErrors)
			{
				var errors = string.Join(System.Environment.NewLine, saveLayoutBusinessObject.GetErrors().Select(x => x.Message));
				Globals.Message.ShowError(errors, caption);
			}
			else
			{
				var userResult = PromptUserToSaveLayoutAndGetResponse(existingLayout);

				if (userResult == DialogResult.OK)
				{
					DataGridLayoutManager.SaveLayout(saveLayoutBusinessObject, Module.FilterBusinessObject, false);
					base.OnOkButtonClicked();
				}
			}
		}

		DialogResult PromptUserToSaveLayoutAndGetResponse(StmModuleFilter existingLayout)
		{
			string caption = Res.GetString("a1caccf1-5368-409e-8f6a-1d7d37e0379f", "Save User-Defined Filter");
			var message = ResString.GetMultilingualString(
				"810d7dc4-7350-4c33-a1b3-6ee1081ece01",
				"Any changes made to the user-defined filter [{0}] will be saved. These changes will affect all saved layouts that use this filter. Would you like to save your changes?",
				existingLayout?.S9_FilterNameMultilingual ?? userDefinedFilterName);
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
		}

		StmModuleFilter GetExistingLayout(ZGuid? layoutPK = null)
		{
			var filterBizo = Module.FilterBusinessObject;
			var layouts = UserDefinedFilterHelper.GetLayoutsForUserDefinedFilters(filterBizo.Factory, filterBizo.LayoutsHelper, Module.ID.Name, includePublishedOnly: false);
			var layoutPKToUse = layoutPK ?? this.filter.LayoutPK;

			return layouts.SingleOrDefault(x => x.PK == layoutPKToUse);
		}

		SaveLayoutBizO GetSaveLayoutBusinessObject()
		{
			var existingLayout = GetExistingLayout();

			return GetSaveLayoutBusinessObject(existingLayout);
		}

		SaveLayoutBizO GetSaveLayoutBusinessObject(StmModuleFilter existingLayout)
		{
			var saveLayoutBusinessObject = new SaveLayoutBizO(Module.FilterBusinessObject)
			{
				LayoutName = userDefinedFilterName,
				IsUserDefinedFilter = true
			};

			if (existingLayout != null) // it could have been deleted by someone else in the meantime, which is okay.
			{
				saveLayoutBusinessObject.PublishLayout = existingLayout.S9_IsPublished;
				saveLayoutBusinessObject.PublishAcrossAllCompanies = existingLayout.S9_GC.IsEmpty;
				saveLayoutBusinessObject.S9_ModuleID = existingLayout.S9_ModuleID;
			}

			return saveLayoutBusinessObject;
		}

		void OnLayoutSaved(object sender, ZFilterStripBaseControl.LayoutSavedEventArgs e)
		{
			if (!e.SavedLayout.IsDeleted && e.SavedLayout.IsUserDefinedFilter && e.SavedLayout.S9_FilterName == userDefinedFilterName)
			{
				filter.Reload(GetExistingLayout(e.SavedLayout.PK));
			}
		}

#if DEBUG
		public ZButton OKButton_Exposed => OK_Button;
#endif
	}
}
