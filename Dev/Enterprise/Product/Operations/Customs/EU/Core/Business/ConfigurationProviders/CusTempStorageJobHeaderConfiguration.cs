using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public class CusTempStorageJobHeaderConfiguration
	{
		public ZBool IsUCC6(BusinessObject businessObject) => IsUCC6Core(businessObject);
		protected virtual ZBool IsUCC6Core(BusinessObject businessObject) => false;

		public static CusTempStorageJobHeaderConfiguration GetConfiguration(BusinessObjectFactory factory, string countryCode)
		{
			return factory.GetCachedValue(FormattableString.Invariant($"CusTempStorageJobHeaderConfiguration_{countryCode}"), () =>
			{
				object supporter = null;
				var builders = ObjectFactory.Get<Hashtable>("CusTempStorageJobHeaderConfiguration");
				if (!string.IsNullOrEmpty(countryCode))
				{
					var objectHandle = (ObjectHandle)builders[countryCode];
					supporter = objectHandle?.GetObject();
				}
				if (supporter == null)
				{
					var objectHandle = (ObjectHandle)builders[Core.Constants.CountryCodes.EuropeanUnion];
					supporter = objectHandle.GetObject();
				}
				return (CusTempStorageJobHeaderConfiguration)supporter;
			});
		}
	}
}
