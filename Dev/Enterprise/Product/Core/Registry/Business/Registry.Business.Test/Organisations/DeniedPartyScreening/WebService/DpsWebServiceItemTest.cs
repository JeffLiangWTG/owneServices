using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DpsWebServiceItem))]
	sealed class DpsWebServiceItemTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new DpsWebServiceItem
		{
			Code = "DPS1",
			WebServiceUrl = "https://dpsv4.wisegrid.net",
			Role = RoleHelper.Code.Production
		};

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public void TestValidateCode_ShouldDisplayErrorMessage()
		{
			var collection = new DpsWebServiceItemCollection();
			collection.AddRange
			(
				new DpsWebServiceItem { Code = "" },
				new DpsWebServiceItem { Code = "DPS" },
				new DpsWebServiceItem { Code = "DPS1" },
				new DpsWebServiceItem { Code = "DPS1" }
			);
			collection[0].RunPreSaveValidation();
			collection[1].RunPreSaveValidation();
			collection[2].RunPreSaveValidation();

			AssertHasError("Code is empty, should display an error message", collection[0].CodeInfo, "Code should not be empty.");
			AssertHasError("Code length less than 4 characters, should display an error message", collection[1].CodeInfo, "Code Length should be 4.");
			AssertHasError("Code found duplicate, should display an error message", collection[2].CodeInfo, "There are duplicate Codes: DPS1.");

			var item = new DpsWebServiceItem() { Code = "SYD1" };
			item.RunPreSaveValidation();

			AssertNoErrors("Should not have error message", item.CodeInfo);
		}

		public void TestValidateWebServiceURL_ShouldDisplayErrorMessage()
		{
			var collection = new DpsWebServiceItemCollection();
			collection.AddRange
			(
				new DpsWebServiceItem { Code = "DPS1", WebServiceUrl = "" },
				new DpsWebServiceItem { Code = "DPS2", WebServiceUrl = "https://test.net" },
				new DpsWebServiceItem { Code = "DPS3", WebServiceUrl = "https://test.net" }
			);
			collection[0].RunPreSaveValidation();
			collection[2].RunPreSaveValidation();

			AssertHasError("Web Service is empty, should display an error message", collection[0].WebServiceUrlInfo, "Web Service URL should not be empty.");
			AssertHasError("Web Service duplicate URL found, should display an error message", collection[2].WebServiceUrlInfo, "There are duplicate Web Service URLs: https://test.net.");

			var item = new DpsWebServiceItem() { Code = "DPS4", WebServiceUrl = "https://test.net" };
			item.RunPreSaveValidation();

			AssertNoErrors("Should not have error message", item.WebServiceUrlInfo);
		}

		public void TestValidateRole_ShouldDisplayErrorMessage_WhenMoreThanOneSameRoleAsPrdOrStg()
		{
			var collection = new DpsWebServiceItemCollection();
			collection.AddRange
			(
				new DpsWebServiceItem { Code = "DPS1", Role = RoleHelper.Code.ProductionFailover, WebServiceUrl = "https://abc1.com" },
				new DpsWebServiceItem { Code = "DPS2", Role = RoleHelper.Code.ProductionFailover, WebServiceUrl = "https://abc2.com" },
				new DpsWebServiceItem { Code = "DPS3", Role = RoleHelper.Code.Production, WebServiceUrl = "https://abc3.com" },
				new DpsWebServiceItem { Code = "DPS4", Role = RoleHelper.Code.Staging, WebServiceUrl = "https://abc4.com" }
			);

			AssertNoErrors("Should not have error message", collection[0].RoleInfo);

			collection[0].Role = "PRD";
			ValidateHasError("production");

			collection[0].Role = RoleHelper.Code.Staging;
			collection[1].Role = RoleHelper.Code.ProductionFailover;
			ValidateHasError("staging");

			void ValidateHasError(string role) {
				collection[0].RunPreSaveValidation();
				AssertHasError("Should only have one role on web service Url", collection[0].RoleInfo, "One " + role + " web service URL must be specified.");
			}
		}

		public void TestValidateRole_ShouldDisplayErrorMessage_WhenRoleIsEmptyOrInvalid()
		{
			var collection = new DpsWebServiceItemCollection();
			collection.AddRange
			(
				new DpsWebServiceItem { Code = "DPS1", Role = "", WebServiceUrl = "https://abc1.com" },
				new DpsWebServiceItem { Code = "DPS2", Role = "TST", WebServiceUrl = "https://abc2.com" },
				new DpsWebServiceItem { Code = "DPS3", Role = "PRD", WebServiceUrl = "https://abc3.com" }
			);
			collection[0].RunPreSaveValidation();
			AssertHasError("Should not have empty role", collection[0].RoleInfo, "Please enter a Role.");

			collection[1].RunPreSaveValidation();
			AssertHasError("Should have valid Role", collection[1].RoleInfo, "Enter a valid Role.");
		}

		public void TestSchema()
		{
			AssertEquals("Code", DpsWebServiceItem.Schema.Code);
			AssertEquals("WebServiceUrl", DpsWebServiceItem.Schema.WebServiceUrl);
			AssertEquals("Role", DpsWebServiceItem.Schema.Role);
		}

		public void TestValidUrls()
		{
			ValidateUrl("https://abc.com", true);
			ValidateUrl("http://abc.com", true);
			ValidateUrl("http://a", true);
			ValidateUrl("https://abc", true);
			ValidateUrl("https://abc.com/", true);
		}

		public void TestInvalidUrls()
		{
			ValidateUrl("http", false);
			ValidateUrl("https", false);
			ValidateUrl("http:", false);
			ValidateUrl("http://", false);
			ValidateUrl("http:/abc.com", false);
			ValidateUrl("https//abc.com", false);
			ValidateUrl("https:/abc", false);
			ValidateUrl("htps:/abc", false);
			ValidateUrl("abc://pqr", false);
			ValidateUrl("abcdef", false);
		}

		void ValidateUrl(string url, bool isValid)
		{
			var item = new DpsWebServiceItem() { Code = "SYD1", WebServiceUrl = url };
			item.RunPreSaveValidation();
			if (isValid)
			{
				AssertNoErrors("Should not have error message", item.WebServiceUrlInfo);
			}
			else
			{
				AssertHasError("Invalid Web Service URL found, should display an error message", item.WebServiceUrlInfo, "Invalid Web Service URL: " + url + ".");
			}
		}
	}
}
