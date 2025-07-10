using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(EarlyReleaseMiscMessageSendingObjectCollection))]
	sealed class EarlyReleaseMiscMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EarlyReleaseMiscMessageSendingObjectCollection>
	{
		protected override EarlyReleaseMiscMessageSendingObjectCollection GetCollectionToTest()
		{
			return new EarlyReleaseMiscMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;
			return new EarlyReleaseMiscMessageSendingObject(entry);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
