using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionV2Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;

namespace Enterprise.Customs.ES.Business
{
	public class T2LExpeditionMessagePrettyFormatter : T2LCommonMessagePrettyFormatter<T2LexpedicionV2Sal>
	{
		public T2LExpeditionMessagePrettyFormatter(T2LexpedicionV2Sal response) : base(response)
		{
		}

		protected override void AppendExtraData(StringBuilder messageDetails)
		{
			AppendMRNDataIfNotEmpty(messageDetails, response.NumeroDeReferenciaDelT2L);
			AppendCircuitIfNotEmpty(messageDetails, GetCircuit(response.CircuitoAsignado ?? CircuitoTipo.R, response.CircuitoAsignadoValueSpecified));
			AppendCSVdelPDFdelT2LDataIfNotEmpty(messageDetails, response.CsVdelPdFdelT2L, !response.CircuitoAsignadoValueSpecified);
		}
	}
}
