using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public static class CustomPropertyCollectionBuilder
	{
		public static ICustomPropertyCollection GetCustomProperties(IEnumerable<ICustomProperty> properties)
		{
			return new BaseICustomPropertyCollectionImpl(properties);
		}

		public static ICustomPropertyCollection GetCustomProperties(Func<string, object> getter, Func<string, object, bool> setter)
		{
			return new CustomPropertyCollectionImpl(getter, setter);
		}

		public static ICustomPropertyCollection GetCustomProperties(Func<BusinessObject, string, object> getter, Func<BusinessObject, string, object, bool> setter)
		{
			return new CustomPropertyCollectionImpl(getter, setter);
		}
	}
}
