using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	sealed class MessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckSend()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);
			var hostWrapper = new DIFHostWrapper((ICADIFHost)declaration);
			var difDocument = new DIFDocument(hostWrapper);
			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			difDocument.EDocsDocumentPK = eDocs.UniqueKey;

			var action = new MessageSendingAction(difDocument);
			action.Send = true;
			AssertHasMessageError(action.SendInfo, MessageSendingActionValidation.PendingResponses);
			AssertHasError(action.SendInfo, DIFDocumentValidation.FileNameHasInvalidCharacters);

			action.Send = false;
			AssertNoMessageError(action.SendInfo, MessageSendingActionValidation.PendingResponses);
			AssertNoError(action.SendInfo, DIFDocumentValidation.FileNameHasInvalidCharacters);
		}

		public void TestCheckSendWithdrawal()
		{
			var caDIFHost = new Mock<ICADIFHost>();
			var hostWrapper = new DIFHostWrapper(caDIFHost.Object);

			var disDocument = new DIFDocument(hostWrapper);
			disDocument.Status = StatusList.Codes.AwaitingAmendment;

			var action = new MessageSendingAction(disDocument);
			action.SendWithdrawal = true;
			AssertHasMessageError(action.SendWithdrawalInfo, MessageSendingActionValidation.PendingResponses);

			action.SendWithdrawal = false;
			AssertNoMessageError(action.SendWithdrawalInfo, MessageSendingActionValidation.PendingResponses);
		}
	}
}
