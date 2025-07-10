//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoECCCPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoECCCPGAHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ECCCPGAHeaderAddInfoLookups : AutoECCCPGAHeaderAddInfoLookups
	{
		public ECCCPGAHeaderAddInfoLookups(AutoECCCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		public ECCCPGAHeader ECCCHeader
		{
			get { return (ECCCPGAHeader)(((ECCCPGAHeaderAddInfo)Parent).Parent); }
		}

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<ECCCPGADepartmentCodes>(); }
		}

		public CodeDescriptionPairList IntendedUseCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				foreach (var programCode in ProgramCodesList.GetAllCodes())
				{
					if (ECCCHeader.IsProgramEnabled(programCode))
					{
						result.AddRange(IntendedUseCodes.GetIntendedCodeForProgram(programCode));
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList LifeStages
		{
			get { return Factory.GetCachedValue<LifeStageCodes>(); }
		}

		public CodeDescriptionPairList SexList
		{
			get { return Factory.GetCachedValue<SexCodes>(); }
		}

		public CodeDescriptionPairList SourceOfSpecimenList
		{
			get { return ECCCProductCategories.GetProductCategoriesForProgram(ECCCPGADepartmentCodes.Codes.WEN); }
		}

		public CodeDescriptionPairList UQList
		{
			get { return Factory.GetCachedValue<UnitOfIngredientQuantity>(); }
		}

		public CodeDescriptionPairList ProcessCodeList
		{
			get { return ProcessCodes.GetProcessCodesFor(((IPGAHeader)ECCCHeader).GovAgencyIDCode, Factory); }
		}

		public CodeDescriptionPairList EngineModelYearList
		{
			get { return Factory.GetCachedValue("ECCCEngineModelYearList", () => YearListHelper.GetYearList(ZDateTime.Today.Year + 2)); }
		}

		public CodeDescriptionPairList MachineModelYearList
		{
			get { return Factory.GetCachedValue("ECCCMachineModelYearList", () => YearListHelper.GetYearList(ZDateTime.Today.Year + 2)); }
		}

		public CodeDescriptionPairList VehicleClassList => ECCCProductCategories.GetVehicleClassList(ECCCHeader.CA_ProcessCode);

		public CodeDescriptionPairList EngineClassList => ECCCProductCategories.GetEngineClassList(ECCCHeader.CA_ProcessCode);

		public OrgHeaderCollection Manufacturers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public OrgHeaderCollection AllOrganisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public CodeDescriptionPairList PowerRatingUQList
		{
			get { return Factory.GetCachedValue<PowerRatingCodes>(); }
		}

		public CodeDescriptionPairList AOSConformityCodeList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("AOSConformityCodeList" + date, () =>
				{
					var list = new CodeDescriptionPairList();
					var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.AOSConformity, SQLComparisonOperator.Equal, YesNoList.Codes.Yes);
					var codes = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, RefCusCodeListTypes.Codes.ECCCComplianceStatement, date, new[] { attributeFilter });
					list.AddRange(codes);
					list.Sort();
					return list;
				});
			}
		}

		public CodeDescriptionPairList AOSReplacementCodeList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("AOSReplacementCodeList" + date, () =>
				{
					var list = new CodeDescriptionPairList();
					var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.AOSReplacement, SQLComparisonOperator.Equal, YesNoList.Codes.Yes);
					var codes = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, RefCusCodeListTypes.Codes.ECCCComplianceStatement, date, new[] { attributeFilter });
					list.AddRange(codes);
					list.Sort();
					return list;
				});
			}
		}

		public CodeDescriptionPairList AOSEvidenceCodeList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("AOSEvidenceCodeList" + date, () =>
				{
					var list = new CodeDescriptionPairList();
					var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.AOSEvidence, SQLComparisonOperator.Equal, YesNoList.Codes.Yes);
					var codes = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, RefCusCodeListTypes.Codes.ECCCComplianceStatement, date, new[] { attributeFilter });
					list.AddRange(codes);
					list.Sort();
					return list;
				});
			}
		}

		public CodeDescriptionPairList AOSRetentionCodeList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("AOSRetentionCodeList" + date, () =>
				{
					var list = new CodeDescriptionPairList();
					var attributeFilter = new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.AOSRetention, SQLComparisonOperator.Equal, YesNoList.Codes.Yes);
					var codes = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, RefCusCodeListTypes.Codes.ECCCComplianceStatement, date, new[] { attributeFilter });
					list.AddRange(codes);
					list.Sort();
					return list;
				});
			}
		}

		public CodeDescriptionPairList AlternativeStandardOfEngineClassCodeList
		{
			get
			{
				var date = ZDateTime.Today;
				return Factory.GetCachedValue("AlternativeStandardOfEngineClass" + date, () =>
				{
					var list = new CodeDescriptionPairList();
					var codes = Enterprise.Customs.Universal.ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Canada, RefCusCodeListTypes.Codes.ECCCAlternativeStandardConformityStatements, date);
					list.AddRange(codes);
					list.Sort();
					return list;
				});
			}
		}
	}
}
