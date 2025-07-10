using Enterprise.Accounting.CountryCompliance.Implementation.Argentina;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation
{
	public class ArgentinaCountryFactory :
		IInstanceProvider<IRecipientConsumptionTaxRegime>
	{
		IRecipientConsumptionTaxRegime IInstanceProvider<IRecipientConsumptionTaxRegime>.Get() => new RecipientConsumptionTaxRegime();
	}
}
