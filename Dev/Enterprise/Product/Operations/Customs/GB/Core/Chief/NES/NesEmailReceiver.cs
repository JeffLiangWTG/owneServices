using System;
using System.Linq;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.MailFilters;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

[assembly: MailSubscriber(typeof(Enterprise.Customs.GB.Chief.NES.NesEmailReceiver))]
namespace Enterprise.Customs.GB.Chief.NES
{
	public class NesEmailReceiver : NewBaseInterchangeRetriever
	{
		public enum ResponseTypes
		{
			RES, // response data
			ACK, // acknowledgement
			WtgAlert // Internal alert from WTG
		}

		public enum WtgAlertTypes
		{
			None,
			SET,
			UNSET
		}

		public NesEmailReceiver(ILogger serviceLogger)
		{
			ServiceLogger = serviceLogger;
		}

		protected override ZString GetInterchangeText(MailItem emailItem, bool processInNewThread)
		{
			acknowledgementOrData = GetSubTypeAckOrResponseOrReport(emailItem);
			var emailBody = emailItem.BodyTextDecoded;  // Don't use MI_Body because quoted-printable will screw you up!
			if (acknowledgementOrData == ResponseTypes.RES)
			{
				var attachmentData = TryReallyHardToGetDataFromEmail(emailItem);            // Edifact is in one of two attachements (the other is human-readable text saying 'open the other attachment, yo')
				var bodyIsEdifact = emailBody.StartsWith(ChiefConstants.UNB) || emailBody.StartsWith(ChiefConstants.UNH);
				var attachmentIsEdifact = attachmentData.StartsWith(ChiefConstants.UNB) || attachmentData.StartsWith(ChiefConstants.UNH);

				if (bodyIsEdifact && !attachmentIsEdifact)
				{
					return emailBody;
				}
				else if (!bodyIsEdifact && attachmentIsEdifact)
				{
					return attachmentData;
				}
				else if (emailBody.Contains("This is a HMCE NES public service broadcast message"))
				{
					SendBroadcastNoteToCustomsNotificationGroup(emailBody, emailItem);
					shouldIgnoreEmptyInterchangeText = true;
					emailItem.MI_Status = MailStatus.Processed;
					return "";
				}
				else
				{
					return "";
				}
			}
			else if (acknowledgementOrData == ResponseTypes.WtgAlert)
			{
				SaveWtgAlertToRegistry(emailItem, emailBody);
				return "";
			}
			else // acknowledgement
			{
				return emailBody;
			}
		}

		void SaveWtgAlertToRegistry(MailItem emailItem, string body)
		{
			//Example subject line: "WTGEDCSALERT SET 1971-09-18 01:02" or "WTGEDCSALERT UNSET"
			if (emailItem.MI_From.Contains("@wisetechglobal.com", StringComparison.OrdinalIgnoreCase))
			{
				var setKeyword = " " + nameof(WtgAlertTypes.SET);
				var unsetKeyword = " " + nameof(WtgAlertTypes.UNSET);
				var alertType = emailItem.MI_Subject.Contains(unsetKeyword, StringComparison.Ordinal) ? WtgAlertTypes.UNSET : emailItem.MI_Subject.Contains(setKeyword, StringComparison.Ordinal) ? WtgAlertTypes.SET : WtgAlertTypes.None;
				if (alertType == WtgAlertTypes.UNSET)
				{
					ServiceLogger.Log(LogType.Information, "WTG EDCS alert unset email from " + emailItem.MI_From + " was processed - " + emailItem.MI_Subject);
					GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue);
					GBCustomsDataRegistry.Instance.EdcsWtgAlertString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				}
				else if (alertType == WtgAlertTypes.SET)
				{
					var setTimeString = emailItem.MI_Subject.SubstringSafe(emailItem.MI_Subject.IndexOf(setKeyword, StringComparison.Ordinal) + setKeyword.Length).Trim();
					if (!ZDateTime.TryParseExact(setTimeString, out ZDateTime dateTime, "yyyy-MM-dd HH:mm"))
					{
						ServiceLogger.Log(LogType.Warning, "WTG EDCS alert set email has invalid date; using current time instead");
						dateTime = ZDateTime.Now;
					}
					ServiceLogger.Log(LogType.Information, "WTG EDCS alert set email from " + emailItem.MI_From + " was processed - " + emailItem.MI_Subject);
					GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateTime.ToDateTime());
					GBCustomsDataRegistry.Instance.EdcsWtgAlertString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, body);
				}
				shouldIgnoreEmptyInterchangeText = true;
			}
			else
			{
				ServiceLogger.Log(LogType.Warning, "WTG EDCS alert email ignored, not from WTG - " + emailItem.MI_From);
			}
			emailItem.MI_Status = MailStatus.Processed;
		}

		protected override bool ShouldIgnoreEmptyInterchangeText(MailItem mailItem)
		{
			return shouldIgnoreEmptyInterchangeText;
		}

		ZString TryReallyHardToGetDataFromEmail(MailItem emailItem)
		{
			MailAttachment[] dataAttachments = (MailAttachment[])emailItem.MailAttachments.Find(new ZQuery(MailDBAttachmentsSchema.MA_FileName, SQLComparisonOperator.Contains, new string[] { ".bin", ".dat" }));
			ZString reportContentToReturn = null;
			if (dataAttachments.Length == 1)
			{
				reportContentToReturn = dataAttachments[0].MA_Data.ToAscii();
			}
			else if (dataAttachments.Length == 0)
			{
				// No attachment, could be embedded as base64
				var message = emailItem.BuildMimeMessage();
				var atts = message.GetFullAttachments().Union(message.BodyParts.Where(x => x?.ContentType?.MimeType == "application/octet-stream"));
				if (!atts.Any())
				{
					reportContentToReturn = string.Empty;
				}
				else
				{
					reportContentToReturn = Encoding.ASCII.GetString(atts.FirstOrDefault().GetData());
				}
			}
			else
			{
				throw new NotSupportedException("Only one RPA per email is supported");
			}
			return reportContentToReturn;
		}

		ResponseTypes GetSubTypeAckOrResponseOrReport(MailItem emailItem)
		{
			if (emailItem.MI_Subject.Contains(NesConstants.SubjectForAcks))
			{
				return ResponseTypes.ACK;
			}
			else if (emailItem.MI_Subject.Contains(NesConstants.WtgAlert))
			{
				return ResponseTypes.WtgAlert;
			}
			else
			{
				return ResponseTypes.RES;
			}
		}

		protected override EDIInterchange CreateInterchangeAndMessages(BusinessObjectFactory factory, string interchangeString, MailItem mailItem)
		{
			EDIInterchange receivedInterchange = null;
			EDIMessage receivedMessage = null;

			if (acknowledgementOrData == ResponseTypes.RES)
			{
				string cleanInterchangeString = interchangeString.Replace(System.Environment.NewLine, string.Empty).Trim();  // Email has new lines in :S
				receivedInterchange = EDIInterchange.CreateNewInterchangeFromString(factory, cleanInterchangeString, NesConstants.ApplicationCode);
				receivedMessage = receivedInterchange.ContainedMessages[0];
				// God bless our constraints.... without this we could only ever receive one DTI print, the second would fail with a unique index violation:
				receivedInterchange.EI_InterchangeNum = receivedMessage.EM_MessageNum.MakeUniqueInterchangeNumber();
				GetBranchOnMessageAndPimaFromInterchange(receivedMessage, receivedInterchange);
			}
			else if (acknowledgementOrData == ResponseTypes.ACK)
			{
				// Body of email is text for humans
				receivedInterchange = EDIInterchange.New(factory);
				receivedInterchange.EI_BodyText = interchangeString;
				receivedInterchange.EI_From = "EDCS";
				receivedInterchange.EI_To = "Broker Software";
				receivedMessage = receivedInterchange.ContainedMessages.AddNew();
				receivedMessage.EM_MessageText = interchangeString;
				receivedMessage.EM_ApplicationCode = NesConstants.ApplicationCode;
				receivedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				receivedMessage.EM_MessageType = NesConstants.ApplicationCode;
				receivedMessage.EM_MessageNum = GbExtensionHelpers.MakeUniqueInterchangeNumber("", EDIMessage.Schema.EM_MessageNumMaxLength);
			}
			else
			{
				throw new NotImplementedException("Processor cannot understand this NES type. Only Ack, Res or Rpa. Type was: " + acknowledgementOrData);
			}

			receivedMessage.EM_ApplicationReference = NesConstants.ApplicationCode + acknowledgementOrData;  // e.g. NESACK or NESRES
			receivedMessage.EM_MessageSubType = acknowledgementOrData.ToString();
			receivedMessage.EM_Status = EDIInterchange.Status.Queued;
			receivedInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			receivedInterchange.EI_Status = EDIInterchange.Status.Queued;
			receivedInterchange.EI_ApplicationCode = NesConstants.ApplicationCode;

			if (ServiceLogger != null)
			{
				ServiceLogger.Log(LogType.Information, delegate
				{ return "Queued inbound " + acknowledgementOrData + " EDIMessage from EDCS: " + ((ZString)interchangeString).SubstringSafe(0, 250); });
			}

			return receivedInterchange;
		}

		void GetBranchOnMessageAndPimaFromInterchange(EDIMessage receivedMessage, EDIInterchange receivedInterchange)
		{
			var gbPk = RegistryPimaAndBadgeHelper.GetPrimaryBranchPkFromRegistryBasedOnPima(receivedInterchange.EI_To, receivedInterchange.Factory, useCompanyFieldNotPimaField: true);
			if (!gbPk.IsEmpty)
			{
				receivedInterchange.EI_GB = gbPk;
				receivedMessage.EM_GB = gbPk;
			}
		}

		protected override IMailFilter GetMailFilter() => BuildMailFilter();

		[MailFilter(MailFilterCodes.GbChiefNesEmail)]
		public static IMailFilter BuildMailFilter()
		{
			// this tells the email engine what we want to see
			var businessEmailFilter = new ZQuery(MailDBItemsSchema.MI_Status, MailStatus.Queued);
			businessEmailFilter.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive);
			businessEmailFilter.AddToFilter(MailDBItemsSchema.MI_Application, MailApplication.Standard);
			var alertEmails = businessEmailFilter.DeepClone();
			businessEmailFilter.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, GBCustomsDataRegistry.Instance.NesEmailAddress);  // 'contains' is vital - since "Billy Bob <edcs@hmce.gov.uk>" or "edcs@hmce.gov.uk <edcs@hmce.gov.uk>" or "edcs@hmce.gov.uk <mailto:edcs@hmce.gov.uk>" will not match an EQUALS operation
			alertEmails.AddToFilter(MailDBItemsSchema.MI_From, SQLComparisonOperator.Contains, "@wisetechglobal.com");
			alertEmails.AddToFilter(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, NesConstants.WtgAlert);
			var both = new ZQuery();
			both.AddToFilter(businessEmailFilter);
			both.AddToFilter(alertEmails, JoinCondition.Or);
			return new QueryMailFilter(MailFilterCodes.GbChiefNesEmail, both);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		void SendBroadcastNoteToCustomsNotificationGroup(string emailBody, MailItem emailItem)
		{
			var emailSender = new HtmlNotificationEmailSender();
			var body = "<p>" + BrandingFactory.Instance.ProductName + " received a notification from EDCS.  Please distribute the note below to the relevant staff.</p><pre>\r\n\r\n" + emailBody + "</pre>";
			var email = emailSender.CreateEmail("NES broadcast message - " + emailItem.MI_Subject, body);

			new Customs.Business.EmailSender(new LoggingInformation()).SendNotification(
				email,
				null,   // user who's doing the job
				GBCustomsDataRegistry.Instance.CustomsResponseNotifications,  // option about whether to send to user, group, group & user, none.
				GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup, GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroupItem,  // Group in question
				emailItem.Factory
				);
		}

		bool shouldIgnoreEmptyInterchangeText;
		ResponseTypes acknowledgementOrData;
		ILogger ServiceLogger { get; set; }
	}
}

