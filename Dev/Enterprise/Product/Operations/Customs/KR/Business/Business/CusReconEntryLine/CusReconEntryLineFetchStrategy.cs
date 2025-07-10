using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusReconEntryLineFetchStrategy(CusReconEntryLine cusReconEntryLine)
			: base(cusReconEntryLine)
		{
		}

		CusReconEntryLine ReconEntryLine
		{
			get { return (CusReconEntryLine)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(CusReconCustomsChargeSchema.CRC_CRL_Line, ReconEntryLine.PK);
		}
	}
}
