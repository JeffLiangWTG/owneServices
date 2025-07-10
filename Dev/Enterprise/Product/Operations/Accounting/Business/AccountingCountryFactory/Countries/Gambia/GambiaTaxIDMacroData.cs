using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class GambiaTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"VAT #: ", OrgCusCode.CodeTypes.VATCode);
		}
	}
}
