namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUnloadingMovementHeaderValidation : NctsCommonMovementHeaderValidation
	{
		public NctsUnloadingMovementHeaderValidation(NctsUnloadingMovementHeader parent)
			: base(parent)
		{
		}

		protected new NctsUnloadingMovementHeader Parent => (NctsUnloadingMovementHeader)base.Parent;
	}
}
