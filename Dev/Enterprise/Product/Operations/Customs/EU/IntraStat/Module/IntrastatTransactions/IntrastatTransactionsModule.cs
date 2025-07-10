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
	public class IntrastatTransactionsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.IntrastatTransactions;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuIntrastatTransactions;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Intrastat;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.IntrastatTransactionsController);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusIntrastatHeaderCollection(Factory, new ZQuery(CusIntrastatHeaderSchema.CIH_GC_Company, GlbCompany.CurrentCompany.PK));

		protected override IFilterControl GetNewFilterControl() => new IntrastatTransactionsFilterStripControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new IntrastatTransactionsFilterStripBusinessObject();
	}
}
