using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	[TestedType(typeof(RomaniaEInvoicingObjectFactory))]
	class RomaniaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new RomaniaEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(RomaniaAdditionalDataItemsProvider);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(RomaniaGlobalXUEFunctionalityProvider);

		protected override Type GetExpectedEInvoicingDataValidatorType() => typeof(RomaniaEInvoicingDataValidator);

		protected override Type GetExpectedBatchCreatorType() => typeof(RomaniaEInvoicingBatchCreator);

		protected override bool IsIEInvoicingCredentialXUEBehaviorProviderImplemented => true;

		public override void TestGetMessageType()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice);
			var batch = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot, 001, EInvoicingBatchState.Ready);

			pivot.AIP_Status = EInvoicingPivotState.Queued;
			var actualMessageType = countryFactory.GetMessageType(null, batch);
			AssertEquals(RomaniaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest, actualMessageType);

			pivot.AIP_Status = EInvoicingPivotState.Delivered;
			actualMessageType = countryFactory.GetMessageType(null, batch);
			AssertEquals(RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest, actualMessageType);
		}

		public void TestAuthorizationRecordType()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			AssertEquals("ROA", countryFactory.AuthorizationRecordType);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
