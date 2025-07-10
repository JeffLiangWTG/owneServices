using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(RF415DocumentSendingObjectValidation))]
	sealed class RF415DocumentSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDocumentType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObject = new RF415MessageSendingObject(bill);

			var documentSendingObject = new RF415DocumentSendingObject(messageSendingObject);
			var targetInfo = documentSendingObject.DocumentTypeInfo;

			messageSendingObject.ShouldSend = ZBool.False;
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			messageSendingObject.ShouldSend = ZBool.True;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo, MandatoryValidation.MustBeEnteredMessage("Document Type"));
		}
	}
}
