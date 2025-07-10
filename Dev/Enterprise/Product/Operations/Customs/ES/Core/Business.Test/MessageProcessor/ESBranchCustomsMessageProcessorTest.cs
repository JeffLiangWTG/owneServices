using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Testing;

class ESBranchCustomsMessageProcessorTest : TestCaseWithFactory
{
	public void TestMessageProcessorProcessesMessage_EHub()
	{
		AssertMessageProcessorProcessesMessage(EDIInterchange.TransportType.eHub, false);
	}

	public void TestMessageProcessorProcessesMessage_DirectxT()
	{
		AssertMessageProcessorProcessesMessage(EDIInterchange.TransportType.xT, true);
	}

	void AssertMessageProcessorProcessesMessage(ZString interchangeTransportType, bool isDirectxT)
	{
		var exportMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Export, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var exportAmendmentMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.ExportAmendment, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var complXExportMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.TypeXExport, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importPreSadMessage = GetNewMessagePDICorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LReceptionMessage = GetNewMessageT2LReceptionCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LReceptionAmendmentMessage = GetNewMessageT2LReceptionAmendmentCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LExpeditionMessage = GetNewMessageT2LExpeditionCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LExpeditionAmendmentMessage = GetNewMessageT2LExpeditionAmendmentCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LClearanceMessage = GetNewMessageT2LClearanceCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var nctsTIRMessage = GetNewMessageNctsTIRCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCT00000001", interchangeTransportType);
		var canPreDUAImportMessage = GetNewMessageCANPreDUAImportCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var djpImportMessage = GetNewMessageDJPImportCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importSimplifiedMessage = GetNewMessagePDSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importCompleteMessage = GetNewMessagePDCCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var box40AmendmentImportMessage = GetNewMessageBox40AmendmentImportCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importQueryMessage = GetNewMessageImportQueryCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importNotificationMessage = GetNewMessageImportNotifCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var exportNotificationMessage = GetNewMessageExportNotifCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var nctsDepartureMessage = GetNewMessageNctsDepartureCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var nctsArrivalAVIMessage = GetNewMessageNctsArrivalAVICorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalOBSMessage = GetNewMessageNctsArrivalOBSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalAVOMessage = GetNewMessageNctsArrivalAVOCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalTNAMessage = GetNewMessageNctsArrivalTNACorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalTAOMessage = GetNewMessageNctsArrivalTAOCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalTNNMessage = GetNewMessageNctsArrivalTNNCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000002", interchangeTransportType);
		var arrivalAtExitMessage = GetNewMessageArrivalAtExit(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType);
		var inboxNotificationNCTSDepartureclearance = GetNewMessageInboxNotificationNCTSDepartureclearance(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var inboxNotificationNCTSDepartureControl = GetNewMessageInboxNotificationNCTSDepartureControl(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var aesGoodsNotificationMessage = GetNewMessageAESGoodsNotification(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var customsServiceErrorEntryHeaderMessage = GetNewMessageCustomsServiceErrorCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var customsServiceErrorNctsHeaderMessage = GetNewMessageCustomsServiceErrorCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var customsServiceErrorExitDetailMessage = GetNewMessageCustomsServiceErrorCorrectForCusExitDetail(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType);
		var customsServiceErrorExitReportMessage = GetNewMessageCustomsServiceErrorCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var customsServiceErrorUniversalEventEntryHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventNctsHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventExitDetailMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitDetail(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventExitReportMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventBadrequestEntryHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var customsServiceErrorUniversalEventBadrequestNctsHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var customsServiceErrorUniversalEventBadrequestExitDetailMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitDetail(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var customsServiceErrorUniversalEventBadrequestExitReportMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var documentCaptureEntryHeaderMessage = GetNewMessageDocumentCaptureCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var documentCaptureNctsHeaderMessage = GetNewMessageDocumentCaptureCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var importClearanceEmailMessage = GetNewMessageImportClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "20ES00999930006184");
		var nctsClearanceEmailMessage = GetNewMessageNctsClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "20ES00999830001288");
		var t2lClearanceEmailMessage = GetNewMessageT2LClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "21ES009999L0002748", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail);
		var t2cClearanceEmailMessage = GetNewMessageT2LClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail);
		var exportClearanceEmailMessage = GetNewMessageExportClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "21ES00280120889150");
		var g5ClearanceEmailMessage = GetNewMessageG5ClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "25ESG5G000000749Y0");
		var declarationAESMessage = GetNewMessageDeclarationAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.ExportUcc6, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var predeclarationAESMessage = GetNewMessageDeclarationAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.ExportPreDeclaration, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var annexAESMessage = GetNewMessageAnnexAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var complXAESMessage = GetNewMessageComplXAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var declarationDVDMessage = GetNewMessageDeclarationDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var queryAESMessage = GetNewMessageQueryAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationClearanceAESMessage = GetNewMessageInboxNotificationClearanceAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationInvalidationAESMessage = GetNewMessageInboxNotificationInvalidationAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationNonConformityAESMessage = GetNewMessageInboxNotificationNonConformityAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationExitResultAESMessage = GetNewMessageInboxNotificationExitResultAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationCceControlAESMessage = GetNewMessageInboxNotificationCceControlAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationExitNonConformityAESMessage = GetNewMessageInboxNotificationExitNonConformityAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var inboxNotificationExitClearanceAESMessage = GetNewMessageInboxNotificationExitClearanceAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var cancelAESMessage = GetNewMessageCancelAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var declarationAESAmendmentMessage = GetNewMessageDeclarationAESAmendmentCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationDVDMessage = GetNewMessageInboxNotificationDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var cancelDVDMessage = GetNewMessageCancelDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var departureCertReqAESMessage = GetNewMessageDepartureCertReqAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var complXDVDMessage = GetNewMessageComplXDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var queryDVDMessage = GetNewMessageQueryDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var ealAESMessage = GetNewMessageEALAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var cancelNCTSMessage = GetNewMessageCancelNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var departureNCTSMessage = GetNewMessageDepartureNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Ncts5Departure, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var departurePreDeclarationNCTSMessage = GetNewMessageDepartureNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var inboxInvalidTransitNCTSMessage = GetNewMessageInboxInvalidTransitNCTS(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var inboxDissatisfiedItemNCTSMessage = GetNewMessageInboxDissatisfiedItemNCTS(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var arrivalNCTSMessage = GetNewMessageArrivalNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000002", interchangeTransportType);
		var notifGoodsNCTSMessage = GetNewMessageNotifGoodsNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var amendmentNCTSMessage = GetNewMessageAmendmentNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var queryNCTSMessage = GetNewMessageQueryNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var annexNCTSMessage = GetNewMessageAnnexNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var notificationUnloadingNCTSMessage = GetNewMessageNotificationUnloadingNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000002", interchangeTransportType);
		var inboxPendingListEntryHeaderMessage = GetNewMessageInboxPendingListCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxPendingListExitReportMessage = GetNewMessageInboxPendingListCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType);
		var annexT2LPOUSMessage = GetNewMessageAnnexT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var queryT2LPOUSMessage = GetNewMessageQueryT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var requestT2LPOUSMessage = GetNewMessageRequestAndReceptionT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.T2lRequestPous);
		var presentationT2LPOUSMessage = GetNewMessagePresentationT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var receptionT2LPOUSMessage = GetNewMessageRequestAndReceptionT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.T2lReceptionPous);
		var exsMessage = GetNewMessageEXSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var g3DeclarationMessage = GetNewMessageG3Declaration(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "H7D0000002", interchangeTransportType);
		var g3RevokeMessage = GetNewMessageG3Revoke(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "H7D0000002", interchangeTransportType);
		var expeditionG5Message = GetNewMessageExpeditionG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var expAmendmentG5Message = GetNewMessageExpAmendmentG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var receptionG5Message = GetNewMessageReceptionG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var expCancelG5Message = GetNewMessageExpCancelG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var declarationH7Message = GetNewMessageDeclarationH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var annexH7Message = GetNewMessageAnnexH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var reexportH7Message = GetNewMessageReexportH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var cancellationH7Message = GetNewMessageCancellationH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var queryH7Message = GetNewMessageQueryH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var incompleteImportH1Message = GetNewMessageIncompleteImportH1Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationInvalidationH1Message = GetNewMessageInboxNotificationInvalidationH1Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var otherMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "AAA", isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		Factory.Save();

		ErrorReporter.Clear();

		CombineAssertions(() =>
		{
			processor.ExecuteBatch();
			exportMessage.Reload();
			AssertEquals("exportMessage is preprocessed", EDIMessage.Status.PreProcessedOK, exportMessage.EM_Status);
			exportAmendmentMessage.Reload();
			AssertEquals("exportAmendmentMessage is preprocessed", EDIMessage.Status.PreProcessedOK, exportAmendmentMessage.EM_Status);
			complXExportMessage.Reload();
			AssertEquals("complXExportMessage is preprocessed", EDIMessage.Status.PreProcessedOK, complXExportMessage.EM_Status);
			importPreSadMessage.Reload();
			AssertEquals("importPreSadMessage is received", EDIMessage.Status.Received, importPreSadMessage.EM_Status);
			t2LReceptionMessage.Reload();
			AssertEquals("t2LReceptionMessage is received", EDIMessage.Status.Received, t2LReceptionMessage.EM_Status);
			t2LReceptionAmendmentMessage.Reload();
			AssertEquals("t2LReceptionAmendmentMessage is received", EDIMessage.Status.Received, t2LReceptionAmendmentMessage.EM_Status);
			t2LExpeditionMessage.Reload();
			AssertEquals("t2LExpeditionMessage is received", EDIMessage.Status.Received, t2LExpeditionMessage.EM_Status);
			t2LExpeditionAmendmentMessage.Reload();
			AssertEquals("t2LExpeditionAmendmentMessage is received", EDIMessage.Status.Received, t2LExpeditionAmendmentMessage.EM_Status);
			nctsTIRMessage.Reload();
			AssertEquals("nctsTIRMessage is received", EDIMessage.Status.Received, nctsTIRMessage.EM_Status);
			canPreDUAImportMessage.Reload();
			AssertEquals("canPreDUAImportMessage is received", EDIMessage.Status.Received, canPreDUAImportMessage.EM_Status);
			djpImportMessage.Reload();
			AssertEquals("djpImportMessage is received", EDIMessage.Status.Received, djpImportMessage.EM_Status);
			importSimplifiedMessage.Reload();
			AssertEquals("importSimplifiedMessage is received", EDIMessage.Status.Received, importSimplifiedMessage.EM_Status);
			importCompleteMessage.Reload();
			AssertEquals("importCompleteMessage is received", EDIMessage.Status.Received, importCompleteMessage.EM_Status);
			box40AmendmentImportMessage.Reload();
			AssertEquals("box40AmendmentImportMessage is received", EDIMessage.Status.Received, box40AmendmentImportMessage.EM_Status);
			importQueryMessage.Reload();
			AssertEquals("importQueryMessage is received", EDIMessage.Status.Received, importQueryMessage.EM_Status);
			importNotificationMessage.Reload();
			AssertEquals("importNotificationMessage is received", EDIMessage.Status.Received, importNotificationMessage.EM_Status);
			exportNotificationMessage.Reload();
			AssertEquals("exportNotificationMessage is received", EDIMessage.Status.Received, exportNotificationMessage.EM_Status);
			nctsDepartureMessage.Reload();
			AssertEquals("nctsDepartureMessage is received", EDIMessage.Status.Received, nctsDepartureMessage.EM_Status);
			nctsArrivalAVIMessage.Reload();
			AssertEquals("nctsArrivalAVIMessage is received", EDIMessage.Status.Received, nctsArrivalAVIMessage.EM_Status);
			nctsArrivalOBSMessage.Reload();
			AssertEquals("nctsArrivalOBSMessage is received", EDIMessage.Status.Received, nctsArrivalOBSMessage.EM_Status);
			nctsArrivalAVOMessage.Reload();
			AssertEquals("nctsArrivalAVOMessage is received", EDIMessage.Status.Received, nctsArrivalAVOMessage.EM_Status);
			nctsArrivalTNAMessage.Reload();
			AssertEquals("nctsArrivalTNAMessage is received", EDIMessage.Status.Received, nctsArrivalTNAMessage.EM_Status);
			nctsArrivalTAOMessage.Reload();
			AssertEquals("nctsArrivalTAOMessage is received", EDIMessage.Status.Received, nctsArrivalTAOMessage.EM_Status);
			nctsArrivalTNNMessage.Reload();
			AssertEquals("nctsArrivalTNNMessage is received", EDIMessage.Status.Received, nctsArrivalTNNMessage.EM_Status);
			arrivalAtExitMessage.Reload();
			AssertEquals("arrivalAtExitMessage is received", EDIMessage.Status.Received, arrivalAtExitMessage.EM_Status);
			inboxNotificationNCTSDepartureclearance.Reload();
			AssertEquals("InboxNotificationNCTSDepartureclearance is received", EDIMessage.Status.Received, inboxNotificationNCTSDepartureclearance.EM_Status);
			inboxNotificationNCTSDepartureControl.Reload();
			AssertEquals("InboxNotificationNCTSDepartureControl is received", EDIMessage.Status.Received, inboxNotificationNCTSDepartureclearance.EM_Status);
			aesGoodsNotificationMessage.Reload();
			AssertEquals("aesGoodsNotificationMessage is received", EDIMessage.Status.Received, arrivalAtExitMessage.EM_Status);
			customsServiceErrorEntryHeaderMessage.Reload();
			AssertEquals("customsServiceErrorEntryHeaderMessage is received", EDIMessage.Status.Failed, customsServiceErrorEntryHeaderMessage.EM_Status);
			customsServiceErrorNctsHeaderMessage.Reload();
			AssertEquals("customsServiceErrorNctsHeaderMessage is received", EDIMessage.Status.Failed, customsServiceErrorNctsHeaderMessage.EM_Status);
			customsServiceErrorExitDetailMessage.Reload();
			AssertEquals("customsServiceErrorExitDetailMessage is received", EDIMessage.Status.Failed, customsServiceErrorExitDetailMessage.EM_Status);
			customsServiceErrorExitReportMessage.Reload();
			AssertEquals("customsServiceErrorExitReportMessage is received", EDIMessage.Status.Failed, customsServiceErrorExitReportMessage.EM_Status);
			customsServiceErrorUniversalEventEntryHeaderMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventEntryHeaderMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventEntryHeaderMessage.EM_Status);
			customsServiceErrorUniversalEventNctsHeaderMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventNctsHeaderMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventNctsHeaderMessage.EM_Status);
			customsServiceErrorUniversalEventExitDetailMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventExitDetailMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventExitDetailMessage.EM_Status);
			customsServiceErrorUniversalEventExitReportMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventExitReportMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventExitReportMessage.EM_Status);
			customsServiceErrorUniversalEventBadrequestEntryHeaderMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventBadrequestEntryHeaderMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventBadrequestEntryHeaderMessage.EM_Status);
			customsServiceErrorUniversalEventBadrequestNctsHeaderMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventBadrequestNctsHeaderMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventBadrequestNctsHeaderMessage.EM_Status);
			customsServiceErrorUniversalEventBadrequestExitDetailMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventBadrequestExitDetailMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventBadrequestExitDetailMessage.EM_Status);
			customsServiceErrorUniversalEventBadrequestExitReportMessage.Reload();
			AssertEquals("customsServiceErrorUniversalEventBadrequestExitReportMessage is received", EDIMessage.Status.Failed, customsServiceErrorUniversalEventBadrequestExitReportMessage.EM_Status);
			documentCaptureEntryHeaderMessage.Reload();
			AssertEquals("documentCaptureEntryHeaderMessage is received", EDIMessage.Status.Received, documentCaptureEntryHeaderMessage.EM_Status);
			documentCaptureNctsHeaderMessage.Reload();
			AssertEquals("documentCaptureNctsHeaderMessage is received", EDIMessage.Status.Received, documentCaptureNctsHeaderMessage.EM_Status);
			importClearanceEmailMessage.Reload();
			AssertEquals("importClearanceEmailMessage is received", EDIMessage.Status.Received, importClearanceEmailMessage.EM_Status);
			nctsClearanceEmailMessage.Reload();
			AssertEquals("nctsClearanceEmailMessage is received", EDIMessage.Status.Received, nctsClearanceEmailMessage.EM_Status);
			t2lClearanceEmailMessage.Reload();
			AssertEquals("t2lClearanceEmailMessage is received", EDIMessage.Status.Received, t2lClearanceEmailMessage.EM_Status);
			t2cClearanceEmailMessage.Reload();
			AssertEquals("t2cClearanceEmailMessage is received", EDIMessage.Status.Received, t2cClearanceEmailMessage.EM_Status);
			exportClearanceEmailMessage.Reload();
			AssertEquals("exportClearanceEmailMessage is received", EDIMessage.Status.Received, exportClearanceEmailMessage.EM_Status);
			g5ClearanceEmailMessage.Reload();
			AssertEquals("g5ClearanceEmailMessage is received", EDIMessage.Status.Received, g5ClearanceEmailMessage.EM_Status);
			declarationAESMessage.Reload();
			AssertEquals("declarationAESMessage is received", EDIMessage.Status.Received, declarationAESMessage.EM_Status);
			predeclarationAESMessage.Reload();
			AssertEquals("predeclarationAESMessage is received", EDIMessage.Status.Received, predeclarationAESMessage.EM_Status);
			annexAESMessage.Reload();
			AssertEquals("annexAESMessage is received", EDIMessage.Status.Received, annexAESMessage.EM_Status);
			complXAESMessage.Reload();
			AssertEquals("complXAESMessage is received", EDIMessage.Status.Received, complXAESMessage.EM_Status);
			declarationDVDMessage.Reload();
			AssertEquals("declarationDVDMessage is received", EDIMessage.Status.Received, declarationDVDMessage.EM_Status);
			queryAESMessage.Reload();
			AssertEquals("queryAESMessage is received", EDIMessage.Status.Received, queryAESMessage.EM_Status);
			inboxNotificationClearanceAESMessage.Reload();
			AssertEquals("inboxNotificationClearanceAESMessage is received", EDIMessage.Status.Received, inboxNotificationClearanceAESMessage.EM_Status);
			inboxNotificationInvalidationAESMessage.Reload();
			AssertEquals("inboxNotificationInvalidationAESMessage is received", EDIMessage.Status.Received, inboxNotificationInvalidationAESMessage.EM_Status);
			inboxNotificationNonConformityAESMessage.Reload();
			AssertEquals("inboxNotificationNonConformityAESMessage is received", EDIMessage.Status.Received, inboxNotificationNonConformityAESMessage.EM_Status);
			inboxNotificationExitResultAESMessage.Reload();
			AssertEquals("inboxNotificationExitResultAESMessage is received", EDIMessage.Status.Received, inboxNotificationExitResultAESMessage.EM_Status);
			inboxNotificationCceControlAESMessage.Reload();
			AssertEquals("inboxNotificationCceControlAESMessage is received", EDIMessage.Status.Received, inboxNotificationCceControlAESMessage.EM_Status);
			inboxNotificationExitNonConformityAESMessage.Reload();
			AssertEquals("inboxNotificationExitNonConformityAESMessage is received", EDIMessage.Status.Received, inboxNotificationExitNonConformityAESMessage.EM_Status);
			inboxNotificationExitClearanceAESMessage.Reload();
			AssertEquals("inboxNotificationExitClearanceAESMessage is received", EDIMessage.Status.Received, inboxNotificationExitClearanceAESMessage.EM_Status);
			cancelAESMessage.Reload();
			AssertEquals("cancelAESMessage is received", EDIMessage.Status.Received, cancelAESMessage.EM_Status);
			declarationAESAmendmentMessage.Reload();
			AssertEquals("declarationAESAmendmentMessage is received", EDIMessage.Status.Received, declarationAESAmendmentMessage.EM_Status);
			inboxNotificationDVDMessage.Reload();
			AssertEquals("inboxNotificationDVDMessage is received", EDIMessage.Status.Received, inboxNotificationDVDMessage.EM_Status);
			cancelDVDMessage.Reload();
			AssertEquals("cancelDVDMessage is received", EDIMessage.Status.Received, cancelDVDMessage.EM_Status);
			departureCertReqAESMessage.Reload();
			AssertEquals("departureCertReqAESMessage is received", EDIMessage.Status.Received, departureCertReqAESMessage.EM_Status);
			complXDVDMessage.Reload();
			AssertEquals("complXDVDMessage is received", EDIMessage.Status.Received, complXDVDMessage.EM_Status);
			queryDVDMessage.Reload();
			AssertEquals("queryDVDMessage is received", EDIMessage.Status.Received, queryDVDMessage.EM_Status);
			ealAESMessage.Reload();
			AssertEquals("ealAESMessage is received", EDIMessage.Status.Received, ealAESMessage.EM_Status);
			cancelNCTSMessage.Reload();
			AssertEquals("cancelNCTSMessage is received", EDIMessage.Status.Received, cancelNCTSMessage.EM_Status);
			departureNCTSMessage.Reload();
			AssertEquals("departureNCTSMessage is received", EDIMessage.Status.Received, departureNCTSMessage.EM_Status);
			departurePreDeclarationNCTSMessage.Reload();
			AssertEquals("departurePreDeclarationNCTSMessage is received", EDIMessage.Status.Received, departurePreDeclarationNCTSMessage.EM_Status);
			inboxInvalidTransitNCTSMessage.Reload();
			AssertEquals("inboxInvalidTransitNCTSMessage is received", EDIMessage.Status.Received, inboxInvalidTransitNCTSMessage.EM_Status);
			inboxDissatisfiedItemNCTSMessage.Reload();
			AssertEquals("inboxDissatisfiedItemNCTSMessage is received", EDIMessage.Status.Received, inboxDissatisfiedItemNCTSMessage.EM_Status);
			arrivalNCTSMessage.Reload();
			AssertEquals("arrivalNCTSMessage is received", EDIMessage.Status.Received, arrivalNCTSMessage.EM_Status);
			notifGoodsNCTSMessage.Reload();
			AssertEquals("notifGoodsNCTSMessage is received", EDIMessage.Status.Received, notifGoodsNCTSMessage.EM_Status);
			amendmentNCTSMessage.Reload();
			AssertEquals("amendmentNCTSMessage is received", EDIMessage.Status.Received, amendmentNCTSMessage.EM_Status);
			queryNCTSMessage.Reload();
			AssertEquals("queryNCTSMessage is received", EDIMessage.Status.Received, queryNCTSMessage.EM_Status);
			annexNCTSMessage.Reload();
			AssertEquals("annexNCTSMessage is received", EDIMessage.Status.Received, annexNCTSMessage.EM_Status);
			notificationUnloadingNCTSMessage.Reload();
			AssertEquals("notificationUnloadingNCTSMessage is received", EDIMessage.Status.Received, notificationUnloadingNCTSMessage.EM_Status);
			inboxPendingListEntryHeaderMessage.Reload();
			AssertEquals("inboxPendingListEntryHeaderMessage is received", EDIMessage.Status.Received, inboxPendingListEntryHeaderMessage.EM_Status);
			inboxPendingListExitReportMessage.Reload();
			AssertEquals("inboxPendingListExitReportMessage is received", EDIMessage.Status.Received, inboxPendingListExitReportMessage.EM_Status);
			annexT2LPOUSMessage.Reload();
			AssertEquals("annexT2LPOUSMessage is received", EDIMessage.Status.Received, annexT2LPOUSMessage.EM_Status);
			queryT2LPOUSMessage.Reload();
			AssertEquals("QueryT2LPOUSMessage is received", EDIMessage.Status.Received, queryT2LPOUSMessage.EM_Status);
			requestT2LPOUSMessage.Reload();
			AssertEquals("requestT2LPOUSMessage is received", EDIMessage.Status.Received, requestT2LPOUSMessage.EM_Status);
			presentationT2LPOUSMessage.Reload();
			AssertEquals("presentationT2LPOUSMessage is received", EDIMessage.Status.Received, presentationT2LPOUSMessage.EM_Status);
			receptionT2LPOUSMessage.Reload();
			AssertEquals("receptionT2LPOUSMessage is received", EDIMessage.Status.Received, receptionT2LPOUSMessage.EM_Status);
			exsMessage.Reload();
			AssertEquals("exsMessage is received", EDIMessage.Status.Received, exsMessage.EM_Status);
			g3DeclarationMessage.Reload();
			AssertEquals("G3DeclarationMessage is received", EDIMessage.Status.Received, g3DeclarationMessage.EM_Status);
			g3RevokeMessage.Reload();
			AssertEquals("G3RevokeMessage is received", EDIMessage.Status.Received, g3RevokeMessage.EM_Status);
			expeditionG5Message.Reload();
			AssertEquals("expeditionG5Message is received", EDIMessage.Status.Received, expeditionG5Message.EM_Status);
			expAmendmentG5Message.Reload();
			AssertEquals("expAmendmentG5Message is received", EDIMessage.Status.Received, expAmendmentG5Message.EM_Status);
			receptionG5Message.Reload();
			AssertEquals("receptionG5Message is received", EDIMessage.Status.Received, receptionG5Message.EM_Status);
			expCancelG5Message.Reload();
			AssertEquals("expCancelG5Message is received", EDIMessage.Status.Received, receptionG5Message.EM_Status);
			declarationH7Message.Reload();
			AssertEquals("declarationH7Message is received", EDIMessage.Status.Received, declarationH7Message.EM_Status);
			annexH7Message.Reload();
			AssertEquals("annexH7Message is received", EDIMessage.Status.Received, annexH7Message.EM_Status);
			reexportH7Message.Reload();
			AssertEquals("reexportH7Message is received", EDIMessage.Status.Received, reexportH7Message.EM_Status);
			cancellationH7Message.Reload();
			AssertEquals("cancellationH7Message is received", EDIMessage.Status.Received, cancellationH7Message.EM_Status);
			queryH7Message.Reload();
			AssertEquals("queryH7Message is received", EDIMessage.Status.Received, queryH7Message.EM_Status);
			incompleteImportH1Message.Reload();
			AssertEquals("incompleteImportH1Message is received", EDIMessage.Status.Received, incompleteImportH1Message.EM_Status);
			inboxNotificationInvalidationH1Message.Reload();
			AssertEquals("inboxNotificationInvalidationH1Message is received", EDIMessage.Status.Received, inboxNotificationInvalidationH1Message.EM_Status);
			otherMessage.Reload();
			AssertEquals("otherMessage is queued", EDIMessage.Status.Queued, otherMessage.EM_Status);

			AssertEquals("ServiceTaskThatRequiresSetUserContext is not reported", ZString.Empty, ErrorReporter.LastMessageReported);
			AssertNotContains("EnableImplicitUserContextAccessReporting message", "accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
		});
	}

	public void TestGetMessageProcessorsCore() => CombineAssertions(() =>
	{
		var processor = new ESBranchCustomsMessageProcessorForTesting();
		foreach (var messageProcessor in processor.GetMessageProcessorsCoreExposed())
		{
			Assert(typeof(BranchCustomsApplicationTypeMessageProcessor).IsInstanceOfType(messageProcessor));
		}
	});

	public void TestMessageFilter_EHub()
	{
		AssertMessageFilter(EDIInterchange.TransportType.eHub, false);
	}

	public void TestMessageFilter_DirectxT()
	{
		AssertMessageFilter(EDIInterchange.TransportType.xT, true);
	}

	public void AssertMessageFilter(ZString interchangeTransportType, bool isDirectxT)
	{
		var newCompany = Factory.New<GlbCompany>();
		newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
		newCompany.GC_Code = "GER";
		var newBranch = Factory.New<GlbBranch>();
		newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
		newBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RL_Code;
		newBranch.GB_Code = "XXX";
		var newBranchForNewCompany = newCompany.Branches.AddNew();
		newBranchForNewCompany.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.Germany)).RL_Code;
		newBranchForNewCompany.GB_Code = "DE1";

		Factory.Save();

		var exportMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Export, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var exportAmendmentMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.ExportAmendment, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var complXExportMessage = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.TypeXExport, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importPreSadMessage = GetNewMessagePDICorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LReceptionMessage = GetNewMessageT2LReceptionCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LReceptionAmendmentMessage = GetNewMessageT2LReceptionAmendmentCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LExpeditionMessage = GetNewMessageT2LExpeditionCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LExpeditionAmendmentMessage = GetNewMessageT2LExpeditionAmendmentCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var t2LClearanceMessage = GetNewMessageT2LClearanceCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var nctsTIRMessage = GetNewMessageNctsTIRCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCT00000001", interchangeTransportType);
		var canPreDUAImportMessage = GetNewMessageCANPreDUAImportCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var djpImportMessage = GetNewMessageDJPImportCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importSimplifiedMessage = GetNewMessagePDSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importCompleteMessage = GetNewMessagePDCCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var box40AmendmentImportMessage = GetNewMessageBox40AmendmentImportCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importQueryMessage = GetNewMessageImportQueryCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var importNotificationMessage = GetNewMessageImportNotifCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var exportNotificationMessage = GetNewMessageExportNotifCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var nctsDepartureMessage = GetNewMessageNctsDepartureCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var nctsArrivalAVIMessage = GetNewMessageNctsArrivalAVICorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalOBSMessage = GetNewMessageNctsArrivalOBSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalAVOMessage = GetNewMessageNctsArrivalAVOCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalTNAMessage = GetNewMessageNctsArrivalTNACorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalTAOMessage = GetNewMessageNctsArrivalTAOCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000001", interchangeTransportType);
		var nctsArrivalTNNMessage = GetNewMessageNctsArrivalTNNCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000002", interchangeTransportType);
		var arrivalAtExitMessage = GetNewMessageArrivalAtExit(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType);
		var inboxNotificationNCTSDepartureclearance = GetNewMessageInboxNotificationNCTSDepartureclearance(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var inboxNotificationNCTSDepartureControl = GetNewMessageInboxNotificationNCTSDepartureControl(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var aesGoodsNotificationMessage = GetNewMessageAESGoodsNotification(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var customsServiceErrorEntryHeaderMessage = GetNewMessageCustomsServiceErrorCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var customsServiceErrorNctsHeaderMessage = GetNewMessageCustomsServiceErrorCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var customsServiceErrorExitDetailMessage = GetNewMessageCustomsServiceErrorCorrectForCusExitDetail(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType);
		var customsServiceErrorExitReportMessage = GetNewMessageCustomsServiceErrorCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var customsServiceErrorUniversalEventEntryHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventNctsHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventExitDetailMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitDetail(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventExitReportMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent);
		var customsServiceErrorUniversalEventBadrequestEntryHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var customsServiceErrorUniversalEventBadrequestNctsHeaderMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var customsServiceErrorUniversalEventBadrequestExitDetailMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitDetail(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var customsServiceErrorUniversalEventBadrequestExitReportMessage = GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType, DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest);
		var documentCaptureEntryHeaderMessage = GetNewMessageDocumentCaptureCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var documentCaptureNctsHeaderMessage = GetNewMessageDocumentCaptureCorrectForNctsHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var importClearanceEmailMessage = GetNewMessageImportClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "20ES00999930006184");
		var nctsClearanceEmailMessage = GetNewMessageNctsClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "20ES00999830001288");
		var t2lClearanceEmailMessage = GetNewMessageT2LClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "21ES009999L0002748", DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail);
		var t2cClearanceEmailMessage = GetNewMessageT2LClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "21ES009999M0000707", DeclarationMessageTypeList.Codes.T2cClearanceEmail);
		var exportClearanceEmailMessage = GetNewMessageExportClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "21ES00280120889150");
		var g5ClearanceEmailMessage = GetNewMessageG5ClearanceEmailCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "25ESG5G000000749Y0");
		var declarationAESMessage = GetNewMessageDeclarationAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.ExportUcc6, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var predeclarationAESMessage = GetNewMessageDeclarationAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.ExportPreDeclaration, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var annexAESMessage = GetNewMessageAnnexAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var complXAESMessage = GetNewMessageComplXAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var declarationDVDMessage = GetNewMessageDeclarationDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var queryAESMessage = GetNewMessageQueryAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationClearanceAESMessage = GetNewMessageInboxNotificationClearanceAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationInvalidationAESMessage = GetNewMessageInboxNotificationInvalidationAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationNonConformityAESMessage = GetNewMessageInboxNotificationNonConformityAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationExitResultAESMessage = GetNewMessageInboxNotificationExitResultAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationCceControlAESMessage = GetNewMessageInboxNotificationCceControlAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationExitNonConformityAESMessage = GetNewMessageInboxNotificationExitNonConformityAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var inboxNotificationExitClearanceAESMessage = GetNewMessageInboxNotificationExitClearanceAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var cancelAESMessage = GetNewMessageCancelAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var declarationAESAmendmentMessage = GetNewMessageDeclarationAESAmendmentCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationDVDMessage = GetNewMessageInboxNotificationDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var cancelDVDMessage = GetNewMessageCancelDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var departureCertReqAESMessage = GetNewMessageDepartureCertReqAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var complXDVDMessage = GetNewMessageComplXDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var queryDVDMessage = GetNewMessageQueryDVDCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var ealAESMessage = GetNewMessageEALAESCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "CCE00000001", interchangeTransportType);
		var cancelNCTSMessage = GetNewMessageCancelNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var departureNCTSMessage = GetNewMessageDepartureNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Ncts5Departure, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var departurePreDeclarationNCTSMessage = GetNewMessageDepartureNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var inboxInvalidTransitNCTSMessage = GetNewMessageInboxInvalidTransitNCTS(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var inboxDissatisfiedItemNCTSMessage = GetNewMessageInboxDissatisfiedItemNCTS(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var arrivalNCTSMessage = GetNewMessageArrivalNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000002", interchangeTransportType);
		var notifGoodsNCTSMessage = GetNewMessageNotifGoodsNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var amendmentNCTSMessage = GetNewMessageAmendmentNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var queryNCTSMessage = GetNewMessageQueryNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var annexNCTSMessage = GetNewMessageAnnexNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCD00000001", interchangeTransportType);
		var notificationUnloadingNCTSMessage = GetNewMessageNotificationUnloadingNCTSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "NCA00000002", interchangeTransportType);
		var inboxPendingListEntryHeaderMessage = GetNewMessageInboxPendingListCorrectForEntryHeader(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxPendingListExitReportMessage = GetNewMessageInboxPendingListCorrectForCusExitReport(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "AAE00000001", interchangeTransportType);
		var annexT2LPOUSMessage = GetNewMessageAnnexT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var queryT2LPOUSMessage = GetNewMessageQueryT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var requestT2LPOUSMessage = GetNewMessageRequestAndReceptionT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.T2lRequestPous);
		var presentationT2LPOUSMessage = GetNewMessagePresentationT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var receptionT2LPOUSMessage = GetNewMessageRequestAndReceptionT2LPOUSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType, DeclarationMessageTypeList.Codes.T2lReceptionPous);
		var exsMessage = GetNewMessageEXSCorrect(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var g3DeclarationMessage = GetNewMessageG3Declaration(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "H7D0000002", interchangeTransportType);
		var g3RevokeMessage = GetNewMessageG3Revoke(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "H7D0000002", interchangeTransportType);
		var expeditionG5Message = GetNewMessageExpeditionG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var expAmendmentG5Message = GetNewMessageExpAmendmentG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var receptionG5Message = GetNewMessageReceptionG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var expCancelG5Message = GetNewMessageExpCancelG5Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "TS00001", interchangeTransportType);
		var declarationH7Message = GetNewMessageDeclarationH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var annexH7Message = GetNewMessageAnnexH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var reexportH7Message = GetNewMessageReexportH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var cancellationH7Message = GetNewMessageCancellationH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var queryH7Message = GetNewMessageQueryH7(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "ABL000001", interchangeTransportType);
		var incompleteImportH1Message = GetNewMessageIncompleteImportH1Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var inboxNotificationInvalidationH1Message = GetNewMessageInboxNotificationInvalidationH1Correct(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var message1 = GetNewMessageWithoutInterchange(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Export, isDirectxT ? string.Empty : "S900052890/1");
		var message2 = GetNewMessage("OTH", GlbBranch.CurrentBranch.PK, DeclarationMessageTypeList.Codes.Export, isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);
		var message3 = GetNewMessage(ApplicationCodeList.Codes.ESCustomsMessage, GlbBranch.CurrentBranch.PK, "AAA", isDirectxT ? string.Empty : "S900052890/1", interchangeTransportType);

		Factory.Save();

		processor.ExecuteBatch();
		CombineAssertions(() =>
		{
			AssertProcessed(DeclarationMessageTypeList.Codes.Export, exportMessage, true, false, true);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportAmendment, exportAmendmentMessage, true, false, true);
			AssertProcessed(DeclarationMessageTypeList.Codes.TypeXExport, complXExportMessage, true, false, true);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration, importPreSadMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lReception, t2LReceptionMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lReceptionAmendment, t2LReceptionAmendmentMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lExpedition, t2LExpeditionMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lExpeditionAmendment, t2LExpeditionAmendmentMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsTir, nctsTIRMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation, canPreDUAImportMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration, importSimplifiedMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration, importCompleteMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportAmendmentBox40, box40AmendmentImportMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportQuery, importQueryMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InBoxNotificationForImport, importNotificationMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.PendingSupportingDocuments, djpImportMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InBoxNotificationForExport, exportNotificationMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsDeparture, nctsDepartureMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsArrivalNotification, nctsArrivalAVIMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsUnloadingRemarks, nctsArrivalOBSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs, nctsArrivalAVOMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi, nctsArrivalTNAMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb, nctsArrivalTAOMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration, nctsArrivalTNNMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ArrivalAtExit, arrivalAtExitMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance, inboxNotificationNCTSDepartureclearance, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls, inboxNotificationNCTSDepartureControl, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportNotification, aesGoodsNotificationMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceError + " for CusEntryHeader", customsServiceErrorEntryHeaderMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceError + " for NctsHeader", customsServiceErrorNctsHeaderMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceError + " for CusExitDetail", customsServiceErrorExitDetailMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceError + " for CusExitReport", customsServiceErrorExitReportMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent + " for CusEntryHeader", customsServiceErrorUniversalEventEntryHeaderMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent + " for NctsHeader", customsServiceErrorUniversalEventNctsHeaderMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent + " for CusExitDetail", customsServiceErrorUniversalEventExitDetailMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent + " for CusExitReport", customsServiceErrorUniversalEventExitReportMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest + " for CusEntryHeader", customsServiceErrorUniversalEventBadrequestEntryHeaderMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest + " for NctsHeader", customsServiceErrorUniversalEventBadrequestNctsHeaderMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest + " for CusExitDetail", customsServiceErrorUniversalEventBadrequestExitDetailMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEventBadRequest + " for CusExitReport", customsServiceErrorUniversalEventBadrequestExitReportMessage, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.EsDocumentRequest + " for CusEntryHeader", documentCaptureEntryHeaderMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.EsDocumentRequest + " for NctsHeader", documentCaptureNctsHeaderMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportClearanceEmail, importClearanceEmailMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.NctsClearanceEmail, nctsClearanceEmailMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lExpeditionClearanceEmail, t2lClearanceEmailMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2cClearanceEmail, t2cClearanceEmailMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportClearanceEmail, exportClearanceEmailMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G5ClearanceEmail, g5ClearanceEmailMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportUcc6, declarationAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportPreDeclaration, predeclarationAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportAnnexes, annexAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportAnnexes, complXAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.DvdH2, declarationDVDMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportQuery, queryAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportClearanceCommunication, inboxNotificationClearanceAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication, inboxNotificationInvalidationAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, inboxNotificationNonConformityAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportExitResultCommunication, inboxNotificationExitResultAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportCceControlCommunication, inboxNotificationCceControlAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification, inboxNotificationExitNonConformityAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, inboxNotificationExitClearanceAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportCancellation, cancelAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExportAmendmentUcc6, declarationAESAmendmentMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, inboxNotificationDVDMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.DvdH2Cancellation, cancelDVDMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.RequestExportExitCertificate, departureCertReqAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.TypeXDvdH2, complXDVDMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.DvdH2Query, queryDVDMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6, ealAESMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, cancelNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5Departure, departureNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, departurePreDeclarationNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation, inboxInvalidTransitNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture, inboxDissatisfiedItemNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, arrivalNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, notifGoodsNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, amendmentNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.TransitNcts5Query, queryNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, annexNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, notificationUnloadingNCTSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxPendingList, inboxPendingListEntryHeaderMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.InboxPendingList, inboxPendingListExitReportMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lDocumentationPous, annexT2LPOUSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lQueryPous, queryT2LPOUSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lRequestPous, requestT2LPOUSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lPresentationPous, presentationT2LPOUSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.T2lReceptionPous, receptionT2LPOUSMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ExitSummaryDeclaration, exsMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G3DeclarationOfGoods, g3DeclarationMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G3RevocationOfGoods, g3RevokeMessage, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G5v1Expedition, expeditionG5Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment, expAmendmentG5Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G5v1Reception, receptionG5Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation, expCancelG5Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.H7Declaration, declarationH7Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.H7Annexes, annexH7Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.H7ReExport, reexportH7Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.H7Cancellation, cancellationH7Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.H7Query, queryH7Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, incompleteImportH1Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.ImportH1InvalidationCommunication, inboxNotificationInvalidationH1Message, true, false, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Export + " without response interchange", message1, true, true, false);
			AssertProcessed(DeclarationMessageTypeList.Codes.Export + " with different application code", message2, false, false, false);
			AssertProcessed("AAA", message3, false, false, false);
			ErrorReporter.Clear();
		});
	}

	TestEdiMessage GetNewMessage(ZString appCode, ZGuid branchPK, ZString messageType, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"
<root>
	<cod-ret>0</cod-ret>
	<RESPUESTA>UNB+UNOA:1+AEATADUE:ZZ+BUZON:ZZ+200102:1100+02110053624233++&EE'UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'</RESPUESTA>
</root>");

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageWithoutInterchange(ZString appCode, ZGuid branchPK, ZString messageType, ZString applicationReference)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"
<root>
	<cod-ret>0</cod-ret>
	<RESPUESTA>UNB+UNOA:1+AEATADUE:ZZ+BUZON:ZZ+200102:1100+02110053624233++&EE'UNH+02110053624233+CUSRES:1:921:UN:ECS003'BGM+962+S900052890/1+11'NAD+EX+A78587268:167:148'NAD+1+ESA78587268:167:148'DTM+148:2001021100:201'DTM+268:20200401:102'GIS+4:117:148'GIS+24:119:A2:1'RFF+ABT:20ES00999910000035'AUT+LT6B5QJ98HC6DHNY+LEVA'DTM+204:2001021100:201'AUT+3J9MCS7TAM3KLZQC+T2LF'DTM+204:2001021100:201'UNT+14+02110053624233'UNZ+1+02110053624233'</RESPUESTA>
</root>");

		return result;
	}

	TestEdiMessage GetNewMessagePDICorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.PreDUAIncompleteImportTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageT2LReceptionCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReception;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		Factory.Save();

		var interchange = CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);
		if (interchangeTransportType != EDIInterchange.TransportType.xT)
		{
			interchange.EI_HeaderText = ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>{0}</SentEDIMessageNumber>
</Headers>", result.EM_MessageNum);
		}

		declaration.JE_GS_NKCusAgent = staff.GS_Code;

		return result;
	}

	TestEdiMessage GetNewMessageT2LReceptionAmendmentCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionAmendmentTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lReceptionAmendment;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageT2LExpeditionCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionTestFilePath, "AcceptedAndClearedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lExpedition;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageT2LExpeditionAmendmentCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionAmendmentTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lExpeditionAmendment;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageT2LClearanceCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ClearanceTestFilePath, "t2l_clearance_accepted.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lClearance;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsTIRCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsTir;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+10100507081242+CUSRES:1:921:UN:TIR002'BGM+962+NCT00000001+11'NAD+AF+ESA78587268:167:148'NAD+DT+ESA78587268:167:148'DTM+148:2004101005:201'DTM+268:20200416:102'GIS+4:117:148'GIS+24:116::A3'GIS+24:119::1'RFF+ABK:20ES00999950012811'AUT+A198543129CBF854'DTM+204:2005101005:201'UNT+13+10100507081242'UNZ+1+10100507081242'");

		CreateResponseInterchange(result, InterchangeIDNcts2, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsDepartureCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsDeparture;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+10100507081242+CUSRES:1:921:UN:TEX011'BGM+962+NCD00000001+11'NAD+EX+ESA78587268:167:148'NAD+DT+ESA78587268:167:148'NAD+AF+ESA78587268:167:148'DTM+148:2004101005:201'DTM+268:20200416:102'GIS+4:117:148'GIS+24:116::A3'GIS+24:119::1'RFF+ABK:20ES00999950012811'AUT+A198543129CBF854'DTM+204:2005101005:201'UNT+14+10100507081242'UNZ+1+10100507081242'");

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsArrivalAVICorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsArrivalNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+20185637975246+CUSRES:D:96B:UN:AVI006'BGM+AVI+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+2:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'");

		CreateResponseInterchange(result, InterchangeIDNcts3, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsArrivalOBSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsArrivalNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+20185637975246+CUSRES:D:96B:UN:OBS006'BGM+OBS+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+2:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'");

		CreateResponseInterchange(result, InterchangeIDNcts3, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsArrivalAVOCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsArrivalNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+20185637975246+CUSRES:D:96B:UN:AVO006'BGM+AVO+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+2:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'");

		CreateResponseInterchange(result, InterchangeIDNcts3, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsArrivalTNACorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsArrivalNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+20185637975246+CUSRES:D:96B:UN:TNA006'BGM+TNA+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+2:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'");

		CreateResponseInterchange(result, InterchangeIDNcts3, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsArrivalTNNCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.TNNNCTSTestFilePath, "AcceptedMessage.txt");

		var mrnCode = "22ES000101500540K2";

		CreateOrUpdateCusEntryNumber(nctsHeader4, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrnCode, ZString.Empty);
		var nctsHeader = (NCTS.Business.NctsHeader)nctsHeader4;
		nctsHeader.ESNctsHeader.CEN_TNNArrival = true;

		var nctsHeaderTNNDeparture = (NCTS.Business.NctsHeader)Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: "NCT00000005", phase: "TNN"));
		CreateOrUpdateCusEntryNumber(nctsHeaderTNNDeparture, CusEntryNumberTypes.Standard.MovementReferenceNumber, mrnCode, ZString.Empty);

		nctsHeaderTNNDeparture.ESNctsHeader.CEN_TNNArrival = true;
		nctsHeader.ArrivalMovementHeader.BM_BM_DepartureMovement = nctsHeaderTNNDeparture.MovementHeader.PK;

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts4, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNctsArrivalTAOCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsArrivalNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format("UNH+20185637975246+CUSRES:D:96B:UN:TAO006'BGM+TAO+17/00179@6+11'DTM+148:2011201856:201'GIS+4:117:148'GIS+2:120:148'NAD+DT+ESA78587268:167:148'RFF+ABT:20ES009998500102'RFF+AFB:99980000521'UNT+8+20185637975246'UNZ+1+20185637975246'");

		CreateResponseInterchange(result, InterchangeIDNcts3, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCANPreDUAImportCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CANPreDUAImportTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessagePDSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.SimplifiedImportTestFilePath, "AcceptedGreenCircuitMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessagePDCCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CompleteImportTestFilePath, "AcceptedGreenCircuitMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportCompletePreDeclaration;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageBox40AmendmentImportCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.Box40AmendmentImportTestFilePath, "AcceptedGreenCircuitMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportAmendmentBox40;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageImportQueryCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ImportQueryTestFilePath, "AcceptedGreenCircuitMessageProcedureB.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportQuery;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageImportNotifCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationImportTestFilePath, "AcceptedGreenCircuitMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InBoxNotificationForImport;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageExportNotifCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExportTestFilePath, "AcceptedNoClearanceMessage.txt");

		var interchangeIDEntry = new ZGuid("87FAA3F5-423A-4C15-B8B4-3170AFA894BB");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("19ES00999910002776");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InBoxNotificationForExport;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageEXSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileRedCircuitContents = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EXSTestFilePath, "AcceptedRedCircuitMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExitSummaryDeclaration;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileRedCircuitContents;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorCorrectForEntryHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorTestFilePath, "ServiceErrorMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.CustomsServiceError;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorCorrectForNctsHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorTestFilePath, "ServiceErrorMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.CustomsServiceError;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorCorrectForCusExitDetail(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorTestFilePath, "ServiceErrorMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.CustomsServiceError;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDExitDetail, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorCorrectForCusExitReport(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorTestFilePath, "ServiceErrorMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.CustomsServiceError;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDExitReport, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorUniversalEventCorrectForEntryHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType, ZString messageType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorUniversalEventTestFilePath, "ServiceErrorUniversalEventMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorUniversalEventCorrectForNctsHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType, ZString messageType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorUniversalEventTestFilePath, "ServiceErrorUniversalEventMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitDetail(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType, ZString messageType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorUniversalEventTestFilePath, "ServiceErrorUniversalEventMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDExitDetail, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCustomsServiceErrorUniversalEventCorrectForCusExitReport(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType, ZString messageType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CustomsServiceErrorUniversalEventTestFilePath, "ServiceErrorUniversalEventMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDExitReport, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDocumentCaptureCorrectForEntryHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DocumentCaptureTestFilePath, "DocumentCaptureMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.EsDocumentRequest;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDocumentCaptureCorrectForNctsHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var serviceErrorMessage = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DocumentCaptureTestFilePath, "DocumentCaptureMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.EsDocumentRequest;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = serviceErrorMessage;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageArrivalAtExit(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ArrivalAtExit;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"UNH+19100229549129+CUSRES:1:921:UN:ECSR02'BGM+962+00229@1+11'NAD+1+ESA78587268:167:148'DTM+148:2101191002:201'GIS+4:117:148'RFF+ABT:21ES00999910000235'AUT+W3HK2AT5T6MFTJ28'DTM+204:2101191002:201'UNT+9+19100229549129'UNZ+1+19100229549129'");

		CreateResponseInterchange(result, InterchangeIDExitDetail, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDJPImportCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DJPTestFilePath, "DJP-accept.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.PendingSupportingDocuments;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageAESGoodsNotification(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.GoodsNotificationAESTestFilePath, "AcceptedGreenCircuitLMessageAEAT.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageImportClearanceEmailCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportClearanceEmail;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"A su declaración de importación con número 20ES00999930006184 se le ha asignado con el siguiente código seguro de verificación (C.S.V.)  del Justificante de Levante. Este C.S.V. le permitirá la consulta e impresión de su contenido en la Sede de la A.E.A.T. https://urldefense.com/v3/__https://www.agenciatributaria.gob.es__;!!Na5NE8kfbMIR6Ys!43cnxwoRROiqYk-EdpeT3qYT-p9zwPWIprBy6yPEMm8jWnsWz0-OCF8EznfLpn_CSg$ .
CSV Levante: 52BD5D2SD67PG42H
CSV Certificado de Importación: PBAGF96MUR2EXDER
BULTOS: 10
NUMERO REFERENCIA CASILLA 7:
CONTENEDORES: FSCU4762868,12312331231,31132312
MARCAS: RTDAS");

		return result;
	}

	TestEdiMessage GetNewMessageNctsClearanceEmailCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.NctsClearanceEmail;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"Su declaración de expedición de tránsito con número MRN: 20ES00999830001288 ha sido despachada con el siguiente Número de Autentificación de Levante. Ya puede/debe imprimir el Documento de Acompañamiento (D.A.T.) en la Sede Electrónica de la AEAT (puede cuando está autorizado a imprimir, y debe cuando no esté autorizado a imprimir).

Número de Autentificación de Levante: 1D4DF4B23C2336D7
Fecha de Levante: 02-07-2021
Fecha Máxima de Llegada: 09-07-2021
Resultado al Despacho: A1
BULTOS: 33
CONTENEDORES: CAIU4568990, CAIU4568990, CAIU4568990, CAIU4568990
MARCAS: PUS, PUS, PUS, PUS, PUS, PUS, PUS, PUS, PUS, PUS");

		return result;
	}

	TestEdiMessage GetNewMessageT2LClearanceEmailCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString messageType)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"Su declaración de T2L con número 21ES009999L0002748 ha sido despachado con el siguiente código seguro de verificación (C.S.V.) del justificante de Levante. Este C.S.V. le permitirá la consulta e impresión de su contenido en la Sede de la A.E.A.T.

CSV: 34YEUXEP8E7GE9XD
BULTOS: 22
CONTENEDORES: CONTENEDOR1, CONTENEDOR2
MARCAS: RTDASÑ");

		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "S900052890/1";

		entryHeader.MovementReferenceNumberSetter("21ES009999L0002748");

		var newEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Spain.T2CMovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "21ES009999M0000707";
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;

		return result;
	}

	TestEdiMessage GetNewMessageExportClearanceEmailCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "S900052890/1";

		entryHeader.MovementReferenceNumberSetter(applicationReference, ZDateTime.Today);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportClearanceEmail;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"Su declaración de exportación con número 21ES00280120889150 ha sido despachada con el siguiente código  seguro de verificación (C.S.V.) del Justificante  de Levante. Este C.S.V. le permitirá la impresión y consulta de dicho justificante en la Sede de la A.E.A.T. (https://www.agenciatributaria.gob.es).

Código Seguro Verificación (C.S.V.): 5ZJV4LCUFX79CJY6 Fecha Máxima de Llegada: 29-06-2021 Fecha de Levante: 20-06-2021 Resultado al Despacho: A2
BULTOS: 17
CONTENEDORES: 
MARCAS: ELECTRIC TRAIN TALGO 250, WAGON, 2502N-01/2502N-02/2502N-03/2502N-04, ROTULADAS, ROTULADAS, ROTULADAS, ROTULADAS");

		return result;
	}

	TestEdiMessage GetNewMessageG5ClearanceEmailCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference)
	{
		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G5ClearanceEmail;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = ZString.Format(@"La aduana 9998 - Pruebas ha despachado en recepción el G5 con MRN 25ESG5G000000749Y0 al que se le asignó un circuito naranja y ampara el movimiento de mercancías en depósito temporal entre los ADT ES009998DDDD02 y ES009999000002.
 
La notificación de recepción se produjo el 24-04-2025 a las 15:11:48 h. y el despacho de recepción el 24-04-2025 a las 15:11:48 h..
 
Puede consultar los detalles del G5 en el siguiente enlace de la sede electrónica de la AEAT:
 
https://urldefense.com/v3/__https://preintranet.dit.aeat/wlpl/ADDS-JDIT/CtrlG5gSede?op=DDS&tipDec=R&mrn=25ESG5G000000749Y0__;!!Na5NE8kfbMIR6Ys!qSdZ-jMkATJ6Z9hyzPKUAfuZpqvHMoRB536iLSOGlVPJeu6Zop1Bj8Ra02hNs9ZziqivynGJrt4Z44DxK0TtuwzmlGpBlbTxUSUZKpDfzz9Z$
 
Con la información declarada en el G5 de recepción y las posibles modificaciones efectuadas por la aduana durante la gestión del despacho de recepción se ha generado automáticamente un G4 en la aduana de destino 9999 - Pruebas al que se ha asignado el MRN 25ESG4A000000263U0. La mercancía se encuentra disponible para su datado mediante las declaraciones aduaneras oportunas.
 
Por favor, no responda a este mensaje. Se trata de un envío automatizado desde una dirección de correo no atendida.");

		return result;
	}

	TestEdiMessage GetNewMessageDeclarationAESCorrect(ZString appCode, ZGuid branchPK, ZString messageType, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationAESTestFilePath, "AcceptedGreenCircuitLMessageAEAT.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageAnnexAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexAESTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAnnexes;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageComplXAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXAESTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.TypeXExportUcc6;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDeclarationDVDCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationDVDTestFilePath, "AcceptedGreenCircuitMessageAEAT.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.DvdH2;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageQueryAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryAESTestFilePath, "AcceptedStatusPAGreenCircuitMessageAEAT.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportQuery;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationNCTSDepartureclearance(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureClearanceNCTSTestFilePath, "ComunicaLevantePar.txt");

		var interchangeIDNcts1 = new ZGuid("207B01E7-244F-47AA-B6DD-EE4F9318657C");

		nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: applicationReference));

		CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "22ES000101500647K2", ZString.Empty);

		var sentInterchangeNcts = CreateSentInterchange(interchangeIDNcts1);
		CreateSentMessage(nctsHeader, sentInterchangeNcts);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, interchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationNCTSDepartureControl(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureControlNCTSTestFilePath, "AcceptedMessage.txt");

		var interchangeIDNcts1 = new ZGuid("5C383DC2-AA74-41C5-B88D-02CA89F54565");

		nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: applicationReference));

		CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "22ES000101500647K1", ZString.Empty);

		var sentInterchangeNcts = CreateSentInterchange(interchangeIDNcts1);
		CreateSentMessage(nctsHeader, sentInterchangeNcts);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxNotificationNctsControls;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, interchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxDissatisfiedItemNCTS(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureDissatisfiedItemNCTSTestFilePath, "AcceptedMessage.txt");

		var interchangeIDNcts1 = new ZGuid("14040258-9189-40FE-8A38-8903D036F10A");

		nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: applicationReference));

		CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "22ES000101500647K2", ZString.Empty);

		var sentInterchangeNcts = CreateSentInterchange(interchangeIDNcts1);
		CreateSentMessage(nctsHeader, sentInterchangeNcts);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, interchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxInvalidTransitNCTS(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureInvalidateTransitNCTSTestFilePath, "AcceptedMessage.txt");

		var interchangeIDNcts1 = new ZGuid("796B476F-4CBE-4615-83D6-0D4B8ECE513E");

		nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: applicationReference));

		CreateOrUpdateCusEntryNumber(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, "22ES000101500540K2", ZString.Empty);

		var sentInterchangeNcts = CreateSentInterchange(interchangeIDNcts1);
		CreateSentMessage(nctsHeader, sentInterchangeNcts);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, interchangeIDNcts1, interchangeTransportType);

		return result;
	}

	protected CusEntryNumber CreateOrUpdateCusEntryNumber(BusinessObject header, ZString entryType, ZString entryNum, ZString entryStatus)
	{
		var newEntryNumber = CusEntryNumber.LoadOrCreate(header, entryType, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = entryNum;
		newEntryNumber.CE_EntryStatus = entryStatus;
		newEntryNumber.CE_EntryIsSystemGenerated = true;

		return newEntryNumber;
	}

	TestEdiMessage GetNewMessageInboxNotificationClearanceAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationClearanceAESTestFilePath, "AcceptedMessage.txt");

		var interchangeIDEntry = new ZGuid("72228BBB-6847-49DD-80E1-A1F9B8766E09");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("22ES000101100047B1");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportClearanceCommunication;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationInvalidationAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationAESTestFilePath, "AcceptedMessageInvalidatedByCustoms.txt");

		var interchangeIDEntry = new ZGuid("9A0C2516-5CD5-45E9-AC6A-C5023E8F7367");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("21ES000101100342B6");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportInvalidationCommunication;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationNonConformityAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationNonConformityAESTestFilePath, "AcceptedMessage.txt");

		var interchangeIDEntry = new ZGuid("206F3640-1449-4551-AFE3-0C9A93835549");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("22ES000101100871B9");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportNonConformityCommunication;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationExitResultAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationExitResultAESTestFilePath, "AcceptedMessage.txt");

		var interchangeIDEntry = new ZGuid("CA9651D1-9F6B-4F9D-95A7-E76606EB9EFF");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("22ES000101100050B2");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportExitResultCommunication;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationCceControlAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationCceControlAESTestFilePath, "AcceptedMessage.txt");

		var interchangeIDEntry = new ZGuid("DE516D3E-9EDA-4009-8A9C-639E45156B18");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("22ES000101100044B4");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportCceControlCommunication;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationExitNonConformityAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EALInboxNotificationExitNonConformityAESTestFilePath, "AcceptedMessage.txt");

		var interchangeIDReport = new ZGuid("65CF5A8A-B5D3-486F-84A5-FD002D2E9770");

		var report = Factory.Load(CusExitReportSchema.Constants.Prefix, ProcessorTestHelper.CreateCusExitReport(clusterKey: 2, referenceNum: "E00000002", mrn: "22ES000101100171B8"));

		var sentInterchangeReport = CreateSentInterchange(interchangeIDReport);
		CreateSentMessage(report, sentInterchangeReport);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDReport, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationExitClearanceAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EALInboxNotificationExitClearanceAESTestFilePath, "AcceptedMessage.txt");

		var interchangeIDReport = new ZGuid("5FD67277-CB45-40B1-9771-7D8D357ADBFE");

		var report = Factory.Load(CusExitReportSchema.Constants.Prefix, ProcessorTestHelper.CreateCusExitReport(clusterKey: 3, referenceNum: "E00000003", mrn: "22ES000101100171B9"));

		var sentInterchangeReport = CreateSentInterchange(interchangeIDReport);
		CreateSentMessage(report, sentInterchangeReport);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportExitClearanceNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDReport, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCancelAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelAESTestFilePath, "AcceptedMessageCancellationOK.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportCancellation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDeclarationAESAmendmentCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentAESTestFilePath, "AcceptedAndClearedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ExportAmendmentUcc6;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationDVDCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotifDVDTestFilePath, "AcceptedGreenCircuitMessageAEAT.txt");

		var interchangeIDEntry = new ZGuid("0B1D0665-4867-4B12-859C-243A1DD1B208");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("22ES009999D04136R3");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCancelDVDCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelDVDTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.DvdH2Cancellation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDepartureCertReqAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureCertReqAESTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.RequestExportExitCertificate;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageComplXDVDCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ComplXDVDTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.TypeXDvdH2;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageCancelNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancelNCTSTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageDepartureNCTSCorrect(ZString appCode, ZGuid branchPK, ZString messageType, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DepartureNCTSTestFilePath, "AcceptedMessageGreenCircuitL.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageArrivalNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ArrivalNCTSTestFilePath, "AcceptedMessageGreenResponseCodeR.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts4, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNotifGoodsNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotifGoodsNCTSTestFilePath, "AcceptedMessageGreenCircuitL.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureNotification;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageAmendmentNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AmendmentNCTSTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageQueryNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryNCTSTestFilePath, "AcceptedMessageStatusPA.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.TransitNcts5Query;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageAnnexNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexNCTSTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts1, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageNotificationUnloadingNCTSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.NotificationUnloadingNCTSTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDNcts4, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageQueryDVDCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryDVDTestFilePath, "AcceptedGreenCircuitMessageNoGuarantees.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.DvdH2Query;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageEALAESCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.EALAESTestFilePath, "AcceptedGreenCircuitLMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDExitReport, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageQueryT2LPOUSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSQueryTestFilePath, "AcceptedMessageGreenCircuit.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lQueryPous;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageAnnexT2LPOUSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSAnnexTestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lDocumentationPous;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageRequestAndReceptionT2LPOUSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType, ZString messageType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSRequestAndReceptionTestFilePath, "AcceptedMessageGreenCircuit.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = messageType;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessagePresentationT2LPOUSCorrect(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.T2LPOUSPresentationTestFilePath, "AcceptedMessageGreenCircuit.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.T2lPresentationPous;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageG3Declaration(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationG3TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G3DeclarationOfGoods;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDG3, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageG3Revoke(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.RevokeG3TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G3RevocationOfGoods;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDG3, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageExpeditionG5Correct(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpeditionG5TestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G5v1Expedition;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDTmpStorage, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageExpAmendmentG5Correct(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpAmendmentG5TestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDTmpStorage, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageReceptionG5Correct(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReceptionG5TestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G5v1Reception;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDTmpStorage, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageExpCancelG5Correct(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ExpCancelG5TestFilePath, "AcceptedMessage.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileCircuit;

		CreateResponseInterchange(result, InterchangeIDTmpStorage, interchangeTransportType);

		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
	TestEdiMessage GetNewMessageInboxPendingListCorrectForEntryHeader(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "NPEMessageSingleElement.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxPendingList;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxPendingListCorrectForCusExitReport(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		string acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxPendingListTestFilePath, "LVSMessageSingleElement.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.InboxPendingList;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDExitReport, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageDeclarationH7(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.DeclarationH7TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.H7Declaration;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDH7, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageAnnexH7(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.AnnexH7TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.H7Annexes;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDH7, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageReexportH7(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.ReexportH7TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.H7ReExport;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDH7, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageCancellationH7(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.CancellationH7TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.H7Cancellation;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDH7, interchangeTransportType);

		return result;
	}

	protected TestEdiMessage GetNewMessageQueryH7(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.QueryH7TestFilePath, "AcceptedMessage.xml");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.H7Query;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, InterchangeIDH7, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageIncompleteImportH1Correct(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileGreenCircuit = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.IncompleteImportH1TestFilePath, "AcceptedMessageOperationRegistered10.txt");

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileGreenCircuit;

		CreateResponseInterchange(result, InterchangeIDEntry, interchangeTransportType);

		return result;
	}

	TestEdiMessage GetNewMessageInboxNotificationInvalidationH1Correct(ZString appCode, ZGuid branchPK, ZString applicationReference, ZString interchangeTransportType)
	{
		var acceptanceTestFileContent = ESTestFileReader.GetEmbeddedFileText(MessageProcessorTestFileConstants.InboxNotificationInvalidationH1TestFilePath, "AcceptedMessageInvalidatedByCustomsWithMRN.txt");

		var interchangeIDEntry = new ZGuid("DFD1A27C-1FA8-4516-A567-2B68915428B7");

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.B;
		var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.CH_BGMReference = applicationReference;

		entryHeader.MovementReferenceNumberSetter("24ES009999I001H2R9");

		var sentInterchangeEntry = CreateSentInterchange(interchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		var result = Factory.New<TestEdiMessage>();
		result.EM_ApplicationCode = appCode;
		result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		result.EM_GB = branchPK;
		result.EM_Status = EDIMessage.Status.Queued;
		result.EM_MessageType = DeclarationMessageTypeList.Codes.ImportH1InvalidationCommunication;
		result.EM_ApplicationReference = applicationReference;
		result.EM_MessageText = acceptanceTestFileContent;

		CreateResponseInterchange(result, interchangeIDEntry, interchangeTransportType);

		return result;
	}

	void AssertProcessed(ZString processorType, TestEdiMessage message, bool shouldHaveBeenProcessed, bool processError, bool preProcessed)
	{
		var messageInNewFactory = new BusinessObjectFactory().Load<TestEdiMessage>(message.PK);
		if (shouldHaveBeenProcessed)
		{
			if (processError)
			{
				AssertEquals(processorType + " - Message should have been selected and processed but returned failed", EDIMessage.Status.Failed, messageInNewFactory.EM_Status);
			}
			else if (preProcessed)
			{
				AssertEquals(processorType + " - Message should have been pre processed correctly", EDIMessage.Status.PreProcessedOK, messageInNewFactory.EM_Status);
			}
			else
			{
				AssertEquals(processorType + " - Message should have been selected and processed correctly", EDIMessage.Status.Received, messageInNewFactory.EM_Status);
			}
		}
		else
		{
			AssertEquals(processorType + " - Message should NOT have been selected and processed", EDIMessage.Status.Queued, messageInNewFactory.EM_Status);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
		staff = staffWithCertificateHelperTest.Staff;
		certificate = staffWithCertificateHelperTest.Certificate;

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = Business.Declaration.EntrySubStyleList.Codes.A;
		entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryHeader.MovementReferenceNumberSetter("20ES00999930006184", ZDateTime.Today);

		var sentInterchangeEntry = CreateSentInterchange(InterchangeIDEntry);
		CreateSentMessage(entryHeader, sentInterchangeEntry);

		nctsHeader = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: "NCD00000001", brokerCode: staff.GS_Code, certName: certificate.CertificateName));

		var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
		newEntryNumber.CE_EntryNum = "20ES00999830001288";
		newEntryNumber.CE_IssueDate = ZDateTime.Today;
		newEntryNumber.CE_EntryIsSystemGenerated = true;
		var sentInterchangeNcts = CreateSentInterchange(InterchangeIDNcts1);
		CreateSentMessage(nctsHeader, sentInterchangeNcts);

		var nctsHeader2 = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "D", referenceNum: "NCT00000001"));
		var sentInterchangeNcts2 = CreateSentInterchange(InterchangeIDNcts2);
		CreateSentMessage(nctsHeader2, sentInterchangeNcts2);

		var nctsHeader3 = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "A", referenceNum: "NCA00000001"));
		var sentInterchangeNcts3 = CreateSentInterchange(InterchangeIDNcts3);
		CreateSentMessage(nctsHeader3, sentInterchangeNcts3);

		nctsHeader4 = Factory.Load(CusInBondHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateNctsHeader(headerType: "A", referenceNum: "NCA00000002"));
		var sentInterchangeNcts4 = CreateSentInterchange(InterchangeIDNcts4);
		CreateSentMessage(nctsHeader4, sentInterchangeNcts4);

		var exitControlHeader = Factory.New<Declaration.CusExitControlHeader>();
		exitControlHeader.CEH_ParentID = declaration.PK;
		exitControlHeader.CEH_ParentTableCode = "JE";
		exitControlHeader.CEH_ReferenceNumber = "CEH001";
		var exitDetail = exitControlHeader.CusExitDetails.AddNew();
		exitDetail.CED_MovementReferenceNumber = "AAE00000001";

		var sentInterchangeExitDetail = CreateSentInterchange(InterchangeIDExitDetail);
		CreateSentMessage(exitDetail, sentInterchangeExitDetail);

		var report = Factory.Load(CusExitReportSchema.Constants.Prefix, ProcessorTestHelper.CreateCusExitReport(clusterKey: 1, referenceNum: "E00000001", mrn: "CCE00000001"));

		var sentInterchangeExitReport = CreateSentInterchange(InterchangeIDExitReport);
		CreateSentMessage(report, sentInterchangeExitReport);

		temporaryStorage = Factory.New<TemporaryStorageHeader>();
		temporaryStorage.AMA_JobReference = "TS00001";
		temporaryStorage.AMA_MessageType = "G5R";
		temporaryStorage.MRN = "25ESG5G000000749Y0";
		var sentInterchangeTmpStorage = CreateSentInterchange(InterchangeIDTmpStorage);
		CreateSentMessage(temporaryStorage, sentInterchangeTmpStorage);

		var bill = Factory.Load(AsycudaBillSchema.Constants.Prefix, ProcessorTestHelper.CreateAsycudaBill(referenceNum: "ABL000001", brokerCode: staff.GS_Code, certName: certificate.CertificateName));
		var sentInterchangeH7 = CreateSentInterchange(InterchangeIDH7);
		CreateSentMessage(bill, sentInterchangeH7);

		var header = Factory.Load(AsycudaManifestHeaderSchema.Constants.Prefix, ProcessorTestHelper.CreateAsycudaManifestHeader(clusterKey: 4, jobReference: "H7D0000002", brokerCode: staff.GS_Code, certName: certificate.CertificateName));
		var sentInterchangeG3 = CreateSentInterchange(InterchangeIDG3);
		CreateSentMessage(header, sentInterchangeG3);

		processor = new ESBranchCustomsMessageProcessor();

		Factory.Save();
		entryHeader.CH_BGMReference = "S900052890/1";
		Factory.Save();
	}

	ESBranchCustomsMessageProcessor processor;
	CusEntryHeader entryHeader;
	BusinessObject nctsHeader;
	BusinessObject nctsHeader4;
	JobDeclaration declaration;
	TemporaryStorageHeader temporaryStorage;

	protected virtual ZGuid InterchangeIDEntry => new ZGuid("5E5A9120-1794-4251-86AC-8038077F5BA6");
	protected virtual ZGuid InterchangeIDNcts1 => new ZGuid("FAB35AC1-2FE2-40E2-9D1B-DFA5BAF6A130");
	protected virtual ZGuid InterchangeIDNcts2 => new ZGuid("A947EDF9-C25A-4A0C-BFAE-178E051028A1");
	protected virtual ZGuid InterchangeIDNcts3 => new ZGuid("3A72B84D-A646-40B2-9C81-D1FEB4659119");
	protected virtual ZGuid InterchangeIDNcts4 => new ZGuid("94CFA4CA-C6A8-437C-9B89-07814BEAE44E");
	protected virtual ZGuid InterchangeIDExitDetail => new ZGuid("8EB99744-47CB-4762-9C2D-0DCE1AD7AF40");
	protected virtual ZGuid InterchangeIDExitReport => new ZGuid("8A7B10F5-E134-4E4D-8096-09FAA5A378F6");
	protected virtual ZGuid InterchangeIDTmpStorage => new ZGuid("1E3C5E49-1366-4B7E-BEFB-38225799D33A");
	protected virtual ZGuid InterchangeIDG3 => new ZGuid("CEE8F1C1-0780-45EE-8EDC-93E83179AE65");
	protected virtual ZGuid InterchangeIDH7 => new ZGuid("9DF12B82-D42B-4D97-95E7-83B55B6F6E5C");

	void CreateSentMessage(BusinessObject linkedObject, EDIInterchange sentInterchange)
	{
		var sentMessage = Factory.New<TestEdiMessage>();
		sentMessage.EM_LinkedObject = linkedObject;
		sentMessage.EM_ApplicationReference = certificate.CertificateName;
		sentInterchange.ContainedMessages.Add(sentMessage);
	}

	EDIInterchange CreateSentInterchange(ZGuid sessionGuid)
	{
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.EI_SessionGUID = sessionGuid;
		sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentInterchange.EI_From = "CW1";
		sentInterchange.EI_To = "ESCustoms";
		sentInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>{0}</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>", certificate.CertificateName);

		return sentInterchange;
	}

	EDIInterchange CreateResponseInterchange(TestEdiMessage message, ZGuid sessionGuid, ZString interchangeTransportType)
	{
		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = sessionGuid;
		responseInterchange.EI_From = "ESCustoms";
		responseInterchange.EI_To = "CW1";
		responseInterchange.EI_TransportType = interchangeTransportType;
		if (interchangeTransportType != EDIInterchange.TransportType.xT)
		{
			responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>{0}</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>N</TestMessage>
  <SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>", certificate.CertificateName);
		}
		responseInterchange.ContainedMessages.Add(message);
		return responseInterchange;
	}

	CertificateProviderTestClass certificate;
	GlbStaff staff;

	class ESBranchCustomsMessageProcessorForTesting : ESBranchCustomsMessageProcessor
	{
		internal List<ApplicationTypeMessageProcessor> GetMessageProcessorsCoreExposed() => GetMessageProcessorsCore();
	}
}
