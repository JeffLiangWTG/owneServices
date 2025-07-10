using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion
	public class PowerBiJsonProcessor
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		public Dictionary<string, ((string oldValue, string newValue)[] replacementValuePair, bool stopProcessingMoreRules)> Rules { get; } = new Dictionary<string, ((string oldValue, string newValue)[], bool stopProcessingMoreRules)>();
		public PowerBiJsonProcessor(string powerBiPortalName)
		{
			var rules = new[] { ("\"value\":\"/Reports\"", $"\"value\":\"/Glow/cw1api/analytics/loadReport/{powerBiPortalName}\"")
							};
			Rules.Add("WebPortalRelativeUrl", (rules, true));
		}

		public string ProcessJSon(string jsFileName, string jsContent)
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
	}
	#endregion
}
