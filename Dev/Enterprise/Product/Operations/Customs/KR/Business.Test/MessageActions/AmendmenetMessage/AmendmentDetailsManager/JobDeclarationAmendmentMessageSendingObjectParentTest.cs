using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(JobDeclarationAmendmentMessageSendingObjectParent))]
	sealed class JobDeclarationAmendmentMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5BB);
		}

		public void TestSendingObjectsCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var objectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals(1, objectParent.SendingObjectsCollection.Count);
		}
	}
}
