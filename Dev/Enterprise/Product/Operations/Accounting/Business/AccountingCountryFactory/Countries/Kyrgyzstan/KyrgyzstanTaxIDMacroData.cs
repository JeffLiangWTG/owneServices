using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KyrgyzstanTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
			=> new TaxIDMacroData((NoResString)"TIN #: ", KyrgyzstanOrgCusCodeInfo.OrgCusCodes.TIN);
	}
}
