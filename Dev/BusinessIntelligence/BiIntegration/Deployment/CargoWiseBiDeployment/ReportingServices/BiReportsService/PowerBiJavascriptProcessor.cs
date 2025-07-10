using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion
	public class PowerBiJavascriptProcessor
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public Dictionary<string, ((string oldValue, string newValue)[] replacementValuePair, bool stopProcessingMoreRules)> Rules { get; } = new Dictionary<string, ((string oldValue, string newValue)[], bool stopProcessingMoreRules)>();
		public PowerBiJavascriptProcessor(string powerBiPortalName)
		{
			var rules = new[] { ("return\"powerbi/libs", "return\"Glow/cw1api/analytics/loadReport/powerbi/libs"),
							("\"allow-scripts\"></iframe>", "\"allow-scripts allow-same-origin\"></iframe>"),
							("\"/powerbi/api\"", "\"/Glow/cw1api/analytics/loadReport/powerbi/api\""),
							("t=this.sandboxLoaderUri","t='/Glow/cw1api/analytics/loadReport'+this.sandboxLoaderUri")
							};
			Rules.Add("powerbireportserverhost.bundle.js", (rules, true));
			rules = new[] { ("api/v2.0", $"cw1api/analytics/loadReport/{powerBiPortalName}/api/v2.0"),
							("\"/powerbi/?id=\"", "\"/Glow/cw1api/analytics/loadReport/powerbi/?id=\""),
							("302:[function(e,t,n){", $"302:[function(e,t,n){{var moddedPath = location.pathname + '?rs:embed=true' + (location.pathname.indexOf('/System/') > 0 ? '' : window.wtgbi.reportFilters); window.history.replaceState('','',moddedPath.replace('/Glow/cw1api/analytics/loadReport/{powerBiPortalName}','/Glow'));"),
							("if(\"PbixPageLoadComplete\"===e.data.name){v.loadingEnded();", $"if(\"PbixPageLoadComplete\"===e.data.name){{v.loadingEnded();var moddedPath=location.pathname+'?rs:embed=true' + (location.pathname.indexOf('/System/') > 0 ? '' : window.wtgbi.reportFilters);window.history.replaceState('','',moddedPath.replace('/Glow','/Glow/cw1api/analytics/loadReport/{powerBiPortalName}'));"),
							("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/"),
							("jt(c.absUrl())" , "jt(r.url())"),
							("<nav class=\"navbar navbar-default\"", "<div class=\"container-fluid\"><h4 class=\"Text\">No content found.</h4></div><nav class=\"navbar navbar-default hidden\""),
							("class=\"toolbar\"", "class=\"toolbar hidden\""),
							("<div class=\"browser\" ng-class=\"{\\'show-hidden\\':display.hidden}\" load-timer=\"Main.Browser\" load-ready=\"isReady()\">", "<div class=\"browser hidden\" load-timer=\"Main.Browser\" load-ready=\"isReady()\">")
			};
			Rules.Add("app-", (rules, true));
			rules = new[] { ("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/"),
							("\"allow-scripts\"", "\"allow-scripts allow-same-origin\""),
							("{var i=this.visualSandboxUri,", "{var i='/Glow/cw1api/analytics/loadReport'+this.visualSandboxUri,")
			};
			Rules.Add("powerbiportal.common.bundle.js", (rules, true));

			rules = new[] {
				(");powerbi.build=\"/powerbi/libs\"", ");powerbi.build=\"/Glow/cw1api/analytics/loadReport/powerbi/libs\""),
				("(this.frontEndUrl,this.hostRootPath);", "(this.frontEndUrl + '/Glow/cw1api/analytics/loadReport',this.hostRootPath);")
			};
			Rules.Add("reportserverhost.js", (rules, true));

			rules = new[] {
				("\"/powerbi/", "\"/Glow/cw1api/analytics/loadReport/powerbi/"),
				("(this.frontEndUrl,this.hostRootPath);", "(this.frontEndUrl + '/Glow/cw1api/analytics/loadReport',this.hostRootPath);"),
				("sandbox=\"allow-scripts\"", "sandbox=\"allow-scripts allow-same-origin\""),
				("c=i.id" , "c=window.wtgbi.reportId"),
				("y=i.filter||i.$filter" , "y=window.wtgbi.reportFilters"),
				("loadReport=function(t,i){var c=this;" , "loadReport=function(t,i){var c=this;lP.powerBIAccessToken='any';"),
				("this.staticResourceUri=c,", "this.staticResourceUri=window.resourceLoaderUrl,"),
				("JN(r,t,i,c)", "JN(r,t,window.resourceLoaderUrl,c)"),
				("jsCommon.QueryStringUtil.parseQueryString().id", "window.wtgbi.reportId")
			};
			Rules.Add("reportServerHostModern.js", (rules, true));

			if (!ReportDeloyer.IsLegacyPowerBiServer)
			{
				rules = new[] {
					("var s=e[s=a[o]]||s;", "var s=e[s=a[o]]||s; s='Glow/cw1api/analytics/loadReport/'+s;")
				};
				Rules.Add("powerbiportal.dependencies.bundle.js", (rules, true));

				rules = new[] {
					("baseUrl:\"./\",", "baseUrl:\"./Glow/cw1api/analytics/loadReport/\",")
				};
				Rules.Add("powerbiportal.dependencies.externals.bundle.js", (rules, true));
			}

			rules = new[] { ("api/v2.0", $"cw1api/analytics/loadReport/{powerBiPortalName}/api/v2.0"),
							("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/")
			};
			Rules.Add("polyfills", (rules, true));

			rules = new[] { ("api/v2.0", $"cw1api/analytics/loadReport/{powerBiPortalName}/api/v2.0"),
							("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/")
			};
			Rules.Add("runtime", (rules, true));

			rules = new[] { ("api/v2.0", $"cw1api/analytics/loadReport/{powerBiPortalName}/api/v2.0"),
							("assets/", $"cw1api/analytics/loadReport/{powerBiPortalName}/assets/")
			};
			Rules.Add("main", (rules, true));

			rules = new[] { (",f=this._scriptLoadedDelegate;", $",f=this._scriptLoadedDelegate; if(b.src) b.src='/Glow/cw1api/analytics/loadReport'+b.src;"),
							("Type._registerScript(\"Timer.js\",[\"MicrosoftAjaxComponentModel.js\"]);", $""),
							("var c=a._events[d];", "var c=[]; if(a._events) c=a._events[d];")
			};
			Rules.Add("ScriptResource.axd", (rules, true));
		}

		public string ProcessJavascript(string jsFileName, string jsContent)
		{
			foreach (var fileRule in Rules)
			{
				if (jsFileName.Contains(fileRule.Key, StringComparison.OrdinalIgnoreCase))
				{
					foreach (var replacementValuePair in fileRule.Value.replacementValuePair)
					{
						jsContent = jsContent.Replace(replacementValuePair.oldValue, replacementValuePair.newValue);
					}

					if (fileRule.Value.stopProcessingMoreRules)
					{
						break;
					}
				}
			}
			return jsContent;
		}

		public AnalyticsReportDeployer ReportDeloyer
		{
			get
			{
				if (reportDeloyer == null)
				{
					reportDeloyer = new AnalyticsReportDeployer(null);
				}

				return reportDeloyer;
			}
		}
		AnalyticsReportDeployer reportDeloyer { get; set; }
	}
	#endregion
}
