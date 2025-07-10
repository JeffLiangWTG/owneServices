using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC015C_v515.CC015CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business;

public class DepartureNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cc015Cv1Sal, DepartureNCTSMessagePrettyFormatter>
{
	public DepartureNCTSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NCTS Departure Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC015CV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5Departure, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration };

	protected override DepartureNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc015Cv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new DepartureNCTSMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Cc015Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
	{
		ProcessAcceptedDepartureAndNotifGoods(message, nctsHeader, response.ControlRespuesta.CodigoRespuesta, response.DatosRespuestaCorrecta?.Mrn, response.DatosRespuestaCorrecta?.CircuitoExpedicion, response.DatosRespuestaCorrecta?.FechaAdmision, response.DatosRespuestaCorrecta?.CsVdeDat,
				response.DatosRespuestaCorrecta?.FechaLevante, response.DatosRespuestaCorrecta?.FechaLimiteLlegada);

		return ZString.Empty;
	}

	protected override void ProcessRejectedDeclaration(Cc015Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
	{
		UpdateGuaranteeTransactionsIfNeeded(message, Customs.Business.PermitTransactionStatusList.Codes.Deleted);

		NctsHeaderDocumentsSequenceNumberHelper.ResetSupportingDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader.MovementHeader, forceReset: true);
		NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader, AdditionalInfoSubTypeList.Codes.AdditionalInformation, forceReset: true);
		NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader, AdditionalInfoSubTypeList.Codes.AdditionalReference, forceReset: true);
		NctsHeaderDocumentsSequenceNumberHelper.ResetAdditionalDocumentsLineNoWhenPhase5(nctsHeader, nctsHeader, AdditionalInfoSubTypeList.Codes.TransportDocument, forceReset: true);

		if (nctsHeader.IsDepartureMovement && TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(nctsHeader.CountryCode))
		{
			var movementHeader = nctsHeader.MovementHeader;
			TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType);
		}
	}

	const string XsdSchemaNameCC015CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CC015CV1Sal.xsd";
}
