using System;
using System.Reflection;
using CargoWise.Definitions;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class BusinessContextTest : TestCase
	{
		public void TestEnum()
		{
			new EnumerationChecker().CheckEnums(typeof(BusinessContext), 20);
			string[] businessContexts = Enum.GetNames(typeof(BusinessContext));
			foreach (FieldInfo fieldInfo in typeof(Constants.BusinessContextPrefixes).GetFields())
			{
				string prefix = (string)fieldInfo.GetValue(null);
				foreach (string businessContext in businessContexts)
				{
					if (businessContext.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
					{
						Fail(string.Format("BusinessContext.{0} starts with '{1}', which is a reserved prefix.", businessContext, prefix));
					}
				}
			}
			Assert("No errors found.", true);
		}
	}
}
