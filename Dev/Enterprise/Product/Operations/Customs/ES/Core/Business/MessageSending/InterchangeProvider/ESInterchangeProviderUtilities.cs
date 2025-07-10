using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public static class ESInterchangeProviderUtilities
	{
		public static ZString GetInterchangeReceiver(ZString messageType, bool isXT = false, bool isTest = false, bool isInboxXT = false)
		{
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.ImportAmendmentBox40:
				case DeclarationMessageTypeList.Codes.Box44Documents:
				case DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation:
				case DeclarationMessageTypeList.Codes.PendingSupportingDocuments:
				case DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration:
				case DeclarationMessageTypeList.Codes.ImportQuery:
				case DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration:
				case DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration:
				case DeclarationMessageTypeList.Codes.H7Cancellation:
				case DeclarationMessageTypeList.Codes.H7Declaration:
				case DeclarationMessageTypeList.Codes.H7Query:
				case DeclarationMessageTypeList.Codes.H7ReExport:
				case DeclarationMessageTypeList.Codes.G3RevocationOfGoods:
				case DeclarationMessageTypeList.Codes.G3DeclarationOfGoods:
				case DeclarationMessageTypeList.Codes.EnsAmendment:
				case DeclarationMessageTypeList.Codes.EnsDeviationRequest:
				case DeclarationMessageTypeList.Codes.EntrySummaryDeclaration:
				case DeclarationMessageTypeList.Codes.ExitSummaryDeclaration:
				case DeclarationMessageTypeList.Codes.T2lAnnex:
				case DeclarationMessageTypeList.Codes.T2lClearance:
				case DeclarationMessageTypeList.Codes.T2lExpedition:
				case DeclarationMessageTypeList.Codes.T2lExpeditionAmendment:
				case DeclarationMessageTypeList.Codes.T2lReception:
				case DeclarationMessageTypeList.Codes.T2lReceptionAmendment:
				case DeclarationMessageTypeList.Codes.ExportUcc6:
				case DeclarationMessageTypeList.Codes.ExportAmendmentUcc6:
				case DeclarationMessageTypeList.Codes.TypeXExportUcc6:
				case DeclarationMessageTypeList.Codes.ExportNotification:
				case DeclarationMessageTypeList.Codes.ExportCancellation:
				case DeclarationMessageTypeList.Codes.ExportAnnexes:
				case DeclarationMessageTypeList.Codes.ExportPreDeclaration:
				case DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6:
				case DeclarationMessageTypeList.Codes.ExportQuery:
				case DeclarationMessageTypeList.Codes.RequestExportExitCertificate:
				case DeclarationMessageTypeList.Codes.DvdH2:
				case DeclarationMessageTypeList.Codes.DvdH2Cancellation:
				case DeclarationMessageTypeList.Codes.DvdH2Query:
				case DeclarationMessageTypeList.Codes.TypeXDvdH2:
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods:
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification:
				case DeclarationMessageTypeList.Codes.Ncts5Departure:
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment:
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes:
				case DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation:
				case DeclarationMessageTypeList.Codes.Ncts5DepartureNotification:
				case DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration:
				case DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration:
				case DeclarationMessageTypeList.Codes.TransitNcts5Query:
				case DeclarationMessageTypeList.Codes.T2lRequestPous:
				case DeclarationMessageTypeList.Codes.T2lPresentationPous:
				case DeclarationMessageTypeList.Codes.T2lDocumentationPous:
				case DeclarationMessageTypeList.Codes.T2lQueryPous:
				case DeclarationMessageTypeList.Codes.T2lReceptionPous:
				case DeclarationMessageTypeList.Codes.G5v1Expedition:
				case DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment:
				case DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation:
				case DeclarationMessageTypeList.Codes.G5v1Reception:
				case DeclarationMessageTypeList.Codes.Box44DocumentsH1:
				case DeclarationMessageTypeList.Codes.ImportActivationH1:
				case DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1:
				case DeclarationMessageTypeList.Codes.ImportAnnexH1:
				case DeclarationMessageTypeList.Codes.ImportCompleteActiveH1:
				case DeclarationMessageTypeList.Codes.ImportCompletePendingActivationH1:
				case DeclarationMessageTypeList.Codes.ImportH1Query:
				case DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1:
				case DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1:
				case DeclarationMessageTypeList.Codes.ImportSimplifiedPendingActivationH1:
				case DeclarationMessageTypeList.Codes.PendingSupportingDocumentH1:
				case DeclarationMessageTypeList.Codes.PreDeclarationCancellationH1:
					return isXT ?
						isTest ? SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt : SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt
						: SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub;

				case DeclarationMessageTypeList.Codes.ArrivalAtExit:
				case DeclarationMessageTypeList.Codes.Export:
				case DeclarationMessageTypeList.Codes.ExportAmendment:
				case DeclarationMessageTypeList.Codes.TypeXExport:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotification:
				case DeclarationMessageTypeList.Codes.NctsUnloadingRemarks:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs:
				case DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb:
				case DeclarationMessageTypeList.Codes.NctsDeparture:
				case DeclarationMessageTypeList.Codes.NctsTir:
					return SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms;

				case DeclarationMessageTypeList.Codes.InBoxNotificationForExport:
				case DeclarationMessageTypeList.Codes.InBoxNotificationForImport:
				case DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2:
				case DeclarationMessageTypeList.Codes.ExportCceControlCommunication:
				case DeclarationMessageTypeList.Codes.ExportNonConformityCommunication:
				case DeclarationMessageTypeList.Codes.ExportInvalidationCommunication:
				case DeclarationMessageTypeList.Codes.ExportClearanceCommunication:
				case DeclarationMessageTypeList.Codes.ExportExitResultCommunication:
				case DeclarationMessageTypeList.Codes.ExportExitClearanceNotification:
				case DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification:
				case DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture:
				case DeclarationMessageTypeList.Codes.InboxNotificationNctsControls:
				case DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance:
				case DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation:
					return isInboxXT ?
						isTest ? SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt : SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt
						: SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub;

				case DeclarationMessageTypeList.Codes.EsDocumentRequest:
					return isXT && isTest
									? SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms
									: SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms;

				case DeclarationMessageTypeList.Codes.InboxPendingList:
					return isTest ? SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt : SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt;

				default:
					return ZString.Empty;
			}
		}

		public static XMLExtraData GetServiceAndOperation(ZString messageType)
		{
			var extraData = new XMLExtraData();
			switch (messageType)
			{
				case DeclarationMessageTypeList.Codes.ImportAmendmentBox40:
					extraData.Service = DeclarationServiceCodeList.Codes.Box40AmendmentImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.Box40AmendmentImport;
					break;
				case DeclarationMessageTypeList.Codes.Box44Documents:
					extraData.Service = DeclarationServiceCodeList.Codes.Box44DocumentsImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.Box44DocumentsImport;
					break;
				case DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation:
					extraData.Service = DeclarationServiceCodeList.Codes.CancelPreDuaImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.CancelPreDuaImport;
					break;
				case DeclarationMessageTypeList.Codes.PendingSupportingDocuments:
					extraData.Service = DeclarationServiceCodeList.Codes.PendingSupportingDocumentsImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.PendingSupportingDocumentsImport;
					break;
				case DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.CompletePreDuaImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.CompletePreDuaImport;
					break;
				case DeclarationMessageTypeList.Codes.ImportQuery:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportQuery;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportQuery;
					break;
				case DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.SimplifiedPreDuaImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.SimplifiedPreDuaImport;
					break;
				case DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.IncompletePreDuaImport;
					extraData.Operation = DeclarationOperationCodeList.Codes.IncompletePreDuaImport;
					break;
				case DeclarationMessageTypeList.Codes.H7Cancellation:
					extraData.Service = DeclarationServiceCodeList.Codes.H7Cancellation;
					extraData.Operation = DeclarationOperationCodeList.Codes.H7Cancellation;
					break;
				case DeclarationMessageTypeList.Codes.H7Declaration:
					extraData.Service = DeclarationServiceCodeList.Codes.H7Declaration;
					extraData.Operation = DeclarationOperationCodeList.Codes.H7Declaration;
					break;
				case DeclarationMessageTypeList.Codes.H7Query:
					extraData.Service = DeclarationServiceCodeList.Codes.H7Query;
					extraData.Operation = DeclarationOperationCodeList.Codes.H7Query;
					break;
				case DeclarationMessageTypeList.Codes.H7ReExport:
					extraData.Service = DeclarationServiceCodeList.Codes.H7ReExport;
					extraData.Operation = DeclarationOperationCodeList.Codes.H7ReExport;
					break;
				case DeclarationMessageTypeList.Codes.EnsAmendment:
					extraData.Service = DeclarationServiceCodeList.Codes.EnsAmendment;
					extraData.Operation = DeclarationOperationCodeList.Codes.EnsAmendment;
					break;
				case DeclarationMessageTypeList.Codes.EnsDeviationRequest:
					extraData.Service = DeclarationServiceCodeList.Codes.EnsDeviationRequest;
					extraData.Operation = DeclarationOperationCodeList.Codes.EnsDeviationRequest;
					break;
				case DeclarationMessageTypeList.Codes.EntrySummaryDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.EntrySummaryDeclaration;
					extraData.Operation = DeclarationOperationCodeList.Codes.EntrySummaryDeclaration;
					break;
				case DeclarationMessageTypeList.Codes.ExitSummaryDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.ExitSummaryDeclaration;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExitSummaryDeclaration;
					break;
				case DeclarationMessageTypeList.Codes.T2lAnnex:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lAnnex;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lAnnex;
					break;
				case DeclarationMessageTypeList.Codes.T2lClearance:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lClearance;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lClearance;
					break;
				case DeclarationMessageTypeList.Codes.T2lExpedition:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lExpedition;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lExpedition;
					break;
				case DeclarationMessageTypeList.Codes.T2lExpeditionAmendment:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lExpeditionAmendment;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lExpeditionAmendment;
					break;
				case DeclarationMessageTypeList.Codes.T2lReception:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lReception;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lReception;
					break;
				case DeclarationMessageTypeList.Codes.T2lReceptionAmendment:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lReceptionAmendment;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lReceptionAmendment;
					break;
				case DeclarationMessageTypeList.Codes.ExportUcc6:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportUcc6;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportUcc6;
					break;
				case DeclarationMessageTypeList.Codes.ExportAmendmentUcc6:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportAmendmentUcc6;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportAmendmentUcc6;
					break;
				case DeclarationMessageTypeList.Codes.TypeXExportUcc6:
					extraData.Service = DeclarationServiceCodeList.Codes.TypeXExportUcc6;
					extraData.Operation = DeclarationOperationCodeList.Codes.TypeXExportUcc6;
					break;
				case DeclarationMessageTypeList.Codes.ExportCancellation:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportCancellation;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportCancellation;
					break;
				case DeclarationMessageTypeList.Codes.ExportAnnexes:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportAnnexes;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportAnnexes;
					break;
				case DeclarationMessageTypeList.Codes.ExportPreDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportPreDeclaration;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportPreDeclaration;
					break;
				case DeclarationMessageTypeList.Codes.ExportNotification:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportNotification;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportNotification;
					break;
				case DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6:
					extraData.Service = DeclarationServiceCodeList.Codes.ArrivalAtExitUcc6;
					extraData.Operation = DeclarationOperationCodeList.Codes.ArrivalAtExitUcc6;
					break;
				case DeclarationMessageTypeList.Codes.ExportQuery:
					extraData.Service = DeclarationServiceCodeList.Codes.ExportQuery;
					extraData.Operation = DeclarationOperationCodeList.Codes.ExportQuery;
					break;
				case DeclarationMessageTypeList.Codes.RequestExportExitCertificate:
					extraData.Service = DeclarationServiceCodeList.Codes.RequestExportExitCertificate;
					extraData.Operation = DeclarationOperationCodeList.Codes.RequestExportExitCertificate;
					break;
				case DeclarationMessageTypeList.Codes.DvdH2:
					extraData.Service = DeclarationServiceCodeList.Codes.DvdH2;
					extraData.Operation = DeclarationOperationCodeList.Codes.DvdH2;
					break;
				case DeclarationMessageTypeList.Codes.DvdH2Cancellation:
					extraData.Service = DeclarationServiceCodeList.Codes.DvdH2Cancellation;
					extraData.Operation = DeclarationOperationCodeList.Codes.DvdH2Cancellation;
					break;
				case DeclarationMessageTypeList.Codes.DvdH2Query:
					extraData.Service = DeclarationServiceCodeList.Codes.DvdH2Query;
					extraData.Operation = DeclarationOperationCodeList.Codes.DvdH2Query;
					break;
				case DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2:
					extraData.Service = DeclarationServiceCodeList.Codes.InboxNotificationForDvdH2;
					extraData.Operation = DeclarationOperationCodeList.Codes.InboxNotificationForDvdH2;
					break;
				case DeclarationMessageTypeList.Codes.TypeXDvdH2:
					extraData.Service = DeclarationServiceCodeList.Codes.TypeXDvdH2;
					extraData.Operation = DeclarationOperationCodeList.Codes.TypeXDvdH2;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5ArrivalDownloadGoods;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5ArrivalDownloadGoods;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5ArrivalNotification;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5ArrivalNotification;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5Departure:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5Departure;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5Departure;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5DepartureAmendment;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5DepartureAmendment;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5DepartureAnnexes;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5DepartureAnnexes;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5DepartureCancellation;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5DepartureCancellation;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DepartureNotification:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5DepartureNotification;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5DepartureNotification;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5DeparturePreDeclaration;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5DeparturePreDeclaration;
					break;
				case DeclarationMessageTypeList.Codes.TransitNcts5Query:
					extraData.Service = DeclarationServiceCodeList.Codes.TransitNcts5Query;
					extraData.Operation = DeclarationOperationCodeList.Codes.TransitNcts5Query;
					break;
				case DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration:
					extraData.Service = DeclarationServiceCodeList.Codes.Ncts5IndirectDepartureRegistration;
					extraData.Operation = DeclarationOperationCodeList.Codes.Ncts5IndirectDepartureRegistration;
					break;
				case DeclarationMessageTypeList.Codes.T2lPresentationPous:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lPousPresentation;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lPousPresentation;
					break;
				case DeclarationMessageTypeList.Codes.T2lRequestPous:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lPousRequest;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lPousRequest;
					break;
				case DeclarationMessageTypeList.Codes.T2lDocumentationPous:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lPousDocumentation;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lPousDocumentation;
					break;
				case DeclarationMessageTypeList.Codes.T2lQueryPous:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lPousQuery;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lPousQuery;
					break;
				case DeclarationMessageTypeList.Codes.T2lReceptionPous:
					extraData.Service = DeclarationServiceCodeList.Codes.T2lPousReception;
					extraData.Operation = DeclarationOperationCodeList.Codes.T2lPousReception;
					break;
				case DeclarationMessageTypeList.Codes.G5v1Expedition:
					extraData.Service = DeclarationServiceCodeList.Codes.G5v1Expedition;
					extraData.Operation = DeclarationOperationCodeList.Codes.G5v1Expedition;
					break;
				case DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment:
					extraData.Service = DeclarationServiceCodeList.Codes.G5v1ExpeditionAmendment;
					extraData.Operation = DeclarationOperationCodeList.Codes.G5v1ExpeditionAmendment;
					break;
				case DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation:
					extraData.Service = DeclarationServiceCodeList.Codes.G5v1ExpeditionCancellation;
					extraData.Operation = DeclarationOperationCodeList.Codes.G5v1ExpeditionCancellation;
					break;
				case DeclarationMessageTypeList.Codes.G5v1Reception:
					extraData.Service = DeclarationServiceCodeList.Codes.G5v1Reception;
					extraData.Operation = DeclarationOperationCodeList.Codes.G5v1Reception;
					break;
				case DeclarationMessageTypeList.Codes.Box44DocumentsH1:
					extraData.Service = DeclarationServiceCodeList.Codes.Box44DocumentsH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.Box44DocumentsH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportActivationH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportActivationH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportActivationH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportAmendmentBox40H1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportAmendmentBox40H1;
					break;
				case DeclarationMessageTypeList.Codes.ImportAnnexH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportAnnexH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportAnnexH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportCompleteActiveH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportCompleteActiveH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportCompleteActiveH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportCompletePendingActivationH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportCompletePendingActivationH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportCompletePendingActivationH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportH1Query:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportH1Query;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportH1Query;
					break;
				case DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportIncompletePreDeclarationH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportIncompletePreDeclarationH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportSimplifiedActiveH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportSimplifiedActiveH1;
					break;
				case DeclarationMessageTypeList.Codes.ImportSimplifiedPendingActivationH1:
					extraData.Service = DeclarationServiceCodeList.Codes.ImportSimplifiedPendingActivationH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.ImportSimplifiedPendingActivationH1;
					break;
				case DeclarationMessageTypeList.Codes.PendingSupportingDocumentH1:
					extraData.Service = DeclarationServiceCodeList.Codes.PendingSupportingDocumentH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.PendingSupportingDocumentH1;
					break;
				case DeclarationMessageTypeList.Codes.PreDeclarationCancellationH1:
					extraData.Service = DeclarationServiceCodeList.Codes.PreDeclarationCancellationH1;
					extraData.Operation = DeclarationOperationCodeList.Codes.PreDeclarationCancellationH1;
					break;
				default:
					extraData.Service = ZString.Empty;
					extraData.Operation = ZString.Empty;
					break;
			}

			return extraData;
		}
	}
}
