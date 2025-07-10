using System;
using System.Net;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDGetSubmittedVATDataRequestTest : MTDRequestBaseTest
	{
		public override void TestSuccessfulResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/#001");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.OK
					, MTDTestHelper.GetSampleSubmittedVATData().ToJSON());

				var request = new MTDGetSubmittedVATDataRequest(client, "#001");

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDVATData>(request);

				AssertNotNull("Content", response);
				AssertNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.OK, client.LastHttpStatusCode);
				AssertEquals("periodKey", "#001", response.periodKey);
				AssertEquals("vatDueSales", 10M, response.vatDueSales);
				AssertEquals("vatDueAcquisitions", 5M, response.vatDueAcquisitions);
				AssertEquals("totalVatDue", 15M, response.totalVatDue);
				AssertEquals("vatReclaimedCurrPeriod", 0M, response.vatReclaimedCurrPeriod);
				AssertEquals("netVatDue", 15M, response.netVatDue);
				AssertEquals("totalValueSalesExVAT", 70M, response.totalValueSalesExVAT);
				AssertEquals("totalValuePurchasesExVAT", 75M, response.totalValuePurchasesExVAT);
				AssertEquals("totalValueGoodsSuppliedExVAT", 55M, response.totalValueGoodsSuppliedExVAT);
				AssertEquals("totalAcquisitionsExVAT", 45M, response.totalAcquisitionsExVAT);
				AssertEquals("finalised", true, response.finalised);
			}
		}

		public override void TestFailedResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/#001");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "PERIOD_KEY_INVALID",
					message = "Invalid period key",
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.BadRequest
					, responseErrorInfo.ToJSON());

				var request = new MTDGetSubmittedVATDataRequest(client, "#001");

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDVATData>(request);

				AssertNull("No Content", response);
				AssertNotNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.BadRequest, client.LastHttpStatusCode);
				AssertErrorInfo(client.LastErrorInfo
					, "PERIOD_KEY_INVALID"
					, "Invalid period key");
			}
		}

		protected override MTDRequestBase GetRequest(MTDClient client) => new MTDGetSubmittedVATDataRequest(client, "#101");

		[TestDate(2021, 7, 11, 11, 23, 9, 9)]
		public override void TestGetAsString()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);
				var request = new MTDGetSubmittedVATDataRequest(client, "#001");
				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestDetails = httpRequest.GetAsString();
					AssertEquals("Get submitted VAT return request details", @"Request Details:
Method: GET
Uri: https://test-api.service.hmrc.gov.uk/organisations/vat/1125463/returns/#001
Header: Accept : application/vnd.hmrc.1.0+json
Authorization : Bearer
Gov-Client-Connection-Method : DESKTOP_APP_DIRECT
Gov-Client-Device-ID : 0A61AF61-CFED-4102-AFBD-A65573EADFAA
Gov-Client-Timezone : UTC+10:00
Gov-Client-User-Agent : os-family=Operating%20System%2099&os-version=99.9.99999&device-manufacturer=WiseTechGlobal&device-model=The%20Ultimate%20Machine
Gov-Client-Local-IPs : 192.168.0.1,10.61.220.57
Gov-Client-Local-IPs-Timestamp : 2021-07-11T11:23:09.009Z
Gov-Client-MAC-Addresses : 01%3A23%3A45%3A67%3A89%3AAB%3ACD%3AEF,FE%3ADC%3ABA%3A98%3A76%3A54%3A32%3A10
Gov-Client-Window-Size : width=1024&height=768
Gov-Client-Screens : width=1920&height=1080&scaling-factor=1.75&colour-depth=32,width=3840&height=2160&scaling-factor=1.5&colour-depth=32
Gov-Client-User-IDs : os=TLA
Gov-Client-Multi-Factor : type=OTHER&timestamp=2021-07-11T11%3A18%3A09.009Z&unique-reference=9A80E58C1FDF000E
Gov-Vendor-License-IDs : WTG%20Test=CD81787E3CB58946DAF545D228BD5DAF428D5072141FEB76E327FF00503D01A2
Gov-Vendor-Product-Name : WTG%20Test
Gov-Vendor-Version : WTG%20Test=99.99.9999.9999
Body: <Empty>", requestDetails);
				}
			}
		}
	}
}
