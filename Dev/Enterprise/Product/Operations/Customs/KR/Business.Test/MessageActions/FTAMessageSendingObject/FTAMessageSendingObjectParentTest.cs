using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(FTAMessageSendingObjectParent))]
	sealed class FTAMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new FTAMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5SC);
		}

		public void TestSendingObjectsCollection()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new FTAMessageSendingObjectParent(declaration1, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CustomsEntryHeaders.AddNew();
			declaration2.CustomsEntryHeaders.AddNew();
			var objectParent2 = new FTAMessageSendingObjectParent(declaration2, ElectronicDocumentTypeList.Codes._5SC);
			AssertEquals(2, objectParent2.SendingObjectsCollection.Count);
		}

		public void TestObjectsToSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.CustomsEntryHeaders.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var objectParent = new FTAMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5SC);
			objectParent.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals(1, objectParent.ObjectsToSend.Count());
			AssertEquals(entry, objectParent.ObjectsToSend.Single().Header);
		}
	}
}
