namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntryCollection : Customs.Business.CusReconEntryCollection
	{
		public CusReconEntryCollection(CusReconDeclaration master) : base(master)
		{
			declaration = master;
		}
		readonly CusReconDeclaration declaration;

		protected override void OnAdded(Customs.Business.CusReconEntry entry)
		{
			base.OnAdded(entry);
			declaration.CalculateEntrySequenceNumberOnAdded((CusReconEntry)entry);
		}
	}
}
