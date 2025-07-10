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
	public class MTDObligationsRequestTest : MTDRequestBaseTest
	{
		public override void TestSuccessfulResponse()
		{
			var company = CreateUKCompany();
			var expectedEndPoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 02, 28).ToMTDCompliantFormat()}");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseObligation = new MTDObligations()
				{
					obligations = new MTDObligation[]
					{
						new MTDObligation()
						{
							start = "2017-01-01",
							end = "2017-01-31",
							due = "2017-03-07",
							status = "O",
							periodKey = "18AD"
						},
						new MTDObligation()
						{
							start = "2017-02-01",
							end = "2017-02-28",
							due = "2017-03-07",
							status = "F",
							periodKey = "18AE",
							received = "2017-05-06"
						}
					}
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndPoint
					, System.Net.HttpStatusCode.OK
					, responseObligation.ToJSON());

				var obligationRequest = new MTDGetObligationsRequest(client);

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{obligationRequest.Path}");
				AssertEquals("Endpoint", expectedEndPoint, actualEndpoint);

				var obligations = client.SendRequest<MTDObligations>(obligationRequest);
				var obligationsItems = obligations.obligations.ToArray();

				AssertNull(client.LastErrorInfo);
				AssertNotNull(obligations);
				AssertEquals(HttpStatusCode.OK, client.LastHttpStatusCode);
				AssertEquals("Count", 2, obligationsItems.Length);

				AssertEquals("start", "2017-01-01", obligationsItems[0].start);
				AssertEquals("end", "2017-01-31", obligationsItems[0].end);
				AssertEquals("due", "2017-03-07", obligationsItems[0].due);
				AssertEquals("status", "O", obligationsItems[0].status);
				AssertEquals("periodKey", "18AD", obligationsItems[0].periodKey);

				AssertEquals("start", "2017-02-01", obligationsItems[1].start);
				AssertEquals("end", "2017-02-28", obligationsItems[1].end);
				AssertEquals("due", "2017-03-07", obligationsItems[1].due);
				AssertEquals("status", "F", obligationsItems[1].status);
				AssertEquals("periodKey", "18AE", obligationsItems[1].periodKey);
				AssertEquals("received", "2017-05-06", obligationsItems[1].received);
			}
		}

		public override void TestFailedResponse()
		{
			var company = CreateUKCompany();
			var expectedEndPoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/obligations?from={new ZDate(2017, 01, 01).ToMTDCompliantFormat()}&to={new ZDate(2017, 02, 28).ToMTDCompliantFormat()}");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "INVALID_DATE_RANGE",
					message = "Invalid date range",
					errors = new MTDErrorInfo[]
					{
						new MTDErrorInfo()
						{
							code = "INVALID_DATE_FROM",
							message = "Invalid date from",
							path = "/obligations"
						},
						new MTDErrorInfo()
						{
							code = "INVALID_DATE_TO",
							message = "Invalid date to",
							path = "/dateOfBirth"
						}
					}
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndPoint
					, System.Net.HttpStatusCode.BadRequest
					, responseErrorInfo.ToJSON());

				var obligationRequest = new MTDGetObligationsRequest(client);

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{obligationRequest.Path}");
				AssertEquals("Endpoint", expectedEndPoint, actualEndpoint);

				var obligations = client.SendRequest<MTDObligations>(obligationRequest);
				AssertNull("No Content", obligations);
				AssertNotNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.BadRequest, client.LastHttpStatusCode);
				AssertErrorInfo(client.LastErrorInfo
					, "INVALID_DATE_RANGE"
					, "Invalid date range"
					, ("INVALID_DATE_FROM", "Invalid date from", "/obligations")
					, ("INVALID_DATE_TO", "Invalid date to", "/dateOfBirth"));
			}
		}

		protected override MTDRequestBase GetRequest(MTDClient client) => new MTDGetObligationsRequest(client);

		[TestDate(2021, 7, 11, 11, 23, 12, 127)]
		public override void TestGetAsString()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);
				var obligationRequest = new MTDGetObligationsRequest(client);
				using (var httpRequest = obligationRequest.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestDetails = httpRequest.GetAsString();

					AssertEquals("obligations request details", @"Request Details:
Method: GET
Uri: https://test-api.service.hmrc.gov.uk/organisations/vat/1125463/obligations?from=2017-01-01&to=2017-02-28
Header: Accept : application/vnd.hmrc.1.0+json
Authorization : Bearer
Gov-Client-Connection-Method : DESKTOP_APP_DIRECT
Gov-Client-Device-ID : 0A61AF61-CFED-4102-AFBD-A65573EADFAA
Gov-Client-Timezone : UTC+10:00
Gov-Client-User-Agent : os-family=Operating%20System%2099&os-version=99.9.99999&device-manufacturer=WiseTechGlobal&device-model=The%20Ultimate%20Machine
Gov-Client-Local-IPs : 192.168.0.1,10.61.220.57
Gov-Client-Local-IPs-Timestamp : 2021-07-11T11:23:12.127Z
Gov-Client-MAC-Addresses : 01%3A23%3A45%3A67%3A89%3AAB%3ACD%3AEF,FE%3ADC%3ABA%3A98%3A76%3A54%3A32%3A10
Gov-Client-Window-Size : width=1024&height=768
Gov-Client-Screens : width=1920&height=1080&scaling-factor=1.75&colour-depth=32,width=3840&height=2160&scaling-factor=1.5&colour-depth=32
Gov-Client-User-IDs : os=TLA
Gov-Client-Multi-Factor : type=OTHER&timestamp=2021-07-11T11%3A18%3A12.127Z&unique-reference=9A80E58C1FDF000E
Gov-Vendor-License-IDs : WTG%20Test=CD81787E3CB58946DAF545D228BD5DAF428D5072141FEB76E327FF00503D01A2
Gov-Vendor-Product-Name : WTG%20Test
Gov-Vendor-Version : WTG%20Test=99.99.9999.9999
Body: <Empty>", requestDetails);
				}
			}
		}
	}
}
