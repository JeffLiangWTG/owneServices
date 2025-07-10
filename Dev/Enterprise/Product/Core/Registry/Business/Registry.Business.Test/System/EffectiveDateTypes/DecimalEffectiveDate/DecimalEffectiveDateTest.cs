using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DecimalEffectiveDate))]
	sealed class DecimalEffectiveDateTest : RegistryBusinessObjectTemplateTestCase<DecimalEffectiveDate>
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

		protected override DecimalEffectiveDate GetBusinessObjectToClone()
		{
			return GetNewDecimalEffectiveDate();
		}

		protected override DecimalEffectiveDate GetBusinessObjectToSerialise()
		{
			return GetNewDecimalEffectiveDate();
		}

		public void TestEffectiveValueForToday()
		{
			var decimalEffectiveDate = GetNewDecimalEffectiveDate();
			decimalEffectiveDate.PreviousValue = 1m;
			decimalEffectiveDate.NewValue = 2m;
			decimalEffectiveDate.EffectiveDate = ZDate.Today.AddDays(+1);
			AssertEquals(1m, decimalEffectiveDate.EffectiveValueForToday);
			decimalEffectiveDate.EffectiveDate = ZDate.Today;
			AssertEquals(2m, decimalEffectiveDate.EffectiveValueForToday);
			decimalEffectiveDate.EffectiveDate = ZDate.Today.AddDays(-1);
			AssertEquals(2m, decimalEffectiveDate.EffectiveValueForToday);
			decimalEffectiveDate.EffectiveDate = ZDate.Today.AddDays(+1);
			AssertEquals(1m, decimalEffectiveDate.EffectiveValueForToday);
		}

		public void TestEffectiveValue()
		{
			var decimalEffectiveDate = GetNewDecimalEffectiveDate();
			decimalEffectiveDate.PreviousValue = 1m;
			decimalEffectiveDate.NewValue = 2m;
			decimalEffectiveDate.EffectiveDate = ZDate.Today;
			AssertEquals(1m, decimalEffectiveDate.EffectiveValue(ZDate.Today.AddDays(-1)));
			AssertEquals(2m, decimalEffectiveDate.EffectiveValue(ZDate.Today));
			AssertEquals(2m, decimalEffectiveDate.EffectiveValue(ZDate.Today.AddDays(1)));
		}

		public void TestValidation()
		{
			var decimalEffectiveDate = GetNewDecimalEffectiveDate();
			AssertEquals(GetValidationType(), decimalEffectiveDate.Validation.GetType());
		}

		internal Type GetValidationType()
		{
			return typeof(DecimalEffectiveDateValidation);
		}

		internal DecimalEffectiveDate GetNewDecimalEffectiveDate()
		{
			return new DecimalEffectiveDate();
		}

		#endregion
	}
}
