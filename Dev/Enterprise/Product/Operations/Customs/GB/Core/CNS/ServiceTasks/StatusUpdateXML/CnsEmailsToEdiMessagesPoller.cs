[assembly: Enterprise.MailManager.MailSubscriber(typeof(Enterprise.Customs.GB.CNS.CnsEmailsToEdiMessagesPoller))]
namespace Enterprise.Customs.GB.CNS
{
	using System.Collections.Generic;
	using System.IO;
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

	/// <summary>
	///  Emails-to-Interchanges. A NewBaseInterchangeRetriever. Calls CnsXmlBaseMessageProcessor when asked to GetNewMessageProcessor()
	/// </summary>
	public class CnsEmailsToEdiMessagesPoller : NewBaseInterchangeRetriever
	{
		public CnsEmailsToEdiMessagesPoller(ILogger serviceLogger)
		{
			this.serviceLogger = serviceLogger;
		}

		const string delimiterToSeparateXmlFiles = "EB7B72C7-FF4A-47f8-90B2-A5956DF11D09"; // some random Guid we'll never see elsewhere

		protected override ZString GetInterchangeText(MailItem emailItem, bool decryptInNewThread)
		{
			if (emailItem.MailAttachments.Count == 0)
			{
				serviceLogger.Log(LogType.Warning, delegate
				{ return string.Format("No attachments present in CNS Compass email with subject {0}.  Cannot process email.", emailItem.MI_Subject); });
				return "";
			}
			this.emailItemReceived = emailItem;

			var concatenatedCnsXmls = new StringBuilder();
			fileNames = new List<ZString>();
			foreach (MailAttachment attachment in emailItem.MailAttachments)
			{
				concatenatedCnsXmls.Append(attachment.MA_Data.ToAscii());
				concatenatedCnsXmls.Append(delimiterToSeparateXmlFiles);
				fileNames.Add(attachment.MA_FileName);
			}

			return concatenatedCnsXmls.ToString();
		}

		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange interchange = EDIInterchange.New(factory);
			interchange.EI_BodyText = interchangeString;
			interchange.EI_HeaderText = emailItemReceived != null ? emailItemReceived.MI_Header : ZString.Empty;
			interchange.EI_From = "CNS Compass";
			interchange.EI_To = "Broker Software";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.GbCnsCompass;
			int i = 0;

			foreach (ZString oneTxtOrXmlFile in System.Text.RegularExpressions.Regex.Split(interchangeString, delimiterToSeparateXmlFiles))
			{
				if (!oneTxtOrXmlFile.IsEmpty)
				{
					EDIMessage message = interchange.ContainedMessages.AddNew();
					message.EM_MessageText = oneTxtOrXmlFile;
					message.EM_ApplicationCode = ApplicationCodeList.Codes.GbCnsCompass;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					message.EM_MessageType = ApplicationCodeList.Codes.GbCnsCompass;
					message.EM_MessageSubType = oneTxtOrXmlFile.TrimStart().StartsWith("<?xml") ? CnsXmlApplicationTypeMessageProcessor.XmlSubCode : CnsTextApplicationTypeMessageProcessor.TxtSubCode;
					if (fileNames.Count > i)
					{
						message.EM_MessageNum = new ZString(Path.GetFileNameWithoutExtension(fileNames[i])).Right(EDIMessage.Schema.EM_MessageNumMaxLength);
					}
					serviceLogger.Log(LogType.Information, delegate
					{ return string.Format("Made EDIMessage #{0} from dbo.EDIInterchange for CNS message {1}", message.EM_MessageNum, oneTxtOrXmlFile); });
				}
				i++;
			}

			return interchange;
		}

		protected override IMailFilter GetMailFilter() => CreateMailFilter();

		[MailFilter(MailFilterCodes.GbCNS)]
		public static IMailFilter CreateMailFilter()
			=> new QueryMailFilter(MailFilterCodes.GbCNS, fromComparison: SQLComparisonOperator.Contains, from: "@cnsonline.net");

		protected override List<ZString> GetAttachmentExtensionsToLookFor()
		{
			List<ZString> atts = base.GetAttachmentExtensionsToLookFor();
			atts.Add("*");
			return atts;
		}

		ILogger serviceLogger { get; set; }
		MailItem emailItemReceived;
		List<ZString> fileNames;

#if DEBUG
		// For soak test we want to get all sample emails
		protected override int NumberToRetrieveAtATime
		{
			get { return 200; }
		}
#endif
	}
}
