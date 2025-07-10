using System;
using System.Linq;
using System.Net;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDGetLiabilitiesRequestTest : MTDRequestBaseTest
	{
		public override void TestSuccessfulResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/liabilities?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 02, 28).ToMTDCompliantFormat()}");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseData = new MTDLiabilities()
				{
					liabilities = new MTDLiability[]
					{
						new MTDLiability()
						{
							taxPeriod = new MTDTaxPeriod()
							{
								from = "2017-01-01",
								to = "2017-03-31"
							},
							type = "VAT GB",
							originalAmount = 250M,
							outstandingAmount = 100M,
							due = "2017-05-01"
						}
					}
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.OK
					, responseData.ToJSON());

				var request = new MTDGetLiabilitiesRequest(client);

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDLiabilities>(request);

				AssertNull(client.LastErrorInfo);
				AssertNotNull(response);
				AssertEquals(HttpStatusCode.OK, client.LastHttpStatusCode);
				AssertEquals("Count", 1, response.liabilities.Count());

				var liability = response.liabilities.ToArray()[0];
				AssertEquals("tax period from", "2017-01-01", liability.taxPeriod.from);
				AssertEquals("tax period to", "2017-03-31", liability.taxPeriod.to);
				AssertEquals("type", "VAT GB", liability.type);
				AssertEquals("originalAmount", 250M, liability.originalAmount);
				AssertEquals("outstandingAmount", 100M, liability.outstandingAmount);
				AssertEquals("due", "2017-05-01", liability.due);
			}
		}

		public override void TestFailedResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/liabilities?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 02, 28).ToMTDCompliantFormat()}");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "NOT_FOUND",
					message = "The remote endpoint has indicated that no associated data is found"
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.BadRequest
					, responseErrorInfo.ToJSON());

				var request = new MTDGetLiabilitiesRequest(client);

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDLiabilities>(request);

				AssertNull("No Content", response);
				AssertNotNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.BadRequest, client.LastHttpStatusCode);
				AssertErrorInfo(client.LastErrorInfo
					, "NOT_FOUND"
					, "The remote endpoint has indicated that no associated data is found");
			}
		}

		protected override MTDRequestBase GetRequest(MTDClient client) => new MTDGetLiabilitiesRequest(client);

		[TestDate(2021, 7, 11, 11, 17, 50, 50)]
		public override void TestGetAsString()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);
				var request = new MTDGetLiabilitiesRequest(client);
				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestDetails = httpRequest.GetAsString();

					AssertEquals("liabilities request details", @"Request Details:
Method: GET
Uri: https://test-api.service.hmrc.gov.uk/organisations/vat/1125463/liabilities?from=2017-01-01&to=2017-02-28
Header: Accept : application/vnd.hmrc.1.0+json
Authorization : Bearer
Gov-Client-Connection-Method : DESKTOP_APP_DIRECT
Gov-Client-Device-ID : 0A61AF61-CFED-4102-AFBD-A65573EADFAA
Gov-Client-Timezone : UTC+10:00
Gov-Client-User-Agent : os-family=Operating%20System%2099&os-version=99.9.99999&device-manufacturer=WiseTechGlobal&device-model=The%20Ultimate%20Machine
Gov-Client-Local-IPs : 192.168.0.1,10.61.220.57
Gov-Client-Local-IPs-Timestamp : 2021-07-11T11:17:50.050Z
Gov-Client-MAC-Addresses : 01%3A23%3A45%3A67%3A89%3AAB%3ACD%3AEF,FE%3ADC%3ABA%3A98%3A76%3A54%3A32%3A10
Gov-Client-Window-Size : width=1024&height=768
Gov-Client-Screens : width=1920&height=1080&scaling-factor=1.75&colour-depth=32,width=3840&height=2160&scaling-factor=1.5&colour-depth=32
Gov-Client-User-IDs : os=TLA
Gov-Client-Multi-Factor : type=OTHER&timestamp=2021-07-11T11%3A12%3A50.050Z&unique-reference=9A80E58C1FDF000E
Gov-Vendor-License-IDs : WTG%20Test=CD81787E3CB58946DAF545D228BD5DAF428D5072141FEB76E327FF00503D01A2
Gov-Vendor-Product-Name : WTG%20Test
Gov-Vendor-Version : WTG%20Test=99.99.9999.9999
Body: <Empty>", requestDetails);
				}
			}
		}
	}
}
