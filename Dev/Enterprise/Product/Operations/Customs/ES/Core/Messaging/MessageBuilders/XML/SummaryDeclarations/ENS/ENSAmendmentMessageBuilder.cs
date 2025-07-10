using CargoWise.Customs.ES.MessageDefinitions.Version1.Adua.Internet.Es.Aeat.Dit.Adu.Aden.Enswsv5;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TCL;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class ENSAmendmentMessageBuilder : ENSCommonMessageBuilder<IENSAmendmentMessageDataProvider, Cc313A>
	{
		public ENSAmendmentMessageBuilder(IENSAmendmentMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override Cc313A GenerateXMLMessage()
		{
			var declaration = new Cc313A
			{
				MesTypMes20 = MessageTypes.Cc313A,

				Heahea = GetPopulatedHeader(),
				Gooitegds = provider.Lines.ConvertToCollection(GetPopulatedLine<GooitegdsType2, Prodocdc2Type2, Connr2Type2, Idemeatragi970Type2, Pacgs2Type>),
			};

			PopulateENSCommonMessageData<TrarepType8, Cusofffent730Type8, Tracarent601Type8>(declaration);

			return declaration;
		}

		HeaheaType2 GetPopulatedHeader()
		{
			HeaheaType2 declarationHeader = null;
			var header = provider.Header;
			if (header != null)
			{
				declarationHeader = new HeaheaType2
				{
					DocNumHea5 = header.MRN,
					AmdPlaHea598 = header.AmendmentPlace,
					AmdPlaHea598Lng = header.AmendmentPlaceLanguage,
					DatTimAmeHea113 = header.AmendmentDate.ToLongCustomsFormatDateTimeString()
				};

				PopulateENSHeaderCommon(declarationHeader, header);
			}

			return declarationHeader;
		}
	}
}
