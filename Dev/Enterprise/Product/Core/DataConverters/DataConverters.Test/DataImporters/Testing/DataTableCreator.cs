using System.Data;
using CargoWise.Types;

namespace Enterprise.DataConverters.Testing.DataImporters.Testing
{
	sealed internal class DataTableCreator
	{
		readonly int dbVersion;

		public DataTableCreator(int dbVersion = 0)
		{
			this.dbVersion = dbVersion;
		}

		#region Implementation

		public DataTable GetTable(ZString name)
		{
			DataTable result = null;
			if (name == "Orgs")
			{
				result = OrgsTable;
			}
			else if (name == "Address")
			{
				result = OrgAddress;
			}
			else if (name == "Contact")
			{
				result = ContactTable;
			}
			else if (name == "Note")
			{
				result = NoteTable;
			}
			else if (name == "Vessel")
			{
				result = VesselTable;
			}
			else if (name == "Part")
			{
				result = PartTable;
			}
			else if (name == "Classification")
			{
				result = ClassificationTable;
			}
			else if (name == "Invoice")
			{
				result = InvoiceTable;
			}
			else if (name == "Receipt")
			{
				result = ReceiptTable;
			}
			return result;
		}

		#region OrganisationTable

		public DataTable OrgsTable
		{
			get
			{
				var table = new DataTable("Orgs");

				var accountId = table.Columns.Add(Cyber2Schema.Org.AccountId, typeof(string));
				var branch = table.Columns.Add(Cyber2Schema.Org.Branch, typeof(string));
				var name = table.Columns.Add(Cyber2Schema.Org.Name, typeof(string));
				var acn_no = table.Columns.Add(Cyber2Schema.Org.Acn_No, typeof(string));
				var abn = table.Columns.Add(Cyber2Schema.Org.Abn, typeof(string));
				var creditLimit = table.Columns.Add(Cyber2Schema.Org.CreditLimit, typeof(decimal));
				var currency = table.Columns.Add(Cyber2Schema.Org.Currency, typeof(string));

				var isDebtor = table.Columns.Add(Cyber2Schema.Org.IsDebtor, typeof(string));
				var isCreditor = table.Columns.Add(Cyber2Schema.Org.IsCreditor, typeof(string));
				var isForwarder = table.Columns.Add(Cyber2Schema.Org.IsForwarder, typeof(string));
				var isShippingLine = table.Columns.Add(Cyber2Schema.Org.IsShippingLine, typeof(string));
				var isConsignee = table.Columns.Add(Cyber2Schema.Org.IsConsignee, typeof(string));
				var isSupplier = table.Columns.Add(Cyber2Schema.Org.IsSupplier, typeof(string));
				var isExporter = table.Columns.Add(Cyber2Schema.Org.IsExporter, typeof(string));
				var isBroker = table.Columns.Add(Cyber2Schema.Org.IsBroker, typeof(string));
				var isSalesLead = table.Columns.Add(Cyber2Schema.Org.IsSalesLead, typeof(string));
				var isDepot = table.Columns.Add(Cyber2Schema.Org.IsDepot, typeof(string));

				var bankAccountName = table.Columns.Add(Cyber2Schema.Org.BankAccountName, typeof(string));
				var bank_Bsb = table.Columns.Add(Cyber2Schema.Org.Bank_Bsb, typeof(string));
				var bankAccountNo = table.Columns.Add(Cyber2Schema.Org.BankAccountNo, typeof(string));

				var customsAgent = table.Columns.Add(Cyber2Schema.Org.CustomsAgent, typeof(string));

				if (dbVersion != 1)
				{
					var cMRSupplierCode = table.Columns.Add(Cyber2Schema.Org.CMRSupplierCode, typeof(string));
				}

				var supplierCurrency = table.Columns.Add(Cyber2Schema.Org.SupplierCurrency, typeof(string));
				var supplierCountryOrigin = table.Columns.Add(Cyber2Schema.Org.SupplierCountryOrigin, typeof(string));
				var cartageCompanySea = table.Columns.Add(Cyber2Schema.Org.CartageCompanySea, typeof(string));
				var cartageCompanyAir = table.Columns.Add(Cyber2Schema.Org.CartageCompanyAir, typeof(string));

				var orgRec = table.NewRow();
				orgRec[Cyber2Schema.Org.AccountId] = "Account";
				orgRec[Cyber2Schema.Org.Branch] = "Branch";
				orgRec[Cyber2Schema.Org.Name] = "Name";
				orgRec[Cyber2Schema.Org.Acn_No] = "BusRegACN";
				orgRec[Cyber2Schema.Org.Abn] = "BusRegABN";
				orgRec[Cyber2Schema.Org.CreditLimit] = 100.0m;
				orgRec[Cyber2Schema.Org.Currency] = "AUD";

				orgRec[Cyber2Schema.Org.IsDebtor] = "Y";
				orgRec[Cyber2Schema.Org.IsCreditor] = "Y";
				orgRec[Cyber2Schema.Org.IsForwarder] = "Y";
				orgRec[Cyber2Schema.Org.IsShippingLine] = "Y";
				orgRec[Cyber2Schema.Org.IsConsignee] = "Y";
				orgRec[Cyber2Schema.Org.IsExporter] = "Y";
				orgRec[Cyber2Schema.Org.IsSupplier] = "Y";
				orgRec[Cyber2Schema.Org.IsBroker] = "Y";
				orgRec[Cyber2Schema.Org.IsSalesLead] = "Y";
				orgRec[Cyber2Schema.Org.IsDepot] = "Y";

				orgRec[Cyber2Schema.Org.BankAccountName] = "BankAccountName";
				orgRec[Cyber2Schema.Org.Bank_Bsb] = "BankBsb";
				orgRec[Cyber2Schema.Org.BankAccountNo] = "BankAccountNo";

				orgRec[Cyber2Schema.Org.CustomsAgent] = "CustomsAgent";

				if (dbVersion != 1)
				{
					orgRec[Cyber2Schema.Org.CMRSupplierCode] = "CMRSupplierCode";
				}

				orgRec[Cyber2Schema.Org.SupplierCurrency] = "AUD";
				orgRec[Cyber2Schema.Org.SupplierCountryOrigin] = "SupplierCountryOrigin";
				orgRec[Cyber2Schema.Org.CartageCompanySea] = "CartageCompanySea";
				orgRec[Cyber2Schema.Org.CartageCompanyAir] = "CartageCompanyAir";

				table.Rows.Add(orgRec);
				return table;
			}
		}

		#endregion

		#region Address Table

		DataTable OrgAddress
		{
			get
			{
				var table = new DataTable();
				var accountId = table.Columns.Add(Cyber2Schema.OrgAddress.AccountId, typeof(string));
				var addrType = table.Columns.Add(Cyber2Schema.OrgAddress.AddrType, typeof(int));
				var address1 = table.Columns.Add(Cyber2Schema.OrgAddress.Address1, typeof(string));
				var address2 = table.Columns.Add(Cyber2Schema.OrgAddress.Address2, typeof(string));
				var city = table.Columns.Add(Cyber2Schema.OrgAddress.City, typeof(string));
				var state = table.Columns.Add(Cyber2Schema.OrgAddress.State, typeof(string));
				var postCode = table.Columns.Add(Cyber2Schema.OrgAddress.PostCode, typeof(string));
				var country = table.Columns.Add(Cyber2Schema.OrgAddress.Country, typeof(string));

				var newRow = table.NewRow();
				newRow[Cyber2Schema.OrgAddress.AccountId] = "Account";
				newRow[Cyber2Schema.OrgAddress.AddrType] = 0;
				newRow[Cyber2Schema.OrgAddress.Address1] = "Address1A";
				newRow[Cyber2Schema.OrgAddress.Address2] = "Address2A";
				newRow[Cyber2Schema.OrgAddress.City] = "CityA";
				newRow[Cyber2Schema.OrgAddress.State] = "NSW";
				newRow[Cyber2Schema.OrgAddress.PostCode] = "CodeA";
				newRow[Cyber2Schema.OrgAddress.Country] = "AU";

				table.Rows.Add(newRow);

				var newRow1 = table.NewRow();
				newRow1[Cyber2Schema.OrgAddress.AccountId] = "Account";
				newRow1[Cyber2Schema.OrgAddress.AddrType] = 1;
				newRow1[Cyber2Schema.OrgAddress.Address1] = "";
				newRow1[Cyber2Schema.OrgAddress.Address2] = "Address2B";
				newRow1[Cyber2Schema.OrgAddress.City] = "CityB";
				newRow1[Cyber2Schema.OrgAddress.State] = "StateB";
				newRow1[Cyber2Schema.OrgAddress.PostCode] = "CodeB";
				newRow1[Cyber2Schema.OrgAddress.Country] = "AU";

				table.Rows.Add(newRow1);

				var newRow2 = table.NewRow();
				newRow2[Cyber2Schema.OrgAddress.AccountId] = "Account";
				newRow2[Cyber2Schema.OrgAddress.AddrType] = 2;
				newRow2[Cyber2Schema.OrgAddress.Address1] = "Postal Address";
				newRow2[Cyber2Schema.OrgAddress.Address2] = "Address2B";
				newRow2[Cyber2Schema.OrgAddress.City] = "CityB";
				newRow2[Cyber2Schema.OrgAddress.State] = "StateB";
				newRow2[Cyber2Schema.OrgAddress.PostCode] = "CodeB";
				newRow2[Cyber2Schema.OrgAddress.Country] = "AU";

				table.Rows.Add(newRow2);

				var newRow3 = table.NewRow();
				newRow3[Cyber2Schema.OrgAddress.AccountId] = "Account";
				newRow3[Cyber2Schema.OrgAddress.AddrType] = 3;
				newRow3[Cyber2Schema.OrgAddress.Address1] = "Despatch Address";
				newRow3[Cyber2Schema.OrgAddress.Address2] = "Address2B";
				newRow3[Cyber2Schema.OrgAddress.City] = "CityB";
				newRow3[Cyber2Schema.OrgAddress.State] = "StateB";
				newRow3[Cyber2Schema.OrgAddress.PostCode] = "CodeB";
				newRow3[Cyber2Schema.OrgAddress.Country] = "AU";

				table.Rows.Add(newRow3);

				return table;
			}
		}
		#endregion

		#region Contact Table
		DataTable ContactTable
		{
			get
			{
				var table = new DataTable();
				var acccontId = table.Columns.Add(Cyber2Schema.OrgContacts.AccountId, typeof(string));
				var contactTitle = table.Columns.Add(Cyber2Schema.OrgContacts.ContactTitle, typeof(string));
				var firstName = table.Columns.Add(Cyber2Schema.OrgContacts.ContactFirstName, typeof(string));
				var lastName = table.Columns.Add(Cyber2Schema.OrgContacts.ContactLastName, typeof(string));
				var workphone = table.Columns.Add(Cyber2Schema.OrgContacts.ContactPhone, typeof(string));
				var mobilphone = table.Columns.Add(Cyber2Schema.OrgContacts.ContactMobile, typeof(string));
				var homephone = table.Columns.Add(Cyber2Schema.OrgContacts.ContactHomePhone, typeof(string));
				var email = table.Columns.Add(Cyber2Schema.OrgContacts.ContactEmail, typeof(string));
				var fax = table.Columns.Add(Cyber2Schema.OrgContacts.ContactFax, typeof(string));

				var newRow = table.NewRow();
				newRow[Cyber2Schema.OrgContacts.AccountId] = "Account";
				newRow[Cyber2Schema.OrgContacts.ContactTitle] = "Title";
				newRow[Cyber2Schema.OrgContacts.ContactFirstName] = "FirstName";
				newRow[Cyber2Schema.OrgContacts.ContactLastName] = "LastName";
				newRow[Cyber2Schema.OrgContacts.ContactPhone] = "Phone";
				newRow[Cyber2Schema.OrgContacts.ContactHomePhone] = "HomePhone";
				newRow[Cyber2Schema.OrgContacts.ContactMobile] = "MobilePhone";
				newRow[Cyber2Schema.OrgContacts.ContactEmail] = "Email";
				newRow[Cyber2Schema.OrgContacts.ContactFax] = "Fax";

				table.Rows.Add(newRow);

				var newRow1 = table.NewRow();
				newRow1[Cyber2Schema.OrgContacts.AccountId] = "Account";
				newRow1[Cyber2Schema.OrgContacts.ContactTitle] = "Title A";
				newRow1[Cyber2Schema.OrgContacts.ContactFirstName] = "FirstName A";
				newRow1[Cyber2Schema.OrgContacts.ContactLastName] = "LastName A";
				newRow1[Cyber2Schema.OrgContacts.ContactPhone] = "Work A";
				newRow1[Cyber2Schema.OrgContacts.ContactHomePhone] = "Home A";
				newRow1[Cyber2Schema.OrgContacts.ContactMobile] = "Mobile A";
				newRow1[Cyber2Schema.OrgContacts.ContactEmail] = "www.com";
				newRow1[Cyber2Schema.OrgContacts.ContactFax] = "Fax A";

				table.Rows.Add(newRow1);

				return table;
			}
		}
		#endregion

		#region Note Table
		DataTable NoteTable
		{
			get
			{
				var table = new DataTable();
				var accountId = table.Columns.Add(Cyber2Schema.OrgNotes.AccountId, typeof(string));
				var type = table.Columns.Add(Cyber2Schema.OrgNotes.NotesType, typeof(int));
				var notes = table.Columns.Add(Cyber2Schema.OrgNotes.Notes, typeof(string));

				var row = table.NewRow();
				row[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row[Cyber2Schema.OrgNotes.NotesType] = 0;
				row[Cyber2Schema.OrgNotes.Notes] = "General Notes";
				table.Rows.Add(row);

				var row1 = table.NewRow();
				row1[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row1[Cyber2Schema.OrgNotes.NotesType] = 1;
				row1[Cyber2Schema.OrgNotes.Notes] = "Debtors Notes";
				table.Rows.Add(row1);

				var row2 = table.NewRow();
				row2[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row2[Cyber2Schema.OrgNotes.NotesType] = 2;
				row2[Cyber2Schema.OrgNotes.Notes] = "Operational Notes";
				table.Rows.Add(row2);

				var row3 = table.NewRow();
				row3[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row3[Cyber2Schema.OrgNotes.NotesType] = 3;
				row3[Cyber2Schema.OrgNotes.Notes] = "IA Notes";
				table.Rows.Add(row3);

				var row4 = table.NewRow();
				row4[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row4[Cyber2Schema.OrgNotes.NotesType] = 4;
				row4[Cyber2Schema.OrgNotes.Notes] = "IS Notes";
				table.Rows.Add(row4);

				var row5 = table.NewRow();
				row5[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row5[Cyber2Schema.OrgNotes.NotesType] = 5;
				row5[Cyber2Schema.OrgNotes.Notes] = "EA Notes";
				table.Rows.Add(row5);

				var row6 = table.NewRow();
				row6[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row6[Cyber2Schema.OrgNotes.NotesType] = 6;
				row6[Cyber2Schema.OrgNotes.Notes] = "ES Notes";
				table.Rows.Add(row6);

				var row7 = table.NewRow();
				row7[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row7[Cyber2Schema.OrgNotes.NotesType] = 7;
				row7[Cyber2Schema.OrgNotes.Notes] = "CUS Notes";
				table.Rows.Add(row7);

				var row8 = table.NewRow();
				row8[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row8[Cyber2Schema.OrgNotes.NotesType] = 8;
				row8[Cyber2Schema.OrgNotes.Notes] = "Invoice Notes";
				table.Rows.Add(row8);

				var row9 = table.NewRow();
				row9[Cyber2Schema.OrgNotes.AccountId] = "Account";
				row9[Cyber2Schema.OrgNotes.NotesType] = 9;
				row9[Cyber2Schema.OrgNotes.Notes] = "Delivery Notes";
				table.Rows.Add(row9);

				return table;
			}
		}
		#endregion

		#region VesselTable

		DataTable VesselTable
		{
			get
			{
				var table = new DataTable("Vessel");

				var vesselName = table.Columns.Add(Cyber2Schema.Vessel.VesselName, typeof(string));
				var lloydsNo = table.Columns.Add(Cyber2Schema.Vessel.LloydsId, typeof(string));

				var row = table.NewRow();
				row[Cyber2Schema.Vessel.VesselName] = "New vessel name";
				row[Cyber2Schema.Vessel.LloydsId] = "LloydsNo";

				table.Rows.Add(row);
				return table;
			}
		}

		#endregion

		#region PartTable

		DataTable PartTable
		{
			get
			{
				var table = new DataTable("Part");

				var buyer = table.Columns.Add(Cyber2Schema.Product.Buyer, typeof(string));
				var supplier = table.Columns.Add(Cyber2Schema.Product.Supplier, typeof(string));
				var partCode = table.Columns.Add(Cyber2Schema.Product.PartCode, typeof(string));
				var description = table.Columns.Add(Cyber2Schema.Product.Description, typeof(string));
				var uQ = table.Columns.Add(Cyber2Schema.Product.UQ, typeof(string));
				var uQ1 = table.Columns.Add(Cyber2Schema.Product.UQ1, typeof(string));
				var lookup = table.Columns.Add(Cyber2Schema.Product.Lookup, typeof(string));
				var division = table.Columns.Add(Cyber2Schema.Product.Division, typeof(string));
				var weight = table.Columns.Add(Cyber2Schema.Product.Weight, typeof(decimal));
				var volume = table.Columns.Add(Cyber2Schema.Product.Volume, typeof(decimal));

				var row = table.NewRow();

				row[Cyber2Schema.Product.Buyer] = "Buyer";
				row[Cyber2Schema.Product.Supplier] = "Supplier";
				row[Cyber2Schema.Product.PartCode] = "PartCode";
				row[Cyber2Schema.Product.Description] = "Description";
				row[Cyber2Schema.Product.UQ] = "UQ";
				row[Cyber2Schema.Product.UQ1] = "UQ1";
				row[Cyber2Schema.Product.Lookup] = "Lookup";
				row[Cyber2Schema.Product.Division] = "Division";
				row[Cyber2Schema.Product.Weight] = 1.0;
				row[Cyber2Schema.Product.Volume] = 1.0;

				table.Rows.Add(row);

				return table;
			}
		}

		#endregion

		#region Classification Table

		DataTable ClassificationTable
		{
			get
			{
				var table = new DataTable("Classification");

				var lookupCode = table.Columns.Add(Cyber2Schema.Lookup.LookupCode, typeof(string));
				var ahecc = table.Columns.Add(Cyber2Schema.Lookup.Ahecc, typeof(string));
				var tariffDescription = table.Columns.Add(Cyber2Schema.Lookup.TariffDescription, typeof(string));
				var statistics = table.Columns.Add(Cyber2Schema.Lookup.Statistics, typeof(string));
				var tariff = table.Columns.Add(Cyber2Schema.Lookup.Tariff, typeof(string));
				var treatment = table.Columns.Add(Cyber2Schema.Lookup.Treatment, typeof(string));
				var instrumentCode = table.Columns.Add(Cyber2Schema.Lookup.InstrumentCode, typeof(string));
				var instrumentType = table.Columns.Add(Cyber2Schema.Lookup.InstrumentType, typeof(string));

				var row = table.NewRow();

				row[Cyber2Schema.Lookup.LookupCode] = "INTERBASELOOKUP";
				row[Cyber2Schema.Lookup.Ahecc] = "AHECC";
				row[Cyber2Schema.Lookup.TariffDescription] = "TARIFFDESCRIPTION";
				row[Cyber2Schema.Lookup.Statistics] = "ST";
				row[Cyber2Schema.Lookup.Tariff] = "TARIFF";
				row[Cyber2Schema.Lookup.Treatment] = "TREATMENT";
				row[Cyber2Schema.Lookup.InstrumentType] = "tc";
				row[Cyber2Schema.Lookup.InstrumentCode] = "INSTR";

				table.Rows.Add(row);

				return table;
			}
		}

		#endregion

		#region Invoice Table

		DataTable InvoiceTable
		{
			get
			{
				var table = new DataTable("Invoice");

				var account = table.Columns.Add(Cyber2Schema.Invoice.Account, typeof(string));
				var number = table.Columns.Add(Cyber2Schema.Invoice.Number, typeof(string));
				var invoiceDate = table.Columns.Add(Cyber2Schema.Invoice.InvoiceDate, typeof(ZDateTime));
				var dueDate = table.Columns.Add(Cyber2Schema.Invoice.DueDate, typeof(ZDateTime));
				var currency = table.Columns.Add(Cyber2Schema.Invoice.Currency, typeof(string));
				var foreignBalance = table.Columns.Add(Cyber2Schema.Invoice.ForeignBalance, typeof(decimal));
				var localBalance = table.Columns.Add(Cyber2Schema.Invoice.LocalBalance, typeof(decimal));
				var branch = table.Columns.Add(Cyber2Schema.Invoice.Branch, typeof(string));
				var d_C_Flag = table.Columns.Add(Cyber2Schema.Invoice.D_C_Flag, typeof(string));

				var row = table.NewRow();

				row[Cyber2Schema.Invoice.Account] = "Account";
				row[Cyber2Schema.Invoice.Number] = "Number";
				row[Cyber2Schema.Invoice.InvoiceDate] = new ZDateTime(2005, 7, 27);
				row[Cyber2Schema.Invoice.DueDate] = new ZDateTime(2005, 8, 27);
				row[Cyber2Schema.Invoice.Currency] = "AUD";
				row[Cyber2Schema.Invoice.ForeignBalance] = 100.0m;
				row[Cyber2Schema.Invoice.LocalBalance] = 150.0m;
				row[Cyber2Schema.Invoice.Branch] = "Branch";
				row[Cyber2Schema.Invoice.D_C_Flag] = "D";

				table.Rows.Add(row);

				return table;
			}
		}

		#endregion

		#region Receipt Table

		DataTable ReceiptTable
		{
			get
			{
				var table = new DataTable("Receipt");

				var number = table.Columns.Add(Cyber2Schema.Receipt.Number, typeof(int));
				var branch = table.Columns.Add(Cyber2Schema.Receipt.Branch, typeof(string));
				var accountId = table.Columns.Add(Cyber2Schema.Receipt.AccountId, typeof(string));
				var currency = table.Columns.Add(Cyber2Schema.Receipt.Currency, typeof(string));
				var balance = table.Columns.Add(Cyber2Schema.Receipt.Balance, typeof(decimal));
				var date = table.Columns.Add(Cyber2Schema.Receipt.Date, typeof(ZDateTime));
				var d_C_Flag = table.Columns.Add(Cyber2Schema.Receipt.D_C_Flag, typeof(string));

				var row = table.NewRow();

				row[Cyber2Schema.Receipt.Number] = 1029;
				row[Cyber2Schema.Receipt.Branch] = "Branch";
				row[Cyber2Schema.Receipt.AccountId] = "AccountId";
				row[Cyber2Schema.Receipt.Currency] = "AUD";
				row[Cyber2Schema.Receipt.Balance] = 100.0m;
				row[Cyber2Schema.Receipt.Date] = new ZDateTime(2005, 7, 27);
				row[Cyber2Schema.Receipt.D_C_Flag] = "C";

				table.Rows.Add(row);

				return table;
			}
		}

		#endregion

		#endregion
	}
}
