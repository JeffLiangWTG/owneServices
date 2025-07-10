using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SubscriptionMessageSendingObjectParent))]
	public class SubscriptionMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTopLevelBusinessObject()
		{
			AssertSame("TopLevelBusinessObject", wrapper, subscriptionSendingObjectParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals("SecurityCheckpointToSendWithMessageError", Env.Security.CustomsDeclarationSendWithMessageErrors, subscriptionSendingObjectParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestSendingObjectsCollection()
		{
			AssertEquals(0, subscriptionSendingObjectParent.SendingObjectsCollection.Count);

			wrapper.EventSubscriptions.AddNew();

			var subscriptionMessageSendingObjectParent = new SubscriptionMessageSendingObjectParent(wrapper);
			AssertEquals(1, subscriptionMessageSendingObjectParent.SendingObjectsCollection.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => subscriptionSendingObjectParent;

		protected override void SetUp()
		{
			base.SetUp();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XX";
			staff.GS_LoginName = "test";

			wrapper = BRGlbStaffWrapper.Get(staff);
			subscriptionSendingObjectParent = new SubscriptionMessageSendingObjectParent(wrapper);
		}

		BRGlbStaffWrapper wrapper;
		SubscriptionMessageSendingObjectParent subscriptionSendingObjectParent;
	}
}
