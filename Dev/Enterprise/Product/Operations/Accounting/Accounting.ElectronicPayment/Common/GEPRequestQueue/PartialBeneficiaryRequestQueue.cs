using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal class PartialBeneficiaryRequestQueue : BeneficiaryRequestQueue
	{
		internal PartialBeneficiaryRequestQueue(AccEPaymentBeneficiaryRequest resumingRequest, int startPageNumber)
		{
			Requests.Enqueue(resumingRequest);
			StartPageNumber = startPageNumber;
		}
		Queue<AccEPaymentBeneficiaryRequest> Requests => requests ?? (requests = new Queue<AccEPaymentBeneficiaryRequest>());
		Queue<AccEPaymentBeneficiaryRequest> requests;

		int StartPageNumber { get; }

		protected override AccEPaymentBeneficiaryRequest GetTopOneQueuedRequestBizO() => Requests.Any() ? Requests.Dequeue() : null;

		protected override IGEPRequestMessage CreateMessageRequestInstance(AccEPaymentBeneficiaryRequest request) => new PartialBeneficiaryRequestMessageBuilder(request, StartPageNumber);
	}
}
