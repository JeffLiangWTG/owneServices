using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC028CMessageInterpreter : BaseMessageInterpreter<ICC028CDataProvider>
	{
		public override string Interpret(ICC028CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			note.Append((NoResString)"New declaration status: Declaration MRN Allocated");
			note.Append((NoResString)"Status granted on: " + dataProvider.EntryDate);
			note.Append((NoResString)"Correlation id: " + dataProvider.CorrelationIdentifier);

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
