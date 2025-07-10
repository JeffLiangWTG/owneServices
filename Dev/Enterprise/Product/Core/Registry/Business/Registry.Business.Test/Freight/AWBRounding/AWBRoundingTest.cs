using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AWBRounding))]
	sealed class AWBRoundingTest : RegistryBusinessObjectTemplateTestCase<AWBRounding>
	{
		#region Validation

		public void TestValidateRoundingMode()
		{
			AssertNoErrors("Precondition: RoundingMode should not have errors.", BizObj.RoundingModeInfo);
			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			AssertNoErrors("Precondition: RoundingScale should not have errors.", BizObj.RoundingModeInfo);
			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			AssertNoErrors("Precondition: RoundingScale should not have errors.", BizObj.RoundingModeInfo);
			BizObj.RoundingMode = "crap";
			AssertHasErrorContaining(BizObj.RoundingModeInfo, "Enter a valid selection");
		}

		public void TestValidateRoundingScale()
		{
			AssertNoErrors("Precondition: RoundingScale should not have errors.", BizObj.RoundingScaleInfo);
			BizObj.RoundingScale = ChargeableWeightRoundingScales.Scale05;
			AssertNoErrors("Precondition: RoundingScale should not have errors.", BizObj.RoundingScaleInfo);
			BizObj.RoundingScale = ChargeableWeightRoundingScales.Scale10;
			AssertNoErrors("Precondition: RoundingScale should not have errors.", BizObj.RoundingScaleInfo);
			BizObj.RoundingScale = "0.3";
			AssertHasErrorContaining(BizObj.RoundingScaleInfo, "Enter a valid selection");
			BizObj.RoundingScale = "3.2";
			AssertHasErrorContaining(BizObj.RoundingScaleInfo, "Enter a valid selection");
		}

		#endregion

		public void TestRoundUp()
		{
			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			AssertEquals("RoundUp True", true, BizObj.RoundUp);
			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			AssertEquals("RoundUp False", false, BizObj.RoundUp);
		}

		public void TestRoundToWholeNumber()
		{
			BizObj.RoundingScale = ChargeableWeightRoundingScales.Scale10;
			AssertEquals("Round to whole number True", true, BizObj.RoundToWholeNumber);
			BizObj.RoundingScale = ChargeableWeightRoundingScales.Scale05;
			AssertEquals("Round to whole number False", false, BizObj.RoundToWholeNumber);
		}

		public void TestRoundingDisabled()
		{
			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.None);
			AssertEquals("Rounding Disabled", false, BizObj.RoundingEnabled);

			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.Down);
			AssertEquals("Rounding Enabled", true, BizObj.RoundingEnabled);

			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.Up);
			AssertEquals("Rounding Enabled", true, BizObj.RoundingEnabled);

			BizObj.RoundingMode = nameof(ChargeableWeightRoundingType.None);
			AssertEquals("Rounding Disabled", false, BizObj.RoundingEnabled);
		}

		#region Implementation

		protected override AWBRounding GetBusinessObjectToClone()
		{
			return new AWBRounding();
		}

		protected override AWBRounding GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
