using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Poland.Testing
{
	[TestedType(typeof(PolandEInvoicingObjectFactory))]
	sealed class PolandEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new PolandEInvoicingObjectFactory();

		protected override Type GetExpectedCredentialsLoaderType()
			=> typeof(PolandCredentialLoader);

		protected override Type GetExpectedCredentialsInterfaceType()
			=> typeof(IEInvoicingCertificateCredentialSettings);

		public void TestCredentialSettings()
		{
			var factory = GetTestCountryFactory() as ICountryEInvoicingObjectFactory;
			AssertEquals("PasswordType", PasswordTypesList.Codes.EIM, factory.Credentials.PasswordType);
			AssertEquals("IsCompanyCredentialsRequired", true, factory.Credentials.IsCompanyCredentialsRequired);
			AssertEquals("IsBranchCredentialsRequired", false, factory.Credentials.IsBranchCredentialsRequired);
			AssertEquals("IsBranchRegistrationRequired", false, factory.Credentials.IsBranchRegistrationRequired);
			AssertEquals("IsCertificate()", true, factory.Credentials.IsCertificate());
			AssertEquals("IsPassword()", false, factory.Credentials.IsPassword());
		}

		protected override ZString MessageTypeAssignedInCountryObjectFactory
			=> PolandEInvoiceAPICommandList.Codes.SendInvoiceBatch;

		protected override IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction
			=> IncludeUniversalTransactionStrategy.BatchedTransactions;

		// Note that Poland populates ShipmentCollection just for the IAdditionalDataItemsProvider
		// ShipmentCollection is removed before the GEI message is sent
		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting
			=> new PopulateOptionalXUTFieldsSetting(populateShipments: true);

		protected override Type GetExpectedBatchCreatorType()
			=> typeof(EInvoicingBatchCreator);

		public void TestBatchCreator_MaximumBatchSize()
		{
			var factory = GetTestCountryFactory() as ICountryEInvoicingObjectFactory;
			var batchCreator = (EInvoicingBatchCreator)factory.GetBatchCreator(GlbCompany.CurrentCompany);
			AssertEquals("Default maximum batch size should be 3 for Poland", 3, batchCreator.MaximumBatchSize);

			AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 10);

			batchCreator = (EInvoicingBatchCreator)factory.GetBatchCreator(GlbCompany.CurrentCompany);
			AssertEquals("Default maximum batch size should be overridden by registry", 10, batchCreator.MaximumBatchSize);
		}

		public override void TestModifyUniversalTransactionBeforeGEI()
		{
			var transactionBatch = CreateTransactionBatch();
			Assert("Precondition: at least one transaction must have a shipment collection", transactionBatch.TransactionCollection.Any(t => t.ShipmentCollection != null && t.ShipmentCollection.Count > 0));

			var factory = GetTestCountryFactory() as ICountryEInvoicingObjectFactory;
			factory.ModifyUniversalTransactionBeforeGEI(transactionBatch, new Common.GlobalElectronicInvoice.GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest());

			Assert("All related shipments should be removed from transaction batch before creating GEI message", transactionBatch.TransactionCollection.All(t => t.ShipmentCollection == null || t.ShipmentCollection.Count == 0));
		}

		protected override Type GetExpectedAdditionalDataItemsProviderType()
			=> typeof(PolandEInvoicingAdditionalDataItemsProvider);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(PolandGlobalXUEFunctionalityProvider);
	}
}
