using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC009CMessageInterpreter : BaseMessageInterpreter<ICC009CDataProvider>
	{
		public override string Interpret(ICC009CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			note.Append($"New declaration status: {(dataProvider.Decision ? (NoResString)"Cancellation accepted" : (NoResString)"Cancellation refused")}");
			note.Append($"Status granted on: {NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(dataProvider.DecisionDateTimeUtc)}");
			note.Append($"Request date and time to invalidate/cancel: {NctsMessageHelper.GetUtcDateTimeToLocalBEBranchString(dataProvider.RequestDateTimeUtc)}");
			note.Append($"Initiated by customs: {(dataProvider.InitiatedByCustoms ? (NoResString)"yes" : (NoResString)"no")}");
			note.Append($"Justification: {dataProvider.Justification}");
			note.Append($"Correlation id: {dataProvider.CorrelationIdentifier}");

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
