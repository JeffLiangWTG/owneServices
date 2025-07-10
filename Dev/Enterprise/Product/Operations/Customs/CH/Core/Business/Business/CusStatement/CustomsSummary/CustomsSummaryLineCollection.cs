using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomsSummaryLineCollection : ActiveBusinessObjectCollection<CustomsSummaryLine>
{
	public CustomsSummaryLineCollection(CustomsSummaryHeader parent) : base(parent)
	{
	}

	public CustomsSummaryLineCollection(BusinessObjectFactory factory) : base(factory, GetCompanyFilter())
	{
	}

	static ZQuery GetCompanyFilter()
	{
		var headerFilter = new ZDBOnlySubQuery(typeof(CustomsSummaryHeader), CusStatementLineSchema.B3_B2);
		headerFilter.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbBranch.CurrentBranch.Company.PK);

		var query = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
		query.AddSubQuery(headerFilter, JoinCondition.And);

		return query;
	}

	protected override bool AllowNew => false;
}
