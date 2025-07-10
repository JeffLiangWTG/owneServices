using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LanexosV1Sal;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;

namespace Enterprise.Customs.ES.Business
{
	public class T2LAnnexMessagePrettyFormatter : T2LCommonMessagePrettyFormatter<T2LanexosV1Sal>
	{
		public T2LAnnexMessagePrettyFormatter(T2LanexosV1Sal response) : base(response)
		{
		}

		protected override void AppendExtraData(StringBuilder messageDetails)
		{
			AppendCircuitIfNotEmpty(messageDetails, GetCircuit(response.CircuitoAsignado ?? CircuitoTipo.R, response.CircuitoAsignadoValueSpecified));
			AppendCSVdelPDFdelT2LDataIfNotEmpty(messageDetails, response.CsVdelPdFdelT2L, !response.CircuitoAsignadoValueSpecified);
		}
	}
}
