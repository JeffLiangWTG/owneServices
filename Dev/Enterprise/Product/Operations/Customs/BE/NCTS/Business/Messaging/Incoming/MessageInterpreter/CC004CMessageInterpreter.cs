using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC004CMessageInterpreter : BaseMessageInterpreter<ICC004CDataProvider>
	{
		public override string Interpret(ICC004CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			note.Append((NoResString)"New detailed status: 'Amendment acceptance'");
			note.Append((NoResString)"Status granted on: " + dataProvider.EntryDate.ToString(BE.Business.Constants.DateTimeFormats.LongTimeIncludingSecondsFormat));
			note.Append((NoResString)"Amendment submission date and time: " + dataProvider.SubmissionDate.ToString(BE.Business.Constants.DateTimeFormats.LongTimeIncludingSecondsFormat));
			note.Append((NoResString)"Correlation id: " + dataProvider.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
