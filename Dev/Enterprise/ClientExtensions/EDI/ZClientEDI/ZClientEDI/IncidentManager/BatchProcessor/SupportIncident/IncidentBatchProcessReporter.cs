using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.IncidentManager
{
	public class IncidentBatchProcessReporter
	{
		public int ProcessedIncidentCount
		{
			get { return GetIncidentCount(ProcessedIncidents); }
		}

		public int UnprocessedIncidentCount
		{
			get { return GetIncidentCount(UnprocessedIncidents); }
		}

		public int ErrorCount
		{
			get { return Errors.Count; }
		}

		public void AddProcessedIncident(SupportIncident incident, string reason)
		{
			AddIncident(incident, reason, ProcessedIncidents);
		}

		public void AddUnprocessedIncident(SupportIncident incident, string reason)
		{
			AddIncident(incident, reason, UnprocessedIncidents);
		}

		public void AddError(string error)
		{
			Errors.Add(error);
		}

		public string SendReport()
		{
			Guid notificationGroup = EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroup;
			HtmlEmailDef email = new HtmlEmailDef();
			email.Subject = "Incidents Batch Process Report for " + ZDate.Today.ToShortDateString();
			email.Body = GetReportBody();

			try
			{
				Env.OutgoingMailManager.CreateAndSave(email, notificationGroup, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.IncidentsBatchProcessNotificationGroupItem));
				return string.Empty;
			}
			catch (EmailSendFailedException e)
			{
				return e.Message;
			}
		}

		const string BuildBranchStyle = "bldbr";
		const string BuildDateStyle = "blddate";
		const string BuildVersionStyle = "bldver";
		const string CheckInDateStyle = "chkdate";
		const string CheckInVersionStyle = "chkver";
		const string AltRowStyle = "row2";

		public string GetReportBody()
		{
			tableOfContents.Clear();

			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<html>");
			builder.AppendLine("<head><style type=\"text/css\">");
			builder.AppendLine("body, td, th { font-family: arial; font-size: 8pt; }");
			builder.AppendLine("th { background-color:Beige }");
			builder.AppendLine("tr." + AltRowStyle + " { background-color:Beige }");
			builder.AppendLine("td { white-space:nowrap; padding-left: 4px; padding-right: 4px; }");
			builder.AppendLine("td." + BuildBranchStyle + " { border-right-style: none; }");
			builder.AppendLine("td." + BuildDateStyle + " { border-right-style: none; border-left-style: none; }");
			builder.AppendLine("td." + BuildVersionStyle + " { border-left-style: none; }");
			builder.AppendLine("td." + CheckInDateStyle + " { border-right-style: none; }");
			builder.AppendLine("td." + CheckInVersionStyle + " { border-left-style: none; }");
			builder.AppendLine("</style></head>");
			builder.AppendLine("<body>");

			builder.AppendLine("<h3>Table of Contents</h3>");
			const string UnprocessedSection = "Unprocessed incidents";
			const string ProcessedSection = "Processed incidents";
			Dictionary<string, List<SupportIncident>> unprocessedByPriority = SplitByPriority(UnprocessedIncidents);

			AddTableOfContentsLink(builder, ExplanationSectionTitle, ExplanationSectionTitle);
			builder.AppendLine("<br />");
			List<string> unprocessedReasons = AppendTableOfContents(builder, UnprocessedSection, unprocessedByPriority);
			builder.AppendLine("<br />");
			List<string> processedReasons = AppendTableOfContents(builder, ProcessedSection, ProcessedIncidents);

			builder.AppendLine("<h3>Current Builds</h3>");
			if (builds != null)
			{
				foreach (string releaseRing in builds.Keys)
				{
					ReleaseBuild build = builds[releaseRing];
					AppendBuild(builder, build);
					builder.AppendLine("<br />");
				}
			}

			AppendExplanationSection(builder);

			AppendIncidentDetails(builder, UnprocessedSection, unprocessedReasons, unprocessedByPriority, true);
			AppendIncidentDetails(builder, ProcessedSection, processedReasons, ProcessedIncidents, false);

			builder.Append("<h3>Errors: ");
			builder.Append(Errors.Count.ToString());
			builder.AppendLine("</h3>");

			if (Errors.Count > 0)
			{
				Errors.Sort();
				foreach (string error in Errors)
				{
					builder.Append("&nbsp; ");
					builder.Append(error);
					builder.AppendLine("<br />");
				}
			}

			builder.AppendLine("</body>");
			builder.AppendLine("</html>");

			return builder.ToString();
		}

		void AddIncident(SupportIncident incident, string reason, Dictionary<string, List<SupportIncident>> incidents)
		{
			List<SupportIncident> incidentList;
			if (!incidents.TryGetValue(reason, out incidentList))
			{
				incidents[reason] = incidentList = new List<SupportIncident>();
			}
			incidentList.Add(incident);
		}

		int GetIncidentCount(Dictionary<string, List<SupportIncident>> incidents)
		{
			int result = 0;
			foreach (List<SupportIncident> incidentList in incidents.Values)
			{
				result += incidentList.Count;
			}
			return result;
		}

		Dictionary<string, List<SupportIncident>> SplitByPriority(Dictionary<string, List<SupportIncident>> incidents)
		{
			Dictionary<string, List<SupportIncident>> result = new Dictionary<string, List<SupportIncident>>(incidents.Count * 2);
			foreach (string reason in incidents.Keys)
			{
				string critical = "Critical: " + reason;
				string nonCritical = "Non-critical: " + reason;
				foreach (SupportIncident incident in incidents[reason])
				{
					bool isCritical = 0 >= string.Compare(incident.IM_Priority.ToString(), Constants.CustomerService.CriticalityCodes.CR3_SingleFunctionNoWorkAround, StringComparison.OrdinalIgnoreCase);
					AddIncident(incident, isCritical ? critical : nonCritical, result);
				}
			}

			return result;
		}

		public void AddBuilds(Dictionary<ZString, ReleaseBuild> builds)
		{
			this.builds = builds;
		}

		Dictionary<ZString, ReleaseBuild> builds;
		readonly Dictionary<string, string> tableOfContents = new Dictionary<string, string>();

		void AddTableOfContentsLink(StringBuilder builder, string key, string text)
		{
			string result;
			if (!tableOfContents.TryGetValue(key, out result))
			{
				result = "toc" + tableOfContents.Count;
				tableOfContents.Add(key, result);
			}

			builder.Append("<a href=\"#");
			builder.Append(result);
			builder.Append("\">");
			builder.Append(text);
			builder.Append("</a>");
		}

		void AddTableOfContentsBookmark(StringBuilder builder, string key)
		{
			string name = tableOfContents[key];
			builder.Append("<a name=\"");
			builder.Append(name);
			builder.AppendLine("\" />");
		}

		List<string> AppendTableOfContents(StringBuilder builder, string description, Dictionary<string, List<SupportIncident>> incidents)
		{
			AddTableOfContentsLink(builder, description, description);
			builder.AppendLine("<br />");
			List<String> sorted = new List<string>();
			foreach (string reason in incidents.Keys)
			{
				sorted.Add(reason);
			}

			sorted.Sort();
			foreach (string reason in sorted)
			{
				AddTableOfContentsLink(builder, reason, reason);
				builder.AppendLine("<br />");
			}

			return sorted;
		}

		const string ExplanationSectionTitle = "Resolving Unprocessed Incidents";

		void AppendExplanationSection(StringBuilder builder)
		{
			AddTableOfContentsBookmark(builder, ExplanationSectionTitle);
			builder.AppendLine("<h3>" + ExplanationSectionTitle + "</h3>");
			builder.AppendLine("<ul>");
			builder.AppendLine("<li>\"Client database has an upgrade method of Blocked\" – manually send an upgrade or change the upgrade method</li>");
			builder.AppendLine("<li>\"RelatedWorkItem not patched to the client’s release ring\" (i.e. a closed WorkItem has a “Checkin To” ring such that the issue has not been resolved in the current build for the client’s release ring) – verify the WorkItem will be resolved in a future build</li>");
			builder.AppendLine("<li>\"RelatedWorkItem’s changes are not in latest ReleaseBuild\" (i.e. a closed WorkItem with a shelf checkin to a ring other than client’s ring has not been promoted to the client’s ring yet) - verify the WorkItem will be resolved in a future build</li>");
			builder.AppendLine("<li>\"RelatedWorkItem has Open Shelf Check In task for client's release ring\" – developer hasn’t finished work yet</li>");
			builder.AppendLine("<li>\"Client is already on a higher release from a different ring\" (i.e. the client’s license ring does not match the ring of the build they are running) – This can only be resolved manually. The license ring needs updating to match or the client needs to be sent a matching release or the incident disposition needs to be changed.</li>");
			builder.AppendLine("<li>\"No RelatedWorkItem\" – a WorkItem may need to be created</li>");
			builder.AppendLine("<li>\"RelatedWorkItem's changes have NOT been patched to current ReleaseBuild\" (i.e. the WorkItem has been checked-in to a build later than the current build) – the current build needs updating</li>");
			builder.AppendLine("</ul>");
		}

		void AppendIncidentDetails(StringBuilder builder,
			string description,
			List<string> reasons,
			Dictionary<string, List<SupportIncident>> incidents,
			bool addHowToFixLink)
		{
			int count = GetIncidentCount(incidents);
			AddTableOfContentsBookmark(builder, description);
			builder.Append("<h3>");
			builder.Append(description);
			builder.Append(": ");
			builder.Append(count.ToString());
			builder.AppendLine("</h3>");

			if (count > 0)
			{
				foreach (string reason in reasons)
				{
					List<SupportIncident> incidentList = incidents[reason];
					incidentList.Sort(
						delegate(SupportIncident a, SupportIncident b)
						{
							int result = string.Compare(a.ClientCode, b.ClientCode);
							if (result == 0)
							{
								result = a.IM_Priority.CompareTo(b.IM_Priority);
							}
							if (result == 0)
							{
								result = string.Compare(a.IM_IncidentNumber, b.IM_IncidentNumber);
							}

							return result;
						});

					AddTableOfContentsBookmark(builder, reason);

					builder.Append("<h4>");
					builder.Append(reason);
					builder.Append(": ");
					builder.Append(incidentList.Count.ToString());
					builder.AppendLine("</h4>");
					if (addHowToFixLink)
					{
						AddTableOfContentsLink(builder, ExplanationSectionTitle, "How To Fix");
					}

					builder.AppendLine("<table border='1' cellpadding='2' style='border-collapse: collapse'>");
					AppendTableHeader1(builder);
					AppendTableHeader2(builder);

					bool oddRow = true;
					foreach (SupportIncident incident in incidentList)
					{
						builder.AppendLine(oddRow ? "<tr>" : "<tr class=\"" + AltRowStyle + "\">");

						AppendIncidentURLCell(builder, incident);
						AppendCloseDateCell(builder, incident);
						AppendCriticalityCell(builder, incident);
						AppendOrgCell(builder, incident);
						AppendReleaseRingCell(builder, incident);
						AppendCurrentVersionCells(builder, incident);
						AppendSentVersionCells(builder, incident);
						AppendCheckInCells(builder, incident);
						AppendDescriptionCell(builder, incident);

						builder.AppendLine("</tr>");
						oddRow = !oddRow;
					}

					builder.AppendLine("</table><br /><br />");
				}
			}
		}

		void AppendTableHeader1(StringBuilder builder)
		{
			builder.AppendLine("<tr>");
			builder.AppendLine("<th colspan='3'>Incident</th>");
			builder.AppendLine("<th colspan='8'>Organization</th>");
			builder.AppendFormat("<th colspan='{0}'>CheckIn</th>", ReleaseRings.List().Count() * 2);
			builder.AppendLine();
			builder.AppendLine("<th>&nbsp;</th>");
			builder.AppendLine("</tr>");
			builder.AppendLine("<tr>");
		}

		void AppendTableHeader2(StringBuilder builder)
		{
			builder.AppendLine("<tr>");
			builder.AppendLine("<th>URL</th>");
			builder.AppendLine("<th>Closed</th>");
			builder.AppendLine("<th>CR</th>");
			builder.AppendLine("<th>Code</th>");
			builder.AppendLine("<th>Ring</th>");
			builder.AppendLine("<th colspan='3'>Current</th>");
			builder.AppendLine("<th colspan='3'>Last Sent</th>");
			foreach (string ringCode in ReleaseRingsLookup.CheckInRingCodes)
			{
				builder.AppendFormat("<th colspan='2'>{0}</th>", ringCode);
				builder.AppendLine();
			}
			builder.AppendLine("<th>Incident Description</th>");
			builder.AppendLine("</tr>");
		}

		void AppendOrgCell(StringBuilder builder, SupportIncident incident)
		{
			using (ConstructCell(builder))
			{
				if (incident.Client != null && !incident.Client.OH_Code.IsEmpty)
				{
					builder.Append(incident.Client.OH_Code);
				}
			}
		}

		void AppendIncidentURLCell(StringBuilder builder, SupportIncident incident)
		{
			using (ConstructCell(builder))
			{
				builder.Append("<a href=\"");
				builder.Append(ShowEditFormUrlHandler.Instance.Create(ClientControllerRegistration.SupportIncident, incident.PK));
				builder.Append("\">");
				builder.Append(incident.IM_IncidentNumber);
				builder.Append("</a>");
			}
		}

		IDisposable ConstructCell(StringBuilder builder, string styleClass)
		{
			if (!string.IsNullOrEmpty(styleClass))
			{
				builder.Append("<td class=\"");
				builder.Append(styleClass);
				builder.Append("\">");
			}
			else
			{
				builder.Append("<td>");
			}

			int cellStart = builder.Length;

			return new CargoWise.Common.DisposableAction(delegate
			{
				if (cellStart == builder.Length)
				{
					builder.Append("&nbsp;");
				}

				builder.Append("</td>");
			});
		}

		IDisposable ConstructCell(StringBuilder builder)
		{
			return ConstructCell(builder, null);
		}

		void AppendCurrentVersionCells(StringBuilder builder, SupportIncident incident)
		{
			ReleaseBuild build = null;
			var database = incident.Database;
			if (database != null)
			{
				build = database.CurrentVersion;
			}

			AppendBuildCells(builder, build);
		}

		void AppendSentVersionCells(StringBuilder builder, SupportIncident incident)
		{
			ReleaseBuild build = null;
			var database = incident.Database;
			if (database != null)
			{
				build = database.SentVersion;
			}

			AppendBuildCells(builder, build);
		}

		void AppendCloseDateCell(StringBuilder builder, SupportIncident incident)
		{
			using (ConstructCell(builder))
			{
				builder.Append(incident.IM_CloseTimeUtc.ToShortDateString());
			}
		}

		void AppendCriticalityCell(StringBuilder builder, SupportIncident incident)
		{
			using (ConstructCell(builder))
			{
				builder.Append(incident.IM_Priority);
			}
		}

		void AppendCheckInCells(StringBuilder builder, SupportIncident incident)
		{
			foreach (string ringCode in ReleaseRingsLookup.CheckInRingCodes)
			{
				var checkInDate = DateTime.MinValue;
				foreach (NewWorkItem workItem in incident.RelatedWorkItems)
				{
					WorkItemProcessTask task = workItem.GetClosedShelfCheckInTasksForReleaseRing(ringCode);
					if (task != null && checkInDate < task.P9_ActualDate)
					{
						checkInDate = task.P9_ActualDate.ToDateTime();
					}
				}

				using (ConstructCell(builder, CheckInDateStyle))
				{
					if (checkInDate != DateTime.MinValue)
					{
						builder.Append(new ZDateTime(checkInDate).ToShortDateString());
					}
				}
			}
		}

		void AppendBuild(StringBuilder builder, ReleaseBuild build)
		{
			builder.Append(build.HL_ReleaseStatus);
			builder.Append(' ');
			builder.Append(build.HL_ExeVersionDate.ToShortDateString());
			builder.Append(' ');
			builder.Append(build.VersionNumber.ToString());
		}

		void AppendBuildCells(StringBuilder builder, ReleaseBuild build)
		{
			using (ConstructCell(builder, BuildBranchStyle))
			{
				if (build != null)
				{
					builder.Append(build.HL_ReleaseStatus);
				}
			}

			using (ConstructCell(builder, BuildDateStyle))
			{
				if (build != null)
				{
					builder.Append(build.HL_ExeVersionDate.ToShortDateString());
				}
			}

			using (ConstructCell(builder, BuildVersionStyle))
			{
				if (build != null)
				{
					builder.Append(build.VersionNumber.ToString());
				}
			}
		}

		void AppendReleaseRingCell(StringBuilder builder, SupportIncident incident)
		{
			using (ConstructCell(builder))
			{
				var database = incident.Database;
				if (database != null && !database.LD_ReleaseRing.IsEmpty)
				{
					builder.Append(database.LD_ReleaseRing);
				}
			}
		}

		void AppendDescriptionCell(StringBuilder builder, SupportIncident incident)
		{
			using (ConstructCell(builder))
			{
				builder.Append(incident.IM_Description);
			}
		}

		#region Lists

		Dictionary<string, List<SupportIncident>> ProcessedIncidents
		{
			get
			{
				if (processedIncidents == null)
				{
					processedIncidents = new Dictionary<string, List<SupportIncident>>();
				}
				return processedIncidents;
			}
		}

		Dictionary<string, List<SupportIncident>> UnprocessedIncidents
		{
			get
			{
				if (unprocessedIncidents == null)
				{
					unprocessedIncidents = new Dictionary<string, List<SupportIncident>>();
				}
				return unprocessedIncidents;
			}
		}

		List<string> Errors
		{
			get
			{
				if (errors == null)
				{
					errors = new List<string>();
				}
				return errors;
			}
		}

		Dictionary<string, List<SupportIncident>> processedIncidents;
		Dictionary<string, List<SupportIncident>> unprocessedIncidents;
		List<string> errors;
		#endregion
		#region Test Helpers
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		internal bool IsIncidentProcessed(SupportIncident incident, out string reason)
		{
			foreach (string key in ProcessedIncidents.Keys)
			{
				foreach (SupportIncident processedIncident in ProcessedIncidents[key])
				{
					if (processedIncident.PK == incident.PK)
					{
						reason = key;
						return true;
					}
				}
			}

			foreach (string key in UnprocessedIncidents.Keys)
			{
				foreach (SupportIncident unprocessedIncident in UnprocessedIncidents[key])
				{
					if (unprocessedIncident.PK == incident.PK)
					{
						reason = key;
						return false;
					}
				}
			}

			reason = null;
			return false;
		}

		internal bool HasError(string error)
		{
			return Errors.Contains(error);
		}
		#endregion
	}
}
