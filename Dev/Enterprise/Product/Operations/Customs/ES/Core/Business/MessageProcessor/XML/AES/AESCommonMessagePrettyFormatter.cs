using System;
using System.Text;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public abstract class AESCommonMessagePrettyFormatter : CommonMessagePrettyFormatter
	{
		const string IndirectFlagCode = "I";
		string IndirectFlagDescription => ResString.GetMultilingualString("A7670F2D-52D8-408F-9D08-AC4A4F1EC3AA", "[I] Indirect");
		string DirectFlagDescription => ResString.GetMultilingualString("C1A5BCA0-18CC-4514-9770-D9AAB43FFDA6", "[D] Direct");
		const string ExitResultDissatisfiedCode = "B1";
		string ExitResultDissatisfiedDescription => ResString.GetMultilingualString("9D084AC9-64CC-486F-A7D6-0E9BC92A322D", "Dissatisfied");
		string ExitResultSatisfiedDescription => ResString.GetMultilingualString("DF764272-2995-46DD-A977-73158B56FA9B", "Satisfied");

		protected void AppendGroupCircuit(StringBuilder messageDetails, string circuitAEAT, string circuitATC)
		{
			var circuitAEATCode = GetCircuitFromText(circuitAEAT);
			var circuitATCCode = GetCircuitFromText(circuitATC);
			if (!circuitAEATCode.IsEmpty || !circuitATCCode.IsEmpty)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				AppendCircuitIfNotEmpty(messageDetails, circuitAEATCode, tableCreator);
				AppendCircuitCanIfNotEmpty(circuitATCCode, tableCreator);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendExitTypeDataIfNotEmpty(StringBuilder messageDetails, string exitTypeFlag)
		{
			if (!string.IsNullOrEmpty(exitTypeFlag))
			{
				var exitTypeDescription = exitTypeFlag == IndirectFlagCode ? IndirectFlagDescription : DirectFlagDescription;
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ExitTypeText, exitTypeDescription);
			}
		}

		protected void AppendMessageType(StringBuilder messageDetails, ZString messageTypeDescription) => AppendDataInNewTableIfNotEmpty(messageDetails, MessageTypeText, messageTypeDescription);

		protected void AppendDateValueWithddMMyyyFormatInNewTableIfNotEmpty(StringBuilder messageDetails, DateTime? receivedDate, ZString dateText)
		{
			if (receivedDate != null)
			{
				var tableCreator = GetNewNonVisibleTableCreator();
				AppendDateWithddMMyyyyFormatIfNotEmpty((ZDateTime)receivedDate, tableCreator, dateText);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendExitResultData(StringBuilder messageDetails, ZString exitResultCode)
		{
			if (!string.IsNullOrEmpty(exitResultCode))
			{
				var exitResultDescription = exitResultCode == ExitResultDissatisfiedCode ? ExitResultDissatisfiedDescription : ExitResultSatisfiedDescription;
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ExitResultText, exitResultCode + " - " + exitResultDescription);
			}
		}
	}
}
