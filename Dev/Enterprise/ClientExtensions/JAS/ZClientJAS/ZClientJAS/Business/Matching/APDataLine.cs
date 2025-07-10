using System.Text;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Matching
{
	public class APDataLine : PreMatchedDataLine
	{
		public APDataLine(AccTransactionHeaderWithJobInfo transaction, ZDateTime nettingCycle)
			: base(transaction)
		{
			this.NettingCycle = nettingCycle;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder(PayingSubsidiary.Left(8).PadRight(8));
			builder.AppendFormat("{0,-8}", ReceivingSubsidiary.Left(8));
			builder.AppendFormat("{0,8:yyyyMMdd}", InvoiceDate);
			builder.AppendFormat("{0,8:yyyyMMdd}", MaturityDate);
			builder.AppendFormat("{0,8:yyyyMMdd}", NettingCycle);
			builder.AppendFormat("{0,-20}", MasterBill.Left(20));
			builder.AppendFormat("{0,-20}", HouseBill.Left(20));
			builder.AppendFormat("{0,-20}", FullInvoiceNumber.Left(20));
			builder.AppendFormat("{0,-20}", OtherRefNumber.Left(20));
			builder.Append(Category.Left(1));
			builder.Append(new ZString(' ', 19));
			builder.AppendFormat("{0,16:###########0.000}", Amount);
			builder.AppendFormat("{0,-3}", CurrencyCode.Left(3));
			builder.Append((CreditNote) ? "T" : "F");
			builder.AppendFormat("{0,-34}", Description.Left(34));
			builder.AppendFormat("{0,-3}", TransactionType.Left(3));
			builder.Append(new ZString(' ', 13));
			return builder.ToString();
		}

		#region Properties

		protected ZString Description
		{
			get { return Transaction.AH_Desc; }
		}

		protected override ZString PayingSubsidiary
		{
			get { return CurrentSubsidiary; }
		}

		protected override ZString ReceivingSubsidiary
		{
			get { return CounterpartSubsidiary; }
		}

		#endregion

		protected readonly ZDateTime NettingCycle;
	}
}
