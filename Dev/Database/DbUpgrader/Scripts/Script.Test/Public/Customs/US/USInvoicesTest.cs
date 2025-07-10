using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USInvoices))]
	class USInvoicesTest : DbCreateScriptTest
	{
		public void TestTotalInvoiceDuty()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACS', 0, 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLinePK = Guid.NewGuid();

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK, 'US', @invoiceHeaderPK, 1, 10000.00, 'CustomsValue=10000*Duty=420*FTADuty=420*FTAPayableMPF=34.64*FTASPI=C*NonFTADuty=420*NonFTAPayableMPF=34.64*PayableMPF=34.64*SPI=N/A*SupDuty=2000*SupTariff=99030124*UC_NKCountryOfOrigin=CN', 1)";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);

				command.ExecuteNonQuery();
			}

			var addDuty1PK = Guid.NewGuid();
			var addDuty2PK = Guid.NewGuid();

			var cusLineTariffDetailSql = @"
INSERT INTO dbo.CusLineTariffDetail(BZ_PK, BZ_DataModel, BZ_IsValid, BZ_NAddInfo, BZ_ParentID, BZ_ParentTableCode, BZ_Type)
VALUES (@addDuty1PK, 'US', 1, 'SupDuty=12500', @invoiceLinePK, 'JI', 'AT1'),
(@addDuty2PK, 'US', 1, 'SupDuty=2500', @invoiceLinePK, 'JI', 'AT2')";
			using (var command = Db.Connection.Command(cusLineTariffDetailSql))
			{
				command.AddParameter("@addDuty1PK", SqlDbType.UniqueIdentifier, addDuty1PK);
				command.AddParameter("@addDuty2PK", SqlDbType.UniqueIdentifier, addDuty2PK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JZ_PK, InvoiceDuty from dbo.USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = reader.GetDecimal(1);
					}

					AssertEquals("Total invoice duty value.", 17420.00m, invoices[invoiceHeaderPK]);
				}
			}
		}

		public void TestJE_OH_ExporterHasMultipleMIDs()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var org = TestDataCreator.CreateOrganisation("ORG1", "ORG1 NAME");
			var address1 = TestDataCreator.CreateAddress(org, "ORG1 Address", "Address 11111");
			var address2 = TestDataCreator.CreateAddress(org, "ORG2 Address", "Address 22222");
			var cusCode1 = TestDataCreator.CreateOrgCusCode(org, "MID", "USMIDTEST1", "US", address1);
			var cusCode2 = TestDataCreator.CreateOrgCusCode(org, "MID", "USMIDTEST2", "US", address2);

			var declaration = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, exporterPK: org);
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, dataModel: "US");

			var reportSql = @"select count(*) from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(1, (int)reader[0]);
					}
					AssertEquals(1, count);
				}
			}
		}

		public void TestInvoiceEffectiveManufacturerAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var org = TestDataCreator.CreateOrganisation("MAN1", "Manufacturer 1");
			var address = TestDataCreator.CreateAddress(org, "Manufacturer Address", "Address 11111");

			var org2 = TestDataCreator.CreateOrganisation("MAN2", "Manufacturer 2");
			var address2 = TestDataCreator.CreateAddress(org2, "Manufacturer Address2", "Address 22222");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_ManufacturerAddress, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACE', 0, @stpAddress, 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@stpAddress", SqlDbType.UniqueIdentifier, address);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLinePK11 = Guid.NewGuid();

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, 'CustomsValue=1', 1)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JZ_PK, InvoiceEffectiveManufacturerAddress from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = (Guid)reader["InvoiceEffectiveManufacturerAddress"];
					}

					AssertEquals("ManufacturerAddress on Declaration", address, invoices[invoiceHeaderPK1]);
				}
			}

			var headerSql = @"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OA_ManufacturerAddress = @stpAddress,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_JE = @declarationPK and JZ_PK = @invoiceHeaderPK1";

			using (var command = Db.Connection.Command(headerSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@stpAddress", SqlDbType.UniqueIdentifier, address2);
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.ExecuteNonQuery();
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = (Guid)reader["InvoiceEffectiveManufacturerAddress"];
					}

					AssertEquals("ManufacturerAddress on Invoice Header", address2, invoices[invoiceHeaderPK1]);
				}
			}
		}

		public void TestInvoiceEffectiveShipToPartyAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var org = TestDataCreator.CreateOrganisation("STP1", "SHIP TO PARTY");
			var address = TestDataCreator.CreateAddress(org, "ShipToParty Address", "Address 11111");

			var org2 = TestDataCreator.CreateOrganisation("STP2", "SHIP TO PARTY2");
			var address2 = TestDataCreator.CreateAddress(org2, "ShipToParty Address2", "Address 22222");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_ShipToPartyAddress, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACE', 0, @stpAddress, 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@stpAddress", SqlDbType.UniqueIdentifier, address);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLinePK11 = Guid.NewGuid();

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, 'CustomsValue=1', 1)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JZ_PK, InvoiceEffectiveShipToPartyAddress from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = (Guid)reader["InvoiceEffectiveShipToPartyAddress"];
					}

					AssertEquals("ShipToPartyAddress on Declaration", address, invoices[invoiceHeaderPK1]);
				}
			}

			var headerSql = @"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OA_ShipToPartyAddress = @stpAddress,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_JE = @declarationPK and JZ_PK = @invoiceHeaderPK1";

			using (var command = Db.Connection.Command(headerSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@stpAddress", SqlDbType.UniqueIdentifier, address2);
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.ExecuteNonQuery();
			}

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, Guid>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = (Guid)reader["InvoiceEffectiveShipToPartyAddress"];
					}

					AssertEquals("ShipToPartyAddress on Invoice Header", address2, invoices[invoiceHeaderPK1]);
				}
			}
		}

		public void TestInvoiceEffectiveExporterAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");
			var org = TestDataCreator.CreateOrganisation("FEXP", "EXPORTER");
			var address = TestDataCreator.CreateAddress(org, "Exporter Address", "Address 11111");
			TestDataCreator.CreateOrgAddressCapability(address, "OFC", true);
			var address2 = TestDataCreator.CreateAddress(org, "Exporter Address2", "Address 22222");
			TestDataCreator.CreateOrgAddressCapability(address2, "OFC", false);
			var orgCusCode = TestDataCreator.CreateOrgCusCode(org, "MID", "USMIDTEST", "US", address2);

			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, exporterPK: org);
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var reportSql = @"select JZ_PK, ExporterPK, ExporterAddressPK, ExporterMID from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);

				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, Guid>();
					var exporterAddressPK = Guid.Empty;
					var exporterMID = string.Empty;
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = (Guid)reader["ExporterPK"];
						exporterAddressPK = (Guid)reader["ExporterAddressPK"];
						exporterMID = (string)reader["ExporterMID"];
					}

					AssertEquals("ExporterPK", org, invoices[invoiceHeaderPK]);
					AssertEquals("ExporterAddressPK", address2, exporterAddressPK);
					AssertEquals("ExporterMID", "USMIDTEST", exporterMID);
				}
			}
		}

		public void TestUSCountryOfExportValueFromAddInfoInLineOnReport()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationAddInfo = "";
			var declarationPK = CreateJobDeclaration(importerPK, branchPK, companyPK, 1, addInfo: declarationAddInfo);

			var billAddInfo = "UC_NKCountryOfExport=AU";
			var billPK = TestDataCreator.CreateCusDecHouseBill(true, billAddInfo, declarationPK, 1);

			var invoiceHeaderAddInfo = "";
			var invoiceHeaderPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, addInfo: invoiceHeaderAddInfo, relatedHouseBillPK: billPK, dataModel: "US");

			var invoiceLineAddInfo = "UC_NKCountryOfExport=CH";
			TestDataCreator.CreateJobComInvoiceLine(invoiceHeaderPK, 1, addInfo: invoiceLineAddInfo);

			var reportSql = @"select JE_PK, CountryOfExport from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var reportList = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						reportList[reader.GetGuid(0)] = (string)reader["CountryOfExport"];
					}
					AssertEquals(1, reportList.Count);
					AssertEquals("CountryOfExport", "AU", reportList[declarationPK]);
				}
			}
		}

		public void TestTotalEnteredValue()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACS', 0, 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");

			var invoiceLinePK11 = Guid.NewGuid();
			var invoiceLinePK12 = Guid.NewGuid();
			var invoiceLinePK13 = Guid.NewGuid();

			var invoiceLinePK21 = Guid.NewGuid();
			var invoiceLinePK22 = Guid.NewGuid();
			var invoiceLinePK23 = Guid.NewGuid();
			var invoiceLinePK24 = Guid.NewGuid();

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, 'CustomsValue=1', 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK12, 'US', @invoiceHeaderPK1, 2, 2.00, 'CustomsValue=2', 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES( @invoiceLinePK13, 'US', @invoiceHeaderPK1, 3, 3.00, 'CustomsValue=3', 1)

INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK21, 'US', @invoiceHeaderPK2, 1, 4.00, 'CustomsValue=4', 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK22, 'US', @invoiceHeaderPK2, 2, 5.00, 'CustomsValue=13*IsParent=Y*SecondarySPI=X', 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ParentID, JI_ClusterKey)
VALUES (@invoiceLinePK23, 'US', @invoiceHeaderPK2, 3, 6.00, 'CustomsValue=6*SecondarySPI=V', @invoiceLinePK22, 1)
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ParentID, JI_ClusterKey)
VALUES (@invoiceLinePK24, 'US', @invoiceHeaderPK2, 4, 7.00, 'CustomsValue=7*SecondarySPI=V', @invoiceLinePK22, 1)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceHeaderPK2", SqlDbType.UniqueIdentifier, invoiceHeaderPK2);

				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);
				command.AddParameter("@invoiceLinePK12", SqlDbType.UniqueIdentifier, invoiceLinePK12);
				command.AddParameter("@invoiceLinePK13", SqlDbType.UniqueIdentifier, invoiceLinePK13);

				command.AddParameter("@invoiceLinePK21", SqlDbType.UniqueIdentifier, invoiceLinePK21);
				command.AddParameter("@invoiceLinePK22", SqlDbType.UniqueIdentifier, invoiceLinePK22);
				command.AddParameter("@invoiceLinePK23", SqlDbType.UniqueIdentifier, invoiceLinePK23);
				command.AddParameter("@invoiceLinePK24", SqlDbType.UniqueIdentifier, invoiceLinePK24);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JZ_PK, InvoiceEnteredValue from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, decimal>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = reader.GetDecimal(1);
					}

					AssertEquals("Should be a straightforward sum of customs values for Invoice 1.", 6.00m, invoices[invoiceHeaderPK1]);
					AssertEquals("Should be a sum of customs values for Invoice 2 that excludes the set X invoice line.", 17.00m, invoices[invoiceHeaderPK2]);
					AssertEquals("Total entered invoice value.", 23.00m, invoices[invoiceHeaderPK1] + invoices[invoiceHeaderPK2]);
				}
			}
		}

		public void TestJZ_OA_Seller()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var sellerOrg = TestDataCreator.CreateOrganisation("SEL2", "SELLER TWO");
			var sellerAddress = TestDataCreator.CreateAddress(sellerOrg, "Seller2 Address", "Address 11111");

			var headerSellerOrg = TestDataCreator.CreateOrganisation("SELH", "SELLER on HEADER");
			var headerSellerAddress = TestDataCreator.CreateAddress(headerSellerOrg, "SellerH Address", "Header Address 333");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_SellerAddress, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACE', 0, @sellerAddress, 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@sellerAddress", SqlDbType.UniqueIdentifier, sellerAddress);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLinePK11 = Guid.NewGuid();

			var headerSql = @"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_OA_SellerAddress = @sellerHAddress,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_JE = @declarationPK and JZ_PK = @invoiceHeaderPK1";

			using (var command = Db.Connection.Command(headerSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@sellerHAddress", SqlDbType.UniqueIdentifier, headerSellerAddress);
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.ExecuteNonQuery();
			}

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, 'CustomsValue=1', 1)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JZ_PK, SellerCode from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = reader.GetString(1);
					}

					AssertEquals("Seller Code on JobComInvoiceHeader", "SELH", invoices[invoiceHeaderPK1]);
				}
			}
		}

		public void TestJE_OA_Seller()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var importerPK = TestDataCreator.CreateOrganisation("OrgCode1", "Company Name 1");

			var sellerOrg = TestDataCreator.CreateOrganisation("SEL2", "SELLER TWO");
			var sellerAddress = TestDataCreator.CreateAddress(sellerOrg, "Seller2 Address", "Address 11111");

			var headerSellerOrg = TestDataCreator.CreateOrganisation("SELH", "SELLER on HEADER");
			var headerSellerAddress = TestDataCreator.CreateAddress(headerSellerOrg, "SellerH Address", "Header Address 333");

			var declarationPK = Guid.NewGuid();

			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_SellerAddress, JE_ClusterKey)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'BJOB1', 'ACE', 0, @sellerAddress, 1)";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@sellerAddress", SqlDbType.UniqueIdentifier, sellerAddress);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(declarationPK, "JobDeclaration", "EntryNum1", "ENS", "CUS", "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "US");
			var invoiceLinePK11 = Guid.NewGuid();

			var invoiceLineSql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_LineNo, JI_LinePrice, JI_AddInfo, JI_ClusterKey)
VALUES (@invoiceLinePK11, 'US', @invoiceHeaderPK1, 1, 1.00, 'CustomsValue=1', 1)
";
			using (var command = Db.Connection.Command(invoiceLineSql))
			{
				command.AddParameter("@invoiceHeaderPK1", SqlDbType.UniqueIdentifier, invoiceHeaderPK1);
				command.AddParameter("@invoiceLinePK11", SqlDbType.UniqueIdentifier, invoiceLinePK11);

				command.ExecuteNonQuery();
			}

			var reportSql = @"select JZ_PK, SellerCode from USInvoices(@companyPK,'','','','','','','',@importerPK,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				using (var reader = command.ExecuteReader())
				{
					var invoices = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						invoices[reader.GetGuid(0)] = reader.GetString(1);
					}

					AssertEquals("Seller Code on JobComInvoiceHeader", "SEL2", invoices[invoiceHeaderPK1]);
				}
			}
		}

		public void TestPGAExpeditedRelease()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = Guid.NewGuid();
			var declarationPK2 = Guid.NewGuid();

			var declarationSql = @"
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_AddInfo, JE_ClusterKey) VALUES (@declarationPK1, 'US', 'IMP', @branchPK, @companyPK, 'JOB1', 'ACS', 0, 'PGAExpeditedRelease=Y', 1);
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_ClusterKey) VALUES (@declarationPK2, 'US', 'IMP', @branchPK, @companyPK, 'JOB2', 'ACS', 0, 2);";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				command.AddParameter("@declarationPK2", SqlDbType.UniqueIdentifier, declarationPK2);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				command.ExecuteNonQuery();
			}

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");

			var reportSql = @"select JE_DeclarationReference, PGAExpeditedRelease from USInvoices(@companyPK,'','','','','','','', null,'', '', 'Y')";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var pgaExpeditedRelease = "";
					var job = "";
					while (reader.Read())
					{
						job = reader.GetString(0);
						pgaExpeditedRelease = reader.GetString(1);
					}

					AssertEquals("PGA Expedited Release", "JOB1", job);
					AssertEquals("PGA Expedited Release", "Y", pgaExpeditedRelease);
				}
			}
		}

		public void TestFTZNo()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, addInfo: "FTZNo=123456", dataModel: "US");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B002", "IMP", 2, dataModel: "US");

			var invoiceHeaderPK1 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK1, false, 1, dataModel: "US");
			var invoiceHeaderPK2 = TestDataCreator.CreateJobComInvoiceHeader(declarationPK2, false, 2, dataModel: "US");

			var sql = @"select JZ_PK, FTZNo from USInvoices(@companyPK,'','','','','','','', null,'', '', '')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var data = new Dictionary<Guid, object>();
					while (reader.Read())
					{
						data.Add(reader.GetGuid(0), reader.GetValue(1));
					}
					CombineAssertions(() =>
					{
						AssertEquals("invoiceHeaderPK1", "123456", data[invoiceHeaderPK1].ToString());
						AssertEquals("invoiceHeaderPK2", string.Empty, data[invoiceHeaderPK2]);
					});
				}
			}
		}

		public void TestInvoicerDocAddress()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			var org = TestDataCreator.CreateOrganisation("ORG1", "ORG1 NAME");
			var address = TestDataCreator.CreateAddress(org, "ORG1 Address", "Address 11111");
			var cusCode = TestDataCreator.CreateOrgCusCode(org, "MID", "USMIDTEST", "US", address);

			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B001", "IMP", 1, dataModel: "US");
			var invoiceHeader = TestDataCreator.CreateJobComInvoiceHeader(declaration, false, 1, dataModel: "US");
			var docaddress = TestDataCreator.CreateDocAddress(address, "", invoiceHeader, "JZ", "IVC");

			var reportSql = @"select InvoiceOrgPK, InvoiceOrgCode, InvoiceOrgName, InvoiceOrgMID from USInvoices(@companyPK,'','','','','','','',null,'', '', '')";
			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(org, (Guid)reader[0]);
						AssertEquals("ORG1", (string)reader[1]);
						AssertEquals("ORG1 NAME", (string)reader[2]);
						AssertEquals("USMIDTEST", (string)reader[3]);
					}
					AssertEquals(1, count);
				}
			}
		}

		public Guid CreateJobDeclaration(Guid importerPK, Guid branchPK, Guid companyPK, int clusterKey, Guid? shipToPartyAddressPK = null, Guid? manufacturerAddressPK = null, Guid? exporterPK = null, string addInfo = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_OH_Importer, JE_DeclarationReference, JE_ApplicationCode, JE_IsCancelled, JE_OA_ShipToPartyAddress, JE_OA_ManufacturerAddress, JE_OH_Exporter, JE_ClusterKey, JE_AddInfo, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@declarationPK, 'US', 'IMP', @branchPK, @companyPK, @importerPK, 'B01TEST', 'ACE', 0, @shipToPartyAddressPK, @manufacturerAddressPK, @exporterPK, @clusterKey, @addInfo, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(declarationSql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				if (shipToPartyAddressPK != null)
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, shipToPartyAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@shipToPartyAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				if (manufacturerAddressPK != null)
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, manufacturerAddressPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@manufacturerAddressPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				if (exporterPK != null)
				{
					command.AddParameter("@exporterPK", SqlDbType.UniqueIdentifier, exporterPK.GetValueOrDefault());
				}
				else
				{
					command.AddParameter("@exporterPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}
	}
}
