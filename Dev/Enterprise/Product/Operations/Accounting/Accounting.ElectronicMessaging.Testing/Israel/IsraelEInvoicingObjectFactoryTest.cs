using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Israel.Testing
{
	[TestedType(typeof(IsraelEInvoicingObjectFactory))]
	class IsraelEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new IsraelEInvoicingObjectFactory();

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(IsraelGlobalXUEFunctionalityProvider);

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => IsraelEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override bool IsIEInvoicingCredentialXUEBehaviorProviderImplemented => true;

		protected override Type GetElectronicMessagingNotificationEmailCreatorType()
			=> typeof(GlobalEInvoiceTokenNotificationEmailCreator);

		protected override Type GetExpectedEInvoicingDataValidatorType() => typeof(EInvoicingDataValidatorForIsrael);
	}
}
