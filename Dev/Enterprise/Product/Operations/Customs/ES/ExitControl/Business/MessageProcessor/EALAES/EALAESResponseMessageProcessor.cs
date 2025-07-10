using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC507C_v514.CC507CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESResponseMessageProcessor : EALCommonResponseMessageProcessor<Cc507Cv1Sal, EALAESMessagePrettyFormatter>
	{
		public EALAESResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Export EAL Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC507CV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6 };

		protected override EALAESMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc507Cv1Sal response) => new EALAESMessagePrettyFormatter(response);

		protected override void ProcessAcceptedDeclaration(Cc507Cv1Sal response, EDIMessage message, CusExitReport report)
		{
			SetEntryStatusAndTriggerRequestIfNeeded(response, report, message);

			var correctResponseData = response.DatosRespuestaCorrecta;
			if (correctResponseData != null)
			{
				report.CER_DateTime = correctResponseData.FechaLlegada;

				var circuit = GetCircuitCodeFromText(correctResponseData.CircuitoLlegada);
				if (!circuit.IsEmpty)
				{
					CreateOrUpdateCusEntryNumber(report, message, correctResponseData.CsvLevanteSalida, circuit, correctResponseData.FechaLevanteSalida ?? ZDateTime.Empty);
				}
			}
		}

		void SetEntryStatusAndTriggerRequestIfNeeded(Cc507Cv1Sal response, CusExitReport report, EDIMessage message)
		{
			var responseCode = response.ControlRespuesta.CodigoRespuesta;
			if (responseCode == EALResponseCodeList.Codes.Clearance)
			{
				report.CER_Status = AESEntryStatusList.Codes.ReleasedForExit;
			}
			else if (responseCode == EALResponseCodeList.Codes.GoodsUnderCustomsControl)
			{
				report.CER_Status = AESEntryStatusList.Codes.ControlledForExit;

				TriggerInboxRequests(message, report);
			}
		}

		void TriggerInboxRequests(EDIMessage message, CusExitReport report)
		{
			var messageTypesList = InboxRequestMessageTypes;

			TriggerInboxRequest(report, message, messageTypesList);
		}

		ZString[] InboxRequestMessageTypes => new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification };

		const string XsdSchemaNameCC507CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.AES.Incoming.CC507CV1Sal.xsd";
	}
}
