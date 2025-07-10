using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ReexportacionH7V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ReexportH7MessageBuilder : H7CommonMessageBuilder<IReexportH7MessageDataProvider, ReexportacionH7V1Ent>
	{
		public ReexportH7MessageBuilder(IReexportH7MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ReexportacionH7V1Ent GenerateXMLMessage()
		{
			return new ReexportacionH7V1Ent
			{
				Message = GetPopulatedMessage(),
				Declaration = new DeclarationTd()
				{
					OperationCode = provider.OperationCode,
					Declarant = GetPopulatedDeclarant(),
					MrnH7 = provider.DeclarationMRNCodes.ConvertToStringCollection()
				}
			};
		}

		DeclarantReTd GetPopulatedDeclarant()
		{
			var declarant = provider.Declarant;
			return declarant == null ? null : new DeclarantReTd
			{
				IdentificationNumber = declarant.Id,
				Name = declarant.Name
			};
		}
	}
}
