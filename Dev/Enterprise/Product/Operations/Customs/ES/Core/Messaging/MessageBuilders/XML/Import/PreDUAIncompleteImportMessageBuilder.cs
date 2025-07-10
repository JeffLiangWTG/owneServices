using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Adua.Internet.Es.Aeat.Advu.Jdit.Ws;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class PreDUAIncompleteImportMessageBuilder : ImportCommonMessageBuilder<IPreDUAIncompleteImportMessageDataProvider, PreDeclaIncompletaV1Ent>
	{
		public PreDUAIncompleteImportMessageBuilder(IPreDUAIncompleteImportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override PreDeclaIncompletaV1Ent GenerateXMLMessage()
		{
			var declaration = new PreDeclaIncompletaV1Ent
			{
				SegmentosDeServicio = GetPopulatedServiceSegment<SegmDeServicioTd>(),
				Operacion = GetPopulatedOperacion(),
				NumeroDeReferencia = provider.MRN,
				CAaduana = provider.Header.CustomsOffice,
				Partida = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};

			PopulateImportHeaderCommon<Cas08ImportadorTd, Cas14DeclaranteTd>(declaration, provider.Header);

			return declaration;
		}

		OperacionTd GetPopulatedOperacion()
		{
			return provider.MRN.IsEmpty ? OperacionTd.A : OperacionTd.M;
		}

		PartidaTd GetPopulatedLine(IImportCommonLine line)
		{
			var partida = new PartidaTd
			{
				C371RegimenAduanero = GetPopulatedRegimen()
			};

			PopulateImportLineCommon(partida, line);

			return partida;

			Cas371RegTd GetPopulatedRegimen()
			{
				var requestedCPC = line.RequestedCPC;
				var previousCPC = line.PreviousCPC;
				return requestedCPC.IsEmpty && previousCPC.IsEmpty ? null : new Cas371RegTd
				{
					C371RegimenSolicitado = requestedCPC,
					C371RegimenPrecedente = previousCPC
				};
			}
		}
	}
}
