using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class IndonesiaTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"NPWP #: ", OrgCusCode.IndonesiaCodeTypes.PPN, (NoResString)"NITKU #: ", OrgCusCode.IndonesiaCodeTypes.NIT);
		}
	}
}
