using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationAmendmentMessageSendingObjectCollection))]
	sealed class JobDeclarationAmendmentMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationAmendmentMessageSendingObjectCollection>
	{
		protected override JobDeclarationAmendmentMessageSendingObjectCollection GetCollectionToTest()
		{
			return new JobDeclarationAmendmentMessageSendingObjectCollection(Declaration, ElectronicDocumentTypeList.Codes._5BB);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
