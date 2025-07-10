using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportDocCas44PendV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[CodeAlive("This class will be used in next Work Items")]
	public class Box44ImportMessageBuilder : ImportCommonMessageBuilder<IBox44ImportMessageDataProvider, ImportDocCas44PendV1Ent>
	{
		public Box44ImportMessageBuilder(IBox44ImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ImportDocCas44PendV1Ent GenerateXMLMessage()
		{
			return new ImportDocCas44PendV1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				NumeroDeReferencia = provider.MRN,
				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}

		PartidaTd GetPopulatedLine(IBox44Line line)
		{
			return new PartidaTd
			{
				C32NumeroDePartida = line.LineNumber,
				C44DocumentosYCertificados = line.DocumentsAndCertificates.ConvertToCollection(GetPopulatedDocAndCert)
			};
		}
	}
}
