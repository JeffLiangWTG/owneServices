using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.India.Testing
{
	[TestedType(typeof(IndiaEInvoicingObjectFactory))]
	class IndiaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new IndiaEInvoicingObjectFactory();

		#region Batching

		protected override Type GetExpectedBatchCreatorType()
			=> typeof(NoGroupingEInvoicingBatchCreator);

		protected override Type GetExpectedEInvoicingDataValidatorType()
			=> typeof(EInvoicingDataValidatorForIndia);

		#endregion

		#region GEI Processing

		protected override ZString MessageTypeAssignedInCountryObjectFactory
			=> IndiaEInvoiceAPICommandList.Codes.GenerateIRN;

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override Type GetExpectedAdditionalDataItemsProviderType()
			=> typeof(IndiaAdditionalDataItemsProvider);

		protected override Type GetExpectedCredentialsLoaderType()
			=> typeof(IndiaCredentialsLoader);

		#endregion

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(IndiaGlobalXUEFunctionalityProvider);
	}
}
