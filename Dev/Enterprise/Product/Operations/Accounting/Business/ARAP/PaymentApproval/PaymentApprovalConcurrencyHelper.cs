using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public static class PaymentApprovalConcurrencyHelper
	{
		public static void CheckAndReportNewQuoteInDB(ZGuid paymentApprovalPK, IEnumerable<ZGuid> existedQuotePKs)
		{
			var tempFactory = new BusinessObjectFactory();
			var query = CreateCheckConcurrencyQuery(paymentApprovalPK, existedQuotePKs);

			if (tempFactory.Exists(typeof(AccEPaymentQuote), query, false))
			{
				ThrowException();
			}
		}

		public static void BatchCheckAndReportNewQuoteInDB(IEnumerable<PaymentApprovalBase> paymentApprovals)
		{
			var queries = new List<ZQuery>();
			foreach (var approval in paymentApprovals)
			{
				var quotes = approval.PaymentQuotes;
				var paymentApprovalConcurrencyCheckQuery = CreateCheckConcurrencyQuery(approval.PK, quotes.Select(x => x.PK));
				queries.Add(paymentApprovalConcurrencyCheckQuery);
			}

			if (CheckHasNewQuoteInDBWithQuery(queries))
			{
				ThrowException();
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:Do Not Use Loop To Add Or Conditions To Filter", Justification = "Already split into small batch")]
		static bool CheckHasNewQuoteInDBWithQuery(IEnumerable<ZQuery> queries)
		{
			if (!queries.Any())
			{
				return false;
			}

			int counter = 0;
			var hasNewQuotesInDB = false;
			var tempFactory = new BusinessObjectFactory();
			var combinedQuery = new ZQuery();

			foreach (var query in queries)
			{
				combinedQuery.AddToFilter(query, JoinCondition.Or);
				counter++;

				if (counter % QueryBatchSize == 0)
				{
					hasNewQuotesInDB = tempFactory.Exists(typeof(AccEPaymentQuote), combinedQuery, false);
					combinedQuery = new ZQuery();
					counter = 0;
				}

				if (hasNewQuotesInDB)
				{
					break;
				}
			}

			if (!hasNewQuotesInDB && counter > 0)
			{
				hasNewQuotesInDB = tempFactory.Exists(typeof(AccEPaymentQuote), combinedQuery, false);
			}

			return hasNewQuotesInDB;
		}

		static ZQuery CreateCheckConcurrencyQuery(ZGuid paymentApprovalPK, IEnumerable<ZGuid> existedQuotePKs)
		{
			var query = new ZDBOnlyQuery(typeof(AccEPaymentQuote));
			query.AddToFilter(AccEPaymentQuoteSchema.PK, SQLComparisonOperator.NotEqual, existedQuotePKs);
			query.AddToFilter(AccEPaymentQuoteSchema.QU_AV, paymentApprovalPK);
			return query;
		}

		static void ThrowException()
		{
			var errorMsg = ResString.GetMultilingualString("40FC0E5C-D913-49D2-B843-77A396861373", @"Another user has changed payment information after this form was opened.
Please reload current form to continue.");
			var errorHeading = ResString.GetMultilingualString("DE9722E0-2712-4FAE-9060-C479F518940D", "Concurrency Error");
			throw new ZCannotSaveException(errorMsg, errorHeading);
		}

		const int QueryBatchSize = 10;

		#endregion

#if DEBUG
		public static bool CheckHasNewQuoteInDBWithQuery_ForTestOnly(IEnumerable<ZQuery> queries) => CheckHasNewQuoteInDBWithQuery(queries);

		public static ZQuery CreateCheckConcurrencyQuery_ForTestOnly(ZGuid paymentApprovalPK, IEnumerable<ZGuid> existedQuotePKs) => CreateCheckConcurrencyQuery(paymentApprovalPK, existedQuotePKs);
#endif
	}
}
