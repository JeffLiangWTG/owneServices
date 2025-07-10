using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	static class PGAHeaderExtensions
	{
		public static UNDGSubstanceCollection GetCachedUNDGSubstanceCollection(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("UNDGSubstanceCollection_IMO", () =>
			{
				var query = new ZQuery(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
				var collection = new UNDGSubstanceCollection(factory, query);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Standard", "Property", (ZString)UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
				return collection;
			});
		}

		public static ZBool IsPGAValidationEnabled(this ICADeclarationProvider provider)
		{
			var result = false;
			if (provider?.Declaration is JobDeclaration declaration)
			{
				result = declaration.IsIID && declaration.JE_MessageType == JobMessageTypeList.Codes.Import;
			}
			return result;
		}

		public static JobComInvoiceLine GetParentInvoiceLine(this IPGAProgramRequirementProvider pgaHeader)
		{
			switch (pgaHeader.GovAgencyIDCode)
			{
				case PGACodes.Codes.CFIA:
					return ((CFIAPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.CNSC:
					return ((CNSCPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.DFO:
					return ((DFOPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.ECCC:
					return ((ECCCPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.GAC:
					return ((GACPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.HC:
					return ((HCPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.NRCan:
					return ((NRCanPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.PHAC:
					return ((PHACPGAHeader)pgaHeader).InvoiceLine;
				case PGACodes.Codes.TC:
					return ((TCPGAHeader)pgaHeader).InvoiceLine;
				default:
					return null;
			}
		}

		public static IEnumerable<string> GetEnabledProgramCodes(this IPGAProgramRequirementProvider pgaHeader)
		{
			return pgaHeader.GetProgramCodesList().GetAllCodes().Where(x => pgaHeader.IsProgramEnabled(x));
		}

		public static bool NeedsDocumentTypeValidation(this IPGAProgramRequirementProvider pgaHeader)
		{
			return pgaHeader.GovAgencyIDCode != PGACodes.Codes.HC;
		}
		public static bool IsProgramEnabled(this IPGAProgramRequirementProvider pgaHeader, ZString programCode)
		{
			var programInfo = pgaHeader.GetProgramIndicatorInfo(programCode);
			return programInfo != null && (ZString)programInfo.Value == YesNoList.Codes.Yes;
		}

		public static IEnumerable<IRequiredDocumentType> GetDefaultLPCOTypesForAllEnalbedPrograms(this IPGAProgramRequirementProvider pgaHeader)
		{
			var defaultTypes = new List<IRequiredDocumentType>();

			foreach (var enabledProgramCode in pgaHeader.GetEnabledProgramCodes())
			{
				defaultTypes.AddRange(pgaHeader.GetDefaultDocumentTypes(enabledProgramCode));
			}

			return defaultTypes;
		}

		public static List<string> GetURNMandatoryDocumentTypes(this IPGAProgramRequirementProvider pgaHeader)
		{
			switch (pgaHeader.GovAgencyIDCode)
			{
				case PGACodes.Codes.ECCC:
					return new List<string> { LPCODocumentTypeQualifier.Codes._8020, LPCODocumentTypeQualifier.Codes._8021, LPCODocumentTypeQualifier.Codes._8022, LPCODocumentTypeQualifier.Codes._8023,
											LPCODocumentTypeQualifier.Codes._8000, LPCODocumentTypeQualifier.Codes._8001, LPCODocumentTypeQualifier.Codes._8010, LPCODocumentTypeQualifier.Codes._8011, LPCODocumentTypeQualifier.Codes._8012,
					LPCODocumentTypeQualifier.Codes._8030, LPCODocumentTypeQualifier.Codes._8031 };
				case PGACodes.Codes.HC:
					return new List<string> { LPCODocumentTypeQualifier.Codes._5008, LPCODocumentTypeQualifier.Codes._5009 };
				case PGACodes.Codes.TC:
					return new List<string> { LPCODocumentTypeQualifier.Codes._4002, LPCODocumentTypeQualifier.Codes._4003, LPCODocumentTypeQualifier.Codes._4005, LPCODocumentTypeQualifier.Codes._4006, LPCODocumentTypeQualifier.Codes._4001, LPCODocumentTypeQualifier.Codes._4004 };
				default:
					return new List<string>() { };
			}
		}

		public static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes(this IPGAProgramRequirementProvider pgaHeader, ZString programCode)
		{
			switch (pgaHeader.GovAgencyIDCode)
			{
				case PGACodes.Codes.CNSC:
					return GetDefaultDocumentTypes_CNSC();
				case PGACodes.Codes.DFO:
					return GetDefaultDocumentTypes_DFO(pgaHeader, programCode);
				case PGACodes.Codes.ECCC:
					return GetDefaultDocumentTypes_ECCC(pgaHeader, programCode);
				case PGACodes.Codes.GAC:
					return GetDefaultDocumentTypes_GAC();
				case PGACodes.Codes.HC:
					return GetDefaultDocumentTypes_HC(pgaHeader, programCode);
				case PGACodes.Codes.NRCan:
					return GetDefaultDocumentTypes_NRCan(programCode);
				case PGACodes.Codes.TC:
					return GetDefaultDocumentTypes_TC(pgaHeader, programCode);
				case PGACodes.Codes.PHAC:
					return GetDefaultDocumentTypes_PHAC(pgaHeader, programCode);
				default:
					return Enumerable.Empty<IRequiredDocumentType>();
			}
		}

		public static void SetPGAReadOnly(this IPGAHeader header)
		{
			if (header is BusinessObject bizObj && !bizObj.IsNull)
			{
				bizObj.SetReadOnlyIncludingChildren(true);
			}
		}

		#region CNSC

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_CNSC()
		{
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._7000, DocumentTypeRequieredType.Alternative);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._7001, DocumentTypeRequieredType.Alternative);
		}

		#endregion

		#region DFO

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_DFO(IPGAHeader pgaHeader, ZString programCode)
		{
			if (programCode == DFOPGADepartmentCodes.Codes.ABI)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6000, DocumentTypeRequieredType.Optional);
			}
			else if (programCode == DFOPGADepartmentCodes.Codes.AIS)
			{
				var dfoPGAHeader = pgaHeader as DFOPGAHeader;
				var requieredType = dfoPGAHeader != null && !dfoPGAHeader.CA_SpeciesCode.IsEmpty && !dfoPGAHeader.CA_Eviscerated
					? DocumentTypeRequieredType.AtLeastOne
					: DocumentTypeRequieredType.Optional;

				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6001, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6002, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6010, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6011, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6012, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6013, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6014, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6015, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6016, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6017, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6018, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6019, requieredType);
			}
			else if (programCode == DFOPGADepartmentCodes.Codes.TTP)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6003, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6004, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6006, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6007, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6008, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6009, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6020, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6022, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6023, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6005, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6008, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._6021, DocumentTypeRequieredType.Optional);
			}
		}

		#endregion

		#region ECCC

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_ECCC(IPGAHeader pgaHeader, ZString programCode)
		{
			if (programCode == ECCCPGADepartmentCodes.Codes.WRM)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8000);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8001);
			}
			else if (programCode == ECCCPGADepartmentCodes.Codes.ODS)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8010, DocumentTypeRequieredType.AtLeastOne);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8011, DocumentTypeRequieredType.AtLeastOne);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8012, DocumentTypeRequieredType.AtLeastOne);
			}
			else if (programCode == ECCCPGADepartmentCodes.Codes.VEE)
			{
				var ecccPGAHeader = pgaHeader as ECCCPGAHeader;
				var typeRequiered = ecccPGAHeader != null && ecccPGAHeader.CA_ProcessCode == ProcessCodes.Codes.XE02 && ecccPGAHeader.CA_AOSConformity == AffirmationOfStatementCodes.Codes.ME03;
				if (typeRequiered)
				{
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8030, DocumentTypeRequieredType.AtLeastOne);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8031, DocumentTypeRequieredType.AtLeastOne);
				}
			}
			else if (programCode == ECCCPGADepartmentCodes.Codes.WEN)
			{
				var ecccPGAHeader = pgaHeader as ECCCPGAHeader;
				var requieredType = ecccPGAHeader != null && !ecccPGAHeader.CA_IntendedUseCode.IsEmpty
					? DocumentTypeRequieredType.AtLeastOne
					: DocumentTypeRequieredType.Optional;

				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8020, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8021, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8022, requieredType);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._8023, requieredType);
			}
		}

		#endregion

		#region GAC

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_GAC()
		{
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._2001, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._2003, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._2004, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._2005, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._2006, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._2007, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._80, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._81, DocumentTypeRequieredType.Optional);
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._83, DocumentTypeRequieredType.Optional);
		}

		#endregion

		#region HC

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC(IPGAHeader pgaHeader, ZString programCode)
		{
			var hcPGAHeader = pgaHeader as HCPGAHeader;
			if (hcPGAHeader == null)
			{
				return Enumerable.Empty<IRequiredDocumentType>();
			}

			switch (programCode)
			{
				case HCPGADepartmentCodes.Codes.API:
					return GetDefaultDocumentTypes_HC_API(hcPGAHeader.CA_IntendedUseCodeAPI, hcPGAHeader.CA_CategoryAPI);
				case HCPGADepartmentCodes.Codes.BBC:
					return GetDefaultDocumentTypes_HC_BBC(hcPGAHeader.CA_IntendedUseCodeBBC, hcPGAHeader.CA_CategoryBBC);
				case HCPGADepartmentCodes.Codes.CTO:
					return GetDefaultDocumentTypes_HC_CTO(hcPGAHeader.CA_IntendedUseCodeCTO, hcPGAHeader.CA_CategoryCTO);
				case HCPGADepartmentCodes.Codes.CPR:
					return GetDefaultDocumentTypes_HC_CPR(hcPGAHeader.CA_IntendedUseCodeCPR, hcPGAHeader.CA_CategoryCPR);
				case HCPGADepartmentCodes.Codes.DSE:
					return GetDefaultDocumentTypes_HC_DSE(hcPGAHeader.CA_IntendedUseCodeDSE, hcPGAHeader.CA_CategoryDSE);
				case HCPGADepartmentCodes.Codes.HDR:
					return GetDefaultDocumentTypes_HC_HDR(hcPGAHeader.CA_IntendedUseCodeHDR, hcPGAHeader.CA_CategoryHDR);
				case HCPGADepartmentCodes.Codes.OCS:
					return GetDefaultDocumentTypes_HC_OCS(hcPGAHeader.CA_IntendedUseCodeOCS, hcPGAHeader.CA_CategoryOCS);
				case HCPGADepartmentCodes.Codes.MDE:
					return GetDefaultDocumentTypes_HC_MDE(hcPGAHeader.CA_IntendedUseCodeMDE, hcPGAHeader.CA_CategoryMDE);
				case HCPGADepartmentCodes.Codes.NHP:
					return GetDefaultDocumentTypes_HC_NHP(hcPGAHeader.CA_IntendedUseCodeNHP, hcPGAHeader.CA_CategoryNHP);
				case HCPGADepartmentCodes.Codes.PES:
					return GetDefaultDocumentTypes_HC_PES(hcPGAHeader.CA_IntendedUseCodePES, hcPGAHeader.CA_CategoryPES);
				case HCPGADepartmentCodes.Codes.RED:
					return GetDefaultDocumentTypes_HC_RED();
				case HCPGADepartmentCodes.Codes.VET:
					return GetDefaultDocumentTypes_HC_VET(hcPGAHeader.CA_IntendedUseCodeVET, hcPGAHeader.CA_CategoryVET);
			}

			return Enumerable.Empty<IRequiredDocumentType>();
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_API(ZString intendedUseCode, ZString category)
		{
			if (intendedUseCode == HCIntendedUseCode.Codes.HC13 && category == HCCategories.Codes.HC01)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5001);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_BBC(ZString intendedUseCode, ZString category)
		{
			if (intendedUseCode == HCIntendedUseCode.Codes.HC01 && category == HCCategories.Codes.HC02)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5002, DocumentTypeRequieredType.Alternative);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5003, DocumentTypeRequieredType.Alternative);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_CTO(ZString intendedUseCode, ZString category)
		{
			if (intendedUseCode == HCIntendedUseCode.Codes.HC01 && new ZString[] { HCCategories.Codes.HC26, HCCategories.Codes.HC27, HCCategories.Codes.HC28 }.Contains(category))
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5004);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5005);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_CPR(ZString intendedUseCode, ZString category)
		{
			if (intendedUseCode == HCIntendedUseCode.Codes.HC21 &&
				new ZString[] {
					HCCategories.Codes.HC29,
					HCCategories.Codes.HC30,
					HCCategories.Codes.HC31,
					HCCategories.Codes.HC32,
					HCCategories.Codes.HC33,
					HCCategories.Codes.HC34,
					HCCategories.Codes.HC35,
					HCCategories.Codes.HC37 }.Contains(category) ||
				new ZString[] {
					HCIntendedUseCode.Codes.HC22,
					HCIntendedUseCode.Codes.HC23,
					HCIntendedUseCode.Codes.HC24,
					HCIntendedUseCode.Codes.HC26,
					HCIntendedUseCode.Codes.HC27 }.Contains(intendedUseCode) &&
				category == HCCategories.Codes.HC37)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5007, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5037, DocumentTypeRequieredType.Optional);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5038, DocumentTypeRequieredType.Optional);
			}
			else if (intendedUseCode == HCIntendedUseCode.Codes.HC21 && category == HCCategories.Codes.HC36)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5006);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_DSE(ZString intendedUseCode, ZString category)
		{
			if (category == HCCategories.Codes.HC04)
			{
				switch (intendedUseCode)
				{
					case HCIntendedUseCode.Codes.HC01:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5009);
						break;
					case HCIntendedUseCode.Codes.HC02:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5008);
						break;
				}
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_HDR(ZString intendedUseCode, ZString category)
		{
			switch (intendedUseCode)
			{
				case HCIntendedUseCode.Codes.HC01:
					switch (category)
					{
						case HCCategories.Codes.HC05:
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5010);
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5011);
							break;
						case HCCategories.Codes.HC06:
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5010);
							break;
					}
					break;
				case HCIntendedUseCode.Codes.HC02:
					if (category == HCCategories.Codes.HC05)
					{
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5012);
					}
					break;
				case HCIntendedUseCode.Codes.HC05:
					switch (category)
					{
						case HCCategories.Codes.HC07:
						case HCCategories.Codes.HC08:
						case HCCategories.Codes.HC09:
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5013);
							break;
						case HCCategories.Codes.HC10:
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5011);
							break;
					}
					break;
				case HCIntendedUseCode.Codes.HC31:
					if (category == HCCategories.Codes.HC05 || category == HCCategories.Codes.HC06)
					{
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5010);
					}
					break;
			}
		}

		#region OCS

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS(ZString intendedUseCode, ZString category)
		{
			switch (intendedUseCode)
			{
				case HCIntendedUseCode.Codes.HC01:
					return GetDefaultDocumentTypes_HC_OCS_HC01(category);
				case HCIntendedUseCode.Codes.HC05:
					return GetDefaultDocumentTypes_HC_OCS_HC05(category);
				case HCIntendedUseCode.Codes.HC15:
					return GetDefaultDocumentTypes_HC_OCS_HC15(category);
				case HCIntendedUseCode.Codes.HC02:
					return GetDefaultDocumentTypes_HC_OCS_HC02(category);
				case HCIntendedUseCode.Codes.HC10:
					return GetDefaultDocumentTypes_HC_OCS_HC10(category);
				case HCIntendedUseCode.Codes.HC16:
				case HCIntendedUseCode.Codes.HC17:
					return GetDefaultDocumentTypes_HC_OCS_HC16OrHC17(category);
				case HCIntendedUseCode.Codes.HC18:
					return GetDefaultDocumentTypes_HC_OCS_HC18(category);
				case HCIntendedUseCode.Codes.HC19:
					return GetDefaultDocumentTypes_HC_OCS_HC19(category);
				case HCIntendedUseCode.Codes.HC13:
					return GetDefaultDocumentTypes_HC_OCS_HC13(category);
				case HCIntendedUseCode.Codes.HC20:
					return GetDefaultDocumentTypes_HC_OCS_HC20(category);
				case HCIntendedUseCode.Codes.HC28:
					return GetDefaultDocumentTypes_HC_OCS_HC28(category);
			}

			return Enumerable.Empty<IRequiredDocumentType>();
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC01(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC18:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					var lpcoCategory = new RequiredDocumentTypeCategory();
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044, DocumentTypeRequieredType.Alternative, lpcoCategory);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5017, DocumentTypeRequieredType.Alternative, lpcoCategory);
					break;
				case HCCategories.Codes.HC19:
				case HCCategories.Codes.HC20:
				case HCCategories.Codes.HC22:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044);
					break;
				case HCCategories.Codes.HC24:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC05(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC18:
				case HCCategories.Codes.HC19:
				case HCCategories.Codes.HC20:
				case HCCategories.Codes.HC21:
				case HCCategories.Codes.HC22:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044);
					break;
				case HCCategories.Codes.HC24:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
					break;
				case HCCategories.Codes.HC25:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5016);
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC15(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC18:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044);
					break;
				case HCCategories.Codes.HC19:
				case HCCategories.Codes.HC20:
				case HCCategories.Codes.HC21:
				case HCCategories.Codes.HC22:
					var firstCategory = new RequiredDocumentTypeCategory(DocumentTypeRequieredType.Alternative);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018, DocumentTypeRequieredType.Mandatory, firstCategory);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044, DocumentTypeRequieredType.Mandatory, firstCategory);
					var secondCategory = new RequiredDocumentTypeCategory(DocumentTypeRequieredType.Alternative);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5015, DocumentTypeRequieredType.Mandatory, secondCategory);
					break;
				case HCCategories.Codes.HC23:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5043);
					break;
				case HCCategories.Codes.HC24:
					var lpcoCategory = new RequiredDocumentTypeCategory();
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040, DocumentTypeRequieredType.Alternative, lpcoCategory);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5014, DocumentTypeRequieredType.Alternative, lpcoCategory);
					break;
				case HCCategories.Codes.HC25:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5016, DocumentTypeRequieredType.Alternative);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5014, DocumentTypeRequieredType.Alternative);
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC02(ZString category)
		{
			if (new ZString[] { HCCategories.Codes.HC19, HCCategories.Codes.HC20, HCCategories.Codes.HC22 }.Contains(category))
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC10(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC19:
				case HCCategories.Codes.HC20:
				case HCCategories.Codes.HC22:
				case HCCategories.Codes.HC23:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044);
					break;
				case HCCategories.Codes.HC24:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC16OrHC17(ZString category)
		{
			if (category == HCCategories.Codes.HC23)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5041);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC18(ZString category)
		{
			if (category == HCCategories.Codes.HC23)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5042, DocumentTypeRequieredType.Optional);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC19(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC23:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5042);
					break;
				case HCCategories.Codes.HC24:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC13(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC24:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
					break;
				case HCCategories.Codes.HC25:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5016);
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC20(ZString category)
		{
			if (category == HCCategories.Codes.HC24)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_OCS_HC28(ZString category)
		{
			switch (category)
			{
				case HCCategories.Codes.HC18:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					var lpcoCategory = new RequiredDocumentTypeCategory();
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044, DocumentTypeRequieredType.Alternative, lpcoCategory);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5017, DocumentTypeRequieredType.Alternative, lpcoCategory);
					break;
				case HCCategories.Codes.HC19:
				case HCCategories.Codes.HC20:
				case HCCategories.Codes.HC21:
				case HCCategories.Codes.HC22:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5044);
					break;
				case HCCategories.Codes.HC23:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5041);
					break;
				case HCCategories.Codes.HC24:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5018);
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5040);
					break;
				case HCCategories.Codes.HC25:
					yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5016);
					break;
			}
		}

		#endregion

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_MDE(ZString intendedUseCode, ZString category)
		{
			switch (intendedUseCode)
			{
				case HCIntendedUseCode.Codes.HC01:
					switch (category)
					{
						case HCCategories.Codes.HC11:
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5020);
							break;
						case HCCategories.Codes.HC12:
						case HCCategories.Codes.HC13:
						case HCCategories.Codes.HC14:
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5019);
							yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5020);
							break;
					}
					break;
				case HCIntendedUseCode.Codes.HC03:
					if (new ZString[] { HCCategories.Codes.HC12, HCCategories.Codes.HC13, HCCategories.Codes.HC14 }.Contains(category))
					{
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5021);
					}
					break;
				case HCIntendedUseCode.Codes.HC02:
				case HCIntendedUseCode.Codes.HC04:
					if (category == HCCategories.Codes.HC13 || category == HCCategories.Codes.HC14)
					{
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5021);
					}
					break;
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_NHP(ZString intendedUseCode, ZString category)
		{
			if (category == HCCategories.Codes.HC15)
			{
				switch (intendedUseCode)
				{
					case HCIntendedUseCode.Codes.HC01:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5022);
						var lpcoCategory = new RequiredDocumentTypeCategory();
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5023, DocumentTypeRequieredType.Alternative, lpcoCategory);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5024, DocumentTypeRequieredType.Alternative, lpcoCategory);
						break;
					case HCIntendedUseCode.Codes.HC05:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5025);
						break;
					case HCIntendedUseCode.Codes.HC02:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5045);
						break;
				}
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_PES(ZString intendedUseCode, ZString category)
		{
			if (category == HCCategories.Codes.HC38 || category == HCCategories.Codes.HC39)
			{
				switch (intendedUseCode)
				{
					case HCIntendedUseCode.Codes.HC06:
					case HCIntendedUseCode.Codes.HC09:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5026);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5027, DocumentTypeRequieredType.Optional);
						break;
					case HCIntendedUseCode.Codes.HC07:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5028, DocumentTypeRequieredType.Alternative);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5029, DocumentTypeRequieredType.Alternative);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5027, DocumentTypeRequieredType.Optional);
						break;
					case HCIntendedUseCode.Codes.HC08:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5030);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5027, DocumentTypeRequieredType.Optional);
						break;
				}
			}
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_RED()
		{
			yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5031);
		}

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_HC_VET(ZString intendedUseCode, ZString category)
		{
			if (category == HCCategories.Codes.HC16)
			{
				switch (intendedUseCode)
				{
					case HCIntendedUseCode.Codes.HC10:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5032);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5039);
						break;
					case HCIntendedUseCode.Codes.HC11:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5034);
						break;
					case HCIntendedUseCode.Codes.HC14:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5035);
						break;
					case HCIntendedUseCode.Codes.HC12:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5036);
						break;
				}
			}
			else if (category == HCCategories.Codes.HC17 && intendedUseCode == HCIntendedUseCode.Codes.HC10)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5033);
			}
		}

		#endregion

		#region NRCan

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_NRCan(ZString programCode)
		{
			if (programCode == NRCanPGADepartmentCodes.Codes.EXP)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._3001, DocumentTypeRequieredType.Alternative);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._3002, DocumentTypeRequieredType.Alternative);
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._3003, DocumentTypeRequieredType.Alternative);
			}
			else if (programCode == NRCanPGADepartmentCodes.Codes.RDA)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._3004);
			}
		}

		#endregion

		#region TC

		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_TC(IPGAHeader pgaHeader, ZString programCode)
		{
			var tcPGAHeader = pgaHeader as TCPGAHeader;

			if (tcPGAHeader != null && programCode == TCPGADepartmentCodes.Codes.VPR)
			{
				switch (tcPGAHeader.CA_SubProgram)
				{
					case TCPGAVehicleProgramCodes.Codes.VCC:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4002);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4004);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4003, DocumentTypeRequieredType.Optional);
						break;
					case TCPGAVehicleProgramCodes.Codes.VFS:
					case TCPGAVehicleProgramCodes.Codes.VCR:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4004);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4003, DocumentTypeRequieredType.Optional);
						break;
					case TCPGAVehicleProgramCodes.Codes.VFC:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4004);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4002);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4003, DocumentTypeRequieredType.Optional);
						break;
					case TCPGAVehicleProgramCodes.Codes.VAE:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4004);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4001, DocumentTypeRequieredType.Optional);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4003, DocumentTypeRequieredType.Optional);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4005, DocumentTypeRequieredType.Optional);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4006, DocumentTypeRequieredType.Optional);
						break;
					case TCPGAVehicleProgramCodes.Codes.VUV:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4004, DocumentTypeRequieredType.Alternative);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4006, DocumentTypeRequieredType.Alternative);
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4001, DocumentTypeRequieredType.Alternative);
						break;
					case TCPGAVehicleProgramCodes.Codes.VVP:
						yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._4004);
						break;
				}
			}
		}

		#endregion

		#region PHAC
		static IEnumerable<IRequiredDocumentType> GetDefaultDocumentTypes_PHAC(IPGAHeader pgaHeader, ZString programCode)
		{
			var phacPGAHeader = pgaHeader as PHACPGAHeader;
			if (phacPGAHeader != null && programCode == PHACPGADepartmentCodes.Codes.HAP)
			{
				yield return new RequiredDocumentType(LPCODocumentTypeQualifier.Codes._5503, DocumentTypeRequieredType.Optional);
			}
		}
		#endregion
	}
}
