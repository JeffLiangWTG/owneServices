using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using DealStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Deal;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	class DealRequestQueue : IGEPRequestQueue
	{
		string IGEPRequestQueue.MessageTypeDescription => Res.GetString("7afe6fc7-2bd7-46af-a464-bc1e33e563a3", "deals");

		IGEPRequestMessage IGEPRequestQueue.GetTopOneQueuedRequest()
		{
			var quoteFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(AccEPaymentDeal));
			query.AddToFilter(AccEPaymentDealSchema.AED_GC_Company, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccEPaymentDealSchema.AED_Status, DealStatusCodes.Queued);
			var deal = quoteFactory.LoadTop1<AccEPaymentDeal>(query);
			return deal != null ? new DealMessageBuilder(deal) : null;
		}
	}
}
