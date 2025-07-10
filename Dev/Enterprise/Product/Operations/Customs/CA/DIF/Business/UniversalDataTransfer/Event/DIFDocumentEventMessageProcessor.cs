using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer
{
	public class DIFDocumentEventMessageProcessor
	{
		public DIFDocumentEventMessageProcessor(IXmlSessionTracker logger, UniversalEvent eventDataObject, EDIMessage message, JobRequiredDocumentAddInfo jobRequiredDocumentAddInfo)
		{
			this.jobRequiredDocumentAddInfo = Argument.NotNull(jobRequiredDocumentAddInfo, "jobRequiredDocumentAddInfo");
			this.eventDataObject = Argument.NotNull(eventDataObject, "eventDataObject");
			this.logger = Argument.NotNull(logger, "logger");
			this.message = Argument.NotNull(message, "message");

			var requiredDocument = jobRequiredDocumentAddInfo.RequiredDocument;
			requiredDocument.ParentType = ObjectFactory.GetType<Freight.Integration.Forwarding.IForwardingDocsAndCartage>();
			var disHostProvider = requiredDocument != null ? requiredDocument.Parent as IDISHostProvider : null;
			disHost = disHostProvider != null ? disHostProvider.DISHost : null;
		}
		protected JobRequiredDocumentAddInfo jobRequiredDocumentAddInfo;
		protected readonly UniversalEvent eventDataObject;
		protected readonly IXmlSessionTracker logger;
		protected readonly EDIMessage message;
		readonly IDISHost disHost;
		HtmlTableCreator htmlTable;

		public void Process()
		{
			if (jobRequiredDocumentAddInfo != null)
			{
				message.EM_LinkUniqueID = jobRequiredDocumentAddInfo.PK;
				message.EM_LinkTable = jobRequiredDocumentAddInfo.TablePrefix;
			}
			htmlTable = new HtmlTableCreator();
			bool failed = eventDataObject.EventType.GetValueOrDefault() == ZArchitecture.Business.AutoEvents.DocumentNotDeliveredCode;
			var eventParameters = eventDataObject.EventParameters;
			bool fromCBSA = (eventParameters?.Department ?? ZString.Empty) == DIFConstants.UniversalEventDepartment.CBSA;
			UpdateMessageStatus(failed, fromCBSA);
			FormatMessageText();
			SendReport(GetEmailAndSetOnMessage(failed, fromCBSA));
		}

		void FormatMessageText()
		{
			var text = message.EM_MessageText;
			if (Regex.Matches(text, @"\r?\n").Count < 1)
			{
				var document = new XmlDocument();
				using (var stream = new MemoryStream())
				{
					var reader = new StreamReader(stream);
					var writer = new XmlTextWriter(stream, Encoding.UTF8) { Formatting = Formatting.Indented, IndentChar = ' ', Indentation = 2 };

					document.LoadXml(text);
					document.WriteContentTo(writer);
					writer.Flush();
					stream.Flush();

					stream.Position = 0;

					message.EM_MessageText = reader.ReadToEnd();
				}
			}
		}

		public void UpdateMessageStatus(bool failed, bool fromCBSA)
		{
			var status = ZString.Empty;
			if (fromCBSA)
			{
				status = CalculateCBSAMessageStatus(failed, jobRequiredDocumentAddInfo.EX_Status);
			}
			else
			{
				status = CalculateMessageStatus(failed, jobRequiredDocumentAddInfo.EX_Status);
			}
			if (!status.IsEmpty)
			{
				jobRequiredDocumentAddInfo.EX_Status = status;
			}
			else
			{
				htmlTable.WriteRow("Orig. status", jobRequiredDocumentAddInfo.EX_Status);
			}
		}

		string CalculateMessageStatus(bool failed, string originalstatus)
		{
			var status = ZString.Empty;
			switch (originalstatus)
			{
				case StatusList.Codes.AwaitingOriginal:
					status = failed ? StatusList.Codes.ErrorAcknowledgedOriginal : StatusList.Codes.AcknowledgedOriginal;
					break;
				case StatusList.Codes.AwaitingAmendment:
					status = failed ? StatusList.Codes.ErrorAcknowledgedAmendment : StatusList.Codes.AcknowledgedAmendment;
					break;
				case StatusList.Codes.AwaitingChange:
					status = failed ? StatusList.Codes.ErrorAcknowledgedChange : StatusList.Codes.AcknowledgedChange;
					break;
				case StatusList.Codes.AwaitingWithdrawal:
					status = failed ? StatusList.Codes.ErrorAcknowledgedWithdrawal : StatusList.Codes.AcknowledgedWithdrawal;
					break;
			}
			return status;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		string CalculateCBSAMessageStatus(bool failed, string originalstatus)
		{
			var status = ZString.Empty;
			switch (originalstatus)
			{
				case StatusList.Codes.AcknowledgedOriginal:
				case StatusList.Codes.AwaitingOriginal:
				case StatusList.Codes.RejectedOriginal:
				case StatusList.Codes.AcceptedOriginal:
					status = failed ? StatusList.Codes.RejectedOriginal : StatusList.Codes.AcceptedOriginal;
					break;
				case StatusList.Codes.AcknowledgedAmendment:
				case StatusList.Codes.AwaitingAmendment:
				case StatusList.Codes.RejectedAmendment:
				case StatusList.Codes.AcceptedAmendment:
					status = failed ? StatusList.Codes.RejectedAmendment : StatusList.Codes.AcceptedAmendment;
					break;
				case StatusList.Codes.AcknowledgedChange:
				case StatusList.Codes.AwaitingChange:
				case StatusList.Codes.RejectedChange:
				case StatusList.Codes.AcceptedChange:
					status = failed ? StatusList.Codes.RejectedChange : StatusList.Codes.AcceptedChange;
					break;
				case StatusList.Codes.AcknowledgedWithdrawal:
				case StatusList.Codes.AwaitingWithdrawal:
				case StatusList.Codes.RejectedWithdrawal:
				case StatusList.Codes.AcceptedWithdrawal:
					status = failed ? StatusList.Codes.RejectedWithdrawal : StatusList.Codes.AcceptedWithdrawal;
					break;
			}
			return status;
		}

		ZString GetEmailBody()
		{
			var eventValueObject = (IXmlEventValueObject)eventDataObject;
			htmlTable.WriteRow("Event Time", eventValueObject.EventTime.ToZDateTime().ToStandardDateTimeString());
			var eventParameters = eventDataObject.EventParameters;
			if (eventParameters != null)
			{
				htmlTable.WriteRow("Document Number", eventParameters.ReferenceNumber.GetValueOrDefault());
				htmlTable.WriteRow("URN", GetURN());
				if (eventParameters.ReceiptNumber.HasValue)
				{
					htmlTable.WriteRow("Receipt Number", eventParameters.ReceiptNumber.Value);
				}
				if (eventParameters.Reason.HasValue)
				{
					htmlTable.WriteRow("Reason", eventParameters.Reason.Value);
				}
				if (eventParameters.Department.HasValue)
				{
					htmlTable.WriteRow("Department", eventParameters.Department.Value);
				}
			}
			return htmlTable.ToHtml();
		}

		ZString GetURN()
		{
			ZString result;
			result = eventDataObject.EventParameters?.RequestNumber.GetValueOrDefault() ?? ZString.Empty;
			return result;
		}

		ZString GetEmailHeader(bool failed, bool fromCBSA)
		{
			return Res.GetString(
				"2003E08E-5DCD-4531-8745-30A66392E4CE",
				"DIF Document {0} on {1} {2} {3}",
				GetURN(),
				disHost?.JobNumber,
				failed ? "is rejected" : "is accepted",
				fromCBSA ? "by CBSA" : ""
			);
		}

		ZString GetEmailHeaderWithLink(bool failed, bool fromCBSA)
		{
			var header = GetEmailHeader(failed, fromCBSA);
			var jobNumber = disHost?.JobNumber ?? ZString.Empty;

			if (!jobNumber.IsEmpty)
			{
				var declaration =
					(jobRequiredDocumentAddInfo.RequiredDocument.Parent as IDISHostProvider)?.DISHost as IControllerIDProvider;

				if (declaration != null)
				{
					var link = EmailDefBuilder.GetJobLink(declaration, jobNumber);
					header = header.Replace(jobNumber, link);
				}
			}
			return header;
		}

		EmailDef GetEmailAndSetOnMessage(bool failed, bool fromCBSA)
		{
			var emailBuilder = new EmailDefBuilder(GetEmailHeader(failed, fromCBSA), message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.IIDResponse);
			emailBuilder.AddArgReplacementRange(GetEmailHeaderWithLink(failed, fromCBSA), ZString.Empty);
			var messageInterpretation = GetEmailBody();
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, messageInterpretation);
			return emailBuilder.ToEmail();
		}

		GlbStaff GetUserToNotify(JobRequiredDocumentAddInfo parent)
		{
			return GetLastNonBatchProcessorStaffToSendMessage(parent);
		}

		GlbStaff GetLastNonBatchProcessorStaffToSendMessage(JobRequiredDocumentAddInfo parent)
		{
			GlbStaff result = null;
			var messages = new Messaging.Business.EDIMessageCollection(parent);
			messages.Load();
			var lastOutGoingMessage = messages.Cast<EDIMessage>()
										.Where(x => x.EM_ApplicationCode == ApplicationCodeList.Codes.UniversalDataMessaging && x.EM_ReceiveTransmit == "TRX" && x.EM_Status == EDIMessageStatusList.Codes.Sent && x.EM_SystemCreateUser != User.ServiceUserCode)
										.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			if (lastOutGoingMessage != null)
			{
				result = parent.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, lastOutGoingMessage.EM_SystemCreateUser);
			}
			return result;
		}

		protected void SendReport(EmailDef email)
		{
			var factory = message.Factory;
			var userToNotify = GetUserToNotify(jobRequiredDocumentAddInfo);
			if (userToNotify != null && !userToNotify.GS_EmailAddress.IsEmpty)
			{
				email.AddRecipientForUserCommunication(userToNotify.GS_EmailAddress, RecipientDef.RecipientTypes.TO);
			}

			if (email.Recipients.Count > 0 || email.CCRecipients.Count > 0 || email.BCCRecipients.Count > 0)
			{
				try
				{
					if (factory != null)
					{
						Env.OutgoingCustomsMailManager.Create(factory, email);
					}
					else // Just in case there's some error handling trying to report something when the factory has not been set.
					{
						Env.OutgoingCustomsMailManager.CreateAndSave(email);
					}
				}
				catch (EmailSendFailedException e)
				{
					logger.LogBoth(Integration.LogType.Error, "Couldn't send email: " + e.Message + ".  Here are the contents of the email that couln't be sent:\r\n\r\n" +
						"SUBJECT: " + email.Subject + "\r\n" +
						"BODY: " + email.Body + "\r\n");
				}
			}
		}
	}
}
