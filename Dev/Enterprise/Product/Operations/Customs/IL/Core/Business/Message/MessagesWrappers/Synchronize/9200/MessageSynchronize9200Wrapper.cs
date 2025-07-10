using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IL.MessageDefinitions.Common;
using CargoWise.Customs.IL.MessageDefinitions.GEN;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public class MessageSynchronize9200Wrapper : IMessageSynchronize9200
	{
		MessageSynchronize9200Wrapper(IEnumerable<ZString> listOfCorrelationIDs)
		{
			this.listOfCorrelationIDs = listOfCorrelationIDs;
		}

		public static IMessageSynchronize9200 NewOrNull(IEnumerable<ZString> listOfCorrelationIDs)
			=> listOfCorrelationIDs.Any() ? new MessageSynchronize9200Wrapper(listOfCorrelationIDs) : null;

		string IMessageSynchronize9200.BatchId => ZString.Empty;

		ICollection<INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs> IMessageSynchronize9200.ListOfCorrelationIDs => correlationIDs ?? (correlationIDs = GetListOfCorrelationIDs());
		ICollection<INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs> correlationIDs;

		ICollection<INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs> GetListOfCorrelationIDs()
		{
			var correlationIDs = new List<INg9200OutgoingMessageDeliveryApprovalListOfCorrelationIDs>();
			foreach (var correlationID in listOfCorrelationIDs)
			{
				correlationIDs.Add(Ng9200OutgoingMessageDeliveryApprovalListOfCorrelationIDsWrapper.NewOrNull(correlationID));
			}
			return correlationIDs;
		}

		IRequestContentHeader IMessageSynchronize9200.RequestContentHeader => RequestContentHeaderWrapper.New();

		readonly IEnumerable<ZString> listOfCorrelationIDs;
	}
}
