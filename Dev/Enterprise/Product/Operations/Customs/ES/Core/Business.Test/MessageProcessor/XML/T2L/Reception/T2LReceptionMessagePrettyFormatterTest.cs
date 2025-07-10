using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Testing
{
	class T2LReceptionMessagePrettyFormatterTest : T2LCommonMessagePrettyFormatterTest<T2LrecepcionV1Sal>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null response", () => new T2LReceptionMessagePrettyFormatter(null));
		}

		protected override T2LCommonMessagePrettyFormatter<T2LrecepcionV1Sal> GetFormatter(T2LrecepcionV1Sal response) => new T2LReceptionMessagePrettyFormatter(response);

		protected override T2LrecepcionV1Sal GetResponse(string responseCode = "", string responseDesc = "")
		{
			var response = new T2LrecepcionV1Sal();
			response.CodigoRespuesta = responseCode;
			response.DescripcionRespuesta = responseDesc;
			return response;
		}

		protected override T2LrecepcionV1Sal SetResponseData(ZString date, ZString hour, ZString reference, ZString csvClearance, ZString csvPdfT2l, ZString description, CircuitoTipo circuito = CircuitoTipo.V, bool hasCircuitAssigment = false)
		{
			var response = GetResponse();
			response.SegmentosDeServicio = new SegmentosDeServicioTipo()
			{
				IdentificadorMensaje = new SegmentosDeServicioTipoIdentificadorMensaje()
				{
					IdenTran = !date.IsEmpty && !hour.IsEmpty ? date + hour + "516039" : (string)ZString.Empty
				},
			};
			response.NumeroDeReferenciaDelT2L = reference;
			response.CsVdeDeclaracionElectronica = csvClearance;
			response.DescripcionRespuesta = description;

			return response;
		}
	}
}
