using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestsSubclassesOf(typeof(GuaranteeAccessCodesSendingObjectParent))]
	public abstract class GuaranteeAccessCodesSendingObjectParentAbstractTest : NonPersistentBusinessObjectTestCase
	{
		public abstract void TestNctsGuarantee();
	}

	[TestedType(typeof(GuaranteeAccessCodesSendingObjectParent))]
	sealed class GuaranteeAccessCodesSendingObjectParentBaseOnlyTest : GuaranteeAccessCodesSendingObjectParentAbstractTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new GuaranteeAccessCodesSendingObjectParent(null));
		}

		public void TestTopLevelBusinessObject()
		{
			AssertType<CusGuaranteeHeader>(sendingObjectParent.TopLevelBusinessObject);
		}

		public override void TestNctsGuarantee()
		{
			var messageSendingObjectParentForTest = new GuaranteeAccessCodesSendingObjectParent(cusGuaranteeHeader);
			AssertSame(cusGuaranteeHeader, messageSendingObjectParentForTest.CusGuaranteeHeader);
		}

		public void TestMessageSendingObjects()
		{
			var messageSendingObjectParentForTest = new GuaranteeAccessCodesSendingObjectParent(cusGuaranteeHeader);
			AssertType<GuaranteeAccessCodesSendingObjectCollection<GuaranteeAccessCodesSendingObject>>(messageSendingObjectParentForTest.SendingObjectsCollection);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals(sendingObjectParent.SecurityCheckpointToSendWithMessageError, Env.Security.CustomsDeclarationSendWithMessageErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			sendingObjectParent = new GuaranteeAccessCodesSendingObjectParent(cusGuaranteeHeader);
		}
		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeAccessCodesSendingObjectParent sendingObjectParent;

		protected override BusinessObject GetNewBusinessObject() => sendingObjectParent;
	}
}
