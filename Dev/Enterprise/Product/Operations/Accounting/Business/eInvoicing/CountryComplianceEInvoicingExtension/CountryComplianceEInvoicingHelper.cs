using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public static class CountryComplianceEInvoicingHelper
	{
		public static IEInvoicingTransactionValidation GetAdditionalValidation(GlbCompany company)
		{
			return ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>().GetIEInvoicingTransactionValidation(company?.GC_RN_NKCountryCode ?? ZString.Empty);
		}

		public static IExistPivotCheckProvider GetExistPivotCheckProvider(GlbCompany company)
		{
			return ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>().GetExistPivotCheckProvider(company?.GC_RN_NKCountryCode ?? ZString.Empty);
		}
	}
}
