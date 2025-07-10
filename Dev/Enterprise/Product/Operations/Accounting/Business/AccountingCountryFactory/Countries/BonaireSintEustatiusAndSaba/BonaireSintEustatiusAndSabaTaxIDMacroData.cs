using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class BonaireSintEustatiusAndSabaTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"CRIB #: ", BonaireSintEustatiusAndSabaOrgCusCodeInfo.OrgCusCodes.CRB);
		}
	}
}
