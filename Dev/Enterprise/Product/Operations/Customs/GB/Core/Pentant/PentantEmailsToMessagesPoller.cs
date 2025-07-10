using System.Globalization;
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

[assembly: MailSubscriber(typeof(Enterprise.Customs.GB.Pentant.PentantEmailsToMessagesPoller))]
namespace Enterprise.Customs.GB.Pentant
{
	public class PentantEmailsToMessagesPoller : NewBaseInterchangeRetriever
	{
		public PentantEmailsToMessagesPoller(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		protected override ZString GetInterchangeText(MailItem emailItem, bool decryptInNewThread)
		{
			return emailItem.MI_Body.Trim();
		}

		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange interchange = EDIInterchange.New(factory);
			interchange.EI_BodyText = interchangeString;
			interchange.EI_From = "Pentant";
			interchange.EI_To = "Broker Software";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.Pentant;

			if (!string.IsNullOrEmpty(interchangeString))
			{
				var message = interchange.ContainedMessages.AddNew();
				message.EM_MessageText = interchangeString;
				message.EM_ApplicationCode = ApplicationCodeList.Codes.Pentant;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = ApplicationCodeList.Codes.Pentant;
				message.EM_MessageNum = mailItem.MI_ReceivedDateTime.Ticks.ToString(CultureInfo.InvariantCulture);
				ServiceLogger.Log(LogType.Information, delegate
				{ return "Made EDIMessage from dbo.EDIInterchange for Pentant report. Text=" + interchangeString; });
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, delegate
				{ return "Received empty or invalid Pentant report. Starts:  " + interchangeString; });
			}

			return interchange;
		}

		[MailFilter(MailFilterCodes.GbPentantEmails)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.GbPentantEmails, fromComparison: SQLComparisonOperator.Contains, from: "@pentant.co.uk");

		protected override IMailFilter GetMailFilter()
			=> CreateMailFilter();

		ILogger ServiceLogger { get; set; }
	}
}

