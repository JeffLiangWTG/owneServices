//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.BatchProcessor;
	using Enterprise.Customs.Business.MessageProcessors;
	using Enterprise.Customs.CA.Registry;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.Auto;
	using Enterprise.ZArchitecture.Environment;

	abstract class ImportResponseMessageProcessor : ResponseMessageProcessor
	{
		protected ImportResponseMessageProcessor(LoggingInformation logger, EDIFACTMessageStatusCalculator statusCalculator, ZString messageTypeCode, ZString messageTypeDescription)
			: base(logger, messageTypeCode, messageTypeDescription)
		{
			StatusCalculator = statusCalculator;
		}

		protected override string DoPreProcessingReturningStatus(Enterprise.Messaging.Business.EDIMessage message)
		{
			this.LinkedObjectManager = new ImportLinkedObjectManager(message);
			return base.DoPreProcessingReturningStatus(message);
		}

		#region Overridden Properties

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return CACustomsDataRegistry.Instance.SendDeclarationMessageErrors.Value; }
		}

		protected override BusinessObject EmailResponseLinkedObject
		{
			get { return linkedObject != null ? linkedObject.Messages.Master : null; }
		}

		#endregion

		#region Implementation

		#region GetAcceptedResponseEmailAndSetOnMessage

		protected EmailDef GetAcceptedResponseEmailAndSetOnMessage(SegmentGroup cusresMessage, EDIMessage message, string messageTypeDescription = "")
		{
			if (string.IsNullOrEmpty(messageTypeDescription))
			{
				messageTypeDescription = StatusCalculator.MessageTypeDescription;
			}
			var subject = EDIReleaseImportEntryStatusList.IsCancellation(message.EM_MessageSubType) ?
				Res.GetString("BE8FD0FF-DFBD-4822-AA48-DC127190EC3E", "Cancellation accepted {0} Response for {1}", messageTypeDescription, linkedObjectReference) :
				Res.GetString("fad20d1d-2c9d-444a-a9e0-73b38405bbc5", "Accepted {0} Response for {1}", messageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.AcceptedResponse);
			emailBuilder.AddArgReplacementRange(LinkProvider.GetLink(linkedObject), linkedObjectReference, messageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetAcceptedMessageText(cusresMessage, message), true);
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		protected virtual string GetAcceptedMessageText(SegmentGroup cusresMessage, EDIMessage message)
		{
			return string.Empty;
		}

		#endregion

		#region GetErrorResponseEmailAndSetOnMessage

		protected EmailDef GetErrorResponseEmailAndSetOnMessage(SegmentGroup cusresMessage, EDIMessage message, string messageTypeDescription = "")
		{
			if (string.IsNullOrEmpty(messageTypeDescription))
			{
				messageTypeDescription = StatusCalculator.MessageTypeDescription;
			}

			var subject = Res.GetString("032b9129-a05a-474d-9b65-66bde6a8863f", "Error {0} Response for {1}", messageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_MessageText.Replace("'", "\r\n"), EmailDefBuilder.HtmlTemplates.ErrorResponse);
			emailBuilder.AddArgReplacementRange(LinkProvider.GetLink(linkedObject), linkedObjectReference, messageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetErrorMessageText1(cusresMessage, message));
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, GetErrorMessageText2(cusresMessage, message));
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		protected virtual string GetErrorMessageText1(SegmentGroup cusresMessage, EDIMessage message)
		{
			return string.Empty;
		}

		protected virtual string GetErrorMessageText2(SegmentGroup cusresMessage, EDIMessage message)
		{
			return string.Empty;
		}

		#endregion

		protected IEDIFACTMessageAttachee linkedObject;
		protected ZString linkedObjectReference;
		protected EDIFACTMessageStatusCalculator StatusCalculator { get; private set; }
		protected ImportLinkedObjectManager LinkedObjectManager { get; private set; }

		#endregion
	}
}
