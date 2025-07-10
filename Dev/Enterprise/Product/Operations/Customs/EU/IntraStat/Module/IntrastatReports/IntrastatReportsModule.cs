using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public class IntrastatReportsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.IntrastatReports;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.EU.IntrastatReports);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ReportsFilterControl(GridCollection, (ReportsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new CusIntrastatGroupCollection(Factory, new ZQuery(CusIntrastatGroupSchema.CIG_GC_Company, GlbCompany.CurrentCompany.PK));
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ReportsFilterBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuIntrastatReports;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Intrastat;
	}
}
