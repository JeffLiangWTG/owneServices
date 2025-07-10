using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public static partial class Universal
			{
				public interface IRefCusCodeListTypesListProvider
				{
					ICodeDescriptionPairList GetList(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime date, ZString[] attributeNames);
					ICodeDescriptionPairList GetList(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime dateBefore);
					IBusinessObjectCollection GetCollection(BusinessObjectFactory factory, ZString dataGrouping, ZString codeType, ZDateTime dateBefore);
					IBusinessObjectCollection GetCollection(BusinessObjectFactory factory, ZString[] dataGroupingCode, ZString codeTypes, ZDateTime date);
				}

				public interface IRefCusCodeListProvider
				{
					ZString[] GetAttributeValues(BusinessObjectFactory factory, ZString dataGroupingCode, ZString codeType, ZDateTime date, ZString code, ZString attributeName);
				}
			}
		}
	}
}
