
namespace Enterprise.ZArchitecture.Core
{
	public static class OrgDescriptions
	{
		public static class AddressType
		{
			public static MultilingualString DocumentaryDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("808a4f72-f9be-43d9-a047-a45048482d8a", "Documentary Address"); }
			}
			public static MultilingualString OfficeDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("5d78355d-504c-4703-b8f0-6aeadf308bcd", "Office Address"); }
			}
			public static MultilingualString PostalDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("ff5fdea3-e0e6-4644-87c0-0a7f5cdfadab", "Postal Address"); }
			}
			public static MultilingualString PickupDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("b1cf523b-70c4-4db7-8f05-b43c1f3bcc5d", "Pickup Address"); }
			}
			public static MultilingualString DeliveryDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("54d0f226-5d98-4604-a8d4-d6b9087b6dde", "Deliver Address"); }
			}
			public static MultilingualString ResidentialDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("ef85f342-cb37-4c66-aaa1-2915b6d8b724", "Residential Address"); }
			}
			public static MultilingualString CustomsAddressOfRecordDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("FFFCCC9E-45E9-492F-A797-9286DF46BF69", "Customs Address of Record"); }
			}

			public static MultilingualString EUCustomsAddressDescription
			{
				get { return SourceGenerated.ResString.GetMultilingualString("4F61AC8A-2922-46D6-99CD-9D68E27B230D", "EU Customs Address"); }
			}
		}

		internal static class ContainerHandling
		{
			public static MultilingualString PackAndUnpack
			{
				get { return SourceGenerated.ResString.GetMultilingualString("9a464adf-621d-4317-b0c3-d20711ee89eb", "Pack and Unpack"); }
			}
			public static MultilingualString PackOnly
			{
				get { return SourceGenerated.ResString.GetMultilingualString("e4eb18ec-3f3b-495b-b77d-4a96d84dac56", "Pack Only"); }
			}
			public static MultilingualString UnpackOnly
			{
				get { return SourceGenerated.ResString.GetMultilingualString("84c1e2ac-2870-4ac8-9e40-eae77b824c97", "Unpack Only"); }
			}
			public static MultilingualString NoPackOrUnpack
			{
				get { return SourceGenerated.ResString.GetMultilingualString("e9164492-de40-44dd-98fd-0b9b0d52e434", "No Pack or Unpack"); }
			}
			public static MultilingualString DropAndPull
			{
				get { return SourceGenerated.ResString.GetMultilingualString("15080481-b0d1-4e4c-9b78-1d49a94ebad2", "Drop and Pull"); }
			}
			public static MultilingualString Ask
			{
				get { return SourceGenerated.ResString.GetMultilingualString("c9b6cc82-6db8-4032-9029-26f3bca7e386", "Ask"); }
			}
			public static MultilingualString Other
			{
				get { return SourceGenerated.ResString.GetMultilingualString("40054c4b-65d1-4b29-b808-5cd1c16993dd", "Other"); }
			}
		}

		internal static class DockHeight
		{
			public static MultilingualString Standard
			{
				get { return SourceGenerated.ResString.GetMultilingualString("312b035f-3336-4a48-bb6b-a4fd9caa7f1d", "Standard Dock Height."); }
			}
			public static MultilingualString NonStandard
			{
				get { return SourceGenerated.ResString.GetMultilingualString("81dfb224-30a6-475f-ab15-acb24b7df5aa", "Non-Standard Dock Height."); }
			}
			public static MultilingualString Other
			{
				get { return SourceGenerated.ResString.GetMultilingualString("96c1967a-b913-43ed-bdb1-eb84eadf7f0c", "Other - See Notes"); }
			}
		}

		internal static class LabourRequired
		{
			public static MultilingualString Yes
			{
				get { return SourceGenerated.ResString.GetMultilingualString("e4476209-1b64-484e-9ee6-825c8649c731", "Yes"); }
			}
			public static MultilingualString No
			{
				get { return SourceGenerated.ResString.GetMultilingualString("07871683-decb-40e2-9f8c-4fef68226abc", "No"); }
			}
			public static MultilingualString Ask
			{
				get { return SourceGenerated.ResString.GetMultilingualString("c9b6cc82-6db8-4032-9029-26f3bca7e386", "Ask"); }
			}
			public static MultilingualString FCLOnly
			{
				get { return SourceGenerated.ResString.GetMultilingualString("dc962584-2e54-40fa-99c3-dec4b2a90a1e", "For FCL Only"); }
			}
			public static MultilingualString OutOfGauge
			{
				get { return SourceGenerated.ResString.GetMultilingualString("a0414248-8269-436e-950f-ece46db82c69", "For Out of Gauge"); }
			}
			public static MultilingualString HeavyPieces
			{
				get { return SourceGenerated.ResString.GetMultilingualString("50c7887c-73bc-43fd-be17-eb52bd1e1590", "For Heavy Pieces"); }
			}
			public static MultilingualString Other
			{
				get { return SourceGenerated.ResString.GetMultilingualString("0d447d04-0817-41a8-b268-d54f21d4efe6", "Other - see notes"); }
			}
		}

		internal static class AccessPoint
		{
			public static MultilingualString Dock
			{
				get { return SourceGenerated.ResString.GetMultilingualString("2230e2c8-2ada-41f1-9f66-f1e20974afc5", "Dock"); }
			}
			public static MultilingualString Rack
			{
				get { return SourceGenerated.ResString.GetMultilingualString("95c8f305-049d-4f64-b791-2801c2cd6d56", "Rack"); }
			}
			public static MultilingualString Interior
			{
				get { return SourceGenerated.ResString.GetMultilingualString("23891b8a-0f8e-47ad-af11-0028af2b646e", "Interior"); }
			}
			public static MultilingualString InteriorViaElevator
			{
				get { return SourceGenerated.ResString.GetMultilingualString("717350bd-96e9-4ced-845e-b973571d0c06", "Interior Via Elevator"); }
			}
			public static MultilingualString InteriorViaStairs
			{
				get { return SourceGenerated.ResString.GetMultilingualString("861f5af1-7e5a-49aa-a696-a631d09177f0", "Interior Via Stairs"); }
			}
			public static MultilingualString Other
			{
				get { return SourceGenerated.ResString.GetMultilingualString("40054c4b-65d1-4b29-b808-5cd1c16993dd", "Other"); }
			}
		}

		internal static class CommunicationRequired
		{
			public static MultilingualString Appointment
			{
				get { return SourceGenerated.ResString.GetMultilingualString("a51afd98-96eb-440a-8efb-7c72206155f6", "Appointment Required"); }
			}
			public static MultilingualString CallBefore
			{
				get { return SourceGenerated.ResString.GetMultilingualString("7ebe9960-3e8e-4297-8a6f-61d1f8722f3a", "Call Before Delivery"); }
			}
			public static MultilingualString NotifyBefore
			{
				get { return SourceGenerated.ResString.GetMultilingualString("872356ef-5a1f-4292-9285-278cd0599880", "Notify Before Delivery"); }
			}
			public static MultilingualString SeeNotes
			{
				get { return SourceGenerated.ResString.GetMultilingualString("96c1967a-b913-43ed-bdb1-eb84eadf7f0c", "Other - See Notes"); }
			}
		}

		public static class ServiceDirection
		{
			public static MultilingualString Import
			{
				get { return SourceGenerated.ResString.GetMultilingualString("5f0a4dd1-8a2b-45ea-837c-b5fbc7e8e499", "Import"); }
			}
			public static MultilingualString Export
			{
				get { return SourceGenerated.ResString.GetMultilingualString("a8a90dc9-8863-4335-b5d5-2f2ef7703304", "Export"); }
			}
			public static MultilingualString Domestic
			{
				get { return SourceGenerated.ResString.GetMultilingualString("ede22551-f09b-4b7a-a759-4d8ed9cdc4d6", "Domestic"); }
			}
			public static MultilingualString CrossTrade
			{
				get { return SourceGenerated.ResString.GetMultilingualString("df8de509-b39a-4351-ac0f-fd6b46e71ebb", "Cross Trade"); }
			}
			public static MultilingualString All
			{
				get { return SourceGenerated.ResString.GetMultilingualString("da947348-6951-47f7-9de9-4854f99cf1e0", "All"); }
			}
		}

		public static class ModesForGroupOrSubTotal
		{
			public static MultilingualString All
			{
				get { return SourceGenerated.ResString.GetMultilingualString("34debe5d-b640-42f7-95f3-45752ff8c858", "All Modes"); }
			}
			public static MultilingualString Air
			{
				get { return SourceGenerated.ResString.GetMultilingualString("52ee3c63-6ce5-4621-9595-2764c6cbbdab", "Air"); }
			}
			public static MultilingualString Sea
			{
				get { return SourceGenerated.ResString.GetMultilingualString("ae41204c-2db8-4399-930b-70a9f2d9dc41", "Sea"); }
			}
			public static MultilingualString FCL
			{
				get { return SourceGenerated.ResString.GetMultilingualString("d61bdb5a-d032-4e46-9c73-d6131f6b273e", "Full Container Load"); }
			}
			public static MultilingualString LCL
			{
				get { return SourceGenerated.ResString.GetMultilingualString("d9cc8d6f-f543-472b-b208-d8673259a152", "Less Container Load"); }
			}
			public static MultilingualString Road
			{
				get { return SourceGenerated.ResString.GetMultilingualString("33fb8d1e-b761-4f6e-8a3d-e5a05841ce31", "Road"); }
			}
			public static MultilingualString Rail
			{
				get { return SourceGenerated.ResString.GetMultilingualString("691d3b77-507f-40b2-954f-dc90889215ed", "Rail"); }
			}
			public static MultilingualString Courier
			{
				get { return SourceGenerated.ResString.GetMultilingualString("10F24294-DA88-4637-9802-42B54B639E32", "Courier"); }
			}
		}

		public static class InvoiceLineGroupings
		{
			public static MultilingualString None { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|None", "No grouping of invoice charges."); } }
			public static MultilingualString All { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|All", "All charges except Customs Duty and Tax as one line."); } }
			public static MultilingualString AEC { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|AEC", "Origin, Loading, Freight, Insurance, Unload and Destination as one line."); } }
			public static MultilingualString OandF { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|OandF", "Origin, Loading, Freight and Insurance as one line."); } }
			public static MultilingualString ORF { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|ORF", "Origin and Freight as one line."); } }
			public static MultilingualString FandD { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|FandD", "Freight, Insurance, Unload and Destination as one line."); } }
			public static MultilingualString OFD { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|OFD", "Origin and Loading as one line, Freight and Insurance as one line, Unload and Dest. as one line."); } }
			public static MultilingualString OFO { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|OFO", "Origin and Loading as one line, Freight and Insurance as one line"); } }
			public static MultilingualString OFF { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|OFF", "Origin and Loading as one line, Freight as one line, Insurance as one line, Unload and Dest..."); } }
			public static MultilingualString OFI { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|OFI", "Origin and Loading as one line, Freight as one line, Insurance as one line"); } }
			public static MultilingualString CCG { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|CCG", "Charge Code Group"); } }
			public static MultilingualString CCD { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|CCD", "Charge Code"); } }
			public static MultilingualString CLC { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|NewCLC", "6 Columns Display, Subtotal by Currency, Roll Up by Charge Code and Currency"); } }
			public static MultilingualString FRT { get { return SourceGenerated.ResString.GetMultilingualString("MasterFiles|InvoiceLineGroupings|FRT", "Freight Charges as one line"); } }
		}

		public static class GroupOrSubTotalCharges
		{
			public static MultilingualString Alphabetical
			{
				get { return SourceGenerated.ResString.GetMultilingualString("dd0c91f5-411e-4906-9663-722b0051f670", "Alphabetical"); }
			}
			public static MultilingualString Sequence
			{
				get { return SourceGenerated.ResString.GetMultilingualString("8240a8cc-ab50-4053-ab79-0ecf2431c941", "Sequence"); }
			}
			public static MultilingualString SubTotalAndSequence
			{
				get { return SourceGenerated.ResString.GetMultilingualString("deaccc9d-4ee0-438c-b79c-cb6281bafee1", "Sub Total Charges AND Sequence"); }
			}
			public static MultilingualString RollUp
			{
				get { return SourceGenerated.ResString.GetMultilingualString("35bf60ad-045f-405e-9d0c-d5cb19249904", "Roll Up Charges"); }
			}
			public static MultilingualString RollUpEntireConsol
			{
				get { return SourceGenerated.ResString.GetMultilingualString("8fe09869-7ea5-449f-b93d-292d6e37f0c6", "Roll Up Charges across Entire Consol"); }
			}
			public static MultilingualString SubTotal
			{
				get { return SourceGenerated.ResString.GetMultilingualString("3697826c-68ee-4fb6-8165-9b50e9bb6fe0", "Sub Total Charges"); }
			}
			public static MultilingualString User
			{
				get { return SourceGenerated.ResString.GetMultilingualString("cad36c0f-df4d-4631-868d-504bb0c914ac", "User Entered"); }
			}
			public static MultilingualString RollupAndSequence
			{
				get { return SourceGenerated.ResString.GetMultilingualString("FE3FB69F-F468-4595-96C2-7E9E92A71887", "Roll up Charges AND Sequence"); }
			}
		}

		public static class CreditAgreedPaymentMethods
		{
			public static MultilingualString BusinessCheck
			{
				get { return SourceGenerated.ResString.GetMultilingualString("7865927a-e038-49e1-a19a-093a6d6943c6", "Business Check"); }
			}
			public static MultilingualString CreditCard
			{
				get { return SourceGenerated.ResString.GetMultilingualString("394372fd-3201-4639-853e-9782959543fc", "Credit Card"); }
			}
			public static MultilingualString BankTransfer
			{
				get { return SourceGenerated.ResString.GetMultilingualString("16f99b67-33f9-43ad-b3d7-d6b478d6ece9", "Bank Transfer"); }
			}
			public static MultilingualString CashAndBankCheck
			{
				get { return SourceGenerated.ResString.GetMultilingualString("ba1af4bf-facc-4115-b461-0e5bbe47c48c", "Cash and/or Bank Check"); }
			}
			public static MultilingualString DebitCard
			{
				get { return SourceGenerated.ResString.GetMultilingualString("ca45ea94-93ea-4336-80ec-1affe641d8e7", "Debit Card"); }
			}
			public static MultilingualString CollectionRequest
			{
				get { return SourceGenerated.ResString.GetMultilingualString("5ccd9d9b-e126-4a00-9dde-b776701a8b72", "Collection Request"); }
			}
			public static MultilingualString EPayment
			{
				get { return SourceGenerated.ResString.GetMultilingualString("b08c7ef1-9461-4d3d-be9c-8056d1acb176", "E-Payment"); }
			}
		}

		public static class Category
		{
			public static MultilingualString Business
			{
				get { return SourceGenerated.ResString.GetMultilingualString("d6cc2943-124f-49ed-8fd0-4e0c57fcf262", "Business"); }
			}
			public static MultilingualString Government
			{
				get { return SourceGenerated.ResString.GetMultilingualString("656a97d4-75d7-4572-845a-7ddf5e9da8f1", "Government"); }
			}
			public static MultilingualString NaturalPersonIndividual
			{
				get { return SourceGenerated.ResString.GetMultilingualString("c8bab418-6f80-4feb-8aa2-95b27f21cc55", "Natural Person/Individual"); }
			}
			public static MultilingualString NonGovernmentOrganisation
			{
				get { return SourceGenerated.ResString.GetMultilingualString("b9b10d83-394b-48ac-a44c-091f319dcbf5", "Non Government Organization"); }
			}
		}
	}
}
