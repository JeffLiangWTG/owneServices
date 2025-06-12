using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests
{
	public static class ObjectComparer
	{
		public static void AssertPropertiesAreEqual(object expected, object actual)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
			var actualType = actual.GetType();
			foreach (var expectedProperty in expected.GetType().GetProperties(flags))
			{
				var actualProperty = actualType.GetProperty(expectedProperty.Name, flags);
				Assert.IsNotNull(actualProperty, "Property {0} exists.", expectedProperty.Name);
				var actualPropertyValue = actualProperty.GetValue(actual, null);
				var expectedPropertyValue = expectedProperty.GetValue(expected, null);
				Assert.AreEqual(expectedPropertyValue, actualPropertyValue, expectedProperty.Name);
			}
		}
	}
}