using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LanexosV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class T2LAnnexMessagePrettyFormatterTest : T2LCommonMessagePrettyFormatterTest<T2LanexosV1Sal>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new T2LAnnexMessagePrettyFormatter(null));
		}

		public void TestCreateMessageDetailsAccepted()
		{
			var declarationResponse = SetResponseData("20211004", "103045", ZString.Empty, "HHAXSXPQA6NT963Y", "9KJLVLPCWNKUV7N8", "Documento Anexado", hasCircuitAssigment: true);
			var messageInterpretationText = GetAcceptedInterpretationText(declarationResponse);

			AssertEquals("<H3>Accepted Declaration</H3><H3>CSV Electronic Declaration = HHAXSXPQA6NT963Y</H3>" +
					"<table border=\"0\"><tr><td>Acceptance:</td><td>&nbsp;&nbsp;</td><td>04-10-2021, 10:30:45</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Circuit:</td><td>&nbsp;&nbsp;</td><td><strong><font color=\"#64AF00\">GREEN</font></strong></td></tr></table>" +
					"<table border=\"0\"><tr><td>C.S.V.:</td><td>&nbsp;&nbsp;</td><td>9KJLVLPCWNKUV7N8</td></tr></table>" +
					"<br><table border=\"0\"><tr><td>Description:</td><td>&nbsp;&nbsp;</td><td>Documento Anexado</td></tr></table>", messageInterpretationText);
		}

		protected override bool ShoulddelPDFdelT2LDataBeTested => true;
		protected override bool ShouldCircuitDataBeTested => true;

		protected override T2LCommonMessagePrettyFormatter<T2LanexosV1Sal> GetFormatter(T2LanexosV1Sal response) => new T2LAnnexMessagePrettyFormatter(response);

		protected override T2LanexosV1Sal GetResponse(string responseCode = "", string responseDesc = "")
		{
			var response = new T2LanexosV1Sal();
			response.CodigoRespuesta = responseCode;
			response.DescripcionRespuesta = responseDesc;
			return response;
		}

		protected override T2LanexosV1Sal SetResponseData(ZString date, ZString hour, ZString reference, ZString csvClearance, ZString csvPdfT2l, ZString description, CircuitoTipo circuito = CircuitoTipo.V, bool hasCircuitAssigment = false)
		{
			var response = GetResponse();
			response.SegmentosDeServicio = new SegmentosDeServicioTipo()
			{
				IdentificadorMensaje = new SegmentosDeServicioTipoIdentificadorMensaje()
				{
					IdenTran = !date.IsEmpty && !hour.IsEmpty ? date + hour + "516039" : (string)ZString.Empty
				},
			};
			response.CsVdeDeclaracionElectronica = csvClearance;
			response.CsVdelPdFdelT2L = csvPdfT2l;
			response.DescripcionRespuesta = description;
			if (hasCircuitAssigment)
			{
				response.CircuitoAsignado = circuito;
				response.CircuitoAsignadoValueSpecified = true;
			}

			return response;
		}
	}
}
