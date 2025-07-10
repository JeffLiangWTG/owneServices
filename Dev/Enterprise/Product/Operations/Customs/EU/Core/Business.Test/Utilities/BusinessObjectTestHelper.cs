using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Testing;

public static class BusinessObjectTestHelper
{
	public static void ClearAllCachedValues(this BusinessObject obj)
	{
		Argument.NotNull(obj, nameof(obj));

		var type = obj.GetType();
		while (type != typeof(BusinessObject))
		{
			type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(f => f.FieldType.IsGenericType &&
							f.FieldType.GetGenericTypeDefinition() == typeof(CachedValue<>))
				.ForEach(cachedField => cachedField.SetValue(obj, null));
			type = type.BaseType;
		}
	}
}
