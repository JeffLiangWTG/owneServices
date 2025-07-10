using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class CapeVerdeTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"NIF#: ", OrgCusCode.CodeTypes.IVA);
		}
	}
}
