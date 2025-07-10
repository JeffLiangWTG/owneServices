namespace Enterprise.Customs.BR.Module
{
	public static class ModelViewConstants
	{
		public static class BRCusEntryHeader
		{
			public const string Name = "BRCusEntryHeader";
			public const string ClusterKey = "CH_ClusterKey";
			public const string RiskChannel = "CH_RiskChannel";
			public const string AdministrativeStatus = "CH_AdministrativeStatus";
			public const string CargoStatus = "CH_CargoStatus";
			public const string ValidityILShipmentDate = "CH_ValidityILShipmentDate";
			public const string ValidityILDispatchDate = "CH_ValidityILDispatchDate";
		}

		public static class BRJobComInvoiceLine
		{
			public const string Name = "BRJobComInvoiceLine";
			public const string ClusterKey = "JI_ClusterKey";
			public const string ManufacturerIndicator = "JI_ManufacturerIndicator";
		}
	}
}
