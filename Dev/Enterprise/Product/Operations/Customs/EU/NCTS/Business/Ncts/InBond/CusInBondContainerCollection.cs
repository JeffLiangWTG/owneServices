namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondContainerCollection : Customs.Business.CusInBondContainerCollection<CusInBondContainer>
	{
		public CusInBondContainerCollection(CusInBondMoveDetail master)
			: base(master)
		{
		}
	}
}
