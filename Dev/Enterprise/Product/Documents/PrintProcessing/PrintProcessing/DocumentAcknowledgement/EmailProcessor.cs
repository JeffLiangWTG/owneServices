using System;
using CargoWise.Common;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MessageProcessor;
using Enterprise.ZArchitecture.Schema;

[assembly: MessageFilter("MAP", MailDBItemsSchema.Constants.TableName, typeof(Enterprise.PrintProcessing.DocumentAcknowledgement.EmailProcessor))]

namespace Enterprise.PrintProcessing.DocumentAcknowledgement
{
	public class EmailProcessor
	{
		[MessageFilterCondition(MailDBItemsSchema.Constants.MI_Subject, "^" + SubjectIdentifier)]

		public bool ProcessAcknowledgementMail(MailItem acknowledgementMail)
		{
			try
			{
				var subjectLineComponents = acknowledgementMail.MI_Subject.ToString().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				if (subjectLineComponents.Length < 3 || subjectLineComponents[0] != SubjectIdentifier)
				{
					ReportBadSubjectLine(acknowledgementMail);
				}
				else
				{
					var documentSentLogPK = new Guid(subjectLineComponents[1]);
					var status = (DocumentAcknowledgementStatus)Enum.Parse(typeof(DocumentAcknowledgementStatus), subjectLineComponents[2], true);
					AcknowledgementProcessor.Process(documentSentLogPK, status);

					return true;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ReportBadSubjectLine(acknowledgementMail);
			}

			return false;
		}

		void ReportBadSubjectLine(MailItem acknowledgementMail)
		{
			ErrorReporter.ReportOnce("DocumentAcknolwedgement.EmailProcess.Process bad subject line", "Bad acknowledgement subject line: " + acknowledgementMail.MI_Subject);
		}

		public int NumberOfEmailsProcessedInLastBatch { get; private set; }

		protected virtual IDocumentAcknowledgementProcessor GetAcknowledgementProcessor()
		{
			return new DocumentAcknowledgementProcessor();
		}

		IDocumentAcknowledgementProcessor AcknowledgementProcessor
		{
			get
			{
				if (acknowledgementProcessor == null)
				{
					acknowledgementProcessor = GetAcknowledgementProcessor();
				}

				return acknowledgementProcessor;
			}
		}

		IDocumentAcknowledgementProcessor acknowledgementProcessor;

		public const string SubjectIdentifier = "{EDIFAX}"; // defined by EDI Fax Gateway
	}
}
