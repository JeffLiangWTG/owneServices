using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC051CMessageInterpreter : BaseMessageInterpreter<ICC051CDataProvider>
	{
		public override string Interpret(ICC051CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var note = new ZStringBuilder();
			var motivationCode = dataProvider.NoReleaseMotivationCode ?? string.Empty;
			note.Append((NoResString)"Declaration is NOT RELEASED FOR TRANSIT AT DEPARTURE.");
			note.Append($"Motivation code: {motivationCode} {new NCTS5NoReleaseMotivation().GetDescriptionFromCode(motivationCode)}");
			if (!string.IsNullOrEmpty(dataProvider.NoReleaseMotivationText))
			{
				note.Append(dataProvider.NoReleaseMotivationText);
			}

			return note.ToStringWithDelimiterBetweenAppends(BE.Business.Constants.HtmlContent.Break);
		}
	}
}
