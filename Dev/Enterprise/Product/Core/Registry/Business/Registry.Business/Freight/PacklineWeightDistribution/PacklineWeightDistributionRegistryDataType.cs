namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.PacklineWeightDistributionRegistryItemEditor, Enterprise.Registry.GUI")]
	public sealed class PacklineWeightDistributionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PacklineWeightDistributionConfiguration>
	{
		public PacklineWeightDistributionRegistryDataType()
		{
		}

		public PacklineWeightDistributionRegistryDataType(PacklineWeightDistributionConfiguration defaultValue)
			: base(defaultValue)
		{
		}
	}
}
