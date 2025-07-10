using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlReportModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.ExitControlReport;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EuExitControlReport;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override ZBool HasActions => true;

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.ExitControlReport);

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			ZQuery zQuery = new ZQuery();
			ZDBOnlyQuery zDBOnlyQuery = new ZDBOnlyQuery(typeof(CusExitReport));
			ZDBOnlySubQuery zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(CusExitHeader), CusExitReportSchema.CER_CXH_Header);
			ZDBOnlySubQuery zDBOnlySubQuery2 = new ZDBOnlySubQuery(typeof(GlbCompany), CusExitHeaderSchema.CXH_GC_Company);
			zDBOnlySubQuery2.AddToFilter(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK);
			zDBOnlySubQuery.AddSubQuery(zDBOnlySubQuery2, JoinCondition.And);
			zDBOnlyQuery.AddSubQuery(zDBOnlySubQuery, JoinCondition.And);
			zQuery.AddToFilter(zDBOnlyQuery);

			return new ExitControlBase.Business.CusExitReportCollection<CusExitReport>(Factory, zQuery);
		}

		protected override IFilterControl GetNewFilterControl() => new ExitControlReportFilterStripControl(GridCollection, (ExitControlReportFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ExitControlReportFilterBusinessObject();
	}
}
