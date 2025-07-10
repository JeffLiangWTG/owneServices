using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DocumentosSimplifiV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class DJPImportMessageBuilder : ImportCommonMessageBuilder<IDJPImportMessageDataProvider, DocumentosSimplifiV1Ent>
	{
		public DJPImportMessageBuilder(IDJPImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		const string DocumentIndicatorEmpty = "N";

		protected override DocumentosSimplifiV1Ent GenerateXMLMessage()
		{
			return new DocumentosSimplifiV1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				DocumentoJustificativoGlobal = provider.Documents.ConvertToCollection(GetPopulatedDocument),
				Declaracion = provider.Declarations.ConvertToCollection(GetPopulatedDeclaration)
			};
		}

		DocumentoTd GetPopulatedDocument(IDJPDocument documentProvider)
		{
			var documentTd = GetPopulatedDocumentCommon<DocumentoTd>(documentProvider);

			var documentProviderDate = documentProvider.Date;
			documentTd.C44Fecha = documentProviderDate.IsEmpty ? null : (string)documentProviderDate.ToCustomsFormatDateStringddMMyyyy();

			var documentProviderIndicator = documentProvider.Indicator;
			documentTd.C44IndicadorAcordeRegularizar = !documentProviderIndicator.IsEmpty ? documentProviderIndicator : (ZString)DocumentIndicatorEmpty;
			return documentTd;
		}

		DeclaracionTd GetPopulatedDeclaration(IDJPDeclaration declarationProvider)
		{
			return new DeclaracionTd()
			{
				NumeroReferenciaDua = declarationProvider.MRN,
				DocumentoJustificativoDua = declarationProvider.Documents.ConvertToCollection(GetPopulatedDocument),
				Partida = declarationProvider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}

		PartidaTd GetPopulatedLine(IDJPLine line)
		{
			return new PartidaTd
			{
				C32NumeroDePartida = line.LineNumber,
				DocumentoJustificativoPartida = line.Documents.ConvertToCollection(GetPopulatedDocument)
			};
		}
	}
}
