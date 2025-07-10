using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	public class GlobalElectronicPaymentDelivery : EServicesDelivery
	{
		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.eHubQueued;

		protected override string TransportType => EDIInterchangeTransportTypeList.Codes.eHub;

		protected override string GetRecipientID(IEDICommunicationsMode mode) => mode.EK_Destination;

		protected override IEnumerable<IStmALogParent> GetInterestedLogParents(IStmALogParent triggerBOWithLogs)
		{
			var interestedLogParent = GetLogParentsToLinkWithGEPMessage(triggerBOWithLogs).ToList();
			interestedLogParent.Add(triggerBOWithLogs);
			return interestedLogParent;
		}

		IEnumerable<IStmALogParent> GetLogParentsToLinkWithGEPMessage(IStmALogParent logParentFromDeliveryContext)
		{
			var eventParentsToLink = new List<IStmALogParent> { };

			if (logParentFromDeliveryContext.LogsParentTableName == AccEPaymentQuoteSchema.Constants.TableName)
			{
				if (logParentFromDeliveryContext is AccEPaymentQuote ePaymentQuote)
				{
					var paymentApproval = ePaymentQuote.PaymentApproval;
					if (paymentApproval != null)
					{
						eventParentsToLink.Add(paymentApproval);
					}
				}
			}
			else if (logParentFromDeliveryContext.LogsParentTableName == AccEPaymentDealSchema.Constants.TableName)
			{
				if (logParentFromDeliveryContext is AccEPaymentDeal ePaymentDeal)
				{
					var paymentApproval = ePaymentDeal?.Quote?.PaymentApproval;
					if (paymentApproval != null)
					{
						eventParentsToLink.Add(paymentApproval);
					}
				}
			}

			return eventParentsToLink;
		}
	}
}
