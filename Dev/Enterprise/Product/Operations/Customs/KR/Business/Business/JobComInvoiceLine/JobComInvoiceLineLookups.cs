using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using CodeList = Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class JobComInvoiceLineLookups : AutoKRJobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public new JobComInvoiceLine InvoiceLine => Parent;

		public override ICodeDescriptionPairList PrimaryPreferenceList
		{
			get
			{
				if (InvoiceLine.IsExport)
				{
					var result = new CodeDescriptionPairList();
					if (InvoiceLine.JI_CountryOfOrigin == Core.Constants.CountryCodes.KoreaSouth && InvoiceLine.CertificateOfOriginIssueStatus == Messaging.CertificateOfOriginIssuedCodeList.Codes.Y)
					{
						foreach (ZZRefCusCodeListCombined code in ExportFTAList)
						{
							result.AddPair(code.ZZD_Code, code.ZZD_Description);
						}
					}
					return result;
				}
				return UniversalReferenceDataHelper.GetPreferenceList(Factory, true, InvoiceLine.JI_Tariff, InvoiceLine.JI_CountryOfOrigin, InvoiceLine.UniversalTariff, InvoiceLine.DutyRateSelectionCriteria, Core.Constants.CountryCodes.KoreaSouth);
			}
		}

		public TariffViewCollection SecondaryPreferences => TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DutyReductionExemption, InvoiceLine.EffectiveAssessmentDate);

		public CodeDescriptionPairList CertificateOfOriginIssuedCodeList => Factory.GetCachedValue<CertificateOfOriginIssuedCodeList>();
		public CodeDescriptionPairList CountryOfOriginDeterminationRuleCodeList => Factory.GetCachedValue<CountryOfOriginDeterminationRuleCodeList>();
		public CodeDescriptionPairList CertificateOfOriginSplitCodeList => Factory.GetCachedValue<CertificateOfOriginSplitCodeList>();
		public override CodeDescriptionPairList InvoiceUQList => (InvoiceLine.Declaration?.IsPersonalItemDeclaration ?? false) ? Factory.GetCachedValue<PersonalItemCodeList>() : Factory.GetCachedValue<InvoiceUnitQuantityCodeList>();

		public ZZRefCusCodeListCombinedCollection ExportFTAList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
							InvoiceLine.Factory,
							null,
							Core.Constants.CountryCodes.KoreaSouth,
							new ZString[] { Constants.ZZ.NKCodeType.EXFTA },
							InvoiceLine.EffectiveAssessmentDate,
							InvoiceLine.Declaration.JE_GoodsDestination.IsEmpty ? null : GetAttributeFiltersOnDestinationCountry());
				result.Load();
				result.Sort(ZZRefCusCodeListCombined.Schema.ZZD_Code);
				return result;
			}
		}

		IEnumerable<RefCusCodeListAttributeFilter> GetAttributeFiltersOnDestinationCountry()
		{
			var tradeGroupCountryFilter = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup);
			tradeGroupCountryFilter.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, InvoiceLine.Declaration.JE_GoodsDestination);
			tradeGroupCountryFilter.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_StartDate, SQLComparisonOperator.LessThanOrEqualTo, InvoiceLine.EffectiveAssessmentDate);
			tradeGroupCountryFilter.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, InvoiceLine.EffectiveAssessmentDate);

			var tradeGroupFilter = new ZDBOnlyQuery(typeof(CusRefTradeGroupView));
			tradeGroupFilter.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.KoreaSouth);
			tradeGroupFilter.AddToFilter(CusRefTradeGroupViewSchema.ZZA_StartDate, SQLComparisonOperator.LessThanOrEqualTo, InvoiceLine.EffectiveAssessmentDate);
			tradeGroupFilter.AddToFilter(CusRefTradeGroupViewSchema.ZZA_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, InvoiceLine.EffectiveAssessmentDate);
			tradeGroupFilter.AddSubQuery(CusRefTradeGroupViewSchema.PK, CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup, tradeGroupCountryFilter, JoinCondition.And);

			var tradeGroups = Factory.Load<CusRefTradeGroupView>(tradeGroupFilter);
			if (tradeGroups.Length > 0)
			{
				var tradeGroupCodes = tradeGroups.Select(x => x.ZZA_TradeGroup).ToArray();
				yield return new RefCusCodeListAttributeFilter(Constants.ZZ.CodeListAttributeNames.FTATradeGroup, JoinCondition.Or, tradeGroupCodes);
			}
		}

		public CodeDescriptionPairList LocalExportDocumentTypeList => Factory.GetCachedValue<LocalExportDocumentTypeList>();
		public CodeDescriptionPairList CourierCargoSelectivityIndicatorCodeList => Factory.GetCachedValue<CourierCargoSelectivityIndicatorCodeList>();

		public CodeDescriptionPairList EntryInstructionList => InvoiceLine.Declaration?.CustomsEntryInstructionProvider.SortedEntryInstructionList ?? new CodeDescriptionPairList();

		public ZZRefCusCodeListCombinedCollection CustomsOfficeList
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.NKCodeType.CustomsOffice, ZDateTime.Today);

		public CodeDescriptionPairList PostClearanceYNCodeList => Factory.GetCachedValue<CodeList.YesNoList>();

		[OrganisationDefaultProvider(OrganisationType = OrganisationTypes.Consignee)]
		public ConsigneeCollection ConsigneeAddress
		{
			get
			{
				if (consigneeAddress == null)
				{
					consigneeAddress = new ConsigneeCollection(Factory);
				}

				return consigneeAddress;
			}
		}
		ConsigneeCollection consigneeAddress;

		public CodeDescriptionPairList ReductionRateRegulationList => Factory.GetCachedValue<ImportReductionRateRegulationList>();

		public CodeDescriptionPairList DutyReductionClassificationList => Factory.GetCachedValue<ImportDutyReductionClassificationList>();

		public ICollection GoodsDestination => new RefCountryCollection(Factory);
		public CodeDescriptionPairList DutyRateCodeList => Factory.GetCachedValue<DutyRateCodeList>();

		public CodeDescriptionPairList CertificateOfOriginProductTypeCodeList => Factory.GetCachedValue<CertificateOfOriginProductTypeCodeList>();
		public CodeDescriptionPairList CertifiticateOfOriginIssuedTypeList => Factory.GetCachedValue<CertifiticateOfOriginIssuedTypeList>();
		public CodeDescriptionPairList CountryOfOriginSupportingDocTypeCodeList => Factory.GetCachedValue<CountryOfOriginSupportingDocTypeCodeList>();

		public CodeDescriptionPairList EducationTaxTypeCodeList => Factory.GetCachedValue<EducationTaxTypeCodeList>();
		public CodeDescriptionPairList AgricultureTaxTypeCodeList => Factory.GetCachedValue<AgricultureTaxTypeCodeList>();

		public CodeDescriptionPairList DomesticTaxClassificationCodeList => Factory.GetCachedValue<DomesticTaxClassificationCodeList>();

		public override CodeDescriptionPairList TaxOrFeeCodeList => Factory.GetCachedValue<VATRateTypeCodeList>();

		public TariffViewCollection DomesticTaxCodes => TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.TariffTypes.DomesticTaxRate, InvoiceLine.EffectiveAssessmentDate);
		public ZZRefCusCodeListCombinedCollection InstallmentCodes => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.InstallmentCode, InvoiceLine.EffectiveAssessmentDate);
		public ZZRefCusCodeListCombinedCollection BrandCodes => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.BrandCode, InvoiceLine.EffectiveAssessmentDate);
		public TariffViewCollection TaxExemptionCodes
		{
			get
			{
				var key = string.Format(CultureInfo.InvariantCulture, "RefCusTariffCollectionTaxExemption_{0}", InvoiceLine.EffectiveAssessmentDate);
				return Factory.GetCachedValue(key, () => GetTaxReductionExamptionCodes(InvoiceLine.JI_DomesticTaxExemptionCode, Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption, InvoiceLine.DomesticTaxClassification.IsEmpty ?
					new string[] { Messaging.Constants.ZZ.TariffAttributes.SpecialConsumptionTax, Messaging.Constants.ZZ.TariffAttributes.LiquorTax, Messaging.Constants.ZZ.TariffAttributes.TransportationTax } : new string[] { InvoiceLine.DomesticTaxClassification }));
			}
		}

		public TariffViewCollection VATReductionCodes
		{
			get
			{
				var key = string.Format(CultureInfo.InvariantCulture, "RefCusTariffCollectionVATReduction_{0}", InvoiceLine.EffectiveAssessmentDate);
				return Factory.GetCachedValue(key, () => GetTaxReductionExamptionCodes(InvoiceLine.JI_VATReductionCode, Messaging.Constants.ZZ.TariffTypes.DomesticTaxReductionExemption, new string[] { Messaging.Constants.ZZ.TariffAttributes.ValueAddedTax }));
			}
		}

		TariffViewCollection GetTaxReductionExamptionCodes(ZString taxCode, string tariffType, string[] taxTypes)
		{
			var query = TariffViewCollection.GetLoadingQuery(Factory, Core.Constants.CountryCodes.KoreaSouth, tariffType, InvoiceLine.EffectiveAssessmentDate);
			var attributeQuery = new ZDBOnlySubQuery(typeof(RefCusTariffAttribute), RefCusTariffAttributeSchema.ZZ3_ZZ1_Tariff);
			attributeQuery.AddToFilter(RefCusTariffAttributeSchema.ZZ3_Name, Messaging.Constants.ZZ.TariffAttributeNames.TaxCode);
			attributeQuery.AddToFilter(RefCusTariffAttributeSchema.ZZ3_Value, taxTypes);
			query.AddSubQuery(attributeQuery, JoinCondition.And);
			var result = new TariffViewCollection(Factory, query);
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffCode, "Property", taxCode, true));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffType, "Property1", (ZString)Core.Constants.CountryCodes.KoreaSouth, false));
			result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.TariffType, "Property2", (ZString)tariffType, false));
			if (InvoiceLine.EffectiveAssessmentDate.IsValid)
			{
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.EffectiveDate, "Property1", InvoiceLine.EffectiveAssessmentDate));
			}
			return result;
		}

		public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<YesNoList>();
		public CodeDescriptionPairList PackageKindCodeList => Factory.GetCachedValue<PackageKindCodeList>();
		public CodeDescriptionPairList AdditionalDutyTypeCodeList => Factory.GetCachedValue<AdditionalDutyTypeCodeList>();
		public CodeDescriptionPairList SkipManifestReportingCodeList => Factory.GetCachedValue<SkipManifestReportingCodeList>();
		public CodeDescriptionPairList CountryOfOriginLabelLocationCodeList => Factory.GetCachedValue<CountryOfOriginLabelLocationCodeList>();
		public CodeDescriptionPairList OriginalStateDocTypeList => Factory.GetCachedValue<OriginalStateDocTypeList>();
		public CodeDescriptionPairList SpecificUseProductTypeList => Factory.GetCachedValue<SpecificUseProductTypeList>();
		public CodeDescriptionPairList CountryOfOriginLabelTypeCodeList => Factory.GetCachedValue<CountryOfOriginLabelTypeCodeList>();
		public CodeDescriptionPairList CountryOfOriginExemptionReasonCodeList => Factory.GetCachedValue<CountryOfOriginExemptionReasonCodeList>();

		public ZZRefCusCodeListCombinedCollection OtherGovernmentAndAssociatedAgencyList => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Messaging.Constants.ZZ.NKCodeType.OtherGovernmentAndAssociatedAgency, InvoiceLine.EffectiveAssessmentDate);
		public CodeDescriptionPairList ProductOrMaterialCodeList => (InvoiceLine.InvoiceHeader?.Is008 ?? false) ? Factory.GetCachedValue<PersonalItemCategoryCodeList>() : Factory.GetCachedValue<ProductOrMaterialCodeList>();
		public CodeDescriptionPairList MightRequireInspectionIndicatorCodeList => Factory.GetCachedValue<MightRequireInspectionIndicatorCodeList>();
		public CodeDescriptionPairList InvoiceUnitQuantityCodeList => Factory.GetCachedValue<InvoiceUnitQuantityCodeList>();
		public CodeDescriptionPairList DutyRateSelectionList
		{
			get
			{
				return Factory.GetCachedValue(InvoiceLine.UniversalDutyRate?.PK ?? ZGuid.Empty, () =>
				{
					var result = new CodeDescriptionPairList();
					if (InvoiceLine.UniversalDutyRate != null)
					{
						var list = InvoiceLine.UniversalDutyRate.FilteredRateApplicabilities.Where(x => Constants.ZZ.ApplicabilityAdditionalCodes.Max == x.ZZT_AdditionalCode || Constants.ZZ.ApplicabilityAdditionalCodes.Min == x.ZZT_AdditionalCode).Select(x => x.ZZT_AdditionalCode).Distinct();
						foreach (var item in list)
						{
							result.AddPair(item);
						}
					}
					return result;
				});
			}
		}
	}
}
