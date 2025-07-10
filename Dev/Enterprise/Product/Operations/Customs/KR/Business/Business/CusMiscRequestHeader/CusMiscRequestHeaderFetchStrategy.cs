using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusMiscRequestHeaderFetchStrategy(CusMiscRequestHeader cusMiscRequestHeader)
			: base(cusMiscRequestHeader)
		{
		}

		new CusMiscRequestHeader BusinessObject
		{
			get { return (CusMiscRequestHeader)base.BusinessObject; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusMiscRequestLineSchema.CML_CMR, BusinessObject.PK);
		}
	}
}
