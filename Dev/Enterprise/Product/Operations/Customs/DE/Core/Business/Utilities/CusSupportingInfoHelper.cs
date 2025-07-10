using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public static class CusSupportingInfoHelper
	{
		public static bool MissesAttribute(this ZZRefCusCodeListCombined refCusCode, ZString attributeName) => !refCusCode?.HasAttribute(attributeName) ?? true;

		public static bool HasAttributeForMandatoryValidation(this ZZRefCusCodeListCombined refCusCode, ZString attributeName) => (string)refCusCode?.GetAttribute(attributeName) == UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes;

		public static ZZRefCusCodeListCombinedCollection GetDocumentCodesFilteredByLevelAttributes(BusinessObjectFactory factory, ZString[] codeTypes, ZString levelAttributeValue)
		{
			return GetDocumentCodesFilteredByLevelAttributes(factory, null, codeTypes, levelAttributeValue);
		}

		public static ZZRefCusCodeListCombinedCollection GetDocumentCodesFilteredByLevelAttributes(BusinessObjectFactory factory, ZQuery additionalFilter, ZString[] codeTypes, ZString levelAttributeValue) => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
			factory,
			additionalFilter,
			Core.Constants.CountryCodes.Germany,
			codeTypes,
			ZDateTime.Today,
			new[] { new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, JoinCondition.And, levelAttributeValue) },
			false);
	}
}
