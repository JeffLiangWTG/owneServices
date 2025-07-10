namespace Enterprise.Customs.EU.EMCS.Business
{
	public class NonPersistentPackagePivotValidation : AutoNonPersistentPackagePivotValidation
	{
		public NonPersistentPackagePivotValidation(AutoNonPersistentPackagePivot parent)
			: base(parent)
		{ }

		protected new NonPersistentPackagePivot Parent => (NonPersistentPackagePivot)base.Parent;
	}
}
