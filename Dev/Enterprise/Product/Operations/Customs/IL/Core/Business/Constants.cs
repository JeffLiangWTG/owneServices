using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public static class Constants
	{
		public static class CustomsDeclaration
		{
			public const string ClassificationIdentificationTypeCodeRegular = "HS";
			public const string ClassificationIdentificationTypeCodeDangerous = "SSO";
			public const string DmExtensionsCustomsBookTypeImport = "1";
			public const string MeasureQualifierInvoiceQuantity = "1";
			public const string MeasureQualifierStatisticQty = "2";
			public const string MeasureQualifierAdditionalQty = "3";
			public const string ImporterSchemeID = "1";
		}

		public static class CusEntryInstruction
		{
			public const string PreviousDocumentCaption = "CusEntryInstruction.PreviousDocumentCaption";
		}

		public static class CustomsFeedBackMessageMainTagName
		{
			public const string InfMsgGenericResponse = "INF_MSG_Generic";
			public const string Manifest = "MN_MSG4_SendManifestFeedBack_Message";
			public const string DeliveryOrder = "MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message";
			public const string GatePassMovement = "GP_NG_1035_MSG2_GatepassFeedbackMessage";
			public const string ImportDeclarationResponse = "DF_NG_2754_MSG10004_ImportDeclarationResponse";
			public const string OutgoingMessageResponse = "NG_9101_MSG_OutgoingMessageResponse";
			public const string DocumentMessageResponse = "D_NG_2716_MSG22001_AddAttachmentResponse";
			public const string ManifestQueryResponse = "MN_NG_8241_Cargo_Message";
			public const string DocumentRqDecisionMessageResponse = "VAL_NG_8228_MSG550_RequiredDocumentVerificationDecisionMessage";
		}

		public static class CustomsDeliveryOrder
		{
			public const int ActionTypeCodeCancel = 1;
			public const int ActionTypeCodeNew = 2;
			public const int CargoIdentifierTypeSea = 11;
			public const int CargoIdentifierTypeRoad = 20;
			public const int EndorsementCode = 1;
			public const int ProducerTypeCode = 5;
		}

		public static class GatePassMovement
		{
			public static class ActionTypeCode
			{
				public const int New = 1;
				public const int Cancel = 2;
			}
			public static class ActivityType
			{
				public const int Forwarder = 5;
				public const int Broker = 3;
			}

			public static class MessageReturnCode
			{
				public const int WithdrawCancelAccepted = 5;
				public const int RejectedByCustoms = 6;
				public const int RejectedByOriginSite = 7;
			}

			public static class MessageResponseStatus
			{
				public const int Rejected = 1;
				public const int Accepted = 2;
			}
		}

		public static class GlbILExternalPassword
		{
			public const string ConfigurationName = "ILClientCertificate";
			public const string InterchangeTypeForSending = "ILC";
		}

		public static class WCOPointer
		{
			public const string Header = "42A";
			public const string Consignment = "28A";
		}

		public static class SupportingDocument
		{
			public const string CustomValidationCode = "2570";
		}

		public static class TransportDocument
		{
			public const string UserOverrideStatusCode = "OVR";
		}

		public static class JobDeclaration
		{
			public static string ManifestNumberSeaDigitsOnly => Res.GetString("48024D99-400C-4C38-BB59-793182849DC0", "Manifest Number should contain only digits");
			public static string ManifestNumberSeaMaxLength => Res.GetString("452AFC28-7149-4C96-9D5B-D2F8F182FA0F", "Manifest Number should not exceed 6 digits");
			public static string ManifestNumberRoaMaxLength => Res.GetString("0FCF885A-C459-4D07-9270-BD2A5B381879", "Manifest Number should not exceed 15 characters");
			public const string BaseAmount = "1";
		}

		public static class CargoIdentifierType
		{
			public const string AirBillOfLadingImport = "1";
			public const string SeaDealImport = "11";
			public const string LandDealImport = "20";
			public const string AirBillOfLadingExport = "16";
			public const string PortExportDeliveryDocument = "13";
			public const string LandExportDeliveryDocument = "30";
		}

		public static class CustomsBoolean
		{
			public static string True => Res.GetString("A5F6345A-AE40-4A36-9966-2238C2CA3799", "True");
			public static string False => Res.GetString("B2F1B084-FCD4-4509-A662-12661D99992A", "False");
		}

		public static class CustomsServiceName
		{
			public const string CurrencyRateRequest = "GetCD_8347_8348_Web01_02_CurrencyRateSearch";
			public const string DeliveryOrderRequest = "SaveMN_MSG1200_1220_DeliveryOrder_Message";
			public const string ForwarderManifestRequest = "SaveMN_MSG1170_1171_MANIFESTRequest";
			public const string GatePassMovementRequest = "GetGP_MSG1030_1035_GatepassFeedbackMessage";
			public const string ImportDeclarationRequest = "SaveDF_MSG2750_2754_ImportDeclarationRequest";
			public const string Sync9100OutgoingMessageRequest = "Get_9100_OutgoingMessageRequest";
			public const string Sync9200OutgoingMessageRequest = "GET_9200_OutgoingMessageDeliveryApproval";
			public const string SystemTableRequest = "GetSYSTBL_MSG9000_9001_SystemTableRequest";
			public const string SupportingDocumentsRequest = "GetDOC_MSG2715_2716_AddAttachmentResponse";
			public const string DeliveryOrderResponse = "SendMN_MSG1220_DeliveryOrderFeedBack_Message";
			public const string GatePassMovementResponse = "SendGP_MSG1035_GatepassFeedbackMessage";
			public const string ImportDeclarationResponse = "SendMN_MSG1171_SendManifestFeedBack_Message";
			public const string ManifestQueryRequest = "GetMN_MSG_8240_8241_CargoQuery_Message";
			public const string ExportDeclarationRequest = "SaveDF_MSG2751_ExportDeclaration";
			public const string RequiredDocumentResponse = "SendVAL_MSG8227_RequiredDocumentMessage";
			public const string RequiredDocumentVerificationDecisionResponse = "SendVAL_MSG8228_RequiredDocumentVerificationDecisionMessage";
		}

		public static class CustomsRequestHeader
		{
			public const string SoftwareProvider = "WTG";
			public const string SoftwareVersion = "1.0";
			public const string WSDLVersion = "1.0";
		}

		public static class CustomsResponseHeader
		{
			public const string ExceptionApplicationCode = "0";
		}

		public static class Unpacker
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			public const string ResponseBody = "Body";
			public const string ResponseHeader = "Header";
			public const string CorrelationId = "CorrelationId";
			public const string Status = "Status";
			public const string StatusSuccess = "Success";
			public const string ResponseHeaderInner = "ResponseHeader";
#pragma warning restore CW1161 // Res.GetString Analyzer
			public static string TheInterchangeBodyTextIsEmpty => Res.GetString("F83C19B2-E796-4F8E-8F7B-B69902EDB54A", "The interchange body text is empty.");
			public static string TheInterchangeBodyTextIsNotValidXML => Res.GetString("CEEFCFFD-72E0-4256-A395-A07EE728D194", "The interchange body text is not valid XML.");
			public static string TheInterchangeBodyTextDoesNotContainValidFeedbackMessageName => Res.GetString("F2C48778-D7CF-4261-A670-569E3AB7B9EB", "The interchange body text does not contain a valid feedback message name. Extracted value:");
			public static string TheStatusIsNotSuccess => Res.GetString("464626B6-9D45-4728-9015-97DFE313AB58", "The status of the message is not success.");
			public static string TheInterchangeTypeIncorrect => Res.GetString("2BF1F83A-91C1-4830-8A0E-2AD6B70BF539", "The type of interchange is incorrect.");
		}

		public static class DCAParametersValidation
		{
			public static string MaxMessagesPerIterationMinValue => Res.GetString("2F68C72C-27E8-4E1C-A2C2-2D1B3EEF4E6F", "The maximum messages per iteration must be greater than or equal to {0}.", DCAParameters.Schema.MaxMessagesPerIterationMinValue);
			public static string MaxMessagesPerIterationMaxValue => Res.GetString("9ADA7791-0F87-401C-88E7-D0E518309864", "The maximum messages per iteration must be less than or equal to {0}.", DCAParameters.Schema.MaxMessagesPerIterationMaxValue);
		}

		public static class ILMessageEventParameter
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			public const string Department = "Customs";
			public const string DeliveryOrderDocumentName = "Delivery Order";
			public const string GatePassMovementDocumentName = "Gatepass Movement";
			public const string GatePassResponseRejectedByCustomsReason = "Rejected By Customs";
			public const string GatePassResponseRejectedByOriginSiteReason = "Rejected By Origin Site";
#pragma warning restore CW1161 // Res.GetString Analyzer
		}

		public static class GlbILExternalPasswordLookups
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			public const string PasswordAwa = "AWA";
			public const string PasswordReg = "REG";
#pragma warning restore CW1161 // Res.GetString Analyzer
			public static MultilingualString PasswordAwaiting => ResString.GetMultilingualString("A54E5103-D9E6-4C9D-8DD0-677AC016ACD8", "Awaiting Response");
			public static MultilingualString PasswordRegistered => ResString.GetMultilingualString("5CA670C5-4253-46C2-99A6-B542394E42A2", "Registered");
		}

		public static class ILEDIMessagePurgeSettingsConfig
		{
			public const int ILCPurgeDuration = 7;
		}

		public static class CustomsDocumentRelatedEntity
		{
			public const string ManifestPath = "11";
			public const string ManifestType = "11152";
		}

		public static class MethodOfCalculationTypes
		{
			public const string Percentage = "%";
		}

		public static class EntryLineFee
		{
			public const string VATFeeTypeCode = "15";
			public const string VATFeeType = "VAT";
		}

		public static class MessageProcessors
		{
			public static MultilingualString UnexpectedMessage => ResString.GetMultilingualString("E937C94F-B7B4-4DDA-8805-6E3FFC4B3D80", "The message does not belong to Israel customs.");
			public static MultilingualString CouldNotLocateByOriginalSentMessageMessage => ResString.GetMultilingualString("1E0B23E2-3499-4242-B47C-FBF175C4B09A", "Could not locate Shipment by original Sent message.");
			public static MultilingualString ApplicationIDIsExceptionMessage => ResString.GetMultilingualString("8A0E3AFB-59BF-47DB-8492-9E2CB1CC05EA", "The application Id of response message is 0.");
		}

		public static class Message274Processor
		{
			public static MultilingualString GetCouldNotLocateEntryWithReference(string externalDeclarationID) => ResString.GetMultilingualString("FC0AFA6D-F216-432C-97C0-CC32D2B10B98", "Could not locate entry with reference {0}", externalDeclarationID);
			public static MultilingualString EntryLineFeeKeysDontMatch => ResString.GetMultilingualString("3D636861-6576-43FE-92E3-8281548C5C18", "Customs feedback line numbers don't match the entry header line numbers");
			public static MultilingualString MessageInvalid => ResString.GetMultilingualString("EFEF2E68-D6B6-4CB0-BE9E-281969ED495D", "The incoming message is invalid");
		}

		public static class MessageSignatureProperty
		{
			public const string SignatureType = "custom.IL.SignatureType";
			public const string SignatureUser = "custom.IL.SignatureUser";
			public const string SignaturePIN = "custom.IL.SignaturePIN";
		}

		public static class MessageSignatureType
		{
			public const string CompanySignature = "C";
			public const string PersonalSignature = "P";
			public const string NoneSignatureType = "N";
		}
	}
}
