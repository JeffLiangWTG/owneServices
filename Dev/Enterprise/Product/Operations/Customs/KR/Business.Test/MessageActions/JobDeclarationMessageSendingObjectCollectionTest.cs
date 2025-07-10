using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectCollection))]
	sealed class JobDeclarationMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationMessageSendingObjectCollection>
	{
		protected override JobDeclarationMessageSendingObjectCollection GetCollectionToTest()
		{
			return new JobDeclarationMessageSendingObjectCollection(Declaration, ElectronicDocumentTypeList.Codes._929);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._929);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
