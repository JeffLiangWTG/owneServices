using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class TaxLookupsCommon
	{
		public TaxLookupsCommon(IEuTax parent)
		{
			this.parent = parent;
		}
		protected IEuTax parent;

		public virtual CodeDescriptionPairList MOPList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var factory = parent.Factory;
				var declaration = GetDeclaration(parent as BusinessObject);
				if (declaration != null)
				{
					if (declaration.IsUCC6)
					{
						result = factory.Get104IMCodeListOnlyList();
					}
					else
					{
						var countryCode = declaration.GetDefaultDataGroupingCode();
						var dateOfValuation = declaration.DateOfValuation;
						result = GetMOPList(factory, countryCode, parent.ImportExportParent, dateOfValuation);
					}
				}
				else if (parent is Tax_CusAddInfoOnlyForPIVOT)
				{
					result = GetMOPList(factory, parent.CountryCode, parent.ImportExportParent, ZDateTime.Today);
				}
				return result;
			}
		}

		protected JobDeclaration GetDeclaration(BusinessObject businessObject)
		{
			JobDeclaration result = null;
			if (businessObject is JobComInvoiceLineTax invLineTax)
			{
				result = invLineTax.InvoiceLine?.Declaration;
			}
			else if (businessObject is CusEntryLineFee fee)
			{
				result = fee.EntryLine?.Declaration;
			}
			return result;
		}

		public static CodeDescriptionPairList GetMOPList(BusinessObjectFactory factory, ZString countryCode, ICanBeImportOrExport importExportParent)
		{
			return GetMOPList(factory, countryCode, importExportParent, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetMOPList(BusinessObjectFactory factory, ZString countryCode, ICanBeImportOrExport importExportParent, ZDateTime date)
		{
			if (importExportParent == null || ((BusinessObject)importExportParent).IsDeleted)
			{
				return new CodeDescriptionPairList();
			}

			return GetMOPList(factory, countryCode, importExportParent.IsImport, importExportParent.IsExport, date);
		}

		public static CodeDescriptionPairList GetMOPList(BusinessObjectFactory factory, ZString countryCode, bool isImport, bool isExport)
		{
			return GetMOPList(factory, countryCode, isImport, isExport, ZDateTime.Today);
		}

		public static CodeDescriptionPairList GetMOPList(BusinessObjectFactory factory, ZString countryCode, bool isImport, bool isExport, ZDateTime date)
		{
			var key = countryCode;
			var defaultPairs = new[]
			{
				new KeyValuePair<ZString, ZString>(
					RefCusCodeListAttributeTypes.Codes.Category,
					UniversalReferenceConstants.RefCusCodeListAttributes.Values.DecBox47)
			};
			var pairs = new List<KeyValuePair<ZString, ZString>>(defaultPairs);
			if (isImport)
			{
				key += "_IMP";
				pairs.Add(new KeyValuePair<ZString, ZString>(RefCusCodeListAttributeTypes.Codes.Import, ZString.Empty));
			}
			else if (isExport)
			{
				key += "_EXP";
				pairs.Add(new KeyValuePair<ZString, ZString>(RefCusCodeListAttributeTypes.Codes.Export, ZString.Empty));
			}
			return factory.GetCachedValue(key, () =>
			{
				const string mop = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment;
				var result = factory.GetCachedListMatchAllAttributes(countryCode, mop, date, pairs.ToArray(), RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				if (result.Count == 0)
				{
					result = factory.GetCachedListMatchAllAttributes(countryCode, mop, date, defaultPairs, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
				}
				result.Sort();
				return result;
			});
		}

		public virtual CodeDescriptionPairList RateDutyList
		{
			get
			{
				var approximateTaxType = parent.G4_Type.SubstringSafe(0, 1);
				return GetRateDutyList(approximateTaxType, parent.Factory);
			}
		}

		public virtual CodeDescriptionPairList BaseQuantityUQList
		{
			get
			{
				var dateForCache = ZDateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture);
				return parent.Factory.GetCachedValue("ZZRefCusCodeListTaxUQ_" + parent.CountryCode + dateForCache, delegate
				{
					var baseQuantityUQList = new CodeDescriptionPairList();
					baseQuantityUQList.AddRange(RefCusCodeListTypes.GetCachedList(parent.Factory, parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, ZDateTime.Today, null, string.Empty, false));
					baseQuantityUQList.AddRange(RefCusCodeListTypes.GetCachedList(parent.Factory, parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today));
					baseQuantityUQList.Sort();
					return baseQuantityUQList;
				});
			}
		}

		public static CodeDescriptionPairList GetUnitOfQuantityList(BusinessObjectFactory factory, string countryCode)
		{
			var dateForCache = ZDateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture);
			return factory.GetCachedValue("ZZRefCusCodeListTaxUQ_" + countryCode + dateForCache, delegate
			{
				var baseQuantityUQList = new CodeDescriptionPairList();
				baseQuantityUQList.AddRange(RefCusCodeListTypes.GetCachedList(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsTaxUQ, ZDateTime.Today, null, string.Empty, false));
				baseQuantityUQList.AddRange(RefCusCodeListTypes.GetCachedList(factory, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today));
				baseQuantityUQList.Sort();
				return baseQuantityUQList;
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Get lost")]
		public static CodeDescriptionPairList GetRateDutyList(string approximateTaxType, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("EU.TaxTypes." + approximateTaxType, delegate
			{
				var rateDutyList = new CodeDescriptionPairList();
				bool addDuty = false;
				bool addVat = false;
				bool addExcise = false;
				switch (approximateTaxType)
				{
					case "A":
						addDuty = true;
						break;
					case "B":
						addVat = true;
						break;
					case "1":
					case "2":
					case "3":
					case "4":
					case "5":
					case "6":
					case "7":
					case "8":
					case "9":
					case "0":
						addExcise = true;
						break;
					default:
						addDuty = true;
						addVat = true;
						addExcise = true;
						break;
				}

				if (addDuty)
				{
					rateDutyList.AddRange(new TaxRateCustomsDutyListImport());
				}

				if (addVat)
				{
					rateDutyList.AddRange(new TaxRateVATDutyListImport());
				}

				if (addExcise)
				{
					rateDutyList.AddRange(new TaxRateExciseDutyListImport());
				}

				rateDutyList.Sort();
				return rateDutyList;
			});
		}

		public virtual RateCodeDescriptionPairList TypeList
		{
			get
			{
				var importExportParent = parent.ImportExportParent;
				if (importExportParent == null || ((BusinessObject)importExportParent).IsDeleted)
				{
					return new RateCodeDescriptionPairList();
				}

				var isImport = importExportParent.IsImport;
				var isExport = importExportParent.IsExport;
				var factory = parent.Factory;
				return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "EU.TypeList_{0}_{1}", isImport, isExport), () =>
				{
					var rateCodes = GetImportExportRelatedRateCodes(factory, isImport, isExport);
					AddVatRateCodeIfNeeded(rateCodes);
					RemoveAdditionalDutiesRateCodes(rateCodes);
					AddCountrySpecificRateCodes(rateCodes);
					rateCodes.Sort();
					return rateCodes;
				});
			}
		}

		#region Implementation

		protected virtual ZString[] GetCountrySpecificRateTypesToInclude() => System.Array.Empty<ZString>();

		RateCodeDescriptionPairList GetImportExportRelatedRateCodes(BusinessObjectFactory factory, ZBool isImport, ZBool isExport)
		{
			var rateCodeList = new RateCodeDescriptionPairList();

			if (!isImport && !isExport)
			{
				return rateCodeList;
			}

			var rateCodeLoadCriteria = new RateCodeLoadCriteria() { InternalUseRate = false };
			if (!isImport)
			{
				rateCodeLoadCriteria.RateTypesToInclude = GetRateTypesToInclude();
			}
			else if (!isExport)
			{
				rateCodeLoadCriteria.RateTypesToExclude = exportRelatedRateTypes.ToArray();
			}

			var rateTypesToExcludeForOtherCountries = GetRateTypesToExcludeForOtherCountries();
			if (rateTypesToExcludeForOtherCountries.Any())
			{
				var existRateTypesToExclude = rateCodeLoadCriteria.RateTypesToExclude ?? System.Array.Empty<ZString>();
				rateCodeLoadCriteria.RateTypesToExclude = existRateTypesToExclude.Concat(rateTypesToExcludeForOtherCountries).ToArray();
			}

			var rateCodes = CusRefRateCodeView.Loader.Load(factory, parent.CountryCode, rateCodeLoadCriteria);
			rateCodeList.AddRange(rateCodes);
			return rateCodeList;
		}

		ZString[] GetRateTypesToInclude()
		{
			var rateTypesToInclude = exportRelatedRateTypes.ToArray();
			var countrySpecificIncludeRateTypes = GetCountrySpecificRateTypesToInclude();

			if (countrySpecificIncludeRateTypes.Any())
			{
				rateTypesToInclude = countrySpecificIncludeRateTypes.Concat(exportRelatedRateTypes).ToArray();
			}

			return rateTypesToInclude;
		}

		void RemoveAdditionalDutiesRateCodes(RateCodeDescriptionPairList rateCodeList)
		{
			foreach (var code in UniversalReferenceConstants.RefCusRateCodes.AdditionalDuties)
			{
				rateCodeList.RemoveCode(code);
			}
		}

		void AddVatRateCodeIfNeeded(RateCodeDescriptionPairList rateCodeList)
		{
			if (!rateCodeList.ContainsCode(UniversalReferenceConstants.RefCusRateCodes.Vat))
			{
				rateCodeList.AddPair(UniversalReferenceConstants.RefCusRateCodes.Vat, BaseJobComInvoiceLine.ConsumptionTaxDescription);
			}
		}

		protected virtual ZString[] GetRateTypesToExcludeForOtherCountries()
		{
			return System.Array.Empty<ZString>();
		}

		protected virtual void AddCountrySpecificRateCodes(RateCodeDescriptionPairList rateCodeDescriptionPairList) { }

		readonly ImmutableArray<ZString> exportRelatedRateTypes = new ZString[]
		{
			Customs.Business.UniversalReferenceConstants.RefCusRateTypes.ExportTaxes,
			Customs.Business.UniversalReferenceConstants.RefCusRateTypes.MiscellaneousOnlyForExport
		}.ToImmutableArray();

		#endregion
	}
}
