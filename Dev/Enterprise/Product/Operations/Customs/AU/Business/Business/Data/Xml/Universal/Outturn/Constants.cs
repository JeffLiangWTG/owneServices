namespace Enterprise.Customs.AU.Declaration.Business.Outturn
{
	public static class Constants
	{
		public static class AddressType
		{
			public const string DischargeAddress = "DischargeAddress";
			public const string OriginAddress = "OriginAddress";
			public const string DestinationAddress = "DestinationAddress";
		}

		public static class DocAddressType
		{
			public const string LocalCartageCFS = "LocalCartageCFS";
			public const string LocalCartageCTO = "LocalCartageCTO";
		}

		public static class AdditionalReference
		{
			public static class EntryType
			{
				public static class Codes
				{
					public const string DischargePremiseID = "DCP";
					public const string OriginPremiseID = "OGP";
					public const string DestinationPremiseID = "DSP";
				}
				public static class Descriptions
				{
					public const string DischargePremiseID = "Discharge Premise ID";
					public const string OriginPremiseID = "Origin Premise ID";
					public const string DestinationPremiseID = "Destination Premise ID";
				}
			}
		}

		public static class EntryNumber
		{
			public static class EntryType
			{
				public static class Codes
				{
					public const string UnderbondStatus = "UBM";
				}

				public static class Descriptions
				{
					public const string UnderbondStatus = "Underbond Status";
				}
			}
		}

		public static class AddInfoType
		{
			public const string UnderbondBySeaVoyage = "UnderbondBySeaVoyage";
			public const string IsMoveFromDischarge = "IsMoveFromDischarge";
			public const string UnderbondBySeaVessel = "UnderbondBySeaVessel";
			public const string IsDamage = "IsDamage";
			public const string IsPillage = "IsPillage";
			public const string Consignee = "Consignee";
			public const string FFInd = "FFInd";
			public const string GrossWt = "GrossWt";
			public const string Mode = "Mode";
			public const string NetWt = "NetWt";
			public const string Site = "Site";
			public const string UBMDest = "UBMDest";
			public const string UBMOrg = "UBMOrg";
			public const string UBMReason = "UBMReason";
			public const string UBMPartyID = "UBMPartyID";
			public const string UBMPartyName = "UBMPartyName";
			public const string LoadList = "LoadList";
			public const string ShipmentOrContainerNumber = "ShipmentOrContainerNumber";
		}

		public static class Note
		{
			public static class Descriptions
			{
				public const string GoodsDescription = "GoodsDescription";
			}
		}
	}
}
