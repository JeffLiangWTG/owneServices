using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class InstrumentNumberModule : CMRSearchOnlyModule
	{
		public InstrumentNumberModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.InstrumentNumber;
			}
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.InstrumentNumber);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new InstrumentNumberFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CMRInstrumentCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InstrumentNumberFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
