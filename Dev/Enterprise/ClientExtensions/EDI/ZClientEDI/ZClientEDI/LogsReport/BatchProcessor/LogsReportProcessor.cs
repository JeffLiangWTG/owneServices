using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.LogsReport;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.LogsReporting.BatchProcessor
{
	internal class LogsReportProcessor : IEmailAttachmentProcessor
	{
		BusinessObjectFactory factory;

		public LogsReportProcessor()
		{
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		public void Process(string xmlData)
		{
			LogsReport report = new LogsReport(xmlData);

			var incident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.IM_IncidentNumber, report.IncidentNumber)) ?? throw new InvalidOperationException(string.Format("Couldn't find incident for code {0}", report.IncidentNumber));

			var attachedZip = incident.DocManagerInfo.AddFileOrDocument(report.LogFilesZip, report.ServiceTaskCode + " logs " + ZDateTime.Now.ToString("yyyyMMdd") + ".zip", "INT");
			attachedZip.IsPublished = false;
			incident.DocManagerInfo.Save();
			Factory.Save();
		}
	}
}
