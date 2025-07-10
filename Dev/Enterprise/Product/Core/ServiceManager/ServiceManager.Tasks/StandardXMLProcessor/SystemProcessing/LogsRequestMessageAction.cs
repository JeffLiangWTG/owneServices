using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business.LogsReport;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public sealed class LogsRequestMessageAction : IMessageAction
	{
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "factoryProvider")]
		public LogsRequestMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			participants = new List<ITransactionParticipant>(0);

			LogsRequestInfo info = null;
			try
			{
				info = new LogsRequestInfo(message.EM_MessageText);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddWarning("Invalid Usage Request:" + ex.ToString());
				return true;
			}

			ProcessRequest(new ZDate(info.DateFromUtc), new ZDate(info.DateToUtc), info.ServiceTaskCode, info.IncidentNumber, info.HostServerName, info.HostDBName, info.HostConnectionServerName, info.MaxZipSize);
			notifications.Add(new InfoNotification("Logs Request sent " + info.DateFromUtc.ToShortDateString() + " " + info.DateToUtc.ToShortDateString() + " " + info.ServiceTaskCode));
			return true;
		}

		void ProcessRequest(ZDate dateFromUtc, ZDate dateToUtc, string serviceTaskCode, string incidentNumber, string hostServerName, string hostDBName, string hostConnectionServerName, int maxZipSize)
		{
			var builder = new LogsReportBuilder(dateFromUtc, dateToUtc, serviceTaskCode, incidentNumber, hostServerName, hostDBName, hostConnectionServerName, maxZipSize);
			builder.CreateAndSend();
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}
	}
}
