using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using CargoWise.EntityFramework.Testing;
using CargoWise.Licensing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Moq;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	internal class LicenceInformationControllerTest : TestCaseWithFactory
	{
		public void TestGetLicenceInformation()
		{
			var response = controller.GetLicenceInformation();
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			var result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 3);
			Assert(result.LicenceInformationCollection.Any(li => li.LicenceCode == "AAAAAAAAA"));
			Assert(result.LicenceInformationCollection.Any(li => li.LicenceCode == "BBBBBBBBB"));
			Assert(result.LicenceInformationCollection.Any(li => li.LicenceCode == "CCCCCCCCC"));
		}

		public void TestGetLicenceInformationByCountryCode()
		{
			var response = controller.GetLicenceInformationByCountryCode("AU");
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			var result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 1);
			Assert(result.LicenceInformationCollection.Any(li => li.LicenceCode == "AAAAAAAAA"));
			response = controller.GetLicenceInformationByCountryCode("US");
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 0);
			response = controller.GetLicenceInformationByCountryCode(null);
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 0);
		}

		public void TestGetLicenceInformationByCountryCode_DuplicateTest()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = "AAA" + "AA2" + " Company";
			org.OH_Code = "AAA" + "AA2";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "AA2" + " Name";
			contact.OC_Email = "AA2" + "@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = licenceHeader1.LA_LD;
			clientCompany.LCC_Code = "AA2";
			clientCompany.LCC_OH = org.PK;
			clientCompany.LCC_RN_NKCountryCode = "AU";
			Factory.Save();
			var response = controller.GetLicenceInformationByCountryCode("AU");
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			var result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 1);
			response = controller.GetLicenceInformationByCountryCode("US");
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 0);
		}

		public void TestGetLicenceInformationByDatabaseIds()
		{
			var validDatabaseId = (ZInt)Base27Encoding.Decode(licenceHeader1.Database.DatabaseId);
			var invalidDatabaseId = Base27Encoding.Encode(validDatabaseId + 25);
			var databaseIds = new[] { (string)licenceHeader1.Database.DatabaseId, invalidDatabaseId };
			var response = controller.GetLicenceInformationByDatabaseIds(databaseIds);
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			var result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 1);
			Assert(result.LicenceInformationCollection.Any(li => li.LicenceCode == "AAAAAAAAA"));
			response = controller.GetLicenceInformationByDatabaseIds(null);
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 0);
		}

		public void TestGetLicenceInformationForDisallowedIpAddress()
		{
			controller = GetLicenceInformationController("122.106.28.55");
			var response = controller.GetLicenceInformation();
			AssertEquals(HttpStatusCode.Forbidden, response.StatusCode);
		}

		public void TestgetLicenceInformationInactive()
		{
			var response = controller.GetLicenceInformationInactive();
			AssertEquals(response.StatusCode, HttpStatusCode.OK);
			var result = DeserializeResponse(response);
			AssertEquals(result.LicenceInformationCollection.Count(), 1);
			Assert(result.LicenceInformationCollection.Any(li => li.LicenceCode == "CCCCCCCCC"));
		}

		#region Implementation
		LicenceHeader licenceHeader1;
		LicenceHeader licenceHeader2;
		LicenceHeader licenceHeader3;
		LicenceInformationController controller;
		protected override void SetUp()
		{
			base.SetUp();
			licenceHeader1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			licenceHeader1.ClientCompany.LCC_RN_NKCountryCode = "AU";
			licenceHeader2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			licenceHeader2.ClientCompany.LCC_RN_NKCountryCode = "CA";
			licenceHeader3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			licenceHeader3.Database.LD_IsActive = false;
			Factory.Save();
			controller = GetLicenceInformationController("10.61.165.176");
		}

		static LicenceInformationController GetLicenceInformationController(string ip)
		{
			var mockRequest = new Mock<HttpWorkerRequest>();
			mockRequest.Setup(o => o.GetRemoteAddress()).Returns(ip);
			mockRequest.Setup(o => o.GetRawUrl()).Returns("/api/LicenceInformation/Mocked");
			HttpContext.Current = new HttpContext(mockRequest.Object);
			var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/myaccount/api/LicenceInformation/Mocked");
			request.Properties.Add("MS_HttpConfiguration", new HttpConfiguration());
			return new LicenceInformationController()
			{ Request = request };
		}

		static LicenceInformationResult DeserializeResponse(HttpResponseMessage response)
		{
			var readTask = response.Content.ReadAsStringAsync();
			readTask.Wait();
			var rawData = readTask.Result.Trim('\"');
			var result = JsonConvert.DeserializeObject<LicenceInformationResult>(rawData);
			return result;
		}

		internal class LicenceInformationResult
		{
			public IEnumerable<LicenceInformationController.LicenceInformation> LicenceInformationCollection { get; set; }
		}
		#endregion
	}
}
