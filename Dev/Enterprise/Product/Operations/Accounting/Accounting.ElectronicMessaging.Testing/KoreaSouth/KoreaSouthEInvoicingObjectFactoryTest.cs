using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingObjectFactory))]
	class KoreaSouthEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new KoreaSouthEInvoicingObjectFactory();

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(KoreaSouthEInvoiceXmlWriter);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(KoreaSouthGlobalXUEFunctionalityProvider);

		public override void TestGetMessageType()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1234567", testObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 1234, EInvoicingBatchState.Ready);

			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			pivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			var actualMessageType = countryFactory.GetMessageType(null, batch);
			AssertEquals(KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, actualMessageType);

			pivot.AIP_ActionType = EInvoicingPivotActionType.StatusCheck;
			actualMessageType = countryFactory.GetMessageType(null, batch);
			AssertEquals(KoreaSouthEInvoiceAPICommandList.Codes.QueryInvoiceRequest, actualMessageType);
		}

		protected override Type GetExpectedCredentialsLoaderType() => typeof(ObjectFactoryCredentialLoader);

		public void TestShouldNotImplementIEInvoicingSignatureBuilderProvider()
		{
			AssertNull("IEInvoicingSignatureBuilderProvider should not be implemented. If this interface is implemented, pivot status can be changed from pending to queued.", GetTestCountryFactory() as IEInvoicingSignatureBuilderProvider);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
