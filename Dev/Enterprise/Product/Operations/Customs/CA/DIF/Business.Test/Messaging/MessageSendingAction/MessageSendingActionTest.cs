using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	[TestedType(typeof(MessageSendingAction))]
	sealed class MessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentDescription()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Example.txt", "DT1", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Example2.txt", "DT2", false);
			var hostWrapper = new DIFHostWrapper(declaration);
			var difDocument = hostWrapper.DISDocuments.AddNew();
			difDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			var action = new MessageSendingAction(difDocument);
			AssertEquals("Example2.txt", action.DocumentDescription);
		}

		public void TestMutualExclusiveBool()
		{
			var action = GetNewBusinessObject() as MessageSendingAction;
			Assert("Bool", !action.Send);
			Assert("Bool", !action.SendAmendment);
			Assert("Bool", !action.SendWithdrawal);
			action.Send = true;
			Assert("Bool", action.Send);
			Assert("Bool", !action.SendAmendment);
			Assert("Bool", !action.SendWithdrawal);
			action.SendAmendment = true;
			Assert("Bool", !action.Send);
			Assert("Bool", action.SendAmendment);
			Assert("Bool", !action.SendWithdrawal);
			action.SendWithdrawal = true;
			Assert("Bool", !action.Send);
			Assert("Bool", !action.SendAmendment);
			Assert("Bool", action.SendWithdrawal);
		}

		public void TestMessageType()
		{
			var difDocument = new DIFDocument(HostWrapper);
			var action = new MessageSendingAction(difDocument);
			AssertEquals("Add", action.MessageType);

			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			AssertEquals("Add", action.MessageType);

			difDocument.Status = StatusList.Codes.AcknowledgedOriginal;
			AssertEquals("Change", action.MessageType);

			action.SendAmendment = true;
			AssertEquals("Amendment", action.MessageType);

			action.SendWithdrawal = true;
			AssertEquals("Withdrawal", action.MessageType);
		}

		public void TestIsWaitingForResponse()
		{
			var disHost = new Mock<ICADIFHost>();
			var hostWrapper = new DIFHostWrapper(disHost.Object);

			var difDocument = new DIFDocument(hostWrapper);
			var action = new MessageSendingAction(difDocument);

			Assert(!action.IsWaitingForResponse);

			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			Assert(action.IsWaitingForResponse);
			disHost.VerifyAll();
		}

		public void TestReadOnly()
		{
			var difDocument = new DIFDocument(HostWrapper);
			var action = new MessageSendingAction(difDocument);

			difDocument.Status = StatusList.Codes.AwaitingOriginal;
			Assert(action.SendAmendmentInfo.ReadOnly);
			Assert(action.SendWithdrawalInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.RejectedOriginal;
			Assert(!action.SendAmendmentInfo.ReadOnly);
			Assert(!action.SendWithdrawalInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.AcceptedOriginal;
			Assert(!action.SendAmendmentInfo.ReadOnly);
			Assert(!action.SendWithdrawalInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.AcknowledgedOriginal;
			Assert(!action.SendAmendmentInfo.ReadOnly);
			Assert(!action.SendWithdrawalInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.RejectedOriginal;
			Assert(!action.SendAmendmentInfo.ReadOnly);
			Assert(!action.SendWithdrawalInfo.ReadOnly);

			difDocument.Status = StatusList.Codes.AcceptedOriginal;
			Assert(!action.SendAmendmentInfo.ReadOnly);
			Assert(!action.SendWithdrawalInfo.ReadOnly);

			action.SendAmendment = true;
			Assert(!action.SendAmendmentInfo.ReadOnly);
			action.SendWithdrawal = true;
			Assert(!action.SendWithdrawalInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject() => new MessageSendingAction(new DIFDocument(HostWrapper));

		DIFHostWrapper hostWrapper;
		DIFHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
