using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CHConditionCalcDataForInvoiceLine : ConditionCalcDataForInvoiceLine
{
	public CHConditionCalcDataForInvoiceLine(BaseJobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	protected override void AddCountrySpecificValues(IDictionary<string, decimal> countrySpecificValueList)
	{
		AddToDictionaryIfNotExists(countrySpecificValueList, UniversalReferenceConstants.FormulaPlaceholder.StatisticalValue, ((JobComInvoiceLine)InvoiceLine).JI_Calc_StatisticalValue);
	}
}
