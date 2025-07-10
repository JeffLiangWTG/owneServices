namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public interface IUSCustomsDataRegistry
			{
				IRegistryItem ABIMessagesGroup { get; }

				IRegistryItem ExportEntryFilerID { get; }

				string ExportEntryFilerIDValue { get; }

				IRegistryItem ShipmentAuditShouldCheckCompany { get; }

				IRegistryItem ShipmentHTSMaximumValue { get; }

				IRegistryItem TransportModeForInBondCreationFromConsol { get; }
			}
		}
	}
}
