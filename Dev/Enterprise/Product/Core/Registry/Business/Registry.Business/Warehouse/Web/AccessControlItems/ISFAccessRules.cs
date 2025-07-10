using System.Collections.Generic;

namespace Enterprise.Registry.Business.Web
{
	public class ISFAccessRules : AccessRulesBase
	{
		//Strings used as constants in case statements, ImporterSecurityFiling\ImporterSecurityFilingDetails.aspx.cs
		#region SuppressResourceStringsCheckRegion

		public static class Roles
		{
			public const string ISFImporter = "Importer";
			public const string ISFShipToLocation = "Ship To Location";
			public const string ISFBuyingParty = "Buying Party";
			public const string ISFSellingParty = "Selling Party";
			public const string ISFStuffingLocation = "Stuffing Location";
			public const string ISFConsolidator = "Consolidator";
			public const string ISFManufacturer = "Manufacturer";
			public const string SendingAgent = "Sending Agent";
			public const string ISFBookingParty = "Booking Party";
		}

		public override string[] GetRoles()
		{
			return roles ?? (roles = new[]
									 {
										Roles.ISFImporter,
										Roles.ISFShipToLocation,
										Roles.ISFBuyingParty,
										Roles.ISFSellingParty,
										Roles.ISFStuffingLocation,
										Roles.ISFConsolidator,
										Roles.ISFManufacturer,
										Roles.SendingAgent,
										Roles.ISFBookingParty
									 });
		}

		public static class Captions
		{
			public const string ISF_Details = "ISF Details";
			public const string Importer_Details = "Importer Details";
			public const string Consignee_Details = "Consignee Details";
			public const string Bond = "Bond";
			public const string Ship_To_Parties = "Ship To Parties";
			public const string Buying_Party = "Buying Party";
			public const string Selling_Party = "Selling Party";
			public const string Stuffing_Location = "Stuffing Location";
			public const string Consolidator = "Consolidator";
			public const string Manufacturers = "Manufacturers";
			public const string Lines = "Lines";
			public const string Containers = "Containers";
			public const string Documents = "Documents";
		}

		public override string[] GetCaptions()
		{
			return captions ?? (captions = new[]
										   {
											Captions.ISF_Details,
											Captions.Importer_Details,
											Captions.Consignee_Details,
											Captions.Bond,
											Captions.Ship_To_Parties,
											Captions.Buying_Party,
											Captions.Selling_Party,
											Captions.Stuffing_Location,
											Captions.Consolidator,
											Captions.Manufacturers,
											Captions.Lines,
											Captions.Containers,
											Captions.Documents
										   });
		}

		public override KeyValuePair<string, string>[] GetDefaultTicked()
		{
			return new[]
				   {
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Importer_Details),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Consignee_Details),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Bond),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Ship_To_Parties),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Buying_Party),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Selling_Party),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Containers),
					new KeyValuePair<string, string>(Roles.ISFImporter, Captions.Documents),

					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Importer_Details),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Consignee_Details),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Ship_To_Parties),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Buying_Party),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Containers),
					new KeyValuePair<string, string>(Roles.ISFShipToLocation, Captions.Documents),

					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Importer_Details),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Consignee_Details),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Ship_To_Parties),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Buying_Party),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Containers),
					new KeyValuePair<string, string>(Roles.ISFBuyingParty, Captions.Documents),

					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.Selling_Party),
					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFSellingParty, Captions.Containers),

					new KeyValuePair<string, string>(Roles.ISFStuffingLocation, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFStuffingLocation, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFStuffingLocation, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFStuffingLocation, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.ISFStuffingLocation, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFStuffingLocation, Captions.Containers),

					new KeyValuePair<string, string>(Roles.ISFConsolidator, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFConsolidator, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFConsolidator, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFConsolidator, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.ISFConsolidator, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFConsolidator, Captions.Containers),

					new KeyValuePair<string, string>(Roles.ISFManufacturer, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFManufacturer, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFManufacturer, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFManufacturer, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.ISFManufacturer, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFManufacturer, Captions.Containers),

					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.Selling_Party),
					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.Lines),
					new KeyValuePair<string, string>(Roles.SendingAgent, Captions.Containers),

					new KeyValuePair<string, string>(Roles.ISFBookingParty, Captions.ISF_Details),
					new KeyValuePair<string, string>(Roles.ISFBookingParty, Captions.Stuffing_Location),
					new KeyValuePair<string, string>(Roles.ISFBookingParty, Captions.Consolidator),
					new KeyValuePair<string, string>(Roles.ISFBookingParty, Captions.Manufacturers),
					new KeyValuePair<string, string>(Roles.ISFBookingParty, Captions.Lines),
					new KeyValuePair<string, string>(Roles.ISFBookingParty, Captions.Containers),
				   };
		}
		#endregion
	}
}
