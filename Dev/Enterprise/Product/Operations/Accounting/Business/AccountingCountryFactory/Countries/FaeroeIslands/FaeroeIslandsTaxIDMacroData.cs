using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class FaeroeIslandsTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"MVG #: ", FaeroeIslandsOrgCusCodeInfo.OrgCusCodes.MVG);
		}
	}
}
