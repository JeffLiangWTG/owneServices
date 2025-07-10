using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;
namespace Enterprise.Accounting.ElectronicMessaging.D365.Testing
{
	[TestedType(typeof(D365ObjectFactory))]
	sealed class D365ObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory()
			=> new D365ObjectFactory();

		#region Batching

		protected override Type GetExpectedBatchCreatorType()
			=> typeof(NoGroupingEInvoicingBatchCreator);

		#endregion

		#region GEI Processing

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.BatchedTransactions;

		protected override ZString MessageTypeAssignedInCountryObjectFactory
			=> D365APICommandList.Codes.GenerateInvoiceRequest;

		public void TestAPICommandList()
		{
			var expectedAPICommands = new[] { D365APICommandList.Codes.GenerateInvoiceRequest };
			AssertContainsExactElementsInAnyOrder(expectedAPICommands, new D365APICommandList().GetAllCodes());
		}

		[TestDate(2025, 05, 16)]
		public void TestUpdateGEIBatchRequest()
		{
			var eInvoicingBatch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
		
			var gei = new GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest();
			AssertNullOrEmpty(gei.MessagingSystem);

			var countryFactory = GetTestCountryFactory() as ICountryEInvoicingObjectFactory;
			countryFactory.UpdateGEIBatchRequest(eInvoicingBatch, gei);

			AssertEquals("Electronic Accounting Integration", gei.MessagingSystem);
		}

		public override void TestEInvoicingServicePoint()
		{
			var countryFactory = GetCountryFactory();
			var servicePoint = countryFactory.GetEInvoicingServicePoint(string.Empty);
			AssertEquals("XHUB_D365_EACCOUNTING", servicePoint);
		}

		protected override Type GetExpectedCredentialsLoaderType() => typeof(D365CredentialsLoader);

		#endregion

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(D365GlobalXUEFunctionalityProvider);
	}
}
