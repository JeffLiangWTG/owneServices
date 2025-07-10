using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesSendingActionParent))]
	sealed class GuaranteeAccessCodesSendingActionParentTest : EU.NCTS.Business.Testing.GuaranteeAccessCodesSendingObjectParentAbstractTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeAccessCodesSendingActionParent(null));
		}

		public void TestTopLevelBusinessObject()
		{
			AssertType<CusGuaranteeHeader>(sendingObjectParent.TopLevelBusinessObject);
		}

		public override void TestNctsGuarantee()
		{
			var messageSendingObjectParentForTest = new GuaranteeAccessCodesSendingActionParent(cusGuaranteeHeader);
			AssertSame(cusGuaranteeHeader, messageSendingObjectParentForTest.CusGuaranteeHeader);
		}

		public void TestMessageSendingObjects()
		{
			var messageSendingObjectParentForTest = new GuaranteeAccessCodesSendingActionParent(cusGuaranteeHeader);
			AssertType<EU.NCTS.Business.GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingAction>>(messageSendingObjectParentForTest.SendingObjectsCollection);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals(sendingObjectParent.SecurityCheckpointToSendWithMessageError, Env.Security.CustomsDeclarationSendWithMessageErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			sendingObjectParent = new GuaranteeAccessCodesSendingActionParent(cusGuaranteeHeader);
		}
		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeAccessCodesSendingActionParent sendingObjectParent;

		protected override BusinessObject GetNewBusinessObject() => sendingObjectParent;
	}
}
