using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Common.EU;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common
{
	public class CusEntryNumberTypes : Integration.Customs.ICusEntryNumberTypes
	{
		#region Country Specific Codes

		#region Standard Codes

		public static class Standard
		{
			public const string ClearancePermitNumber = "PMT";
			public const string DrawbackClaim = "DCI";
			public const string UniqueConsignementReference = "UCR";
			public const string MovementReferenceNumber = "MRN";
			public const string LocalReferenceNumber = "LRN";
			public const string ImportControlNumber = "ICN";
		}

		#endregion

		#region Australian Codes

		public static class Australia
		{
			public const string ECN = "ECN";
			public const string CRN = "CRN";
			public const string MMN = "MMN";
			public const string CAN = "CAN";

			public const string EX1 = "EX1";
			public const string EX2 = "EX2";
			public const string EX3 = "EX3";
			public const string EX5 = "EX5";
			public const string EX7 = "EX7";
			public const string EX9 = "EX9";
			public const string EXA = "EXA";
			public const string EXB = "EXB";
			public const string EXC = "EXC";

			public const string IMP = "IMP";
			public const string RFS = "RFS";

			public const string QuarantineCertificateNumber = "QCN";
			public const string InactiveLodgmentReferenceNumber = "LCL";
		}

		#endregion

		#region Brazilian Codes

		public static class Brazil
		{
			public const string DUE = "DUE";
			public const string EAK = "EAK";
			public const string CustomsOwnNumber = "CON";
			public const string ImportLicenseIdentifier = "LIC";
			public const string BillNumber = "BLN";
		}

		#endregion

		#region EU Codes

		public static class EU
		{
			/// <summary>
			/// This code is applied to the declaration and automatically copied to the entry header if there is one only
			/// Multiple entry headers will require the user to entry the 'second' Master UCR manually
			/// </summary>
			public const string MasterUCR = "MUC";
			public const string T1 = "T1";
			public const string TIRCarnetNumber = "TIR";
			public const string PRE = "PRE";
			public const string LocalReferenceNumber = "LRN";
			public const string CustomsRegistrationNumber = "CRN";
			public const string FunctionalReferenceNumber = "FRN";
			public const string ArrivalReferenceNumber = "ARN";
			public const string SimplifiedDeclarationMRN = "SIM";
			public const string CustomsRegistry = "REG";
			public const string EntrySummary = "ENS";
			public const string CorrelationIdentifier = "CID";
			public const string Fallback = "FBK";

			public static CodeDescriptionPairList EUCustomsEntryTypeList
			{
				get
				{
					CodeDescriptionPairList result = new UntranslatableCodeDescriptionPairList((NoResString)"Country-Specific Customs values");
					result.AddPair("IMP", (NoResString)"Import entry");
					result.AddPair("EXP", (NoResString)"Export entry");
					result.AddPair(MasterUCR, (NoResString)"Master UCR");
					result.AddPair(CusEntryNumberTypes.Standard.UniqueConsignementReference, (NoResString)"Declaration UCR");
					result.AddPair(CusEntryNumberTypes.Standard.MovementReferenceNumber, (NoResString)"Movement Reference Number");

					// The following line adds Community Transit Status codes to the shipment's "entry number type" dropdown. This is incorrect, X is not an entry number type.
					// However there is some VAT behaviour that depends on this.  See WI00015985 and WI00032890.
					// When this VAT behavoiur is changed to look at the shipment's CT Status field, not at the shipment's entry number field, we can remove this line:
					result.AddRange(CommunityTransitStatusCodesList_Export);

					return result;
				}
			}

			public static CodeDescriptionPairList CommunityTransitStatusCodesList
			{
				get
				{
					var result = new UntranslatableCodeDescriptionPairList((NoResString)"Country-Specific Customs values");
					result.AddRangeOverwriteIfExists(CommunityTransitStatusCodesList_Import);
					result.AddRangeOverwriteIfExists(CommunityTransitStatusCodesList_Export);
					result.Sort();
					return result;
				}
			}

			public static CodeDescriptionPairList CommunityTransitStatusCodesList_Import
			{
				get { return new ImportCommunityTransitStatusList(); }
			}

			public static CodeDescriptionPairList CommunityTransitStatusCodesList_Export
			{
				get { return new ExportCommunityTransitStatusList(); }
			}
		}

		#endregion

		#region Indian

		public static class Indian
		{
			public const string InMan = "INM";
			public const string ImportGeneralManifest = "IGM";
			public const string ReservedBankOfIndia = "RBI";
			public const string ShippingBill = "SB";
		}

		#endregion

		#region Malaysian Codes

		public static class Malaysia
		{
			public const string MAN = "MAN";
		}

		#endregion

		#region Hong Kong Codes

		public static class HongKong
		{
			public const string ExportLicense = "E/L";
			public const string ImportLicense = "I/L";
		}

		#endregion

		#region United States Codes

		public static class UnitedStates
		{
			public const string CRN = "CRN";
			public const string EntrySummary = "ENS";
			public const string InBond = "INB";
			public const string FTZ = "FTZ";
			public const string Protest = "PRO";
			public const string ITN = "ITN";
			public const string AES = "AES";
		}

		#endregion

		#region Iceland Codes

		public static class Iceland
		{
			public const string CRN = "CRN";
		}

		#endregion

		#region Switzerland Codes

		public static class Switzerland
		{
			public const string AccessCode = "ACC";
			public const string GoodsDeclarationReferenceNumber = "GDR";
			public const string GoodsDeclarationReferenceNumber_4DIGITS = "GDRN";
		}

		#endregion

		#region Israel Codes

		public static class Israel
		{
			public const string DeliveryOrderNumber = "DON";
			public const string GatepassMovementNumber = "GMN";
		}

		#endregion

		public static string UserFriendlyEntryType(ZString entryType)
		{
			return entryType == CanadaAdditionalReferenceNumberTypes.Codes.CTN ? "POR" : entryType.ToString();
		}

		#endregion

		public static bool IsExemptionCode(ZString entryType)
		{
			return (entryType.StartsWith("EX") && entryType != "EXP") || new CMRExportExemptionCodesList().ContainsCode(entryType);
		}

		#region Customs Permit Number Type List
		public static bool IsUSCustomsCountryNeededEXPEntryType(ZString countryCode)
		{
			return countryCode == Constants.CountryCodes.UnitedStates || countryCode == Constants.CountryCodes.PuertoRico || countryCode == Constants.CountryCodes.VirginIslands;
		}

		public static CodeDescriptionPairList CountrySpecificCustomsEntryNumberTypeList(BusinessObjectFactory factory, ZString countryCode, bool isImport)
		{
			Argument.NotNull(factory, "factory");

			return factory.GetCachedValue(string.Format("CusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList|{0}|{1}", countryCode, isImport),
				() => GetNewCountrySpecificCustomsEntryNumberTypeList(countryCode, isImport));
		}

		static CodeDescriptionPairList GetNewCountrySpecificCustomsEntryNumberTypeList(ZString countryCode, bool isImport)
		{
			CodeDescriptionPairList result = new UntranslatableCodeDescriptionPairList((NoResString)"Country-Specific Customs values");

			AddEconomicGroupCodes(result, countryCode);

			if (Constants.CountryCodes.IsUsaOrTerritory(countryCode))
			{
				result = GetUSEntryNumberTypeList(isImport, result);
			}
			else
			{
				switch (countryCode)
				{
					case Constants.CountryCodes.Australia:
						result = GetAUEntryNumberTypeList(isImport, result);
						break;

					case Constants.CountryCodes.Brazil:
						result = GetBREntryNumberTypeList(isImport, result);
						break;

					case Constants.CountryCodes.Singapore:
						result = new SG.CustomsEntryTypeList();
						break;

					case Constants.CountryCodes.HongKong:
						result = GetHKEntryNumberTypeList(isImport, result);
						break;

					case Constants.CountryCodes.NewZealand:
						result = new NZ.CusEntryNumberTypeList();
						break;

					case Constants.CountryCodes.Iceland:
						result.AddPair(Iceland.CRN, (NoResString)"Sendingarnumer");
						break;

					case Constants.CountryCodes.SouthAfrica:
						result = GetDefaultEntryNumberTypeList(result);
						result.AddPair(Standard.MovementReferenceNumber, (NoResString)"Movement Reference Number");
						break;

					case Constants.CountryCodes.Germany:
						result = GetDefaultEntryNumberTypeList(result);
						result.AddPair(Standard.LocalReferenceNumber, (NoResString)"Local Reference Number");
						break;

					case Constants.CountryCodes.Switzerland:
						result = GetDefaultEntryNumberTypeList(result);
						result.AddPair(Switzerland.GoodsDeclarationReferenceNumber, (NoResString)"Goods Declaration Reference Number");
						break;

					default:
						result = GetDefaultEntryNumberTypeList(result);
						break;
				}
			}

			if (result.Count == 0)
			{
				result.AddPair("IMP", (NoResString)"Import entry");
				result.AddPair("EXP", (NoResString)"Export entry");
			}

			return result;
		}

		static CodeDescriptionPairList GetDefaultEntryNumberTypeList(CodeDescriptionPairList result)
		{
			result.AddPair(Standard.ClearancePermitNumber, (NoResString)"Customs Permit/Clearance Number");
			result.AddRange(FreightDataRegistry.Instance.CustomsPermitClearanceNumbers.Value);
			return result;
		}

		static CodeDescriptionPairList GetUSEntryNumberTypeList(bool isImport, CodeDescriptionPairList result)
		{
			if (isImport)
			{
				result.AddPair(UnitedStates.CRN, (NoResString)"Customs Permit/Clearance Number");
				result.AddPair(UnitedStates.EntrySummary, (NoResString)"Entry Summary");
				result.AddPair(UnitedStates.InBond, "In-Bond");
			}
			else
			{
				result = new US.CusEntryNumberTypeList();
			}
			return result;
		}

		static CodeDescriptionPairList GetHKEntryNumberTypeList(bool isImport, CodeDescriptionPairList result)
		{
			result.AddPair(isImport ? HongKong.ImportLicense : HongKong.ExportLicense, isImport ? (NoResString)"Import License" : (NoResString)"Export License");
			return result;
		}

		static CodeDescriptionPairList GetAUEntryNumberTypeList(bool isImport, CodeDescriptionPairList result)
		{
			if (!isImport)
			{
				result = new CANTypeList();
			}
			return result;
		}

		static CodeDescriptionPairList GetBREntryNumberTypeList(bool isImport, CodeDescriptionPairList result)
		{
			if (GlbCompany.CurrentCompany.Country.Code == Constants.CountryCodes.Brazil && !isImport)
			{
				result.AddPair(Brazil.DUE, (NoResString)"Declaração Única de Exportação");
				result.AddPair(Brazil.EAK, (NoResString)"Entry Access Key");
			}
			return result;
		}

		static void AddEconomicGroupCodes(CodeDescriptionPairList result, ZString countryCode)
		{
			RefCountry country = new BusinessObjectFactory().LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, countryCode));
			if (country != null && ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsMemberOfEU(countryCode))
			{
				result.AddRange(CusEntryNumberTypes.EU.EUCustomsEntryTypeList);
			}
		}

		#endregion

		#region Default Number

		public static ZString CountrySpecificDefaultEntryNumberType(ZString countryCode, bool isImport)
		{
			if (Constants.CountryCodes.IsUsaOrTerritory(countryCode))
			{
				return GetUSDefaultEntryNumberType(isImport);
			}
			else
			{
				switch (countryCode)
				{
					case Constants.CountryCodes.Australia:
						return GetAUDefaultEntryNumberType(isImport);

					case Constants.CountryCodes.Brazil:
					case Constants.CountryCodes.Singapore:
					case Constants.CountryCodes.NewZealand:
					case Constants.CountryCodes.UnitedKingdom:
						return ZString.Empty;

					case Constants.CountryCodes.HongKong:
						return GetHKDefaultEntryNumberType(isImport);

					case Constants.CountryCodes.Iceland:
						return Iceland.CRN;

					default:
						return Standard.ClearancePermitNumber;
				}
			}
		}

		static ZString GetAUDefaultEntryNumberType(bool isImport)
		{
			return isImport ? string.Empty : CANType.CustomsAuthorityNumber.Code;
		}

		static ZString GetHKDefaultEntryNumberType(bool isImport)
		{
			return isImport ? HongKong.ImportLicense : HongKong.ExportLicense;
		}

		static ZString GetUSDefaultEntryNumberType(bool isImport)
		{
			return isImport ? UnitedStates.CRN : US.CusEntryNumberTypeList.Codes.ITN;
		}

		#endregion

		#region Not Cleared by Agent Number Type

		public static ZString NotClearedByAgentNumberType(ZString countryCode)
		{
			ZString result = ZString.Empty;
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Germany:
					result = EU.T1;
					break;
			}

			return result;
		}

		#endregion

		#region ICusEntryNumberTypes Members

		ICodeDescriptionPairList Integration.Customs.ICusEntryNumberTypes.CountrySpecificCustomsEntryNumberTypeList(BusinessObjectFactory factory, ZString countryCode, bool isImport)
		{
			return CountrySpecificCustomsEntryNumberTypeList(factory, countryCode, isImport);
		}

		#endregion

		public static class ASYCUDA
		{
			public const string AsycudaRegistration = "ASY";
		}

		public static class SouthAfrica
		{
			public const string CustomsAssignedReferenceNumberCARN = "CAR";
			public const string LocalReferenceNumber = Standard.LocalReferenceNumber;
		}

		public static class China
		{
			public const string BillOfLading = "B/L";
			public const string PreEntryNumber = "PRE";
			public const string DeclarationUnifiedNumber = "UNI";
			public const string CIQNumber = "CIQ";
		}

		public static class Germany
		{
			public const string AirWaybillEntryNumber = "AWB";
			public const string SumAEntryNumber = "SUM";
			public const string ReExportEntryNumber = "REX";
		}

		public static class France
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Fallback = "FBK";
			public const string DDT = "DDT";
		}

		public static class Taiwan
		{
			public const string Import = "IMP";
			public const string Export = "EXP";
			public const string Transhipment = "TRS";
			public const string ForwarderHouseManifest = "FHM";
			public const string LicensingMessage = "NXM";
			public const string ImportBriefCustomsDeclaration = "IBC";
			public const string ExportBriefCustomsDeclaration = "EBC";
			public const string Permit = "PRN";
			public const string ProcessingNumber = "PRS";
			public const string CustomsMessageIdentifier = "CMI";
		}

		public static class Turkey
		{
			public const string TIR = "TIR";
			public const string PRV = "PRV";
			public const string ManifestToOpen = "MTO";
		}

		public static class Uruguay
		{
			public const string DNA = "DNA";
		}

		public static class Spain
		{
			public const string ClearanceCSV = "CLR";
			public const string T2CMovementReferenceNumber = "TMR";
			public const string SummaryEntryNumber = "SUM";
			public const string ArrivalReferenceNumber = "ARN";
			public const string CircuitCan = "CIS";
		}

		public static class Mexico
		{
			public const string MXSeaCustomsNumber = "MSC";
		}

		public static class Chile
		{
			public const string IDS = "IDS";
		}

		public static class Norway
		{
			public const string CustomsEntryReleaseNumber = "CER";
			public const string GoodsNumber = "GNO";
			public const string GoodsNumberSubPosition = "GSP";
		}

		public static class JP
		{
			public const string InputReference = "JPR";
			public const string ExportControlNumber = "ECR";
			public const string MoveInNotice = "MIN";
			public const string BillNumber = "BNO";
			public const string BookingNumber = "BKG";
			public const string TemporaryLandingRegistrationNumber = "TLN";
			public const string NSI = "NSI";
		}
	}
}
