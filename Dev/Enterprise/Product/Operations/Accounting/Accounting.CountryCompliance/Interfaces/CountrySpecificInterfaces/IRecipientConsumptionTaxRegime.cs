using CargoWise.Types;

namespace Enterprise.Accounting.CountryCompliance.Interfaces
{
	public interface IRecipientConsumptionTaxRegime
	{
		ZString[] GetOrgCusCodes();
	}
}
