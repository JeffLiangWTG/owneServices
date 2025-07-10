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
	public class AirCTOExportModule : ZFilterGridModule
	{
		public AirCTOExportModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.AU.AirCTOExport; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCTOExport);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AirCTOExportFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ExportCustomsManifestHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AirCTOExportFilterBusinessObject(true, false);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ExportAirCTOReport; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AUCustomsAirCTOExport; }
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}
}
