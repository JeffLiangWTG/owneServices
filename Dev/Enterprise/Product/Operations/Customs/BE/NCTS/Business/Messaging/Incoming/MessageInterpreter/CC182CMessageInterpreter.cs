using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC182CMessageInterpreter : BaseMessageInterpreter<ICC182CDataProvider>
	{
		public override string Interpret(ICC182CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var noteBuilder = new ZStringBuilder();
			AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Phase: {0} - {1}", NctsMovementHeaderTransactionStatusList.Codes.ForwardedIncidentNotification, NctsMovementHeaderTransactionStatusList.Descriptions.ForwardedIncidentNotification));
			AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Status granted on: {0}", dataProvider.IncidentDateAndTime.ToString("dd/MM/yyyy HH:mm:ss")));

			return noteBuilder.ToString();
		}
	}
}
