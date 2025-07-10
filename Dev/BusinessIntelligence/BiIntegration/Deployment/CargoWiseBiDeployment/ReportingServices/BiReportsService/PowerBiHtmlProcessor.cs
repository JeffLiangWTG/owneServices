using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Bi.Registration.PowerBi;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion
	class PowerBiHtmlProcessor
	{
		public Dictionary<string, ((string oldValue, string newValue)[] replacementValuePair, bool stopProcessingMoreRules)> Rules { get; } = new Dictionary<string, ((string oldValue, string newValue)[], bool stopProcessingMoreRules)>();

		string SessionRefreshScript(bool shouldSessionBePerpetualInGLOW)
		{
			return @"
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

				if (" + shouldSessionBePerpetualInGLOW.ToString().ToLower() + @"){
					window.setInterval(function ()
					{
						RenewSession();
					}, 540000); // 9 mins
				}
				else {
					window.onunload = () => {
					   window.localStorage.removeItem('Glow:/Glow/')
					}
				}
			"; // JS script
		}

		public PowerBiHtmlProcessor(PowerBiItem reportItem, string powerBiPortalName, string companyCode, string countryCode, string branchCode)
		{
			string reportParameters = AddReportParameters(reportItem, companyCode, countryCode, branchCode);

			var rules = new[] { ("/ReportServer/", $"/Glow/cw1api/analytics/loadReport/ReportServer/"),
								("./ReportViewer.aspx", $"/Glow/cw1api/analytics/loadReport/ReportServer/Pages/ReportViewer.aspx"),
								("/ReportServer?", $"/Glow/cw1api/analytics/loadReport/ReportServer?"),
								($"<base href=\"/{powerBiPortalName}/\" >", $"<base href=\"/Glow/\" >")
			};
			Rules.Add("ReportServer", (rules, true));

			rules = new[] {
				("=\"powerbi/", "=\"Glow/cw1api/analytics/loadReport/powerbi/"),
				(" <script type=\"text/javascript\">",$" <script type=\"text/javascript\"> {reportParameters} // endinject:js"),
				("RSUtils.getQueryParameterValue('id')", "window.wtgbi.reportId"),
				("'/powerbi/libs" ,"'/Glow/cw1api/analytics/loadReport/powerbi/libs"),
				($"<base href=\"/{powerBiPortalName}/\" >", $"<base href=\"/Glow/\" >")
			};
			Rules.Add("?id=", (rules, true));

			rules = new[] { ("'assets/", $"'cw1api/analytics/loadReport/{powerBiPortalName}/assets/"),
						($"<base href=\"/{powerBiPortalName}/\" >", $"<base href=\"/Glow/\" >"),
						("// endinject:js", $"{reportParameters}// endinject:js"),
						("<script type=\"text/javascript\">",$"<script type=\"text/javascript\"> {reportParameters} // endinject:js")
				};
			if (reportItem != null)
			{
				rules = new[] { ("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/"),
						($"<base href=\"/{powerBiPortalName}/\" >", $"<base href=\"/Glow/\" >"),
						("// endinject:js", $"{reportParameters}// endinject:js"),
						("<script type=\"text/javascript\">",$"<script type=\"text/javascript\"> {reportParameters} // endinject:js")
				};
			}
			Rules.Add("rs:Embed=true", (rules, true));

			rules = new[] { ("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/"),
						($"<base href=\"/{powerBiPortalName}/\" >", $"<base href=\"/Glow/\" >"),
						("// endinject:js", $"{reportParameters}// endinject:js")
			};
			Rules.Add("reports", (rules, true));

			rules = new[] { ("=\"/powerbi/", $"=\"/Glow/cw1api/analytics/loadReport/powerbi/") };
			Rules.Add("VisualSandboxMinimal.htm", (rules, true));

			rules = new[] { ("=\"/powerbi/", $"=\"/Glow/cw1api/analytics/loadReport/powerbi/") };
			Rules.Add("cvSandboxMinimal.htm", (rules, true));

			rules = new[] {
				("\"IE=edge\">", $"\"IE=edge\">{System.Environment.NewLine}    <base href=\"/\">"),
				("=\"/powerbi/", $"=\"Glow/cw1api/analytics/loadReport/powerbi/"),
				("javascript\" src", "javascript\" crossorigin=\"use-credentials\" src"),
				("DOMContentLoaded", "load")
			};
			Rules.Add("VisualResourceLoader.htm", (rules, true));
		}

		string AddReportParameters(PowerBiItem reportItem, string companyCode, string countryCode, string branchCode)
		{
			var jsFilters = new StringBuilder();
			var queryFilterStr = ReportQueryStringHelper.GetQueryFilterString(reportItem, companyCode, countryCode, branchCode);
			if (queryFilterStr != null)
			{
				jsFilters.Append("window.wtgbi = {}; ");
				jsFilters.Append($"window.wtgbi.reportFilters = \"{queryFilterStr}\";");
				if (reportItem != null)
				{
					jsFilters.Append($"window.wtgbi.reportId = \"{reportItem.Id}\";");
					jsFilters.Append(SessionRefreshScript(reportItem.ShouldSessionBePerpetualInGLOW));
				}
			}
			return jsFilters.ToString();
		}

		public string ProcessHtml(string htmlFileName, string htmlContent)
		{
			foreach (var fileRule in Rules)
			{
				if (htmlFileName.Contains(fileRule.Key, StringComparison.OrdinalIgnoreCase))
				{
					foreach (var replacementValuePair in fileRule.Value.replacementValuePair)
					{
						htmlContent = htmlContent.Replace(replacementValuePair.oldValue, replacementValuePair.newValue);
					}

					if (fileRule.Value.stopProcessingMoreRules)
					{
						break;
					}
				}
			}
			return htmlContent;
		}
	}
	#endregion
}
