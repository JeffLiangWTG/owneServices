using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing
{
	[TestedType(typeof(SaudiArabiaEInvoicingObjectFactory))]
	class SaudiArabiaEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new SaudiArabiaEInvoicingObjectFactory();

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.SingleTransaction;

		protected override ZString MessageTypeAssignedInCountryObjectFactory => EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCertificateCredentialSettings);

		protected override Type GetExpectedCredentialsLoaderType() => typeof(ObjectFactoryCredentialLoader);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(SaudiArabiaGlobalXUEFunctionalityProvider);

		public void TestCredentialsLoaderConfiguration()
		{
			var actualLoader = ((ICountryEInvoicingObjectFactory)GetTestCountryFactory()).GetCredentialsLoader();
			AssertNotNull(actualLoader);
			AssertType<ObjectFactoryCredentialLoader>(actualLoader);

			var concreteLoader = (ObjectFactoryCredentialLoader)actualLoader;
			AssertEquals(nameof(concreteLoader.CertificateOrderByColumn), GlbExternalPassword.Schema.GP_IssueDate, concreteLoader.CertificateOrderByColumn.Name);
		}

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForSaudiArabia);

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(SaudiArabiaEInvoicingAdditionalDataItemsProvider);
	}

	[TestedType(typeof(SaudiArabiaEInvoicingCredentialSettings))]
	class SaudiArabiaEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new SaudiArabiaEInvoicingCredentialSettings();

		protected override bool ExpectedIsBranchCredentialsRequired => true;

		protected override bool ExpectedIsBranchRegistrationRequired => true;
	}
}
