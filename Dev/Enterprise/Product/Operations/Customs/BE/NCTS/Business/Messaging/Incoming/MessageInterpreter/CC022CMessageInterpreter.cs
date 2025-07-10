using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC022CMessageInterpreter : BaseMessageInterpreter<ICC022CDataProvider>
	{
		public override string Interpret(ICC022CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			if (dataProvider.AmendmentNotificationDateAndTime is DateTime amendmentNotificationDateAndTime)
			{
				note.Append($"Declaration received a request to amend the declaration on {amendmentNotificationDateAndTime.ToString("dd-MMM-y HH:mm:ss")}");
			}
			else
			{
				note.Append($"Declaration received a request to amend the declaration");
			}
			foreach (var functionaError in dataProvider.FunctionalErrors)
			{
				note.Append($"{functionaError.SequenceNumber}. Functional error code: {functionaError.ErrorCode}");
				note.Append($"Reason: {functionaError.ErrorReason}");
				note.Append($"Attribute: {functionaError.ErrorPointer}");
				note.Append($"Element in declaration contains now the value: {functionaError.OriginalAttributeValue}{BE.Business.Constants.HtmlContent.Break}");
			}
			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
