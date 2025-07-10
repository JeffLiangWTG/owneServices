using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class DocumentCommandDeliveryRestrictionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateSDR_RN_NKOriginCountryCode()
		{
			DeliveryRestriction.SDR_RN_NKOriginCountryCode = "";
			DeliveryRestriction.SDR_RN_NKDestinationCountryCode = "";
			DeliveryRestriction.SDR_DeliveryRestrictionType = "NON";
			AssertNoErrors(DeliveryRestriction.SDR_RN_NKOriginCountryCodeInfo);
			AssertHasError(DeliveryRestriction.SDR_DeliveryRestrictionTypeInfo, "At least one of the Origin/Delivery cannot be empty.");

			DeliveryRestriction.SDR_RN_NKOriginCountryCode = "XX";
			AssertHasError(DeliveryRestriction.SDR_RN_NKOriginCountryCodeInfo, "Enter a valid selection.");

			DeliveryRestriction.SDR_RN_NKOriginCountryCode = "AU";
			AssertNoErrors(DeliveryRestriction.SDR_RN_NKOriginCountryCodeInfo);
		}

		public void TestValidateSDR_RN_NKDestinationCountryCode()
		{
			DeliveryRestriction.SDR_RN_NKOriginCountryCode = "";
			DeliveryRestriction.SDR_RN_NKDestinationCountryCode = "";
			DeliveryRestriction.SDR_DeliveryRestrictionType = "NON";
			AssertNoErrors(DeliveryRestriction.SDR_RN_NKDestinationCountryCodeInfo);
			AssertHasError(DeliveryRestriction.SDR_DeliveryRestrictionTypeInfo, "At least one of the Origin/Delivery cannot be empty.");

			DeliveryRestriction.SDR_RN_NKDestinationCountryCode = "XX";
			AssertHasError(DeliveryRestriction.SDR_RN_NKDestinationCountryCodeInfo, "Enter a valid selection.");

			DeliveryRestriction.SDR_RN_NKDestinationCountryCode = "AU";
			AssertNoErrors(DeliveryRestriction.SDR_RN_NKDestinationCountryCodeInfo);
		}

		public void TesttValidateSDR_DeliveryRestrictionType()
		{
			DeliveryRestriction.SDR_DeliveryRestrictionType = "";
			AssertHasError(DeliveryRestriction.SDR_DeliveryRestrictionTypeInfo, "Please enter a Delivery Restriction.");

			DeliveryRestriction.SDR_DeliveryRestrictionType = "CNH";
			AssertHasError(DeliveryRestriction.SDR_DeliveryRestrictionTypeInfo, "Enter a valid selection.");

			DeliveryRestriction.SDR_DeliveryRestrictionType = "NON";
			AssertHasError(DeliveryRestriction.SDR_DeliveryRestrictionTypeInfo, "At least one of the Origin/Delivery cannot be empty.");

			DeliveryRestriction.SDR_DeliveryRestrictionType = "UDF";
			AssertNoErrors(DeliveryRestriction.SDR_DeliveryRestrictionTypeInfo);
		}

		public void TestValidateSDR_DeliveryRestrictionMacro()
		{
			DeliveryRestriction.SDR_DeliveryRestrictionType = "NON";
			AssertNoErrors(DeliveryRestriction.SDR_DeliveryRestrictionMacroInfo);

			DeliveryRestriction.SDR_DeliveryRestrictionType = "UDF";
			DeliveryRestriction.SDR_DeliveryRestrictionMacro = "";
			AssertHasError(DeliveryRestriction.SDR_DeliveryRestrictionMacroInfo, "User defined delivery restriction macro cannot be empty.");

			DeliveryRestriction.SDR_DeliveryRestrictionMacro = "Test";
			AssertNoErrors(DeliveryRestriction.SDR_DeliveryRestrictionMacroInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DeliveryRestriction = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
		}

		DocumentCommandDeliveryRestriction DeliveryRestriction;

		#endregion
	}
}
