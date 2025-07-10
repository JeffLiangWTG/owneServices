using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

[assembly: MailSubscriber(typeof(Enterprise.Customs.GB.MCP.Misc.MiscTextAndIslEmailsToEdiMessagesPoller))]
namespace Enterprise.Customs.GB.MCP.Misc
{
	/// <summary>
	///  Emails-to-Interchanges. A NewBaseInterchangeRetriever. Calls MiscTextAndIslBaseMessageProcessor when asked to GetNewMessageProcessor()
	/// </summary>
	public class MiscTextAndIslEmailsToEdiMessagesPoller : NewBaseInterchangeRetriever
	{
		public MiscTextAndIslEmailsToEdiMessagesPoller(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		protected override ZString GetInterchangeText(MailItem emailItem, bool decryptInNewThread)
		{
			emailSubjectForUcn = emailItem.MI_Subject;
			StringBuilder concatenatedMiscTextAndIsls = new StringBuilder();
			foreach (MailAttachment attachment in emailItem.MailAttachments)
			{
				concatenatedMiscTextAndIsls.AppendLine(attachment.MA_Data.ToAscii());
			}

			return concatenatedMiscTextAndIsls.ToString();
		}

		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange interchange = EDIInterchange.New(factory);
			interchange.EI_BodyText = interchangeString;
			interchange.EI_From = "MCP Destin8";
			interchange.EI_To = "Broker Software";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = Constants.ServiceTasksCode.MiscTextAndIslServiceTaskCode;

			foreach (string fullBlockOfMiscTextAndIsl in Regex.Split(interchangeString, EndOfFileTerminator.ToString()))  // Prince
			{
				ZString miscTextAndIslEdiMessageText = fullBlockOfMiscTextAndIsl.Trim();
				if (!miscTextAndIslEdiMessageText.IsEmpty)
				{
					EDIMessage message = interchange.ContainedMessages.AddNew();
					message.EM_MessageText = miscTextAndIslEdiMessageText;
					message.EM_ApplicationCode = Constants.ServiceTasksCode.MiscTextAndIslServiceTaskCode;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_MessageType = Constants.ServiceTasksCode.MiscTextAndIslServiceTaskCode;
					message.EM_MessageSubType = GetSubTypeFromFirstFewChars(miscTextAndIslEdiMessageText);
					message.EM_ApplicationReference = GetUcnFromEmailSubject();
					ServiceLogger.Log(LogType.Information, delegate
					{ return "Made EDIMessage from dbo.EDIInterchange for MiscTextAndIsl: " + miscTextAndIslEdiMessageText; });
				}
			}
			if (string.IsNullOrEmpty(interchangeString.Trim()) || interchangeString.IndexOf(EndOfFileTerminator) == 0)
			{
				ServiceLogger.Log(LogType.Warning, delegate
				{ return "Received empty MiscTextAndIsl or unterminated message. Entire interchange: " + interchangeString; });
			}

			return interchange;
		}

		ZString GetUcnFromEmailSubject()
		{
			ZString result = null;
			if (!emailSubjectForUcn.IsEmpty)
			{
				Regex regex = new Regex(@"(UCN)? = ([0-9]*)");
				if (regex.IsMatch(emailSubjectForUcn))
				{
					result = regex.Matches(emailSubjectForUcn)[0].Groups[2].Captures[0].Value;
				}
			}
			return result;
		}

		string GetSubTypeFromFirstFewChars(ZString input)
		{
			return input.StartsWith("=") ? input.Substring(1, 3) : input.Left(3);
		}

		protected override IMailFilter GetMailFilter()
			=> CreateMailFilter();

		[MailFilter(MailFilterCodes.GbMcpMiscTextAndEmails)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.GbMcpMiscTextAndEmails,
				fromComparison: SQLComparisonOperator.Contains, from: new[] { "@destin8.co.uk" },
				subjectComparison: SQLComparisonOperator.Contains, subjects: GetAllMailBoxes());

		static string[] GetAllMailBoxes()
		{
			return new string[] { "CSN01", // Self nomination
									"CAU01", // Nominated Agent Renomination 
									"LUM01"   // local unsolicited message e.g. weather report
								};
		}

		protected override List<ZString> GetAttachmentExtensionsToLookFor()
		{
			List<ZString> atts = base.GetAttachmentExtensionsToLookFor();
			atts.Add(".TXT");
			atts.Add(".ISL");
			return atts;
		}

		ZString emailSubjectForUcn;
		ILogger ServiceLogger { get; set; }
		const char EndOfFileTerminator = '\f';  // this is special char \f =   (the character formerly known as Prince)
	}
}
