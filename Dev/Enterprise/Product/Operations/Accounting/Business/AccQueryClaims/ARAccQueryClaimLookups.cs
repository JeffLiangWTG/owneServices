using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class ARAccQueryClaimLookups : AccQueryClaimLookups
	{
		public ARAccQueryClaimLookups(ARAccQueryClaim parent)
			: base(parent)
		{
			if (!(parent is ARAccQueryClaim))
			{
				throw new NotSupportedException("Please pass in an AccQuery claim. Currently constructor allows an auto until bug in generator is fixed.");
			}
		}

		ARAccQueryClaim Claim
		{
			get { return (ARAccQueryClaim)Parent; }
		}

		#region Collections

		#region TransactionHeaders

		public override AccTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				var collection = new ARInvoiceForClaimCollection(Claim, TransactionUniqueFilter);
				collection.SetOverrideNotificationWhenAdditionalFilterNotMet(Claim.Validation.GetErrorWhenTransactionHeadersAdditionalFilterNotMet);
				return collection;
			}
		}

		public override CodeDescriptionPairList ClaimStatus
		{
			get
			{
				var collection = base.ClaimStatus;
				collection.RemoveCode(QueryClaimStatusCodeList.Codes.QCStatus7RejectedWithDCRCCRNotClosed);
				collection.RemoveCode(QueryClaimStatusCodeList.Codes.QCStatus8AcceptedNotClosed);
				collection.RemoveCode(QueryClaimStatusCodeList.Codes.QCStatus9AcceptedClosed);
		
				return collection;
			}
		}

		#endregion

		#endregion
	}
}

