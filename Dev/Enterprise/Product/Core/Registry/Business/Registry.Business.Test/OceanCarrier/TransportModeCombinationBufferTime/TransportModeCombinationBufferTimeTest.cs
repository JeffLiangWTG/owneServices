using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportModeCombinationBufferTime))]
	sealed class TransportModeCombinationBufferTimeInHoursTest : RegistryBusinessObjectTemplateTestCase<TransportModeCombinationBufferTime>
	{
		#region Validation

		public void TestBufferTimeInHours_ZeroValueShouldNotThrowError()
		{
			BizObj.BufferTimeInHours = default;

			AssertNoErrors("Buffer time can have Zero value", BizObj.BufferTimeInHoursInfo);
		}

		public void TestBufferTimeInHours_NegativeValueShouldThrowError()
		{
			BizObj.BufferTimeInHours = -1;

			AssertHasError("Has the expected error", BizObj.BufferTimeInHoursInfo, "value cannot be negative.");
		}

		public void TestBufferTimeInHours_PositiveValueShouldNotThrowError()
		{
			BizObj.BufferTimeInHours = 120;

			AssertNoErrors("Buffer time should have positive value", BizObj.BufferTimeInHoursInfo);
		}

		#endregion

		#region Implementation

		protected override TransportModeCombinationBufferTime GetBusinessObjectToClone()
		{
			return new TransportModeCombinationBufferTime();
		}

		protected override TransportModeCombinationBufferTime GetBusinessObjectToSerialise()
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
