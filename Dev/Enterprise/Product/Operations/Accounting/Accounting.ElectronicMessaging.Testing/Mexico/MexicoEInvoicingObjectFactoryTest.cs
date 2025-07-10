using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	[TestedType(typeof(MexicoEInvoicingObjectFactory))]
	class MexicoEInvoicingObjectFactoryTest : CountryEInvoicingObjectFactoryTest
	{
		protected override CountryEInvoicingObjectFactory GetTestCountryFactory() => new MexicoEInvoicingObjectFactory();

		protected override Type GetExpectedTransactionBatchToPayloadWriterType() => typeof(CFDiXmlWriter);

		protected override Type GetExpectedCredentialsInterfaceType() => typeof(IEInvoicingCertificateCredentialSettings);

		protected override Type GetExpectedPayloadValidationType() => typeof(XsdValidation);

		protected override Type GetExpectedBatchCreatorType() => typeof(EInvoicingBatchCreatorForMexico);

		protected override Type GetExpectedAdditionalDataItemsProviderType() => typeof(MexicoEInvoicingAdditionalDataItemsProvider);

		protected override Type GetExpectedGlobalXUEFunctionalityProviderInterfaceType() => typeof(MexicoGlobalXUEFunctionalityProvider);

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting => PopulateOptionalXUTFieldsSetting.AllTrue();

		public override void TestGetMessageType()
		{
			var countryFactory = (ICountryEInvoicingObjectFactory)GetTestCountryFactory();

			var invoice = Factory.New<ARInvoice>();
			invoice.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;

			var batch = Factory.New<AccEInvoicingBatch>();
			var pivot = batch.TransactionPivots.AddNew();
			pivot.AIP_ParentID = invoice.PK;

			invoice.ReversalStatusCode = string.Empty;
			AssertMessageTypeInGEIRequest(MexicoEInvoiceMessageTypeProvider.Codes.GenerateInvoiceRequest);

			invoice.ReversalStatusCode = "01";
			AssertMessageTypeInGEIRequest(MexicoEInvoiceMessageTypeProvider.Codes.GenerateCancellationRequest);

			pivot.AIP_ActionType = Core.Constants.EInvoicingPivotActionType.DocumentAction;
			AssertMessageTypeInGEIRequest(MexicoEInvoiceMessageTypeProvider.Codes.RequestPDFDocumentForInvoice);

			void AssertMessageTypeInGEIRequest(string expectedMessageType)
			{
				var actualMessageType = countryFactory.GetMessageType(null, batch);
				AssertEquals("Transaction should create a GEI message of type", expectedMessageType, actualMessageType);
			}
		}

		protected override bool? ExpectedSupportsWaitForOriginalTransactionForAmending => true;

		public void TestRegistryTaxRegimeInformation()
		{
			using (AccountingElectronicMessagingRegistry.Instance.MexicoTaxRegimeID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "624"))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
				{
					var countryFactory = (ICountryEInvoicingRegistryInformationProvider)GetTestCountryFactory();
					ZString? result = countryFactory.GetTaxRegimeInformation();

					AssertEquals("624 - Coordinados", result);
				}
			}
		}
	}
}
