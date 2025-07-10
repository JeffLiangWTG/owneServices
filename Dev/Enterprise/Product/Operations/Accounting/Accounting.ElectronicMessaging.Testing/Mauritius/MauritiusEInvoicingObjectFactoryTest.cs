using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mauritius.Testing
{
	[TestedType(typeof(MauritiusEInvoicingObjectFactory))]
	sealed class MauritiusEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory()
			=> (CountryEInvoicingObjectFactory)GlobalEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(CountryCodes.Mauritius);

		#region Batching

		protected override Type GetExpectedBatchCreatorType()
			=> typeof(NoGroupingEInvoicingBatchCreator);

		#endregion

		#region GEI Processing

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory
			=> MauritiusEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		public void TestAPICommandList()
		{
			var expectedAPICommands = new[] { MauritiusEInvoiceAPICommandList.Codes.GenerateInvoiceRequest };
			AssertContainsExactElementsInAnyOrder(expectedAPICommands, new MauritiusEInvoiceAPICommandList().GetAllCodes());
		}

		protected override Type GetExpectedCredentialsLoaderType()
			=> typeof(MauritiusCredentialLoader);

		#endregion

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(GlobalXUEFunctionalityProvider);
	}
}
