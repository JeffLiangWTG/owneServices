using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class ContactTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			#region SuppressResourceStringsCheckRegion

			public const string Administration = "Administration";
			public const string AirWholesaler = "AirWholesaler";
			public const string All = "All";
			public const string CommissionAgreementRecipient = "CommissionAgreementRecipient";
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";
			public const string CTO = "CTO";
			public const string CustomerService = "CustomerService";
			public const string Depot = "Depot";
			public const string ExportAirDepot = "ExportAirDepot";
			public const string ExportAirFreightAgent = "ExportAirFreightAgent";
			public const string ExportBroker = "ExportBroker";
			public const string ExportDepot = "ExportDepot";
			public const string ExportFreightAgent = "ExportFreightAgent";
			public const string ExportSeaDepot = "ExportSeaDepot";
			public const string ExportSeaFreightAgent = "ExportSeaFreightAgent";
			public const string FreightAgent = "FreightAgent";
			public const string ImportAirDepot = "ImportAirDepot";
			public const string ImportAirFreightAgent = "ImportAirFreightAgent";
			public const string ImportBroker = "ImportBroker";
			public const string ImportDepot = "ImportDepot";
			public const string ImportFreightAgent = "ImportFreightAgent";
			public const string ImportSeaDepot = "ImportSeaDepot";
			public const string ImportSeaFreightAgent = "ImportSeaFreightAgent";
			public const string LocalTransport = "LocalTransport";
			public const string LocalClient = "LocalClient";
			public const string Marketing = "Marketing";
			public const string NotifyParty = "NotifyParty";
			public const string Payables = "Payables";
			public const string Receivables = "Receivables";
			public const string Sales = "Sales";
			public const string ShippingLine = "ShippingLine";
			public const string TransportServices = "TransportServices";
			public const string Warehouse = "Warehouse";
			public const string Warehouse3PL = "Warehouse3PL";
			public const string TransitWarehouse = "TransitWarehouse";
			public const string VerifiedGrossWeightContact = "VerifiedGrossWeightContact";
			public const string NettingClearingJournal = "NettingClearingJournal";
			public const string NettingParticipantStatement = "NettingParticipantStatement";
			public const string ControllingCustomer = "ControllingCustomer";
			public const string ControllingAgent = "ControllingAgent";
			public const string Applicant = "Applicant";
			public const string Importer = "Importer";
			public const string Declarant = "Declarant";
			public const string Principal = "Principal";

			#endregion SuppressResourceStringsCheckRegion
		}

		public static class Descriptions
		{
			public static string Administration { get { return Res.GetString("4b5f74c5-df9e-4458-9827-57bdad0f8866", "Administration Contact"); } }
			public static string AirWholesaler { get { return Res.GetString("e7edc032-43b1-47c0-ad18-e579544f1050", "Air Wholesaler Contact"); } }
			public static string All { get { return Res.GetString("29c06266-5a65-4ca5-9ff5-dffe3f980b28", "All Purpose Contact"); } }
			public static string CommissionAgreementRecipient { get { return Res.GetString("f9eb38d2-3944-410c-bf71-e12f4c7d514d", "Commission Agreement Recipient Contact"); } }
			public static string Consignee { get { return Res.GetString("7a2fde40-7a28-4ac4-944d-4f539a2a84f6", "Consignee Contact"); } }
			public static string Consignor { get { return Res.GetString("780dca22-898c-4842-a5aa-8810e46b9ff6", "Consignor Contact"); } }
			public static string CTO { get { return Res.GetString("6b147d3f-2f3d-4968-85b0-fa2fc2b926fa", "CTO Contact"); } }
			public static string CustomerService { get { return Res.GetString("5751395f-70d0-4640-b0c0-f9512fdaba00", "Customer Service Contact"); } }
			public static string Depot { get { return Res.GetString("703753ec-2483-4dc1-8f3a-5dc5902eb286", "Depot Contact"); } }
			public static string ExportAirDepot { get { return Res.GetString("1ff6b259-2be9-418e-a8dc-25a34d842a62", "Export Air Depot Contact"); } }
			public static string ExportAirFreightAgent { get { return Res.GetString("3ce182c5-e472-4e57-a6d6-4129e086bee3", "Export Air Freight Agent Contact"); } }
			public static string ExportBroker { get { return Res.GetString("863ecd80-66fd-490d-8e6f-bda16c786bc3", "Export Broker Contact"); } }
			public static string ExportDepot { get { return Res.GetString("76a348ae-ee0b-403d-91b8-91c3f4fe88b1", "Export Depot Contact"); } }
			public static string ExportFreightAgent { get { return Res.GetString("20ac24f8-083d-4caf-a7b2-df19bf82048d", "Export Freight Agent Contact"); } }
			public static string ExportSeaDepot { get { return Res.GetString("38361cd2-2d10-456e-8268-339b9fdbf258", "Export Sea Depot Contact"); } }
			public static string ExportSeaFreightAgent { get { return Res.GetString("88e0202e-18b3-4a08-a4dc-1e7dcb6e4ae8", "Export Sea Freight Agent Contact"); } }
			public static string FreightAgent { get { return Res.GetString("cdfe3f05-bc05-4137-a98c-5ece8b226068", "Freight Agent Contact"); } }
			public static string ImportAirDepot { get { return Res.GetString("dadfec0f-c7bd-42e7-9f7a-069bbba22ec2", "Import Air Depot Contact"); } }
			public static string ImportAirFreightAgent { get { return Res.GetString("91c875be-f0ee-486b-8ee7-04df31495e39", "Import Air Freight Agent Contact"); } }
			public static string ImportBroker { get { return Res.GetString("11b6034c-4253-4839-ace8-037d4b75d665", "Import Broker Contact"); } }
			public static string ImportDepot { get { return Res.GetString("33255a94-f3e8-4590-900d-89a98d74240b", "Import Depot Contact"); } }
			public static string ImportFreightAgent { get { return Res.GetString("085e4e9b-e514-496e-bcdb-df707cdbc201", "Import Freight Agent Contact"); } }
			public static string ImportSeaDepot { get { return Res.GetString("31a6987c-1386-4de0-8252-868d5c55f2e5", "Import Sea Depot Contact"); } }
			public static string ImportSeaFreightAgent { get { return Res.GetString("5fe771e4-e303-474b-bddb-a95c9022d2ef", "Import Sea Freight Agent Contact"); } }
			public static string LocalTransport { get { return Res.GetString("b07967c6-5b12-4f91-9061-0b32d6ab92e7", "Port Transport Contact"); } }
			public static string LocalClient { get { return Res.GetString("e8e0618f-f805-4c4e-b13f-f4efeeaa17bd", "Local Client Contact"); } }
			public static string Marketing { get { return Res.GetString("451fcd0e-f62e-4fef-97f2-bc38e06aeb24", "Marketing Contact"); } }
			public static string NotifyParty { get { return Res.GetString("717e2fea-f5b5-42b2-adb6-adce8600fdc4", "Notify Party Contact"); } }
			public static string NettingClearingJournal { get { return Res.GetString("53060805-9d92-44d0-b006-823fffd7eb05", "Netting Clearing Journal"); } }
			public static string NettingParticipantStatement { get { return Res.GetString("94d96419-ee1a-40be-b0cb-f274e38b9c0f", "Netting Participant Statement"); } }
			public static string Payables { get { return Res.GetString("f152e155-5d05-4156-834f-e929c6680166", "Accounts Payable Contact"); } }
			public static string Receivables { get { return Res.GetString("125591a9-7164-4fdd-ad9f-38ee2d171950", "Accounts Receivable Contact"); } }
			public static string Sales { get { return Res.GetString("e37d8f48-4dad-45f5-9401-f565327ac5c6", "Sales Contact"); } }
			public static string ShippingLine { get { return Res.GetString("47651201-c455-455d-bda4-e31c7bcaca8d", "Shipping Line Contact"); } }
			public static string TransportServices { get { return Res.GetString("5207472e-e245-441b-ac0e-997472cc3c11", "Transport Services Contact"); } }
			public static string TransitWarehouse { get { return Res.GetString("edeb96ed-ce53-470d-91af-ad1a8de1cf9", "Transit Warehouse Contact"); } }
			public static string Warehouse { get { return Res.GetString("66d16b8f-3289-46dc-aaef-f99e5c92fdeb", "Warehouse Contact"); } }
			public static string Warehouse3PL { get { return Res.GetString("f30b9deb-e1fa-40a7-bb99-44325d22dfe2", "Warehouse 3PL Contact"); } }
			public static string VerifiedGrossWeightContact { get { return Res.GetString("5eaa9326-2ce7-40b2-884b-6ee66c5d9447", "Verified Gross Weight Contact"); } }
			public static string ControllingCustomer { get { return Res.GetString("15b52b7d-dc66-4ca2-b91e-fcff29894303", "Controlling Customer"); } }
			public static string ControllingAgent { get { return Res.GetString("f39bad30-4e64-40f7-bfc6-b84b872260a1", "Controlling Agent"); } }
			public static string Applicant { get { return Res.GetString("0B817FE1-56A3-4A0C-A946-B10E45231626", "Applicant"); } }
			public static string Importer { get { return Res.GetString("F533AD2C-8BBA-41F1-91E9-91415F011F5B", "Importer"); } }
			public static string Declarant { get { return Res.GetString("0A80CF2E-3F02-4FDE-8297-083265734BFE", "Declarant"); } }
			public static string Principal { get { return Res.GetString("B69FF646-EB2B-423D-937C-AE0424993D81", "Principal"); } }
		}

		public ContactTypeList()
		{
			Add(new ContactTypeItem(Codes.Administration, Descriptions.Administration, ContactType.Administration));
			Add(new ContactTypeItem(Codes.AirWholesaler, Descriptions.AirWholesaler, ContactType.AirWholesaler));
			Add(new ContactTypeItem(Codes.All, Descriptions.All, ContactType.All));
			Add(new ContactTypeItem(Codes.CommissionAgreementRecipient, Descriptions.CommissionAgreementRecipient, ContactType.CommissionAgreementRecipient));
			Add(new ContactTypeItem(Codes.Consignee, Descriptions.Consignee, ContactType.Consignee));
			Add(new ContactTypeItem(Codes.Consignor, Descriptions.Consignor, ContactType.Consignor));
			Add(new ContactTypeItem(Codes.CTO, Descriptions.CTO, ContactType.CTO));
			Add(new ContactTypeItem(Codes.CustomerService, Descriptions.CustomerService, ContactType.CustomerService));
			Add(new ContactTypeItem(Codes.Depot, Descriptions.Depot, ContactType.Depot));
			Add(new ContactTypeItem(Codes.ExportAirDepot, Descriptions.ExportAirDepot, ContactType.ExportAirDepot));
			Add(new ContactTypeItem(Codes.ExportAirFreightAgent, Descriptions.ExportAirFreightAgent, ContactType.ExportAirFreightAgent));
			Add(new ContactTypeItem(Codes.ExportBroker, Descriptions.ExportBroker, ContactType.ExportBroker));
			Add(new ContactTypeItem(Codes.ExportDepot, Descriptions.ExportDepot, ContactType.ExportDepot));
			Add(new ContactTypeItem(Codes.ExportFreightAgent, Descriptions.ExportFreightAgent, ContactType.ExportFreightAgent));
			Add(new ContactTypeItem(Codes.ExportSeaDepot, Descriptions.ExportSeaDepot, ContactType.ExportSeaDepot));
			Add(new ContactTypeItem(Codes.ExportSeaFreightAgent, Descriptions.ExportSeaFreightAgent, ContactType.ExportSeaFreightAgent));
			Add(new ContactTypeItem(Codes.FreightAgent, Descriptions.FreightAgent, ContactType.FreightAgent));
			Add(new ContactTypeItem(Codes.ImportAirDepot, Descriptions.ImportAirDepot, ContactType.ImportAirDepot));
			Add(new ContactTypeItem(Codes.ImportAirFreightAgent, Descriptions.ImportAirFreightAgent, ContactType.ImportAirFreightAgent));
			Add(new ContactTypeItem(Codes.ImportBroker, Descriptions.ImportBroker, ContactType.ImportBroker));
			Add(new ContactTypeItem(Codes.ImportDepot, Descriptions.ImportDepot, ContactType.ImportDepot));
			Add(new ContactTypeItem(Codes.ImportFreightAgent, Descriptions.ImportFreightAgent, ContactType.ImportFreightAgent));
			Add(new ContactTypeItem(Codes.ImportSeaDepot, Descriptions.ImportSeaDepot, ContactType.ImportSeaDepot));
			Add(new ContactTypeItem(Codes.ImportSeaFreightAgent, Descriptions.ImportSeaFreightAgent, ContactType.ImportSeaFreightAgent));
			Add(new ContactTypeItem(Codes.LocalTransport, Descriptions.LocalTransport, ContactType.LocalTransport));
			Add(new ContactTypeItem(Codes.Marketing, Descriptions.Marketing, ContactType.Marketing));
			Add(new ContactTypeItem(Codes.NotifyParty, Descriptions.NotifyParty, ContactType.NotifyParty));
			Add(new ContactTypeItem(Codes.NettingClearingJournal, Descriptions.NettingClearingJournal, ContactType.NettingClearingJournal));
			Add(new ContactTypeItem(Codes.NettingParticipantStatement, Descriptions.NettingParticipantStatement, ContactType.NettingParticipantStatement));
			Add(new ContactTypeItem(Codes.Payables, Descriptions.Payables, ContactType.Payables));
			Add(new ContactTypeItem(Codes.Receivables, Descriptions.Receivables, ContactType.Receivables));
			Add(new ContactTypeItem(Codes.Sales, Descriptions.Sales, ContactType.Sales));
			Add(new ContactTypeItem(Codes.ShippingLine, Descriptions.ShippingLine, ContactType.ShippingLine));
			Add(new ContactTypeItem(Codes.TransportServices, Descriptions.TransportServices, ContactType.TransportServices));
			Add(new ContactTypeItem(Codes.TransitWarehouse, Descriptions.TransitWarehouse, ContactType.TransitWarehouse));
			Add(new ContactTypeItem(Codes.Warehouse, Descriptions.Warehouse, ContactType.Warehouse));
			Add(new ContactTypeItem(Codes.Warehouse3PL, Descriptions.Warehouse3PL, ContactType.Warehouse3PL));
			Add(new ContactTypeItem(Codes.LocalClient, Descriptions.LocalClient, ContactType.LocalClient));
			Add(new ContactTypeItem(Codes.VerifiedGrossWeightContact, Descriptions.VerifiedGrossWeightContact, ContactType.VerifiedGrossWeightContact));
			Add(new ContactTypeItem(Codes.ControllingCustomer, Descriptions.ControllingCustomer, ContactType.ControllingCustomer));
			Add(new ContactTypeItem(Codes.ControllingAgent, Descriptions.ControllingAgent, ContactType.ControllingAgent));
			Add(new ContactTypeItem(Codes.Applicant, Descriptions.Applicant, ContactType.Applicant));
			Add(new ContactTypeItem(Codes.Importer, Descriptions.Importer, ContactType.Importer));
			Add(new ContactTypeItem(Codes.Declarant, Descriptions.Declarant, ContactType.Declarant));
			Add(new ContactTypeItem(Codes.Principal, Descriptions.Principal, ContactType.Principal));
		}

		internal class ContactTypeItem : CodeDescriptionPair
		{
			public ContactTypeItem(string code, string description, ContactType contactType)
				: base(code, description)
			{
				ContactType = contactType;
			}
			public readonly ContactType ContactType;
		}

		public ContactType GetContactType(string code)
		{
			ContactTypeItem contactTypeItem = (ContactTypeItem)this[code];
			return contactTypeItem == null ? null : contactTypeItem.ContactType;
		}
	}
}
