using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomsSummaryLineCharge : BaseCusStatementLineCharge
{
	public CustomsSummaryLineCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}
}
