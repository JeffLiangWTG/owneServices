using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.EPayment
{
	public class EPaymentQuoteCollection : AccEPaymentQuoteCollection<EPaymentQuote>
	{
		public EPaymentQuoteCollection(PaymentApprovalBase paymentApproval)
			: base(paymentApproval.Factory)
		{
			GetQuoteParentPKs = () => new[] { paymentApproval.PK };
		}

		public EPaymentQuoteCollection(APPaymentBatchPoster batchPoster)
			: base(batchPoster.Factory)
		{
			GetQuoteParentPKs = () => batchPoster?.PaymentApprovalCollection.Cast<PaymentApprovalBase>().Select(x => x.PK);
		}

		readonly Func<IEnumerable<ZGuid>> GetQuoteParentPKs;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (GetQuoteParentPKs != null)
			{
				query.AddToFilter(AccEPaymentQuoteSchema.QU_AV, GetQuoteParentPKs());
			}
			return query;
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
