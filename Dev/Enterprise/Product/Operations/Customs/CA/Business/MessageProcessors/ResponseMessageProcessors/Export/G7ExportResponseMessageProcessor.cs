//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Core;
	using Enterprise.Customs.Business.MessageInterpretation;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Customs.Common;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Edifact.D00A.Messages.CUSRES;
	using Enterprise.Environment;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Schema;

	[ApplicationIdentifier(ServiceOptions.Codes.G7EDIExport)]
	public class G7ExportResponseMessageProcessor : ResponseMessageProcessor
	{
		public G7ExportResponseMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypeList.Codes.G7Export, Res.GetString("beac7ef5-9446-451f-9f42-9b0f5b6fc5d1", "G7 Export Response"))
		{
		}

		internal string DoProcessingReturningStatusForGenericSyntaxError(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			return DoProcessingReturningStatus(ediMessage);
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var resultStatus = ZString.Empty;
			message = (EXPEDIMessage)ediMessage;
			cusresMessage = (CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			if (cusresMessage != null)
			{
				linkedObjectReference = cusresMessage.BGM[0].DocumentMessageIdentification.DocumentIdentifier.Replace(" ", "");

				linkedObject = GetLinkedObjectAndSetOnMessage();
				if (linkedObject != null)
				{
					message.EM_MessageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);
					if (cusresMessage.GIS.Count > 0)
					{
						using (DisposableEnvironment.ForBranch(GetMessageBranchAndSetOnMessage(linkedObject, message).PK.ToGuid()))
						{
							var processingIndicator = cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode;
							if (processingIndicator == ProcessingIndicatorDescriptionCodeList.MessageReceived)
							{
								if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
								{
									linkedObject.MessageStatus = StatusCalculator.GetMessageAcknowledgedStatus(message);
								}
								resultStatus = EDIMessage.Status.Acknowledged;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted)
							{
								linkedObject.MessageStatus = StatusCalculator.GetMessageClearedStatus(message);
								SendAcknowledgementReport(EmailResponseLinkedObject, GetClearResponseEmailAndSetOnMessage());
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
							else if (processingIndicator == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
							{
								linkedObject.MessageStatus = StatusCalculator.GetMessageRejectedStatus(message);
								SendErrorReport(EmailResponseLinkedObject, GetErrorResponseEmailAndSetOnMessage());
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								resultStatus = EDIMessage.Status.Received;
							}
						}
					}
				}
				else
				{
					throw new CouldNotFindLinkedObjectException(linkedObjectReference, ediMessage, this);
				}
			}

			if (resultStatus.IsEmpty)
			{
				throw new UnableToInterpretMessageException(ediMessage, this);
			}

			return resultStatus;
		}

		#region Implementation

		#region Response Emails

		EmailDef GetClearResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("58c64b2b-61fb-4f8a-8b57-84112f05281d", "Clear {0} Response for {1}",
										StatusCalculator.MessageTypeDescription, linkedObjectReference);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.ClearResponse);
		}

		EmailDef GetErrorResponseEmailAndSetOnMessage()
		{
			var subject = Res.GetString("855d5cd7-47e3-4414-8c9a-e4a8b946cf78", "Error {0} Response for {1}",
										StatusCalculator.MessageTypeDescription, linkedObjectReference);
			return GetResponseEmailAndSetOnMessage(subject, EmailDefBuilder.HtmlTemplates.ErrorResponse);
		}

		EmailDef GetResponseEmailAndSetOnMessage(string subject, string templateName)
		{
			string sourceMessage;
			var wrapper = new G7ExportResponseMessageWrapper(message);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, templateName);
			emailBuilder.AddArgReplacementRange(LinkProvider.GetLink(linkedObject), linkedObjectReference, StatusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetHeaderHtmlText(wrapper));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, GetErrorMessagesHtmlText(wrapper, out sourceMessage));
			if (!string.IsNullOrEmpty(sourceMessage))
			{
				emailBuilder.AddAttachment("SourceMessageWithErrorMarks.txt", sourceMessage);
			}

			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		string GetHeaderHtmlText(G7ExportResponseMessageWrapper wrapper)
		{
			var dailyTotalTable = new FieldValueTableInterpretation();
			dailyTotalTable.AddIfNotEmpty(() => wrapper.ProcessingDate);

			var tranNumber = wrapper.CERSProofOfReportNumber;
			if (!tranNumber.IsEmpty)
			{
				((CusEntryHeader)linkedObject).Declaration.JE_CERSProofOfReportNumber = tranNumber;
				dailyTotalTable.Add(() => wrapper.CERSProofOfReportNumber, tranNumber);
			}

			return dailyTotalTable.ToHtml();
		}

		string GetErrorMessagesHtmlText(G7ExportResponseMessageWrapper wrapper, out string sourceMessageWithErrorMarks)
		{
			sourceMessageWithErrorMarks = null;
			if (wrapper.IsSyntaxError)
			{
				ErrorReporter.ReportOnce("G7 Export syntax error for " + linkedObjectReference, cusresMessage.ToString(new CACharSet()));

				message.EM_MessageType = MessageTypeList.Codes.SyntaxError;
				message.EM_MessageSubType = MessageTypeList.Codes.G7Export;
				var syntaxErrorMessage = message.Factory.Load<SyntaxErrorMessage>(message.PK);

				if (!wrapper.ErrorMessages.Any())
				{
					return Res.GetString("3f174914-0e7a-4769-84cb-1fdb2d80879c", "Malformed Syntax Error message from Customs, see message text for details.");
				}
				sourceMessageWithErrorMarks = syntaxErrorMessage.SourceMessageTextWithErrorMarks;
				return TableInterpretation.GetTableInterpretation(syntaxErrorMessage.SyntaxErrors, null, TableInterpretation.Attributes.AlignLeft);
			}
			return TableInterpretation.GetTableInterpretation(wrapper.ErrorMessages, null, TableInterpretation.Attributes.AlignLeft);
		}

		#endregion

		IEDIFACTMessageAttachee GetLinkedObjectAndSetOnMessage()
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));
			query.AddToFilter(CusEntryHeaderSchema.CH_MessageType, MessageTypeList.Codes.G7Export);
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, linkedObjectReference);
			subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, JobMessageTypeList.Codes.Export);
			subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Constants.CountryCodes.Canada);
			query.AddSubQuery(subQuery, JoinCondition.And);

			var cusEntryHeader = message.Factory.Load<CusEntryHeader>(query).FirstOrDefault();
			if (cusEntryHeader != null && cusEntryHeader.Declaration != null)
			{
				cusEntryHeader.Messages.Add(message);
			}

			return cusEntryHeader;
		}

		EDIFACTMessageStatusCalculator StatusCalculator
		{
			get { return statusCalculator ?? (statusCalculator = new G7ExportStatusCalculator()); }
		}
		EDIFACTMessageStatusCalculator statusCalculator;

		protected override BusinessObject EmailResponseLinkedObject
		{
			get { return linkedObject != null ? linkedObject.Messages.Master : null; }
		}

		#region Overridden Properties

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendEXPAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendEXPAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendEXPErrorsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendEXPErrors.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendEXPErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendEXPErrors.Value; }
		}

		#endregion

		IEDIFACTMessageAttachee linkedObject;
		ZString linkedObjectReference;
		CUSRESMessage cusresMessage;
		EXPEDIMessage message;

		#endregion
	}
}
