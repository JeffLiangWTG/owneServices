using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business
{
	public static class MonthlyClosingHelper
	{
		public static ZString GetMonthlyClosingLinesNote(this EDIMessage message) => message.GetNote(MonthlyClosingLinesNoteDescription);

		public static void CreateMonthlyClosingLinesNote(this EDIMessage message, IEnumerable<int> lineNumbersInMessage)
			=> message.CreateOrUpdateNote(MonthlyClosingLinesNoteDescription, lineNumbersInMessage?.Select(x => new ZString(x.ToString())).ToArray() ?? Array.Empty<ZString>(), MonthlyClosingLinesNoteDescriptionSeperator);

		public static ZString GetFinalizationFlagNote(this CusReconDeclaration reconDeclaration) => reconDeclaration.GetNote(FinalizationFlagNoteDescription);

		public static void CreateFinalizationFlagNote(this CusReconDeclaration reconDeclaration, ZString value)
		{
			reconDeclaration.CreateOrUpdateNote(FinalizationFlagNoteDescription, value);
		}

		internal const string MonthlyClosingLinesNoteDescription = "MonthlyClosingLines";

		internal const string MonthlyClosingLinesNoteDescriptionSeperator = "|";

		internal const string FinalizationFlagNoteDescription = "FinalizationFlag";

		internal const string DeclarationIsFinalizedFlag = "1";

		internal const string DeclarationNotFinalizedFlag = "0";
	}
}
