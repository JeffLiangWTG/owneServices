using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class AccountingTransactionExportGetAddressesTest : ScriptTest
	{
		public void TestCorrectPortAndCountry_NonDefaultAddressChosenAndTransactionOverridePresent()
		{
			var orgHeader = CreateOrganizationWithMultipleAddresses();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, orgHeader);
			var factory = TestObjectCreator.Factory;
			invoice.AH_OA_InvoiceAddressOverride = orgHeader.Addresses.ElementAt(1).PK;
			factory.Save();

			using (var command = Db.Connection.Command(
				@"EXEC AccountingTransactionExportGetAddresses '@Company', @BatchNumber, @TransactionHeaderPK"))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.VarChar, "AAA");
				command.AddParameter("@BatchNumber", System.Data.SqlDbType.Int, 1);
				command.AddParameter("@TransactionHeaderPK", System.Data.SqlDbType.UniqueIdentifier, invoice.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					var countryCode = reader["RN_Code"];
					var countryName = reader["RN_Desc"];
					var portCode = reader["RL_Code"];
					var portName = reader["RL_PortName"];
					var address = reader["OA_Address1"];
					AssertEquals("AE", countryCode);
					AssertEquals("United Arab Emirates", countryName);
					AssertEquals("AEFAT", portCode);
					AssertEquals("Fateh Terminal", portName);
					AssertEquals("Cell 123 Irithyll Dungeon", address);
				}
			}
		}

		public void TestCorrectPortAndCountry_DefaultAddressChosenLocalAddressFallback()
		{
			var orgHeader = CreateOrganizationWithMultipleAddresses();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, orgHeader);
			var jobHeader = TestObjectCreator.CreateJobHeader();
			jobHeader.JH_OA_LocalChargesAddr = orgHeader.Addresses.ElementAt(0).PK;
			invoice.AH_JH = jobHeader.PK;
			var factory = TestObjectCreator.Factory;
			factory.Save();

			using (var command = Db.Connection.Command(
				@"EXEC AccountingTransactionExportGetAddresses '@Company', @BatchNumber, @TransactionHeaderPK"))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.VarChar, "AAA");
				command.AddParameter("@BatchNumber", System.Data.SqlDbType.Int, 1);
				command.AddParameter("@TransactionHeaderPK", System.Data.SqlDbType.UniqueIdentifier, invoice.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					var countryCode = reader["RN_Code"];
					var countryName = reader["RN_Desc"];
					var portCode = reader["RL_Code"];
					var portName = reader["RL_PortName"];
					var address = reader["OA_Address1"];
					AssertEquals("AU", countryCode);
					AssertEquals("Australia", countryName);
					AssertEquals("AUABD", portCode);
					AssertEquals("Aberdeen", portName);
					AssertEquals("184 Bourke Road", address);
				}
			}
		}

		public void TestCorrectPortAndCountry_DefaultAddressChosenAgentAddressFallback()
		{
			var orgHeader = CreateOrganizationWithMultipleAddresses();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, orgHeader);
			var jobHeader = TestObjectCreator.CreateJobHeader();
			jobHeader.JH_OA_AgentCollectAddr = orgHeader.Addresses.ElementAt(0).PK;
			invoice.AH_JH = jobHeader.PK;
			var factory = TestObjectCreator.Factory;
			factory.Save();

			using (var command = Db.Connection.Command(
				@"EXEC AccountingTransactionExportGetAddresses '@Company', @BatchNumber, @TransactionHeaderPK"))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.VarChar, "AAA");
				command.AddParameter("@BatchNumber", System.Data.SqlDbType.Int, 1);
				command.AddParameter("@TransactionHeaderPK", System.Data.SqlDbType.UniqueIdentifier, invoice.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					var countryCode = reader["RN_Code"];
					var countryName = reader["RN_Desc"];
					var portCode = reader["RL_Code"];
					var portName = reader["RL_PortName"];
					var address = reader["OA_Address1"];
					AssertEquals("AU", countryCode);
					AssertEquals("Australia", countryName);
					AssertEquals("AUABD", portCode);
					AssertEquals("Aberdeen", portName);
					AssertEquals("184 Bourke Road", address);
				}
			}
		}

		public void TestCorrectPortAndCountry_DefaultAddressChosenOrgARMAddressFallback()
		{
			var orgHeader = CreateOrganizationWithMultipleAddresses();
			var orgAddressCapability = CreateOrganizationAddressCapability((OrgAddress)orgHeader.Addresses.ElementAt(0), "ARM");

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, orgHeader);
			invoice.AH_TransactionType = "INV";
			invoice.AH_Ledger = "AR";
			var jobHeader = TestObjectCreator.CreateJobHeader();
			invoice.AH_JH = jobHeader.PK;
			var factory = TestObjectCreator.Factory;
			factory.Save();

			using (var command = Db.Connection.Command(
				@"EXEC AccountingTransactionExportGetAddresses '@Company', @BatchNumber, @TransactionHeaderPK"))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.VarChar, "AAA");
				command.AddParameter("@BatchNumber", System.Data.SqlDbType.Int, 1);
				command.AddParameter("@TransactionHeaderPK", System.Data.SqlDbType.UniqueIdentifier, invoice.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					var countryCode = reader["RN_Code"];
					var countryName = reader["RN_Desc"];
					var portCode = reader["RL_Code"];
					var portName = reader["RL_PortName"];
					var address = reader["OA_Address1"];
					AssertEquals("AU", countryCode);
					AssertEquals("Australia", countryName);
					AssertEquals("AUABD", portCode);
					AssertEquals("Aberdeen", portName);
					AssertEquals("184 Bourke Road", address);
				}
			}
		}

		public void TestCorrectPortAndCountry_DefaultAddressChosenOrgAPMAddressFallback()
		{
			var orgHeader = CreateOrganizationWithMultipleAddresses();
			var orgAddressCapability = CreateOrganizationAddressCapability((OrgAddress)orgHeader.Addresses.ElementAt(0), "APM");

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, orgHeader);
			invoice.AH_TransactionType = "INV";
			invoice.AH_Ledger = "AP";
			var jobHeader = TestObjectCreator.CreateJobHeader();
			invoice.AH_JH = jobHeader.PK;
			var factory = TestObjectCreator.Factory;
			factory.Save();

			using (var command = Db.Connection.Command(
				@"EXEC AccountingTransactionExportGetAddresses '@Company', @BatchNumber, @TransactionHeaderPK"))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.VarChar, "AAA");
				command.AddParameter("@BatchNumber", System.Data.SqlDbType.Int, 1);
				command.AddParameter("@TransactionHeaderPK", System.Data.SqlDbType.UniqueIdentifier, invoice.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					var countryCode = reader["RN_Code"];
					var countryName = reader["RN_Desc"];
					var portCode = reader["RL_Code"];
					var portName = reader["RL_PortName"];
					var address = reader["OA_Address1"];
					AssertEquals("AU", countryCode);
					AssertEquals("Australia", countryName);
					AssertEquals("AUABD", portCode);
					AssertEquals("Aberdeen", portName);
					AssertEquals("184 Bourke Road", address);
				}
			}
		}

		public void TestCorrectPortAndCountry_DefaultAddressChosenOrgOFCAddressFallback()
		{
			var orgHeader = CreateOrganizationWithMultipleAddresses();

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0m, orgHeader);
			var jobHeader = TestObjectCreator.CreateJobHeader();
			invoice.AH_JH = jobHeader.PK;
			var factory = TestObjectCreator.Factory;
			factory.Save();

			using (var command = Db.Connection.Command(
				@"EXEC AccountingTransactionExportGetAddresses '@Company', @BatchNumber, @TransactionHeaderPK"))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.VarChar, "AAA");
				command.AddParameter("@BatchNumber", System.Data.SqlDbType.Int, 1);
				command.AddParameter("@TransactionHeaderPK", System.Data.SqlDbType.UniqueIdentifier, invoice.PK.ToGuid());

				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					var countryCode = reader["RN_Code"];
					var countryName = reader["RN_Desc"];
					var portCode = reader["RL_Code"];
					var portName = reader["RL_PortName"];
					var address = reader["OA_Address1"];
					AssertEquals("AU", countryCode);
					AssertEquals("Australia", countryName);
					AssertEquals("AUABD", portCode);
					AssertEquals("Aberdeen", portName);
					AssertEquals("184 Bourke Road", address);
				}
			}
		}

		OrgAddressCapability CreateOrganizationAddressCapability(OrgAddress address, string addressType)
		{
			var newFactory = new BusinessObjectFactory();
			OrgAddressCapability oac = newFactory.New<OrgAddressCapability>();
			oac.PZ_OA = address.PK;
			oac.PZ_AddressType = addressType;
			oac.PZ_IsMainAddress = true;
			newFactory.Save();
			return oac;
		}

		OrgHeader CreateOrganizationWithMultipleAddresses()
		{
			var newFactory = new BusinessObjectFactory();
			OrgHeader header = newFactory.New<OrgHeader>();

			header.OH_FullName = "Octo Latria Patroleum";
			header.OH_Code = "ZLATPA";

			header.MainAddress.OA_Address1 = "184 Bourke Road";
			header.MainAddress.OA_City = "Alexandria";
			header.MainAddress.OA_State = "NSW";
			header.MainAddress.OA_RL_NKRelatedPortCode = "AUABD";
			header.MainAddress.OA_RN_NKCountryCode = "AU";

			header.OH_IsDebtor = true;
			header.OH_IsCreditor = false;

			var address = header.Addresses.AddNew();
			address.OA_Address1 = "Cell 123 Irithyll Dungeon";
			address.OA_City = "Irithyll of the Boreal Valley";
			address.OA_State = "Lordran";
			address.OA_RL_NKRelatedPortCode = "AEFAT";
			address.OA_RN_NKCountryCode = "AE";

			newFactory.Save();

			return newFactory.Load<OrgHeader>(header.PK);
		}
	}
}
