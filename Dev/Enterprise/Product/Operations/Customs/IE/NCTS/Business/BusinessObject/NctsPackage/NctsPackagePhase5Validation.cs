namespace Enterprise.Customs.IE.NCTS.Business
{
	public class NctsPackagePhase5Validation : EU.NCTS.Business.NctsPackagePhase5Validation
	{
		public NctsPackagePhase5Validation(NctsPackage parent)
			: base(parent)
		{
		}

		protected override bool NeededZeroPackageError()
		{
			if(Parent.IsBulk || Parent.IsUnpacked)
			{
				return base.NeededZeroPackageError();
			}
			return false;
		}
	}
}
