using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class MiscellaneousTransactionCreatorAP : MiscellaneousTransactionCreator
	{
		public MiscellaneousTransactionCreatorAP(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Instantiation Methods

		public override Discount GetNewDiscount()
		{
			return (Discount)Factory.New(typeof(APDiscount));
		}

		public override ExchangeDifference GetNewExchangeDifference()
		{
			return (ExchangeDifference)Factory.New(typeof(APExchangeDifference));
		}

		public override Overpayment GetNewOverpayment()
		{
			return (Overpayment)Factory.New(typeof(APOverpayment));
		}

		public override Journal GetNewBankFee()
		{
			return Factory.New<APJournal>();
		}

		#endregion
	}
}
