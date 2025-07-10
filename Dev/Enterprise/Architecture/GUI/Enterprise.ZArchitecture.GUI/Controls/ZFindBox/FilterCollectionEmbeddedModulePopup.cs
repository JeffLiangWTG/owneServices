using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class FilterCollectionEmbeddedModulePopup : EmbeddedModulePopup
	{
		public FilterCollectionEmbeddedModulePopup(ZFilterModule module, bool shouldLoadLayoutEvenWhenUnsaved = false, bool makeReadOnly = false)
			: base(module, null, shouldLoadLayoutEvenWhenUnsaved)
		{
			var filterControl = module.DisplayGrid.GetParentFilterControl();
			filterControl.ShouldLoadStripsWhenNoLayoutsLoaded = true;
			filterControl.SetRecentItemsVisibility(false);

			if (makeReadOnly)
			{
				filterControl.IsFilterReadonly = true;
				filterControl.SetToolStripPermissionsLabel(false);
				RequireAtLeastOneItemToBeSelected = false;
			}
		}

		protected override void OnOkButtonClicked()
		{
			EmbeddedModulePopupOKButtonStrategy?.HandleFindBoxOKButton(Module.FilterBusinessObject);
		}

		protected internal override void HandleSelection(BusinessObject[] selectedBizObjs)
		{
		}
	}
}
