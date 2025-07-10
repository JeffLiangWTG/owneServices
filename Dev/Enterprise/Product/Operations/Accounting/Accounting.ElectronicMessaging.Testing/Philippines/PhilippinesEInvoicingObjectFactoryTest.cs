using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Philippines.Credential;
using Enterprise.Integration.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Philippines.Testing
{
	[TestedType(typeof(PhilippinesEInvoicingObjectFactory))]
	sealed class PhilippinesEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new PhilippinesEInvoicingObjectFactory();

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCredentialSettings);

		protected override Type GetExpectedCredentialsLoaderType() => typeof(PhilippinesCredentialLoader);

		public override void TestGetMessageType() => Assert("Overridden to implements country specific tests", true);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(GlobalXUEFunctionalityProvider);

		protected override ZString MessageTypeAssignedInCountryObjectFactory => PhilippinesEInvoiceAPICommandList.Codes.GenerateSubmitTaxInvoice;

		public void TestGetMessageType_ReturnsGEN_ForNonCancelledTransaction()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var batch = new TransactionBatch();
			batch.TransactionCollection.Add(new TransactionInfo() { IsCancelled = false });
			var actualMessageType = countryFactory.GetMessageType(batch, null);

			AssertEquals("Non-cancelled transaction should create a GEI message of type GEN", PhilippinesEInvoiceAPICommandList.Codes.GenerateSubmitTaxInvoice, actualMessageType);
		}

		public void TestGetMessageType_ReturnsGEN_ForNullCancelledTransaction()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var batch = new TransactionBatch();
			batch.TransactionCollection.Add(new TransactionInfo() { IsCancelled = null });
			var actualMessageType = countryFactory.GetMessageType(batch, null);

			AssertEquals("Null IsCancelled should create a GEI message of type GEN", PhilippinesEInvoiceAPICommandList.Codes.GenerateSubmitTaxInvoice, actualMessageType);
		}

		public override void TestGEIMessageShouldIncludeUniversalTransaction()
		{
			TestGEIMessageShouldIncludeUniversalTransaction(PhilippinesEInvoiceAPICommandList.Codes.GenerateSubmitTaxInvoice);
			TestGEIMessageShouldIncludeUniversalTransaction("___");
		}

		void TestGEIMessageShouldIncludeUniversalTransaction(string code)
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var geiRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest() { MessageType = code };
			var actual = countryFactory.GEIMessageShouldIncludeUniversalTransaction(null, geiRequest);
			var expected = IncludeUniversalTransactionStrategy.SingleTransaction;

			AssertEquals($"API Command Code '{code}' should include {expected} in Universal Transaction", expected, actual);
		}
	}
}
