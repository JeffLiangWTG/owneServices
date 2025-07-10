using CargoWise.Customs.BE.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC928CMessageInterpreter : BE.Business.BaseMessageInterpreter<ICC928CDataProvider>
	{
		public override string Interpret(ICC928CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			note.Append((NoResString)"New declaration status: 'Declaration Accepted'");
			note.Append((NoResString)"Correlation id: " + dataProvider.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
