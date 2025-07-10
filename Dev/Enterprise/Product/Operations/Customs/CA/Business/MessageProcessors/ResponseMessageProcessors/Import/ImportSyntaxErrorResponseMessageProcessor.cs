using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class ImportSyntaxErrorResponseMessageProcessor : ImportResponseMessageProcessor
	{
		public ImportSyntaxErrorResponseMessageProcessor(LoggingInformation logger)
			: base(logger, new B3ImportStatusCalculator(), MessageTypeList.Codes.SyntaxError, Res.GetString("b9e9750e-cf5a-4f18-9d99-3244fcef9685", "Import Syntax Error Response"))
		{
		}

		protected override string DoProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			base.DoPreProcessingReturningStatus(ediMessage);
			var resultStatus = ZString.Empty;
			var cusresMessage = (CUSRESMessage)ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			if (cusresMessage != null)
			{
				if (cusresMessage.GIS.Count > 0 && cusresMessage.GIS[0].ProcessingIndicator_X.ProcessingIndicatorDescriptionCode == ProcessingIndicatorDescriptionCodeList.ErrorMessage)
				{
					var message = (SyntaxErrorMessage)ediMessage;
					var accountSecurityNumber = D99BMessageUtilities.GetAccountSecurityNumber(cusresMessage);
					var documentNumber = D99BMessageUtilities.GetDocumentName(cusresMessage);
					linkedObjectReference = accountSecurityNumber + documentNumber;
					linkedObject = ImportLinkedObjectManager.GetCusEntryHeaderByTransactionNumber(ediMessage.Factory, linkedObjectReference, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
					if (linkedObject == null)
					{
						var matchingDeclarations = ImportLinkedObjectManager.LoadObjectsByCusEntryNumber<JobDeclaration>(message.Factory, new[] { Common.CusEntryNumber.EntryType.CATransactionNumber }, linkedObjectReference);
						if (matchingDeclarations.Length == 1)
						{
							var dec = matchingDeclarations[0];
							if (dec.IsB3X)
							{
								dec.Messages.Add(ediMessage);
								linkedObject = dec;
							}
						}
					}
					else
					{
						linkedObject.Messages.Add(message);
					}

					if (linkedObject != null)
					{
						if (linkedObject.TopLevelBusinessObject is JobDeclaration dec && dec.IsB3X)
						{
							ediMessage.EM_MessageSubType = MessageTypeList.Codes.XTypeEntry;
						}
						else
						{
							ediMessage.EM_MessageSubType = MessageTypeList.Codes.B3CUSDEC;
						}

						using (DisposableEnvironment.ForBranch(GetMessageBranchAndSetOnMessage(linkedObject, ediMessage).PK.ToGuid()))
						{
							var email = GetErrorResponseEmailAndSetOnMessage(cusresMessage, message);
							if (StatusCalculator.IsAwaitingReply(linkedObject.MessageStatus))
							{
								var messageSubType = StatusCalculator.GetMessageSubType(linkedObject.MessageStatus);
								linkedObject.MessageStatus = StatusCalculator.GetMessageRejectedStatus(messageSubType);
								linkedObject.JobStatus = StatusCalculator.CalculatedJobStatus(linkedObject);
								SendErrorReport(EmailResponseLinkedObject, email);
							}

							resultStatus = EDIMessage.Status.Received;
						}
					}
					else if (string.IsNullOrEmpty(documentNumber))
					{
						ediMessage.EM_MessageSubType = MessageTypeList.Codes.Query;
						SendErrorReport(null, GetQueryMessageErrorResponseEmailAndSetOnMessage(cusresMessage, message)); //TODO: Enhance SendErrorReport to take query to find notification user
						resultStatus = EDIMessage.Status.Received;
					}
					else
					{
						throw new CouldNotFindLinkedObjectException(linkedObjectReference, ediMessage, this);
					}
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

		#region GetQueryMessageErrorResponseEmailAndSetOnMessage

		EmailDef GetQueryMessageErrorResponseEmailAndSetOnMessage(CUSRESMessage cusresMessage, EDIMessage message)
		{
			var subject = Res.GetString("2ecdfa62-831c-4445-b907-5e9b15fdb4a1", "Query Message Syntax Error Response");
			var emailBuilder = new EmailDefBuilder(subject, message.EM_MessageText.Replace("'", "\r\n"), EmailDefBuilder.HtmlTemplates.QueryResponse);
			emailBuilder.AddArgReplacement(Res.GetString("d8a465c7-b252-406b-9716-c0499e9b3fcf", "Syntax Error"));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetErrorMessageText1(cusresMessage, message) + "<br /><br />" + GetErrorMessageText2(cusresMessage, message));
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return AttachSourceMessageText(emailBuilder.ToEmail(), message);
		}

		#endregion

		#region GetErrorResponseEmailAndSetOnMessage

		EmailDef GetErrorResponseEmailAndSetOnMessage(SegmentGroup cusresMessage, EDIMessage message)
		{
			return AttachSourceMessageText(base.GetErrorResponseEmailAndSetOnMessage(cusresMessage, message), message);
		}

		static EmailDef AttachSourceMessageText(EmailDef email, EDIMessage message)
		{
			var sourceMessage = ((SyntaxErrorMessage)message).SourceMessageTextWithErrorMarks;
			if (!sourceMessage.IsEmpty)
			{
				email.Attachments.Add(new AttachmentDef("SourceMessageWithErrorMarks.txt", Encoding.ASCII.GetBytes(sourceMessage)));
			}
			return email;
		}

		protected override string GetErrorMessageText1(SegmentGroup cusresMessage, EDIMessage message)
		{
			return TableInterpretation.GetTableInterpretation(((SyntaxErrorMessage)message).SyntaxErrors, null, TableInterpretation.Attributes.AlignLeft);
		}

		protected override string GetErrorMessageText2(SegmentGroup cusresMessage, EDIMessage message)
		{
			return Res.GetString("f5913854-1226-4814-9857-65b48e68a115", "Please report this to CargoWise.");
		}

		#endregion

		#endregion

		#endregion
	}
}
