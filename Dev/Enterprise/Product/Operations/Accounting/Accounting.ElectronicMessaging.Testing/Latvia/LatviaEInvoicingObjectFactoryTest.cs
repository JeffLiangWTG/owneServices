using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Latvia.Testing
{
	[TestedType(typeof(LatviaEInvoicingObjectFactory))]
	sealed class LatviaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory()
			=> (CountryEInvoicingObjectFactory)GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.Latvia);

		#region Batching

		protected override Type GetExpectedBatchCreatorType()
			=> typeof(NoGroupingEInvoicingBatchCreator);

		#endregion

		#region GEI Processing

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory
			=> LatviaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		public void TestAPICommandList()
		{
			var expectedAPICommands = new[] { LatviaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest };
			AssertContainsExactElementsInAnyOrder(expectedAPICommands, new LatviaEInvoiceAPICommandList().GetAllCodes());
		}

		#endregion

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(GlobalXUEFunctionalityProvider);
	}
}
