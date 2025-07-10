namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.BoleroEBLConfigurationRegistryItemEditor, Enterprise.Registry.GUI")]
	public sealed class BoleroEBLConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BoleroEBLConfiguration>
	{
		public BoleroEBLConfigurationRegistryDataType()
		{
		}

		public BoleroEBLConfigurationRegistryDataType(BoleroEBLConfiguration defaultValue)
			: base(defaultValue)
		{
		}
	}
}
