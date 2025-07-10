using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class TaxLookupsCommon : EU.Business.Declaration.MultiLineAddInfos.TaxLookupsCommon
	{
		public TaxLookupsCommon(EU.Business.Declaration.IEuTax parent) : base(parent)
		{
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
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "IE.TypeList_{0}_{1}", parent.ImportExportParent.IsImport, parent.ImportExportParent.IsExport); // Constant Key for caching
				return parent.Factory.GetCachedValue(cacheKey, delegate
				{
					var result = new RateCodeDescriptionPairList(base.TypeList.OfType<ICodeDescription>());
					if (importExportParent.IsImport && !result.ContainsCode(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat))
					{
						result.InsertInSortOrder(new CodeDescriptionPair(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat,
							CommonResStrings.Vat));
					}
					return result;
				});
			}
		}
	}
}
