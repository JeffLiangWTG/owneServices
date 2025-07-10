using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class PremisesModule : CMRSearchOnlyModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Premises; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.Premises);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PremisesFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CMRAqisPremisesCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PremisesFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
