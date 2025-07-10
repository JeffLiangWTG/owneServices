using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class TuvaluTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"TIN #: ", TuvaluOrgCusCodeInfo.OrgCusCodes.TCT);
		}
	}
}
