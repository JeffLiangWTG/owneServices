using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(AESMessageSendingActionCollection))]
	class AESMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AESMessageSendingActionCollection>
	{
		public void TestCreateElementsDefaultValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.PendingControl;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExportPresentation);

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Prelodged;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExportPresentation);

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExportCancellation);

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.ReleasedForExport;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ReleaseAmendment);

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ReExport;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ReExport, "JE_MessageType REX, CH_EntryStatus empty, should send 570 by default.");
			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.AmendmentRequested;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ReExport, "JE_MessageType REX, CH_EntryStatus other values, should send 570 by default.");

			entryHeader.EntryNumber = "32342KDS3";
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ReExportAmendment, "JE_MessageType REX, CH_EntryStatus not CAR, MRN has values, should send 573.");

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.CancellationRequestedByCustoms;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExitCancellation, "JE_MessageType REX, CH_EntryStatus CAR, should send 614 by default.");

			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExitCancellation, "JE_MessageType EXS, CH_EntryStatus CAR, should send 614 by default.");

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExportCancellation, "JE_MessageType EXP, CH_EntryStatus REQ, should send 514 by default.");

			entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Requested;

			entryHeader.EntryNumber = ZString.Empty;
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.ExitSummary;
			AssertDefaultElement(declaration, AESOutgoingMessageTypeList.Codes.ExitOriginal, "JE_MessageType EXS, CH_EntryStatus not CAR, MRN empty, should send 615 by default.");
		}

		void AssertDefaultElement(JobDeclaration declaration, string expectedMessageType, string message = "MessageType")
		{
			var sendingActionParent = new AESMessageSendingActionParent(declaration);
			var collection = (AESMessageSendingActionCollection)sendingActionParent.SendingObjectsCollection;
			collection.PopulateElements();

			AssertEquals(message, expectedMessageType, collection[0].MessageType);
		}

		protected override AESMessageSendingActionCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			_ = declaration.ActiveEntryHeaders.AddNew();
			var sendingAction = new AESMessageSendingActionParent(declaration);
			return (AESMessageSendingActionCollection)sendingAction.SendingObjectsCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => null;

		public override void TestAdd()
		{
			Assert("AESMessageSendingActionCollection does not support adding.", true);
		}

		public override void TestDelete()
		{
			Assert("AESMessageSendingActionCollection does not support deleting.", true);
		}

		public override void TestRemoveFromRelationship()
		{
			Assert("AESMessageSendingActionCollection does not support RemoveFromRelationship.", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("AESMessageSendingActionCollection haschanges as default", true);
		}
	}
}
