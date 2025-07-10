using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary.Testing
{
	[TestedType(typeof(HungaryEInvoicingObjectFactory))]
	class HungaryEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory()
			=> new HungaryEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.NoTransaction;

		protected override Type GetExpectedBatchCreatorType()
			=> typeof(EInvoicingBatchCreatorForHungary);

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCertificateCredentialSettings);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType()
			=> typeof(HungaryGlobalXUEFunctionalityProvider);

		protected override ZString MessageTypeAssignedInCountryObjectFactory
			=> HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(HungaryPayloadWriter);

		[TestDate(2024, 04, 04)]
		public void TestUpdateGEIBatchRequest()
		{
			var eInvoicingBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var credential = Factory.New<GlbCompanyExternalPasswordHUI>();
			credential.SignatureKey = "meow";
			credential.ReplacementKey = "woof";
			credential.GP_GC = eInvoicingBatch.AIB_GC;

			var gei = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest();
			AssertNullOrEmpty(gei.MessagingSystem);
			AssertNullOrEmpty(gei.SignKey);
			AssertNullOrEmpty(gei.ReplacementKey);

			var countryFactory = GetTestCountryFactory() as ICountryEInvoicingObjectFactory;
			countryFactory.UpdateGEIBatchRequest(eInvoicingBatch, gei);

			AssertEquals("Hungary NAV Online Invoicing System", gei.MessagingSystem);
			AssertEquals("meow", EInvoicingTestHelper.DecodePassword(gei.SignKey));
			AssertEquals("woof", EInvoicingTestHelper.DecodePassword(gei.ReplacementKey));
		}
	}
}
