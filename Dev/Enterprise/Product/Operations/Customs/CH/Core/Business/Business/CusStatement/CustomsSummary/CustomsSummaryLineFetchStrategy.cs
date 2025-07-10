using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomsSummaryLineFetchStrategy : CusStatementLineFetchHint
{
	public CustomsSummaryLineFetchStrategy(BaseCusStatementLine statementLine) : base(statementLine)
	{
	}

	new BaseCusStatementLine BusinessObject => (BaseCusStatementLine)base.BusinessObject;

	protected override void FetchForLoadCore()
	{
		base.FetchForLoadCore();
		Factory.AddFetchHint(CusStatementLineChargeSchema.B4_B3, BusinessObject.PK);
	}

	protected override void FetchForViewCore(TableColumn[] columns)
	{
		base.FetchForViewCore(columns);
		Factory.AddFetchHint(typeof(CustomsSummaryHeader), BusinessObject.B3_B2);
	}
}
