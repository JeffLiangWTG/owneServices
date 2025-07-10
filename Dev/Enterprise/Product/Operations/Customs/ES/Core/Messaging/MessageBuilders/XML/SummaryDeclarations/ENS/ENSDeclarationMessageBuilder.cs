using CargoWise.Customs.ES.MessageDefinitions.Version1.Adua.Internet.Es.Aeat.Dit.Adu.Aden.Enswsv5;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TCL;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class ENSDeclarationMessageBuilder : ENSCommonMessageBuilder<IENSDeclarationMessageDataProvider, Cc315A>
	{
		public ENSDeclarationMessageBuilder(IENSDeclarationMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override Cc315A GenerateXMLMessage()
		{
			var declaration = new Cc315A
			{
				MesTypMes20 = MessageTypes.Cc315A,

				Heahea = GetPopulatedHeader(),
				Gooitegds = provider.Lines.ConvertToCollection(GetPopulatedLine<GooitegdsType, Prodocdc2Type9, Connr2Type9, Idemeatragi970Type9, Pacgs2Type9>),
				Cusofflon = GetPopulatedCustomsOffice<CusofflonType9>(provider.LodgingCustomsOffice),
			};

			PopulateENSCommonMessageData<TrarepType9, Cusofffent730Type9, Tracarent601Type9>(declaration);

			return declaration;
		}

		HeaheaType9 GetPopulatedHeader()
		{
			HeaheaType9 declarationHeader = null;
			var header = provider.Header;
			if (header != null)
			{
				declarationHeader = new HeaheaType9
				{
					RefNumHea4 = header.ReferenceNumber,
					DecPlaHea394 = header.DeclarationPlace,
					DecPlaHea394Lng = header.DeclarationPlaceLanguage,
					DecDatTimHea114 = header.DeclarationDate.ToLongCustomsFormatDateTimeString()
				};

				PopulateENSHeaderCommon(declarationHeader, header);
			}

			return declarationHeader;
		}
	}
}
