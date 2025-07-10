using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationLoadingCompletionMessageSendingObjectCollection))]
	sealed class JobDeclarationLoadingCompletionMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationLoadingCompletionMessageSendingObjectCollection>
	{
		protected override JobDeclarationLoadingCompletionMessageSendingObjectCollection GetCollectionToTest()
		{
			return new JobDeclarationLoadingCompletionMessageSendingObjectCollection(Declaration, ElectronicDocumentTypeList.Codes._DF3);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();
			return new JobDeclarationLoadingCompletionMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._DF3);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}
	}
}
