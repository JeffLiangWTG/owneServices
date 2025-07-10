using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business
{
	public class GBTaxLookupsCommon : TaxLookupsCommon
	{
		public CodeDescriptionPairList RateSuspensionList
		{
			get
			{
				var result = parent.Factory.GetCachedValue<CodeDescriptionPairLists.TaxRateCustomsSuspensionListImport>();
				result.Sort();
				return result;
			}
		}

		public CodeDescriptionPairList RateOverrideList
		{
			get
			{
				var rateOverrideList = new CodeDescriptionPairList();
				rateOverrideList.AddRange(new CodeDescriptionPairLists.TaxRateAntiDumpingOverrideListImport());
				rateOverrideList.AddRange(new CodeDescriptionPairLists.TaxRateCAPOverrideListImport());
				rateOverrideList.AddRange(new CodeDescriptionPairLists.TaxRateCustomsOverrideListImport());
				rateOverrideList.AddRange(new CodeDescriptionPairLists.TaxRateExciseOverrideListImport());
				rateOverrideList.AddRange(new CodeDescriptionPairLists.TaxRateVATOverrideListImport());
				rateOverrideList.Sort();
				return rateOverrideList;
			}
		}

		public override RateCodeDescriptionPairList TypeList
		{
			get
			{
				var importExportParent = parent.ImportExportParent;
				if (importExportParent == null || ((BusinessObject)importExportParent).IsDeleted)
				{
					return new RateCodeDescriptionPairList();
				}

				var cacheKey = string.Format(CultureInfo.InvariantCulture, "GB.TypeList_{0}_{1}", importExportParent.IsImport, importExportParent.IsExport); // Constant Key for caching
				return parent.Factory.GetCachedValue(cacheKey, delegate
				{
					var result = new RateCodeDescriptionPairList(base.TypeList.OfType<ICodeDescription>());
					if (importExportParent.IsImport)
					{
						result.InsertInSortOrder(new CodeDescriptionPair(UniversalReferenceConstants.RefCusRateCodes.Vat,
							VATDescriptionENG));
					}
					return result;
				});
			}
		}

		public const string VATDescriptionENG = "Value Added Tax (VAT)";

		public GBTaxLookupsCommon(IEuTax parent) : base(parent)
		{
		}
	}
}
