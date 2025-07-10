using System;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil.Testing
{
	[TestedType(typeof(BrazilEInvoicingObjectFactory))]
	class BrazilEInvocingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new BrazilEInvoicingObjectFactory();

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(BrazilPayloadWriter);

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCertificateCredentialSettings);

		protected override Type GetExpectedCredentialsLoaderType() => typeof(ObjectFactoryCredentialLoader);

		protected override Type GetExpectedBatchCreatorType() => typeof(NoGroupingCancellationEInvoicingBatchCreator);

		protected override Type GetExpectedEInvoicingDataValidatorType() => typeof(EInvoicingDataValidatorForBrazil);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(GlobalXUEFunctionalityProvider);

		public override void TestGetMessageType() => Assert("Overridden to implements country specific tests", true);

		public void TestGetMessageType_ReturnsGEN_ForNonCancelledTransaction()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var batch = new TransactionBatch();
			batch.TransactionCollection.Add(new TransactionInfo() { IsCancelled = false });
			var actualMessageType = countryFactory.GetMessageType(batch, null);
			AssertEquals("Non-cancelled transaction should create a GEI message of type GEN", BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, actualMessageType);
		}

		public void TestGetMessageType_ReturnsGEN_ForNullCancelledTransaction()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var batch = new TransactionBatch();
			batch.TransactionCollection.Add(new TransactionInfo() { IsCancelled = null });
			var actualMessageType = countryFactory.GetMessageType(batch, null);
			AssertEquals("Null IsCancelled should create a GEI message of type GEN", BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, actualMessageType);
		}

		public void TestGetMessageType_ReturnsCAN_ForCancelledTransaction()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var batch = new TransactionBatch();
			batch.TransactionCollection.Add(new TransactionInfo() { IsCancelled = true });
			var actualMessageType = countryFactory.GetMessageType(batch, null);
			AssertEquals("Cancelled transaction should create a GEI message of type CAN", BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest, actualMessageType);
		}

		public void TestAPICommandList()
		{
			var expectedAPICommands = new[] { "GEN", "CAN" };
			AssertContainsExactElementsInAnyOrder(expectedAPICommands, new BrazilEInvoiceAPICommandList().GetAllCodes());
		}

		public override void TestGEIMessageShouldIncludeUniversalTransaction()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var expectedApiCodesToReturnTrue = new[]
			{
				BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest,
				BrazilEInvoiceAPICommandList.Codes.GenerateCancellationRequest,
			};

			foreach (ICodeDescription pair in new BrazilEInvoiceAPICommandList())
			{
				var geiRequest = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest() { MessageType = pair.Code };
				var expected = expectedApiCodesToReturnTrue.Contains(pair.Code)
					? IncludeUniversalTransactionStrategy.SingleTransaction
					: IncludeUniversalTransactionStrategy.NoTransaction;
				var actual = countryFactory.GEIMessageShouldIncludeUniversalTransaction(null, geiRequest);
				AssertEquals($"API Command Code '{pair.Code}' should include {expected} in Universal Transaction", expected, actual);
			}
		}

		public void TestCredentialsLoaderConfiguration()
		{
			var actualLoader = ((ICountryEInvoicingObjectFactory)GetTestCountryFactory()).GetCredentialsLoader();
			AssertNotNull(actualLoader);
			AssertType<ObjectFactoryCredentialLoader>(actualLoader);

			var concreteLoader = (ObjectFactoryCredentialLoader)actualLoader;
			AssertEquals(nameof(concreteLoader.CertificateOrderByColumn), GlbExternalPassword.Schema.GP_IssueDate, concreteLoader.CertificateOrderByColumn.Name);
		}
	}

	[TestedType(typeof(BrazilEInvoicingCredentialSettings))]
	class BrazilEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new BrazilEInvoicingCredentialSettings();

		protected override bool ExpectedIsBranchCredentialsRequired => true;
	}
}
