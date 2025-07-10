using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public static class Constants
	{
		public static class AsycudaAdditionalInfoCodes
		{
			public const string PartnerVatNumber = "2";
			public const string UNLOCO = "3";
			public const string ExporterTypeID = "5";
			public const string NightStop = "9";
			public const string IsCooling = "11";
			public const string MultipleDeals = "15";
			public const string CargoType = "16";
			public const string PackageFeeType = "18";
			public const string ActionCode = "12";
			public const string IsDirectDelivery = "21";
		}

		public static class AsycudaAdditionalInfoDescriptions
		{
			public const string NoPacks = "2";
			public const string Containerized = "3";
			public const string Pallet = "4";
			public const string Bulk = "5";
		}

		public static class AdditionalInformation
		{
			public const string ManifestStatementTypeCode = "1";
			public const string RoadStatementTypeCodeContainerizedCargo = "16";
			public const string RoadStatementCodeContainerizedCargo = "1";
		}

		public static class IsraeliCustoms
		{
			public const string ManifestTransportContractDocumentId = "IL2";
			public const string DangerousGoodsPackingHighDangerCode = "47";
			public const string DangerousGoodsPackingMediumDangerCode = "48";
			public const string DangerousGoodsPackingLowDangerCode = "49";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static class AsycudaTransportDocumentInfoValidation
		{
			public const string RuleCC_BR1_WCO_091_IL1IL2_Regex = "^[I](\\w{9})$";
			public const string RuleCC_BR1_WCO_091_IL3_Regex = "^[I](\\w{15})$";
		}

		public static class MessagesWrappers
		{
			public static class DeclarationConsignmentConsignmentItemTransportEquipment
			{
				public const string FullnessCode4 = "4";
				public const string FullnessCode5 = "5";
			}

			public static class CommunicationType
			{
				public const string Phone = "TE";
				public const string Email = "EM";
			}
		}

		public static class ILDatabaseValidationHelper
		{
#pragma warning disable CW1161 // Res.GetString Analyzer
			public static string Seal => "Seal";
#pragma warning restore CW1161 // Res.GetString Analyzer

			public static string SealIsRequired => Res.GetString("ILDatabaseValidationHelper|Seal", "A Seal is required");
		}

		public static class AsycudaLinkPackageValidation
		{
			public static string SelectedPackageIsRequired => Res.GetString("FA5CB01E-FA60-44FF-B260-6A6FE00898EE", "A selected package is required.");
			public static string MoreThanOneSelectedPackageIsProhibited => Res.GetString("F1E18FD8-DCD1-48A7-B6CC-E330F60FCCE0", "Only one package can be linked to a Bill Item.");
		}

		public static class AsycudaAdditionalInfoValidation
		{
			public static string StatementCodeOrContentMustBeProvided => Res.GetString("CDA188DB-6C40-45E4-963C-9DD74E1C4E79", "Statement Code or Content must be provided.");
		}

		public static class AsycudaBill
		{
			public static class PackageTypes
			{
				public const string PX = "PX";
				public const string NE = "NE";
			}
		}

		public static class Message1171Processor
		{
			public static MultilingualString ManifestNotFound => ResString.GetMultilingualString("3FF41581-51E6-476F-A3C9-DD6DD9EE330F", "Couldn't locate Job using provided Manifest #");
			public static MultilingualString GetRequestedSupportingDocumentNotFound(decimal billSequenceNumber) => ResString.GetMultilingualString("C9433E5A-34E4-45DB-85F9-0E8D28F24E9A", "Couldn't locate Bill with Sequence Number {0}", billSequenceNumber);
		}

		public static class Message2716Processor
		{
			public static MultilingualString GetSupportingDocumentNotFound(string documentId) => ResString.GetMultilingualString("A9ACBD77-70BA-4147-9E34-0D2DDF67F002", "Couldn't locate document with reference {0}", documentId);
		}

		public static class Message828Processor
		{
			public static MultilingualString DocumentIDIsNull => ResString.GetMultilingualString("DCF8D833-1ED7-4952-AB0E-9A4D2547C92E", "Couldn't find Document ID from the message");
			public static MultilingualString GetSupportingDocumentNotFound(string documentId) => ResString.GetMultilingualString("384CFF95-2ED1-458C-8157-62BD048259FB", "Couldn't locate document with reference {0}", documentId);
		}

		public static class Message8241Processor
		{
			public static MultilingualString GetManifestNotFound(string suffix) => ResString.GetMultilingualString("228DCAC4-5DBE-4BA0-81A2-DDF2C23FC5F0", "Couldn't locate Job using provided Manifest # {0}", suffix);
		}

		public static class AsycudaSupportingDocument
		{
			public static class StatusCode
			{
				public const string Verified = "2";
				public const string VerifiedWithClient = "3";
				public const string Reject = "4";
				public const string AutoVerified = "5";
			}
		}
	}
}
