using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.AdditionalDataItems;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	public interface ICountryEInvoicingObjectFactory : ICountryEInvoicingObjectFactorySettings
	{
		/// <summary>
		/// EInvoicing related feature settings for this country as defined by Feature Control rule ACCEINVCF.
		/// Never null, but leaf properties may be empty / null unless the Feature Control rule is applied to the current database and country code.
		/// </summary>
		/// <remarks>
		/// For further details, see https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/14784/Feature-Control-Rule-ACCEINVCF
		/// </remarks>
		EInvoicingFeatureSettings FeatureSettings { get; }

		ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter();

		void UpdateGEIBatchRequest(AccEInvoicingBatch eInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request);

		/// <summary>
		/// Chooses the message type for GEI message.
		/// Supports override via Feature Control > Transport > MessageType. But only supports one message type for the country; no difference between submission, cancellation, etc.
		/// Value must be chosen from EInvoiceAPICommandList.
		/// </summary>
		ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch);

		/// <summary>
		/// Chooses destination for GEI message.
		/// Supports override via Feature Control > Transport > Destination; suffix parameter is ignored in this case.
		/// Value must match routing rules in eHub and / or xT.
		/// </summary>
		ZString GetEInvoicingServicePoint(ZString suffix);

		IncludeUniversalTransactionStrategy GEIMessageShouldIncludeUniversalTransaction(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest);

		PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting();

		/// <summary>
		/// Executed before the XUT(s) are set against GEI Message model.
		/// Can be used to remove or edit parts of the XUT.
		/// Note the full XUT will be available to IAdditionalDataItemsProvider implementations.
		/// </summary>
		void ModifyUniversalTransactionBeforeGEI(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest);

		/// <summary>
		/// Choose between EHub (HUB; default) and DirectXT (XTT) to send messages.
		/// Supports override via Feature Control > Transport > Delivery.
		/// </summary>
		string CommunicationTransport { get; }

		ICredentialsLoader GetCredentialsLoader();

		IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider();

		EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company);

		BaseEInvoicingDataValidator GetDataValidator(GlbCompany company);

		ZQuery GetInvoiceEventMessageTargetQuery(UniversalEvent xmlEvent);

		IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider();

		IElectronicMessagingNotificationEmailCreator GetElectronicMessagingNotificationEmailCreator();
	}

	public abstract class CountryEInvoicingObjectFactory : ICountryEInvoicingObjectFactory, ICountryEInvoicingRegistryInformationProvider, IElectronicMessagingNotificationQueryProvider
	{
		EInvoicingFeatureSettings ICountryEInvoicingObjectFactory.FeatureSettings => FeatureSettingsReader.Value;
		EInvoicingFeatureSettingsReader FeatureSettingsReader
			=> FeatureSettingsReaderUnsafe ??= new EInvoicingFeatureSettingsReader(CountryCode, ObjectFactory.Get<IFeatureControlManager>());
		EInvoicingFeatureSettingsReader FeatureSettingsReaderUnsafe;

		IElectronicMessagingNotificationEmailCreator ICountryEInvoicingObjectFactory.GetElectronicMessagingNotificationEmailCreator()
			=> GetElectronicMessagingNotificationEmailCreator();

		protected virtual IElectronicMessagingNotificationEmailCreator GetElectronicMessagingNotificationEmailCreator()
			=> null;

		#region ICountryEInvoicingObjectFactorySettings

		ZString ICountryEInvoicingObjectFactorySettings.CountryCode => CountryCode;

		ZString ICountryEInvoicingObjectFactorySettings.AuthorizationRecordType => AuthorizationRecordType;

		ZString ICountryEInvoicingObjectFactory.GetEInvoicingServicePoint(ZString suffix)
			=> FeatureSettingsReader.Value.Transport.Destination.FallbackOnNullOrEmpty(
				() => GetEInvoicingServicePoint(suffix)
			);

		IEInvoicingCredentialSettings ICountryEInvoicingObjectFactorySettings.Credentials => Credentials;

		string ICountryEInvoicingObjectFactorySettings.ApTransactionListRequestBatchId => ApTransactionListRequestBatchId;

		protected abstract ZString CountryCode { get; }

		protected virtual ZString AuthorizationRecordType =>
			(ObjectFactory.Get<ICountryComplianceFactory>().GetICountryComplianceInfoBase(CountryCode) as IComplianceInfoElectronicInvoicing)?.GetAccTransactionHeaderAuthorisationRecordType() ?? ZString.Empty;

		protected virtual ZString GetEInvoicingServicePoint(ZString suffix)
		{
			if (!suffix.IsEmpty)
			{
				suffix = $"_{suffix}";
			}

			return $"XHUB_{CountryCode}_EINVOICING{suffix}"; // Constant string used internally by EServices team.
		}

		protected virtual IEInvoicingCredentialSettings Credentials => new GlobalEInvoicingNoCredentialSettings();

		protected virtual string ApTransactionListRequestBatchId => null;

		#endregion

		#region ICountryEInvoicingObjectFactory

		ITransactionBatchToPayloadWriter ICountryEInvoicingObjectFactory.GetTransactionBatchToPayloadWriter() => GetTransactionBatchToPayloadWriter();

		void ICountryEInvoicingObjectFactory.UpdateGEIBatchRequest(AccEInvoicingBatch eInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request) => UpdateGEIBatchRequest(eInvoicingBatch, request);
		ZString ICountryEInvoicingObjectFactory.GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> FeatureSettingsReader.Value.Transport.MessageType.FallbackOnNullOrEmpty(
				() => GetMessageType(transactionBatch, eInvoicingBatch)
			);

		IncludeUniversalTransactionStrategy ICountryEInvoicingObjectFactory.GEIMessageShouldIncludeUniversalTransaction(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest) => GEIMessageIncludeUniversalTransactionStrategy(transactionBatch, geiRequest);

		PopulateOptionalXUTFieldsSetting ICountryEInvoicingObjectFactory.GEIMessagePopulateOptionalXUTFieldsSetting() => GEIMessagePopulateOptionalXUTFieldsSetting();

		void ICountryEInvoicingObjectFactory.ModifyUniversalTransactionBeforeGEI(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> ModifyUniversalTransactionBeforeGEI(transactionBatch, geiRequest);

		string ICountryEInvoicingObjectFactory.CommunicationTransport
			=> FeatureSettingsReader.Value.Transport.Delivery.FallbackOnNullOrEmpty(
				() => GetCommunicationTransport()
			);

		ICredentialsLoader ICountryEInvoicingObjectFactory.GetCredentialsLoader() => GetCredentialsLoader();

		IAdditionalDataItemsProvider ICountryEInvoicingObjectFactory.GetIAdditionalDataItemsProvider() => GetIAdditionalDataItemsProvider();

		protected virtual ITransactionBatchToPayloadWriter GetTransactionBatchToPayloadWriter() => null;

		EInvoicingBatchCreatorBase ICountryEInvoicingObjectFactory.GetBatchCreator(GlbCompany company) => GetBatchCreator(company);

		BaseEInvoicingDataValidator ICountryEInvoicingObjectFactory.GetDataValidator(GlbCompany company) => GetDataValidator(company);

		ZQuery ICountryEInvoicingObjectFactory.GetInvoiceEventMessageTargetQuery(UniversalEvent xmlEvent) => GetInvoiceEventMessageTargetQuery(xmlEvent);

		protected virtual void UpdateGEIBatchRequest(AccEInvoicingBatch eInvoicingBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest request)
		{
		}

		protected virtual ZString GetMessageType(TransactionBatch transactionBatch, AccEInvoicingBatch eInvoicingBatch)
			=> ZString.Empty;

		protected virtual IncludeUniversalTransactionStrategy GEIMessageIncludeUniversalTransactionStrategy(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest)
			=> IncludeUniversalTransactionStrategy.NoTransaction;

		protected virtual PopulateOptionalXUTFieldsSetting GEIMessagePopulateOptionalXUTFieldsSetting() => new PopulateOptionalXUTFieldsSetting(populateAttachedDocuments: false, populateShipments: false);

		protected virtual void ModifyUniversalTransactionBeforeGEI(TransactionBatch transactionBatch, GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequest geiRequest) { }

		protected virtual ZString GetCommunicationTransport() => EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

		protected virtual ICredentialsLoader GetCredentialsLoader() => null;

		protected virtual IAdditionalDataItemsProvider GetIAdditionalDataItemsProvider() => null;

		protected virtual EInvoicingBatchCreatorBase GetBatchCreator(GlbCompany company) => new NoGroupingEInvoicingBatchCreator(company);

		protected virtual BaseEInvoicingDataValidator GetDataValidator(GlbCompany company) => new NullEInvoicingDataValidator(company);

		protected virtual ZQuery GetInvoiceEventMessageTargetQuery(UniversalEvent xmlEvent) => new ZQuery();

		#endregion

		ZString ICountryEInvoicingRegistryInformationProvider.GetTaxRegimeInformation() => GetTaxRegimeInformation;

		protected virtual ZString GetTaxRegimeInformation => ZString.Empty;

		#region IGlobalXUEFunctionalityProvider

		IGlobalXUEFunctionalityProvider ICountryEInvoicingObjectFactory.GetGlobalXUEFunctionalityProvider()
			=> GetGlobalXUEFunctionalityProvider;

		protected virtual IGlobalXUEFunctionalityProvider GetGlobalXUEFunctionalityProvider
			=> new GlobalXUEFunctionalityProvider(this);

		#endregion

		#region IElectronicMessagingNotificationQueryProvider

		ZQuery IElectronicMessagingNotificationQueryProvider.GetQueryForCompany(GlbCompany company, DateTime localTimeNow, int alertDays) => GetQueryForCompany(company, localTimeNow, alertDays);

		ZQuery IElectronicMessagingNotificationQueryProvider.GetQueryForBranch(GlbCompany company) => GetQueryForBranch(company);

		protected virtual ZQuery GetQueryForCompany(GlbCompany company, DateTime localTimeNow, int alertDays) => null;

		protected virtual ZQuery GetQueryForBranch(GlbCompany company) => null;

		#endregion
	}

	public enum IncludeUniversalTransactionStrategy
	{
		NoTransaction = 0,
		SingleTransaction = 1,
		BatchedTransactions = 2,
	}
}
