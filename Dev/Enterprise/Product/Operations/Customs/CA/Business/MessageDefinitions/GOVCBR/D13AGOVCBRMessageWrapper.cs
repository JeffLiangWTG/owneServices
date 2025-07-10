using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Edifact.D13A.Elements;
using Enterprise.Edifact.D13A.Messages.GOVCBR;
using Enterprise.Edifact.D13A.Segments;

namespace Enterprise.Customs.CA.Business
{
	public class D13AGOVCBRMessageWrapper : GOVCBRMessageWrapper
	{
		public D13AGOVCBRMessageWrapper(UniversalEventMessage message)
			: base(message)
		{
			Argument.NotNull(message, "message");
			GOVCBR = Argument.NotNull(message.GOVCBR, "message", "Supported EDIFACT message type is D13A GOVCBR");
		}
		GOVCBRMessage GOVCBR { get; }

		public override ZBool HasGOVCBRMessage => GOVCBR != null;

		protected override ZString GetDocumentName => GOVCBR.BGM.Count > 0 ? GOVCBR.BGM[0].DocumentMessageName.DocumentNameCode : string.Empty;

		protected override ZString GetDocumentReference => GOVCBR.BGM.Count > 0 ? GOVCBR.BGM[0].DocumentMessageIdentification.DocumentIdentifier : string.Empty;

		protected override ZDateTime GetProcessingDate => (from DTMSegment dtm in GOVCBR.DTM
														   where dtm.DateTimePeriod.DateOrTimeOrPeriodFunctionCodeQualifier == DateOrTimeOrPeriodFunctionCodeQualifierList.ProcessingDateTime
														   select D00AMessageUtilities.ParseDate(dtm.DateTimePeriod.DateOrTimeOrPeriodText)).FirstOrDefault();

		public override ZBool IsMessageContentAccepted => ProcessingIndicator == ProcessingIndicatorDescriptionCodeList.MessageContentAccepted;

		public override ZBool IsMessageContentAcceptedWithComments => ProcessingIndicator == ProcessingIndicatorDescriptionCodeList.MessageContentAcceptedWithComments;

		public override ZBool IsMessageContentRejectedWithComment => ProcessingIndicator == ProcessingIndicatorDescriptionCodeList.MessageContentRejectedWithComment;

		public override ZBool IsMessageReceived => ProcessingIndicator == ProcessingIndicatorDescriptionCodeList.MessageReceived;

		public override ZBool IsErrorMessage => ProcessingIndicator == ProcessingIndicatorDescriptionCodeList.ErrorMessage;

		ProcessingIndicatorDescriptionCodeList ProcessingIndicator
		{
			get
			{
				if (processingIndicator == null)
				{
					if (GOVCBR.Group13.Count > 0 && GOVCBR.Group13[0].GEI.Count > 0)
					{
						processingIndicator = GOVCBR.Group13[0].GEI[0].ProcessingIndicator.ProcessingIndicatorDescriptionCode;
					}
					else
					{
						processingIndicator = ProcessingIndicatorDescriptionCodeList.GetFromString(ZString.Empty);
					}
				}

				return processingIndicator;
			}
		}
		ProcessingIndicatorDescriptionCodeList processingIndicator;

		protected override ZString GetOriginalMessageReference => (from RFFSegment rff in GOVCBR.RFF
																   where rff.Reference.ReferenceCodeQualifier == ReferenceCodeQualifierList.SendersReferenceToTheOriginalMessage
																   select rff.Reference.ReferenceIdentifier).FirstOrDefault();

		protected override IEnumerable<string> GetErrorComments => from SegmentGroup13 group13 in GOVCBR.Group13
																   from FTXSegment ftx in group13.FTX
																   where ftx.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.ErrorDescriptionFreeText
																   select ErrorCommentText(ftx.TextLiteral.FreeText1);

		protected override IEnumerable<Notification> GetNotifications => from SegmentGroup17 group17 in GOVCBR.Group17
																		 from ERCSegment ercSeg in group17.ERC
																		 select new Notification(Message, ercSeg.ApplicationErrorDetail.ApplicationErrorCode);

		protected override ZString GetNoticeStatusCode => DocumentName == ServiceOptions.Codes.StatusInfo && GOVCBR.Group5.Count > 0 && GOVCBR.Group5[0].STS.Count > 0 &&
						GOVCBR.Group5[0].STS[0].Status.StatusDescriptionCode == StatusDescriptionCodeList.Done ? GOVCBR.Group5[0].STS[0].Status.StatusDescription : string.Empty;
	}
}
