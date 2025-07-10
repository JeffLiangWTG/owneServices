using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	[TestedType(typeof(ArgentinaEInvoicingObjectFactory))]
	class ArgentinaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		public override void TestGetMessageType()
		{
			var argentinaEInvoiceObjectFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var transactionInfo = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			var transactionBatch = new TransactionBatch(DefaultDataObjectWriterStrategy.TestInstance);
			transactionBatch.TransactionCollection.Add(transactionInfo);

			AssertExceptionThrown<ArgumentException>("Invalid Message Type.", () =>
			{
				argentinaEInvoiceObjectFactory.GetMessageType(transactionBatch, null);
			});

			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			AccountingElectronicMessagingRegistry.Instance.ReportDomesticTransactionsWithWSMTXCAWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertMessageTypeShort(ArgentinaEInvoiceAPICommandList.Codes.GenerateLocalInvoiceRequest);

			AccountingElectronicMessagingRegistry.Instance.ReportDomesticTransactionsWithWSMTXCAWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertMessageTypeShort(ArgentinaEInvoiceAPICommandList.Codes.GenerateDetailItemsInvoiceRequest);

			transactionInfo.ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXE;
			AssertMessageTypeShort(ArgentinaEInvoiceAPICommandList.Codes.GenerateExportInvoiceRequest);

			void AssertMessageTypeShort(ZString expectedValue)
			{
				var actualMessageType = argentinaEInvoiceObjectFactory.GetMessageType(transactionBatch, null);
				AssertEquals("MessageType", expectedValue, actualMessageType);
			}
		}

		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new ArgentinaEInvoicingObjectFactory();

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(ArgentinaEInvoiceXmlWriter);

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCertificateCredentialSettings);

		protected override Type GetExpectedCredentialsLoaderType() => typeof(ObjectFactoryCredentialLoader);

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForArgentina);

		protected override bool? ExpectedSupportsWaitForOriginalTransactionForAmending => true;

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => PopulateOptionalXUTFieldsSetting.AllTrue();
	}
}
