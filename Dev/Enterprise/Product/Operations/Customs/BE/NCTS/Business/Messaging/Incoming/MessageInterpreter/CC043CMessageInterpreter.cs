using System;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class CC043CMessageInterpreter : BaseMessageInterpreter<ICC043CDataProvider>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1062:DoNotUseDateTimeToday", Justification = "Baseline")]
		public override string Interpret(ICC043CDataProvider dataProvider, EDIMessage ediMessage)
		{
			var continueUnloadingText = ZString.Empty;

			if (dataProvider.ContinueUnloading == null)
			{
				continueUnloadingText = (NoResString)"Started";
			}
			else
			{
				if (dataProvider.ContinueUnloading == false)
				{
					continueUnloadingText = (NoResString)"Final";
				}
				else
				{
					continueUnloadingText = (NoResString)"Continue";
				}
			}

			var noteBuilder = new ZStringBuilder();
			AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Unloading Permission: {0} ", continueUnloadingText));
			AddHTMLNoteTextLineInterpretation(noteBuilder, ZString.Format((NoResString)"Status granted on: {0}", DateTime.Today.ToString("dd/MM/yyyy HH:mm:ss")));

			return noteBuilder.ToString();
		}
	}
}
