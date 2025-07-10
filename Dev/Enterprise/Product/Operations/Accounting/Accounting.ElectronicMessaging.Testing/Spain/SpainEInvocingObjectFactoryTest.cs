using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Spain.Testing
{
	[TestedType(typeof(SpainEInvoicingObjectFactory))]
	class SpainEInvocingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForSpain);

		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new SpainEInvoicingObjectFactory();

		protected override ZString MessageTypeAssignedInCountryObjectFactory => SpainEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCertificateCredentialSettings);

		protected override Type GetExpectedCredentialsLoaderType() => typeof(ObjectFactoryCredentialLoader);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(SpainGlobalXUEFunctionalityProvider);

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction => IncludeUniversalTransactionStrategy.BatchedTransactions;

		public void TestGEIMessageShouldIncludeUniversalTransactionStategySetToBatched()
		{
			var factory = GetTestCountryFactory() as ICountryEInvoicingObjectFactory;
			var strategy = factory.GEIMessageShouldIncludeUniversalTransaction(null, null);
			AssertEquals(IncludeUniversalTransactionStrategy.BatchedTransactions, strategy);
		}

		public void TestCredentialsLoaderConfiguration()
		{
			var actualLoader = ((ICountryEInvoicingObjectFactory)GetTestCountryFactory()).GetCredentialsLoader();
			AssertNotNull(actualLoader);
			AssertType<ObjectFactoryCredentialLoader>(actualLoader);

			var concreteLoader = (ObjectFactoryCredentialLoader)actualLoader;
			AssertEquals(nameof(concreteLoader.CertificateOrderByColumn), GlbExternalPassword.Schema.GP_IssueDate, concreteLoader.CertificateOrderByColumn.Name);
		}
	}

	[TestedType(typeof(SpainEInvoicingCredentialSettings))]
	class SpainEInvoicingCredentialSettingsTest : GlobalEInvoicingCertificateCredentialSettingsTest
	{
		protected override IEInvoicingCertificateCredentialSettings GetCredentialSettings() => new SpainEInvoicingCredentialSettings();

		protected override bool ExpectedIsBranchCredentialsRequired => true;
	}
}
