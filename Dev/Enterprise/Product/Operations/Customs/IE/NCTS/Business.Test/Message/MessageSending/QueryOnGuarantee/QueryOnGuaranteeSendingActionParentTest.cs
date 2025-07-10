using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(QueryOnGuaranteeSendingActionParent))]
	sealed class QueryOnGuaranteeSendingActionParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTopLevelBusinessObject()
		{
			AssertType<NctsHeader>(sendingActionParent.TopLevelBusinessObject);
		}

		public void TestMessageSendingActions()
		{
			var messageSendingActionParentForTest = new QueryOnGuaranteeSendingActionParentForTest(nctsHeader);
			AssertType<QueryOnGuaranteeSendingActionCollection>(messageSendingActionParentForTest.GetSendingObjectsCollectionCoreExposed);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertEquals(sendingActionParent.SecurityCheckpointToSendWithMessageError, Env.Security.CustomsDeclarationSendWithMessageErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			sendingActionParent = new QueryOnGuaranteeSendingActionParentForTest(nctsHeader);
		}
		NctsHeader nctsHeader;
		QueryOnGuaranteeSendingActionParent sendingActionParent;

		protected override BusinessObject GetNewBusinessObject() => sendingActionParent;

		class QueryOnGuaranteeSendingActionParentForTest : QueryOnGuaranteeSendingActionParent
		{
			public QueryOnGuaranteeSendingActionParentForTest(NctsHeader nctsHeader) : base(nctsHeader)
			{
			}

			public NonPersistentBusinessObjectCollection<QueryOnGuaranteeSendingAction> GetSendingObjectsCollectionCoreExposed => GetSendingObjectsCollectionCore();
		}
	}
}
