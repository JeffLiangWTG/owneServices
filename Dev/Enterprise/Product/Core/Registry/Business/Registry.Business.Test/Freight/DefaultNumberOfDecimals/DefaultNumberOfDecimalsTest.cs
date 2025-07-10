using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultNumberOfDecimals))]
	sealed class DefaultNumberOfDecimalsTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestValidateUnitOfMeasure()
		{
			BizObj.UnitOfMeasure = Constants.Weight.Kilograms;
			AssertNoErrors(BizObj.UnitOfMeasureInfo);

			BizObj.UnitOfMeasure = Constants.Length.Metres;
			AssertNoErrors(BizObj.UnitOfMeasureInfo);

			BizObj.UnitOfMeasure = Constants.Volume.CubicCentimeters;
			AssertNoErrors(BizObj.UnitOfMeasureInfo);

			BizObj.UnitOfMeasure = "";
			AssertHasErrors(BizObj.UnitOfMeasureInfo);
			BizObj.UnitOfMeasure = "XX";
			AssertHasErrorContaining(BizObj.UnitOfMeasureInfo, "Enter a valid selection");
		}

		public void TestValidateTransportMode()
		{
			BizObj.TransportMode = "AIR";
			AssertNoErrors(BizObj.TransportModeInfo);

			BizObj.TransportMode = "";
			AssertHasErrors(BizObj.TransportModeInfo);
			BizObj.TransportMode = "ABC";
			AssertHasErrorContaining(BizObj.TransportModeInfo, "Enter a valid selection");
		}

		public void TestValidateRoundingMode()
		{
			BizObj.RoundingMode = RoundingModes.Up;
			AssertNoErrors(BizObj.RoundingModeInfo);
			BizObj.RoundingMode = RoundingModes.Down;
			AssertNoErrors(BizObj.RoundingModeInfo);
			BizObj.RoundingMode = RoundingModes.BankersRounding;
			AssertNoErrors(BizObj.RoundingModeInfo);

			BizObj.RoundingMode = "";
			AssertHasErrors(BizObj.RoundingModeInfo);
			BizObj.RoundingMode = "ABC";
			AssertHasErrorContaining(BizObj.RoundingModeInfo, "Enter a valid selection");
		}

		public void TestValidateNumberOfDecimals()
		{
			BizObj.UnitOfMeasure = "KG";
			BizObj.NumberOfDecimals = 3;
			AssertNoErrors(BizObj.NumberOfDecimalsInfo);
			BizObj.NumberOfDecimals = 0;
			AssertNoErrors(BizObj.NumberOfDecimalsInfo);
			BizObj.NumberOfDecimals = -3;
			AssertHasErrorContaining(BizObj.NumberOfDecimalsInfo, "Please enter a numeric value between 0 and 3 for weight/volume units.");
			BizObj.NumberOfDecimals = 1;
			AssertNoErrors(BizObj.NumberOfDecimalsInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new DefaultNumberOfDecimals();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
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

		new DefaultNumberOfDecimals BizObj
		{
			get { return (DefaultNumberOfDecimals)base.BizObj; }
		}

		#endregion
	}
}
