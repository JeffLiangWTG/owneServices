using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class CANPreDUAImportMessageBuilder : ImportCommonMessageBuilder<ICANPreDUAImportMessageDataProvider, AnulaImportacionV1Ent>
	{
		public CANPreDUAImportMessageBuilder(ICANPreDUAImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override AnulaImportacionV1Ent GenerateXMLMessage()
		{
			return new AnulaImportacionV1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				NumeroDeReferencia = provider.MRN
			};
		}
	}
}
