using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	class MalaysiaEInvoicingPivotPreHandleProviderTest : TestCaseWithFactory
	{
		public void TestDiscardDocumentDetailPivotAndBatchForAction()
		{
			var eInvoicingPivotActionTypesProvider = GetEInvoicingPivotActionTypesProvider(CountryCode);
			var discardAdditionalPivotActionTypes = eInvoicingPivotActionTypesProvider.GetAdditionalPivotActionTypes(EInvoicingPivotActionType.DocumentAction).ToList<string>();
			AssertEquals(0, discardAdditionalPivotActionTypes.Count);

			discardAdditionalPivotActionTypes = eInvoicingPivotActionTypesProvider.GetAdditionalPivotActionTypes(EInvoicingPivotActionType.StatusCheck).ToList<string>();
			AssertEquals(1, discardAdditionalPivotActionTypes.Count);
			AssertEquals(EInvoicingPivotActionType.DocumentDetail, discardAdditionalPivotActionTypes[0]);
		}

		const string CountryCode = CountryCodes.Malaysia;

		IEInvoicingDiscardAdditionalPiviotActionTypesProvider GetEInvoicingPivotActionTypesProvider(string countryCode) => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IInstanceProvider<IEInvoicingDiscardAdditionalPiviotActionTypesProvider>).Get();
	}
}
