using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LanexosV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.TD;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class AnnexMessageBuilder : T2LCommonMessageBuilder<IAnnexMessageDataProvider, T2LanexosV1Ent>
	{
		public AnnexMessageBuilder(IAnnexMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override T2LanexosV1Ent GenerateXMLMessage()
		{
			return new T2LanexosV1Ent
			{
				SegmentosDeServicio = GetPopulatedSegmentosDeServicio<SegmentosDeServicioTipo>(),
				NumeroDeReferenciaDelT2L = provider.Header.T2LReferenceNumber,
				Declarante = GetPopulatedAddressInformationDeclarantCommon<DeclaranteTipo>(provider.Header.Declarant),
				IndicadorFinDeAnexado = provider.Header.FinalAnnexIndicator ? IndicadorSiNoTipo.S : IndicadorSiNoTipo.N,
				DocumentoAnexo = GetPopulatedDocumento()
			};
		}

		DocumentoTipo GetPopulatedDocumento()
		{
			var document = provider.Document;
			return document == null ? null : new DocumentoTipo
			{
				Descripcion = document.Description,
				ReferenciaDelDocumento = document.ReferenceNumber,
				ImagenDelDocumento = document.Image,
				ExtensionDelDocumento = (ExtensionTipo)Enum.Parse(typeof(ExtensionTipo), document.Extension.SubstringSafe(0, 1) + document.Extension.SubstringSafe(1).ToLower())
			};
		}
	}
}
