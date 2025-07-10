using System;
using System.Linq;
using System.Linq.Expressions;
using Moq;

namespace Enterprise.Customs.AE.Business.Testing;

public static class TestHelperUtils
{
	public static Mock<T> CreateMockInstance<T>() where T : class
	{
		var interfaceType = typeof(T);
		var mock = new Mock<T>();
		var properties = interfaceType.GetInterfaces()
							.Prepend(interfaceType)
							.SelectMany(x => x.GetProperties());

		foreach (var property in properties)
		{
			var value = Expression.Parameter(typeof(T), "value");
			var propertyDeclaringType = property.DeclaringType;
			Expression setupProperty = Expression.Property(value, propertyDeclaringType, property.Name);
			if (property.PropertyType == typeof(string))
			{
				var func = Expression.Lambda<Func<T, string>>(setupProperty, value); // Equivalent to v => v.property
				mock.Setup(func).Returns(property.Name + "_X");
			}
			else if (property.PropertyType == typeof(decimal))
			{
				var func = Expression.Lambda<Func<T, decimal>>(setupProperty, value);
				mock.Setup(func).Returns(123.45m);
			}
			else if (property.PropertyType == typeof(int))
			{
				var func = Expression.Lambda<Func<T, int>>(setupProperty, value);
				mock.Setup(func).Returns(123);
			}
		}
		return mock;
	}
}
