using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Mexico.Testing
{
	public class MexicoReversalStatusCodeConfigurationTest : TestCaseWithFactory
	{
		public void TestGetIsReversalStatusCodeAllowed()
		{
			AssertIsReversalStatusCodeAllowed(LedgerTypes.AccountsPayable, true, false);
			AssertIsReversalStatusCodeAllowed(LedgerTypes.AccountsPayable, false, false);
			AssertIsReversalStatusCodeAllowed("", false, false);
			AssertIsReversalStatusCodeAllowed("", true, false);
			AssertIsReversalStatusCodeAllowed(LedgerTypes.AccountsReceivable, false, false);
			AssertIsReversalStatusCodeAllowed(LedgerTypes.AccountsReceivable, true, true);

			void AssertIsReversalStatusCodeAllowed(ZString ledger, bool value, bool expectedResult)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
				{
					var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IInstanceProvider<IReversalStatusCodeConfiguration>;
					var result = provider.Get().GetIsReversalStatusCodeAllowed(ledger);

					AssertEquals(expectedResult, result);
				}
			}
		}

		public void TestGetReversalStatusCodeLookup()
		{
			var expectedValue = new CodeDescriptionPairList();
			expectedValue.AddPair("01", "first value test");
			expectedValue.AddPair("xx", "another value test");

			using (AccountingConfigurationRegistry.Instance.MexicoEInvoicingReversalStatusCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, expectedValue))
			{
				var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IInstanceProvider<IReversalStatusCodeConfiguration>;

				AssertContainsExactElementsInAnyOrder(expectedValue, provider.Get().GetReversalStatusCodeLookup());
			}
		}

		public void TestGetReversalStatusCodeReferenceType()
		{
			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Mexico) as IInstanceProvider<IReversalStatusCodeConfiguration>;

			AssertEquals("MXR", provider.Get().GetReversalStatusCodeReferenceType());
		}
	}
}
