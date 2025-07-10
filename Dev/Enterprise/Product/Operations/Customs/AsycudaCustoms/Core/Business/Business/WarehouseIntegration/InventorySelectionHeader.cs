namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class InventorySelectionHeader : Customs.Business.DeclarationInventorySelectionHeader
	{
		public InventorySelectionHeader(JobDeclaration declaration)
			: base(declaration)
		{
			IsGroupByCartonSupported = false;
		}

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	}
}
