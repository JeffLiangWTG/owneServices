using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC057CMessageInterpreter : BaseMessageInterpreter<ICC057CDataProvider>
	{
		public override string Interpret(ICC057CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var noteBuilder = new ZStringBuilder();

			if (dataProvider.TransitOperation is TransitOperationXmlProvider transitOperation)
			{
				var rejectionTypeDescription = new BEOutgoingMessageTypes().GetMultilingualDescriptionFromCode(transitOperation.BusinessRejectionType);
				var rejectionDateAndTimeString = NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(transitOperation.RejectionDateAndTimeUtc);
				var rejectionCodeDescription = new EU.NCTS.Business.RejectionCodes().GetMultilingualDescriptionFromCode(transitOperation.RejectionCode);

				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Declaration received an error for type {0} ({1}) on {2}.", transitOperation.BusinessRejectionType, rejectionTypeDescription, rejectionDateAndTimeString));
				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Reason: {0} ({1}) {2}", transitOperation.RejectionCode, rejectionCodeDescription, transitOperation.RejectionReason));
			}

			foreach (var functionalError in dataProvider.FunctionalErrors)
			{
				var functionalErrorCodeDescription = new EU.NCTS.Business.FunctionalErrorCodes().GetMultilingualDescriptionFromCode(functionalError.ErrorCode);
				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Functional error code: {0} ({1})", functionalError.ErrorCode, functionalErrorCodeDescription));
				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Reason: {0}", functionalError.ErrorReason));
				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Attribute: {0}", functionalError.ErrorPointer));
				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Element in declaration contains now the value: {0}", functionalError.OriginalAttributeValue));
				AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Empty);
			}
			return noteBuilder.ToString();
		}
	}
}
