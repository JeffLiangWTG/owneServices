using System.Collections.Immutable;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public static class UniversalReferenceConstants
	{
		public static class MethodOfPayments
		{
			public const string Cash = "A";
		}

		public static readonly ZDateTime UNPackTypeStartDate = new ZDateTime(2012, 1, 1);

		public static class RefCusRateCodes
		{
			public const string CustomsDutyOnIndustrialProducts = "A00";
			public const string DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts = "A10";
			public const string AdditionalDutyCountervailingSafeguardChargeVariableCharge = "A20";
			public const string DefinitiveAntiDumpingDuty = "A30";
			public const string ProvisionalAntiDumpingDuty = "A35";
			public const string DefinitiveCountervailingDuty = "A40";
			public const string ProvisionalCountervailingDuty = "A45";
			public const string Vat = "B00";
			public const string VatOnAdditionalDutiesForNorthernIreland = "B05";
			public const string CompensatoryInterestVat = "B10";

			public const string AgriculturalComponent = "EA";
			public const string AdditionalDutyOnSugarContents = "ADSZ";
			public const string AdditionalDutyOnFlourContents = "ADFM";
			public const string ExportRefund = "350";
			public const string ClimateChangeLevy = "990";

			public static readonly ImmutableArray<string> AdditionalDuties = new string[] { AgriculturalComponent, AdditionalDutyOnSugarContents, AdditionalDutyOnFlourContents }.ToImmutableArray();
		}

		public static class CustomsOfficeAttributes
		{
			public const string Excise = "EXC";
			public const string Street = "Street";
			public const string PostCode = "PostCode";
			public const string CITY = "CITY";
		}

		public static class RefCusTaxOrFee
		{
			public const string GST = "GST";
			public const string GSD = "GSD";
		}

		public static class RefCusCodeListAttributes
		{
			public static class Names
			{
				public const string Level = "Level";
			}

			public static class Values
			{
				public const string DecBox47 = "Dec|Box47";
				public const string Import = "IMPORT";
				public const string Export = "EXPORT";
				public const string Item = "ITEM";
				public const string Header = "HEADER";
			}
		}

		public static class RefCusCodeListType
		{
			public const string Port = "PORT";
			public const string Shed = "FAC";

			public static class Code
			{
				public const string Code_2AAA = "2AAA";
				public const string Code_2AAC = "2AAC";

				public const string Code_I0810 = "I0810";
				public const string Code_NC008 = "NC008";
				public const string Code_NCNAT = "NCNAT";
				public const string Code_C0009 = "C0009";
				public const string Code_C0063 = "C0063";
				public const string Code_I0800 = "I0800";
				public const string Code_NC010 = "NC010";
				public const string Code_A2055 = "A2055";

				public const string Code_EX17 = "EX17";
				public const string Code_IM17 = "IM17";
				public const string Code_CO17 = "CO17";
				public const string Code_EU17 = "EU17";
				public const string Code_EX15 = "EX15";
				public const string Code_IM15 = "IM15";
				public const string Code_CO15 = "CO15";
				public const string Code_EU15 = "EU15";
				public const string Code_EXNAT = "EXNAT";

				public const string Code_ECICS = "ECICS";
				public const string Code_IC2TG = "IC2TG";
				public const string Code_IC2SC = "IC2SC";

				public const string SupportingDocumentOfNCTS = "DC44N";

				public const string Code_AI008 = "AI008";
				public const string Code_AI44X = "AI44X";
				public const string Code_CL010 = "CL010";
				public const string Code_CL019 = "CL019";
				public const string Code_CL030 = "CL030";
				public const string Code_CL148 = "CL148";
				public const string Code_CL170 = "CL170";
				public const string Code_CL180 = "CL180";
				public const string Code_CL210 = "CL210";
				public const string Code_CL211 = "CL211";
				public const string Code_CL226 = "CL226";
				public const string Code_CL227 = "CL227";
				public const string Code_CL251 = "CL251";
				public const string Code_CL560 = "CL560";
				public const string Code_CL570 = "CL570";
				public const string Code_CL701 = "CL701";
				public const string Code_CL716 = "CL716";
				public const string Code_CL733 = "CL733";
				public const string Code_CL754 = "CL754";
				public const string Code_AUTH = "AUTH";
				public const string Code_EUNAU = "EUNAU";
				public const string Code_IC2AI = "IC2AI";
				public const string Code_IC2DT = "IC2DT";
				public const string Code_IC2MS = "IC2MS";
				public const string Code_IC2MT = "IC2MT";
				public const string Code_IC2SA = "IC2SA";
				public const string Code_IC2SM = "IC2SM";

				public const string ExportAdditionalInformation = "AI44E";
				public const string ExportAdditionalReference = "AR44E";
				public const string ExportTransportDocument = "TD44E";
				public const string ImportAdditionalInformation = "AI44I";
				public const string ImportAdditionalReference = "AR44I";
				public const string ImportTransportDocument = "TD44I";

				public const string LegalBasisCode = "LBC";

				public const string TemporaryStorageCustomsStatus = "TSTA";
				public const string TemporaryStorageStatusReason = "TSTAR";
			}
		}

		public static class PortAttributes
		{
			public const string Type = "Type";
		}

		public static class ShedAttributes
		{
			public const string ACPCode = "ACPCODE";
			public const string Chief = "CHIEF";
			public const string ETSF = "ETSF";
			public const string DEP = "DEP";
			public const string ChiefShed = "CHIEFSHED";
			public const string AirportName = "AIRPORTNAME";
			public const string SITECODE = "SITECODE";
		}

		public static class SupportingDocumentTypes
		{
			public const string Certificate = "CERT";
			public const string _337 = "337";
			public const string _1217 = "1217";
			public const string D005 = "D005";
			public const string D008 = "D008";
			public const string N325 = "N325";
			public const string N337 = "N337";
			public const string N380 = "N380";
			public const string N704 = "N704";
			public const string N705 = "N705";
			public const string N740 = "N740";
			public const string N741 = "N741";
			public const string N830 = "N830";
			public const string N935 = "N935";
			public const string Other = "ZZZ";
			public const string U164 = "U164";
			public const string U165 = "U165";
			public const string U166 = "U166";
			public const string U167 = "U167";
			public static readonly ImmutableArray<ZString> InvoiceDocumentTypesForImport = new ZString[] { N380, N325, N935, D005, D008 }.ToImmutableArray();
			public static readonly ImmutableArray<ZString> InvoiceDocumentTypesForExport = new ZString[] { N380, N325, N935 }.ToImmutableArray();
		}

		public static class AdditionalDocumentTypes
		{
			public const string _30600 = "30600";
			public const string _00100 = "00100";
		}

		public static class CusSupportingInfoTypes
		{
			public const string C057 = "C057";
			public const string C079 = "C079";
			public const string C082 = "C082";
			public const string C085 = "C085";
			public const string C640 = "C640";
			public const string C644 = "C644";
			public const string C678 = "C678";
			public const string E013 = "E013";
			public const string L100 = "L100";
			public const string N853 = "N853";
			public const string Y120 = "Y120";
			public const string Y121 = "Y121";
			public const string Y123 = "Y123";
			public const string Y124 = "Y124";
			public const string Y125 = "Y125";
			public const string Y798 = "Y798";
			public const string Y951 = "Y951";
			public const string Y986 = "Y986";
		}

		public static class RefCusCodeListLevelType
		{
			public const string Item = "Item";
			public const string Header = "Header";
			public const string Both = "Both";
		}

		public static class RefCusCodeListDirectionType
		{
			public const string Import = "Import";
			public const string Export = "Export";
			public const string Both = "Both";
		}

		public static class RefCusCodeBulkPackageUnitType
		{
			public const string BulkGas = "VG";
			public const string BulkLiquid = "VL";
			public const string BulkNodules = "VO";
			public const string BulkLiquidGas = "VQ";
			public const string BulkGrains = "VR";
			public const string BulkScrap = "VS";
			public const string BulkPowders = "VY";
		}

		public static class RefCusCodeUnPackedPackageUnitType
		{
			public const string Unpacked = "NE";
			public const string UnpackedSingle = "NF";
			public const string UnpackedMultiple = "NG";
		}

		public static class RefCusCodeListEntryStyle
		{
			public const string Export = "EX";
			public const string Import = "IM";
			public const string ImportOfGoodsFromEftaMemberState = "EU";
			public const string ImportOfGoodsFromSpecialTerritoryOfTheCommunity = "CO";
		}

		public static class BillNumber
		{
			public const string TBA = "TBA";
		}

		public static class RefCusCodeListEntrySubStyle
		{
			public const string EXS = "EXS";
		}

		public static class RefCusTradeGroups
		{
			public static class Groups
			{
				public const string EUSpecialFiscalTerritories = "EUSFT";
				public const string EUSpecialFiscalTerritoryCountries = "EUSFR";
				public const string EFTA = "1021";
			}
		}

		public static class RefZoneHeaders
		{
			public const string EUR1 = "EUR1";
		}

		public static class CusReferenceTypeCodes
		{
			public const string AUT = "AUT";
		}

		public static class CusReferenceTypeDescriptions
		{
			public const string AUT = "Customs Authorization";
		}

		public static class CusAuthorisationHeaderType
		{
			public const string CustomsAuthorizedLocations = "CLO";
			public const string BindingOriginInformation = "BOI";
			public const string BindingTariffInformation = "BTI";
		}

		public static class CountryOfDestination
		{
			public static class B1872
			{
				public const string StoresAndProvisions = "QQ";
				public const string StoresAndProvisionsIntraComm = "QR";
				public const string CountriesNotSpecIntraComm = "QV";

				public static readonly ImmutableArray<ZString> ExtraToCL140 = new ZString[] {
					Core.Constants.CountryCodes.Andorra,
					Core.Constants.CountryCodes.SanMarino,
					Core.Constants.CountryCodes.Germany,
					Core.Constants.CountryCodes.Italy
				}.ToImmutableArray();

				public static readonly ImmutableArray<ZString> ExcludeFromCL140 = new ZString[] {
					StoresAndProvisions, StoresAndProvisionsIntraComm, CountriesNotSpecIntraComm
				}.ToImmutableArray();
			}

			public static class B1871
			{
				public static readonly ImmutableArray<ZString> ExcludeFromCL063 = new ZString[] { Core.Constants.CountryCodes.Andorra, Core.Constants.CountryCodes.SanMarino }.ToImmutableArray();
			}
		}

		public static class PNTS
		{
			public static class CustomsStatus
			{
				public const string Cancelled = "CAN";
				public const string Clearance = "CLR";
				public const string TSDInvalidated = "INV";
				public const string PresentationNotificationLinked = "PLN";
				public const string PresentationNotificationNotLinked = "PNL";
				public const string IntendedControl = "TCT";
				public const string ProofOfUnionStatusPresented = "TEP";
				public const string IrregularityUnderInvestigation = "TII";
				public const string MeasuresRequired = "TMR";
				public const string TemporaryStorageEnded = "TND";
				public const string TemporaryStorageActivated = "TSA";
				public const string TemporaryStoragePreLodged = "TSP";
				public const string UnderControl = "TUC";
			}
		}

		public static class PreviusDocumentType
		{
			public const string SummaryDeclaration = "SUM";
		}
	}
}
