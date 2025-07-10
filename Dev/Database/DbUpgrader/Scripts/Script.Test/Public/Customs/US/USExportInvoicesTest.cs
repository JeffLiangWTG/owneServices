using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USExportInvoices))]
	class USExportInvoicesTest : DbCreateScriptTest
	{
		class Address
		{
			public Guid OrgPK { get; set; }
			public string OrgCode { get; set; }
			public string OrgFullName { get; set; }
			public string Address1 { get; set; }
			public string Address2 { get; set; }
			public string City { get; set; }
			public string State { get; set; }
			public string PostCode { get; set; }
			public string Contact { get; set; }
			public string Phone { get; set; }
			public string IDNumber { get; set; }
		}

		public void TestMainSupplierIDNumber()
		{
			var supplierOrgPK = TestDataCreator.CreateOrganisation("Supplier", "Test Supplier", "USPHL");
			var supplierAddressPK = TestDataCreator.CreateAddress(supplierOrgPK, "Supplier Address", "Address 1");
			TestDataCreator.CreateOrgAddressCapability(supplierAddressPK, "OFC", true);
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, supplierAddressPK, "DUN", "758516744", "US");

			var companyPK = TestDataCreator.CreateCompany("DUS", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "PHL", "USPHL");
			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B0000001", "EXP", 1, null, supplierOrgPK, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, invoiceNumber: "INV000001", dataModel: "US");

			var script = $@"SELECT * FROM USExportInvoices('{companyPK}','','','','')";
			using (var command = TestConnection.Command(script))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				CombineAssertions(() =>
				{
					AssertEquals("B0000001", reader["JE_DeclarationReference"].ToString());
					AssertEquals("INV000001", reader["JZ_InvoiceNumber"].ToString());
					AssertEquals("DUN: 758516744", reader["MainSupplierIDNumber"].ToString());
				});
			}
		}

		public void TestOrgCusCode()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var supplierOrgPK = TestDataCreator.CreateOrganisation("ORG1", "Company Name 1");
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, "EIN", "EIN_CODE 1", "US");
			TestDataCreator.CreateOrgCusCode(supplierOrgPK, "DUN", "DUN_CODE 1", "US");
			var forwarderOrgPK = TestDataCreator.CreateOrganisation("ORG2", "Company Name 2");
			TestDataCreator.CreateOrgCusCode(forwarderOrgPK, "EIN", "EIN_CODE 2", "US");
			TestDataCreator.CreateOrgCusCode(forwarderOrgPK, "DUN", "DUN_CODE 2", "US");

			var usPPIOrgPK = TestDataCreator.CreateOrganisation("ORG3", "Company Name 3");
			TestDataCreator.CreateOrgCusCode(usPPIOrgPK, "EIN", "EIN_CODE 3", "US");
			TestDataCreator.CreateOrgCusCode(usPPIOrgPK, "DUN", "DUN_CODE 3", "US");
			var ultimateConsigneeOrgPK = TestDataCreator.CreateOrganisation("ORG4", "Company Name 4");
			TestDataCreator.CreateOrgCusCode(ultimateConsigneeOrgPK, "EIN", "EIN_CODE 4", "US");
			TestDataCreator.CreateOrgCusCode(ultimateConsigneeOrgPK, "DUN", "DUN_CODE 4", "US");
			var intermConsigneeOrgPK = TestDataCreator.CreateOrganisation("ORG5", "Company Name 5");
			TestDataCreator.CreateOrgCusCode(intermConsigneeOrgPK, "EIN", "EIN_CODE 5", "US");
			TestDataCreator.CreateOrgCusCode(intermConsigneeOrgPK, "DUN", "DUN_CODE 5", "US");
			var fPPIOrgPK = TestDataCreator.CreateOrganisation("ORG6", "Company Name 1");
			TestDataCreator.CreateOrgCusCode(fPPIOrgPK, "EIN", "EIN_CODE 6", "US");
			TestDataCreator.CreateOrgCusCode(fPPIOrgPK, "DUN", "DUN_CODE 6", "US");

			var addressPK1 = TestDataCreator.CreateAddress(usPPIOrgPK, "Add1", "Road1", "Street1", "City1", "State1", "Post1");
			var addressPK2 = TestDataCreator.CreateAddress(ultimateConsigneeOrgPK, "Add2", "Road2", "Street2", "City2", "State2", "Post2");
			var addressPK3 = TestDataCreator.CreateAddress(intermConsigneeOrgPK, "Add3", "Road3", "Street3", "City3", "State3", "Post3");
			var shipmentPK1 = TestDataCreator.CreateShipment("S001");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "EXP", 1, shipmentPK1, supplierPK: supplierOrgPK, forwarderPK: forwarderOrgPK, dataModel: "US");
			var usDeclarationPK1 = TestDataCreator.CreateJobUSDeclaration(declarationPK1, fPPIOrgPK, 1);
			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			TestDataCreator.CreateDocAddress(addressPK1, "", invoiceHeaderPK1, "JZ", "PPI", "", "", "", "", "", "Cont1", "", "", "", false);
			TestDataCreator.CreateDocAddress(addressPK2, "", invoiceHeaderPK1, "JZ", "UCE", "", "", "", "", "", "Cont2", "", "", "", false);
			TestDataCreator.CreateDocAddress(addressPK3, "", invoiceHeaderPK1, "JZ", "ICE", "", "", "", "", "", "Cont3", "", "", "", false);

			var reportSql = $"SELECT UltimateConsigneeIDNumber, IntermConsigneeIDNumber, MainSupplierIDNumber, ForwarderIDNumber, USPPIIDNumber, FPPIIDNumber FROM USExportInvoices('{companyPK}', '', '', '', '')";

			TestConnection.ExecuteReader(
				reportSql,
				(reader) =>
				{
					CombineAssertions(() =>
					{
						AssertEquals("EIN: EIN_CODE 1", (string)reader["MainSupplierIDNumber"]);
						AssertEquals("EIN: EIN_CODE 2", (string)reader["ForwarderIDNumber"]);
						AssertEquals("EIN: EIN_CODE 3", (string)reader["USPPIIDNumber"]);
						AssertEquals("EIN: EIN_CODE 4", (string)reader["UltimateConsigneeIDNumber"]);
						AssertEquals("EIN: EIN_CODE 5", (string)reader["IntermConsigneeIDNumber"]);
						AssertEquals("EIN: EIN_CODE 6", (string)reader["FPPIIDNumber"]);
					});
				}
			);
		}

		public void TestSupplierPickupAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var organisationPK1 = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var addressPK1 = TestDataCreator.CreateAddress(organisationPK1, "Delivery Address", "Address 1");
			var shipmentPK1 = TestDataCreator.CreateShipment("S001");
			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "S001", "EXP", 1, shipmentPK1, organisationPK1, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			TestDataCreator.CreateDocAddress(addressPK1, "", shipmentPK1, "JS", "CRG");

			var organisationPK2 = TestDataCreator.CreateOrganisation("OrgCode2", "Company Name 2");
			var addressPK2 = TestDataCreator.CreateAddress(organisationPK2, "Delivery Address", "Address 1");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "EXP", 2, null, organisationPK2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");
			TestDataCreator.CreateDocAddress(addressPK2, "", declarationPK2, "JE", "SUG");

			var shipmentPK2 = TestDataCreator.CreateShipment("S002");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "S002", "EXP", 3, shipmentPK2, organisationPK2, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK3, false, 3, dataModel: "US");
			TestDataCreator.CreateDocAddress(addressPK2, "", shipmentPK2, "JS", "CRG");

			var declarationPK4 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B004", "EXP", 4, null, organisationPK1, dataModel: "US");
			TestDataCreator.CreateJobComInvoiceHeader(declarationPK4, false, 4, dataModel: "US");
			TestDataCreator.CreateDocAddress(addressPK1, "", declarationPK4, "JE", "SUG");

			var sql = @"select JE_DeclarationReference, USPPIPK, USPPICode, USPPIFullName from dbo.USExportInvoices(@companyPK, '', '', '', '')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var addresses = new Dictionary<string, Address>();
					while (reader.Read())
					{
						addresses[reader.GetString(0)] = new Address()
						{
							OrgPK = reader.GetGuid(1),
							OrgCode = reader.GetString(2),
							OrgFullName = reader.GetString(3),
						};
					}
					CombineAssertions(() =>
					{
						AssertAddress(addresses["S001"], organisationPK1, "OrgCode1", "Company Name 1");
						AssertAddress(addresses["B002"], organisationPK2, "OrgCode2", "Company Name 2");
						AssertAddress(addresses["S002"], organisationPK2, "OrgCode2", "Company Name 2");
						AssertAddress(addresses["B004"], organisationPK1, "OrgCode1", "Company Name 1");
					});
				}
			}
		}

		public void TestUSPPIUltimateConsigneeIntermConsigneeAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var organisationPK1 = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			TestDataCreator.CreateOrgCusCode(organisationPK1, "EIN", "11-987654321", "US");
			var addressPK1 = TestDataCreator.CreateAddress(organisationPK1, "Add1", "Road1", "Street1", "City1", "State1", "Post1");
			var addressPK2 = TestDataCreator.CreateAddress(organisationPK1, "Add2", "Road2", "Street2", "City2", "State2", "Post2");
			var addressPK3 = TestDataCreator.CreateAddress(organisationPK1, "Add3", "Road3", "Street3", "City3", "State3", "Post3");
			TestDataCreator.CreateContact(organisationPK1, "Cont1", "Pho1");
			TestDataCreator.CreateContact(organisationPK1, "Cont2", "Pho2");
			TestDataCreator.CreateContact(organisationPK1, "Cont3", "Pho3");
			var shipmentPK1 = TestDataCreator.CreateShipment("S001");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "EXP", 1, shipmentPK1, organisationPK1, dataModel: "US");
			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			TestDataCreator.CreateDocAddress(addressPK1, "", invoiceHeaderPK1, "JZ", "PPI", "", "", "", "", "", "Cont1", "", "", "", false);
			TestDataCreator.CreateDocAddress(addressPK2, "", invoiceHeaderPK1, "JZ", "UCE", "", "", "", "", "", "Cont2", "", "", "", false);
			TestDataCreator.CreateDocAddress(addressPK3, "", invoiceHeaderPK1, "JZ", "ICE", "", "", "", "", "", "Cont3", "", "", "", false);

			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "EXP", 2, shipmentPK1, organisationPK1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");
			TestDataCreator.CreateDocAddress(Guid.Empty, "", invoiceHeaderPK2, "JZ", "PPI", "Doc Road1", "Doc Street1", "Doc City1", "Doc State1", "Doc Post1", "Doc Cont1", "Doc Pho1", "EIN", "11-123456789", true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "", invoiceHeaderPK2, "JZ", "UCE", "Doc Road2", "Doc Street2", "Doc City2", "Doc State2", "Doc Post2", "Doc Cont2", "Doc Pho2", "EIN", "22-123456789", true);
			TestDataCreator.CreateDocAddress(Guid.Empty, "", invoiceHeaderPK2, "JZ", "ICE", "Doc Road3", "Doc Street3", "Doc City3", "Doc State3", "Doc Post3", "Doc Cont3", "Doc Pho3", "DUN", "33-123456789", true);

			var sql = @"select JE_DeclarationReference, USPPIPK, USPPICode, USPPIFullName, USPPIAddress1, USPPIAddress2, USPPICity, USPPIState, USPPIPostCode, USPPIContact, USPPIPhone, USPPIIDNumber from dbo.USExportInvoices(@companyPK, '', '', '', '')";
			var usppiAddressDictionary = GetAddressDictionary(sql, companyPK);
			AssertAddress(usppiAddressDictionary["B001"], organisationPK1, "OrgCode1", "Company Name 1", "Road1", "Street1", "City1", "State1", "Post1", "Cont1", "Pho1", "EIN: 11-987654321");
			AssertAddress(usppiAddressDictionary["B002"], Guid.Empty, "", "", "Doc Road1", "Doc Street1", "Doc City1", "Doc State1", "Doc Post1", "Doc Cont1", "Doc Pho1", "EIN: 11-123456789");

			sql = @"select JE_DeclarationReference, UltimateConsigneePK, UltimateConsigneeCode, UltimateConsigneeFullName, UltimateConsigneeAddress1, UltimateConsigneeAddress2, UltimateConsigneeCity, UltimateConsigneeState, UltimateConsigneePostCode, UltimateConsigneeContact, UltimateConsigneePhone, UltimateConsigneeIDNumber from dbo.USExportInvoices(@companyPK, '', '', '', '')";
			var ultimateConsigneeAddressDictionary = GetAddressDictionary(sql, companyPK);
			AssertAddress(ultimateConsigneeAddressDictionary["B001"], organisationPK1, "OrgCode1", "Company Name 1", "Road2", "Street2", "City2", "State2", "Post2", "Cont2", "Pho2", "EIN: 11-987654321");
			AssertAddress(ultimateConsigneeAddressDictionary["B002"], Guid.Empty, "", "", "Doc Road2", "Doc Street2", "Doc City2", "Doc State2", "Doc Post2", "Doc Cont2", "Doc Pho2", "EIN: 22-123456789");

			sql = @"select JE_DeclarationReference, IntermConsigneePK, IntermConsigneeCode, IntermConsigneeFullName, IntermConsigneeAddress1, IntermConsigneeAddress2, IntermConsigneeCity, IntermConsigneeState, IntermConsigneePostCode, IntermConsigneeContact, IntermConsigneePhone, IntermConsigneeIDNumber from dbo.USExportInvoices(@companyPK, '', '', '', '')";
			var intermConsigneeAddressDictionary = GetAddressDictionary(sql, companyPK);
			AssertAddress(intermConsigneeAddressDictionary["B001"], organisationPK1, "OrgCode1", "Company Name 1", "Road3", "Street3", "City3", "State3", "Post3", "Cont3", "Pho3", "EIN: 11-987654321");
			AssertAddress(intermConsigneeAddressDictionary["B002"], Guid.Empty, "", "", "Doc Road3", "Doc Street3", "Doc City3", "Doc State3", "Doc Post3", "Doc Cont3", "Doc Pho3", "DUN: 33-123456789");
		}

		Dictionary<string, Address> GetAddressDictionary(string sql, Guid companyPK)
		{
			var addresses = new Dictionary<string, Address>();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						addresses[reader.GetString(0)] = new Address()
						{
							OrgPK = reader[1] == DBNull.Value ? Guid.Empty : (Guid)reader[1],
							OrgCode = (string)reader[2],
							OrgFullName = (string)reader[3],
							Address1 = (string)reader[4],
							Address2 = (string)reader[5],
							City = (string)reader[6],
							State = (string)reader[7],
							PostCode = (string)reader[8],
							Contact = (string)reader[9],
							Phone = (string)reader[10],
							IDNumber = (string)reader[11],
						};
					}
				}
			}
			return addresses;
		}

		void AssertAddress(Address address, Guid organisationPK, string orgCode, string orgName)
		{
			AssertEquals("OrgPK", organisationPK, address.OrgPK);
			AssertEquals("OrgCode", orgCode, address.OrgCode);
			AssertEquals("OrgFullName", orgName, address.OrgFullName);
		}

		void AssertAddress(Address address, Guid organisationPK, string orgCode, string orgName, string address1, string address2, string city, string state, string postCode, string contact, string phone, string idNumber)
		{
			AssertAddress(address, organisationPK, orgCode, orgName);
			AssertEquals("Address1", address1, address.Address1);
			AssertEquals("Address2", address2, address.Address2);
			AssertEquals("City", city, address.City);
			AssertEquals("State", state, address.State);
			AssertEquals("PostCode", postCode, address.PostCode);
			AssertEquals("Contact", contact, address.Contact);
			AssertEquals("Phone", phone, address.Phone);
			AssertEquals("IDNumber", idNumber, address.IDNumber);
		}
	}
}
