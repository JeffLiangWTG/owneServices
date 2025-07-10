using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DeclaComplemVinculV2Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ComplXDVDMessageBuilder : DVDCommonMessageBuilder<IComplXDVDMessageDataProvider, DeclaComplemVinculV2Ent>
	{
		public ComplXDVDMessageBuilder(IComplXDVDMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override DeclaComplemVinculV2Ent GenerateXMLMessage()
		{
			return new DeclaComplemVinculV2Ent()
			{
				SegmentosDeServicio = GetPopulatedServiceSegment(),
				NumeroReferenciaDvd = provider.MRN,
				C06TotalBultos = provider.TotalPackages,
				TotalMasaBruta = provider.TotalGrossMass,
				C14Declarante = GetPopulatedDeclarantAndRepresentative(provider.DeclarantAndRepresentative),
				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}
		protected override ZString RemoveExtraDataFromUnsignedMessageText(ZString unsignedMessageText) => unsignedMessageText.Replace(UnwantedXMLNSText, ZString.Empty);

		SegmDeServicioTd GetPopulatedServiceSegment()
		{
			var serviceSegment = GetPopulatedCommonServiceSegment<SegmDeServicioTd>();
			if (serviceSegment != null)
			{
				serviceSegment.Test = string.Empty;
			}
			return serviceSegment;
		}

		PartidaTd GetPopulatedLine(IComplXDVDLine line)
		{
			return new PartidaTd
			{
				C32NumeroDePartida = line.LineNumber,
				C35MasaBrutaEnKg = line.GrossMassKg,
				C38MasaNetaEnKg = line.NetMassKg,
				C41UnidadesSuplementarias = GetPopulatedSupplementaryUnits(line),
				C31UnidadesDeMedidaDeposito = GetPopulatedUnitOfMeasure(line),
				C31EmpaquetamientoInterno = line.Packages.ConvertToCollection(GetPopulatedCommonPackage<C31EmpaquetamientoInternoTd>),
				C31Vehiculos = line.Vehicles.ConvertToCollection(GetPopulatedCommonVehicle<C31VehiculosTd>)
			};
		}

		C14DeclaranteV2Td GetPopulatedDeclarantAndRepresentative(IComplXDVDDeclarantAndRepresentative declarant)
		{
			return declarant == null ? null : new C14DeclaranteV2Td
			{
				C14DeclaranteNid = declarant.Declarant?.Id ?? ZString.Empty,
				C14DeclaranteRazonSocial = declarant.Declarant?.Name ?? ZString.Empty,
				C14RepresentanteCaunid = declarant.Representative?.Id ?? ZString.Empty,
				C14RepresentanteCauRazonSocial = declarant.Representative?.Name ?? ZString.Empty,
				C14DeclaranteTipoAutorizaDespacho = declarant.RepresentativeTypeAuthorization
			};
		}

		C31UnidadesDeMedidaDepositoV2Td GetPopulatedUnitOfMeasure(IComplXDVDLine line)
		{
			var uomEU = line.DepositUnitOfMeasureCodeEU;
			var quantity = line.DepositUnitOfMeasureQuantity;

			return uomEU.IsEmpty && quantity.IsEmpty ? null : new C31UnidadesDeMedidaDepositoV2Td
			{
				C31UnidadesDeMedidaDepositoCodigoUe = uomEU,
				C31UnidadesDeMedidaDepositoNumero = quantity
			};
		}

		Cas41V2Td GetPopulatedSupplementaryUnits(IComplXDVDLine line)
		{
			var quantity = line.SupplementaryQuantity;

			return quantity.IsEmpty ? null : new Cas41V2Td
			{
				C41UnidadesNumero = quantity
			};
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		const string UnwantedXMLNSText = @" xmlns=""https://www2.agenciatributaria.gob.es/ADUA/internet/es/aeat/addv/jdit/ws/VinculaTiposDeDatos.xsd""";
	}
}
