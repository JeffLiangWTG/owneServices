using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class TurkmenistanTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
			=> new TaxIDMacroData((NoResString)"VAT #: ", OrgCusCode.CodeTypes.VATCode);
	}
}
