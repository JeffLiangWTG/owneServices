using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class CancelDVDMessageBuilder : DVDCommonMessageBuilder<ICancelDVDMessageDataProvider, AnulaPdcVinculacionV1Ent>
	{
		public CancelDVDMessageBuilder(ICancelDVDMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override AnulaPdcVinculacionV1Ent GenerateXMLMessage()
		{
			return new AnulaPdcVinculacionV1Ent()
			{
				Mensaje = GetPopulatedCommonMessage<TdMensaje>()
			};
		}
	}
}
