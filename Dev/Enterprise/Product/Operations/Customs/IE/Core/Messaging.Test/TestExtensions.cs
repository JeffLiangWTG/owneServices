using System;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Messaging.Testing;

public static class TestExtensions
{
	public static void TestXlsxField(this Type providerType, string propertyName, int expectedOrder, string displayName)
	{
		var xlsxFieldAttribute = providerType.GetProperty(propertyName).GetCustomAttribute<XlsxFieldAttribute>();
		Assertion.AssertNotNull($"Property {propertyName} should have an XlsxFieldAttribute.", xlsxFieldAttribute);
		Assertion.AssertEquals("Order", expectedOrder, xlsxFieldAttribute.Order);
		Assertion.AssertEquals("DisplayName", displayName, xlsxFieldAttribute.DisplayName);
	}
}
