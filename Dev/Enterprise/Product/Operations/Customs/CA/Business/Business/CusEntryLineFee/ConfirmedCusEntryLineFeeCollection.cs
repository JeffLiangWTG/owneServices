namespace Enterprise.Customs.CA.Business
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
	}
}
