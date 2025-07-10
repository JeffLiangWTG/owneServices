using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC029CMessageInterpreter : BaseMessageInterpreter<ICC029CDataProvider>
	{
		public override string Interpret(ICC029CDataProvider dataProvider, EDIMessage ediMessage)
		{
			const string format = "dd/MM/yyyy";
			var note = new ZStringBuilder();
			note.Append((NoResString)"New detailed status: Goods Released for Transit at Departure.");
			note.Append((NoResString)"Status granted on " + dataProvider.ReleaseDate.ToString(format));
			note.Append((NoResString)"Acceptance Date " + dataProvider.DeclarationAcceptanceDate.ToString(format));

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
