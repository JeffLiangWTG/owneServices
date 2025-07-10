using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicPayment.Common
{
	internal class PaymentRequestPropertyHelper
	{
		public static string CreatePaymentReference(PaymentApprovalBase paymentApproval, AccAPAccountDetails apAccountDetails)
		{
			string result = null;

			if (paymentApproval == null || apAccountDetails == null || apAccountDetails.A1_EPaymentReferenceType.IsEmpty)
			{
				return result;
			}

			switch (apAccountDetails.A1_EPaymentReferenceType)
			{
				case EPaymentReferenceTypes.FreeText:
					result = apAccountDetails.A1_EPaymentReference;
					break;

				case EPaymentReferenceTypes.InvoiceNumbers:
					var builder = new ZStringBuilder();
					var collection = paymentApproval.PaymentMatchingBaseObject?.MatchedTransactions;
					if (collection != null)
					{
						collection.Cast<IMatching>().
								Select(x => x.TransactionNumber).
								Distinct().OrderBy(x => x).
								ForEach(x => builder.AppendIfNotEmpty(x));
						result = TruncateReference(builder.ToStringWithDelimiterBetweenAppends(" "));
					}
					break;

				case EPaymentReferenceTypes.PaymentReferenceNum:
					result = paymentApproval.AV_ChequeOrReference;
					break;

				default:
					throw new InvalidOperationException("Invalid E-Payment Reference Type");
			}

			return result;
		}

		static string TruncateReference(string reference)
		{
			var result = reference;

			if (reference.Length > MaximumLengthOfPaymentRefernce)
			{
				result = reference.Substring(0, MaximumLengthOfPaymentRefernce - 3) + "...";
			}

			return result;
		}

		const int MaximumLengthOfPaymentRefernce = 140;
	}
}
