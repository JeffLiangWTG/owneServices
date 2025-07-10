using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Utils = Enterprise.Customs.ES.Business.MessageSending.ESInterchangeProviderUtilities;

namespace Enterprise.Customs.ES.Business.MessageSending.Testing;

public class ESInterchangeProviderUtilitiesTest : TestCaseWithFactory
{
	public void TestGetInterchangeReceiver()
	{
		CombineAssertions(() =>
		{
			var exportReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Export);
			AssertEquals("For Export messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms, exportReceiver);

			var importAmendmentBox40Receiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAmendmentBox40);
			AssertEquals("For ImportAmendmentBox40 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, importAmendmentBox40Receiver);

			var importAmendmentBox40ReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAmendmentBox40, isXT: true, isTest: true);
			AssertEquals("For ImportAmendmentBox40 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, importAmendmentBox40ReceiverXTTest);

			var importAmendmentBox40ReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAmendmentBox40, isXT: true, isTest: false);
			AssertEquals("For ImportAmendmentBox40 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, importAmendmentBox40ReceiverXTPro);

			var exportUcc6Receiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertEquals("For ExportUcc6 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, exportUcc6Receiver);

			var exportUcc6ReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportUcc6, isXT: true, isTest: true);
			AssertEquals("For ExportUcc6 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, exportUcc6ReceiverXTTest);

			var exportUcc6ReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportUcc6, isXT: true, isTest: false);
			AssertEquals("For ExportUcc6 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, exportUcc6ReceiverXTPro);

			var dvdReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.DvdH2);
			AssertEquals("For DvdH2 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, dvdReceiver);

			var dvdReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.DvdH2, isXT: true, isTest: true);
			AssertEquals("For DvdH2 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, dvdReceiverXTTest);

			var dvdReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.DvdH2, isXT: true, isTest: false);
			AssertEquals("For DvdH2 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, dvdReceiverXTPro);

			var ncts5ArrivalDownloadGoodsReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods);
			AssertEquals("For Ncts5ArrivalDownloadGoods messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5ArrivalDownloadGoodsReceiver);

			var ncts5ArrivalDownloadGoodsReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, isXT: true, isTest: true);
			AssertEquals("For Ncts5ArrivalDownloadGoods messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5ArrivalDownloadGoodsReceiverXTTest);

			var ncts5ArrivalDownloadGoodsReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods, isXT: true, isTest: false);
			AssertEquals("For Ncts5ArrivalDownloadGoods messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5ArrivalDownloadGoodsReceiverXTPro);

			var ncts5ArrivalNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification);
			AssertEquals("For Ncts5ArrivalNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5ArrivalNotificationReceiver);

			var ncts5ArrivalNotificationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, isXT: true, isTest: true);
			AssertEquals("For Ncts5ArrivalNotification messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5ArrivalNotificationReceiverXTTest);

			var ncts5ArrivalNotificationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification, isXT: true, isTest: false);
			AssertEquals("For Ncts5ArrivalNotification messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5ArrivalNotificationReceiverXTPro);

			var ncts5DepartureReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5Departure);
			AssertEquals("For Ncts5Departure messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5DepartureReceiver);

			var ncts5DepartureReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5Departure, isXT: true, isTest: true);
			AssertEquals("For Ncts5Departure messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5DepartureReceiverXTTest);

			var ncts5DepartureReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5Departure, isXT: true, isTest: false);
			AssertEquals("For Ncts5Departure messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5DepartureReceiverXTPro);

			var ncts5DepartureAmendmentReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment);
			AssertEquals("For Ncts5DepartureAmendment messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5DepartureAmendmentReceiver);

			var ncts5DepartureAmendmentReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, isXT: true, isTest: true);
			AssertEquals("For Ncts5DepartureAmendment messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5DepartureAmendmentReceiverXTTest);

			var ncts5DepartureAmendmentReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureAmendment, isXT: true, isTest: false);
			AssertEquals("For Ncts5DepartureAmendment messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5DepartureAmendmentReceiverXTPro);

			var ncts5DepartureAnnexesReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes);
			AssertEquals("For Ncts5DepartureAnnexes messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5DepartureAnnexesReceiver);

			var ncts5DepartureAnnexesReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, isXT: true, isTest: true);
			AssertEquals("For Ncts5DepartureAnnexes messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5DepartureAnnexesReceiverXTTest);

			var ncts5DepartureAnnexesReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes, isXT: true, isTest: false);
			AssertEquals("For Ncts5DepartureAnnexes messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5DepartureAnnexesReceiverXTPro);

			var ncts5DepartureCancellationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation);
			AssertEquals("For Ncts5DepartureCancellation messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5DepartureCancellationReceiver);

			var ncts5DepartureCancellationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, isXT: true, isTest: true);
			AssertEquals("For Ncts5DepartureCancellation messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5DepartureCancellationReceiverXTTest);

			var ncts5DepartureCancellationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation, isXT: true, isTest: false);
			AssertEquals("For Ncts5DepartureCancellation messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5DepartureCancellationReceiverXTPro);

			var ncts5DepartureNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification);
			AssertEquals("For Ncts5DepartureNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5DepartureNotificationReceiver);

			var ncts5DepartureNotificationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, isXT: true, isTest: true);
			AssertEquals("For Ncts5DepartureNotification messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5DepartureNotificationReceiverXTTest);

			var ncts5DepartureNotificationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DepartureNotification, isXT: true, isTest: false);
			AssertEquals("For Ncts5DepartureNotification messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5DepartureNotificationReceiverXTPro);

			var ncts5DeparturePreDeclarationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration);
			AssertEquals("For Ncts5DeparturePreDeclaration messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, ncts5DeparturePreDeclarationReceiver);

			var ncts5DeparturePreDeclarationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, isXT: true, isTest: true);
			AssertEquals("For Ncts5DeparturePreDeclaration messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, ncts5DeparturePreDeclarationReceiverXTTest);

			var ncts5DeparturePreDeclarationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5DeparturePreDeclaration, isXT: true, isTest: false);
			AssertEquals("For Ncts5DeparturePreDeclaration messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, ncts5DeparturePreDeclarationReceiverXTPro);

			var transitNcts5QueryReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.TransitNcts5Query);
			AssertEquals("For TransitNcts5Query messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, transitNcts5QueryReceiver);

			var transitNcts5QueryReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.TransitNcts5Query, isXT: true, isTest: true);
			AssertEquals("For TransitNcts5Query messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, transitNcts5QueryReceiverXTTest);

			var transitNcts5QueryReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.TransitNcts5Query, isXT: true, isTest: false);
			AssertEquals("For TransitNcts5Query messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, transitNcts5QueryReceiverXTPro);

			var tnnNcts5QueryReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration);
			AssertEquals("For Ncts5IndirectDepartureRegistration messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, tnnNcts5QueryReceiver);

			var tnnNcts5QueryReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration, isXT: true, isTest: true);
			AssertEquals("For Ncts5IndirectDepartureRegistration messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, tnnNcts5QueryReceiverXTTest);

			var tnnNcts5QueryReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration, isXT: true, isTest: false);
			AssertEquals("For Ncts5IndirectDepartureRegistration messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, tnnNcts5QueryReceiverXTPro);

			var t2lRequestPousReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lRequestPous);
			AssertEquals("For T2lRequestPousReceiver messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, t2lRequestPousReceiver);

			var t2lRequestPousReceiverReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lRequestPous, isXT: true, isTest: true);
			AssertEquals("For T2lRequestPousReceiver messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, t2lRequestPousReceiverReceiverXTTest);

			var t2lRequestPousReceiverReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lRequestPous, isXT: true, isTest: false);
			AssertEquals("For T2lRequestPousReceiver messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, t2lRequestPousReceiverReceiverXTPro);

			var t2lPresentationPousReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lPresentationPous);
			AssertEquals("For T2lPresentationPous messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, t2lPresentationPousReceiver);

			var t2lPresentationPousReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lPresentationPous, isXT: true, isTest: true);
			AssertEquals("For T2lPresentationPous messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, t2lPresentationPousReceiverXTTest);

			var t2lPresentationPousReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lPresentationPous, isXT: true, isTest: false);
			AssertEquals("For T2lPresentationPous messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, t2lPresentationPousReceiverXTPro);

			var t2lDocumentationPousReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lDocumentationPous);
			AssertEquals("For T2lDocumentationPous messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, t2lDocumentationPousReceiver);

			var t2lDocumentationPousReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lDocumentationPous, isXT: true, isTest: true);
			AssertEquals("For T2lDocumentationPous messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, t2lDocumentationPousReceiverXTTest);

			var t2lDocumentationPousReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lDocumentationPous, isXT: true, isTest: false);
			AssertEquals("For T2lDocumentationPous messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, t2lDocumentationPousReceiverXTPro);

			var t2lQueryPousReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lQueryPous);
			AssertEquals("For T2lQueryPous messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, t2lQueryPousReceiver);

			var t2lQueryPousReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lQueryPous, isXT: true, isTest: true);
			AssertEquals("For T2lQueryPous messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, t2lQueryPousReceiverXTTest);

			var t2lQueryPousReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lQueryPous, isXT: true, isTest: false);
			AssertEquals("For T2lQueryPous messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, t2lQueryPousReceiverXTPro);

			var t2lReceptionPousReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lReceptionPous);
			AssertEquals("For T2lQueryPous messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, t2lReceptionPousReceiver);

			var t2lReceptionPousReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lReceptionPous, isXT: true, isTest: true);
			AssertEquals("For T2lReceptionPous messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, t2lReceptionPousReceiverXTTest);

			var t2lReceptionPousReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.T2lReceptionPous, isXT: true, isTest: false);
			AssertEquals("For T2lReceptionPous messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, t2lReceptionPousReceiverXTPro);

			var g3DeclarationOfGoodsReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G3DeclarationOfGoods);
			AssertEquals("For G3DeclarationOfGoodsReceiver messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, g3DeclarationOfGoodsReceiver);

			var g3DeclarationOfGoodsReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G3DeclarationOfGoods, isXT: true, isTest: true);
			AssertEquals("For G3DeclarationOfGoodsReceiver messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, g3DeclarationOfGoodsReceiverXTTest);

			var g3DeclarationOfGoodsReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G3DeclarationOfGoods, isXT: true, isTest: false);
			AssertEquals("For G3DeclarationOfGoodsReceiver messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, g3DeclarationOfGoodsReceiverXTPro);

			var g3RevocationOfGoodsReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G3RevocationOfGoods);
			AssertEquals("For g3RevocationOfGoodsReceiver messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, g3RevocationOfGoodsReceiver);

			var g3RevocationOfGoodsReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G3RevocationOfGoods, isXT: true, isTest: true);
			AssertEquals("For g3RevocationOfGoodsReceiver messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, g3RevocationOfGoodsReceiverXTTest);

			var g3RevocationOfGoodsReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G3RevocationOfGoods, isXT: true, isTest: false);
			AssertEquals("For g3RevocationOfGoodsReceiver messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, g3RevocationOfGoodsReceiverXTPro);

			var g5ExpeditionReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1Expedition);
			AssertEquals("For G5v1Expedition messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, g5ExpeditionReceiver);

			var g5ExpeditionReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1Expedition, isXT: true, isTest: true);
			AssertEquals("For G5v1Expedition messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, g5ExpeditionReceiverXTTest);

			var g5ExpeditionReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1Expedition, isXT: true, isTest: false);
			AssertEquals("For G5v1Expedition messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, g5ExpeditionReceiverXTPro);

			var g5ExpeditionAmendmentReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment);
			AssertEquals("For G5v1ExpeditionAmendment messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, g5ExpeditionAmendmentReceiver);

			var g5ExpeditionAmendmentReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment, isXT: true, isTest: true);
			AssertEquals("For G5v1ExpeditionAmendment messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, g5ExpeditionAmendmentReceiverXTTest);

			var g5ExpeditionAmendmentReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment, isXT: true, isTest: false);
			AssertEquals("For G5v1ExpeditionAmendment messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, g5ExpeditionAmendmentReceiverXTPro);

			var g5ExpeditionCancellationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation);
			AssertEquals("For G5v1ExpeditionCancellation messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, g5ExpeditionCancellationReceiver);

			var g5ExpeditionCancellationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation, isXT: true, isTest: true);
			AssertEquals("For G5v1ExpeditionCancellation messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, g5ExpeditionCancellationReceiverXTTest);

			var g5ExpeditionCancellationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation, isXT: true, isTest: false);
			AssertEquals("For G5v1ExpeditionCancellation messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, g5ExpeditionCancellationReceiverXTPro);

			var g5ReceptionReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1Reception);
			AssertEquals("For G5v1Reception messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, g5ReceptionReceiver);

			var g5ReceptionReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1Reception, isXT: true, isTest: true);
			AssertEquals("For G5v1Reception messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, g5ReceptionReceiverXTTest);

			var g5ReceptionReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.G5v1Reception, isXT: true, isTest: false);
			AssertEquals("For G5v1Reception messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, g5ReceptionReceiverXTPro);

			var h1Box44DocumentsReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Box44DocumentsH1);
			AssertEquals("For Box44DocumentsH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1Box44DocumentsReceiver);

			var h1Box44DocumentsReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Box44DocumentsH1, isXT: true, isTest: true);
			AssertEquals("For Box44DocumentsH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1Box44DocumentsReceiverXTTest);

			var h1Box44DocumentsReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.Box44DocumentsH1, isXT: true, isTest: false);
			AssertEquals("For Box44DocumentsH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1Box44DocumentsReceiverXTPro);

			var h1ImportActivationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportActivationH1);
			AssertEquals("For ImportActivationH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportActivationReceiver);

			var h1ImportActivationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportActivationH1, isXT: true, isTest: true);
			AssertEquals("For ImportActivationH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportActivationReceiverXTTest);

			var h1ImportActivationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportActivationH1, isXT: true, isTest: false);
			AssertEquals("For ImportActivationH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportActivationReceiverXTPro);

			var h1ImportAmendmentBox40Receiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1);
			AssertEquals("For ImportAmendmentBox40H1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportAmendmentBox40Receiver);

			var h1ImportAmendmentBox40ReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1, isXT: true, isTest: true);
			AssertEquals("For ImportAmendmentBox40H1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportAmendmentBox40ReceiverXTTest);

			var h1ImportAmendmentBox40ReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1, isXT: true, isTest: false);
			AssertEquals("For ImportAmendmentBox40H1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportAmendmentBox40ReceiverXTPro);

			var h1ImportAnnexReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAnnexH1);
			AssertEquals("For ImportAnnexH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportAnnexReceiver);

			var h1ImportAnnexReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAnnexH1, isXT: true, isTest: true);
			AssertEquals("For ImportAnnexH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportAnnexReceiverXTTest);

			var h1ImportAnnexReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportAnnexH1, isXT: true, isTest: false);
			AssertEquals("For ImportAnnexH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportAnnexReceiverXTPro);

			var h1ImportCompleteActiveReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportCompleteActiveH1);
			AssertEquals("For ImportCompleteActiveH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportCompleteActiveReceiver);

			var h1ImportCompleteActiveReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportCompleteActiveH1, isXT: true, isTest: true);
			AssertEquals("For ImportCompleteActiveH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportCompleteActiveReceiverXTTest);

			var h1ImportCompleteActiveReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportCompleteActiveH1, isXT: true, isTest: false);
			AssertEquals("For ImportCompleteActiveH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportCompleteActiveReceiverXTPro);

			var h1ImportCompletePendingActivationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportCompletePendingActivationH1);
			AssertEquals("For ImportCompletePendingActivationH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportCompletePendingActivationReceiver);

			var h1ImportCompletePendingActivationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportCompletePendingActivationH1, isXT: true, isTest: true);
			AssertEquals("For ImportCompletePendingActivationH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportCompletePendingActivationReceiverXTTest);

			var h1ImportCompletePendingActivationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportCompletePendingActivationH1, isXT: true, isTest: false);
			AssertEquals("For ImportCompletePendingActivationH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportCompletePendingActivationReceiverXTPro);

			var h1ImportQueryReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportH1Query);
			AssertEquals("For ImportH1Query messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportQueryReceiver);

			var h1ImportQueryReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportH1Query, isXT: true, isTest: true);
			AssertEquals("For ImportH1Query messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportQueryReceiverXTTest);

			var h1ImportQueryReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportH1Query, isXT: true, isTest: false);
			AssertEquals("For ImportH1Query messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportQueryReceiverXTPro);

			var h1ImportIncompletePreDeclarationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1);
			AssertEquals("For ImportIncompletePreDeclarationH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportIncompletePreDeclarationReceiver);

			var h1ImportIncompletePreDeclarationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, isXT: true, isTest: true);
			AssertEquals("For ImportIncompletePreDeclarationH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportIncompletePreDeclarationReceiverXTTest);

			var h1ImportIncompletePreDeclarationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1, isXT: true, isTest: false);
			AssertEquals("For ImportIncompletePreDeclarationH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportIncompletePreDeclarationReceiverXTPro);

			var h1ImportSimplifiedActiveReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1);
			AssertEquals("For ImportSimplifiedActiveH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportSimplifiedActiveReceiver);

			var h1ImportSimplifiedActiveReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1, isXT: true, isTest: true);
			AssertEquals("For ImportSimplifiedActiveH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportSimplifiedActiveReceiverXTTest);

			var h1ImportSimplifiedActiveReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1, isXT: true, isTest: false);
			AssertEquals("For ImportSimplifiedActiveH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportSimplifiedActiveReceiverXTPro);

			var h1ImportSimplifiedPendingActivationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportSimplifiedPendingActivationH1);
			AssertEquals("For ImportSimplifiedPendingActivationH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1ImportSimplifiedPendingActivationReceiver);

			var h1ImportSimplifiedPendingActivationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportSimplifiedPendingActivationH1, isXT: true, isTest: true);
			AssertEquals("For ImportSimplifiedPendingActivationH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1ImportSimplifiedPendingActivationReceiverXTTest);

			var h1ImportSimplifiedPendingActivationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ImportSimplifiedPendingActivationH1, isXT: true, isTest: false);
			AssertEquals("For ImportSimplifiedPendingActivationH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1ImportSimplifiedPendingActivationReceiverXTPro);

			var h1PendingSupportingDocumentReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.PendingSupportingDocumentH1);
			AssertEquals("For PendingSupportingDocumentH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1PendingSupportingDocumentReceiver);

			var h1PendingSupportingDocumentReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.PendingSupportingDocumentH1, isXT: true, isTest: true);
			AssertEquals("For PendingSupportingDocumentH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1PendingSupportingDocumentReceiverXTTest);

			var h1PendingSupportingDocumentReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.PendingSupportingDocumentH1, isXT: true, isTest: false);
			AssertEquals("For PendingSupportingDocumentH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1PendingSupportingDocumentReceiverXTPro);

			var h1PreDeclarationCancellationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.PreDeclarationCancellationH1);
			AssertEquals("For PreDeclarationCancellationH1 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.SoapSpanishCustomsForEHub, h1PreDeclarationCancellationReceiver);

			var h1PreDeclarationCancellationReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.PreDeclarationCancellationH1, isXT: true, isTest: true);
			AssertEquals("For PreDeclarationCancellationH1 messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt, h1PreDeclarationCancellationReceiverXTTest);

			var h1PreDeclarationCancellationReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.PreDeclarationCancellationH1, isXT: true, isTest: false);
			AssertEquals("For PreDeclarationCancellationH1 messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt, h1PreDeclarationCancellationReceiverXTPro);

			var ldiReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxPendingList, isTest: true);
			AssertEquals("For InboxPendingList messageType Interchange receiver is not empty and is Test", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, ldiReceiverXTTest);

			var ldiReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxPendingList, isTest: false);
			AssertEquals("For InboxPendingList messageType Interchange receiver is not empty and is not Test", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, ldiReceiverXTPro);

			var exportInboxNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InBoxNotificationForExport);
			AssertEquals("For InBoxNotificationForExport messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportInboxNotificationReceiver);

			var importInboxNotificationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InBoxNotificationForImport, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For InBoxNotificationForImport messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, importInboxNotificationReceiverInboxXTTest);

			var exportInboxNotificationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InBoxNotificationForExport, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For InBoxNotificationForExport messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportInboxNotificationReceiverInboxXTPro);

			var exportCceControlCommunicationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportCceControlCommunication);
			AssertEquals("For ExportCceControlCommunication messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportCceControlCommunicationReceiver);

			var exportCceControlCommunicationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportCceControlCommunication, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportCceControlCommunication messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportCceControlCommunicationReceiverInboxXTTest);

			var exportCceControlCommunicationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportCceControlCommunication, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportCceControlCommunication messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportCceControlCommunicationReceiverInboxXTPro);

			var exportNonConformityCommunicationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication);
			AssertEquals("For ExportNonConformityCommunication messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportNonConformityCommunicationReceiver);

			var exportNonConformityCommunicationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportNonConformityCommunication messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportNonConformityCommunicationReceiverInboxXTTest);

			var exportNonConformityCommunicationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportNonConformityCommunication, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportNonConformityCommunication messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportNonConformityCommunicationReceiverInboxXTPro);

			var exportInvalidationCommunicationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication);
			AssertEquals("For ExportInvalidationCommunication messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportInvalidationCommunicationReceiver);

			var exportInvalidationCommunicationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportInvalidationCommunication messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportInvalidationCommunicationReceiverInboxXTTest);

			var exportInvalidationCommunicationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportInvalidationCommunication, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportInvalidationCommunication messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportInvalidationCommunicationReceiverInboxXTPro);

			var exportClearanceCommunicationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportClearanceCommunication);
			AssertEquals("For ExportClearanceCommunication messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportClearanceCommunicationReceiver);

			var exportClearanceCommunicationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportClearanceCommunication, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportClearanceCommunication messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportClearanceCommunicationReceiverInboxXTTest);

			var exportClearanceCommunicationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportClearanceCommunication, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportClearanceCommunication messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportClearanceCommunicationReceiverInboxXTPro);

			var exportExitResultCommunicationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitResultCommunication);
			AssertEquals("For ExportExitResultCommunication messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportExitResultCommunicationReceiver);

			var exportExitResultCommunicationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitResultCommunication, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportExitResultCommunication messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportExitResultCommunicationReceiverInboxXTTest);

			var exportExitResultCommunicationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitResultCommunication, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportExitResultlCommunication messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportExitResultCommunicationReceiverInboxXTPro);

			var inboxNotificationForDvdH2Receiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2);
			AssertEquals("For InboxNotificationForDvdH2 messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, inboxNotificationForDvdH2Receiver);

			var inboxNotificationForDvdH2ReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For InboxNotificationForDvdH2 messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, inboxNotificationForDvdH2ReceiverInboxXTTest);

			var inboxNotificationForDvdH2ReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationForDvdH2, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For InboxNotificationForDvdH2 messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, inboxNotificationForDvdH2ReceiverInboxXTPro);

			var exportExitClearanceNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification);
			AssertEquals("For ExportExitClearanceNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportExitClearanceNotificationReceiver);

			var exportExitClearanceNotificationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportExitClearanceNotification messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportExitClearanceNotificationReceiverInboxXTTest);

			var exportExitClearanceNotificationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportExitClearanceNotification messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportExitClearanceNotificationReceiverInboxXTPro);

			var exportExitNonConformityNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification);
			AssertEquals("For ExportExitNonConformityNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, exportExitNonConformityNotificationReceiver);

			var exportExitNonConformityNotificationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For ExportExitNonConformityNotification messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, exportExitNonConformityNotificationReceiverInboxXTTest);

			var exportExitNonConformityNotificationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For ExportExitNonConformityNotification messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, exportExitNonConformityNotificationReceiverInboxXTPro);

			var nctsNonConformityNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture);
			AssertEquals("For NCTSNonConformityNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, nctsNonConformityNotificationReceiver);

			var nctsNonConformityNotificationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For NCTSNonConformityNotification messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, nctsNonConformityNotificationReceiverInboxXTTest);

			var nctsNonConformityNotificationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationForNonConformityNctsDeparture, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For NCTSNonConformityNotification messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, nctsNonConformityNotificationReceiverInboxXTPro);

			var nctsCceControlCommunicationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls);
			AssertEquals("For NCTSCceControlCommunication messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, nctsCceControlCommunicationReceiver);

			var nctsCceControlCommunicationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For NCTSCceControlCommunication messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, nctsCceControlCommunicationReceiverInboxXTTest);

			var nctsCceControlCommunicationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsControls, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For NCTSCceControlCommunication messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, nctsCceControlCommunicationReceiverInboxXTPro);

			var nctsClearanceNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance);
			AssertEquals("For NCTSClearanceNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, nctsClearanceNotificationReceiver);

			var nctsClearanceNotificationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For NCTSClearanceNotification messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, nctsClearanceNotificationReceiverInboxXTTest);

			var nctsClearanceNotificationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureClearance, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For NCTSClearanceNotification messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, nctsClearanceNotificationReceiverInboxXTPro);

			var nctsInvalidationNotificationReceiver = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation);
			AssertEquals("For NCTSClearanceNotification messageType Interchange receiver is not empty", SpanishCustomsTypeCodeList.Codes.InBoxSpanishCustomsForEHub, nctsInvalidationNotificationReceiver);

			var nctsInvalidationNotificationReceiverInboxXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation, isXT: false, isTest: true, isInboxXT: true);
			AssertEquals("For NCTSClearanceNotification messageType Interchange receiver is not empty, is not xT, is Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt, nctsInvalidationNotificationReceiverInboxXTTest);

			var nctsInvalidationNotificationReceiverInboxXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.InboxNotificationNctsDepartureInvalidation, isXT: false, isTest: false, isInboxXT: true);
			AssertEquals("For NCTSClearanceNotification messageType Interchange receiver is not empty, is not xT, is not Test and is InboxXT", SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt, nctsInvalidationNotificationReceiverInboxXTPro);

			var esDocumentRequestReceivereHubTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.EsDocumentRequest, isTest: true);
			AssertEquals("For EsDocumentRequest messageType Interchange receiver is not empty, is not xT and is Test", SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, esDocumentRequestReceivereHubTest);

			var esDocumentRequestReceivereHubPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.EsDocumentRequest, isTest: false);
			AssertEquals("For EsDocumentRequest messageType Interchange receiver is not empty, is not xT and is not Test", SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, esDocumentRequestReceivereHubPro);

			var esDocumentRequestReceiverXTTest = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.EsDocumentRequest, isXT: true, isTest: true);
			AssertEquals("For EsDocumentRequest messageType Interchange receiver is not empty, is xT and is Test", SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms, esDocumentRequestReceiverXTTest);

			var esDocumentRequestReceiverXTPro = Utils.GetInterchangeReceiver(DeclarationMessageTypeList.Codes.EsDocumentRequest, isXT: true, isTest: false);
			AssertEquals("For EsDocumentRequest messageType Interchange receiver is not empty, is xT and is not Test", SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms, esDocumentRequestReceiverXTPro);

			var incorrectReceiver = Utils.GetInterchangeReceiver("AAA");
			AssertEquals("For AAA messageType Interchange receiver is empty", ZString.Empty, incorrectReceiver);
		});
	}

	public void TestGetServiceAndOperation()
	{
		CombineAssertions(() =>
		{
			var exportData = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.Export);
			AssertEquals("For Export messageType Service is empty", ZString.Empty, exportData.Service);
			AssertEquals("For Export messageType Operation is empty", ZString.Empty, exportData.Operation);

			var importAmendmentBox40Data = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportAmendmentBox40);
			AssertEquals("For ImportAmendmentBox40 messageType Service is not empty", "ModificacionPdcCas40V1Service", importAmendmentBox40Data.Service);
			AssertEquals("For ImportAmendmentBox40 messageType Operation is not empty", "ModificacionPdcCas40V1", importAmendmentBox40Data.Operation);

			var exportUcc6Data = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ExportUcc6);
			AssertEquals("For ExportUcc6 messageType Service is not empty", "CC515CV1Service", exportUcc6Data.Service);
			AssertEquals("For ExportUcc6 messageType Operation is not empty", "CC515CV1", exportUcc6Data.Operation);

			var dvdData = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.DvdH2);
			AssertEquals("For DvdH2 messageType Service is not empty", "DVDH2V1Service", dvdData.Service);
			AssertEquals("For DvdH2 messageType Operation is not empty", "DVDH2V1", dvdData.Operation);

			var ncts5DepartureData = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.Ncts5Departure);
			AssertEquals("For Ncts5Departure messageType Service is not empty", "CC015CV1Service", ncts5DepartureData.Service);
			AssertEquals("For Ncts5Departure messageType Operation is not empty", "CC015CV1", ncts5DepartureData.Operation);

			var t2lPOUSData = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.T2lPresentationPous);
			AssertEquals("For T2lPresentationPous messageType Service is not empty", "CCIEJECV1Service", t2lPOUSData.Service);
			AssertEquals("For T2lPresentationPous messageType Operation is not empty", "CCIEJECV1", t2lPOUSData.Operation);

			var t2lPOUSDataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.T2lQueryPous);
			AssertEquals("For T2lQueryPous messageType Service is not empty", "CCIEP01CONSV1Service", t2lPOUSDataQuery.Service);
			AssertEquals("For T2lQueryPous messageType Operation is not empty", "CCIEP01CONSV1", t2lPOUSDataQuery.Operation);

			var t2lPOUSDataReception = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.T2lReceptionPous);
			AssertEquals("For T2lReceptionPous messageType Service is not empty", "CCIEP01INDV1Service", t2lPOUSDataReception.Service);
			AssertEquals("For T2lReceptionPous messageType Operation is not empty", "CCIEP01INDV1", t2lPOUSDataReception.Operation);

			var g5ExpeditionDataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.G5v1Expedition);
			AssertEquals("For G5v1Expedition messageType Service is not empty", "G5ExpNotifV1Service", g5ExpeditionDataQuery.Service);
			AssertEquals("For G5v1Expedition messageType Operation is not empty", "G5ExpNotifV1", g5ExpeditionDataQuery.Operation);

			var box44DocumentsH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.Box44DocumentsH1);
			AssertEquals("For Box44DocumentsH1 messageType Service is not empty", "SUP_DOC_V1Service", box44DocumentsH1DataQuery.Service);
			AssertEquals("For Box44DocumentsH1 messageType Operation is not empty", "SUP_DOC_V1", box44DocumentsH1DataQuery.Operation);

			var importActivationH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportActivationH1);
			AssertEquals("For ImportActivationH1 messageType Service is not empty", "CC432AV1Service", importActivationH1DataQuery.Service);
			AssertEquals("For ImportActivationH1 messageType Operation is not empty", "CC432AV1", importActivationH1DataQuery.Operation);

			var importAmendmentBox40H1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportAmendmentBox40H1);
			AssertEquals("For ImportAmendmentBox40H1 messageType Service is not empty", "ADD_DDT_V1Service", importAmendmentBox40H1DataQuery.Service);
			AssertEquals("For ImportAmendmentBox40H1 messageType Operation is not empty", "ADD_DDT_V1", importAmendmentBox40H1DataQuery.Operation);

			var importAnnexH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportAnnexH1);
			AssertEquals("For ImportAnnexH1 messageType Service is not empty", "EnvioDeDocumentosV1Service", importAnnexH1DataQuery.Service);
			AssertEquals("For ImportAnnexH1 messageType Operation is not empty", "EnvioDeDocumentosV1", importAnnexH1DataQuery.Operation);

			var importCompleteActiveH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportCompleteActiveH1);
			AssertEquals("For ImportCompleteActiveH1 messageType Service is not empty", "CC415AV1Service", importCompleteActiveH1DataQuery.Service);
			AssertEquals("For ImportCompleteActiveH1 messageType Operation is not empty", "CC415AV1", importCompleteActiveH1DataQuery.Operation);

			var importCompletePendingActivationH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportCompletePendingActivationH1);
			AssertEquals("For ImportCompletePendingActivationH1 messageType Service is not empty", "CC415AV1Service", importCompletePendingActivationH1DataQuery.Service);
			AssertEquals("For ImportCompletePendingActivationH1 messageType Operation is not empty", "CC415AV1", importCompletePendingActivationH1DataQuery.Operation);

			var importH1QueryDataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportH1Query);
			AssertEquals("For ImportH1Query messageType Service is not empty", "ConsultaImportacionH1V2Service", importH1QueryDataQuery.Service);
			AssertEquals("For ImportH1Query messageType Operation is not empty", "ConsultaImportacionH1V2", importH1QueryDataQuery.Operation);

			var importIncompletePreDeclarationH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportIncompletePreDeclarationH1);
			AssertEquals("For ImportIncompletePreDeclarationH1 messageType Service is not empty", "PDI400V1Service", importIncompletePreDeclarationH1DataQuery.Service);
			AssertEquals("For ImportIncompletePreDeclarationH1 messageType Operation is not empty", "PDI400V1", importIncompletePreDeclarationH1DataQuery.Operation);

			var importSimplifiedActiveH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportSimplifiedActiveH1);
			AssertEquals("For ImportSimplifiedActiveH1 messageType Service is not empty", "CCSimplificadaV1Service", importSimplifiedActiveH1DataQuery.Service);
			AssertEquals("For ImportSimplifiedActiveH1 messageType Operation is not empty", "CCSimplificadaV1", importSimplifiedActiveH1DataQuery.Operation);

			var importSimplifiedPendingActivationH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.ImportSimplifiedPendingActivationH1);
			AssertEquals("For ImportSimplifiedPendingActivationH1 messageType Service is not empty", "CCSimplificadaV1Service", importSimplifiedPendingActivationH1DataQuery.Service);
			AssertEquals("For ImportSimplifiedPendingActivationH1 messageType Operation is not empty", "CCSimplificadaV1", importSimplifiedPendingActivationH1DataQuery.Operation);

			var pendingSupportingDocumentH1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.PendingSupportingDocumentH1);
			AssertEquals("For PendingSupportingDocumentH1 messageType Service is not empty", "DocJustificativosV1Service", pendingSupportingDocumentH1DataQuery.Service);
			AssertEquals("For PendingSupportingDocumentH1 messageType Operation is not empty", "DocJustificativosV1", pendingSupportingDocumentH1DataQuery.Operation);

			var preDeclarationCancellationH1H1DataQuery = Utils.GetServiceAndOperation(DeclarationMessageTypeList.Codes.PreDeclarationCancellationH1);
			AssertEquals("For PreDeclarationCancellationH1 messageType Service is not empty", "CC414AV1Service", preDeclarationCancellationH1H1DataQuery.Service);
			AssertEquals("For PreDeclarationCancellationH1 messageType Operation is not empty", "CC414AV1", preDeclarationCancellationH1H1DataQuery.Operation);
		});
	}
}
