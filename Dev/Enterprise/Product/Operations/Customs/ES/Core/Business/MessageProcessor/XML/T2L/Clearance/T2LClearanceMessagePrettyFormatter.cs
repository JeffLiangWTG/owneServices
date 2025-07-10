using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LdatadoV2Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;

namespace Enterprise.Customs.ES.Business
{
	public class T2LClearanceMessagePrettyFormatter : T2LCommonMessagePrettyFormatter<T2LdatadoV2Sal>
	{
		public T2LClearanceMessagePrettyFormatter(T2LdatadoV2Sal response) : base(response)
		{
		}

		protected override void AppendExtraData(StringBuilder messageDetails)
		{
			AppendMRNDataIfNotEmpty(messageDetails, response.NumeroDeReferenciaDelJec);
			AppendCircuitIfNotEmpty(messageDetails, GetCircuit(response.CircuitoAsignado ?? CircuitoTipo.R, response.CircuitoAsignadoValueSpecified));
			AppendCSVdelPDFdelT2LDataIfNotEmpty(messageDetails, response.CsVdelJustificanteDeLevante, !response.CircuitoAsignadoValueSpecified);
		}
	}
}
