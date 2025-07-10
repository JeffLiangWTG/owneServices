using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC007C_v515.CC007CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class ArrivalNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cc007Cv1Sal, ArrivalNCTSMessagePrettyFormatter>
	{
		public ArrivalNCTSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"NCTS Arrival Declaration Message Processor";

		protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC007CV1Sal;

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification };

		protected override ArrivalNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc007Cv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new ArrivalNCTSMessagePrettyFormatter(response);

		protected override ZString ProcessAcceptedDeclaration(Cc007Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
		{
			if (!nctsHeader.IsArrivalMovement)
			{
				throw new InvalidOperationException(SetNctsMessageFailedLogDescription(nctsHeader, "Arrival"));
			}

			SetArrivalStatus(nctsHeader, response);
			SetEntryNumbers(message, nctsHeader, response.DatosRespuestaCorrecta?.Mrn, response.DatosRespuestaCorrecta?.CircuitoRecepcion, response.DatosRespuestaCorrecta?.FechaHoraRecepcion, ZString.Empty, response.ControlRespuesta?.CodigoRespuesta, null, null, response.DatosRespuestaCorrecta?.NumeroSumariaRecepcionG4Ultimacion);

			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = response.DatosRespuestaCorrecta?.FechaHoraRecepcion.ConvertToZDateTime().ToDateTimeOffset(nctsHeader.Branch.HomePort) ?? ZDateTimeOffset.Empty;

			if (nctsHeader.ArrivalMovementHeader.BM_CustomsStatus == ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease
				&& response.DatosRespuestaCorrecta?.CircuitoRecepcion == MessageFunctionCodeList.Codes.GreenCircuitText)
			{
				SetReleaseDate(response.DatosRespuestaCorrecta?.FechaHoraRecepcion, nctsHeader);
			}

			return ZString.Empty;
		}

		void SetArrivalStatus(NctsHeader nctsHeader, Cc007Cv1Sal response)
		{
			if (response.ControlRespuesta.CodigoRespuesta != ResponseCodeS)
			{
				SetArrivalStatus(nctsHeader, response.DatosRespuestaCorrecta?.CircuitoRecepcion, response.DatosRespuestaCorrecta?.Estado);
			}
		}

		void SetArrivalStatus(NctsHeader nctsHeader, ZString circuit, ZString status)
		{
			if (circuit == MessageFunctionCodeList.Codes.GreenCircuitText)
			{
				SetArrivalStatusCommon(nctsHeader, status, ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted);
			}
			else if (circuit == MessageFunctionCodeList.Codes.OrangeCircuitText || circuit == MessageFunctionCodeList.Codes.RedCircuitText)
			{
				nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = ESNCTS5ArrivalCustomsStatusList.Codes.DecisionToControl;
			}
		}

		const string XsdSchemaNameCC007CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CC007CV1Sal.xsd";
	}
}
