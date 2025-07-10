
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.MasterFiles.Business.LogsReport;
using Enterprise.ZArchitecture.Schema;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.MailManager.LogsRequestProcessor))]

namespace Enterprise.MailManager
{
	public class LogsRequestProcessor
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^" + LogsRequestSubject)]
		public bool ProcessMailItem(MailItem item, ILogger logger)
		{
			ZString[] requestParams = item.MI_Subject.Substring(LogsRequestSubject.Length).Trim().Split(' ');
			ZDateTime dateFromUtc;
			ZDateTime dateToUtc;
			ZString serviceTaskCode = ZString.Empty;
			ZString incidentNumber = ZString.Empty;
			ZString hostServerName = ZString.Empty;
			ZString hostDBName = ZString.Empty;
			ZString hostConnectionServerName = ZString.Empty;
			if (requestParams.Length >= 6 &&
				ZDateTime.TryParseExact(requestParams[0], out dateFromUtc, DateFormat) &&
				ZDateTime.TryParseExact(requestParams[1], out dateToUtc, DateFormat))
			{
				serviceTaskCode = requestParams[2];
				incidentNumber = requestParams[3];
				hostServerName = requestParams[4];
				hostDBName = requestParams[5];
				if (requestParams.Length >= 7)
				{
					hostConnectionServerName = requestParams[6];
				}

				ProcessRequest(dateFromUtc.Date, dateToUtc.Date, serviceTaskCode, incidentNumber, hostServerName, hostDBName, hostConnectionServerName);
				logger.Log(LogType.Information, "Enterprise Report Mailed");
			}

			return true;
		}

		void ProcessRequest(ZDate dateFromUtc, ZDate dateToUtc, string serviceTaskCode, string incidentNumber, string hostServerName, string hostDBName, string hostConnectionServerName)
		{
			var builder = new LogsReportBuilder(dateFromUtc, dateToUtc, serviceTaskCode, incidentNumber, hostServerName, hostDBName, hostConnectionServerName);
			builder.CreateAndSend();
		}

		#region Constants

		public const string DateFormat = "yyyy/MM/dd";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "may be used as key")]
		public const string LogsRequestSubject = "Request Service Task Logs";

		#endregion
	}
}
