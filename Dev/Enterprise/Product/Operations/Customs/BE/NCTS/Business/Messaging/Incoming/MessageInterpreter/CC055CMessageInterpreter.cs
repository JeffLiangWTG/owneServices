using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.BE.MessageContracts.MessageProviders;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC055CMessageInterpreter : BaseMessageInterpreter<ICC055CDataProvider>
	{
		public override string Interpret(ICC055CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			note.Append((NoResString)"Guarantee invalid.");

			var list = new NCTS5InvalidGuaranteeReason();

			foreach (GuaranteeReferenceXmlProvider guaranteeReference in dataProvider.GuaranteeReferences)
			{
				note.Append("GRN " + guaranteeReference.GRN);
				var invalidGuaranteeReason = guaranteeReference.InvalidGuaranteeReason;
				foreach (var reason in invalidGuaranteeReason)
				{
					note.Append((NoResString)"Reason : sequence: " + reason.SequenceNumber);
					note.Append((NoResString)"Reason : code: " + reason.Code + " " + list.GetDescriptionFromCode(reason.Code));
					note.Append((NoResString)"Reason : text: " + reason.Text);
				}
			}

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
