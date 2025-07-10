using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class ZimbabweTaxIDMacroData : ITaxIDMacroDataProvider
	{
		public TaxIDMacroData GetTaxIDMacroData()
		{
			return new TaxIDMacroData((NoResString)"VAT #: ", OrgCusCode.CodeTypes.VATCode, (NoResString)"TIN #: ", ZimbabweOrgCusCodeInfo.OrgCusCodes.TIN);
		}
	}
}
