namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsCommonMovementHeaderCollection : Customs.Business.CusInBondMoveHeaderCollection
	{
		public NctsCommonMovementHeaderCollection(NctsHeader nctsHeader)
			: base(nctsHeader)
		{
		}

		public new NctsCommonMovementHeader this[int index] => (NctsCommonMovementHeader)base[index];

		protected override bool AllowNew => false;
	}
}
