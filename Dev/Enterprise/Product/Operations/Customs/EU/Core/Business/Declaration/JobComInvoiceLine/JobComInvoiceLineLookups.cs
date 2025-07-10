using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

		public sealed override CodeDescriptionPairList CustomsUQList //sealed. Passing Declaration.GetDefaultDataGroupingCode will take care of loading country-specific UQs. No need to override this and pass a country code.
			=> GetCachedRefCusCodeList(CustomsUQListType, CustomsUQListIncludeParentDataGrouping);

		protected virtual ZString CustomsUQListType => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ;

		protected virtual bool CustomsUQListIncludeParentDataGrouping => true;

		public ZDateTime DateOfValuationForCustomsUQList => GetDateOfValuation();

		public List<TaxOrFeeDetailEntity> TaxOrFeeDetailEntities
		{
			get
			{
				var cacheKey = "EU.JobComInvoiceLineLookups.TaxOrFeeDetailEntities" + GetVATCacheKey();
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var returnList = new List<TaxOrFeeDetailEntity>();
					var vatApplicabilities = GetVatApplicabilities();
					if (vatApplicabilities != null && vatApplicabilities.Any())
					{
						var taxOrFeeDictionary = TaxOrFeeList.Cast<RefCusTaxOrFee>().GroupBy(x => x.ZZF_Code).ToDictionary(x => x.Key.ToUpperInvariant(), y => y.ToArray());
						foreach (var vatApplicability in vatApplicabilities)
						{
							var taxType = vatApplicability.ZX5_ZZF_NKTaxOrFeeCode.ToUpperInvariant();
							var additionalCode = vatApplicability.ZX5_AdditionalCode.ToUpperInvariant();
							var category = vatApplicability.ZX5_VATCategory.ToUpperInvariant();
							var taxOrFeeValue = taxOrFeeDictionary.TryGetValue(taxType, out var taxOrFees) && taxOrFees.Length > 0 ? taxOrFees[0].ZZF_Value.ToString("#0.00%", CultureInfo.InvariantCulture) : string.Empty;
							var description = new ZStringBuilder();
							description.AppendIfNotEmpty(GetTaxTypeDescription(taxType));
							description.AppendIfNotEmpty(additionalCode);
							description.AppendIfNotEmpty(category);
							description.AppendIfNotEmpty(taxOrFeeValue);
							returnList.Add(new TaxOrFeeDetailEntity
							{
								Code = category,
								VATCode = taxType,
								Description = description.ToStringWithDelimiterBetweenAppends(", "),
								AdditionalCode = additionalCode,
								Category = category,
							});
						}
					}
					return returnList;
				});
			}
		}

		protected internal ZString GetVATCacheKey() => $"{Parent.JI_Tariff}-{GetCustomsCountryCode()}-{GetDateOfValuation().ToShortDateString()}-{TaxOrFeeCodeListType}-{Parent.EffectiveAssessmentDate}";

		protected virtual IEnumerable<VATApplicabilityView> GetVatApplicabilities() => Parent.GetEffectiveVATApplicabilities().Where(x => x.ZX5_ZZZ_NKDataGrouping == GetCustomsCountryCode()).OrderBy(x => x.ZX5_ZZF_NKTaxOrFeeCode);

		ZString GetTaxTypeDescription(ZString taxType) => TaxOrFeeList.GetDescriptionFromCode(taxType);

		protected virtual CodeDescriptionPairList TaxOrFeeList
			=> Factory.GetCachedValue("EU.JobComInvoiceLineLookups.TaxOrFeeList" + GetVATCacheKey(), () =>
				{
					return RefCusTaxOrFee.Loader.GetList(Factory, GetCustomsCountryCode(), GetDateOfValuation(), TaxOrFeeCodeListType);
				});

		public override CodeDescriptionPairList TaxOrFeeCodeList
		{
			get
			{
				var returnList = new CodeDescriptionPairList();
				var vatApplicabilities = GetVatApplicabilities();
				if (vatApplicabilities == null || !vatApplicabilities.Any())
				{
					returnList = TaxOrFeeList;
				}
				else
				{
					foreach (RefCusTaxOrFee taxorFee in TaxOrFeeList)
					{
						var vatApplicability = vatApplicabilities.FirstOrDefault(x => x.ZX5_ZZF_NKTaxOrFeeCode == taxorFee.ZZF_Code);
						if (vatApplicability != null)
						{
							var formattedDescription = GetTaxOrFeeDescription(taxorFee, vatApplicability);

							returnList.AddPair(taxorFee.ZZF_Code, formattedDescription);
						}
					}
				}

				return returnList;
			}
		}

		protected virtual ZString GetTaxOrFeeDescription(RefCusTaxOrFee taxorFee, VATApplicabilityView vatApplicability)
		{
			var additionalCode = vatApplicability.ZX5_AdditionalCode;

			var descriptionAdditionalCode = GetCachedRefCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation).GetDescriptionFromCode(additionalCode);
			var taxFeeCodeDescription = (ZString)FormattableString.Invariant($" - {additionalCode} - {descriptionAdditionalCode}");
			var formattedDescription = taxorFee.ZZF_Description + (!additionalCode.IsEmpty ? taxFeeCodeDescription : ZString.Empty);
			return formattedDescription;
		}

		protected virtual ZString TaxOrFeeCodeListType => ZString.Empty;

		public SupervisingOfficeCollection SupervisingOfficeList => new SupervisingOfficeCollection(Factory);

		public virtual RefCusProcedureCollection CPCList
			=> new RefCusProcedureCollection(Factory, GetDefaultCusProcedureDataGroupingCode(), GetDateOfValuation(), Parent.EntryInstruction?.CEI_Style ?? ZString.Empty, Parent.Declaration?.JE_MessageType ?? ZString.Empty);

		public WarehouseClientCollection WarehouseList => new WarehouseClientCollection(Factory);

		public override CodeDescriptionPairList ValuationCodeList => Factory.GetCachedValue<ValuationMethodList>();

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
			=> Parent != null && Parent.InvoiceHeader != null ? new EuOrgSupplierPartCollection(Factory, Parent, InvoiceLine.InvoiceHeader?.IsExport ?? ZBool.False) : new EuOrgSupplierPartCollection(Factory);

		// Base EU behaviour: limit CPCs by ZZ6_ShipmentType=JE_MessageType (c.f. GB which also limits on ZZ6_Category.Contains(JE_DeclarationType))
		public override ICodeDescriptionPairList Procedures => CPCsCore;

		// Limit CPCs by ZZ6_ShipmentType=JE_MessageType AND on ZZ6_Category.Contains(JE_DeclarationType)).
		protected ICodeDescriptionPairList CPCsCore
		{
			get
			{
				var countryCode = GetDefaultCusProcedureDataGroupingCode();
				var declarationType = Parent.EntryInstruction?.CEI_Style ?? ZString.Empty;
				var shipmentType = Parent.Declaration?.JE_MessageType ?? ZString.Empty;
				return Factory.GetCachedValue("Enterprise.Customs." + countryCode + ".CPCs_" + shipmentType + "_" + declarationType, delegate
				{
					var result = new CodeDescriptionPairList();
					var allProcs = new RefCusProcedure.Loader(Factory).LoadForShipmentTypeAndZzzDataGroupingAndGroup(shipmentType, countryCode, declarationType);
					if (allProcs != null)
					{
						result.AddRange(allProcs);
					}
					result.Sort();
					return result;
				});
			}
		}

		public override CodeDescriptionPairList BondedWhsUnitQtyList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);

		public virtual CodeDescriptionPairList NationalAdditionalCodeList => new CodeDescriptionPairList();

		CodeDescriptionPairList GetCachedRefCusCodeList(ZString codeType, bool includeParentDataGrouping = true) => RefCusCodeListTypes.GetCachedList(Factory, GetDefaultDataGroupingCode(), codeType, GetDateOfValuation(), includeParentDataGrouping: includeParentDataGrouping);

		public RefCountryCollection CountryOfDestinations => new RefCountryCollection(Factory);

		protected override ICollection CountryOfOriginsCore() => CusRefTradeGroupCountryView.Loader.GetCachedListTradeGroupCountries(Factory, ZString.Empty, GetDefaultDataGroupingCode());

		public OrgHeaderCollection ConsignorList => new ConsignorCollection(Factory);

		public OrgHeaderCollection BuyerList => new OrgHeaderCollection(Factory);

		public OrgHeaderCollection SellerList => new OrgHeaderCollection(Factory);

		public CodeDescriptionPairList ValuationIndicators => Factory.GetCachedValue<ValuationIndicatorCodeList>();
	}
}
