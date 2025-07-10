using CargoWise.Customs.IL.MessageDefinitions.GEN;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapper : INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs
	{
		Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapper(ZString correlationID)
		{
			this.correlationID = correlationID;
		}

		public static INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs NewOrNull(ZString correlationID)
			=> !correlationID.IsEmpty ? new Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapper(correlationID) : null;

		public string CorrelationIDs => correlationID;

		readonly ZString correlationID;
	}
}
