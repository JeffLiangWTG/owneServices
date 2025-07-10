using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public abstract class BaseGuidedDecisionMakingSource : IGuidedDecisionMakingSource
	{
		public BaseGuidedDecisionMakingSource(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		bool IsUCC6 => ((IUcc6ValueProvider)invoiceLine).IsUCC6;

		public ZString DataGrouping => invoiceLine.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff);

		public ZString ParentDataGrouping => RefDataGrouping.GetParentDataGroupingCode(invoiceLine.Factory, DataGrouping);

		public ZString UserLanguage => GlbStaff.CurrentUser.Language;

		public ZString DutyRateTypeCode => Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty;

		public ZBool AllowQuickAdditionalCodeScreen => false;

		public ZBool AllowQuickConditionScreen => false;

		public ZBool IsImport => invoiceLine.IsImport;

		public ZBool IsExport => invoiceLine.IsExport;

		public ZDate EffectiveDate => (invoiceLine.EffectiveAssessmentDate.IsEmpty ? ZDateTime.Today : invoiceLine.EffectiveAssessmentDate).Date;

		public ZString TariffCode => invoiceLine.JI_Tariff;

		public ZString CountryCode => invoiceLine.Declaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public ZString CountryOfOrigin => (invoiceLine.Lookups.CountryOfOrigins as ICodeDescriptionPairList)?.ContainsCode(invoiceLine.JI_CountryOfOrigin) ?? true ? invoiceLine.JI_CountryOfOrigin : ZString.Empty;

		public ZString Preference
		{
			get
			{
				var list = invoiceLine.Lookups.PrimaryPreferenceList;
				if (list.Count == 1)
				{
					return (list as CodeDescriptionPairList).CodesAsString;
				}

				return list.ContainsCode(invoiceLine.JI_PrimaryPreference) ? invoiceLine.JI_PrimaryPreference : ZString.Empty;
			}
		}

		public ZString QuotaOrderNumber
		{
			get
			{
				var list = invoiceLine.Lookups.OrderNumbersList;
				if (list.Count == 1)
				{
					return list.CodesAsString;
				}

				return list.ContainsCode(invoiceLine.JI_ConcessionOrder) ? invoiceLine.JI_ConcessionOrder : ZString.Empty;
			}
		}

		public ZDecimal CustomsFirstQuantity => invoiceLine.CustomsFirstQuantityInKG;

		public ZDecimal CustomsSecondQuantity => invoiceLine.JI_CustomsSecondUnitQty == UOM ? invoiceLine.JI_CustomsSecondQuantity : ZDecimal.Zero;

		public ZString CustomsSecondUnitQty => UOM;

		ZString UOM => invoiceLine.UniversalTariff?.UnitsOfMeasure?.FirstOrDefault(a => a.ZZ8_Type == UOMTypeList.Codes.CU2 && CusTradeGroupIsMatched(a.CusTradeGroup, EffectiveTradeGroupCountry))?.ZZ8_UOM ?? ZString.Empty;

		public ZDecimal CustomsThirdQuantity => invoiceLine.JI_CustomsThirdQuantity;

		public ZString CustomsThirdUnitQty => invoiceLine.JI_CustomsThirdUnitQty;

		bool CusTradeGroupIsMatched(CusRefTradeGroupView cusTradeGroup, ZString country) => cusTradeGroup == null || cusTradeGroup.GetApplicableTradeGroupCountries(EffectiveDate).Any(b => b.ZZB_RN_NKTradeGroupCountryCode == country);

		public ZString TariffType => invoiceLine.UniversalTariffType;

		public IEnumerable<(ZString Code, ZString Reference, ZDateTime DateOfIssue)> SupportingAndAdditionalDocuments
		{
			get
			{
				var documents = invoiceLine.SupportingDocuments.Select<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue))
					.Union(invoiceLine.InvoiceHeader.SupportingDocuments.Select<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue)))
					.Union(invoiceLine.Declaration?.SupportingDocuments.Select<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue)) ?? Enumerable.Empty<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>());

				if (IsUCC6)
				{
					documents = documents.Union(invoiceLine.AdditionalInfos.Select<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue)))
						.Union(invoiceLine.InvoiceHeader.AdditionalInfos.Select<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue)))
						.Union(invoiceLine.Declaration?.AdditionalInfos.Select<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>(x => (x.CSI_Code, x.CSI_ReferenceNumber, x.CSI_DateOfIssue)) ?? Enumerable.Empty<(ZString Code, ZString Reference, ZDateTime DateOfIssue)>());
				}

				return documents;
			}
		}

		public IEnumerable<ZString> SupplementaryCodes => invoiceLine.SupplementaryCodes.Select(x => x.CY_Code);

		public ZString VATCode => invoiceLine.JI_ZZF_NKTaxType;

		public ZString EffectiveCountryOfDestination => (invoiceLine.Lookups.CountryOfOrigins as ICodeDescriptionPairList)?.ContainsCode(invoiceLine.EffectiveCountryOfDestination) ?? true ? invoiceLine.EffectiveCountryOfDestination : ZString.Empty;

		ZString EffectiveTradeGroupCountry => IsExport ? EffectiveCountryOfDestination : CountryOfOrigin;

		public IBusiness ParentBusinessObject => invoiceLine;

		public abstract ZBool IsCustomsFirstQuantityReadOnly { get; }
		public abstract ZBool IsCustomsSecondQuantityReadOnly { get; }
		public abstract ZBool IsCustomsThirdQuantityReadOnly { get; }
	}
}
