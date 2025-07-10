using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionModificaV1Sal;

namespace Enterprise.Customs.ES.Business
{
	public class T2LExpeditionAmendmentMessagePrettyFormatter : T2LCommonMessagePrettyFormatter<T2LexpedicionModificaV1Sal>
	{
		public T2LExpeditionAmendmentMessagePrettyFormatter(T2LexpedicionModificaV1Sal response) : base(response)
		{
		}

		protected override void AppendExtraData(StringBuilder messageDetails)
		{
			AppendMRNDataIfNotEmpty(messageDetails, response.NumeroDeReferenciaDelT2L);
			AppendCSVdelPDFdelT2LDataIfNotEmpty(messageDetails, response.CsVdelPdFdelT2L);
		}
	}
}
