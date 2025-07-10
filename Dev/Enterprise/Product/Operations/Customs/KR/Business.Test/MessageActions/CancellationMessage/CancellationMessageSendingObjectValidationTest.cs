using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class CancellationMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void Test_5BFMessageSendingObject()
		{
			var parent = new CancellationMessageSendingObject(importEntry);
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCancellationReason()
		{
			var parent = new CancellationMessageSendingObject(importEntry);
			parent.ShouldSend = true;
			parent.CancellationReason = "";
			AssertHasMessageErrorContaining(parent.CancellationReasonInfo, MandatoryValidation.YouHaveNotEntered);

			parent.CancellationReason = "취하신청사유를 기재";
			AssertNoMessageErrors(parent.CancellationReasonInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			importEntry = declaration.CustomsEntryHeaders.AddNew();
		}
		CusEntryHeader importEntry;
	}
}
