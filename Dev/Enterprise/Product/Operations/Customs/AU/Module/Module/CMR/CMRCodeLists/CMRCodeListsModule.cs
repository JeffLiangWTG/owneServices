using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class CMRCodeListsModule : CMRSearchOnlyModule
	{
		public CMRCodeListsModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CMRCodeLists; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.CMRCodeLists);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CMRCodeListsFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CMRCodeListsCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CMRCodeListsFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
