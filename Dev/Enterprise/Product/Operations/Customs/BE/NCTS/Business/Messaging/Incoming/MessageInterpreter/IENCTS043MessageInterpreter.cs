using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class IENCTS043MessageInterpreter : BaseMessageInterpreter<ICC043CAndIENCTS043CDataProvider>
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1062:DoNotUseDateTimeToday", Justification = "Baseline")]
	public override string Interpret(ICC043CAndIENCTS043CDataProvider dataProvider, EDIMessage ediMessage)
	{
		var noteBuilder = new ZStringBuilder();
		AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Unloading Permission"));
		AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Status granted on: {0}", dataProvider.DeclarationAcceptanceDate.ToString("dd/MM/yyyy HH:mm:ss")));

		return noteBuilder.ToString();
	}
}
