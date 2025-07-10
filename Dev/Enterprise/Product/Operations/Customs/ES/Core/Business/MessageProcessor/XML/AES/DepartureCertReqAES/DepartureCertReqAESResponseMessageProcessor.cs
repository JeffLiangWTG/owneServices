using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCCSEC_v514.CCCSECV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class DepartureCertReqAESResponseMessageProcessor : AESCommonResponseMessageProcessor<Cccsecv1Sal>
	{
		public DepartureCertReqAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export Exit Certificate Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCCCSECV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.RequestExportExitCertificate };

		protected override IMessagePrettyFormatter GetNewMessagePrettyFormatter(Cccsecv1Sal response, EDIMessage message, CusEntryHeader entryHeader) => new DepartureCertReqAESMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cccsecv1Sal response, EDIMessage message, CusEntryHeader entryHeader)
		{
			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				entryHeader.ZG_CSVExitCertificate = correctResponseData.CsvCertificadoSalida;
			}

			return ZString.Empty;
		}

		const string XsdSchemaNameCCCSECV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CCCSECV1Sal.xsd";
	}
}
