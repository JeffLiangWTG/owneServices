using System;
using System.Data;
using System.IO;
using System.Net;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.DocumentWrappers;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ICSharpCode.SharpZipLib.Zip;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiReportingQueue : AutoEdiReportingQueue
	{
		public EdiReportingQueue(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ERQ_CreateTimeUtc = ZDateTime.UtcNow;
			ERQ_Status = QueueStatus.NewReport;
		}

		public static class QueueStatus
		{
			public const string NewReport = "NEW";
			public const string Processed = "PCD";
			public const string Failed = "FAL";
		}

		protected ZDateTime Period
		{
			get
			{
				return this.ERQ_Period != 0 ? new ZDateTime(this.ERQ_Period / 100, this.ERQ_Period % 100, 1) : ZDateTime.Empty;
			}
		}

		protected LicenceDatabase Database
		{
			get { return Factory.Load<LicenceDatabase>(ERQ_LD); }
		}

		public override void Delete()
		{
			if (File.Exists(ERQ_ReportFileFullName))
			{
				File.Delete(ERQ_ReportFileFullName);
			}

			base.Delete();
		}

		public void GenerateReport()
		{
			var fullFileName = Path.Combine(EDIDataRegistry.Instance.MyAccountReportsShareFolderPath.Value, string.Concat(PK.ToString(), ".zip"));
			ERQ_ReportFileFullName = fullFileName;

			if (File.Exists(ERQ_ReportFileFullName))
			{
				File.Delete(ERQ_ReportFileFullName);
			}

			FileStream fileStream = null;
			ZipOutputStream compressStream = null;
			try
			{
				//Nested using statements can cause violations of the CA2202 warning.
				fileStream = new FileStream(ERQ_ReportFileFullName, FileMode.Create);
				compressStream = new ZipOutputStream(fileStream);
				fileStream = null;

				var entry = new ZipEntry(string.Concat(ERQ_ReportName, ".csv"));
				compressStream.PutNextEntry(entry);

				using (var streamWriter = new StreamWriter(compressStream))
				{
					compressStream = null;
					GenerateReportCore(streamWriter);
				}
			}
			finally
			{
				if (fileStream != null)
				{
					fileStream.Dispose();
					fileStream = null;
				}

				if (compressStream != null)
				{
					compressStream.Dispose();
					compressStream = null;
				}
			}

			SendNotification();
			ERQ_Status = QueueStatus.Processed;
		}

		protected void SendNotification()
		{
			var emailAddress = SupportStaff?.GS_EmailAddress ?? Contact?.Email ?? ZString.Empty;

			if (!emailAddress.IsEmpty)
			{
				var emailDef = new HtmlEmailDef();
				emailDef.FromAddress = Env.Registry.SMTPDefaultDoNotReplyEmailAddress;
				emailDef.FromDisplayName = "PleaseDoNotReply";
				emailDef.Subject = EDIDataRegistry.Instance.MyAccountReportsDownloadNotificationEmailTemplate.Value.EmailSubject;
				var bodyTemplate = EDIDataRegistry.Instance.MyAccountReportsDownloadNotificationEmailTemplate.Value.EmailBody;

				var downloadLink = ZString.Format(EDIDataRegistry.Instance.MyAccountReportsDownloadURL.Value, "report=" + PK.ToString());
				emailDef.Body = bodyTemplate.ReplaceIgnoringCase("{DOWNLOAD_LINK}", downloadLink).ReplaceIgnoringCase("{REPORT_NAME}", WebUtility.HtmlEncode(ERQ_ReportName));
				emailDef.AddRecipientForUserCommunication(emailAddress);
				Env.OutgoingMailManager.Create(Factory, emailDef);
			}
		}

		protected virtual void GenerateReportCore(StreamWriter streamWriter)
		{
		}

		public bool CanRetryOnError => ERQ_CreateTimeUtc.IsValid && ZDateTime.UtcNow - ERQ_CreateTimeUtc < TimeSpan.FromHours(1);

		#region Type Decider

		public static readonly TypeDecider TypeDecider = new EdiReportingQueueTypeDecider();

		class EdiReportingQueueTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(EdiReportingQueue);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				string reportType = row[EdiReportingQueueSchema.Constants.ERQ_ReportType].ToString();
				switch (reportType)
				{
					case StlCombinedUsageReportQueue.ReportType: return typeof(StlCombinedUsageReportQueue);
					default: return typeof(EdiReportingQueue);
				}
			}

			public override Type GetTypeForNew()
			{
				return typeof(EdiReportingQueue);
			}
		}

		#endregion
	}

	//Empty Doc class for Registry.MyAccountReportsDownloadNotificationEmailTemplate
	public class DocEdiReportingQueue : DocBaseWrapper
	{
		public DocEdiReportingQueue(object objectToWrap, BusinessObjectFactory factory) : base(objectToWrap, factory)
		{
		}
	}
}

