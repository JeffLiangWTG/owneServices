using System;
using System.Collections.Generic;
using System.Net;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC.Testing
{
	public class MTDPostVATDataRequestTest : MTDRequestBaseTest
	{
		public override void TestSuccessfulResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseData = new MTDVATSubmitResponseContent()
				{
					processingDate = "2018-01-16T08:20:27.895+0000",
					paymentIndicator = "BANK",
					formBundleNumber = "256660290587",
					chargeRefNumber = "aCxFaNx0FZsCvyWF"
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, System.Net.HttpStatusCode.OK
					, responseData.ToJSON()
					, null
					, new KeyValuePair<string, IEnumerable<string>>[] { new KeyValuePair<string, IEnumerable<string>>("Receipt-ID", new string[] { "e0606fe6233348119cf18023c0fb5271" }) });

				var request = new MTDPostVATDataRequest(client, GetSampleSubmissionData());

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDVATSubmitResponseContent>(request);

				AssertNull(client.LastErrorInfo);
				AssertNotNull(response);
				AssertEquals(HttpStatusCode.OK, client.LastHttpStatusCode);
				AssertEquals("processingDate", "2018-01-16T08:20:27.895+0000", response.processingDate);
				AssertEquals("paymentIndicator", "BANK", response.paymentIndicator);
				AssertEquals("formBundleNumber", "256660290587", response.formBundleNumber);
				AssertEquals("chargeRefNumber", "aCxFaNx0FZsCvyWF", response.chargeRefNumber);
				AssertEquals("ReceiptID", "e0606fe6233348119cf18023c0fb5271", response.ReceiptID);
			}
		}

		public override void TestFailedResponse()
		{
			var company = CreateUKCompany();
			var expectedEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/organisations/vat/{MTDSubmissionDataTestEnvironmentCreator.MockVRN}/returns/");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);

				var responseErrorInfo = new MTDErrorInfo()
				{
					code = "INVALID_REQUEST",
					message = "Multiple Errors",
					errors = new MTDErrorInfo[]
					{
						new MTDErrorInfo()
						{
							code = "INVALID_MONETARY_AMOUNT",
							message = "The value must be between -9999999999999 and 9999999999999",
							path = "/returns"
						},
						new MTDErrorInfo()
						{
							code = "VAT_NET_VALUE",
							message = "netVatDue should be the difference between the largest and the smallest values among totalVatDue and vatReclaimedCurrPeriod",
							path = ""
						}
					}
				};

				ClientHandlerMock.AddJsonResponse(
					expectedEndpoint
					, HttpStatusCode.BadRequest
					, responseErrorInfo.ToJSON());

				var request = new MTDPostVATDataRequest(client, GetSampleSubmissionData());

				var actualEndpoint = FormattableString.Invariant($"{MTDTestHelper.MockHost}/{request.Path}");
				AssertEquals("API Endpoint", expectedEndpoint, actualEndpoint);

				var response = client.SendRequest<MTDVATSubmitResponseContent>(request);

				AssertNull("No Content", response);
				AssertNotNull("Error", client.LastErrorInfo);
				AssertEquals(HttpStatusCode.BadRequest, client.LastHttpStatusCode);
				AssertErrorInfo(client.LastErrorInfo
					, "INVALID_REQUEST"
					, "Multiple Errors"
					, ("INVALID_MONETARY_AMOUNT", "The value must be between -9999999999999 and 9999999999999", "/returns")
					, ("VAT_NET_VALUE", "netVatDue should be the difference between the largest and the smallest values among totalVatDue and vatReclaimedCurrPeriod", ""));
			}
		}

		[TestDate(2017, 12, 11, 10, 09, 08, 07)]
		public override void TestRequestHeaderContent()
		{
			var company = CreateUKCompany();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);
				var request = new MTDPostVATDataRequest(client, GetSampleSubmissionData());

				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestHeader = httpRequest.Headers;
					AssertHeaderContainsKey(requestHeader, "Accept", "application/vnd.hmrc.1.0+json");
					AssertHeaderContainsKey(requestHeader, "Authorization", "Bearer");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Connection-Method", "DESKTOP_APP_DIRECT");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Device-ID", "0A61AF61-CFED-4102-AFBD-A65573EADFAA");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Timezone", "UTC+10:00");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-User-Agent", "os-family=Operating%20System%2099&os-version=99.9.99999&device-manufacturer=WiseTechGlobal&device-model=The%20Ultimate%20Machine");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Local-IPs", "192.168.0.1,10.61.220.57");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Local-IPs-Timestamp", "2017-12-11T10:09:08.007Z");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-MAC-Addresses", "01%3A23%3A45%3A67%3A89%3AAB%3ACD%3AEF,FE%3ADC%3ABA%3A98%3A76%3A54%3A32%3A10");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Window-Size", "width=1024&height=768");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Screens", "width=1920&height=1080&scaling-factor=1.75&colour-depth=32,width=3840&height=2160&scaling-factor=1.5&colour-depth=32");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-User-IDs", "os=TLA");
					AssertHeaderContainsKey(requestHeader, "Gov-Client-Multi-Factor", "type=OTHER&timestamp=2017-12-11T10%3A04%3A08.007Z&unique-reference=9A80E58C1FDF000E");
					AssertHeaderContainsKey(requestHeader, "Gov-Vendor-License-IDs", "WTG%20Test=CD81787E3CB58946DAF545D228BD5DAF428D5072141FEB76E327FF00503D01A2");
					AssertHeaderContainsKey(requestHeader, "Gov-Vendor-Product-Name", "WTG%20Test");
					AssertHeaderContainsKey(requestHeader, "Gov-Vendor-Version", "WTG%20Test=99.99.9999.9999");
				}
			}
		}

		public override void TestRequestBodyContent()
		{
			var company = CreateUKCompany();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = new MTDClient(report);
				var request = new MTDPostVATDataRequest(client, GetSampleSubmissionData());

				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestBody = httpRequest.Content;

					//Assert Content Header
					AssertContentHeaderContainsKey(requestBody.Headers, "Content-Type", "application/json");

					//Assert Content Body
					var expectedRequestBodyContent = GetSampleSubmissionData().ToJSON();
					var actualRequestBodyContent = requestBody.ReadAsStringAsync().Result;
					AssertEquals("Content body", expectedRequestBodyContent, actualRequestBodyContent);
				}
			}
		}

		MTDVATData GetSampleSubmissionData()
		{
			return new MTDVATData()
			{
				finalised = true,
				netVatDue = 15M,
				periodKey = "#001",
				totalAcquisitionsExVAT = 45M,
				totalValueGoodsSuppliedExVAT = 55M,
				totalValuePurchasesExVAT = 75M,
				totalValueSalesExVAT = 70M,
				totalVatDue = 15M,
				vatDueAcquisitions = 5M,
				vatDueSales = 10M,
				vatReclaimedCurrPeriod = 0M
			};
		}

		protected override MTDRequestBase GetRequest(MTDClient client) => new MTDPostVATDataRequest(client, GetSampleSubmissionData());

		[TestDate(2021, 12, 11, 10, 09, 08, 07)]
		public override void TestGetAsString()
		{
			var company = CreateUKCompany();
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var report = SetupReportAndConfiguration(new ZDate(2017, 01, 01), new ZDate(2017, 02, 28));
				var client = SetupClient(report);
				var request = new MTDPostVATDataRequest(client, GetSampleSubmissionData());

				using (var httpRequest = request.GetAsHttpRequest(Helper.TestHttpClient))
				{
					var requestDetails = httpRequest.GetAsString();
					AssertEquals("Refresh Token request details", @"Request Details:
Method: POST
Uri: https://test-api.service.hmrc.gov.uk/organisations/vat/1125463/returns/
Header: Accept : application/vnd.hmrc.1.0+json
Authorization : Bearer
Gov-Client-Connection-Method : DESKTOP_APP_DIRECT
Gov-Client-Device-ID : 0A61AF61-CFED-4102-AFBD-A65573EADFAA
Gov-Client-Timezone : UTC+10:00
Gov-Client-User-Agent : os-family=Operating%20System%2099&os-version=99.9.99999&device-manufacturer=WiseTechGlobal&device-model=The%20Ultimate%20Machine
Gov-Client-Local-IPs : 192.168.0.1,10.61.220.57
Gov-Client-Local-IPs-Timestamp : 2021-12-11T10:09:08.007Z
Gov-Client-MAC-Addresses : 01%3A23%3A45%3A67%3A89%3AAB%3ACD%3AEF,FE%3ADC%3ABA%3A98%3A76%3A54%3A32%3A10
Gov-Client-Window-Size : width=1024&height=768
Gov-Client-Screens : width=1920&height=1080&scaling-factor=1.75&colour-depth=32,width=3840&height=2160&scaling-factor=1.5&colour-depth=32
Gov-Client-User-IDs : os=TLA
Gov-Client-Multi-Factor : type=OTHER&timestamp=2021-12-11T10%3A04%3A08.007Z&unique-reference=9A80E58C1FDF000E
Gov-Vendor-License-IDs : WTG%20Test=CD81787E3CB58946DAF545D228BD5DAF428D5072141FEB76E327FF00503D01A2
Gov-Vendor-Product-Name : WTG%20Test
Gov-Vendor-Version : WTG%20Test=99.99.9999.9999
Body: {""finalised"":true,""netVatDue"":15,""periodKey"":""#001"",""totalAcquisitionsExVAT"":45,""totalValueGoodsSuppliedExVAT"":55,""totalValuePurchasesExVAT"":75,""totalValueSalesExVAT"":70,""totalVatDue"":15,""vatDueAcquisitions"":5,""vatDueSales"":10,""vatReclaimedCurrPeriod"":0}", requestDetails);
				}
			}
		}
	}
}
