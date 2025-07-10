namespace Enterprise.Customs.DE.Business
{
	public class CusReconEntryLineCollection : Customs.Business.CusReconEntryLineCollection
	{
		public CusReconEntryLineCollection(CusReconEntry master) : base(master)
		{
		}

		public new CusReconEntryLine AddNew() => (CusReconEntryLine)base.AddNew();

		public new CusReconEntryLine this[int index] => (CusReconEntryLine)base[index];
	}
}
