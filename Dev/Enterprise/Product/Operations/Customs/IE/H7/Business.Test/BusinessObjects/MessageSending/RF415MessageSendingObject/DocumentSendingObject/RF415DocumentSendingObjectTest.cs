using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415DocumentSendingObject))]
	sealed class RF415DocumentSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var messageSendingObject = new RF415MessageSendingObject(bill);

			return new RF415DocumentSendingObject(messageSendingObject);
		}
	}
}
