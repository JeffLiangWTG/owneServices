using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class ConfirmedCusEntryLineFeeCollection : Customs.Business.ConfirmedCusEntryLineFeeCollection
	{
		public ConfirmedCusEntryLineFeeCollection(CusEntryLine master) : base(master)
		{
		}

		public new CusEntryLineFee this[int index]
		{
			get { return (CusEntryLineFee)base[index]; }
		}

		public new CusEntryLineFee AddNew()
		{
			return (CusEntryLineFee)base.AddNew();
		}

		public new CusEntryLineFee AddOrUpdate(ZString feeType, ZDecimal amount)
		{
			return (CusEntryLineFee)base.AddOrUpdate(feeType, amount, false);
		}
	}
}
