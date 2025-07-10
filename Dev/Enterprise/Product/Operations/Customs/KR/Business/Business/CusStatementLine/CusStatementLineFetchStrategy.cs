using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusStatementLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusStatementLineFetchStrategy(CusStatementLine statementLine)
			: base(statementLine)
		{
		}

		new CusStatementLine BusinessObject
		{
			get { return (CusStatementLine)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusStatementLineChargeSchema.B4_B3, BusinessObject.PK);
		}
	}
}
