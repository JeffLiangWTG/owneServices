using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.EnvioDeDocumentosV1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CommonAnnex.TDV1;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Tag names")]
public class CommonAnnexMessageBuilder : XMLMessageBuilder<ICommonAnnexMessageDataProvider, EnvioDeDocumentosV1Ent>
{
	public CommonAnnexMessageBuilder(ICommonAnnexMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
	{
	}

	const string ReferenceTagName = "NumeroDeReferencia";
	const string AdminTagName = "Administracion";

	protected override EnvioDeDocumentosV1Ent GenerateXMLMessage() => GetPopulatedEnvioDeDocumentosV1Ent();

	EnvioDeDocumentosV1Ent GetPopulatedEnvioDeDocumentosV1Ent()
	{
		return new EnvioDeDocumentosV1Ent
		{
			SegmentosDeServicio = GetPopulatedSegmentosDeServicio(),
			Operacion = provider.Operation,
			InformacionAdicionalEnvio = new Collection<InfoAdicionalTd>
			{
				GetReferenceAddInfo(),
				GetAdminAddInfo(),
				GetRequestDispatchAddInfo()
			},
			Documento = GetPopulatedDocumento()
		};
	}

	SegmDeServicioTd GetPopulatedSegmentosDeServicio()
	{
		return new SegmDeServicioTd
		{
			Id = TransactionId,
			Fecha = DateOfCET,
			Hora = TimeOfCET,
			Test = null
		};
	}

	InfoAdicionalTd GetReferenceAddInfo()
	{
		return new InfoAdicionalTd
		{
			NombreEtiqueta = ReferenceTagName,
			Valor = provider.Reference
		};
	}

	InfoAdicionalTd GetAdminAddInfo()
	{
		if (!string.IsNullOrEmpty(provider.AdministrationCode))
		{
			return new InfoAdicionalTd
			{
				NombreEtiqueta = AdminTagName,
				Valor = provider.AdministrationCode
			};
		}

		return new InfoAdicionalTd { };
	}

	InfoAdicionalTd GetRequestDispatchAddInfo()
	{
		return new InfoAdicionalTd
		{
			NombreEtiqueta = provider.RequestDispatchTagName,
			Valor = provider.DispatchRequest
		};
	}

	DocumentoTd GetPopulatedDocumento()
	{
		var document = provider.Document;
		return document == null ? null : new DocumentoTd
		{
			Descripcion = document.Description,
			ReferenciaDelDocumento = document.ReferenceNumber,
			ImagenDelDocumento = document.Image,
			ExtensionDelDocumento = (ExtensionTipo)Enum.Parse(typeof(ExtensionTipo), document.Extension.SubstringSafe(0, 1) + document.Extension.SubstringSafe(1).ToLower())
		};
	}
}
