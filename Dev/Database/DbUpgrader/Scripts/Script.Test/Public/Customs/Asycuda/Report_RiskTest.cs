using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.Asycuda;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Asycuda
{
	[TestedType(typeof(Report_Risk))]
	class Report_RiskTest : DbCreateScriptTest
	{
		public void TestTableValuedFunction()
		{
			PrepareTestData();

			var sql = @"SELECT * FROM dbo.Report_Risk(@EffectiveDate, @EnterpriseCode, @ServerCode) ORDER BY JE_DeclarationReference";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@EffectiveDate", SqlDbType.SmallDateTime, DateTime.Today);
				command.AddParameter("@EnterpriseCode", SqlDbType.VarChar, string.Empty);
				command.AddParameter("@ServerCode", SqlDbType.VarChar, string.Empty);

				var report = new Dictionary<string, List<string>>();
				using (var reader = command.ExecuteReader())
				{
					var rowNumber = 0;
					while (reader.Read())
					{
						rowNumber++;
						AssertEquals("Report Columns", 28, reader.FieldCount);

						var row = new List<string>();
						row.Add(ConvertToString(reader["GC_Code"]));
						row.Add(ConvertToString(reader["GC_Name"]));
						row.Add(ConvertToString(reader["GB_Code"]));
						row.Add(ConvertToString(reader["GB_BranchName"]));
						row.Add(ConvertToString(reader["GE_Code"]));
						row.Add(ConvertToString(reader["GE_Desc"]));
						row.Add(ConvertToString(reader["JE_TransportMode"]));
						row.Add(ConvertToString(reader["JE_MessageType"]));
						row.Add(ConvertToString(reader["JE_DeclarationReference"]));
						row.Add(ConvertToString(reader["CE_EntryNum"]));
						row.Add(ConvertToString(reader["CH_BGMReference"]));
						row.Add(ConvertToString(reader["CH_EntryStatus"]));
						row.Add(ConvertToString(reader["CH_EntryReleaseDate"]));
						row.Add(ConvertToString(reader["ImporterCode"]));
						row.Add(ConvertToString(reader["ImporterName"]));
						row.Add(ConvertToString(reader["SupplierCode"]));
						row.Add(ConvertToString(reader["SupplierName"]));
						row.Add(ConvertToString(reader["CustomerCode"]));
						row.Add(ConvertToString(reader["CustomerName"]));
						row.Add(ConvertToString(reader["CEI_Style"]));
						row.Add(ConvertToString(reader["CH_BondValidToDate"]));
						row.Add(ConvertToString(reader["RemainingCustomsValue"]));
						row.Add(ConvertToString(reader["RemainingNetWeight"]));
						row.Add(ConvertToString(reader["RemainingCustomsQuantity"]));

						report.Add($"Row {rowNumber}", row);
					}

					CombineAssertions(() =>
					{
						AssertEquals("Only 4 Entry Headers are printed because Entry Header 1 and 3 have zero remaining values; Entry Header 5 and 6 are not enabled", 4, rowNumber);

						var row1 = report["Row 1"];
						AssertContainsExactElementsInAnyOrder("Attached Declaration, Entry Header 2, single entry line and single invoice line",
							new string[] {
								"001", //GC_Code
								"Test Company CG", //GC_Name
								"001", //GB_Code
								"Test Branch CG", //GB_BranchName
								"001", //GE_Code
								"Test Department CG", //GE_Description
								"AIR", //JE_TransportMode
								"IMP", //JE_MessageType
								"B00001", //JE_DeclarationReference
								"A:12512345001", //CE_EntryNum
								"B00001/2", //CH_BGMReference
								"CLR", //CH_EntryStatus
								"10/09/2021 12:00:00 AM", //CH_EntryReleaseDate
								"IMPFZ", //ImporterCode
								"Test Importer", //ImporterName
								"SUPFZ", //SupplierCode
								"Test Supplier", //SupplierName
								"CUSFZ", //CustomerCode
								"Test Customer", //CustomerName
								"EX8", //CEI_Style
								"1/01/2022 12:00:00 AM", //CH_BondValidToDate
								"70.0000", //RemainingCustomsValue
								"15.000000", //RemainingNetWeight
								"35.000000" //RemainingCustomsQuantity
							},
							row1);

						var row2 = report["Row 2"];
						AssertContainsExactElementsInAnyOrder("Standalone Declaration, Entry Header 4, aggregated entry lines and invoice lines",
							new string[] {
								"001", //GC_Code
								"Test Company CG", //GC_Name
								"001", //GB_Code
								"Test Branch CG", //GB_BranchName
								"001", //GE_Code
								"Test Department CG", //GE_Description
								"AIR", //JE_TransportMode
								"IMP", //JE_MessageType
								"B00002", //JE_DeclarationReference
								"A:12512345002", //CE_EntryNum
								"B00002/2", //CH_BGMReference
								"CLR", //CH_EntryStatus
								"10/09/2021 12:00:00 AM", //CH_EntryReleaseDate
								"IMPFZ", //ImporterCode
								"Test Importer", //ImporterName
								"SUPFZ", //SupplierCode
								"Test Supplier", //SupplierName
								"CUSFZ", //CustomerCode
								"Test Customer", //CustomerName
								"EX8", //CEI_Style
								"1/01/2022 12:00:00 AM", //CH_BondValidToDate
								"70.0000", //RemainingCustomsValue
								"15.000000", //RemainingNetWeight
								"35.000000" //RemainingCustomsQuantity
							},
							row2);

						var row3 = report["Row 3"];
						AssertContainsExactElementsInAnyOrder("Attached Declaration, Entry Header 6, single entry line and single invoice line",
							new string[] {
								"002", //GC_Code
								"Test Company NA", //GC_Name
								"002", //GB_Code
								"Test Branch NA", //GB_BranchName
								"002", //GE_Code
								"Test Department NA", //GE_Description
								"AIR", //JE_TransportMode
								"IMP", //JE_MessageType
								"B00003", //JE_DeclarationReference
								"A:12512345003", //CE_EntryNum
								"B00003/2", //CH_BGMReference
								"CLR", //CH_EntryStatus
								"10/09/2021 12:00:00 AM", //CH_EntryReleaseDate
								"IMPFZ", //ImporterCode
								"Test Importer", //ImporterName
								"SUPFZ", //SupplierCode
								"Test Supplier", //SupplierName
								"CUSFZ", //CustomerCode
								"Test Customer", //CustomerName
								"EX8", //CEI_Style
								"1/01/2022 12:00:00 AM", //CH_BondValidToDate
								"70.0000", //RemainingCustomsValue
								"15.000000", //RemainingNetWeight
								"35.000000" //RemainingCustomsQuantity
							},
							row3);

						var row4 = report["Row 4"];
						AssertContainsExactElementsInAnyOrder("Standalone Declaration, Entry Header 8, aggregated entry lines and invoice lines",
							new string[] {
								"002", //GC_Code
								"Test Company NA", //GC_Name
								"002", //GB_Code
								"Test Branch NA", //GB_BranchName
								"002", //GE_Code
								"Test Department NA", //GE_Description
								"AIR", //JE_TransportMode
								"IMP", //JE_MessageType
								"B00004", //JE_DeclarationReference
								"A:12512345004", //CE_EntryNum
								"B00004/2", //CH_BGMReference
								"CLR", //CH_EntryStatus
								"10/09/2021 12:00:00 AM", //CH_EntryReleaseDate
								"IMPFZ", //ImporterCode
								"Test Importer", //ImporterName
								"SUPFZ", //SupplierCode
								"Test Supplier", //SupplierName
								"CUSFZ", //CustomerCode
								"Test Customer", //CustomerName
								"EX8", //CEI_Style
								"1/01/2022 12:00:00 AM", //CH_BondValidToDate
								"70.0000", //RemainingCustomsValue
								"15.000000", //RemainingNetWeight
								"35.000000" //RemainingCustomsQuantity
							},
							row4);
					});
				}
			}
		}

		string ConvertToString(object value)
		{
			var result = "";
			if (value is decimal decimalValue)
			{
				result = Convert.ToString(decimalValue);
			}
			else if (value is DateTime dateTimeValue)
			{
				result = Convert.ToString(dateTimeValue);
			}
			else if (value != DBNull.Value)
			{
				result = value.ToString();
			}
			return result;
		}

		void PrepareTestData()
		{
			importerPK = Guid.NewGuid();
			supplierPK = Guid.NewGuid();
			customerPK = Guid.NewGuid();
			SetupOrganisations(importerPK, supplierPK, customerPK);

			SetupFUNCS("CG");
			var companyCGPK = Guid.NewGuid();
			var branchCGPK = Guid.NewGuid();
			var departmentCGPK = Guid.NewGuid();
			SetupCompany(companyCGPK, branchCGPK, departmentCGPK, 1, "CG");
			var shipmentCGPK = Guid.NewGuid();
			SetupShipment(shipmentCGPK, "CG");
			var declarationCGPK = Guid.NewGuid();
			SetupDeclaration(companyCGPK, branchCGPK, departmentCGPK, 1, "CG", "JS", shipmentCGPK, shipmentCGPK, declarationCGPK);
			var standAloneDeclarationCGPK = Guid.NewGuid();
			SetupDeclaration(companyCGPK, branchCGPK, departmentCGPK, 2, "CG", "JE", standAloneDeclarationCGPK, DBNull.Value, standAloneDeclarationCGPK);

			SetupFUNCS("NA");
			var companyNAPK = Guid.NewGuid();
			var branchNAPK = Guid.NewGuid();
			var departmentNAPK = Guid.NewGuid();
			SetupCompany(companyNAPK, branchNAPK, departmentNAPK, 2, "NA");
			var shipmentNAPK = Guid.NewGuid();
			SetupShipment(shipmentNAPK, "NA");
			var declarationNAPK = Guid.NewGuid();
			SetupDeclaration(companyNAPK, branchNAPK, departmentNAPK, 3, "NA", "JS", shipmentNAPK, shipmentNAPK, declarationNAPK);
			var standAloneDeclarationNAPK = Guid.NewGuid();
			SetupDeclaration(companyNAPK, branchNAPK, departmentNAPK, 4, "NA", "JE", standAloneDeclarationNAPK, DBNull.Value, standAloneDeclarationNAPK);

			var companyCAPK = Guid.NewGuid();
			var branchCAPK = Guid.NewGuid();
			var departmentCAPK = Guid.NewGuid();
			SetupCompany(companyCAPK, branchCAPK, departmentCAPK, 3, "CA");
			var shipmentCAPK = Guid.NewGuid();
			SetupShipment(shipmentCAPK, "CA");
			var declarationCAPK = Guid.NewGuid();
			SetupDeclaration(companyCAPK, branchCAPK, departmentCAPK, 5, "CA", "JS", shipmentCAPK, shipmentCAPK, declarationCAPK);
			var standAloneDeclarationCAPK = Guid.NewGuid();
			SetupDeclaration(companyCAPK, branchCAPK, departmentCAPK, 6, "CA", "JE", standAloneDeclarationCAPK, DBNull.Value, standAloneDeclarationCAPK);
		}

		Guid importerPK;
		Guid supplierPK;
		Guid customerPK;

		void SetupOrganisations(Guid importerPK, Guid supplierPK, Guid customerPK)
		{
			var sqlScript = @"--Importer, Supplier and Customer
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@ImporterPK, 'IMPFZ', 'Test Importer')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@SupplierPK, 'SUPFZ', 'Test Supplier')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@CustomerPK, 'CUSFZ', 'Test Customer')
";
			using (var cmd = TestConnection.Command(sqlScript))
			{
				cmd.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, importerPK);
				cmd.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, supplierPK);
				cmd.AddParameter("@CustomerPK", SqlDbType.UniqueIdentifier, customerPK);
				cmd.ExecuteNonQuery();
			}
		}

		void SetupFUNCS(string countryCode)
		{
			var sqlScript = @"
--Enable Risk Function
INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (NEWID(), @CountryCode, @CountryCode + ' COUNTRY')
INSERT INTO RefDatabase_RefCusCodeType(ZZK_PK, ZZK_CodeType, ZZK_Description, ZZK_ZZZ_NKDataGrouping)
VALUES (NEWID(), 'FUNCS', 'Customs Effective Dates for New Functionality', @CountryCode)
INSERT INTO RefDatabase_RefCusCodeList (ZZD_PK, ZZD_Code, ZZD_Description, ZZD_ZZK_NKCodeType, ZZD_ZZZ_NKDataGrouping, ZZD_StartDate, ZZD_EndDate)
VALUES (NEWID(), 'RISK', 'RISK', 'FUNCS', @CountryCode, '1900-01-01', '2079-06-06')";
			using (var cmd = TestConnection.Command(sqlScript))
			{
				cmd.AddParameter("@CountryCode", SqlDbType.Char, countryCode);
				cmd.ExecuteNonQuery();
			}
		}

		void SetupShipment(Guid shipmentPK, string countryCode)
		{
			var sqlScript = @"INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES (@ShipmentPK, @CountryCode + 'ABC000123231')";
			using (var cmd = TestConnection.Command(sqlScript))
			{
				cmd.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				cmd.AddParameter("@CountryCode", SqlDbType.Char, countryCode);
				cmd.ExecuteNonQuery();
			}
		}

		void SetupCompany(Guid companyPK, Guid branchPK, Guid departmentPK, int identifier, string countryCode)
		{
			var sqlScript = @"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPK, @Identifier, 'Test Company ' + @CountryCode, @CountryCode, '___')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code, GB_BranchName) VALUES (@BranchPK, @CompanyPK, @Identifier, 'Test Branch ' + @CountryCode)
INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) VALUES (@DepartmentPK, @Identifier, 'Test Department ' + @CountryCode)
";
			using (var cmd = TestConnection.Command(sqlScript))
			{
				cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
				cmd.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				cmd.AddParameter("@CountryCode", SqlDbType.Char, countryCode);
				cmd.AddParameter("@Identifier", SqlDbType.Char, identifier.ToString().PadLeft(3, '0'));
				cmd.ExecuteNonQuery();
			}
		}

		void SetupDeclaration(Guid companyPK, Guid branchPK, Guid departmentPK, int clusterKey, string countryCode, string jh_ParentTableCode, Guid jh_ParentID, object shipmentPK, Guid declarationPK)
		{
			var sqlScript = @"
DECLARE @EntryInstructionPK1 UNIQUEIDENTIFIER = NEWID(), @EntryInstructionPK2 UNIQUEIDENTIFIER = NEWID()
DECLARE @EntryHeaderPK1 UNIQUEIDENTIFIER = NEWID(), @EntryHeaderPK2 UNIQUEIDENTIFIER = NEWID()
DECLARE @EntryLinePK1 UNIQUEIDENTIFIER = NEWID(), @EntryLinePK2 UNIQUEIDENTIFIER = NEWID()
DECLARE @InvoicePK UNIQUEIDENTIFIER = NEWID()
DECLARE @InvoiceLinePK1 UNIQUEIDENTIFIER = NEWID(), @InvoiceLinePK2 UNIQUEIDENTIFIER = NEWID()

--Declaration
INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_JS, JE_GC, JE_GB, JE_ClusterKey, JE_TransportMode, JE_MessageType, JE_DeclarationReference, JE_OH_Importer, JE_OH_Supplier, JE_OH_ControllingCustomer, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
VALUES (@DeclarationPK, @CountryCode, @ShipmentPK, @CompanyPK, @BranchPK, @ClusterKey, 'AIR', 'IMP', 'B00' + @Identifier, @ImporterPK, @SupplierPK, @CustomerPK, GETDATE(), '~BP', GETDATE(), '~BP')

--Job Header
INSERT INTO dbo.JobHeader (JH_PK, JH_JobNum, JH_GE, JH_GB, JH_GC, JH_ParentTableCode, JH_ParentID, JH_Status, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser) VALUES (NEWID(), @Identifier, @DepartmentPK, @BranchPK, @CompanyPK, @JH_ParentTableCode, @JH_ParentID, 'WRK', GETDATE(), '~BP', GETDATE(), '~BP')

--Entry Instructions
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@EntryInstructionPK1, @CountryCode, @DeclarationPK, @ClusterKey, 'EX8', GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_Style, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@EntryInstructionPK2, @CountryCode, @DeclarationPK, @ClusterKey, 'EX8', GETDATE(), '~BP', GETDATE(), '~BP')

--Entry Headers
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE ,CH_ClusterKey, CH_CEI_Instruction, CH_MessageType, CH_BGMReference, CH_EntryStatus, CH_EntryReleaseDate, CH_BondValidToDate, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@EntryHeaderPK1, @CountryCode, @DeclarationPK, @ClusterKey, @EntryInstructionPK1, 'IMP', 'B00' + @Identifier + '/1', 'CLR', '2021-09-10', '2022-01-01', GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE ,CH_ClusterKey, CH_CEI_Instruction, CH_MessageType, CH_BGMReference, CH_EntryStatus, CH_EntryReleaseDate, CH_BondValidToDate, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@EntryHeaderPK2, @CountryCode, @DeclarationPK, @ClusterKey, @EntryInstructionPK2, 'IMP', 'B00' + @Identifier + '/2', 'CLR', '2021-09-10', '2022-01-01', GETDATE(), '~BP', GETDATE(), '~BP')

--Entry Num
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryNum, CE_EntryType, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES (NEWID(), 'CusEntryHeader', @EntryHeaderPK1, 'A:12512345690', 'IMP', @CountryCode, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryNum, CE_EntryType, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES (NEWID(), 'CusEntryHeader', @EntryHeaderPK2, 'A:12512345' + @Identifier, 'IMP', @CountryCode, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryNum, CE_EntryType, CE_RN_NKCountryCode, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES (NEWID(), 'CusEntryHeader', @EntryHeaderPK2, 'A:32112345' + @Identifier, 'IMP', 'ZZ', GETDATE(), '~BP', GETDATE(), '~BP')

--Entry Lines
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_CustomsValue, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) VALUES (@EntryLinePK1, @CountryCode, @EntryHeaderPK1, @ClusterKey, 100, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_CustomsValue, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) VALUES (@EntryLinePK2, @CountryCode, @EntryHeaderPK2, @ClusterKey, 100, GETDATE(), '~BP', GETDATE(), '~BP')

--Invoice Headers
INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_ClusterKey, JZ_JE, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@InvoicePK, @CountryCode, @ClusterKey, @DeclarationPK, GETDATE(), '~BP', GETDATE(), '~BP')

--Invoice Lines
INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_ClusterKey, JI_JZ, JI_LinePrice, JI_NetWeight, JI_NetWeightUQ, JI_CustomsQuantity, JI_CL, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES (@InvoiceLinePK1, @CountryCode, @ClusterKey, @InvoicePK, 100, 10, 'KG', 20, @EntryLinePK1, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_DataModel, JI_ClusterKey, JI_JZ, JI_LinePrice, JI_NetWeight, JI_NetWeightUQ, JI_CustomsQuantity, JI_CL, JI_Procedure, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
VALUES (@InvoiceLinePK2, @CountryCode, @ClusterKey, @InvoicePK, 100, 20, 'KG', 40, @EntryLinePK2, '1000', GETDATE(), '~BP', GETDATE(), '~BP')

--Cus Supporting Info
INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_Type, CSI_ParentTableCode, CSI_ParentID, CSI_Value, CSI_Quantity, CSI_Quantity2, CSI_DataModel, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser) VALUES (NEWID(), 'RMG', 'CEI', @EntryInstructionPK1, 40, 4, 8, @CountryCode, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_Type, CSI_ParentTableCode, CSI_ParentID, CSI_Value, CSI_Quantity, CSI_Quantity2, CSI_DataModel, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser) VALUES (NEWID(), 'RMG', 'CEI', @EntryInstructionPK1, 60, 6, 12, @CountryCode, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_Type, CSI_ParentTableCode, CSI_ParentID, CSI_Value, CSI_Quantity, CSI_Quantity2, CSI_DataModel, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser) VALUES (NEWID(), 'RMG', 'CEI', @EntryInstructionPK2, 10, 2, 2, @CountryCode, GETDATE(), '~BP', GETDATE(), '~BP')
INSERT INTO dbo.CusSupportingInfo (CSI_PK, CSI_Type, CSI_ParentTableCode, CSI_ParentID, CSI_Value, CSI_Quantity, CSI_Quantity2, CSI_DataModel, CSI_SystemCreateTimeUtc, CSI_SystemCreateUser, CSI_SystemLastEditTimeUtc, CSI_SystemLastEditUser) VALUES (NEWID(), 'RMG', 'CEI', @EntryInstructionPK2, 20, 3, 3, @CountryCode, GETDATE(), '~BP', GETDATE(), '~BP')";
			using (var cmd = TestConnection.Command(sqlScript))
			{
				cmd.AddParameter("@ImporterPK", SqlDbType.UniqueIdentifier, importerPK);
				cmd.AddParameter("@SupplierPK", SqlDbType.UniqueIdentifier, supplierPK);
				cmd.AddParameter("@CustomerPK", SqlDbType.UniqueIdentifier, customerPK);
				cmd.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
				cmd.AddParameter("@DepartmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				cmd.AddParameter("@CountryCode", SqlDbType.Char, countryCode);
				cmd.AddParameter("@Identifier", SqlDbType.Char, clusterKey.ToString().PadLeft(3, '0'));
				cmd.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				cmd.AddParameter("@JH_ParentTableCode", SqlDbType.VarChar, jh_ParentTableCode);
				cmd.AddParameter("@JH_ParentID", SqlDbType.UniqueIdentifier, jh_ParentID);
				cmd.AddParameter("@ShipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				cmd.AddParameter("@DeclarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
