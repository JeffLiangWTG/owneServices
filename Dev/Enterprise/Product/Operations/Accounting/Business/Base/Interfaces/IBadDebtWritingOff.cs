
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Interfaces
{
	public interface IBadDebtWritingOff : IReversing
	{
		bool IsWritingOff { get; set; }
	}
}

#if DEBUG
namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	using CargoWise.Types;

	public class TestIBadDebtWritingOffTransaction : TestIPayablesAndReceivables, IBadDebtWritingOff
	{
		#region IBadDebtWritingOff Members

		public bool IsWritingOff
		{
			get { return fIsWritingOff; }
			set { fIsWritingOff = value; }
		}
		protected bool fIsWritingOff;

		#endregion

		public ZDecimal AH_OSOutstandingAmount
		{
			get { return fAH_OSOutstandingAmount; }
			set { fAH_OSOutstandingAmount = value; }
		}
		protected ZDecimal fAH_OSOutstandingAmount;

		public ZDecimal AH_InvoiceAmount
		{
			get { return fAH_InvoiceAmount; }
			set { fAH_InvoiceAmount = value; }
		}
		protected ZDecimal fAH_InvoiceAmount;

		public ZDecimal AH_OSTaxAmount
		{
			get { return fAH_OSTaxAmount; }
			set { fAH_OSTaxAmount = value; }
		}
		protected ZDecimal fAH_OSTaxAmount;
	}
}
#endif