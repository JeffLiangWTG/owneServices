using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class BurkinaFasoTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"IFU #: ", BurkinaFasoOrgCusCodeInfo.OrgCusCodes.IFU, (NoResString)"RCCM #: ", BurkinaFasoOrgCusCodeInfo.OrgCusCodes.RCM);
		}
	}
}
