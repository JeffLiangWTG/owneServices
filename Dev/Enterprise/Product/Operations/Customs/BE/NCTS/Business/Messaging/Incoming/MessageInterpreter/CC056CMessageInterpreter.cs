using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC056CMessageInterpreter : BaseMessageInterpreter<ICC056CDataProvider>
	{
		public override string Interpret(ICC056CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();

			note.Append($"Declaration received an error for type {dataProvider.BusinessRejectionType} on {NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(dataProvider.RejectionDateAndTimeUtc)}");
			note.Append($"Reason: {dataProvider.RejectionCode} {dataProvider.RejectionReason}");

			var errorReasonList = new ErrorReasonList();

			foreach (var functionalError in dataProvider.FunctionalErrorList)
			{
				var errorReasonCode = functionalError.ErrorReason;
				var errorReasonDescription = errorReasonList.GetDescriptionFromCode(errorReasonCode);
				var errorReason = errorReasonCode + (!string.IsNullOrEmpty(errorReasonDescription) ? $" == {errorReasonDescription}" : "");

				note.Append($"Functional error code: {functionalError.ErrorCode}");
				note.Append($"Reason: {errorReason}");
				note.Append($"Attribute: {functionalError.ErrorPointer}");
				note.Append($"Element in declaration contains now the value: {functionalError.OriginalAttributeValue}");
				note.Append("");
			}
			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
