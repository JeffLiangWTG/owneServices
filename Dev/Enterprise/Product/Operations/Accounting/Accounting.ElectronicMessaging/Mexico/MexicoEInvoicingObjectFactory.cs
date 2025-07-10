using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class MexicoEInvoicingObjectFactory : CountryEInvoicingObjectFactory, IBatchCreatorStrategy
	{
		protected override ZString CountryCode => CountryCodes.Mexico;

		protected override ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() => new CFDiXmlWriter();

		protected override IEInvoicingCredentialSettings Credentials { get; } = new MexicoEInvoicingCredentialSettings();

		protected override EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new EInvoicingBatchCreatorForMexico(company);

		protected override ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> MexicoEInvoiceMessageTypeProvider.GetMessageType(eInvoicingBatch.TransactionPivots[0]);

		protected override ZString GetTaxRegimeInformation
		{
			get
			{
				var value = AccountingElectronicMessagingRegistry.Instance.MexicoTaxRegimeID.Value;
				var description = ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IDebtorTaxRegime>(CountryCode).GetTaxRegimeIdTypes().GetDescriptionFromCode(value);

				return value + " - " + description;
			}
		}

		bool IBatchCreatorStrategy.SupportsWaitForOriginalTransactionForAmending => true;

		protected override IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => new MexicoEInvoicingAdditionalDataItemsProvider();

		protected override PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => PopulateOptionalXUTFieldsSetting.AllTrue();

		protected override IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider => new MexicoGlobalXUEFunctionalityProvider(this);
	}
}
