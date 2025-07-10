using CargoWise.Customs.IL.MessageDefinitions.GEN;

namespace Enterprise.Customs.IL.Business.Testing
{
	public class Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapperTest : Customs.Business.Testing.DataProviderTestCase<INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs>
	{
		public void TestNewOrNull()
		{
			AssertNull("When correlationID is empty", Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapper.NewOrNull(""));
			AssertNotNull("When correlationID is not empty", Provider);
		}

		public void TestCorrelationIDs()
		{
			AssertEquals("correlationIDs", Provider.CorrelationIDs);
		}

		protected override INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs GetProvider()
		{
			return Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapper.NewOrNull("correlationIDs");
		}
	}
}
