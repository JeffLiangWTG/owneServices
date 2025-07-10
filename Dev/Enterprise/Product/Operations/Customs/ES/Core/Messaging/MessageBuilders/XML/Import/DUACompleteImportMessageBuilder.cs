using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportacionCompletaV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class DUACompleteImportMessageBuilder : ImportCommonMessageBuilder<IDUACompleteImportMessageDataProvider, ImportacionCompletaV1Ent>
	{
		public DUACompleteImportMessageBuilder(IDUACompleteImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ImportacionCompletaV1Ent GenerateXMLMessage()
		{
			var declaration = new ImportacionCompletaV1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				FechaDelProcedimiento = provider.ProcedureDate.IsEmpty ? null : (string)provider.ProcedureDate.ToCustomsFormatDateString(),

				C02Exportador = GetPopulatedExportador(),
				C17APaisDestino = provider.Header.DestinationCountry,
				C17BProvinciaIslaDestino = provider.Header.DestinationState,
				C181IdentMedioTransporteLlegada = provider.Header.ArrivalTransportId,
				C20CondicionesDeEntrega = GetPopulatedCondicionesDeEntrega(),
				C21PaisMedioTransporteFrontera = provider.Header.FrontierTransportCountry,
				InvoiceAmount = provider.Header.InvoiceAmount.Truncate(MaxDecimals3),
				C24NaturalezaTransaccion = provider.Header.TransactionNature,
				C25ModoTransporteFrontera = provider.Header.FrontierTransportMode,
				C26ModoTransporteInterior = provider.Header.InteriorTransportMode,
				C29AduanaDeEntrada = provider.Header.CustomsOfficeOfEntry,
				C49IdentificacionDeposito = provider.Header.DepositId,

				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};

			PopulateDUACommon(declaration, provider);
			PopulateDUAHeaderCommon(declaration, provider.Header, provider.MRN);

			return declaration;
		}

		C02ExportadorTd GetPopulatedExportador()
		{
			var exporter = provider.Header.Exporter;
			var declaredExporter = GetPopulatedAddressInformationCommon<C02ExportadorTd>(exporter, true);
			if (declaredExporter != null)
			{
				declaredExporter.C02ExportadorProcSimplificado = exporter.SimplifiedProcedureType;
			}
			return declaredExporter;
		}

		C20CondicionesDeEntregaTd GetPopulatedCondicionesDeEntrega()
		{
			var deliveryConditions = provider.Header.DeliveryConditions;
			return deliveryConditions == null ? null : new C20CondicionesDeEntregaTd
			{
				C201CondicionesEntregaCodigo = deliveryConditions.Code,
				C202CondicionesEntregaNombre = deliveryConditions.Place,
				C203CondicionesEntregaZona = deliveryConditions.ZoneIndicator
			};
		}

		PartidaTd GetPopulatedLine(IDUACompleteImportLine line)
		{
			var partida = new PartidaTd
			{
				C33CodigoProductoRea = line.REACode,
				PositiveAdjustment = line.PositiveAdjustment,
				NegativeAdjustment = line.NegativeAdjustment,
				C46ValorEstadistico = line.StatisticalValue.Truncate(MaxDecimals2)
			};

			PopulateDUALineCommon(partida, line);

			return partida;
		}
	}
}
