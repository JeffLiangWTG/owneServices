namespace Enterprise.Registry.Business
{
	[RegistryEditor("Enterprise.Registry.GUI.EBookingAirCarrierConfigurationRegistryItemEditor, Enterprise.Registry.GUI")]
	public sealed class EBookingAirCarrierConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EBookingCarrierConfiguration>
	{
		public EBookingAirCarrierConfigurationRegistryDataType()
		{
		}

		public EBookingAirCarrierConfigurationRegistryDataType(EBookingCarrierConfiguration defaultValue)
			: base(defaultValue)
		{
		}
	}
}
