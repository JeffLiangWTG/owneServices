using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class CusSupportingInfoHelper
	{
		public static bool MissesAttribute(this ZZRefCusCodeListCombined refCusCode, ZString attributeName,
			ZString? level = null) => level.HasValue ? !HasAttributeAtLevel(refCusCode, attributeName, level.Value) : !refCusCode?.HasAttribute(attributeName) ?? true;

		public static bool HasAttributeForMandatoryValidation(this ZZRefCusCodeListCombined refCusCode, ZString attributeName, ZString? level = null) =>
			level.HasValue ? HasAttributeAtLevel(refCusCode, attributeName, level.Value, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes) : refCusCode?.HasAttribute(attributeName, UniversalReferenceConstants.RefCusCodeListAttributeValues.Yes) ?? false;

		static bool HasAttributeAtLevel(ZZRefCusCodeListCombined refCusCode, ZString attributeName, ZString level, ZString? attrValue = null)
		{
			var retVal = false;

			if (refCusCode != null)
			{
				ZString attributeNameWithLevel = $"{attributeName};{level}";

				if (attrValue.HasValue)
				{
					retVal = refCusCode.HasAttribute(attributeNameWithLevel, attrValue) ||
						   !refCusCode.HasAttribute(attributeNameWithLevel) && refCusCode.HasAttribute(attributeName, attrValue);
				}
				else
				{
					retVal = refCusCode.HasAttribute(attributeName) || refCusCode.HasAttribute(attributeNameWithLevel);
				}
			}

			return retVal;
		}

		public static ZString GetLevelAttributeValue(CusSupportingInfo cusSupportingInfo)
		{
			var levelAttributeValue = ZString.Empty;
			var parent = cusSupportingInfo.Parent;
			if (parent is NctsHeader || parent is NctsArrivalMovementHeader || parent is NctsDepartureMovementHeader)
			{
				levelAttributeValue = UniversalReferenceConstants.RefCusCodeListLevelTypes.Header;
			}
			else if (parent is NctsBill)
			{
				levelAttributeValue = UniversalReferenceConstants.RefCusCodeListLevelTypes.House;
			}
			else if (parent is NctsCommonCargoDesc)
			{
				levelAttributeValue = UniversalReferenceConstants.RefCusCodeListLevelTypes.Item;
			}
			return levelAttributeValue;
		}

		public static ZZRefCusCodeListCombinedCollection GetTypeCodeList(BusinessObjectFactory factory, ZString codeType, bool includeParent, string levelAttributeValue, string dataGroupingCode, bool applyLevelAttributeToChild = true)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory
				, dataGroupingCode
				, [codeType]
				, ZDateTime.Today
				, applyLevelAttributeToChild ? CreateRefCusListRefCusCodeListAttributeFilters(levelAttributeValue) : []
				, includeParent);
		}

		public static ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection GetTypeCodeListUniqueFromChildFirstThenParent(BusinessObjectFactory factory, string dataGrouping, string codeType, string levelAttributeValue, bool applyLevelAttributeToChild)
		{
			var attributeFilters = CreateRefCusListRefCusCodeListAttributeFilters(levelAttributeValue);

			return new ZzRefCusCodeListCombinedUniqueChildFirstThenParentCollection(factory
				, dataGrouping
				, codeType
				, ZDateTime.Today, attributeFilters: applyLevelAttributeToChild ? attributeFilters : [], parentDataGroupFilters: attributeFilters);
		}

		static RefCusCodeListAttributeFilter[] CreateRefCusListRefCusCodeListAttributeFilters(string levelAttributeValue)
		{
			var attributeFilters = new[]
			{
				new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Level, ZDateTime.Today, SQLComparisonOperator.Equal, levelAttributeValue)
			};
			return attributeFilters;
		}

		public static int GetPhase5DepartureReferenceNumberMaxLength(bool isInPhase5TransitionPeriod) => isInPhase5TransitionPeriod ? 35 : 70;
	}
}
