using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.PrintProcessing.DocumentAcknowledgement
{
	public interface IDocumentAcknowledgementProcessor
	{
		void Process(ZGuid referencePK, DocumentAcknowledgementStatus status);
		void Process(ZGuid referencePK, DocumentAcknowledgementStatus status, string message);
	}

	public enum DocumentAcknowledgementStatus
	{
		Unknown,
		Success,
		Failure
	}

	public class DocumentAcknowledgementProcessor : IDocumentAcknowledgementProcessor
	{
		public void Process(ZGuid referencePK, DocumentAcknowledgementStatus status)
		{
			Process(referencePK, status, Res.GetString("7c232edd-9dd1-4961-9fe7-266c9541eb66", "The fax number may be busy; please try re-sending the document again. If you have received this message multiple times, please check that the fax number is correct."));
		}

		public void Process(ZGuid referencePK, DocumentAcknowledgementStatus status, string message)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			StmPrintJob printJob = factory.Load<StmPrintJob>(referencePK);
			if (printJob != null)
			{
				using (DisposableEnvironment.ForBranch(printJob.ProperBranchPK.ToGuid()))
				{
					switch (status)
					{
						case DocumentAcknowledgementStatus.Success:
							printJob.CreateLogOnParent(Events.DocumentDelivered);
							if (printJob.SP_EDocsProcessed)
							{
								printJob.Delete();
							}
							else
							{
								printJob.SP_JobType = nameof(PrintType.DDS);
							}
							break;

						case DocumentAcknowledgementStatus.Failure:
							printJob.CreateLogOnParent(Events.DocumentNotDelivered, new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Reason, Constants.DocumentNotDeliveredReasons.Codes.Failed));
							SendFailureEmail(printJob, message);
							printJob.Delete();
							break;

						default:
							ErrorReporter.ReportOnce("Unknown DocumentAcknowledgementStatus", "Unknown DocumentAcknowledgementStatus: " + status.ToString());
							break;
					}
				}
			}
			factory.Save();
		}

		protected void SendFailureEmail(StmPrintJob printJob, string message)
		{
			if (!printJob.Staff.GS_EmailAddress.IsEmpty)
			{
				try
				{
					EmailDef failureEmail = new EmailDef();
					failureEmail.AddRecipientForUserCommunication(printJob.Staff.GS_EmailAddress);
					failureEmail.Subject = Res.GetString("13f62c78-79d5-4594-8233-20f7a39e1573", "{0} Document Delivery Failure", Core.Constants.ProductName);
					failureEmail.Body = Res.GetString("3017688d-1557-4161-b8ee-4e5bfc691c64", "The following fax could not be delivered:\r\n{0}\r\nFax number: {1}\r\nSent:", printJob.SP_AbbreviatedEmailSubjectLine, printJob.SP_FaxDestination) + " " + Env.Time.FormatDateTime(printJob.SP_RunDateTime.ToDateTime()) + System.Environment.NewLine
						+ System.Environment.NewLine
						+ message + System.Environment.NewLine
						;
					Env.OutgoingMailManager.Create(printJob.Factory, failureEmail);
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{
						throw;
					}

					ErrorReporter.ReportOnce("Error sending document failure email", exception);
				}
			}
		}
	}
}
