using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC170C_v515.CC170CV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business;

public class NotifGoodsNCTSResponseMessageProcessor : NCTS5CommonResponseMessageProcessor<Cc170Cv1Sal, NotifGoodsNCTSMessagePrettyFormatter>
{
	public NotifGoodsNCTSResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"NCTS Notification Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameCC170CV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { DeclarationMessageTypeList.Codes.Ncts5DepartureNotification };

	protected override ZBool SetPhaseStatusTo015 => true;

	protected override NotifGoodsNCTSMessagePrettyFormatter GetNewMessagePrettyFormatter(Cc170Cv1Sal response, EDIMessage message, NctsHeader nctsHeader) => new NotifGoodsNCTSMessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(Cc170Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
	{
		ProcessAcceptedDepartureAndNotifGoods(message, nctsHeader, response.ControlRespuesta.CodigoRespuesta, response.DatosRespuestaCorrecta?.Mrn, response.DatosRespuestaCorrecta?.CircuitoExpedicion, response.DatosRespuestaCorrecta?.FechaAdmision, response.DatosRespuestaCorrecta?.CsVdeDat,
				response.DatosRespuestaCorrecta?.FechaLevante, response.DatosRespuestaCorrecta?.FechaLimiteLlegada);

		return ZString.Empty;
	}

	protected override void ProcessRejectedDeclaration(Cc170Cv1Sal response, EDIMessage message, NctsHeader nctsHeader)
	{
		UpdateGuaranteeTransactionsIfNeeded(message, Customs.Business.PermitTransactionStatusList.Codes.Deleted);

		if (nctsHeader.IsDepartureMovement && EU.Business.TemporaryStorageHelper.IsTemporaryStorageRegisterEnabled(nctsHeader.CountryCode))
		{
			var movementHeader = nctsHeader.MovementHeader;
			EU.Business.TemporaryStorageHelper.CancelPendingRegLineTransactions(message.Factory, movementHeader.TemporaryStorageTransactionInternalReferenceNumber, movementHeader.TemporaryStorageTransactionInternalReferenceType);
		}
	}

	const string XsdSchemaNameCC170CV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.Incoming.CC170CV1Sal.xsd";
}
