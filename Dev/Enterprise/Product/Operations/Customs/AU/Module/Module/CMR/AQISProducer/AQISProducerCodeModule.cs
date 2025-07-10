using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AQISProducerCodeModule : CMRSearchOnlyModule
	{
		public AQISProducerCodeModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AQISProducerCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.AQISProducerCode);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AQISProducerCodeFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CMRAqisProducerCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AQISProducerCodeFilterBusinessObject();
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
