using System;
using System.IO;
using System.Text;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.OrgConstants;

namespace Enterprise.PrintProcessing.Mailer.Email
{
	public static class EmailBody
	{
		public static string Body(StmPrintJob printJob, AttachmentDefCollection attachments, bool isHTML = false, bool hasInvalidPDf = false, string extraHtmlBody = "")
		{
			var body = new StringBuilder();
			var footer = new StringBuilder();

			if (printJob != null)
			{
				if (printJob.EmailFaxCoverNote != "" && DocumentsDataRegistry.Instance.AddEmailFaxCoverNote.Value && printJob.SP_EmailAttachmentFormat != AttachmentType.HTMF)
				{
					body.Append(printJob.EmailFaxCoverNote);
				}
				else if (attachments.Count > 0)
				{
					bool hasNonHtmlEmbeddedAttachment = false;
					foreach (AttachmentDef attachment in attachments)
					{
						if (!Path.GetExtension(attachment.DisplayName).Equals(".HTML", StringComparison.OrdinalIgnoreCase)
							&& !Path.GetExtension(attachment.DisplayName).Equals(".PNG", StringComparison.OrdinalIgnoreCase))
						{
							hasNonHtmlEmbeddedAttachment = true;
						}
					}

					if (hasNonHtmlEmbeddedAttachment)
					{
						body.AppendLine()
							.AppendLine(Res.GetString("13a03cad-795d-44a4-bd35-87e760ca547b", "Please see the attached documents."));
					}

					if (hasInvalidPDf)
					{
						body.AppendLine()
							.AppendLine(Res.GetString("f65afd1f-0962-446e-8742-b95a93e2fbd3", "Some files could not be merged and have been attached separately."))
							.AppendLine();
					}
				}

				if (isHTML && !string.IsNullOrEmpty(extraHtmlBody))
				{
					body.AppendLine()
						.AppendLine(extraHtmlBody);
				}

				if (DocumentsDataRegistry.Instance.AddEmailSignature.Value)
				{
					footer.AppendLine()
							.AppendLine()
							.Append(printJob.SP_EmailSignature);
				}
			}
			var result = body.ToString() + footer.ToString();
			if (isHTML)
			{
				result = result.Replace("\r\n", "<br />");
				result = (NoResString)"<body style='font-family:Calibri,sans-serif; font-size:16px'>" + result + (NoResString)"</body>";
			}
			return result;
		}
	}
}
