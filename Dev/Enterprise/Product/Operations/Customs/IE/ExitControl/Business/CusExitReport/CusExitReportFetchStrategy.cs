using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitReportFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusExitReportFetchStrategy(CusExitReport businessObject) : base(businessObject)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(CusGoodsLocationSchema.CGL_ParentID, CusExitReport.PK);

			var docAddressQuery = new ZQuery()
				.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusExitReportSchema.Constants.Prefix)
				.AddToFilter(JobDocAddressSchema.E2_ParentID, CusExitReport.PK);
			Factory.AddFetchHint(typeof(JobDocAddress), docAddressQuery);
		}

		CusExitReport CusExitReport => (CusExitReport)BusinessObject;
	}
}
