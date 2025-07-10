using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeVoucherSoldSendingActionParent))]
	sealed class GuaranteeVoucherSoldSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTopLevelBusinessObject()
		{
			AssertType<CusGuaranteeHeader>(sendingActionParent.TopLevelBusinessObject);
		}

		public void TestMessageSendingActions()
		{
			var messageSendingActionParentForTest = new GuaranteeVoucherSoldSendingActionParentForTest(header);
			AssertType<GuaranteeVoucherSoldSendingActionCollection>(messageSendingActionParentForTest.GetSendingObjectsCollectionCoreExposed);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals(sendingActionParent.SecurityCheckpointToSendWithMessageError, Env.Security.CustomsDeclarationSendWithMessageErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusGuaranteeHeader>();
			sendingActionParent = new GuaranteeVoucherSoldSendingActionParentForTest(header);
		}
		CusGuaranteeHeader header;
		GuaranteeVoucherSoldSendingActionParent sendingActionParent;

		protected override BusinessObject GetNewBusinessObject() => sendingActionParent;

		class GuaranteeVoucherSoldSendingActionParentForTest : GuaranteeVoucherSoldSendingActionParent
		{
			public GuaranteeVoucherSoldSendingActionParentForTest(CusGuaranteeHeader header) : base(header)
			{
			}

			public NonPersistentBusinessObjectCollection<GuaranteeVoucherSoldSendingAction> GetSendingObjectsCollectionCoreExposed => GetSendingObjectsCollectionCore();
		}
	}
}
