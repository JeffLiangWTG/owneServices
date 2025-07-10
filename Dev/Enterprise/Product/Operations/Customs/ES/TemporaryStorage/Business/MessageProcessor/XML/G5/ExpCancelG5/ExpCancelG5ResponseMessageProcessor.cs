using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpCancelV1Sal;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class ExpCancelG5ResponseMessageProcessor : G5CommonResponseMessageProcessor<G5ExpCancelV1Sal, ExpCancelG5MessagePrettyFormatter>
{
	public ExpCancelG5ResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver = null) : base(logger, eDocsSaver)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"G5 Expedition Cancellation Declaration Message Processor";

	protected override ZString XsdSchemaEmbeddedResourceName => XsdSchemaNameG5ExpCancelV1Sal;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCoreES => new ZString[] { Messaging.DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation };

	protected override ExpCancelG5MessagePrettyFormatter GetNewMessagePrettyFormatter(G5ExpCancelV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader) => new ExpCancelG5MessagePrettyFormatter(response);

	protected override ZString ProcessAcceptedDeclaration(G5ExpCancelV1Sal response, EDIMessage message, TemporaryStorageHeader temporaryStorageHeader)
	{
		temporaryStorageHeader.CustomsStatus = EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Cancelled;

		var cancelationDate = DateTime.ParseExact(response.EnvelopeG5.PreparationDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds, CultureInfo.InvariantCulture);
		temporaryStorageHeader.AddNewGuaranteeTransactionForG5V1ExpeditionCancel(cancelationDate);

		temporaryStorageHeader.CustomsStatusDate = cancelationDate;

		EU.Business.TemporaryStorageHelper.ManageTemporaryStorageCancelationWhenProcessResponse(temporaryStorageHeader.Factory, temporaryStorageHeader.CountryCode,
			temporaryStorageHeader.GoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty, temporaryStorageHeader.TemporaryStorageTransactionInternalReferenceNumber, temporaryStorageHeader.JobNumber, temporaryStorageHeader.TemporaryStorageTransactionInternalReferenceType,
			temporaryStorageHeader.MRN, temporaryStorageHeader.TemporaryStorageTransactionCommentPrefix, PrefixCanceledComment, cancelationDate);

		return ZString.Empty;
	}

	const string XsdSchemaNameG5ExpCancelV1Sal = "CargoWise.Customs.ES.MessageDefinitions.Version1.G5.Incoming.G5ExpCancelV1Sal.xsd";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string PrefixCanceledComment = "G5X:";
}
