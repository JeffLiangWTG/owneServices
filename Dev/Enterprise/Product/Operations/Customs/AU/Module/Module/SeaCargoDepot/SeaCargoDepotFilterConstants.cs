namespace Enterprise.Customs.AU.Module
{
	public static class SeaCargoDepotFilterConstants
	{
		public static class NumberFilterTypes
		{
			public const string None = "None";
			public const string MasterBillNumber = "Master Bill";
			public const string HouseBillNumber = "House Bill";
			public const string ContainerNumber = "Container #";
			public const string LloydsNumber = "Lloyds/IMO Number";
			public const string ShipmentOrContainerJobID = "Ship. or Cont. Job #";
			public const string PremiseID = "Premise ID";
			public const string OutturnReference = "Outturn Ref #";
		}

		public static class DateFilterTypes
		{
			public const string UnpackDate = "Unpack Date";
			public const string CargoReceiptDate = "Cargo Receipt Date";
		}
	}
}
