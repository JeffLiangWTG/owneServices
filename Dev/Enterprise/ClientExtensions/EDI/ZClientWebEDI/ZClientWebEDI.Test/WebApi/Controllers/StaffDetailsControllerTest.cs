using System.Net;
using System.Web;
using System.Web.Http.Results;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class StaffDetailsControllerTest : TestCaseWithFactory
	{
		public void TestGetUserEmailAddress()
		{
			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserEmailAddress(TestUsername);
				var okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestEmail, okResult.Content);
			}
		}

		public void TestGetUserEmailAddressFromInvalidIp()
		{
			SetRequestContext("8.8.8.8");

			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserEmailAddress(TestUsername);
				var notOkResult = response as NegotiatedContentResult<string>;
				AssertEquals("Invalid IP", notOkResult.Content);
				AssertEquals(HttpStatusCode.Forbidden, notOkResult.StatusCode);
			}
		}

		public void TestGetUserEmailAddressIgnoreCase()
		{
			using (var controller = new StaffDetailsController())
			{
				var uppercaseUsername = TestUsername.ToUpperInvariant();
				var response = controller.GetUserEmailAddress(uppercaseUsername);
				var okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestEmail, okResult.Content);
				var lowercaseUsername = TestUsername.ToLowerInvariant();
				okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestEmail, okResult.Content);
				AssertNotEquals(uppercaseUsername, lowercaseUsername);
			}
		}

		public void TestGetUserEmailAddressNotFound()
		{
			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserEmailAddress("Unknown.UsernameNotExist");
				Assert(response is NotFoundResult);
			}
		}

		public void TestGetUserEmailAddressWithDomainPrefix()
		{
			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserEmailAddress($"CORP\\{TestUsername}");
				var okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestEmail, okResult.Content);
				response = controller.GetUserEmailAddress($"ANYDOMAIN\\{TestUsername}");
				okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestEmail, okResult.Content);
			}
		}

		public void TestGetUserEmailAddressWithInvalidUsernames()
		{
			using (var controller = new StaffDetailsController())
			{
				foreach (var username in new[] { "", " ", System.Environment.NewLine, "a space", ":Þ~", "bad/slash", "a_really_long_username_over_35_chars" })
				{
					var response = controller.GetUserEmailAddress(username);
					AssertType<BadRequestErrorMessageResult>($"username [{username}] should give a BadRequestErrorMessageResult", response);
					var notOkResult = response as BadRequestErrorMessageResult;
					AssertEquals($"Invalid Username [{username}]", notOkResult.Message);
				}
			}
		}

		public void TestGetUserEmailAddressWithValidUsernames()
		{
			using (var controller = new StaffDetailsController())
			{
				foreach (var username in new[] { "some_user", "ihavea.hyphenated-surname", "super_long_username_under_35_chars" })
				{
					CreateStaff(username.Substring(0, 3).ToUpper(), username, username, $"{username}@wisetechglobal.com");
					Factory.Save();

					var response = controller.GetUserEmailAddress(username);
					var okResult = response as OkNegotiatedContentResult<string>;
					AssertEquals($"{username}@wisetechglobal.com", okResult.Content);
				}
			}
		}

		public void TestGetUserStaffCode()
		{
			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserStaffCode(TestUsername);
				var okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestStaffCode, okResult.Content);
			}
		}

		public void TestGetUserStaffCodeIgnoreCase()
		{
			using (var controller = new StaffDetailsController())
			{
				var uppercaseUsername = TestUsername.ToUpperInvariant();
				var response = controller.GetUserStaffCode(uppercaseUsername);
				var okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestStaffCode, okResult.Content);
				var lowercaseUsername = TestUsername.ToLowerInvariant();
				response = controller.GetUserStaffCode(lowercaseUsername);
				okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestStaffCode, okResult.Content);
				AssertNotEquals(uppercaseUsername, lowercaseUsername);
			}
		}

		public void TestGetUserStaffCodeNotFound()
		{
			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserStaffCode("Unknown.UsernameNotExist");
				Assert(response is NotFoundResult);
			}
		}

		public void TestGetUserStaffCodeWithDomainPrefix()
		{
			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserStaffCode($"CORP\\{TestUsername}");
				var okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestStaffCode, okResult.Content);
				response = controller.GetUserStaffCode($"ANYDOMAIN\\{TestUsername}");
				okResult = response as OkNegotiatedContentResult<string>;
				AssertEquals(TestStaffCode, okResult.Content);
			}
		}

		public void TestGetUserStaffCodeFromInvalidIp()
		{
			SetRequestContext("8.8.8.8");

			using (var controller = new StaffDetailsController())
			{
				var response = controller.GetUserStaffCode(TestUsername);
				var notOkResult = response as NegotiatedContentResult<string>;
				AssertEquals("Invalid IP", notOkResult.Content);
				AssertEquals(HttpStatusCode.Forbidden, notOkResult.StatusCode);
			}
		}

		public void TestGetUserStaffCodeWithInvalidUsernames()
		{
			using (var controller = new StaffDetailsController())
			{
				foreach (var username in new[] { "", " ", System.Environment.NewLine, "a space", ":Þ~", "bad/slash", "a_really_long_username_over_35_chars" })
				{
					var response = controller.GetUserStaffCode(username);
					AssertType<BadRequestErrorMessageResult>($"username [{username}] should give a BadRequestErrorMessageResult", response);
					var notOkResult = response as BadRequestErrorMessageResult;
					AssertEquals($"Invalid Username [{username}]", notOkResult.Message);
				}
			}
		}

		public void TestGetUserStaffCodeWithValidUsernames()
		{
			using (var controller = new StaffDetailsController())
			{
				foreach (var username in new[] { "some_user", "ihavea.hyphenated-surname", "super_long_username_under_35_chars" })
				{
					CreateStaff(username.Substring(0, 3).ToUpper(), username, username, $"{username}@wisetechglobal.com");
					Factory.Save();

					var response = controller.GetUserStaffCode(username);
					var okResult = response as OkNegotiatedContentResult<string>;
					AssertEquals(username.Substring(0, 3).ToUpper(), okResult.Content);
				}
			}
		}

		GlbStaff CreateStaff(string staffCode, string loginName, string fullName, string email)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = staffCode;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		void SetRequestContext(string ip)
		{
			var mockRequest = new Mock<HttpWorkerRequest>();
			mockRequest.Setup(o => o.GetRemoteAddress()).Returns(ip);
			mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/StaffDetails");
			HttpContext.Current = new HttpContext(mockRequest.Object);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CreateStaff(TestStaffCode, TestUsername, "Sta Ff", TestEmail);
			Factory.Save();
			SetRequestContext("10.61.165.176");
		}

		const string TestUsername = "sta.ff";
		const string TestStaffCode = "STF";
		const string TestEmail = "sta.ff@wisetechglobal.com";
	}
}
