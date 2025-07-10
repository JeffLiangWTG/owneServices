using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Now))]
	sealed class NowTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match < now >", ValueProviderToTest.IsResponsibleForReplacing("< now >", Passes.FirstPass));
			Assert("should not match < n ow>", !ValueProviderToTest.IsResponsibleForReplacing("< n ow>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			Assert(ValueProviderToTest.GetReplacement("<Now>", Report).GetType() == typeof(ZDateTime));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Now();
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>() { typeof(Now).GetField("nowTime", BindingFlags.Instance | BindingFlags.NonPublic) };
			}
		}

		[TestDate(2018, 11, 7, 12, 0, 0)]
		public override void TestDocumentation()
		{
			base.TestDocumentation();
		}
	}
}
