using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTAMessageSendingObjectCollection))]
	sealed class FTAMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<FTAMessageSendingObjectCollection>
	{
		protected override FTAMessageSendingObjectCollection GetCollectionToTest()
		{
			return new FTAMessageSendingObjectCollection(Declaration, ElectronicDocumentTypeList.Codes._DHR);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new FTAMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DHR);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
