using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultMinimumStayAndTravelTime))]
	sealed class DefaultMinimumStayAndTravelTimeTest : RegistryBusinessObjectTemplateTestCase<DefaultMinimumStayAndTravelTime>
	{
		#region Validation

		public void TestMinimumStayTime_ZeroValueShouldNotThrowError()
		{
			BizObj.StayTime = default;

			AssertNoErrors("Stay time can have Zero value", BizObj.StayTimeInfo);
		}

		public void TestMinimumStayTime_NegativeValueShouldThrowError()
		{
			BizObj.StayTime = -1;

			AssertHasError("Has the expected error", BizObj.StayTimeInfo, "value cannot be negative.");
		}

		public void TestMinimumStayTime_PositiveValueShouldNotThrowError()
		{
			BizObj.StayTime = 120;

			AssertNoErrors("Stay time should have positive value", BizObj.StayTimeInfo);
		}

		public void TestMinimumTravelTime_ZeroValueShouldNotThrowError()
		{
			BizObj.TravelTime = default;

			AssertNoErrors("Travel time can have Zero", BizObj.TravelTimeInfo);
		}

		public void TestMinimumTravelTime_NegativeValueShouldThrowError()
		{
			BizObj.TravelTime = -1;

			AssertHasError("Has the expected error", BizObj.TravelTimeInfo, "value cannot be negative.");
		}

		public void TestMinimumTravelTime_PositiveValueShouldNotThrowError()
		{
			BizObj.TravelTime = 120;

			AssertNoErrors("Travel time should have positive value", BizObj.TravelTimeInfo);
		}

		#endregion

		#region Implementation

		protected override DefaultMinimumStayAndTravelTime GetBusinessObjectToClone()
		{
			return new DefaultMinimumStayAndTravelTime();
		}

		protected override DefaultMinimumStayAndTravelTime GetBusinessObjectToSerialise()
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
