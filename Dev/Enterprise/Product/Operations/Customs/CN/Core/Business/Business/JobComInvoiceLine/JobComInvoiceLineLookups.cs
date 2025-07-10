using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> CustomsEntryInstructions
		{
			get
			{
				var declaration = InvoiceLine?.Declaration;
				if (declaration == null)
				{
					return null;
				}

				ICusEntryInstructionCollection<CusEntryInstruction> result;
				if (declaration.WillGenerateBothEntries)
				{
					result = new SubsetCusEntryInstructionCollection(declaration, x => !x.IsChild);
				}
				else
				{
					result = declaration.CustomsEntryInstructionProvider?.CustomsEntryInstructions;
				}

				return result;
			}
		}

		public override ICodeDescriptionPairList PrimaryPreferenceList => UniversalReferenceDataHelper.GetPreferenceListByCountry(Factory, Core.Constants.CountryCodes.China);

		public override CodeDescriptionPairList CustomsUQList => Factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China);

		public UNDGSubstanceCollection UNDGSubs
		{
			get
			{
				if (fUNDGSubs == null)
				{
					fUNDGSubs = new UNDGSubstanceCollection(Factory, new ZQuery(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
				}
				return fUNDGSubs;
			}
		}
		UNDGSubstanceCollection fUNDGSubs;

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			return new OrgSupplierPartCollection(Factory, InvoiceLine, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False);
		}

		public CodeDescriptionPairList TradeAgreementCodeList
		{
			get
			{
				CodeDescriptionPairList result;

				if (Parent.JI_PrimaryPreference == Constants.PrimaryPreferenceCodes.LeastDevelopedCountries)
				{
					result = Factory.GetCachedValue("SecondaryPreference_CN_LDC", GetLeastDevelopedCountriesTradeAgreementCodeList);
				}
				else
				{
					var applicableCountry = Parent.IsImport ? Parent.EffectiveCountryOfOrigin : Parent.JI_RN_NKCountryOfExport;
					result = CNRefCusCodeListTypes.GetApplicablePreferentialTradeAgreements(Factory, Parent.EffectiveAssessmentDate, applicableCountry);
				}
				return result;
			}
		}

		CodeDescriptionPairList GetLeastDevelopedCountriesTradeAgreementCodeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.TradeAgreementCodes.Codes.LDC, Constants.TradeAgreementCodes.Descriptions.LDC);
			return result;
		}

		public CodeDescriptionPairList CertificateOfOriginTypeList => Factory.GetCachedValue<CertificateOfOriginTypeList>();

		public override ICodeDescriptionPairList Procedures => new CodeDescriptionPairList();

		public RefCountryCollection CertificateOfOriginCountryList => new RefCountryCollection(Factory);

		public CodeDescriptionPairList EndUseList => Factory.GetCachedValue<EndUseList>();

		public ZZRefCusCodeListCombinedCollection OrigDistrictList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetDistrictList(Factory, Parent.EffectiveAssessmentDate);

				if (Parent.JI_OriginDistrict.IsEmpty && !Parent.JI_OriginRegion.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JI_OriginRegion.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection DestDistrictList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetDistrictList(Factory, Parent.EffectiveAssessmentDate);

				if (Parent.JI_DestinationDistrict.IsEmpty && !Parent.JI_DestinationRegion.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JI_DestinationRegion.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}

				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection OrigRegionList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetRegionList(Factory, Parent.EffectiveAssessmentDate);

				if (Parent.JI_OriginRegion.IsEmpty && !Parent.JI_OriginDistrict.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JI_OriginDistrict.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection DestRegionList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetRegionList(Factory, Parent.EffectiveAssessmentDate);

				if (Parent.JI_DestinationRegion.IsEmpty && !Parent.JI_DestinationDistrict.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", Parent.JI_DestinationDistrict.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}
				return result;
			}
		}

		public CodeDescriptionPairList DutyModes
		{
			get
			{
				var isImport = Parent?.Declaration?.IsImport ?? false;
				var levyType = Parent?.CustomsEntryOrOnlyInstruction?.CEI_LevyType ?? ZString.Empty;
				var result = Factory.GetCachedValue(
					"CNDutyModeList_" + levyType + (isImport ? "EXP" : ""),
					() => DutyModeList.GetSupportedDutyModesByLevyType(Factory, levyType, isImport)
				);

				return result;
			}
		}

		public ChildTariffViewCollection CIQTariffList
		{
			get
			{
				var result = ChildTariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.China,
					Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff, Parent.EffectiveAssessmentDate,
					Universal.Constants.TariffTypes.HarmonizedSystem, Parent.JI_Tariff);

				if (!Parent.JI_Tariff.IsEmpty && Parent.JI_CIQTariff.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffCode, "Property", Parent.JI_Tariff, true));
				}
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.DefaultLanguageDescription, "Property", ZString.Empty));

				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection CIQOriginStateList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetStatesList(Factory, Parent.EffectiveAssessmentDate);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Code, "Property", new ZString(Parent.CountryOfOrigin?.RN_IsoNumericUNM49Code)));
				return result;
			}
		}

		public CodeDescriptionPairList UNDGPackageTypes => Factory.GetCachedValue<UNDGPackageTypeList>();

		public CodeDescriptionPairList TradeUnitQtyList => Factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China);

		public CodeDescriptionPairList ConfirmationTypeList => Business.ConfirmationTypeList.GetYesAndNoList(Factory);
	}
}
