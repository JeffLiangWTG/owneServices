using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class ExportMessagePrettyFormatter : EdiFactV921ESMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ExportMessagePrettyFormatter(IExportResponseMessageProvider messageHelperProvider) : base(messageHelperProvider)
		{
			messageProvider = messageHelperProvider;
		}
		readonly IExportResponseMessageProvider messageProvider;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string preDueText = "PreSAD";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string complementaryDueText = "Complementary";
		protected override ZString GetCircuit()
		{
			ZString circuit = base.GetCircuit();
			switch (messageProvider.MessageFunction)
			{
				case MessageFunctionCodeList.Codes.PreDue:
					circuit = HTMLNoColourString(preDueText);
					break;
				case MessageFunctionCodeList.Codes.ComplementaryDue:
					circuit = HTMLNoColourString(complementaryDueText);
					break;
			}
			return circuit;
		}

		protected override ZBool HasAllClearanceData() => !messageProvider.CSVReleaseCode.IsEmpty || !messageProvider.CSVReleaseCreationDate.IsEmpty || !messageProvider.CustomsClearanceStatus.IsEmpty || !messageProvider.PrintActionRequired.IsEmpty;

		protected override ZString GetClearanceCriteria()
		{
			ZString clearanceCriteria = string.Empty;
			switch (messageProvider.CustomsClearanceStatus)
			{
				case ClearanceResultCodeList.Codes.A1:
					clearanceCriteria = ClearanceResultCodeList.Descriptions.A1;
					break;
				case ClearanceResultCodeList.Codes.A2:
					clearanceCriteria = ClearanceResultCodeList.Descriptions.A2;
					break;
			}
			return ReplaceBrackets(clearanceCriteria);
		}

		ZString ReplaceBrackets(ZString clearanceCriteria) => clearanceCriteria.Replace("[", "(").Replace("]", ")");

		protected override ZString GetPrintProcedure()
		{
			ZString printProcedure = string.Empty;
			switch (messageProvider.PrintActionRequired)
			{
				case EADPrintProcedureCodeList.Codes._0NoEADPrint:
					printProcedure = EADPrintProcedureCodeList.Descriptions._0NoEADPrint.Replace("[0] ", "");
					break;
				case EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities:
					printProcedure = EADPrintProcedureCodeList.Descriptions._1EADPrintedByCustomsAuthorities.Replace("[1] ", "");
					break;
				case EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant:
					printProcedure = EADPrintProcedureCodeList.Descriptions._2EADCanBePrintedByDeclarant.Replace("[2] ", "");
					break;
			}
			return printProcedure;
		}
	}
}
