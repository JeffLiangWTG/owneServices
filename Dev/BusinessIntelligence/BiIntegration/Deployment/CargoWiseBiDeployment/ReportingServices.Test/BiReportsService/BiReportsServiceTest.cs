using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Registration;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Bi.Registration.PowerBi.Logistics;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Billing.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class BiReportsServiceTest : TransactionedTestCase
	{
		PowerBiItem ReportItem
		{
			get
			{
				return reportItem = reportItem ?? new PowerBIReportItemForTest();
			}
		}
		PowerBIReportItemForTest reportItem;

		public void TestBasePowerBiBaseReportPath()
		{
			var service = new BiReportsServiceForTest();
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";
			var expectedBasePath = $"cw1api/analytics/loadReport/{service.PowerBiPortalName}/report/ENT/SVR/Analytics";
			AssertEquals(expectedBasePath, new BiReportsServiceForTest().GetPowerBiBaseReportPath("report"));
		}

		public void TestModifyProxyResponseForHtmlAsync()
		{
			var content = new StringContent("Line1: =\"powerbi/" +
				"Line2: RSUtils.getQueryParameterValue('id')" +
				"Line3: '/powerbi/libs");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "?id=1419f3cd-eb25-4c54-be71-e72aaed199ce", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: =\"Glow/cw1api/analytics/loadReport/powerbi/" +
					"Line2: window.wtgbi.reportId" +
					"Line3: '/Glow/cw1api/analytics/loadReport/powerbi/libs", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("'assets/");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "JobProfitDashboard?rs:Embed=true", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("'cw1api/analytics/loadReport/pbirs/assets/", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("'assets/");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "/reports", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("'cw1api/analytics/loadReport/pbirs/assets/", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("<script type=\"text/javascript\" src=\"/powerbi/libs/scripts/visualSandbox.minimal.externals.js\"></script>");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "VisualSandboxMinimal.htm?plugin=WordCloud", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("<script type=\"text/javascript\" src=\"/Glow/cw1api/analytics/loadReport/powerbi/libs/scripts/visualSandbox.minimal.externals.js\"></script>", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("<script type=\"text/javascript\" src=\"/powerbi/libs/scripts/visualSandbox.minimal.externals.js\"></script>");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "cvSandboxMinimal.htm?plugin=WordCloud", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("<script type=\"text/javascript\" src=\"/Glow/cw1api/analytics/loadReport/powerbi/libs/scripts/visualSandbox.minimal.externals.js\"></script>", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: \"IE=edge\">" +
				"Line2: =\"/powerbi/" +
				"Line3: <script type=\"text/javascript\" src=\"/powerbi/libs/scripts/visualSandbox.minimal.externals.js\"></script>" +
				"Line4: DOMContentLoaded");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "VisualResourceLoader.htm?plugin=WordCloud", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals($"Line1: \"IE=edge\">{System.Environment.NewLine}    <base href=\"/\">" +
					"Line2: =\"Glow/cw1api/analytics/loadReport/powerbi/" +
					"Line3: <script type=\"text/javascript\" crossorigin=\"use-credentials\" src=\"Glow/cw1api/analytics/loadReport/powerbi/libs/scripts/visualSandbox.minimal.externals.js\"></script>" +
					"Line4: load", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("<base href=\"/pbirs/\" >");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "SomeReport?rs:Embed=true", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals($"<base href=\"/Glow/\" >", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("// endinject:js");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};

			var queryFilterStr = ReportQueryStringHelper.GetQueryFilterString(reportItem, "", "AU", "");
			var windowVariables = $"window.wtgbi = {{}}; window.wtgbi.reportFilters = \"{queryFilterStr}\";";
			var jsFilterStr = $"window.wtgbi.reportId = \"\";";

			string sessionRefreshScript(bool shouldSessionBePerpetualInGlow)
			{
				return windowVariables + jsFilterStr + @"
				var BuildAbsoluteUri = function (relativeUri) {
					return '/Glow/' + relativeUri;
				};

				var RenewSession = function()
				{						
					var uriBegin = BuildAbsoluteUri('auth/v2/session/begin');
					var authToken = JSON.parse(window.localStorage.getItem('Glow:/Glow/')).session.authenticationToken
					var beginBody = { 'authenticationToken': authToken, 'tokenType': 1 };
					var myInit = { method: 'POST',
						body: JSON.stringify(beginBody),
						headers: {'content-type':'application/json; charset=utf-8'}};
					var postRequest = new Request(uriBegin, myInit);
					fetch(postRequest).then(function(res) {
						if (res.status === 200) {
							console.log('Session Renewed');
						}
						else {
							console.error('BeginSession Error: ' + res.status);
							console.error(res);
						}
					});
				};

				if (" + shouldSessionBePerpetualInGlow.ToString().ToLower() + @"){
					window.setInterval(function ()
					{
						RenewSession();
					}, 540000); // 9 mins
				}
				else {
					window.onunload = () => {
					   window.localStorage.removeItem('Glow:/Glow/')
					}
				}"; // JS script
			}

			Task.Run(() =>
			{
				var shipmentProfileReport = new PowerBIReportItemForTest();
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(shipmentProfileReport, "SomeReport?rs:Embed=true", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals($"{sessionRefreshScript(shipmentProfileReport.ShouldSessionBePerpetualInGLOW)}{System.Environment.NewLine}\t\t\t// endinject:js", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestModifyProxyResponseForJavascriptAsync()
		{
			var content = new StringContent("Line1: return\"powerbi/libs" +
				"Line2: \"/powerbi/api\"");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbireportserverhost.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: return\"Glow/cw1api/analytics/loadReport/powerbi/libs" +
					"Line2: \"/Glow/cw1api/analytics/loadReport/powerbi/api\"", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: \"allow-scripts\"></iframe>");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbireportserverhost.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: \"allow-scripts allow-same-origin\"></iframe>", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: api/v2.0" +
				"Line2: \"/powerbi/?id=\"" +
				"Line3: 302:[function(e,t,n){\"use strict\";" +
				"Line4: if(\"PbixPageLoadComplete\"===e.data.name){v.loadingEnded();" +
				"Line5: assets/");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "app-1419f3cd-eb25-4c54-be71-e72aaed199ce", responseToBeModified, "");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: cw1api/analytics/loadReport/pbirs/api/v2.0" +
					"Line2: \"/Glow/cw1api/analytics/loadReport/powerbi/?id=\"" +
					"Line3: 302:[function(e,t,n){var moddedPath = location.pathname + '?rs:embed=true' + (location.pathname.indexOf('/System/') > 0 ? '' : window.wtgbi.reportFilters); window.history.replaceState('','',moddedPath.replace('/Glow/cw1api/analytics/loadReport/pbirs','/Glow'));\"use strict\";" +
					"Line4: if(\"PbixPageLoadComplete\"===e.data.name){v.loadingEnded();var moddedPath=location.pathname+'?rs:embed=true' + (location.pathname.indexOf('/System/') > 0 ? '' : window.wtgbi.reportFilters);window.history.replaceState('','',moddedPath.replace('/Glow','/Glow/cw1api/analytics/loadReport/pbirs'));" +
					"Line5: cw1api/analytics/loadReport/pbirs/assets/", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: api/v2.0" +
				"Line2: \"/powerbi/?id=\"" +
				"Line3: 302:[function(e,t,n){\"use strict\";" +
				"Line4: if(\"PbixPageLoadComplete\"===e.data.name){v.loadingEnded();" +
				"Line5: assets/");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "app-1419f3cd-eb25-4c54-be71-e72aaed199ce", responseToBeModified, "AU", "EDI");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: cw1api/analytics/loadReport/pbirs/api/v2.0" +
					"Line2: \"/Glow/cw1api/analytics/loadReport/powerbi/?id=\"" +
					"Line3: 302:[function(e,t,n){var moddedPath = location.pathname + '?rs:embed=true' + (location.pathname.indexOf('/System/') > 0 ? '' : window.wtgbi.reportFilters); window.history.replaceState('','',moddedPath.replace('/Glow/cw1api/analytics/loadReport/pbirs','/Glow'));\"use strict\";" +
					"Line4: if(\"PbixPageLoadComplete\"===e.data.name){v.loadingEnded();var moddedPath=location.pathname+'?rs:embed=true' + (location.pathname.indexOf('/System/') > 0 ? '' : window.wtgbi.reportFilters);window.history.replaceState('','',moddedPath.replace('/Glow','/Glow/cw1api/analytics/loadReport/pbirs'));" +
					"Line5: cw1api/analytics/loadReport/pbirs/assets/", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: assets/");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbiportal.common.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: cw1api/analytics/loadReport/pbirs/assets/", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: jt(c.absUrl())");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "app-1419f3cd-eb25-4c54-be71-e72aaed199ce", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: jt(r.url())", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: \"allow-scripts\"");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbiportal.common.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: \"allow-scripts allow-same-origin\"", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1:);powerbi.build=\"/powerbi/libs\"" +
				"Line2: (this.frontEndUrl,this.hostRootPath);");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "reportserverhost.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1:);powerbi.build=\"/Glow/cw1api/analytics/loadReport/powerbi/libs\"" +
					"Line2: (this.frontEndUrl + '/Glow/cw1api/analytics/loadReport',this.hostRootPath);",
					new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1:t=this.sandboxLoaderUri");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbireportserverhost.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1:t='/Glow/cw1api/analytics/loadReport'+this.sandboxLoaderUri",
					new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1:{var i=this.visualSandboxUri,");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbiportal.common.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1:{var i='/Glow/cw1api/analytics/loadReport'+this.visualSandboxUri,",
					new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: \"/powerbi/" +
				"Line2: (this.frontEndUrl,this.hostRootPath);" +
				"Line3: sandbox=\"allow-scripts\"" +
				"Line4: c=i.id" +
				"Line5: y=i.filter||i.$filter" +
				"Line6: loadReport=function(t,i){var c=this;" +
				"Line7: this.staticResourceUri=c," +
				"Line8: JN(r,t,i,c)" +
				"Line9: jsCommon.QueryStringUtil.parseQueryString().id");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "reportServerHostModern.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: \"/Glow/cw1api/analytics/loadReport/powerbi/" +
					"Line2: (this.frontEndUrl + '/Glow/cw1api/analytics/loadReport',this.hostRootPath);" +
					"Line3: sandbox=\"allow-scripts allow-same-origin\"" +
					"Line4: c=window.wtgbi.reportId" +
					"Line5: y=window.wtgbi.reportFilters" +
					"Line6: loadReport=function(t,i){var c=this;lP.powerBIAccessToken='any';" +
					"Line7: this.staticResourceUri=window.resourceLoaderUrl," +
					"Line8: JN(r,t,window.resourceLoaderUrl,c)" +
					"Line9: window.wtgbi.reportId", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("var s=e[s=a[o]]||s;");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbiportal.dependencies.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("var s=e[s=a[o]]||s; s='Glow/cw1api/analytics/loadReport/'+s;", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("baseUrl:\"./\",");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "powerbiportal.dependencies.externals.bundle.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("baseUrl:\"./Glow/cw1api/analytics/loadReport/\",", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestModifyProxyResponseDoesntChangeOtherJsFilesAsync()
		{
			var content = new StringContent("Line1: powerbi/sandbox" +
				"Line2: return\"powerbi/libs" +
				"Line3: \"/powerbi/api\"");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "JsFileNoRuleShouldBeApplied.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: powerbi/sandbox" +
				"Line2: return\"powerbi/libs" +
				"Line3: \"/powerbi/api\"", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();

			content = new StringContent("Line1: return\"allow-scripts\"");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "JsFileNoRuleShouldBeApplied.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: return\"allow-scripts\"", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestModifyProxyResponseForJavascriptHideComponentsAsync()
		{
			var content = new StringContent("Line1: <nav class=\"navbar navbar-default\"" +
											"Line2: class=\"toolbar\"" +
											"Line3: <div class=\"browser\" ng-class=\"{\\'show-hidden\\':display.hidden}\" load-timer=\"Main.Browser\" load-ready=\"isReady()\">");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "app-test.js", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("Line1: <div class=\"container-fluid\"><h4 class=\"Text\">No content found.</h4></div><nav class=\"navbar navbar-default hidden\"" +
							"Line2: class=\"toolbar hidden\"" +
							"Line3: <div class=\"browser hidden\" load-timer=\"Main.Browser\" load-ready=\"isReady()\">", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestModifyProxyResponseReturnsHtmlContentForOtherHtmlFilesAsync()
		{
			var content = new StringContent("'assets/");
			content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "OtherHtmlFiles", responseToBeModified, "AU");
				var byteStream = modifiedResponse.Result.Content.ReadAsStreamAsync();
				AssertEquals("'assets/", new StreamReader(byteStream.Result).ReadToEnd());
			}).Wait();
		}

		public void TestBiSecurityCheckpointsReferToExistingBusinessAreaReports()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "XXX";

			var actualReportCheckpoints = Env.Security.AnalyticsReports.ChildCheckPoints.Select(c => c.HumanReadableName.ToString()).ToList();

			var testBiReportsService = new BiReportsServiceForTest();
			var deploymentFileLoader = new DeploymentFileLoader();

			var analyticsReportItems = deploymentFileLoader.GetPowerBiReports(BiReportCategory.All);
			var expectedReportCheckpoints = analyticsReportItems.Select(r => r.BusinessArea).Distinct();

			AssertContainsExactElementsInAnyOrder("Delete unused SecurityCheckpoints:", expectedReportCheckpoints, actualReportCheckpoints);
		}

		public void TestCloneRequest()
		{
			var content = new StringContent("Sample Content");
			content.Headers.ContentType = new MediaTypeHeaderValue("application/javascript");
			var requestToBeCloned = new HttpRequestMessage()
			{
				Version = HttpVersion.Version10,
				Method = HttpMethod.Post,
				RequestUri = new Uri("http://server.com/oldPath"),
				Content = content
			};

#if NETFRAMEWORK
			requestToBeCloned.Properties.Add("Property", "PropertyValue");
#else
			requestToBeCloned.Options.Set(new HttpRequestOptionsKey<string>("Property"), "PropertyValue");
#endif
			requestToBeCloned.Headers.Add("Header", "HeaderValue");

			var clonedRequest = new BiReportsServiceForTest().CloneRequest(requestToBeCloned, new Uri("http://server.com/newPath"));

			AssertEquals(requestToBeCloned.Version, clonedRequest.Version);
			AssertEquals(requestToBeCloned.Method, clonedRequest.Method);
			AssertEquals(requestToBeCloned.Content, clonedRequest.Content);
			AssertEquals("http://server.com/newPath", clonedRequest.RequestUri);

#if NETFRAMEWORK
			AssertEquals(requestToBeCloned.Properties.First().Key, clonedRequest.Properties.First().Key);
#else
			var key = new HttpRequestOptionsKey<string>("Property");

			if (requestToBeCloned.Options.TryGetValue(key, out var value))
			{
				AssertEquals(key, value);
			}
#endif
			AssertEquals(requestToBeCloned.Headers.First().Key, clonedRequest.Headers.First().Key);
		}

		public void TestHttpResponseWithNullContentShouldReturnNotFoundStatusCodeAsync()
		{
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = null
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "OtherHtmlFiles", responseToBeModified, "AU");
				AssertEquals(HttpStatusCode.NotFound, modifiedResponse.Result.StatusCode);
				AssertEquals("No content found", modifiedResponse.Result.Content.ReadAsStringAsync().Result);
				AssertEquals("text/html", modifiedResponse.Result.Content.Headers.ContentType.MediaType);
			}).Wait();
		}

		public void TestHttpResponseWithNullContentTypeShouldNotChangeContentAsync()
		{
			var content = new StringContent("content");
			content.Headers.ContentType = null;
			var responseToBeModified = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content
			};
			Task.Run(() =>
			{
				var modifiedResponse = new BiReportsServiceForTest().ModifyProxyResponse(ReportItem, "OtherHtmlFiles", responseToBeModified, "AU");
				AssertEquals(HttpStatusCode.OK, modifiedResponse.Result.StatusCode);
				AssertEquals(content, modifiedResponse.Result.Content);
			}).Wait();
		}

		[UseSnapshotProtection]
		public void TestGetUserContextInformationFromEnvProxy()
		{
			var factory = new BusinessObjectFactory();

			var homeCompany = factory.NewWithValidTestData<GlbCompany>();
			homeCompany.CompanyName = "homeCompany";
			homeCompany.GC_Code = "HMC";

			var homeBranch = factory.NewWithValidTestData<GlbBranch>();
			homeBranch.GB_BranchName = "homeBranch";
			homeBranch.GB_Code = "HMB";
			homeBranch.GB_GC = homeCompany.PK;

			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "STF";
			staff.GS_GB_HomeBranch = homeBranch.PK;

			var currentCompany = factory.NewWithValidTestData<GlbCompany>();
			currentCompany.CompanyName = "currentCompany";
			currentCompany.GC_Code = "CRC";

			var currentBranch = factory.NewWithValidTestData<GlbBranch>();
			currentBranch.GB_GC = currentCompany.PK;
			currentBranch.GB_BranchName = "currentBranch";
			currentBranch.GB_Code = "CRB";

			factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), homeBranch.PK.ToGuid(), Guid.Empty))
			{
				var serialisedDetails = new BiReportsServiceForTest().GetUserContextInformationFromEnvProxy();
				var userContextInformation = JsonConvert.DeserializeObject<BiReportsService.UserContextInformation>(serialisedDetails);

				AssertEquals("User full name should match the environment staff name.", staff.GS_FullName, userContextInformation.UserFullName);
				AssertEquals("User code should match the environment staff code.", staff.GS_Code, userContextInformation.UserCode);

				AssertEquals("Home company name should match the environment home company name.", homeCompany.CompanyName, userContextInformation.HomeCompanyName);
				AssertEquals("Home company code should match the environment home company code.", homeCompany.GC_Code, userContextInformation.HomeCompanyCode);
				AssertEquals("Home company PK should match the environment home company PK.", homeCompany.PK.ToString(), userContextInformation.HomeCompanyPK);

				AssertEquals("Home branch name should match the environment home branch code.", homeBranch.GB_BranchName, userContextInformation.HomeBranchName);
				AssertEquals("Home branch code should match the environment home branch code.", homeBranch.GB_Code, userContextInformation.HomeBranchCode);
				AssertEquals("Home branch PK should match the environment home branch PK.", homeBranch.PK.ToString(), userContextInformation.HomeBranchPK);

				AssertEquals("Current company name should match the environment current company name.", homeCompany.CompanyName, userContextInformation.CurrentCompanyName);
				AssertEquals("Current company code should match the environment current company code.", homeCompany.GC_Code, userContextInformation.CurrentCompanyCode);
				AssertEquals("Current company PK should match the environment current company PK.", homeCompany.PK.ToString(), userContextInformation.CurrentCompanyPK);

				AssertEquals("Current branch name should match the environment current branch code.", homeBranch.GB_BranchName, userContextInformation.CurrentBranchName);
				AssertEquals("Current branch code should match the environment current branch code.", homeBranch.GB_Code, userContextInformation.CurrentBranchCode);
				AssertEquals("Current branch PK should match the environment current branch PK.", homeBranch.PK.ToString(), userContextInformation.CurrentBranchPK);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), currentBranch.PK.ToGuid(), Guid.Empty))
			{
				var serialisedDetails = new BiReportsServiceForTest().GetUserContextInformationFromEnvProxy();
				var userContextInformation = JsonConvert.DeserializeObject<BiReportsService.UserContextInformation>(serialisedDetails);

				AssertEquals("User full name should match the environment staff name.", staff.GS_FullName, userContextInformation.UserFullName);
				AssertEquals("User code should match the environment staff code.", staff.GS_Code, userContextInformation.UserCode);

				AssertEquals("Home company name should match the environment home company name.", homeCompany.CompanyName, userContextInformation.HomeCompanyName);
				AssertEquals("Home company code should match the environment home company code.", homeCompany.GC_Code, userContextInformation.HomeCompanyCode);
				AssertEquals("Home company PK should match the environment home company PK.", homeCompany.PK.ToString(), userContextInformation.HomeCompanyPK);

				AssertEquals("Home branch name should match the environment home branch code.", homeBranch.GB_BranchName, userContextInformation.HomeBranchName);
				AssertEquals("Home branch code should match the environment home branch code.", homeBranch.GB_Code, userContextInformation.HomeBranchCode);
				AssertEquals("Home branch PK should match the environment home branch PK.", homeBranch.PK.ToString(), userContextInformation.HomeBranchPK);

				AssertEquals("Current company name should match the environment current company name.", currentCompany.CompanyName, userContextInformation.CurrentCompanyName);
				AssertEquals("Current company code should match the environment current company code.", currentCompany.GC_Code, userContextInformation.CurrentCompanyCode);
				AssertEquals("Current company PK should match the environment current company PK.", currentCompany.PK.ToString(), userContextInformation.CurrentCompanyPK);

				AssertEquals("Current branch name should match the environment current branch code.", currentBranch.GB_BranchName, userContextInformation.CurrentBranchName);
				AssertEquals("Current branch code should match the environment current branch code.", currentBranch.GB_Code, userContextInformation.CurrentBranchCode);
				AssertEquals("Current branch PK should match the environment current branch PK.", currentBranch.PK.ToString(), userContextInformation.CurrentBranchPK);
			}
		}

		public void TestReportUsageWithNoErrorReturnsCorrectFields()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), staff.HomeBranch.PK.ToGuid(), staff.HomeDepartment.PK.ToGuid()))
			{
				var endpoint = "/test-url";
				var parameters = "test_param: test_value, param2: value2";
				var timestamp = ZDateTime.UtcNow.ToBestReadableDateTimeString();

				var biReportsService = new BiReportsServiceForTest();
				biReportsService.ReportUsage(endpoint, parameters, ((int)HttpStatusCode.OK).ToString(), "TestDbName", "10", timestamp);

				var helper = new UsageCollectorTestHelper(factory);
				Assert(helper.AssertUsageMessagesContains("BRP", new List<(string, object)>
				{
					(UsageProperties.FeatureCode, "BRP"),
					(UsageProperties.Module, "BusinessIntelligence"),
					(UsageProperties.FeatureDescription, "Replication API"),
					(UsageProperties.OrganisationName, GlbCompany.CurrentCompany.OrgProxy.OH_FullName),
					(UsageProperties.RequestUrl, "/test-url"),
					(UsageProperties.Parameters, "test_param: test_value, param2: value2"),
					(UsageProperties.StatusCode, ((int)HttpStatusCode.OK).ToString()),
					(UsageProperties.DatabaseName, "TestDbName"),
					(UsageProperties.Count, "10"),
					(UsageProperties.Timestamp, timestamp),
					(UsageProperties.UserLoginName, staff.GS_LoginName)
				}));
			}
		}

		public void TestReportUsageWithErrorHandlesAndReports()
		{
			var endpoint = "/test-url";
			var parameters = "test_param: test_value, param2: value2";
			var timestamp = ZDateTime.UtcNow.ToBestReadableDateTimeString();

			var biReportsService = new BiReportsServiceForTest();

			var testErrorMsg = "Test Error Message";
			biReportsService.UsageReporter = _ => throw new Exception(testErrorMsg);

			biReportsService.ReportUsage(endpoint, parameters, "500", "TestDbName", "10", timestamp);

			Assert(ErrorReporter.LastMessageReported.Contains($"Could not report usage of Replication API. Error: {testErrorMsg}"));
			ErrorReporter.Clear();
		}
	}
}
