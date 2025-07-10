using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListTypesCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class PreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public PreviousDocumentLookups(PreviousDocument parent) : base(parent) { }

		public new PreviousDocument Parent => (PreviousDocument)base.Parent;

		string GetCacheKeyWithDate(string constKey) => constKey + "|" + ZDateTime.Today.ToString("yyMMdd", CultureInfo.InvariantCulture);

		public override CodeDescriptionPairList PackTypeList => Parent.Factory.GetCachedValue(GetCacheKeyWithDate("Enterprise.Customs.IE.Business.Declaration.PreviousDocument.PackTypeList"), GetPackageTypeList);

		CodeDescriptionPairList GetPackageTypeList()
		{
			var result = new CodeDescriptionPairList();
			var codeList = ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(
				factory: Parent.Factory,
				dataGroupingCode: RefDataGroupingCodes.UnitedNationsRecommendations,
				codeType: RefCusCodeListTypesCodes.UnitedNationsPackageTypes,
				date: ZDateTime.Today
			);
			if (codeList.Length > 0)
			{
				result.AddRange(codeList);
				result.SortByDescription();
			}
			return result;
		}

		public override CodeDescriptionPairList UnitOfQuantityList => Parent.Factory.GetCachedValue(GetCacheKeyWithDate("Enterprise.Customs.IE.Business.Declaration.PreviousDocument|UnitOfQuantityList"), GetUnitOfQuantityList);

		CodeDescriptionPairList GetUnitOfQuantityList()
		{
			var result = new CodeDescriptionPairList();
			var codeList = ZZRefCusCodeListCombined.Loader.LoadAndFallbackToParentDataGroupingIfNotFound(
				factory: Parent.Factory,
				dataGroupingCode: Core.Constants.CountryCodes.Ireland,
				codeType: RefCusCodeListTypesCodes.SupportingDocumentsUnitOfMeasure,
				date: ZDateTime.Today
			);
			if (codeList.Length > 0)
			{
				result.AddRange(codeList);
				result.SortByDescription();
			}

			return result;
		}
	}
}
