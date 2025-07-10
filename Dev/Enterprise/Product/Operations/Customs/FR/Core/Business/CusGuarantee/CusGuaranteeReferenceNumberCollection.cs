namespace Enterprise.Customs.FR.Business
{
	public class CusGuaranteeReferenceNumberCollection : Customs.Business.CusGuaranteeReferenceNumberCollection<CusGuaranteeReferenceNumber>
	{
		public CusGuaranteeReferenceNumberCollection(CusGuaranteeHeader master)
			: base(master)
		{
		}
	}
}
