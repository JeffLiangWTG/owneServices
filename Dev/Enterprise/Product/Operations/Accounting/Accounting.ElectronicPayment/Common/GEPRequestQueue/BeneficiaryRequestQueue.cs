using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using BeneficiaryRequestStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.BeneficiaryRequest;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal class BeneficiaryRequestQueue : IGEPRequestQueue
	{
		protected virtual AccEPaymentBeneficiaryRequest GetTopOneQueuedRequestBizO()
		{
			var benFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(AccEPaymentBeneficiaryRequest));
			query.AddToFilter(AccEPaymentBeneficiaryRequestSchema.ABR_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccEPaymentBeneficiaryRequestSchema.ABR_Status, BeneficiaryRequestStatusCodes.Queued);
			var benRequest = benFactory.LoadTop1<AccEPaymentBeneficiaryRequest>(query);
			return benRequest;
		}

		protected virtual IGEPRequestMessage CreateMessageRequestInstance(AccEPaymentBeneficiaryRequest request) => new BeneficiaryRequestMessageBuilder(request);

		string IGEPRequestQueue.MessageTypeDescription => Res.GetString("33379fd6-f5d3-4816-a6c9-fa7f3f308f82", "beneficiary requests");

		IGEPRequestMessage IGEPRequestQueue.GetTopOneQueuedRequest()
		{
			var bizo = GetTopOneQueuedRequestBizO();
			return bizo != null ? CreateMessageRequestInstance(bizo) : null;
		}
	}
}
