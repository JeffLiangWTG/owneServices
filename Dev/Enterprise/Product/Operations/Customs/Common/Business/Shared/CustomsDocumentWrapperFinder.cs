using System.Collections;
using CargoWise.Application;

namespace Enterprise.Customs.Common.Shared;

public static class CustomsDocumentWrapperFinder
{
	public static Integration.Customs.Shared.ICustomsDocumentWrapperProvider GetCustomsDocumentWrapperProvider(string countryCode)
	{
		return ObjectFactory.Get<Hashtable>("CustomsDocumentWrapperProviders")[countryCode] is ObjectHandle providerHandle && providerHandle.GetObject() is Integration.Customs.Shared.ICustomsDocumentWrapperProvider provider ? provider : null;
	}
}
