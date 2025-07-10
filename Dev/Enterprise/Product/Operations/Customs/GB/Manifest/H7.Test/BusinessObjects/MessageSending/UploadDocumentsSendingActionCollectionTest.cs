using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(UploadDocumentsSendingActionCollection))]
	public class UploadDocumentsSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UploadDocumentsSendingActionCollection>
	{
		public void TestAllowNew()
		{
			var collection = new UploadDocumentsSendingActionCollection(Factory);
			AssertEquals(false, collection.AllowNew);
		}

		protected override UploadDocumentsSendingActionCollection GetCollectionToTest() => new UploadDocumentsSendingActionCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new UploadDocumentsSendingAction(bill);
		}
	}
}
