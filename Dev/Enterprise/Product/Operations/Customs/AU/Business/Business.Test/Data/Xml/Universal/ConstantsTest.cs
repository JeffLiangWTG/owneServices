using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ConstantsTest : TestCase
	{
		public void TestInvoiceHeaderKeysDoNotContainSpaces()
		{
			AssertKeysContainNoSpaces(typeof(Constants.InvoiceHeader.Keys));
		}

		public void TestInvoiceLineKeysDoNotContainSpaces()
		{
			AssertKeysContainNoSpaces(typeof(Constants.InvoiceLine.Keys));
		}

		void AssertKeysContainNoSpaces(Type type)
		{
			var constantsList = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(x => x.IsLiteral && !x.IsInitOnly);
			CombineAssertions(() =>
			{
				foreach (var constant in constantsList)
				{
					var constantValue = (string)constant.GetRawConstantValue();
					AssertNotContains("String should not contain empty space", " ", constantValue);
				}
			});
		}
	}
}
