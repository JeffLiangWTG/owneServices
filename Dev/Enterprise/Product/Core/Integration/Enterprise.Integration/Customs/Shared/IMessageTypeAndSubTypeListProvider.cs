using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface IMessageTypeAndSubTypeListProvider
			{
				ICodeDescriptionPairList MessageTypeList(BusinessObjectFactory factory, ZString companyCode);

				ICodeDescriptionPairList MessageSubTypeList(BusinessObjectFactory factory, ZString companyCode, ZString messageType);
			}
		}
	}
}
