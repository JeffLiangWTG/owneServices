using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC515X_v514.CC515XV1Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class ComplXAESMessagePrettyFormatter : AESCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ComplXAESMessagePrettyFormatter(Cc515Xv1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly Cc515Xv1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				messageDetails.Append(blankLine);
				AppendMrnDataIfNotEmpty(messageDetails, correctResponseData.Mrn);
				AppendCircuitComplementary(messageDetails, correctResponseData.ComplementariaFueraPlazo);
				AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, correctResponseData.CsvDeclaracionElectronica);
				AppendStatusData(messageDetails, correctResponseData.EstadoAes);
				AppendComplementary(messageDetails, correctResponseData.ComplementariaFueraPlazo);
			}

			return messageDetails.ToString();
		}

		void AppendComplementary(StringBuilder messageDetails, string complementary)
		{
			if (!string.IsNullOrEmpty(complementary))
			{
				var complementaryDescription = complementary == ComplementaryInTerm ? ComplementaryInTermDescription : ComplementaryOutTermDescription;
				var tableCreator = GetNewNonVisibleTableCreator();
				tableCreator.WriteRow(complementaryDescription);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendCircuitComplementary(StringBuilder messageDetails, string complementary)
		{
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CircuitText, CircuitComplementaryText);
		}

		string CircuitComplementaryText => ResString.GetMultilingualString("0F4AB7F7-DC89-415D-A2A2-00BEC9470B79", "Complementary");
		const string ComplementaryInTerm = "D";
		string ComplementaryInTermDescription => ResString.GetMultilingualString("EDBAB7C4-CCDB-4A1B-B6D0-A9F7F88FEA9D", "Complementary In Term");
		string ComplementaryOutTermDescription => ResString.GetMultilingualString("A812D565-26E0-4403-BF38-2F878214AB7F", "Complementary Out Term");

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedCommon(response.ControlRespuesta.TipoRespuesta, response);
	}
}
