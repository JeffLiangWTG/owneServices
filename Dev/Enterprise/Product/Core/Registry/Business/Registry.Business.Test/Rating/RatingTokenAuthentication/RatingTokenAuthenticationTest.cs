using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatingTokenAuthentication))]
	class RatingTokenAuthenticationTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestCacheStaffCollection()
		{
			var staff = Factory.New<IGlbStaff>();
			(staff as BusinessObject).FillWithValidTestData();
			staff.GS_Code = "JWA";
			Factory.Save();

			var tokenAuth = new RatingTokenAuthentication();
			tokenAuth.StaffCode = "JWA";

			RegistryFactory.Instance.TryGetValueFromCacheOnly<IActiveBusinessObjectCollection>("RatingTokenAuthentication.StaffCollection", out var cachedCollection);
			AssertNotNull("the collection should be cached to the RegistryFactory.", cachedCollection);
		}

		public void TestValidateClientId()
		{
			var ratingTokenAuthentication = new RatingTokenAuthentication();

			ratingTokenAuthentication.ClientId = "randomclientid";
			Assert(ratingTokenAuthentication.ClientIdInfo.HasError("The client id should be in GUID format."));

			ratingTokenAuthentication.ClientId = "";
			Assert(ratingTokenAuthentication.ClientIdInfo.HasError("Please enter a value."));

			ratingTokenAuthentication.ClientId = "06255AA6-8544-4465-A7F8-C5481641BEAC";
			Assert(!ratingTokenAuthentication.ClientIdInfo.HasErrors());
		}

		public void TestValidateStaffCode()
		{
			var staff = Factory.New<IGlbStaff>();
			(staff as BusinessObject).FillWithValidTestData();
			staff.GS_Code = "JWA";
			Factory.Save();

			var ratingTokenAuthentication = new RatingTokenAuthentication();

			ratingTokenAuthentication.StaffCode = "AAA";
			Assert(ratingTokenAuthentication.StaffCodeInfo.HasError("Enter a valid selection."));

			ratingTokenAuthentication.StaffCode = "";
			Assert(ratingTokenAuthentication.StaffCodeInfo.HasError("Please enter a value."));

			ratingTokenAuthentication.StaffCode = "JWA";
			Assert(!ratingTokenAuthentication.StaffCodeInfo.HasErrors());
		}

		public void TestValidateEndpoint()
		{
			var ratingTokenAuthentication = new RatingTokenAuthentication();

			ratingTokenAuthentication.Endpoint = "invalidendpoint";
			Assert(ratingTokenAuthentication.EndpointInfo.HasError("Endpoint is in an invalid format"));

			ratingTokenAuthentication.Endpoint = "";
			Assert(ratingTokenAuthentication.EndpointInfo.HasError("Please enter a value."));

			ratingTokenAuthentication.Endpoint = "https://validendpoint.com";
			Assert(!ratingTokenAuthentication.EndpointInfo.HasErrors());
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new RatingTokenAuthentication();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new RatingTokenAuthentication();
		}
	}
}
