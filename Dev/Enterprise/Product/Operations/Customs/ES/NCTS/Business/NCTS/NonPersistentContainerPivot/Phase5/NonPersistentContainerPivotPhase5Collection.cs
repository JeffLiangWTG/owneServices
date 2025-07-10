namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NonPersistentContainerPivotPhase5Collection : EU.NCTS.Business.NonPersistentContainerPivotPhase5Collection
	{
		public NonPersistentContainerPivotPhase5Collection(NctsPackage package) : base(package)
		{
		}

		protected override void AddNewNonPersistentContainer(EU.NCTS.Business.NctsCusInBondContainer container)
		{
			using (SuspendSettingHasChanges())
			{
				Add(new NonPersistentContainerPivotPhase5((NctsPackage)package, container));
			}
		}
	}
}
