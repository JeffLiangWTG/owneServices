using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.AnulaPreH7V1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class CancelH7MessageBuilder : H7CommonMessageBuilder<ICancelH7MessageDataProvider, AnulaPreH7V1Ent>
	{
		public CancelH7MessageBuilder(ICancelH7MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override AnulaPreH7V1Ent GenerateXMLMessage()
		{
			return new AnulaPreH7V1Ent
			{
				Message = GetPopulatedMessage(),
				Declaration = new DeclarationTd()
				{
					DeclarationMrn = provider.DeclarationMRN
				}
			};
		}
	}
}
