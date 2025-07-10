using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocPaymentApprovalMatchLink : DocumentWrapper
	{
		DocPaymentApprovalMatchLink(IDocMachedTransaction transaction, BusinessObjectFactory factory)
			: base(transaction, factory)
		{
			MatchAmount = transaction.Amount;
			CreatorName = transaction.PreparedBy;
			TransactionType = transaction.TransactionType;
			OSAmount = transaction.OSAmount;
		}

		DocPaymentApprovalMatchLink() : base() { }

		public static DocPaymentApprovalMatchLink New(IDocMachedTransaction transaction, BusinessObjectFactory factoryToWrap)
		{
			return new DocPaymentApprovalMatchLink(transaction, factoryToWrap);
		}

		public static DocPaymentApprovalMatchLink New()
		{
			return new DocPaymentApprovalMatchLink();
		}

		public ZDecimal MatchAmount { get; private set; }

		public ZString CreatorName { get; private set; }

		public ZString TransactionType { get; private set; }

		public ZDecimal OSAmount { get; private set; }

		public ZDecimal Amount
		{
			get { return MatchAmount * InvertAmount; }
		}

		public ZDecimal InvertedOSAmount
		{
			get { return OSAmount * InvertAmount; }
		}

		#region Implementation

		protected ZInt InvertAmount
		{
			get
			{
				return (TransactionType == ZArchitecture.Core.TransactionTypes.Payment ||
						TransactionType == "UNA") ? 1 : -1;
			}
		}

		#endregion
	}
}
