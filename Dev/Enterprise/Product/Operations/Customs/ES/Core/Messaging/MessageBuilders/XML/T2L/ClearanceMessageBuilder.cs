using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LdatadoV2Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ClearanceMessageBuilder : T2LCommonMessageBuilder<IClearanceMessageDataProvider, T2LdatadoV2Ent>
	{
		public ClearanceMessageBuilder(IClearanceMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override T2LdatadoV2Ent GenerateXMLMessage()
		{
			return new T2LdatadoV2Ent
			{
				SegmentosDeServicio = GetPopulatedSegmentosDeServicio<SegmentosDeServicioTipo>(),
				AduanaDeRecepcion = provider.Header.ReceptionCustomsOffice,
				NumeroDeReferenciaDelT2LdeCargo = provider.Header.ReceptionT2LReference,
				Declarante = GetPopulatedAddressInformationDeclarantCommon<DeclaranteTipo>(provider.Header.Declarant),
				UbicacionDeLaMercancia = provider.Header.GoodsLocation,
				NumeroDePartidasDeOrden = provider.Header.TotalLinesNum,
				IndicadorDeContenedores = provider.Header.ContainersIndicator ? IndicadorContenedoresSiNoTipo.Item1 : IndicadorContenedoresSiNoTipo.Item0,
				CorreoElectronico = GetPopulatedCommunications(provider.Header.Communications),
				PartidasDeOrdenAutorizadas = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}

		PartidasDeOrdenTipo GetPopulatedLine(IClearanceLine line)
		{
			return line == null ? null : new PartidasDeOrdenTipo
			{
				NumeroDeOrdenDeLaPartida = line.LineNumber,
				SumariaDeCargo = line.SummaryDeclaration,
				PartidaDeCargoDelT2L = line.LineNumberReferenced,
				MasaBrutaaDatarEnKg = line.GrossWeightInKG,
				NumeroDeBultosaDatar = line.PackageQty,
				Contenedores = line.Containers.ConvertToStringCollection()
			};
		}
	}
}
