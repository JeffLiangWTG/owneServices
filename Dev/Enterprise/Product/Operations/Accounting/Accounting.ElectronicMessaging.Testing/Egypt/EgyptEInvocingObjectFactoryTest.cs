using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt.Testing
{
	[TestedType(typeof(EgyptEInvoicingObjectFactory))]
	class EgyptEInvocingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new EgyptEInvoicingObjectFactory();

		protected override ZString MessageTypeAssignedInCountryObjectFactory => EgyptEInvoiceAPICommandList.Codes.GenerateInvoiceSubmission;

		protected override Type GetExpectedCredentialsLoaderType() => typeof(RegistryCredentialLoader);

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(EgyptPayloadWriter);

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreator);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(EgyptGlobalXUEFunctionalityProvider);

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => PopulateOptionalXUTFieldsSetting.AllTrue();

		public void TestBatchSize_Is100ByDefault()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
			var batchCreator = (EInvoicingBatchCreator)countryFactory.GetBatchCreator(GlbCompany.CurrentCompany);
			AssertEquals("Due to architectural limitations, the batch size should be 98.", 98, batchCreator.MaximumBatchSize);
		}

		public void TestBatchSize_IsOverridableViaRegistry()
		{
			using (AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();
				var batchCreator = (EInvoicingBatchCreator)countryFactory.GetBatchCreator(GlbCompany.CurrentCompany);
				AssertEquals(5, batchCreator.MaximumBatchSize);
			}
		}
	}
}
