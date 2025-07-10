using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Test
{
	[TestedType(typeof(DocumentRequestSendingAction))]
	class DocumentRequestSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMovementReference()
		{
			testBizObj.MovementReferenceNumber = "MRN001";
			AssertEquals("MovementReference", "MRN001", documentRequestSendingAction.MovementReferenceNumber);
		}

		public void TestMovementReference_Caption()
		{
			var resData = ZPropertyInfoExtensions.GetAttribute<ResourceStringDataAttribute>(documentRequestSendingAction.MovementReferenceNumberInfo);
			CombineAssertions(() =>
			{
				AssertEquals("MovementReferenceInfo full description", "Movement Reference Number", resData.FullDescription);
				AssertEquals("MovementReferenceInfo Caption", "MRN", resData.Caption);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => documentRequestSendingAction;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			testBizObj = header.Bills.AddNew();
			documentRequestSendingAction = new DocumentRequestSendingAction(testBizObj);
		}

		AsycudaBill testBizObj;
		DocumentRequestSendingAction documentRequestSendingAction;
	}
}
