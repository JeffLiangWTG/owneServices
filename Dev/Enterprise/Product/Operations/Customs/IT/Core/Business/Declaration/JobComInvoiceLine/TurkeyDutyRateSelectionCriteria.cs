using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class TurkeyDutyRateSelectionCriteria : EU.Business.Declaration.JobComInvoiceLine.RateSelectionCriteria<JobComInvoiceLine>
{
	internal TurkeyDutyRateSelectionCriteria(JobComInvoiceLine invoiceLine)
		: base(invoiceLine, Universal.Constants.RateTypes.Duty, ZString.Empty)
	{
		TradeGroupCountry = Core.Constants.CountryCodes.Turkey;
	}
}
