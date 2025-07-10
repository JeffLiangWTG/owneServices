using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.DbHealth.Check
{
	public class DbHealthWarningList
	{
		internal void Add(DbHealthWarning warning)
		{
			internalList.Add(warning);
		}

		internal void AddRange(DbHealthWarningList anotherWarningList)
		{
			foreach (DbHealthWarning warning in anotherWarningList)
			{
				internalList.Add(warning);
			}
		}

		internal void Clear()
		{
			internalList.Clear();
		}

		public IEnumerator<DbHealthWarning> GetEnumerator()
		{
			return internalList.GetEnumerator();
		}

		public int Count
		{
			get { return internalList.Count; }
		}

		public DbHealthWarning this[int index]
		{
			get { return internalList[index]; }
		}

		readonly List<DbHealthWarning> internalList = new List<DbHealthWarning>();

		#region Enterprise Server Info

		public string LicenceRegistrationInfo
		{
			get { return licenceRegistrationInfo ?? (licenceRegistrationInfo = Env.CurrentCompany.GetSummaryAndRegistrationText()); }
		}
		string licenceRegistrationInfo;

		public string GetFormattedLicenceInfo(string separator)
		{
			return LicenceRegistrationInfo.Trim().Replace("\r\n", separator);
		}

		public string DbWindowsServerName
		{
			get { return dbWindowsServerName; }
			internal set { dbWindowsServerName = value; }
		}
		string dbWindowsServerName;

		public string DbServerAndInstance
		{
			get { return dbServerAndInstance; }
			internal set { dbServerAndInstance = value; }
		}
		string dbServerAndInstance;

		public string MainDbName
		{
			get { return mainDbName; }
			internal set { mainDbName = value; }
		}
		string mainDbName;

		#endregion

		#region ToHtmlMessage

		public string ToHtmlMessage()
		{
			StringBuilder warningMessage = new StringBuilder();

			if (internalList.Count > 0)
			{
				WriteWarningHeader(warningMessage);

				int i = 0;

				foreach (DbHealthWarning warning in internalList)
				{
					i++;
					WriteWarningDetail(warningMessage, i.ToString(CultureInfo.InvariantCulture), warning);
				}

				WriteWarningFooter(warningMessage);
			}

			return warningMessage.ToString();
		}

		void WriteWarningHeader(StringBuilder warningMessage)
		{
			string foundWarningsInfo = Invariant($"Found {internalList.Count} database health warning(s)");
			string dbWindowsServerInfo = Invariant($"Database Windows Server: {DbWindowsServerName}");
			string dbServerInstanceInfo = Invariant($"Database Server\\Instance: {DbServerAndInstance}");
			string dbNameInfo = Invariant($"Database Name: {MainDbName}");

			warningMessage.Append(Invariant($"<br>&nbsp;{foundWarningsInfo}<br>"));
			warningMessage.Append(Invariant($"<br>&nbsp;{GetFormattedLicenceInfo("<br>")}<br>"));
			warningMessage.Append(Invariant($"<br>&nbsp;{dbWindowsServerInfo}"));
			warningMessage.Append(Invariant($"<br>&nbsp;{dbServerInstanceInfo}"));
			warningMessage.Append(Invariant($"<br>&nbsp;{dbNameInfo}<br><br>"));
			warningMessage.Append(@"<table class=""table"" cellpadding=""3"" cellspacing=""0"" border=""1"">");
			warningMessage.Append(@"<tr class= ""tableheadings"">");
			warningMessage.Append("<th>&nbsp;</th>");
			warningMessage.Append("<th><b>Type</b></th>");
			warningMessage.Append("<th><b>Source</b></th>");
			warningMessage.Append("<th><b>Description</b></th>");
			warningMessage.Append("<th><b>Recommended Action</b></th>");
			warningMessage.Append("</tr>");
		}

		void WriteWarningDetail(StringBuilder warningMessage, string number, DbHealthWarning warning)
		{
			warningMessage.Append("<tr>");
			warningMessage.Append("<td>&nbsp;");
			warningMessage.Append(number);
			warningMessage.Append("&nbsp;</td>");
			warningMessage.Append("<td>");
			warningMessage.Append(warning.WarningType);
			warningMessage.Append("</td>");
			warningMessage.Append("<td>");
			warningMessage.Append(warning.SourceType + " " + warning.Source);
			warningMessage.Append("</td>");
			warningMessage.Append("<td>");
			warningMessage.Append(warning.Description.Replace("\n", "<br>"));
			warningMessage.Append("</td>");
			warningMessage.Append("<td>");
			warningMessage.Append(warning.Action.Replace("\n", "<br>"));
			warningMessage.Append("</td>");
			warningMessage.Append("</tr>");
		}

		void WriteWarningFooter(StringBuilder warningMessage)
		{
			warningMessage.Append("</table>");
			warningMessage.Append("<br><br>");
		}

		#endregion
	}
}
