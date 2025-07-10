using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: MailSubscriber(typeof(Enterprise.Customs.GB.MCP.PHS11.PHS11EmailsToEdiMessagesPoller))]
namespace Enterprise.Customs.GB.MCP.PHS11
{
	public class PHS11EmailsToEdiMessagesPoller : NewBaseInterchangeRetriever
	{
		public PHS11EmailsToEdiMessagesPoller(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		protected override ZString GetInterchangeText(MailItem emailItem, bool decryptInNewThread)
		{
			var messages = new List<string>();
			foreach (MailAttachment attachment in emailItem.MailAttachments)
			{
				messages.Add(attachment.MA_Data.ToAscii());
			}
			return string.Join(delimiterBetweenMessages, messages.ToArray());
		}

		readonly string delimiterBetweenMessages = "{54AC16BF-A379-4847-B572-509AE22C728E}";

		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange interchange = EDIInterchange.New(factory);
			interchange.EI_BodyText = interchangeString;
			interchange.EI_From = "MCP Destin8 PHS11";
			interchange.EI_To = "Broker Software";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbMcpPortHealth;

			string[] pHS11EdiMessagesText = Regex.Split(interchangeString.Trim(), delimiterBetweenMessages);
			foreach (ZString pHS11EdiMessageText in pHS11EdiMessagesText)
			{
				if (!pHS11EdiMessageText.IsEmpty && pHS11EdiMessageText.StartsWith("=PHS11~"))
				{
					EDIMessage message = interchange.ContainedMessages.AddNew();
					message.EM_MessageText = pHS11EdiMessageText;
					message.EM_ApplicationCode = ApplicationCodeList.Codes.GbMcpPortHealth;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_MessageType = ApplicationCodeList.Codes.GbMcpPortHealth;
					message.EM_MessageNum = mailItem.MI_ReceivedDateTime.Ticks.ToString();
					message.EM_MessageSubType = "ISL";
					ServiceLogger.Log(LogType.Information, delegate
					{ return "Made EDIMessage from dbo.EDIInterchange for PHS11. Text=" + pHS11EdiMessageText; });
				}
				else
				{
					ServiceLogger.Log(LogType.Warning, delegate
					{ return "Received empty or invalid PHS11. Starts:  " + pHS11EdiMessageText.SubstringSafe(0, 100); });
				}
			}
			return interchange;
		}

		// NB the subjects below are not debug-only because debug-only does not allow testing on CMR
		protected override IMailFilter GetMailFilter() => CreateMailFilter();

		[MailFilter(MailFilterCodes.GbMcpPHS)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.GbMcpPHS,
				fromComparison: SQLComparisonOperator.Contains, from: new[] { "@destin8.co.uk" },
				subjectComparison: SQLComparisonOperator.Contains, subjects: new[] { "PHS11", "PORT HEALTH STATUS", "PORT HEALTH STATUS FOR CARGOWISE TESTING {F1445654-A3EA-427F-996C-344D8B70B08B}" });

		protected override List<ZString> GetAttachmentExtensionsToLookFor()
		{
			List<ZString> extensions = base.GetAttachmentExtensionsToLookFor();
			extensions.Add(".ISL");
			extensions.Add(".TXT");
			return extensions;
		}

		ILogger ServiceLogger { get; set; }
	}
}
