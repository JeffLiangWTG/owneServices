using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ModificacionPdcCas40V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class Box40AmendmentImportMessageBuilder : ImportCommonMessageBuilder<IBox40AmendmentImportMessageDataProvider, ModificacionPdcCas40V1Ent>
	{
		public Box40AmendmentImportMessageBuilder(IBox40AmendmentImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ModificacionPdcCas40V1Ent GenerateXMLMessage()
		{
			return new ModificacionPdcCas40V1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				NumeroReferenciaDua = provider.MRN,
				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}

		PartidaTd GetPopulatedLine(IBox40AmendmentLine line)
		{
			return new PartidaTd
			{
				C32NumeroDePartida = line.LineNumber,
				C40DocumentoCargoPrecedente = GetPopulatedDocumentoCargoPrecedente()
			};

			Cas402Td GetPopulatedDocumentoCargoPrecedente()
			{
				var docType = line.PrecedentDocumentType;
				var docClass = line.PrecedentDocumentClass;
				var docRef = line.PrecedentDocumentReference;
				return docType.IsEmpty && docClass.IsEmpty && docRef.IsEmpty ? null : new Cas402Td
				{
					C40TipoDocumento = docType,
					C40ClaseDocumento = docClass,
					C40ReferenciaDocumento = docRef
				};
			}
		}
	}
}
