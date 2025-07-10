
namespace Enterprise.DataConverters
{
	public abstract class Cyber2Schema
	{
		public static class Org
		{
			public const string AccountId = "AccountId";
			public const string Branch = "Branch";
			public const string Name = "Name1";
			public const string Acn_No = "Acn_No";
			public const string Abn = "Abn";
			public const string CreditLimit = "CredLimit";
			public const string Currency = "Curr_type";

			public const string IsDebtor = "Debtor";
			public const string IsCreditor = "Creditor";
			public const string IsForwarder = "Agent";
			public const string IsShippingLine = "Shipping_l";
			public const string IsConsignee = "Is_Importer";
			public const string IsSupplier = "Supplier";
			public const string IsExporter = "Is_Exporter";
			public const string IsBroker = "Cust_agent";
			public const string IsSalesLead = "S_lead";
			public const string IsDepot = "Depot";

			public const string BankAccountName = "Account_Name";
			public const string Bank_Bsb = "Bank_bsb";
			public const string BankAccountNo = "Bank_acc";

			public const string CustomsAgent = "CustAgent";
			public const string SupplierCurrency = "Supplier_Currency";
			public const string CMRSupplierCode = "S_ID_CMR";
			public const string SupplierCountryOrigin = "Supplier_country_origin";
			public const string CartageCompanySea = "Cartage_Company_Sea";
			public const string CartageCompanyAir = "Cartage_Company_Air";
		}

		public static class OrgAddress
		{
			public const string AccountId = "AccountId";
			public const string AddrType = "Type1";
			public const string Address1 = "Street1";
			public const string Address2 = "Street2";
			public const string City = "City";
			public const string State = "State";
			public const string PostCode = "PostCode";
			public const string Country = "Country";
		}

		public static class OrgContacts
		{
			public const string AccountId = "AcccontId";
			public const string ContactTitle = "Description";
			public const string ContactFirstName = "FirstName";
			public const string ContactLastName = "LastName";
			public const string ContactPhone = "Workphone";
			public const string ContactMobile = "Mobilphone";
			public const string ContactHomePhone = "Homephone";
			public const string ContactEmail = "Email";
			public const string ContactFax = "Fax";
		}

		public static class OrgNotes
		{
			public const string AccountId = "AccountId";
			public const string NotesType = "Type1";
			public const string Notes = "Notes";
		}

		public static class Vessel
		{
			public const string VesselName = "Vessel";
			public const string LloydsId = "Lloyds_no";
		}

		public static class Lookup
		{
			public const string LookupCode = "Code";
			public const string TariffDescription = "Description";
			public const string Tariff = "Tariff";
			public const string Statistics = "Statistic";
			public const string Treatment = "Treat";
			public const string InstrumentType = "Instr_type";
			public const string InstrumentCode = "Instument";
			public const string Ahecc = "Ahecc";
			public const string By_Law = "By_law";
		}

		public static class Product
		{
			public const string Buyer = "AccountId";
			public const string Supplier = "Supplier";
			public const string PartCode = "PartNo";
			public const string Description = "Description";
			public const string UQ = "UQ";
			public const string UQ1 = "UQ1";
			public const string Lookup = "Lookup";
			public const string Division = "P_Group";
			public const string Weight = "Weight";
			public const string Volume = "Volume";
		}

		public static class Invoice
		{
			public const string Branch = "Branch";
			public const string Account = "AccountId";
			public const string Number = "Number";
			public const string InvoiceDate = "Doc_date";
			public const string DueDate = "Datedue";
			public const string Currency = "Curr_name1";
			public const string LocalBalance = "Balance";
			public const string ForeignBalance = "Balancef";
			public const string D_C_Flag = "D_c_Flag";
		}

		public static class Receipt
		{
			public const string Number = "Receipt_no";
			public const string Branch = "Branch";
			public const string AccountId = "Accountid";
			public const string Currency = "Currency";
			public const string Balance = "Balance";
			public const string Date = "Date1";
			public const string D_C_Flag = "D_c_Flag";
		}
	}
}
