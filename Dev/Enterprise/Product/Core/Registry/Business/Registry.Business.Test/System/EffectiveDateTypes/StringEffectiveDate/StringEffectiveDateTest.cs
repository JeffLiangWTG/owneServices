using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(StringEffectiveDate))]
	sealed class StringEffectiveDateTest : RegistryBusinessObjectTemplateTestCase<StringEffectiveDate>
	{
		#region Implementation
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override StringEffectiveDate GetBusinessObjectToClone()
		{
			return new StringEffectiveDate();
		}

		protected override StringEffectiveDate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestEffectiveValueForToday()
		{
			StringEffectiveDate stringEffectiveDate = new StringEffectiveDate();
			stringEffectiveDate.PreviousValue = "AAA";
			stringEffectiveDate.NewValue = "BBB";
			stringEffectiveDate.EffectiveDate = ZDate.Today.AddDays(+1);
			AssertEquals("AAA", stringEffectiveDate.EffectiveValueForToday);
			stringEffectiveDate.EffectiveDate = ZDate.Today;
			AssertEquals("BBB", stringEffectiveDate.EffectiveValueForToday);
			stringEffectiveDate.EffectiveDate = ZDate.Today.AddDays(-1);
			AssertEquals("BBB", stringEffectiveDate.EffectiveValueForToday);
			stringEffectiveDate.EffectiveDate = ZDate.Today.AddDays(+1);
			AssertEquals("AAA", stringEffectiveDate.EffectiveValueForToday);
		}

		public void TestEffectiveValue()
		{
			StringEffectiveDate stringEffectiveDate = new StringEffectiveDate();
			stringEffectiveDate.PreviousValue = "AAA";
			stringEffectiveDate.NewValue = "BBB";
			stringEffectiveDate.EffectiveDate = ZDate.Today;
			AssertEquals("AAA", stringEffectiveDate.EffectiveValue(ZDate.Today.AddDays(-1)));
			AssertEquals("BBB", stringEffectiveDate.EffectiveValue(ZDate.Today));
			AssertEquals("BBB", stringEffectiveDate.EffectiveValue(ZDate.Today.AddDays(1)));
		}
		#endregion
	}
}
