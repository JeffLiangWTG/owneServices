using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ApplicationSpecificTypeDeciderTest : TestCase
	{
		class ApplicationSpecificTypeDeciderTestClass : ApplicationSpecificTypeDecider
		{
			protected override IEnumerable<ApplicationSpecificType> ApplicationSpecificTypesCore
			{
				get
				{
					yield return new ApplicationSpecificType("ABC", () => typeof(string));
					yield return new ApplicationSpecificType("DEF", () => typeof(double));
				}
			}
		}

		public void TestApplicationSpecificTypeDecider()
		{
			var testClass = new ApplicationSpecificTypeDeciderTestClass();
			AssertEquals(typeof(string), testClass.GetTypeForApplicationCode("ABC"));
			AssertEquals(typeof(double), testClass.GetTypeForApplicationCode("DEF"));
			AssertNull(testClass.GetTypeForApplicationCode("ZZX"));
		}
	}
}
