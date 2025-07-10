using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class JobComInvoiceLineLookups : EU.Business.Declaration.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		protected override CodeDescriptionPairList TaxOrFeeList => Factory.GetCachedValue("FR.JobComInvoiceLineLookups.TaxOrFeeList" + GetVATCacheKey(), () =>
		{
			var list = base.TaxOrFeeList;
			var result = new CodeDescriptionPairList();
			foreach (RefCusTaxOrFee taxorFee in list)
			{
				if (taxorFee.ZZF_ZX0_NKTaxOrFeeType != CustomsChargeTypeList.Codes.OtherCharges)
				{
					result.Add(taxorFee);
				}
			}

			return result;
		});

		protected override IEnumerable<VATApplicabilityView> GetVatApplicabilities()
		{
			return UniversalReferenceDataHelper.GetVATApplicabilitiesWithSecondaryTradeGroup(Factory, Parent.UniversalTariff, GetCustomsCountryCode(), FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined(InvoiceLine.Declaration?.JE_RegionOrTerritoryOfDestination ?? ZString.Empty), GetDateOfValuation());
		}

		protected override ZString GetTaxOrFeeDescription(RefCusTaxOrFee taxorFee, VATApplicabilityView vatApplicability = null)
		{
			return taxorFee.ZZF_Value.ToString("#0.00%", CultureInfo.InvariantCulture);
		}

		public List<ZString> FRAddInRefCusCodeListWithCategoryDCCAttribute => Factory.GetCachedValue("AddinCategoryDCCList_FR", delegate
		{
			var additionalCodes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, ZDateTime.Today);
			return additionalCodes.Where(x => x.HasAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Category, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.DCC)).Select(x => x.ZZD_Code).ToList();
		});

		public override ICodeDescriptionPairList PrimaryPreferenceList
		{
			get
			{
				var parent = Parent;
				var declaration = parent.Declaration;
				var result = (CodeDescriptionPairList)base.PrimaryPreferenceList;

				if (result.Count == 0 && declaration != null)
				{
					result = Factory.GetCachedValue("FR.JobComInvoiceLineLookups.PrimaryPreferenceList." + parent.JI_CountryOfOrigin + "." + declaration.JE_RegionOrTerritoryOfDestination, delegate
					{
						var list = new CodeDescriptionPairList();
						if (parent.JI_CountryOfOrigin != Core.Constants.CountryCodes.France
							&& Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction.Contains(parent.JI_CountryOfOrigin.ToString())
							&& (declaration.JE_RegionOrTerritoryOfDestination == FRDomesticOverseasTerritories.Codes.CONTI || declaration.JE_RegionOrTerritoryOfDestination == FRDomesticOverseasTerritories.Codes.CORSE))
						{
							list.AddPair(Core.Constants.Customs.Universal.RefCusPreference.Codes._100, Core.Constants.Customs.Universal.RefCusPreference.Descriptions._100);
						}

						return list;
					});
				}
				return result;
			}
		}

		protected override ICollection CountryOfOriginsCore()
		{
			var result = UniversalReferenceDataHelper.GetCustomsApprovedCountryList(Factory, Parent.GetDefaultDataGroupingCode());
			result.Load();
			return result;
		}

		protected override ZString[] ConditionTypesToExcludeFromAdditionalCodesList => new ZString[] { FRConstants.RefCusConditionType.Vat };
	}
}
