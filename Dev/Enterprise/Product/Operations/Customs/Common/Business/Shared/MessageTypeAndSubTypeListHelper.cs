using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public static class MessageTypeAndSubTypeListHelper
	{
		public static Integration.Customs.Shared.IMessageTypeAndSubTypeListProvider GetMessageTypeAndSubTypeListProvider(ZString countryCode)
		{
			var allProviders = ObjectFactory.Get<Hashtable>("MessageTypeAndSubTypeListProvider");
			var countryCodeKey = countryCode.ToString();
			var providerHandle = allProviders.ContainsKey(countryCodeKey) ? (ObjectHandle)allProviders[countryCodeKey] : (ObjectHandle)allProviders["Shared"];
			return (Integration.Customs.Shared.IMessageTypeAndSubTypeListProvider)providerHandle.GetObject();
		}

		public static ICodeDescriptionPairList MessageTypeList(BusinessObjectFactory factory, ZString companyCode, ZString countryCode) => GetMessageTypeAndSubTypeListProvider(countryCode).MessageTypeList(factory, companyCode);

		public static ICodeDescriptionPairList MessageSubTypeList(BusinessObjectFactory factory, ZString companyCode, ZString countryCode, ZString messageType) => GetMessageTypeAndSubTypeListProvider(countryCode).MessageSubTypeList(factory, companyCode, messageType);
	}
}
