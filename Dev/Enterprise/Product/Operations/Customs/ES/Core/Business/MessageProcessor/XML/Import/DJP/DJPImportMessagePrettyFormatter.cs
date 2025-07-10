using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business
{
	public class DJPImportMessagePrettyFormatter : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public DJPImportMessagePrettyFormatter(DocumentosSimplifiV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly DocumentosSimplifiV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			AppendDescriptionDataIfNotEmpty(messageDetails);
			AppendAcceptanceDataIfNotEmpty(messageDetails, response.SegmentosDeServicio.Fecha + response.SegmentosDeServicio.Hora);
			AppendReferenceResponseDataIfNotEmpty(messageDetails);
			AppendCircuitIfNotEmpty(messageDetails, GetCircuit());

			return messageDetails.ToString();
		}

		protected void AppendDescriptionDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.DescripcionRespuesta))
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, DescriptionText, "(" + response.CodigoRespuesta + ")" + response.DescripcionRespuesta);
			}
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(RejectedDeclarationText);

			AppendErrorCodeDataIfNotEmpty(messageDetails);

			if (response.DeclaracionErronea != null && response.DeclaracionErronea.Count > 0)
			{
				var tableCreator = GetNewTableCreator();
				tableCreator.WriteRow(ErrorErrorColumnText, ErrorLocationAndDescriptionColumnText);

				foreach (var declarationWithError in response.DeclaracionErronea)
				{
					foreach (var error in declarationWithError.Error)
					{
						var locationDescription = $"{error.NumeroOrdenPartidaConError ?? 0}.{error.NumeroOrdenElementoErroneo ?? 0}\n{error.DescripcionError}.{error.EtiquetaConError}.{error.ValorErroneo}";
						tableCreator.WriteRow(error.CodigoError, locationDescription);
					}
				}
				messageDetails.Append(tableCreator.ToHtml());
			}

			return messageDetails.ToString();
		}

		protected void AppendReferenceResponseDataIfNotEmpty(StringBuilder messageDetails)
		{
			AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, ReferenceText, response.NumeroDeReferencia);
		}

		protected void AppendErrorCodeDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.CodigoRespuesta))
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, response.CodigoRespuesta + ":", response.DescripcionRespuesta);

				messageDetails.Append(tableCreator.ToHtml());
				messageDetails.Append(blankLine);
			}
		}

		ZString GetCircuit() => response.CircuitoValueSpecified ? circuitoTdList.ContainsKey(response.Circuito) ? GetColourCircuitStringXML(response.Circuito) : ZString.Empty : ZString.Empty;

		readonly ImmutableDictionary<CircuitoTd?, (string Code, string Format)> circuitoTdList = new Dictionary<CircuitoTd?, (string, string)>()
		{
			{ CircuitoTd.V, (CircuitCodeList.Descriptions.GREEN, green) },
			{ CircuitoTd.R, (CircuitCodeList.Descriptions.RED, red) },
			{ CircuitoTd.N, (CircuitCodeList.Descriptions.ORANGE, orange) },
			{ CircuitoTd.A, (CircuitCodeList.Descriptions.YELLOW, yellow) }
		}.ToImmutableDictionary();

		protected ZString GetColourCircuitStringXML(CircuitoTd? circuitoTd) => HTMLColourString(circuitoTdList[circuitoTd].Format, circuitoTdList[circuitoTd].Code);
	}
}
