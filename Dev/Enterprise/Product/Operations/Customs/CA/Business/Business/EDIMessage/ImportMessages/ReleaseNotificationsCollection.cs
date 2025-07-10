using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	/// <summary>
	/// outbound unattached status queries + imbound unattached responses + all non error inbound (attached or unattached)
	/// </summary>
	public class CAReleaseNotificationsCollection : Enterprise.Messaging.Business.NonDependentEDIMessageCollection
	{
		public CAReleaseNotificationsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public virtual new EDIMessage AddNew()
		{
			return (EDIMessage)base.AddNew();
		}

		public new EDIMessage this[int index]
		{
			get { return (EDIMessage)base[index]; }
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);

			var messageTypeFilter = new ZQuery();

			var sentUnattachedRNSRequests = new ZQuery(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
			sentUnattachedRNSRequests.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			sentUnattachedRNSRequests.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, null);
			messageTypeFilter.AddToFilter(sentUnattachedRNSRequests, JoinCondition.Or);

			var receivedMessageUnattachedOrNonError = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, null);
			var errorTypes = new[] { EDIReleaseImportEntryStatusList.Codes.Error, EDIReleaseImportEntryStatusList.Codes.MessageContentRejected, EDIReleaseImportEntryStatusList.Codes.SyntaxError };
			receivedMessageUnattachedOrNonError.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_MessageSubType, SQLComparisonOperator.NotEqual, errorTypes);

			var receivedMessages = new ZQuery(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.EDIRelease);
			receivedMessages.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			receivedMessages.AddToFilter(receivedMessageUnattachedOrNonError);
			messageTypeFilter.AddToFilter(receivedMessages, JoinCondition.Or);

			result.AddToFilter(messageTypeFilter);
			return result;
		}
	}
}
