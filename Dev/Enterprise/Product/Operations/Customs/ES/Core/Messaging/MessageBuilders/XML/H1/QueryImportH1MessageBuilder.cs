using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ConsultaImportacionV2Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.ES_ConsultaImportacion;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[CodeAlive("This will be used in the next WI")]

public class QueryImportH1MessageBuilder : H1ImportCommonMessageBuilder<IQueryImportH1MessageDataProvider, ConsultaImportacionV2Ent>
{
	public QueryImportH1MessageBuilder(IQueryImportH1MessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	protected override ConsultaImportacionV2Ent GenerateXMLMessage()
	{
		return new ConsultaImportacionV2Ent()
		{
			Message = GetPopulatedMessage(),
			ConsultaCompleta = GetPopulatedConsultaCompleta()
		};
	}

	ConsultaImportacionType GetPopulatedConsultaCompleta()
	{
		return new ConsultaImportacionType()
		{
			Mrn = provider.DataProviderMRN.MRN,
			CustomsRegistrationNumber = provider.CustomsRegistrationNumber,
			Atc = provider.ATC
		};
	}
}
