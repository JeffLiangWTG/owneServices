namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUnloadingMovementHeaderLookups : NctsCommonMovementHeaderLookups
	{
		public NctsUnloadingMovementHeaderLookups(NctsUnloadingMovementHeader parent)
			: base(parent)
		{
		}

		protected new NctsUnloadingMovementHeader Parent => (NctsUnloadingMovementHeader)base.Parent;
	}
}
