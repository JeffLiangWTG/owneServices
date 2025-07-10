using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DeclaSimpliImporV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class DUASimplifiedImportMessageBuilder : ImportCommonMessageBuilder<IDUASimplifiedImportMessageDataProvider, DeclaSimpliImporV1Ent>
	{
		public DUASimplifiedImportMessageBuilder(IDUASimplifiedImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override DeclaSimpliImporV1Ent GenerateXMLMessage()
		{
			var declaration = new DeclaSimpliImporV1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};

			PopulateDUACommon(declaration, provider);
			PopulateDUAHeaderCommon(declaration, provider.Header, provider.MRN);

			return declaration;
		}

		PartidaTd GetPopulatedLine(IDUAImportCommonLine line)
		{
			var partida = new PartidaTd { };

			PopulateDUALineCommon(partida, line);

			return partida;
		}
	}
}
