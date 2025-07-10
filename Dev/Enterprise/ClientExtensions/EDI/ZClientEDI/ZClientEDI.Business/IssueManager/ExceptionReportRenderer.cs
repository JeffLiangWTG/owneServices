using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public sealed class ExceptionReportRenderer
	{
		public static readonly ImmutableHashSet<string> SelfCloseTags = ImmutableHashSet.Create("br");

		public ExceptionReportRenderer(ExceptionReport report)
		{
			this.report = report;
		}

		int stackCount;
		string version;

		public TempFile CreateHtmlFile()
		{
			stackCount = 0;

			TempFile result;

			XmlDocument xmlDoc = null;

			try
			{
				xmlDoc = GetSanitizedXmlDocument(report.Xml);
			}
			catch (XmlException)
			{
				failed = true;
			}

			writer = new OutputWriter();

			if (!failed)
			{
				var xmlVersionNumber = xmlDoc.DocumentElement["VersionNumber"];

				if (xmlVersionNumber != null && xmlVersionNumber.InnerText != null && xmlVersionNumber.InnerText.Length > 0)
				{
					version = xmlVersionNumber.InnerText;
				}

				ProcessExceptionSummary(xmlDoc.DocumentElement);
				var exceptionDetailsNode = xmlDoc.DocumentElement["ExceptionDetails"];
				if (exceptionDetailsNode != null)
				{
					ProcessExceptionDetails(exceptionDetailsNode);
				}

				var previousExceptions = xmlDoc.DocumentElement["PreviousExceptions"];
				if (previousExceptions != null)
				{
					ProcessPreviousExceptionsThrown(previousExceptions, "Previous Exceptions thrown");
				}

				var processDetailsNode = xmlDoc.DocumentElement["ProcessDetails"];
				if (processDetailsNode != null)
				{
					ProcessProcessDetails(processDetailsNode);
				}
				var httpRequestDetailsNode = xmlDoc.DocumentElement["HttpRequest"];
				if (httpRequestDetailsNode != null)
				{
					ProcessHttpRequestDetails(httpRequestDetailsNode);
				}
				var factoryDebugNode = xmlDoc.DocumentElement["FactoryDebugInfo"];
				if (factoryDebugNode != null)
				{
					ProcessFactoryDebugInfo(factoryDebugNode);
				}
				var userEvents = xmlDoc.DocumentElement["UserEvents"];
				if (userEvents != null)
				{
					ProcessUserEvents(userEvents, true);
				}
				var sqlEvents = xmlDoc.DocumentElement["SqlEvents"];
				if (sqlEvents != null)
				{
					ProcessSqlEvents(sqlEvents, "Sql Events");
				}
				var sqlFailedEvents = xmlDoc.DocumentElement["SqlFailedEvents"];
				if (sqlFailedEvents != null)
				{
					ProcessSqlEvents(sqlFailedEvents, "Sql Failed Events");
				}
				var dbConnectionEvents = xmlDoc.DocumentElement["ConnectionEvents"];
				if (dbConnectionEvents != null)
				{
					ProcessSqlEvents(dbConnectionEvents, "Connection Events");
				}
				var logs = xmlDoc.DocumentElement["Logs"];
				if (logs != null)
				{
					ProcessLogs(logs);
				}
				var appleCrashReport = xmlDoc.DocumentElement["ExceptionDetails"]?["AppleCrashReport"];
				if (appleCrashReport != null)
				{
					ProcessAppleCrashReport(appleCrashReport, "Crash Report");
				}

				AddMessageBox();

				result = writer.CreateFile();
			}
			else
			{
				result = writer.CreateInvalidFile();
			}

			return result;
		}

		void AddMessageBox()
		{
			writer.WriteBody("<div id=overlay class=overlay><div class=message-box><span id=MsgBoxContent></span> <button class=close-btn onclick=closeMessageBox()>Close</button></div></div>");
		}

		internal XmlDocument GetSanitizedXmlDocument(string xml)
		{
			var xmlDoc = new XmlDocument();
			try
			{
				xmlDoc.LoadXml(xml);
			}
			catch (XmlException)
			{
				//maybe unescaped nulls?
				xml = xml.Replace("\0", "\\0");
				xmlDoc.LoadXml(xml);
			}
			SanitizeAllNodes(xmlDoc);
			return xmlDoc;
		}

		public bool Failed
		{
			get { return failed; }
		}

		readonly ExceptionReport report;
		bool failed;
		OutputWriter writer;

		void ProcessExceptionSummary(XmlNode parentNode)
		{
			writer.WriteBody("<h1>Summary</h1><br>");
			writer.WriteBody("<table>");

			foreach (XmlNode node in parentNode)
			{
				if ((node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlText) || !node.HasChildNodes)
				{
					switch (node.Name)
					{
						case "ExceptionDescription":
							WriteRow(node.Name, "<pre>" + node.InnerText.TrimEnd() + "</pre>");
							break;
						case "TimeOfException":
						case "ExeCreationTime":
							WriteRow(node.Name, GetLocalTimeStrFromDateNode(node)); 
							break;
						default:
							if (HasConcurrencyTable(node))
							{
								WriteConcurrencyTable(node);
							}
							else
							{
								WriteRow(node.Name, FormatStringForHtml(node.InnerText));
							}
							break;
					}
				}
				else if (node.Name == "OpenedForms")
				{
					string openForms = "";
					foreach (XmlNode formNode in node)
					{
						openForms += formNode.InnerText + "<br>";
					}

					WriteRow("Opened Forms", openForms);
				}
			}

			writer.WriteBody("</table>");
		}

		string GetLocalTimeStrFromDateNode(XmlNode node)
		{
			if (node == null)
			{
				return string.Empty;
			}

			var result = ExceptionXml.ParseDateTimeFromXmlNode(node, null);

			if (result.IsEmpty)
			{
				return string.Empty;
			}

			return result.ToLocalBranchTime().ToString("dd-MMM-yy HH:mm:ss", CultureInfo.InvariantCulture);
		}

		void ProcessProcessDetails(XmlNode processDetailsNode)
		{
			writer.WriteHeading("Process Details", true);

			using (writer.WriteClosableElement("table"))
			{
				if (processDetailsNode["PID"] is var pidNode && pidNode != null)
				{
					WriteRow("Process ID", pidNode.InnerText);
				}

				if (processDetailsNode["Name"] is var processName && processName != null)
				{
					WriteRow("Process Name", processName.InnerText);
				}

				if (processDetailsNode["StartTime"] is var pidStartTime && pidStartTime != null)
				{
					WriteRow("Process Start Time", pidStartTime.InnerText);
				}
			}
		}

		void ProcessHttpRequestDetails(XmlNode httpRequestNode)
		{
			writer.WriteHeading("Http Request", true);

			if (httpRequestNode["ClientAddress"] is var clientAddressNode && clientAddressNode != null)
			{
				writer.WriteBody("<b>Client Address: " + FormatStringForHtml(clientAddressNode.InnerText) + "</b>");
			}

			if (httpRequestNode["Headers"] is var headersNode && headersNode != null)
			{
				writer.WriteBody("<h4>Headers</h4>");

				using (writer.WriteClosableElement("table"))
				{
					foreach (XmlNode header in headersNode.GetElementsByTagName("Header"))
					{
						WriteRow(header.Attributes["Name"]?.Value, header.InnerText);
					}
				}
			}
		}

		void ProcessFactoryDebugInfo(XmlNode factoryDebugNode)
		{
			ProcessFactoryStatistics(factoryDebugNode);
			ProcessUncollectedTypes(factoryDebugNode);
		}

		void ProcessFactoryStatistics(XmlNode factoryDebugNode)
		{
			var statisticsNodeMainFactories = factoryDebugNode["FactoryStatistics"]?.SelectNodes("FactoryStatistic");
			var statisticsNodeOtherFactories = factoryDebugNode["OtherFactoryStatistics"]?.SelectNodes("FactoryStatistic");

			if (statisticsNodeMainFactories != null)
			{
				WriteFactoryStatisticsNodesToIssueReport(statisticsNodeMainFactories, "Factory Statistics", string.Empty);
			}

			if (statisticsNodeOtherFactories != null)
			{
				WriteFactoryStatisticsNodesToIssueReport(statisticsNodeOtherFactories, "Other Factory Statistics", "These factories may not be relevant to this issue.");
			}
		}

		void WriteFactoryStatisticsNodesToIssueReport(XmlNodeList statNodeList, string header, string caption)
		{
			if (statNodeList.Count > 0)
			{
				writer.WriteHeading(header, true);
				writer.WriteBody(string.Format(CultureInfo.InvariantCulture, "<p>{0} Factory Count: {1} </p><br /><br />", caption, statNodeList.Count));
				using (writer.WriteClosableElement("table"))
				{
					WriteHeaderRow("Name", "Creation Stack", "Factory Statistics Count", "Details");
					const string threadIdNodeName = "ThreadID";

					var groupStatNodes = statNodeList.OfType<XmlNode>().GroupBy(x => new { name = GetNodeInnerText(x, "Name"), allocationpath = GetNodeInnerText(x, "AllocationPath") }).Select(x => new { x.Key, xmlNode = x });

					foreach (var groupStatNode in groupStatNodes)
					{
						string factoryName = string.Empty;
						if (groupStatNode.Key.name != null)
						{
							factoryName = groupStatNode.Key.name;
						}
						if (string.IsNullOrEmpty(factoryName))
						{
							factoryName = "&lt;Unknown&gt;";
						}

						WriteRow(factoryName,
							groupStatNode.Key.allocationpath.Trim().Replace(" at ", "<br />at ")
								.Replace("\r\nat ", "<br />at "),
							groupStatNode.xmlNode.Count().ToString(),
							AddFactoryStatisticsDetails(groupStatNode.xmlNode, threadIdNodeName)
						);
					}
				}
			}
		}

		string AddFactoryStatisticsDetails(IEnumerable<XmlNode> xmlNodes, string threadIdNodeName)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("<table>");
			string header = GetHeaderRow("Creation Thread ID", "Active Fetch Hints", "Business Objects",
				"Child Factories", "Child Factory IDs", "Database Loads", "Data Rows", "Factory Instance",
				"Creation Time", "Tracked Objects");

			sb.Append(header);
			foreach (var statNode in xmlNodes.OrderBy(n => GetNodeInnerText(n, threadIdNodeName)))
			{
				var threadIdNode = statNode[threadIdNodeName];
				var threadId = threadIdNode != null ? threadIdNode.InnerText : "Unknown";
				var factoryInstance = statNode["FactoryInstance"]?.InnerText;
				factoryInstance = string.IsNullOrEmpty(factoryInstance)
					? "Unknown"
					: factoryInstance;
				var data = GetRow(
							threadId,
							GetNodeInnerText(statNode, "ActiveFetchHintsCount"),
							GetNodeInnerText(statNode, "BusinessObjectCount"),
							GetNodeInnerText(statNode, "ChildFactoriesCount"),
							GetNodeInnerText(statNode, "ChildFactoryIDs"),
							GetNodeInnerText(statNode, "DatabaseLoadCount"),
							GetNodeInnerText(statNode, "DataRowCount"),
							factoryInstance,
							GetNodeInnerText(statNode, "FactoryCreationTime"),
							GetTrackedObjectsHtml(statNode["TrackedInstances"])
						);
				sb.Append(data);
			}

			sb.Append("</table>");

			return sb.ToString();
		}

		string GetTrackedObjectsHtml(XmlNode trackedObjectsNode)
		{
			var html = new StringBuilder();
			if (trackedObjectsNode != null)
			{
				foreach (XmlNode bizoNode in trackedObjectsNode.SelectNodes("Bizo"))
				{
					if (!string.IsNullOrEmpty(bizoNode["Count"]?.Value) && !string.IsNullOrEmpty(bizoNode["Type"]?.Value))
					{
						html.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}x {1}<br />", bizoNode["Count"].InnerText, bizoNode["Type"].InnerText));
					}
				}
			}
			return html.ToString();
		}

		void ProcessUncollectedTypes(XmlNode factoryDebugNode)
		{
			var statisticsNode = factoryDebugNode["UncollectedTypes"];
			if (statisticsNode != null)
			{
				writer.WriteHeading("Uncollected Types", true);

				var uncollectedTypeNodes = statisticsNode.SelectNodes("UncollectedType").Cast<XmlNode>().ToArray();
				if (uncollectedTypeNodes.Length > 0)
				{
					writer.WriteBody(string.Format(CultureInfo.InvariantCulture, "Count: {0}<br /><br />", uncollectedTypeNodes.Length));

					using (writer.WriteClosableElement("table"))
					{
						WriteHeaderRow("Type Name", "Quantity");
						foreach (var uncollectedTypeNode in uncollectedTypeNodes)
						{
							WriteRow(
								GetNodeInnerText(uncollectedTypeNode, "TypeName"),
								GetNodeInnerText(uncollectedTypeNode, "Quantity")
								);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		public void ProcessUserEvents(XmlNode userEventsNode, bool includeTocHyperlink)
		{
			if (userEventsNode != null)
			{
				writer.WriteHeading("User Events", includeTocHyperlink);
				writer.WriteBody("<table>");

				foreach (XmlNode node in userEventsNode)
				{
					if (node.Name == "Event")
					{
						WriteRow(
							node.FirstChild.Value,
							FormatStringForHtml(GetNodeInnerText(node, "Occurrence")),
							FormatStringForHtml(GetNodeInnerText(node, "Data")),
							FormatStringForHtml(GetNodeInnerText(node, "Control")),
							FormatStringForHtml(GetNodeInnerText(node, "Reference")));
					}
				}

				writer.WriteBody("</table>");
			}
		}

		void ProcessSqlEvents(XmlNode sqlEventsNode, string header)
		{
			if (sqlEventsNode != null)
			{
				writer.WriteHeading(header, true);
				writer.WriteBody("<table>");

				int i = 1;
				foreach (XmlNode node in sqlEventsNode)
				{
					if (node.Name == "Command")
					{
						string text = node.InnerText;
						if (text.Length > 20000 + 24) //24 being length of the extra text added
						{
							var bytesSkipped = text.Length - 20000;
							text = text.Substring(0, 20000) + string.Format(CultureInfo.InvariantCulture, "... ({0} bytes skipped)", bytesSkipped);
						}
						WriteRow("Command " + i.ToString(CultureInfo.InvariantCulture), "<pre>" + text + "</pre>");
						i++;
					}
				}

				writer.WriteBody("</table>");
			}
		}

		void ProcessLogs(XmlNode logsNode)
		{
			var logs = logsNode.SelectNodes("Entry").Cast<XmlNode>().ToArray();
			if (logs.Length > 0)
			{
				writer.WriteHeading("Related Logs By Request ID", true);

				writer.WriteBody(string.Format(Culture.Current, "Log Count: {0}<br /><br />", logs.Length));

				foreach (var log in logs)
				{
					using (writer.WriteClosableElement("table"))
					{
						foreach (XmlNode node in log)
						{
							WriteRow(node.Name, node.InnerText);
						}
					}
					writer.WriteBody("<br /><br />");
				}
			}
		}

		void ProcessAppleCrashReport(XmlNode appleCrashNode, string header)
		{
			if (appleCrashNode != null)
			{
				var cdataValue = appleCrashNode.ChildNodes.Item(0).Value;

				writer.WriteHeading(header, true);
				writer.WriteBody("<table>");
				WriteRow(string.Format(CultureInfo.InvariantCulture, "<pre>{0}</pre>", cdataValue));
				writer.WriteBody("</table>");
			}
		}

		#region Exception Details

		void ProcessExceptionDetails(XmlNode exceptionDetailsNode)
		{
			writer.WriteHeading("Exception Details", true);
			ProcessInnerException(exceptionDetailsNode, 0);
			ProcessEnvironmentInfo(exceptionDetailsNode["EnvironmentInfo"]);
		}

		void ProcessPreviousExceptionsThrown(XmlNode previousExceptionsNode, string header)
		{
			if (previousExceptionsNode != null)
			{
				writer.WriteHeading(header, true);
				writer.WriteBody("<table>");

				foreach (XmlNode node in previousExceptionsNode)
				{
					WriteRow(node.Name, "<pre>" + node.InnerText + "</pre>");
				}

				writer.WriteBody("</table>");
			}
		}

		void ProcessStackTrace(XmlNode stackTraceNode)
		{
			StringBuilder callStack = new StringBuilder();
			if (stackTraceNode != null && stackTraceNode.HasChildNodes)
			{
				foreach (XmlNode node in stackTraceNode)
				{
					stackCount++;

					if (node != null && node.Name == "Call" && node.InnerText != null && node.InnerText.Length > 0)
					{
						callStack.Append(node.InnerText);
						var attrs = node.Attributes;

						if (attrs != null && !string.IsNullOrEmpty(attrs["ApproxLine"]?.Value) && !string.IsNullOrEmpty(attrs["Source"]?.Value) && !string.IsNullOrEmpty(attrs["Line"]?.Value))
						{
							// calc line
							var lineNo = Convert.ToInt32(attrs["Line"].Value, CultureInfo.InvariantCulture);
							var line = (lineNo == CalculateLineNo.HIDDEN_LINE_NO) ? "(hidden)" : attrs["Line"].Value;
							var actualLineNo = line;

							if (attrs["ApproxLine"].Value == "true")
							{
								line = "a. " + line;
							}

							line = ": line " + line;
							var source = attrs["Source"]?.Value;
							var vsnetUrl = $"vsnet:{source}#{actualLineNo}";

							var lineNoInfo = "<a" +
								" Name=sourceLineNoHyperLink" + stackCount.ToString(CultureInfo.InvariantCulture) +
								" href=\"" + vsnetUrl + "\"" +
								" Source=" + HttpUtility.HtmlEncode(attrs["Source"].Value) +
								" Line=" + attrs["Line"].Value +
								">" +
								attrs["Source"].Value + line + "</a>";
							callStack.Append(" " + lineNoInfo);
						}

						if (attrs != null && !string.IsNullOrEmpty(version) &&
							!string.IsNullOrEmpty(attrs["Assembly"]?.Value) &&
							!string.IsNullOrEmpty(attrs["Type"]?.Value) &&
							!string.IsNullOrEmpty(attrs["Method"]?.Value) &&
							!string.IsNullOrEmpty(attrs["ILOffset"]?.Value))
						{
							var parameters = (attrs["Parameters"] == null) ? "NoInfo" : attrs["Parameters"].Value;
							var base64AdvanceInfo = attrs["AdvanceInfo"]?.Value ?? string.Empty;

							var advancedLineNoInfo = "<a" +
								" Name=advancedLineNoHyperLink" + stackCount.ToString(CultureInfo.InvariantCulture) +
								" href=\"javascript:void(0)\"" +
								" Version=" + HttpUtility.HtmlEncode(version) +
								" Assembly=" + HttpUtility.HtmlEncode(attrs["Assembly"].Value) +
								" Type=" + HttpUtility.HtmlEncode(attrs["Type"].Value) +
								" Method=" + HttpUtility.HtmlEncode(attrs["Method"].Value) +
								" ILOffset=" + attrs["ILOffset"].Value +
								" Parameters=" + HttpUtility.HtmlEncode(parameters) +
								" AdvanceInfo=" + HttpUtility.HtmlEncode(base64AdvanceInfo) +
								"> [+] </a>";
							callStack.Append(" " + advancedLineNoInfo);
						}

						callStack.Append("<br>");
					}
				}
			}
			else
			{
				callStack.Append(FormatStringForHtml(WebUtility.HtmlEncode("<null>")));
			}

			WriteRow("StackTrace", callStack.ToString());
		}

		void ProcessInnerException(XmlNode exceptionDetailsNode, int level)
		{
			if (level > 0)
			{
				if (HasInnerExceptionDetailsNode(exceptionDetailsNode))
				{
					writer.WriteBody("<h2>Inner Exception Details (" + level.ToString(CultureInfo.InvariantCulture) + ")</h2>");
				}
				else
				{
					writer.WriteHeading("Inner Exception Details (" + level.ToString(CultureInfo.InvariantCulture) + ")", true);
				}
			}

			writer.WriteBody("<table>");

			var nonNullInnerExceptionNodes = new List<XmlNode>();
			foreach (XmlNode node in exceptionDetailsNode)
			{
				if (HasConcurrencyTable(node))
				{
					WriteConcurrencyTable(node);
				}
				else if (HasNewLines(node))
				{
					WriteNewLines(node);
				}
				else if (
					(
						(node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlText) ||
						!node.HasChildNodes
					)
					&& node.Name != "InnerException" && node.Name != "StackTrace")
				{
					var innerText = node.InnerText.Length > 0 ? node.InnerText : "<empty>";
					WriteRow(node.Name, FormatStringForHtml(innerText));
				}

				if (HasComplementaryData(node))
				{
					WriteComplementaryData(node);
				}

				if (node.Name == "InnerException" && node.InnerText != "NULL")
				{
					nonNullInnerExceptionNodes.Add(node);
				}
			}

			ProcessStackTrace(exceptionDetailsNode["StackTrace"]);
			writer.WriteBody("</table>");

			foreach (XmlNode innerExceptionNode in nonNullInnerExceptionNodes)
			{
				ProcessInnerException(innerExceptionNode, level + 1);
			}
		}

		static bool HasComplementaryData(XmlNode node) => node.Name.Equals("Data");

		void WriteComplementaryData(XmlNode node)
		{
			var dataMessage = new StringBuilder();
			if (node.InnerText.Length > 0)
			{
				foreach (var item in node.ChildNodes.Cast<XmlNode>().Where(n => n.Name.Equals("Item")))
				{
					if (item.FirstChild.Name.Equals("Key"))
					{
						var key = item.FirstChild.InnerText;
						var value = item.LastChild.InnerText;
						dataMessage.Append(FormatStringForHtml(WebUtility.HtmlEncode($"{key}: {value}")));
					}
					else
					{
						dataMessage.Append(FormatStringForHtml(node.InnerText));
					}
					dataMessage.Append("\r\n");
				}
			}
			else
			{
				dataMessage.Append(FormatStringForHtml(WebUtility.HtmlEncode("<empty>")));
			}

			WriteRow("Data", FormatStringForHtml(WebUtility.HtmlEncode(dataMessage.ToString())));
		}

		static bool HasNewLines(XmlNode node)
		{
			return node.HasChildNodes && node.ChildNodes[0].Name == "Line";
		}

		void WriteNewLines(XmlNode nodes)
		{
			var lines = new StringBuilder();
			foreach (XmlNode node in nodes)
			{
				if (node != null && node.Name == "Line" && node.InnerText != null && node.InnerText.Length > 0)
				{
					lines.Append(FormatStringForHtml(node.InnerText));
					lines.Append("<br>");
				}
			}
			WriteRow(nodes.Name, lines.ToString());
		}

		static bool HasInnerExceptionDetailsNode(XmlNode exceptionDetailsNode)
		{
			return exceptionDetailsNode["InnerException"] != null && exceptionDetailsNode["InnerException"].InnerText != "NULL";
		}

		public enum LineNoInfoState { NO_LINENO_INFO, HAS_ILOFFSET_INFO, HAS_LINENO_INFO }

		public static LineNoInfoState GetLineNoInfoState(XmlAttributeCollection attrs)
		{
			if (attrs["ApproxLine"] != null)
			{
				return LineNoInfoState.HAS_LINENO_INFO;
			}
			else if (attrs["Assembly"] != null)
			{
				return LineNoInfoState.HAS_ILOFFSET_INFO;
			}
			else
			{
				return LineNoInfoState.NO_LINENO_INFO;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes")]
		internal static LineNoInfoState GetLineNoInfoState(XmlNode exceptionDetailsNode)
		{
			LineNoInfoState state = LineNoInfoState.NO_LINENO_INFO;

			if (exceptionDetailsNode == null)
			{
				return state;
			}

			var stackTraceNode = exceptionDetailsNode["StackTrace"];

			if (stackTraceNode == null)
			{
				return state;
			}

			foreach (XmlNode node in stackTraceNode)
			{
				if (node != null && node.Name == "Call" && node.InnerText != null && node.InnerText.Length > 0)
				{
					var attrsState = GetLineNoInfoState(node.Attributes);

					if (attrsState == LineNoInfoState.HAS_LINENO_INFO)
					{
						return LineNoInfoState.HAS_LINENO_INFO;
					}

					if (attrsState == LineNoInfoState.HAS_ILOFFSET_INFO)
					{
						state = LineNoInfoState.HAS_ILOFFSET_INFO;
					}
				}
			}

			if (HasInnerExceptionDetailsNode(exceptionDetailsNode))
			{
				LineNoInfoState innerExceptionState = GetLineNoInfoState(exceptionDetailsNode["InnerException"]);

				if (innerExceptionState == LineNoInfoState.HAS_LINENO_INFO || innerExceptionState == LineNoInfoState.HAS_ILOFFSET_INFO)
				{
					return innerExceptionState;
				}
			}

			return state;
		}

		void WriteConcurrencyTable(XmlNode node)
		{
			string highlight = @"<span style='background-color: #FF8080;'>DB Changed</span>";
			node.InnerXml = node.InnerXml.Replace("</Line><Line /><Line>", "\r\n\r\n").Replace("<Line /><Line>", "\r\n").Replace("</Line><Line>", "\r\n").Replace("</Line><Line />", "\r\n").Replace("<Line />", "\r\n").Replace("</Line>", "\r\n").Replace("<Line>", "\r\n");
			string text = node.InnerText.Contains("ConcurrencyTableRows") ? node.InnerText : node.InnerXml;

			Regex regex = new Regex("(<|&lt;)(/?)(Column|Original|DB|Changed|Conflict|Concurrency)( ?/?)(>|&gt;)");
			text = regex.Replace(text, "<$2td$4>");
			text = text
				.Replace(WebUtility.HtmlEncode("<ConcurrencyTableRows>"), "<table>" +
					GetHeaderRow("Column", "Original Value", "DB Value", "Changed Value", "Conflict", "Concurrency"))
				.Replace("\r\n", "<br>")
				.Replace(WebUtility.HtmlEncode("</ConcurrencyTableRows>"), "</table>")
				.Replace(WebUtility.HtmlEncode("<ConcurrencyTableRow>"), "<tr>")
				.Replace(WebUtility.HtmlEncode("</ConcurrencyTableRow>"), "</tr>")
				.Replace("DB Changed", highlight);

			WriteRow(node.Name, text);
		}

		bool HasConcurrencyTable(XmlNode node)
		{
			return node.Name.Contains("Message") && (node.InnerText.Contains("ConcurrencyTableRows") || node.InnerXml.Contains("ConcurrencyTableRows"));
		}

		#endregion

		#region Environment Info

		void ProcessEnvironmentInfo(XmlNode environmentInfoNode)
		{
			if (environmentInfoNode != null)
			{
				writer.WriteHeading("Environment Info", true);

				ProcessWebInfoInfo(environmentInfoNode["WebInfo"]);
				ProcessChildNodes(environmentInfoNode, "DatabaseInfo");
				ProcessChildNodes(environmentInfoNode, "AuditDatabaseInfo");
				ProcessChildNodes(environmentInfoNode, "EdwDatabaseInfo");
				ProcessDllVersions(environmentInfoNode["DLLVersions"]);
				ProcessOSInfo(environmentInfoNode["OSInfo"]);
				ProcessDeviceInfo(environmentInfoNode["DeviceInfo"]);
				ProcessChildNodes(environmentInfoNode, "RegionalSettings");
				ProcessPCInfo(environmentInfoNode["PCInfo"]);
				ProcessChildNodes(environmentInfoNode, "SystemResourcesUsage");
			}
		}

		void ProcessWebInfoInfo(XmlNode webInfoNode)
		{
			if (webInfoNode != null)
			{
				writer.WriteBody("<h2>WebInfo</h2>");
				ProcessXmlNodeChildren(webInfoNode);
			}
		}

		void ProcessXmlNode(XmlNode xmlNode)
		{
			writer.WriteBody("<td>" + xmlNode.Name + "</td>");
			writer.WriteBody("<td>");
			ProcessXmlNodeChildren(xmlNode);
			writer.WriteBody("</td>");
		}

		void ProcessXmlNodeChildren(XmlNode xmlNode)
		{
			if (xmlNode.HasChildNodes && xmlNode.ChildNodes.Count == 1 && (xmlNode.ChildNodes[0] is XmlText || xmlNode.ChildNodes[0] is XmlCDataSection))
			{
				writer.WriteBody(xmlNode.ChildNodes[0].InnerText);
			}
			else
			{
				writer.WriteBody("<table>");
				foreach (XmlNode node in xmlNode)
				{
					writer.WriteBody("<tr>");
					ProcessXmlNode(node);
					writer.WriteBody("</tr>");
				}
				writer.WriteBody("</table>");
			}
		}

		void ProcessDllVersions(XmlNode dllVersionsNode)
		{
			if (dllVersionsNode != null)
			{
				writer.WriteBody("<h2>Dll Versions</h2>");
				writer.WriteBody("<table>");

				foreach (XmlNode node in dllVersionsNode)
				{
					string name = (node.Attributes != null && node.Attributes["Name"] != null) ? node.Attributes["Name"].Value : "null";
					string versionText = (node.Attributes != null && node.Attributes["Version"] != null) ? node.Attributes["Version"].Value : "null";
					WriteRow(name, versionText);
				}

				writer.WriteBody("</table>");
			}
		}

		void ProcessDeviceInfo(XmlNode deviceInfoNode)
		{
			if (deviceInfoNode != null)
			{
				foreach (XmlNode xmlNode in deviceInfoNode)
				{
					writer.WriteBody($"<h2>Device Info - {xmlNode.Name}</h2>");
					if (xmlNode.HasChildNodes && xmlNode.ChildNodes.Count == 1 && (xmlNode.ChildNodes[0] is XmlText || xmlNode.ChildNodes[0] is XmlCDataSection))
					{
						writer.WriteBody($"<p> {xmlNode.ChildNodes[0].InnerText} </p>");
					}
					else
					{
						writer.WriteBody("<table>");
						foreach (XmlNode node in xmlNode)
						{
							writer.WriteBody("<tr>");
							ProcessXmlNode(node);
							writer.WriteBody("</tr>");
						}
						writer.WriteBody("</table>");
					}
				}
			}
		}

		void ProcessPCInfo(XmlNode pcInfoNode)
		{
			if (pcInfoNode != null)
			{
				foreach (XmlNode node in pcInfoNode)
				{
					foreach (XmlNode childNode in node)
					{
						writer.WriteBody($"<h2>PC Info - {node.Name} - {childNode.Name}</h2>");
						writer.WriteBody("<table>");
						foreach (XmlNode childChildNode in childNode)
						{
							WriteRow(childChildNode.Name, childChildNode.InnerText);
						}

						writer.WriteBody("</table>");
					}
				}
			}
		}

		void ProcessChildNodes(XmlNode envInfoNode, string nodeTitle)
		{
			if (envInfoNode != null)
			{
				writer.WriteBody("<h2>" + nodeTitle + "</h2>");

				XmlNode childNode = envInfoNode[nodeTitle];
				if (childNode != null)
				{
					writer.WriteBody("<table>");

					foreach (XmlNode node in childNode)
					{
						if ((node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlText) || !node.HasChildNodes)
						{
							WriteRow(node.Name, node.InnerText);
						}
					}

					writer.WriteBody("</table>");
				}
				else
				{
					writer.WriteBody("<p>No Value</p>");
				}
			}
		}

		void ProcessOSInfo(XmlNode osInfoNode)
		{
			writer.WriteBody("<h2>OSInfo</h2>");

			if (osInfoNode is null)
			{
				writer.WriteBody("<p>No Value</p>");
				return;
			}

			writer.WriteBody("<table>");

			var isWindows11 = string.Equals(osInfoNode["InstallationType"]?.InnerText, "Client", StringComparison.Ordinal) &&
				string.Equals(osInfoNode["OSType"]?.InnerText, "Win32NT", StringComparison.Ordinal) &&
				Version.TryParse(osInfoNode["OSVersion"]?.InnerText, out var windowsVersion) &&
				windowsVersion >= new Version(10, 0, 22000) && windowsVersion <= new Version(10, 1);

			foreach (XmlNode node in osInfoNode)
			{
				if ((node.ChildNodes.Count == 1 && node.ChildNodes[0] is XmlText) || !node.HasChildNodes)
				{
					var key = node.Name;
					var value = node.InnerText;

					if (isWindows11 && string.Equals(key, "OSQuickInfo", StringComparison.Ordinal))
					{
						// Windows 11 lies about its version number and pretends to be Windows 10.
						value = string.Format(CultureInfo.InvariantCulture, "Windows 11 (reported as: '{0}')", value);
					}

					WriteRow(key, value);
				}
			}

			writer.WriteBody("</table>");
		}

		#endregion

		void WriteHeaderRow(params string[] headerCells)
		{
			writer.WriteBody(GetHeaderRow(headerCells));
		}

		string GetHeaderRow(params string[] headerCells)
		{
			string body = "<tr>";
			foreach (var header in headerCells)
			{
				body += string.Format(CultureInfo.InvariantCulture, "	<td><b>{0}</b></td>", header);
			}
			return body + "</tr>";
		}

		string GetRow(params string[] headerCells)
		{
			string body = "<tr>";
			foreach (var header in headerCells)
			{
				body += string.Format(CultureInfo.InvariantCulture, "	<td>{0}</td>", header);
			}
			return body + "</tr>";
		}

		void WriteRow(params string[] cellValues)
		{
			writer.WriteBody("<tr>");
			foreach (string value in cellValues)
			{
				writer.WriteBody("	<td>" + value + "</td>");
			}
			writer.WriteBody("</tr>");
		}

		const int MaxHtmlTextLength = 1024 * 1024;

		string FormatStringForHtml(string text)
		{
			text = text.Replace(new string('\\', 10), "");
			text = text.Length > MaxHtmlTextLength ? text.Substring(0, MaxHtmlTextLength) : text;
			return text
				.Replace("\r\n", "<br>")
				.Replace("\n", "<br>");
		}

		void SanitizeAllNodes(XmlNode xmlNode)
		{
			foreach (XmlNode node in xmlNode.ChildNodes)
			{
				if (node is XmlText)
				{
					node.InnerText = WebUtility.HtmlEncode(node.InnerText);
				}
				else
				{
					SanitizeAllNodes(node);
				}
			}
		}

		static string GetNodeInnerText(XmlNode element, string elementName)
		{
			return element[elementName] != null ? element[elementName].InnerText : string.Empty;
		}

		class OutputWriter
		{
			public OutputWriter()
			{
				toc = new StringBuilder();
				body = new StringBuilder();
			}

			public void WriteHeading(string value, bool includeTocHyperlink)
			{
				if (toc.Length != 0)
				{
					toc.Append(" | ");
				}

				toc.Append("<a href=\"#" + value + "\">" + value + "</a>");
				body.Append("<a name=\"" + value + "\"><h1>" + value + "</h1></a>");
				if (includeTocHyperlink)
				{
					body.Append("(<a href=\"#Top\">Top</a>)<br><br>");
				}
			}

			public void AddCalcLineNoButton(string label)
			{
				body.Append("<button name=\"calcLineNoButton\" style=\"width:200\">" + label + "</button><br><br>");
			}

			public void WriteBody(string value)
			{
				body.Append(value + System.Environment.NewLine);
			}

			public IDisposable WriteClosableElement(string elementName)
			{
				body.AppendFormat(CultureInfo.InvariantCulture, "<{0}>", elementName);
				return new DisposableAction(() =>
				{
					body.AppendFormat(CultureInfo.InvariantCulture, "</{0}>", elementName.Split(' ')[0]);
				});
			}

			public TempFile CreateFile()
			{
				TempFile file = TempFile.NewWithExtension("html");
				using (StreamWriter writer = new StreamWriter(file.Filename, false, Encoding.UTF8))
				{
					WriteToStream(writer, true);
				}

				return file;
			}

			public string BodyToString()
			{
				string result;
				using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
				{
					WriteToStream(writer, false);
					result = writer.ToString();
				}

				return result;
			}

			public TempFile CreateInvalidFile()
			{
				body.Append("Invalid Xml.");
				return CreateFile();
			}

			readonly StringBuilder toc;
			readonly StringBuilder body;

			void WriteToStream(TextWriter stream, bool writeTOC)
			{
				stream.WriteLine("<html><head>");
				stream.WriteLine(styleSheet);
				stream.WriteLine("</head><body>");
				if (writeTOC && toc.Length > 0)
				{
					stream.WriteLine(toc.ToString() + "<br>" + System.Environment.NewLine);
				}
				stream.Write(body.ToString());
				stream.WriteLine("</body>");
				stream.WriteLine(script);
				stream.WriteLine("</html>");
			}
		}

		const string styleSheet = @"
					<style type=""text/css"">

					body 
					{
						font-family: arial;
						font-size: 10pt;
						padding: 0 1em;
					}

					span 
					{
						font-family: arial;
						font-size: 10pt;
					}
					
					p 
					{
						margin-top: 0em;
						margin-bottom: 0em;
					}
					
					table 
					{
						border-collapse: collapse;
						border-style: solid;
						border-width: 1px;
					}

					h1
					{
						font-family: arial;
						font-size: 14pt;
						margin: .75em 0 0;
					}

					h2
					{
						font-family: arial;
						font-size: 12pt;
					}
							
					td 
					{
						font-family: arial;
						font-size: 10pt;
						padding-left: 5px;
						padding-right: 2px;
						border-style: solid;
						border-width: 1px;
						vertical-align: top;
					}
					.overlay {
						position: fixed;
						top: 0;
						left: 0;
						width: 100%;
						height: 100%;
						background: rgba(0, 0, 0, 0.5);
						display: flex;
						align-items: center;
						justify-content: center;
						visibility: hidden;
					}

					.message-box {
						background: #fff;
						padding: 20px;
						border-radius: 5px;
						box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
						text-align: center;
						overflow-y: auto;
						max-width: 90%;
						max-height: 90%;
					}

					.overlay.show {
						visibility: visible;
					}

					.close-btn {
						margin-top: 10px;
						padding: 5px 10px;
						background: #007BFF;
						color: white;
						border: none;
						border-radius: 3px;
						cursor: pointer;
					}

					.close-btn:hover {
						background: #0056b3;
					}
					</style>";
		const string script = @"
				<script>
					function closeMessageBox() {
						document.getElementById('overlay').classList.remove('show');
					}
					function openMessageBox() {
						document.getElementById('overlay').classList.add('show');
					}

					var advancedLineNoHyperLinks = Array.from(document.querySelectorAll(""a""))
						.filter(e => e.name && e.name.startsWith(""advancedLineNoHyperLink""));
					advancedLineNoHyperLinks.forEach(e => {
						e.addEventListener(""click"", (obj) => {
							var msgBoxContent = document.getElementById(""MsgBoxContent"");
							var encodedValue = e.getAttribute('advanceinfo');
							if (encodedValue) {
								msgBoxContent.innerText = atob(encodedValue);
							}
							openMessageBox();
						});
					});
</script>
";
	}
}
