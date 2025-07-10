using System.Collections.Generic;
using System.Reflection;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Counter))]
	sealed class CounterTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< Counter (    Tst ) >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Counter(12,1)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Couter(Name)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Counter(Tbl.Name)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Counter()>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			AssertEquals(1, ValueProviderToTest.GetReplacement("< Counter ( V1 )  >", Report));
			AssertEquals(1, ValueProviderToTest.GetReplacement("< Counter ( V2 )  >", Report));
			AssertEquals(2, ValueProviderToTest.GetReplacement("< Counter ( V2 )  >", Report));
			AssertEquals(3, ValueProviderToTest.GetReplacement("< Counter ( V2 )  >", Report));
			AssertEquals(2, ValueProviderToTest.GetReplacement("< Counter ( V1 )  >", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Counter();
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>() { typeof(Counter).GetField("Counters", BindingFlags.Instance | BindingFlags.NonPublic) };
			}
		}
	}
}
