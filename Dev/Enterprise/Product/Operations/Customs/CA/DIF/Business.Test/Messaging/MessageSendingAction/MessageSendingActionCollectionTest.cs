using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CA.DIF;
using Enterprise.MasterFiles.Business.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	[TestedType(typeof(MessageSendingActionCollection))]
	sealed class MessageSendingActionCollectionTest : MessageSendingActionCollectionBaseTest<MessageSendingActionCollection, MessageSendingAction, DIFDocument>
	{
		public override void TestSendMessages()
		{
			var declaration = (ICADIFHost)JobDeclaration;

			var requiredDocument1 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocument2 = declaration.RequiredDocumentsProvider.RequiredDocuments.AddNew();

			var difDocument1 = HostWrapper.DISDocuments.AddNew();
			difDocument1.RequiredDocumentPK = requiredDocument1.PK;

			var difDocument2 = HostWrapper.DISDocuments.AddNew();
			difDocument2.Status = StatusList.Codes.AcknowledgedOriginal;
			difDocument2.RequiredDocumentPK = requiredDocument2.PK;

			HostWrapper.DISDocuments.AddNew();

			var coll = new MessageSendingActionCollection(HostWrapper as DIFHostWrapper);
			AssertEquals("PreCondition", 3, coll.Count);
			Assert("Status is empty therefore 'Send' is defaulted", coll[0].Send);
			Assert("Status is NOT empty therefore 'Send' is not defaulted", !coll[1].Send);
		}

		protected override MessageSendingActionCollection GetCollectionToTest() => new MessageSendingActionCollection(HostWrapper as DIFHostWrapper);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var difDocument = HostWrapper.DISDocuments.AddNew();
			return new MessageSendingAction(difDocument);
		}

		DIFHostWrapper hostWrapper;
		protected override DISHostWrapperBase<DIFDocument> HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		protected override BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
