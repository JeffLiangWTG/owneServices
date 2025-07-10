using System.Collections.Generic;
using System.Text;
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

[assembly: MailSubscriber(typeof(Enterprise.Customs.GB.MCP.RRA12.RRA12EmailsToEdiMessagesPoller))]
namespace Enterprise.Customs.GB.MCP.RRA12
{
	/// <summary>
	///  Emails-to-Interchanges. A NewBaseInterchangeRetriever. Calls RRA12BaseMessageProcessor when asked to GetNewMessageProcessor()
	/// </summary>
	public class RRA12EmailsToEdiMessagesPoller : NewBaseInterchangeRetriever
	{
		public RRA12EmailsToEdiMessagesPoller(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		protected override ZString GetInterchangeText(MailItem emailItem, bool decryptInNewThread)
		{
			StringBuilder concatenatedRra12s = new StringBuilder();
			foreach (MailAttachment attachment in emailItem.MailAttachments)
			{
				concatenatedRra12s.AppendLine(attachment.MA_Data.ToAscii());
			}

			return concatenatedRra12s.ToString();
		}

		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange interchange = EDIInterchange.New(factory);
			interchange.EI_BodyText = interchangeString;
			interchange.EI_From = "MCP Destin8 RRA12";
			interchange.EI_To = "Broker Software";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbMcpRra12;

			ZString rra12EdiMessageText = interchangeString.Trim();
			if (!rra12EdiMessageText.IsEmpty)
			{
				EDIMessage message = interchange.ContainedMessages.AddNew();
				message.EM_MessageText = rra12EdiMessageText;
				message.EM_ApplicationCode = ApplicationCodeList.Codes.GbMcpRra12;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationCodeList.Codes.GbMcpRra12;
				message.EM_MessageSubType = rra12EdiMessageText.StartsWith("=RRA12~") ? "ISL" : "UNK";
				ServiceLogger.Log(LogType.Information, delegate
				{ return "Made EDIMessage from dbo.EDIInterchange for RRA12. Text=" + rra12EdiMessageText; });
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, delegate
				{ return "Received empty RRA12"; });
			}

			return interchange;
		}

		protected override IMailFilter GetMailFilter() => CreateMailFilter();

		[MailFilter(MailFilterCodes.GbMcpRra12)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.GbMcpRra12,
				fromComparison: SQLComparisonOperator.Contains, from: new[] { "@destin8.co.uk" },
				subjectComparison: SQLComparisonOperator.Contains, subjects: new[] { "RRA12", "Amalgamation Clearance Advice", "RRA12 FOR CARGOWISE TESTING {F1445654-A3EA-427F-996C-344D8B70B08B}" });

		protected override List<ZString> GetAttachmentExtensionsToLookFor()
		{
			List<ZString> atts = base.GetAttachmentExtensionsToLookFor();
			atts.Add(".TXT");
			atts.Add(".ISL");
			return atts;
		}

		ILogger ServiceLogger { get; set; }
	}
}
