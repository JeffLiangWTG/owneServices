using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILEDIMessageTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var manResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.MAN, ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse);
			var dloResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.DLO, ILEDIMessageSubTypeList.Codes.DeliveryOrderResponse);
			var gpmResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.GPM, ILEDIMessageSubTypeList.Codes.GatepassMovementResponse);
			var decImportResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.DEC, ILEDIMessageSubTypeList.Codes.ImportDeclarationResponse);
			var manRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.MAN, ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest);
			var manQueryRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.MAN, ILEDIMessageSubTypeList.Codes.ManifestQueryRequest);
			var manQueryResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.MAN, ILEDIMessageSubTypeList.Codes.ManifestQueryResponse);
			var dloRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.DLO, ILEDIMessageSubTypeList.Codes.DeliveryOrderRequest);
			var gpmRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.GPM, ILEDIMessageSubTypeList.Codes.GatepassMovementRequest);
			var decImportRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.DEC, ILEDIMessageSubTypeList.Codes.ImportDeclarationRequest);
			var docRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.DOC, ILEDIMessageSubTypeList.Codes.SupportingDocumentsRequest);
			var docResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.DOC, ILEDIMessageSubTypeList.Codes.SupportingDocumentsResponse);
			var decExportRequestMessage = CreateEDIMessage(ILMessageTypeList.Codes.DEC, ILEDIMessageSubTypeList.Codes.ExportDeclarationRequest);
			var docRqDecisionResponseMessage = CreateEDIMessage(ILMessageTypeList.Codes.DOC, ILEDIMessageSubTypeList.Codes.SupportingDocumentsRqDecisionResponse);
			var xerMessage = CreateEDIMessage(ILMessageTypeList.Codes.XER);
			var ilMessage = CreateEDIMessage("XXX");

			Factory.Save();

			var factory = new BusinessObjectFactory();
			AssertType<ILMAN171ResponseMessage>(factory.Load<EDIMessage>(manResponseMessage.PK));
			AssertType<ILDLO122ResponseMessage>(factory.Load<EDIMessage>(dloResponseMessage.PK));
			AssertType<ILGPM135ResponseMessage>(factory.Load<EDIMessage>(gpmResponseMessage.PK));
			AssertType<ILDEC274ResponseMessage>(factory.Load<EDIMessage>(decImportResponseMessage.PK));
			AssertType<ILMAN170RequestMessage>(factory.Load<EDIMessage>(manRequestMessage.PK));
			AssertType<ILMAN820RequestMessage>(factory.Load<EDIMessage>(manQueryRequestMessage.PK));
			AssertType<ILMAN821ResponseMessage>(factory.Load<EDIMessage>(manQueryResponseMessage.PK));
			AssertType<ILDLO120RequestMessage>(factory.Load<EDIMessage>(dloRequestMessage.PK));
			AssertType<ILGPM130RequestMessage>(factory.Load<EDIMessage>(gpmRequestMessage.PK));
			AssertType<ILDEC275RequestMessage>(factory.Load<EDIMessage>(decImportRequestMessage.PK));
			AssertType<ILDOC271RequestMessage>(factory.Load<EDIMessage>(docRequestMessage.PK));
			AssertType<ILDOC276ResponseMessage>(factory.Load<EDIMessage>(docResponseMessage.PK));
			AssertType<ILDEC751RequestMessage>(factory.Load<EDIMessage>(decExportRequestMessage.PK));
			AssertType<ILDOC828ResponseMessage>(factory.Load<EDIMessage>(docRqDecisionResponseMessage.PK));
			AssertType<ILXERResponseMessage>(factory.Load<EDIMessage>(xerMessage.PK));
			AssertType<ILEDIMessage>(factory.Load<EDIMessage>(ilMessage.PK));
		}

		EDIMessage CreateEDIMessage(string messageType, string messageSubType = null)
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_MessageSubType = messageSubType;
			message.EM_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;

			return message;
		}
	}
}
