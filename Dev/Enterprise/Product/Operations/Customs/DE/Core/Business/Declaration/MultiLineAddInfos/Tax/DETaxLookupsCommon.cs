using System.Globalization;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DETaxLookupsCommon : TaxLookupsCommon
	{
		public override RateCodeDescriptionPairList TypeList
		{
			get
			{
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "DE.TypeList_{0}_{1}", parent.ImportExportParent.IsImport, parent.ImportExportParent.IsExport); // Constant Key for caching
				return parent.Factory.GetCachedValue(cacheKey, delegate
				{
					var result = new RateCodeDescriptionPairList(base.TypeList.Cast<ICodeDescription>().Select(rateCode => new CodeDescriptionPair(rateCode.Code.PadRight(5, '0'), rateCode.Description)));
					result.Sort();
					if (parent.ImportExportParent.IsImport)
					{
						//Sales Tax Code
						result.InsertInSortOrder(new CodeDescriptionPair("B0000", Res.GetString("08e05e93-8a3a-438d-a62b-9854ffd8e25e", "Import Sales Tax (EU St)")));
					}
					return result;
				}
				);
			}
		}

		public DETaxLookupsCommon(EU.Business.Declaration.IEuTax parent) : base(parent)
		{
		}
	}
}
