using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class MiscellaneousTransactionCreatorAR : MiscellaneousTransactionCreator
	{
		public MiscellaneousTransactionCreatorAR(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Instantiation Methods

		public override Discount GetNewDiscount()
		{
			return (Discount)Factory.New(typeof(ARDiscount));
		}

		public override ExchangeDifference GetNewExchangeDifference()
		{
			return (ExchangeDifference)Factory.New(typeof(ARExchangeDifference));
		}

		public override Overpayment GetNewOverpayment()
		{
			return (Overpayment)Factory.New(typeof(AROverpayment));
		}

		public override Journal GetNewBankFee()
		{
			return Factory.New<ARJournal>();
		}

		#endregion
	}
}
