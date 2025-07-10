using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Forwarding.GUI
{
	public static class ForwardingShipmentCustomsColumnConstants
	{
		public static class Schema
		{
			public const string CustomsCargoStatus = "CustomsCargoStatus";
			public const string CustomsMessageStatus = "CustomsMessageStatus";

			public const string CRLStatus = "CRLStatus";
			public const string SEBillStatus = "SEBillStatus";
			public const string HLDOrEXMStatus = "HLDOrEXMStatus";
			public const string ENSStatus = "ENSStatus";
			public const string EXPStatus = "EXPStatus";
			public const string ITStatus = "ITStatus";

			public const string ISFBillNumber = "ISFBillNumber";
			public const string ISFBillStatus = "ISFBillStatus";
			public const string ISFBillStatusDescription = "ISFBillStatusDescription";

			public const string AFRBillStatus = "AFRBillStatus";
			public const string AFRBillStatusDescription = "AFRBillStatusDescription";

			public const string EntryStatusDescription = "EntryStatusDescription";

			public const string ACICargoStatus = "ACICargoStatus";
			public const string ACIMessageStatus = "ACIMessageStatus";

			public const string EManifestCargoStatus = "EManifestCargoStatus";
			public const string EManifestMessageStatus = "EManifestMessageStatus";

			public const string CustomsEntryType = "CustomsEntryType";
			public const string ITEntryType = "ITEntryType";

			public const string CustomsEntryAuthorisationDate = "CustomsEntryAuthorisationDate";

			public const string DestinationGoodsValue = "DestinationGoodsValue";
			public const string DestinationCurrencyCode = "DestinationCurrencyCode";
			public const string DestinationExchangeRate = "DestinationExchangeRate";
		}

		public static class Captions
		{
			public static MultilingualString CustomsCargoStatusDescription
			{
				get
				{
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedKingdom)
					{
						return ResString.GetMultilingualString("9d0498a4-d747-4755-8ba6-6e80b9067dda", "CCS-UK Status");
					}

					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						return ResString.GetMultilingualString("a9dc22bc-11aa-48d8-9175-da3989f5a12a", "CRL Status Desc.");
					}

					return ResString.GetMultilingualString("1fd9c971-d0e0-44eb-8003-106724f02907", "Customs Cargo Status");
				}
			}

			public static MultilingualString CustomsMessageStatusDescription
			{
				get
				{
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						return ResString.GetMultilingualString("71b80061-7368-4a00-9873-d156586eada6", "ENS Status Desc.");
					}

					return ResString.GetMultilingualString("2446dc85-4001-439e-8588-cf338a44004e", "Customs Message Status");
				}
			}

			public static MultilingualString CRLStatusDescription => ResString.GetMultilingualString("a58d0582-b812-45c7-854b-35012dbe817f", "CRL Status");

			public static MultilingualString SEBillStatusDescription
			{
				get
				{
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						return ResString.GetMultilingualString("94CE34C1-B862-4C87-AA88-2C4749A79811", "SE Bill Status Desc.");
					}

					return ResString.GetMultilingualString("D563D92E-74D0-480D-9B60-6686FCBDD486", "SE Bill Status");
				}
			}

			public static MultilingualString HLDOrEXMStatusDescription
			{
				get
				{
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						return ResString.GetMultilingualString("B3AB4D51-8EB2-42F0-9320-44F682734B81", "Bill Hold/Exam");
					}

					return ResString.GetMultilingualString("51487844-e00a-465b-a5a7-0fba5ea36526", "SE Bill Status");
				}
			}

			public static MultilingualString EManifestCargoStatusDescription => ResString.GetMultilingualString("a648ed87-bf05-496b-a1be-9826fba57cb6", "eManifest Cargo Status");

			public static MultilingualString EManifestMessageStatusDescription => ResString.GetMultilingualString("c5b37376-74d9-4b9c-9978-927fb39181f8", "eManifest Msg. Status", "eManifest Message Status");

			public static MultilingualString ENSStatusDescription => ResString.GetMultilingualString("c1e95a1d-24b4-4241-b842-2c76c251a18e", "ENS Status");

			public static MultilingualString EXPStatusDescription => ResString.GetMultilingualString("0a0fe722-2980-4ef8-b8cb-7cd6216fce69", "EXP Status");

			public static MultilingualString ITStatusDescription => ResString.GetMultilingualString("42661419-7ac7-4ddb-bc24-43236e7678a8", "IT Status");

			public static MultilingualString ISFBillNumberDescription => ResString.GetMultilingualString("8661ddac-6b25-4843-a3db-e96c0653a2d9", "ISF Bill Num.", "ISF Bill Number");

			public static MultilingualString ISFBillStatusDescription => ResString.GetMultilingualString("eb5b1cb0-6495-413d-a7e2-20d729a453a3", "ISF Bill Status");

			public static MultilingualString ISFBillStatusDescriptionDescription => ResString.GetMultilingualString("a14d4912-ed96-4843-81ba-aa0bcb4a5805", "ISF Bill Status Desc.", "ISF Bill Status Description");

			public static MultilingualString AFRBillStatusDescription => ResString.GetMultilingualString("b5e84120-ba75-4656-86cb-bea001381aaa", "AFR Bill Status", "Advance Filling Rules Bill Status");

			public static MultilingualString AFRBillStatusDescriptionDescription => ResString.GetMultilingualString("f723d390-3ee8-49a3-ad1c-49628944aa75", "AFR Bill Status Desc.", "AFR Bill Status Description", "Advance Filling Rules Bill Status Description");

			public static MultilingualString EntryStatusDescriptionDescription => ResString.GetMultilingualString("ecba87af-648b-4f11-b4d9-203d2ddf4424", "Entry Status Description");

			public static MultilingualString ACICargoStatusDescription => ResString.GetMultilingualString("a2243026-1fed-428a-a5eb-c55815897ae1", "ACI Cargo Status");

			public static MultilingualString ACIMessageStatusDescription => ResString.GetMultilingualString("33e0edcc-eabe-4756-a797-c77f9a60e541", "ACI Msg. Status", "ACI Message Status");

			public static MultilingualString CustomsEntryTypeDescription => ResString.GetMultilingualString("e9c94ce7-f7d9-482b-a732-978ea0b9bd77", "Customs Entry Type");

			public static MultilingualString ITEntryTypeDescription => ResString.GetMultilingualString("74b62138-3e89-4c2e-b103-f0fb5602f7ec", "IT Entry Type");

			public static MultilingualString CustomsEntryAuthorisationDateDescription => ResString.GetMultilingualString("631d76b7-cd39-4416-929e-d505b85821c5", "Customs Auth. Date", "Customs Authorization Date");

			public static MultilingualString DestinationGoodsValue => ResString.GetMultilingualString("A772B7BC-9B02-4A64-B3BF-890889C089B4", "Dest. Value", "Destination Value");

			public static MultilingualString DestinationCurrencyCode => ResString.GetMultilingualString("CF135AEF-82B8-4827-AC24-853EF08CE792", "Dest. Curr.", "Destination Currency");

			public static MultilingualString DestinationExchangeRate => ResString.GetMultilingualString("67283B2E-BB03-4315-882A-B02122BFB787", "Dest. Ex. Rate", "Destination Exchange Rate");
		}
	}

	public static class ForwardingConsolCustomsColumnConstants
	{
		public static class Schema
		{
			public const string CustomsCargoStatus = "CustomsCargoStatus";
			public const string AMSBillStatus = "AMSBillStatus";
			public const string AMSBillStatusDescription = "AMSBillStatusDescription";
			public const string LatestAMSDispositionCode = "LatestAMSDispositionCode";
			public const string LatestAMSDispositionDesc = "LatestAMSDispositionDesc";
			public const string AFRBillStatus = "AFRBillStatus";
			public const string AFRBillStatusDescription = "AFRBillStatusDescription";
			public const string OutwardReportStatus = "OutwardReportStatus";
			public const string OutwardReportStatusDescription = "OutwardReportStatusDescription";
			public const string OutwardReportEntryNumber = "OutwardReportEntryNumber";
			public const string AsycudaRegistrationStatus = "AsycudaRegistrationStatus";
		}

		public static class Captions
		{
			public static MultilingualString CustomsCargoStatusDescription => ResString.GetMultilingualString("6c01e35b-22d3-4537-b053-c8f01fe6d36e", "Customs Cargo Status");

			public static MultilingualString AMSBillStatusDescription => ResString.GetMultilingualString("1e250751-0bf8-4774-aa03-6bc46aae9be6", "AMS Bill Status");

			public static MultilingualString AMSBillStatusDescriptionDescription => ResString.GetMultilingualString("26a1730a-4418-41fa-a74b-5e83a4bd2e52", "AMS Bill Status Description");

			public static MultilingualString LatestAMSDispositionCode => ResString.GetMultilingualString("27fd19ab-3aac-430f-83c5-217b0540fadd", "Latest AMS Disposition Code", "Latest AMS Disp. Code");

			public static MultilingualString LatestAMSDispositionDesc => ResString.GetMultilingualString("1051ed28-a937-49c9-ab46-fb0d19ca0626", "Latest AMS Disposition Description", "Latest AMS Disp. Desc.");

			public static MultilingualString AFRBillStatusDescription => ResString.GetMultilingualString("0e5d4c1a-7610-475c-ba82-2f3cf6523a5f", "Advance Filling Rules Bill Status");

			public static MultilingualString AFRBillStatusDescriptionDescription => ResString.GetMultilingualString("7dc35fa9-796d-45dd-8324-0e490a32a0c7", "Advance Filling Rules Bill Status Description");

			public static MultilingualString OutwardReportStatusDescription => ResString.GetMultilingualString("4af10d4f-7a8a-4a89-9aea-2632d3e1aa4d", "Outward Report Status");

			public static MultilingualString OutwardReportStatusDescriptionDescription => ResString.GetMultilingualString("73845366-2cb0-4b6d-bbff-4d44d641d57b", "Outward Report Status Description");

			public static MultilingualString OutwardReportEntryNumberDescription => ResString.GetMultilingualString("1627d467-d589-468d-a0c6-c34ffb3b157f", "Outward Report Number");

			public static MultilingualString AsycudaRegistrationStatusDescription => ResString.GetMultilingualString("D9994072-0F99-405C-9DA1-963C866F4C7D", "Manifest Registration Status");
		}
	}
}
