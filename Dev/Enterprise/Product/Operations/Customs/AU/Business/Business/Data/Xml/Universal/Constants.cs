namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class Constants
	{
		public static class RFPAttachment
		{
			public static class Keys
			{
				public const string AttachmentFileName = "FileName";
				public const string AttachmentMimeType = "mimeType";
				public const string AttachmentType = "AttachmentType";
				public const string AttachmentDescription = "Description";
			}
		}

		public static class Declaration
		{
			public static class Keys
			{
				public const string UseNEXDOCStaging = "UseNEXDOCStaging";
			}

			public static class Codes
			{
				public const string Y = "Y";
			}
		}

		public static class InvoiceHeader
		{
			public static class Keys
			{
				public const string ProduceType = "ProduceType";
				public const string ProductUse = "ProductUse";
				public const string ObtainExportCustomsPermit = "ObtainExportCustomsPermit";
				public const string ConsigneeAgentName = "ConsigneeAgentName";
				public const string ExporterDeclaration = "ExporterDeclaration";
				public const string CertificatePrintIndicator = "CertificatePrintIndicator";
				public const string ProductionRegion = "ProductionRegion";
				public const string SplitHealthCertByContainer = "SplitHealthCertByContainer";
				public const string SplitHealthCertByPacker = "SplitHealthCertByPacker";
				public const string SplitHealthCertByMarks = "SplitHealthCertByMarks";
				public const string AMLCQuota = "AMLCQuota";
				public const string QuotaType = "QuotaType";
				public const string ShipsStores = "ShipsStores";
				public const string AMLCQuotaYear = "AMLCQuotaYear";
				public const string CertificateRequiredLocation = "CertificateRequiredLocation";
				public const string ProductSource = "ProductSource";
				public const string BorderInspectionPort = "BorderInspectionPort";
				public const string PackDate = "PackDate";
				public const string AbsoluteTemperature = "AbsoluteTemperature";
				public const string MinimumTemperature = "MinimumTemperature";
				public const string MaximumTemperature = "MaximumTemperature";
				public const string TemperatureUnit = "TemperatureUnit";
				public const string RecommendationLetterNumber = "RecommendationLetterNumber";
				public const string RecommendationLetterDate = "RecommendationLetterDate";
				public const string DeclarationOfCompliance = "DeclarationOfCompliance";
				public const string ImportedProductFlag = "ImportedProductFlag";
				public const string TrueAndComplete = "TrueAndComplete";
				public const string ManufacturedTreatedPackagedLabelledInAustralia = "ManufacturedTreatedPackagedLabelledInAustralia";
				public const string LegallyImportedFlag = "LegallyImportedFlag";
				public const string CustomsConsigneeName = "CustomsConsigneeName";
				public const string ExemptionCode = "ExemptionCode";
				public const string AuthorisationDate = "AuthorisationDate";
				public const string AuthorisationComments = "AuthorisationComments";
				public const string AuthorisationEstablishment = "AuthorisationEstablishment";
				public const string AuthorisationFlag = "AuthorisationFlag";
				public const string StorageEstablishment = "StorageEstablishment";
				public const string ApprovedCertifier = "ApprovedCertifier";
				public const string LotNumber = "LotNumber";
				public const string CatchZone = "CatchZone";
				public const string AverageAnimalAge = "AverageAnimalAge";
				public const string StartHoldSeal = "StartHoldSeal";
				public const string EndHoldSeal = "EndHoldSeal";
				public const string InspectionRequestedDate = "InspectionRequestedDate";
				public const string AuthorisedStartDate = "AuthorisedStartDate";
				public const string AuthorisedEndDate = "AuthorisedEndDate";
				public const string AuthorisingOfficerID = "AuthorisingOfficerID";
				public const string InspectorComments = "InspectorComments";
				public const string ForwardeeEDIUserIdentifier = "ForwardeeEDIUserIdentifier";
				public const string ForwardStatus = "ForwardStatus";
				public const string TransfereeEDIUserIdentifier = "TransfereeEDIUserIdentifier";
				public const string TransfereeExporterNumber = "TransfereeExporterNumber";
				public const string CancelTransferIndicator = "CancelTransferIndicator";
				public const string Compartments = "Compartments";
				public const string InspectionPort = "InspectionPort";
				public const string InspectionDate = "InspectionDate";
				public const string ApprovalNumber = "ApprovalNumber";
				public const string TransitLocationType = "TransitLocationType";
				public const string EUComments = "EUComments";
				public const string EUTestResultRequired = "EUTestResultRequired";
				public const string PrintLocation = "PrintLocation";
				public const string LastAmendDateTime = "LastAmendDateTime";
				public const string SubmitAmendmentRequest = "SubmitAmendmentRequest";
				public const string RequestAmendReason = "RequestAmendReason";
				public const string ReissueCertificateName = "ReissueCertificateName";
				public const string ReissueCertificateReason = "ReissueCertificateReason";
				public const string LoadingEstablishment = "LoadingEstablishment";
				public const string LoadingDate = "LoadingDate";
			}

			public static class Codes
			{
				public const string QHA = "QHA";
				public const string QH = "QH";
				public const string NRL = "NRL";
				public const string NSI = "NSI";
			}

			public static class Descriptions
			{
				public const string QHA = "Quarantine Header Attachment";
				public const string QH = "Quarantine ExDoc Header";
				public const string NRL = "Recommendation Letter";
				public const string NSI = "Ships Compartment Inspections";
			}
		}

		public static class InvoiceLine
		{
			public static class Keys
			{
				public const string NetQuantity = "NetQuantity";
				public const string NetQuantityUnit = "NetQuantityUnit";
				public const string ImperialNetWeight = "ImperialNetWeight";
				public const string ImperialNetWeightUnit = "ImperialNetWeightUnit";
				public const string GrossMetricWeight = "GrossMetricWeight";
				public const string GrossMetricWeightUnit = "GrossMetricWeightUnit";
				public const string ShippingMarks = "ShippingMarks";
				public const string BatchCode = "BatchCode";
				public const string OuterPackCount = "OuterPackCount";
				public const string OuterPackType = "OuterPackType";
				public const string OuterPackAccuracy = "OuterPackAccuracy";
				public const string OuterPackWeight = "OuterPackWeight";
				public const string OuterPackWeightUnit = "OuterPackWeightUnit";
				public const string IntermediatePackCount = "IntermediatePackCount";
				public const string IntermediatePackType = "IntermediatePackType";
				public const string IntermediatePackAccuracy = "IntermediatePackAccuracy";
				public const string IntermediatePackWeight = "IntermediatePackWeight";
				public const string IntermediatePackWeightUnit = "IntermediatePackWeightUnit";
				public const string InnerPackCount = "InnerPackCount";
				public const string InnerPackType = "InnerPackType";
				public const string InnerPackAccuracy = "InnerPackAccuracy";
				public const string InnerPackWeight = "InnerPackWeight";
				public const string InnerPackWeightUnit = "InnerPackWeightUnit";
				public const string ProductType = "ProductType";
				public const string Category = "Category";
				public const string SupplementaryCode = "SupplementaryCode";
				public const string PackType = "PackType";
				public const string PreservationType = "PreservationType";
				public const string CutCode = "CutCode";
				public const string ProductDescriptionLocationQualifier = "ProductDescriptionLocationQualifier";
				public const string ProductDescriptionQualityQualifier = "ProductDescriptionQualityQualifier";
				public const string NatureOfCommodity = "NatureOfCommodity";
				public const string TreatmentType = "TreatmentType";
				public const string AdditionalDeclarationComments = "AdditionalDeclarationComments";
				public const string RelatedExportPermitNumber = "RelatedExportPermitNumber";
				public const string RelatedExportPermitAuthority = "RelatedExportPermitAuthority";
				public const string RelatedExportPermitDate = "RelatedExportPermitDate";
				public const string ClientLineItemID = "ClientLineItemID";
				public const string ProcessingType = "ProcessingType";
				public const string EstablishmentID = "EstablishmentID";
				public const string StartDate = "StartDate";
				public const string EndDate = "EndDate";
				public const string DepurationDate = "DepurationDate";
				public const string HarvestArea = "HarvestArea";
				public const string InspectionRequestedDate = "InspectionRequestedDate";
				public const string LeaseNumber = "LeaseNumber";
				public const string TreatmentCode = "TreatmentCode";
				public const string TreatmentInformation = "TreatmentInformation";
				public const string UseByStart = "UseByStart";
				public const string UseByEnd = "UseByEnd";
				public const string InspectionDescription = "InspectionDescription";
				public const string AdditionalProductDescription = "AdditionalProductDescription";
				public const string CommercialProductDescription = "CommercialProductDescription";
				public const string HealthCertificateDescription = "HealthCertificateDescription";
				public const string FormatRequested = "FormatRequested";
				public const string ExtraFormatRequested = "ExtraFormatRequested";
				public const string FormatAllocated = "FormatAllocated";
				public const string CertificateNumber = "CertificateNumber";
				public const string DrainedWeight = "DrainedWeight";
				public const string DrainedWeightUnit = "DrainedWeightUnit";
				public const string FishWaterIndicator = "FishWaterIndicator";
				public const string CatchStartDate = "CatchStartDate";
				public const string CatchEndDate = "CatchEndDate";
				public const string PercentOfMilkProtein = "PercentOfMilkProtein";
				public const string TotalWeightOfMilkProteinInMixtures = "TotalWeightOfMilkProteinInMixtures";
				public const string PercentOfMilkFat = "PercentOfMilkFat";
				public const string TotalWeightOfMilkFatInMixtures = "TotalWeightOfMilkFatInMixtures";
				public const string IMA1SerialNumber = "IMA1SerialNumber";
				public const string IMA1QuotaYear = "IMA1QuotaYear";
				public const string IMA1ProductDescription = "IMA1ProductDescription";
				public const string GrowerNumber = "GrowerNumber";
				public const string SaltingDate = "SaltingDate";
				public const string ChemicalLeanPercentage = "ChemicalLeanPercentage";
				public const string BeefVealWeightAmount = "BeefVealWeightAmount";
				public const string LabelApprovalNumber = "LabelApprovalNumber";
				public const string LabelApprovalIndicator = "LabelApprovalIndicator";
				public const string UngradedProductIndicator = "UngradedProductIndicator";
				public const string StatementNumber1 = "StatementNumber1";
				public const string StatementNumber2 = "StatementNumber2";
				public const string StatementNumber3 = "StatementNumber3";
				public const string StatementNumber4 = "StatementNumber4";
				public const string StatementNumber5 = "StatementNumber5";
				public const string StatementText = "StatementText";
				public const string EstablishmentIndicator = "EstablishmentIndicator";
				public const string RemoveEntry = "RemoveEntry";
				public const string AHECCCode = "AHECCC";
				public const string FinalConsumer = "FinalConsumer";
				public const string CombinedNomenclature = "CombinedNomenclature";
				public const string FarmCode = "FarmCode";
				public const string FarmType = "FarmType";
			}

			public static class Codes
			{
				public const string QLA = "QLA";
				public const string QL = "QL";
				public const string NPD = "NPD";
			}
			public static class Descriptions
			{
				public const string QLA = "Quarantine Line Attachment";
				public const string QL = "Quarantine ExDoc Line";
				public const string NPD = "Processing Details";
			}
		}

		public static class BankAccountOwnerType
		{
			public const string Broker = "B";
			public const string Importer = "I";
			public const string Other = "O";
			public const string DrawbackClaimant = "D";
		}

		public static class EventContextTypes
		{
			public const string ConsigneeCity = "ConsigneeCity";
			public const string ConsigneeCountry = "ConsigneeCountry";
			public const string ConsigneeName = "ConsigneeName";
			public const string ConsigneePhoneNumber = "ConsigneePhoneNumber";
			public const string ConsigneePhoneNumberType = "ConsigneePhoneNumberType";
			public const string ConsigneePostalCode = "ConsigneePostalCode";
			public const string ConsigneeRepresentative = "ConsigneeRepresentative";
			public const string ConsigneeState = "ConsigneeState";
			public const string ConsigneeStreetAddress1 = "ConsigneeStreetAddress1";
			public const string ConsigneeStreetAddress2 = "ConsigneeStreetAddress2";
			public const string DepartureDate = "DepartureDate";
			public const string ExporterReference = "ExporterReference";
			public const string HealthCertificateDescription = "HealthCertificateDescription";
			public const string LastAmendDateTime = Constants.InvoiceHeader.Keys.LastAmendDateTime;
			public const string LineNumber = "LineNumber";
			public const string Message = "Message";
			public const string Notice = "Notice";
			public const string NoticeID = "NoticeID";
			public const string NoticeMessage = "NoticeMessage";
			public const string NoticeType = "NoticeType";
			public const string PermitNumber = "PermitNumber";
			public const string PrimaryCertificateEndorsementNumber = "PrimaryCertificateEndorsementNumber";
			public const string PrimaryCertificateTemplateCode = "PrimaryCertificateTemplateCode";
			public const string SecondaryCertificateEndorsementNumber = "SecondaryCertificateEndorsementNumber";
			public const string SecondaryCertificateTemplateCode = "SecondaryCertificateTemplateCode";
			public const string ValidationNotice = "ValidationNotice";
		}

		public static class EventReference
		{
			public const string ExportDeclarationNumber = "EDN";
			public const string Notify = "MST=NOTIF";
			public const string CanRex = "MST=CANREX";
			public const string Reissue = "MST=REISSUE";
			public const string Replace = "MST=REPLACE";
		}

		public static class MessageStatus
		{
			public const string Error = "ERO";
		}

		public static class DataProvider
		{
			public const string NEXDOCS = "NEXDOCS";
			public const string NEXDOCSTest = "NEXDOCSTest";
		}

		public static class ActionPurpose
		{
			public const string ADD = "ADD";
		}

		public static class MessageType
		{
			public const string NEXDOCS = "NEX";
		}

		public static class CMRConsolStatus
		{
			public const string NotSent = "";
			public const string Clear = "CLR";
			public const string Revoked = "REV";
			public const string Rejected = "REJ";
			public const string Errors = "ERR";
			public const string Withdrawn = "WDW";
			public const string WaitingForResponse = "WAI";
		}

		public static class DutyCalcTypes
		{
			public const string Free = "FREE";
			public const string Info = "INFO";
			public const string Calc = "CALC";
			public const string InCalc = "INCALC";
			public const string Lower = "LOWER";
			public const string Higher = "HIGHER";
		}

		public static class HoldPrealertReference
		{
			public const string IsPrealertHeld = "Pre-alert held";
			public const string IsPrealertHeldReleased = "Pre-alert holding released";
		}

		public const string HVLVConsignment = "HVLVConsignment";
	}
}
