using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EBookingCarrierConfiguration))]
	sealed class EBookingCarrierConfigurationTest : RegistryBusinessObjectTemplateTestCase<EBookingCarrierConfiguration>
	{
		#region Implementation
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override EBookingCarrierConfiguration GetBusinessObjectToClone()
		{
			return new EBookingCarrierConfiguration
			{
				LastUpdatedTime = new ZDateTime(2016, 2, 8),
				LastResponse = "TEST"
			};
		}

		protected override EBookingCarrierConfiguration GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestEBookingCarrierConfigurationSuccess()
		{
			var testUpdateTime = ZDateTime.Now;
			var eBookingCarrierConfiguration = new EBookingCarrierConfiguration
			{
				LastResponse = "TEST",
				LastUpdatedTime = testUpdateTime
			};
			AssertEquals("TEST", eBookingCarrierConfiguration.LastResponse);
			AssertEquals(testUpdateTime, eBookingCarrierConfiguration.LastUpdatedTime);
		}

		public void TestLastResponseHasErrorOnInvalidJson()
		{
			AssertLastResponseHasError("a string");
			AssertLastResponseHasError(@"{""a"": }");
		}

		public void TestLastResponseHasNoErrorOnValidJson()
		{
			AssertLastResponseHasNoErrors(@" {
	""a"": 1,
	""b"": ""a string"",
	""c"": [""a"", ""b""],
	""d"": [{""a"": 1}, {""b"": 2}]
} ");
		}

		void AssertLastResponseHasError(ZString lastResponse)
		{
			var testUpdateTime = ZDateTime.Now;
			var eBookingCarrierConfiguration = new EBookingCarrierConfiguration
			{
				LastResponse = lastResponse,
				LastUpdatedTime = testUpdateTime
			};
			AssertHasErrors("Expected errors for invalid JSON: " + lastResponse, eBookingCarrierConfiguration.LastResponseInfo);
		}

		void AssertLastResponseHasNoErrors(ZString lastResponse)
		{
			var testUpdateTime = ZDateTime.Now;
			var eBookingCarrierConfiguration = new EBookingCarrierConfiguration
			{
				LastResponse = lastResponse,
				LastUpdatedTime = testUpdateTime
			};
			AssertNoErrors("Expected no errors for JSON: " + lastResponse, eBookingCarrierConfiguration.LastResponseInfo);
		}
		#endregion
	}
}
