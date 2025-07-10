using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ComunicaLevanteSalidaV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALInboxNotificationExitClearanceAESResponseMessageProcessor : EALAESCommonInboxNotificationResponseMessageProcessor<ComunicaLevanteSalidaV1Sal, EALInboxNotificationExitClearanceAESMessagePrettyFormatter>
	{
		public EALInboxNotificationExitClearanceAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Inbox Notification Export Exit Clearance Declaration Message Processor";

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification };

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameComunicaLevanteSalidaV1Sal;

		protected override EALInboxNotificationExitClearanceAESMessagePrettyFormatter GetNewMessagePrettyFormatter(ComunicaLevanteSalidaV1Sal response) => new EALInboxNotificationExitClearanceAESMessagePrettyFormatter(response);

		protected override void ProcessAcceptedDeclaration(ComunicaLevanteSalidaV1Sal response, EDIMessage message, CusExitReport report)
		{
			report.CER_Status = AESEntryStatusList.Codes.ReleasedForExit;

			var correctResponseData = response.DatosComunicacion;
			if (correctResponseData != null)
			{
				CreateOrUpdateCusEntryNumber(report, message, correctResponseData.CsvLevanteSalida, ZString.Empty, correctResponseData.FechaLevanteSalida);
			}
		}

		const string XsdSchemaNameComunicaLevanteSalidaV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.ComunicaLevanteSalidaV1Sal.xsd";
	}
}
