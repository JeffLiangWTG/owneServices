using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.DataTransfer.CusTempStorage;

public static class Extensions
{
	public static ICusTempStorageJobHeaderDataObjectProvider GetCusTempStorageJobHeaderDataObjectProvider(this BusinessObjectFactory factory, string countryCode)
	{
		return factory.GetCachedValue(System.FormattableString.Invariant($"{countryCode}-ICusTempStorageJobHeaderDataObjectProvider"), () =>
		{
			var types = ObjectFactory.Get<Hashtable>("CusTempStorageJobHeaderDataObjectProviders");
			var objectHandle = (ObjectHandle)(types[countryCode] ?? types[Core.Constants.CountryCodes.EuropeanUnion]);
			return (ICusTempStorageJobHeaderDataObjectProvider)objectHandle?.GetObject();
		});
	}
}
