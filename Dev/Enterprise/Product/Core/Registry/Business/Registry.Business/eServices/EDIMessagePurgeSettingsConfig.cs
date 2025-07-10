using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using static Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Registry.Business
{
	public class EDIMessagePurgeSettingsConfig : PurgeSettingsConfig
	{
		public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
		{
			ZShort defaultDur = 3;
			var defaultUnit = TimeUnit.Month;

			yield return AddApplicationCodeMessageSubTypePurgeType(NativeDataMessaging, new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.XmlNativeAirline, EDIMessageSubTypeList.Descriptions.XmlNativeAirline, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCommodityCode, EDIMessageSubTypeList.Descriptions.XmlNativeCommodityCode, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCompany, EDIMessageSubTypeList.Descriptions.XmlNativeCompany, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeContainer, EDIMessageSubTypeList.Descriptions.XmlNativeContainer, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCountry, EDIMessageSubTypeList.Descriptions.XmlNativeCountry, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCurrencyExchangeRate, EDIMessageSubTypeList.Descriptions.XmlNativeCurrencyExchangeRate, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeDangerousGood, EDIMessageSubTypeList.Descriptions.XmlNativeDangerousGood, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeDeclaration, EDIMessageSubTypeList.Descriptions.XmlNativeDeclaration, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeOrder, EDIMessageSubTypeList.Descriptions.XmlNativeOrder, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeOrganization, EDIMessageSubTypeList.Descriptions.XmlNativeOrganization, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeProduct, EDIMessageSubTypeList.Descriptions.XmlNativeProduct, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeRate, EDIMessageSubTypeList.Descriptions.XmlNativeRate, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeServiceLevel, EDIMessageSubTypeList.Descriptions.XmlNativeServiceLevel, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeShipment, EDIMessageSubTypeList.Descriptions.XmlNativeShipment, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeStaff, EDIMessageSubTypeList.Descriptions.XmlNativeStaff, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeUNLOCO, EDIMessageSubTypeList.Descriptions.XmlNativeUNLOCO, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeVessel, EDIMessageSubTypeList.Descriptions.XmlNativeVessel, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeWorkflowTemplate, EDIMessageSubTypeList.Descriptions.XmlNativeWorkflowTemplate, defaultDur, defaultUnit)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(NativeDataQuery, new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.XmlNativeAcceptabilityBand, EDIMessageSubTypeList.Descriptions.XmlNativeAcceptabilityBand, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeAddOnRule, EDIMessageSubTypeList.Descriptions.XmlNativeAddOnRule, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeAirline, EDIMessageSubTypeList.Descriptions.XmlNativeAirline, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeBMControlCustomisation, EDIMessageSubTypeList.Descriptions.XmlNativeBMControlCustomisation, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeBMSystem, EDIMessageSubTypeList.Descriptions.XmlNativeBMSystem, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCommodityCode, EDIMessageSubTypeList.Descriptions.XmlNativeCommodityCode, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCommunication, EDIMessageSubTypeList.Descriptions.XmlNativeCommunication, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCompany, EDIMessageSubTypeList.Descriptions.XmlNativeCompany, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeContainer, EDIMessageSubTypeList.Descriptions.XmlNativeContainer, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCountry, EDIMessageSubTypeList.Descriptions.XmlNativeCountry, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCurrencyExchangeRate, EDIMessageSubTypeList.Descriptions.XmlNativeCurrencyExchangeRate, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeCusStatement, EDIMessageSubTypeList.Descriptions.XmlNativeCusStatement, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeDangerousGood, EDIMessageSubTypeList.Descriptions.XmlNativeDangerousGood, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeDeclaration, EDIMessageSubTypeList.Descriptions.XmlNativeDeclaration, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeOrder, EDIMessageSubTypeList.Descriptions.XmlNativeOrder, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeOrganization, EDIMessageSubTypeList.Descriptions.XmlNativeOrganization, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeProduct, EDIMessageSubTypeList.Descriptions.XmlNativeProduct, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeRate, EDIMessageSubTypeList.Descriptions.XmlNativeRate, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeServiceLevel, EDIMessageSubTypeList.Descriptions.XmlNativeServiceLevel, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeShipment, EDIMessageSubTypeList.Descriptions.XmlNativeShipment, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeStaff, EDIMessageSubTypeList.Descriptions.XmlNativeStaff, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeTag, EDIMessageSubTypeList.Descriptions.XmlNativeTag, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeTagRule, EDIMessageSubTypeList.Descriptions.XmlNativeTagRule, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeUNLOCO, EDIMessageSubTypeList.Descriptions.XmlNativeUNLOCO, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeVessel, EDIMessageSubTypeList.Descriptions.XmlNativeVessel, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeWorkflowTemplate, EDIMessageSubTypeList.Descriptions.XmlNativeWorkflowTemplate, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.Unknown, EDIMessageSubTypeList.Descriptions.XmlNativeWorkflowTemplate, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeNewsAnnouncement, EDIMessageSubTypeList.Descriptions.XmlNativeNewsAnnouncement, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeEDICodeMapping, EDIMessageSubTypeList.Descriptions.XmlNativeEDICodeMapping, defaultDur, defaultUnit)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(SYS, new InterchangeObjCollection() { NewInterchangeConfigObj(2, TimeUnit.Week) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(SystemMessageList.Codes.CurrentVersionReport, SystemMessageList.Descriptions.CurrentVersionReport, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.CustomerServiceRequest, SystemMessageList.Descriptions.CustomerServiceRequest, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.CustomerServiceResponse, SystemMessageList.Descriptions.CustomerServiceResponse, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.DeliveredVersionReport, SystemMessageList.Descriptions.DeliveredVersionReport, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.ERequestDocument, SystemMessageList.Descriptions.ERequestDocument, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.LicenceUsageRequest, SystemMessageList.Descriptions.LicenceUsageRequest, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.LinkTrack, SystemMessageList.Descriptions.LinkTrack, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.LogsReport, SystemMessageList.Descriptions.LogsReport, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.LogsRequest, SystemMessageList.Descriptions.LogsRequest, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.ReferenceDataUpdate, SystemMessageList.Descriptions.ReferenceDataUpdate, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.ReferenceDataUpdateResponse, SystemMessageList.Descriptions.ReferenceDataUpdateResponse, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.TranslationFeedbackEntry, SystemMessageList.Descriptions.TranslationFeedbackEntry, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.TranslationFeedbackUpdate, SystemMessageList.Descriptions.TranslationFeedbackUpdate, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.UpgradeDownload, SystemMessageList.Descriptions.UpgradeDownload, 2, TimeUnit.Week)
				.Add(SystemMessageList.Codes.UserAccountReport, SystemMessageList.Descriptions.UserAccountReport, 2, TimeUnit.Week)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(Telematics, new InterchangeObjCollection() { NewInterchangeConfigObj(2, TimeUnit.Week) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(TelematicsMessageList.Codes.TelematicsXmlData, TelematicsMessageList.Descriptions.TelematicsXmlData, 2, TimeUnit.Week)
				.Add(TelematicsMessageList.Codes.ProtobufData, TelematicsMessageList.Descriptions.ProtobufData, 2, TimeUnit.Week)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(UniversalDataMessaging, new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalEvent, EDIMessageSubTypeList.Descriptions.XmlUniversalEvent, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessageSubTypeList.Descriptions.XmlUniversalShipment, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalTransaction, EDIMessageSubTypeList.Descriptions.XmlUniversalTransaction, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch, EDIMessageSubTypeList.Descriptions.XmlUniversalTransactionBatch, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalSchedule, EDIMessageSubTypeList.Descriptions.XmlUniversalSchedule, 1, TimeUnit.Week)
				.Add(EDIMessageSubTypeList.Codes.Unknown, EDIMessageSubTypeList.Descriptions.Unknown, defaultDur, defaultUnit)
				.Add(string.Empty, ApplicationCodeList.Descriptions.UniversalDataMessaging, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalDocumentRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeContainer, EDIMessageSubTypeList.Descriptions.XmlNativeContainer, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlNativeShipment, EDIMessageSubTypeList.Descriptions.XmlNativeShipment, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalShipmentRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalActivity, EDIMessageSubTypeList.Descriptions.XmlUniversalActivity, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.Undefined, EDIMessageSubTypeList.Descriptions.Undefined, defaultDur, defaultUnit)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(UniversalDataQuery, new InterchangeObjCollection() { NewInterchangeConfigObj(defaultDur, defaultUnit) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalActivity, EDIMessageSubTypeList.Descriptions.XmlUniversalActivity, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalActivityRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalDocumentRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalEvent, EDIMessageSubTypeList.Descriptions.XmlUniversalEvent, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalInterchangeRequeueRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalInterchangeRequeueRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalResponse, EDIMessageSubTypeList.Descriptions.XmlUniversalResponse, 1, TimeUnit.Week)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalSchedule, EDIMessageSubTypeList.Descriptions.XmlUniversalSchedule, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessageSubTypeList.Descriptions.XmlUniversalShipment, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalShipmentRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalTransaction, EDIMessageSubTypeList.Descriptions.XmlUniversalTransaction, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch, EDIMessageSubTypeList.Descriptions.XmlUniversalTransactionBatch, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest, EDIMessageSubTypeList.Descriptions.XmlUniversalTransactionBatchRequest, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.Unknown, EDIMessageSubTypeList.Descriptions.Unknown, defaultDur, defaultUnit)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(UsageData, new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(string.Empty, EDIMessageTypeList.Descriptions.UsageData, 1, TimeUnit.Month)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(UsageDataToSummarise, new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(string.Empty, EDIMessageTypeList.Descriptions.UsageDataToSummarise, 1, TimeUnit.Month)
			);

			yield return AddApplicationCodeMessageSubTypePurgeType(XMS, new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
				.Add(EDIMessageSubTypeList.Codes.AgencyBillsOfLading, EDIMessageSubTypeList.Descriptions.AgencyBillsOfLading, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Consols, EDIMessageSubTypeList.Descriptions.Consols, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.ContainerMovements, EDIMessageSubTypeList.Descriptions.ContainerMovements, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Events, EDIMessageSubTypeList.Descriptions.Events, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.FinancialTransactions, EDIMessageSubTypeList.Descriptions.FinancialTransactions, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Orders, EDIMessageSubTypeList.Descriptions.Orders, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Products, EDIMessageSubTypeList.Descriptions.Products, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Shipments, EDIMessageSubTypeList.Descriptions.Shipments, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.WhsDockets, EDIMessageSubTypeList.Descriptions.WhsDockets, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Brokerage, EDIMessageSubTypeList.Descriptions.Brokerage, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Invoices, EDIMessageSubTypeList.Descriptions.Invoices, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.LocalCartageBooking, EDIMessageSubTypeList.Descriptions.LocalCartageBooking, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.LocalCartageStatus, EDIMessageSubTypeList.Descriptions.LocalCartageStatus, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.CFSLoadList, EDIMessageSubTypeList.Descriptions.CFSLoadList, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.DocumentMessages, EDIMessageSubTypeList.Descriptions.DocumentMessages, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.NettingClearingJournals, EDIMessageSubTypeList.Descriptions.NettingClearingJournals, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Organizations, EDIMessageSubTypeList.Descriptions.Organizations, 12, TimeUnit.Month)
				.Add(EDIMessageSubTypeList.Codes.Unknown, EDIMessageSubTypeList.Descriptions.Unknown, 12, TimeUnit.Month)
				.Add(string.Empty, ApplicationCodeList.Descriptions.XMS, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.BankStatements, EDIMessageSubTypeList.Descriptions.BankStatements, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.Rates, EDIMessageSubTypeList.Descriptions.Rates, defaultDur, defaultUnit)
				.Add(EDIMessageSubTypeList.Codes.XmlUniversalShipment, EDIMessageSubTypeList.Descriptions.XmlUniversalShipment, defaultDur, defaultUnit)
			);
		}
	}
}
