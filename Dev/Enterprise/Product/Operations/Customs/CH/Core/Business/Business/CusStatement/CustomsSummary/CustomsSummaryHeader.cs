using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

[SingleObjectAroundARow]
public sealed class CustomsSummaryHeader : BaseCusStatementHeader
{
	public CustomsSummaryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public CustomsSummaryLineCollection SummaryLines => summaryLines ??= new CustomsSummaryLineCollection(this);
	CustomsSummaryLineCollection summaryLines;
}
