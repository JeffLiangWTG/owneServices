using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ConsultaImportacionV2Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class DUAImportQueryMessageBuilder : ImportCommonMessageBuilder<IDUAImportQueryMessageDataProvider, ConsultaImportacionV2Ent>
	{
		public DUAImportQueryMessageBuilder(IDUAImportQueryMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ConsultaImportacionV2Ent GenerateXMLMessage()
		{
			return new ConsultaImportacionV2Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				NumeroDeReferencia = provider.MRN,
				DatosEnAtc = provider.RequestATCData
			};
		}
	}
}
