using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	class QuoteRequestQueue : IGEPRequestQueue
	{
		string IGEPRequestQueue.MessageTypeDescription => Res.GetString("8ef1c5b3-477b-47a2-96f3-84d11c94f2cf", "quotes");

		IGEPRequestMessage IGEPRequestQueue.GetTopOneQueuedRequest()
		{
			var quoteFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(AccEPaymentQuote));
			query.AddToFilter(AccEPaymentQuoteSchema.QU_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccEPaymentQuoteSchema.QU_Status, QuoteStatusCodes.Queued);
			var quote = quoteFactory.LoadTop1<AccEPaymentQuote>(query);
			return quote != null ? new QuoteMessageBuilder(quote) : null;
		}
	}
}
