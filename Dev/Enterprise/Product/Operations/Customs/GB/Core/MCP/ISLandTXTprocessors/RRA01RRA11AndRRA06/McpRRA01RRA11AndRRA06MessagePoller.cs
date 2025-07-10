using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

[assembly: MailSubscriber(typeof(Enterprise.Customs.GB.MCP.RRA01AndRRA11.McpRRA01RRA11AndRRA06MessagePoller))]
namespace Enterprise.Customs.GB.MCP.RRA01AndRRA11
{
	/// <summary>
	/// this will get all the emails and process them into interchanges
	/// </summary>
	public class McpRRA01RRA11AndRRA06MessagePoller : NewBaseInterchangeRetriever
	{
		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange interchange = EDIInterchange.New(factory);
			interchange.EI_BodyText = interchangeString;
			interchange.EI_From = "GB customs";
			interchange.EI_To = "Broker Software";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbMcpRra01AndRra11;

			foreach (string lineOfInterchangeText in System.Text.RegularExpressions.Regex.Split(interchangeString, System.Environment.NewLine))
			{
				ZString rraEdiMessageText = lineOfInterchangeText.Trim();
				if (!rraEdiMessageText.IsEmpty)
				{
					var message = factory.New<GbEDIMessage>();
					message.EM_MessageText = lineOfInterchangeText.Trim();
					message.EM_ApplicationCode = ApplicationCodeList.Codes.GbMcpRra01AndRra11;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_EI = interchange.PK;
				}
			}
			return interchange;
		}

		protected override ZString GetInterchangeText(MailItem emailThatWasReceived, bool decryptInNewThread)
		{
			ZStringBuilder sb = new ZStringBuilder();
			if (!HasAtLeastOneRelevantAttachment(emailThatWasReceived))
			{
				CheckEmailForPotentialBogusVirusScannerAndThenReportEmptyEmailToPostmaster(emailThatWasReceived);
				Logger.LogError("RRA message had no relevant attachments. Subject: " + emailThatWasReceived.MI_Subject);
				return ZString.Empty;
			}
			foreach (MailAttachment soMailAttachment in emailThatWasReceived.MailAttachments)
			{
				ZString rraStringFromWithinAttachment = soMailAttachment.MA_Data.ToAscii();
				if (!string.IsNullOrEmpty(rraStringFromWithinAttachment))
				{
					sb.Append(rraStringFromWithinAttachment);
				}
			}
			return sb.ToStringWithDelimiterBetweenAppends(System.Environment.NewLine);
		}

		bool HasAtLeastOneRelevantAttachment(MailItem emailThatWasReceived)
		{
			return Array.Exists(
													emailThatWasReceived.MailAttachments.ToArray<MailAttachment>(),
													mailAtt => mailAtt.MA_FileName.ToLower().EndsWith(".isl", StringComparison.OrdinalIgnoreCase)
												);
		}

		/// <summary>
		/// We do not expect to receive an email from MCP without an attachment. Look at the headers of the received mail and if we see the word 'virus', give the postmaster a spanking.
		/// </summary>
		/// <param name="emailThatWasReceived"></param>
		void CheckEmailForPotentialBogusVirusScannerAndThenReportEmptyEmailToPostmaster(MailItem emailThatWasReceived)
		{
			string potentialVirusScannerWarning = "";
			if (emailThatWasReceived.MI_Header.ToLower().Contains("virus", StringComparison.OrdinalIgnoreCase) || emailThatWasReceived.MI_Header.ToLower().Contains("scanned", StringComparison.OrdinalIgnoreCase))
			{
				potentialVirusScannerWarning = "Does your virus software strip attachments if it thinks they are viruses? There was indeed a mention of the word 'virus' or 'scanned' in the mail's header.";
			}

			string body = "This is the CargoWise RRA11 processor, which takes status updates from MCP/Destin8 and turns them into events. An email was received from MCP without a relevant attachment. It may be that MCP are sending you emails without ISL attachments.  Perhaps your profile is set to receive TXT RRAs, not ISL RRAs.  Please lisaise with MCP regarding changing this setting. Or it may be that your email server stripped the attachment. " + potentialVirusScannerWarning;

			EmailDef warningToPostmaster = new EmailDef();
			warningToPostmaster.Body = body;
			byte[] data = Array.Empty<byte>();
			string str = emailThatWasReceived.MI_Header + System.Environment.NewLine + System.Environment.NewLine + emailThatWasReceived.MI_Body;
			System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
			data = encoding.GetBytes(str);
			warningToPostmaster.Attachments.Add(new AttachmentDef("Received mail item.txt", data));
			Enterprise.Environment.Env.OutgoingCustomsMailManager.CreateAndSaveToPostmasterGroup(warningToPostmaster, emailThatWasReceived.Factory);
		}

		protected override IMailFilter GetMailFilter() => CreateMailFilter();

		[MailFilter(MailFilterCodes.GbMcpRra11AndRra06AndRra01)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.GbMcpRra11AndRra06AndRra01,
				fromComparison: SQLComparisonOperator.Contains, from: new[] { "@destin8.co.uk" },
				subjectComparison: SQLComparisonOperator.Contains, subjects: new[] { "RRA01", "RRA11", "RRA06", "RRA FOR CARGOWISE TESTING {F1445654-A3EA-427F-996C-344D8B70B08B}" });

		protected override List<ZString> GetAttachmentExtensionsToLookFor()
		{
			// this tells the engine what attachements we want to see
			List<ZString> soListOfExtensionsToSeek = new List<ZString>();
			soListOfExtensionsToSeek.Add(".ISL");
			return soListOfExtensionsToSeek;
		}
	}
}
