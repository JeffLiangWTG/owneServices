using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC140CMessageInterpreter : BaseMessageInterpreter<ICC140CDataProvider>
	{
		public override string Interpret(ICC140CDataProvider dataProvider, EDIMessage ediMessage)
		{
			const string format = "dd/MM/yyyy";
			var note = new ZStringBuilder();
			note.Append((NoResString)"New Customs Status: 'Request on Non-Arrived Movement'");
			note.Append((NoResString)"Status granted on " + dataProvider.RequestOnNonArrivedMovementDate.ToString(format));
			note.Append((NoResString)"Response on the Request for info on Non-Arrived Movement, must be sent to customs before " + dataProvider.LimitForResponseDate.ToString(format));

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
