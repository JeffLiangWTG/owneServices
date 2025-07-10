using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	public interface ICountryEInvoicingRegistryInformationProvider
	{
		ZString GetTaxRegimeInformation();
	}
}
