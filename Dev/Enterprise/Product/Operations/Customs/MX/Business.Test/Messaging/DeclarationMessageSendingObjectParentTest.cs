using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DeclarationMessageSendingObjectParent))]
	class DeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSendingObjectProperties()
		{
			var parent = (DeclarationMessageSendingObjectParent)GetNewBusinessObject();
			var properties = parent.MessageSendingObjectProperties;

			CombineAssertions(() =>
			{
				AssertEquals("Properties count", 1, properties.Count());

				AssertEquals("MessageType", properties.ElementAt(0).PropertyName);

				AssertEquals("Msg. Type", 100, properties.ElementAt(0).ColumnWidth);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new DeclarationMessageSendingObjectParent(declaration);
		}

		public void TestGetSendingObjectsCollection()
		{
			var sendingObjParent = (DeclarationMessageSendingObjectParent)GetNewBusinessObject();
			var declaration = sendingObjParent.TopLevelBusinessObject as JobDeclaration;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(2, sendingObjParent.SendingObjectsCollection.Count);
			foreach (var sendingObject in sendingObjParent.SendingObjectsCollection)
			{
				AssertType("Sending Object Type", typeof(DeclarationMessageSendingObject), sendingObject);
			}
		}
	}
}
