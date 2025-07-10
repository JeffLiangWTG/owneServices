using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(MessageSendingObject))]
	class MessageSendingObjectBaseOnlyTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAmendmentInvalidationReason_ReadOnly()
		{
			var sendingObject = GetNewBusinessObject();
			var readOnlyForMessageSendingObject = typeof(MessageSendingObject).GetProperty("AmendmentInvalidationReason_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
			AssertEquals("MessageSendingObject's AmendmentInvalidationReason_ReadOnly should be False", readOnlyForMessageSendingObject.GetValue(sendingObject).ToString(), "False");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new MessageSendingObject(bill);
		}
	}
}
