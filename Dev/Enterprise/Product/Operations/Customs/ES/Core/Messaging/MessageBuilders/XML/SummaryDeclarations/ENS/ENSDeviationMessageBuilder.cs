using CargoWise.Customs.ES.MessageDefinitions.Version1.Adua.Internet.Es.Aeat.Dit.Adu.Aden.Enswsv5;
using CargoWise.Customs.ES.MessageDefinitions.Version1.COMPLEX_ICS;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TCL;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class ENSDeviationMessageBuilder : SummaryDeclarationsCommonMessageBuilder<IENSDeviationMessageDataProvider, Cc323A>
	{
		public ENSDeviationMessageBuilder(IENSDeviationMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override Cc323A GenerateXMLMessage()
		{
			var declaration = new Cc323A
			{
				MesTypMes20 = MessageTypes.Cc323A,

				Heahea = GetPopulatedHeader(),
				Cusoffentactoff700 = GetPopulatedCustomsOffice<Cusoffentactoff700Type>(provider.ActualEntryCustomsOffice),
				Cusofffent730 = GetPopulatedCustomsOffice<Cusofffent730Type10>(provider.FirstEntryCustomsOffice),
				Trareqdiv456 = GetPopulatedAddressInformationENS<Trareqdiv456Type>(provider.RequestingTrader),
				Impope200 = provider.ImportOperations.ConvertToCollection(GetPopulatedImportOperation)
			};

			PopulateENSGenericMessageData(declaration, provider);

			return declaration;
		}

		HeaheaType10 GetPopulatedHeader()
		{
			var header = provider.Header;
			return header == null ? null : new HeaheaType10
			{
				TraModAtBorHea76 = header.BorderTransportMode,
				CouCodOffFirEntDecHea100 = header.FirstEntryOfficeCountry,
				InfTypHea122 = header.InformationType,
				DivRefNumHea119 = header.DeviationReferenceNumber,
				UniIdeDivHea132 = header.TransportId,
				ExpDatArrHea701 = header.ExpectedArrivalDate.ToCustomsFormatDateString()
			};
		}

		Impope200Type GetPopulatedImportOperation(IENSDeviationImportOperation operation)
		{
			return new Impope200Type
			{
				DocRefNumImpope201 = operation.ReferenceNumber,
				Gooiteimp248 = operation.GoodsItems.ConvertToCollection(GetPopulatedGoodsItem)
			};

			Gooiteimp248Type GetPopulatedGoodsItem(ZString goodItemNumber)
			{
				return new Gooiteimp248Type
				{
					IteNumGiimp297 = goodItemNumber
				};
			}
		}
	}
}
