using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.MessageSending.Testing;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	public class SendCancellationMessageDataObjectLookupsTest : TestCaseWithFactory
	{
		public void TestMotivationList()
		{
			var cancellationMessage = new SendCancellationMessageDataObject();
			AssertEquals("MotivationList should contain data for codeType: INVMO and grouping DIE", "INV01, INV02, INV13", cancellationMessage.Lookups.MotivationList.CodesAsString);
		}

		public void TestMotivationList_ShouldBeTheSame()
		{
			var motivationListFromCancellationMessage = new SendCancellationMessageDataObject().Lookups.MotivationList;
			var motivationForInvalidationListFromDeltaIEMessageSendingObject = DeltaIEJobDeclarationMessageSendingObjectLookupsTest.GetMessageSendingObjectForTest(Factory).Lookups.MotivationForInvalidationList;
			AssertContainsExactElementsInAnyOrder("These two motivationList should be exactly the same.", motivationListFromCancellationMessage, motivationForInvalidationListFromDeltaIEMessageSendingObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeltaIEJobDeclarationMessageSendingObjectLookupsTest.SetUpCusCodeList_INVMO(Factory);
		}
	}
}
