using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(ManifestMessageSendingObjectValidation))]
sealed class ManifestMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckMessageType()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(SendingObject.MessageTypeInfo, "0", ManifestMessageTypeList.Codes.Fresh);
	}

	public void TestCheckMessageTypeAmendmentWithAction()
	{
		CombineAssertions(() =>
		{
			Header.Messages.AddNew();
			const string expectedMessage = "Could not find any Amendment in the current transaction. Cannot Proceed.";
			Header.Action = ManifestMessageTypeList.Codes.Fresh;
			SendingObject.MessageType = ManifestMessageTypeList.Codes.Amendment;
			AssertHasError("When Header Action=F and no bills", SendingObject.MessageTypeInfo, expectedMessage);

			Header.Action = ManifestMessageTypeList.Codes.Amendment;
			SendingObject.Validation.ValidateMessageType();
			AssertNoError("When Header Action=A and no bills", SendingObject.MessageTypeInfo, expectedMessage);

			Header.Action = ManifestMessageTypeList.Codes.Fresh;
			var bill1 = Header.Bills.AddNew();
			var bill2 = Header.Bills.AddNew();
			SendingObject.Validation.ValidateMessageType();
			AssertHasError("When Header Action=F and all Bills Action=F", SendingObject.MessageTypeInfo, expectedMessage);

			bill1.ABL_BillStatus = BillActionList.Codes.Amendment;
			SendingObject.Validation.ValidateMessageType();
			AssertNoError("When Header Action=F and one Bills Action=A", SendingObject.MessageTypeInfo, expectedMessage);

			bill1.ABL_BillStatus = BillActionList.Codes.Supplementary;
			SendingObject.Validation.ValidateMessageType();
			AssertNoError("When Header Action=F and one Bills Action=S", SendingObject.MessageTypeInfo, expectedMessage);
		});
	}

	CGMAsycudaManifestHeader Header => header ??= Factory.New<CGMAsycudaManifestHeader>();
	CGMAsycudaManifestHeader header;

	ManifestMessageSendingObjectParent SendingObjectParent => sendingObjectParent ??= new ManifestMessageSendingObjectParent(Header);
	ManifestMessageSendingObjectParent sendingObjectParent;

	ManifestMessageSendingObject SendingObject => sendingObject ??= SendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
	ManifestMessageSendingObject sendingObject;
}
