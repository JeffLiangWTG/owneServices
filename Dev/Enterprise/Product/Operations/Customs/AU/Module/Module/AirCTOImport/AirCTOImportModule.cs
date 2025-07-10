using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AirCTOImportModule : CMRModule
	{
		public AirCTOImportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.AirCTOImport; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCTOImport);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AirCTOFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CTOCusHAWBModuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AirCTOImportFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ImportAirCTOReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsAirCTOImport; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
