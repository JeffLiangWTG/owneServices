using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{//
	[TestedType(typeof(KREntryHeaderDetailsView))]
	class KREntryHeaderDetailsViewTest : DbCreateScriptTest
	{
		public void TestColumnsFromJobDeclaration()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (Guid declarationPK, Guid companyPK, Guid branchPK, string packType, string bondedAreaCode)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], (reader.GetGuid(3), reader.GetGuid(4), reader.GetGuid(28), reader.GetString(5), reader.GetString(6)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_JE_PK, KEH_CompanyPK, KEH_BranchPK, KEH_PackType, KEH_BondedAreaCode", (declarationPK1, companyPK, branchPK, "BG", "15306004"), result[entryPK1]);
						AssertEquals("KEH_JE_PK, KEH_CompanyPK, KEH_BranchPK, KEH_PackType, KEH_BondedAreaCode", (declarationPK1, companyPK, branchPK, "BG", "15306004"), result[entryPK2]);
						AssertEquals("KEH_JE_PK, KEH_CompanyPK, KEH_BranchPK, KEH_PackType, KEH_BondedAreaCode", (declarationPK2, companyPK, branchPK, "CT", "15470001"), result[entryPK3]);
						AssertEquals("KEH_JE_PK, KEH_CompanyPK, KEH_BranchPK, KEH_PackType, KEH_BondedAreaCode", (declarationPK2, companyPK, branchPK, "CT", "15470001"), result[entryPK4]);
						AssertEquals("KEH_JE_PK, KEH_CompanyPK, KEH_BranchPK, KEH_PackType, KEH_BondedAreaCode", (declarationPK3, companyPK, branchPK, "CL", "15420005"), result[entryPK5]);
						AssertEquals("KEH_JE_PK, KEH_CompanyPK, KEH_BranchPK, KEH_PackType, KEH_BondedAreaCode", (declarationPK3, companyPK, branchPK, "CL", "15420005"), result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromSupplierAddress()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], reader.GetString(8));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_SupplierName", "Test Supplier 1", result[entryPK1]);
						AssertEquals("KEH_SupplierName", "Test Supplier 1", result[entryPK2]);
						AssertEquals("KEH_SupplierName", string.Empty, result[entryPK3]);
						AssertEquals("KEH_SupplierName", string.Empty, result[entryPK4]);
						AssertEquals("KEH_SupplierName", string.Empty, result[entryPK5]);
						AssertEquals("KEH_SupplierName", string.Empty, result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromPayer()
		{
			var payerPK1 = Guid.NewGuid();

			var sql = @"
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES (@payerPK1, 'PAY1', 'Duty Payer 1')
UPDATE dbo.JobDeclaration
SET
	JE_OH_DutyPayer = @payerPK1,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK1";

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@payerPK1", SqlDbType.UniqueIdentifier, payerPK1);
				cmd.AddParameter("@declarationPK1", SqlDbType.UniqueIdentifier, declarationPK1);
				cmd.ExecuteNonQuery();
			}

			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], reader.GetString(10));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_PayerName", "Duty Payer 1", result[entryPK1]);
						AssertEquals("KEH_PayerName", "Duty Payer 1", result[entryPK2]);
						AssertEquals("KEH_PayerName", string.Empty, result[entryPK3]);
						AssertEquals("KEH_PayerName", string.Empty, result[entryPK4]);
						AssertEquals("KEH_PayerName", string.Empty, result[entryPK5]);
						AssertEquals("KEH_PayerName", string.Empty, result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromCusEntryLine()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (string hsCode, decimal customsValue)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], (reader.GetString(11), reader.GetDecimal(15)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_HSCode, KEH_TotalCustomsValueInKRW", ("0101291000", 1330933m), result[entryPK1]);
						AssertEquals("KEH_HSCode, KEH_TotalCustomsValueInKRW", ("2805120000", 5844470m), result[entryPK2]);
						AssertEquals("KEH_HSCode, KEH_TotalCustomsValueInKRW", ("5206231000", 16269285m), result[entryPK3]);
						AssertEquals("KEH_HSCode, KEH_TotalCustomsValueInKRW", (string.Empty, 4122326m), result[entryPK4]);
						AssertEquals("KEH_HSCode, KEH_TotalCustomsValueInKRW", ("4206332000", 16377285m), result[entryPK5]);
						AssertEquals("KEH_HSCode, KEH_TotalCustomsValueInKRW", (string.Empty, 4172730m), result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromCusEntryNum()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (string entryNum, DateTime issueDate, string expiryDateToString)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], (reader.GetString(12), reader.GetDateTime(14), reader["KEH_EstimatedDateOfFinalPrice"].ToString()));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_EntryNum, KEH_EntryNumIssueDate, KEH_EstimatedDateOfFinalPrice", ("6N00220000052X", new DateTime(2022, 1, 2), string.Empty), result[entryPK1]);
						AssertEquals("KEH_EntryNum, KEH_EntryNumIssueDate, KEH_EstimatedDateOfFinalPrice", ("6N00220000062X", new DateTime(2022, 2, 2), string.Empty), result[entryPK2]);
						AssertEquals("KEH_EntryNum, KEH_EntryNumIssueDate, KEH_EstimatedDateOfFinalPrice", ("6N00220000072X", new DateTime(2022, 3, 2), string.Empty), result[entryPK3]);
						AssertEquals("KEH_EntryNum, KEH_EntryNumIssueDate, KEH_EstimatedDateOfFinalPrice", ("6N00220000082X", new DateTime(2022, 3, 16), string.Empty), result[entryPK4]);
						AssertEquals("KEH_EntryNum, KEH_EntryNumIssueDate, KEH_EstimatedDateOfFinalPrice", ("6N00224000052U", new DateTime(2024, 3, 2), new DateTime(2024, 4, 2).ToString()), result[entryPK5]);
						AssertEquals("KEH_EntryNum, KEH_EntryNumIssueDate, KEH_EstimatedDateOfFinalPrice", ("6N00224000062X", new DateTime(2024, 3, 16), new DateTime(2024, 5, 2).ToString()), result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromJobComInvoiceLine()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (decimal, string, decimal)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], (reader.GetDecimal(16), reader.GetString(26), reader.GetDecimal(27)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_TotalWeightInKG, KEH_HSDescription, KEH_CustomsQuantity", (112.7095301800000m, "Other chromates and dichromates; peroxochromates", 12040.000000m), result[entryPK1]);
						AssertEquals("KEH_TotalWeightInKG, KEH_HSDescription, KEH_CustomsQuantity", (472.6314781822000m, "Pneumatic mattresses, of cotton", 24080.000000m), result[entryPK2]);
						AssertEquals("KEH_TotalWeightInKG, KEH_HSDescription, KEH_CustomsQuantity", (131753.0000000000000m, "", 12040.000000m), result[entryPK3]);
						AssertEquals("KEH_TotalWeightInKG, KEH_HSDescription, KEH_CustomsQuantity", (40.0000000000000m, "", 0.000000m), result[entryPK4]);
						AssertEquals("KEH_TotalWeightInKG, KEH_HSDescription, KEH_CustomsQuantity", (138353.0000000000000m, "", 23000.000000m), result[entryPK5]);
						AssertEquals("KEH_TotalWeightInKG, KEH_HSDescription, KEH_CustomsQuantity", (42.0000000000000m, "", 0.000000m), result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromJobComInvoiceHeader()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (decimal packQty, string contractExpirationDateToString, decimal provisionalAdditionalRate, decimal provisionalAdditionalAmount, string cargoManagementNumber)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], (reader.GetDecimal(17), reader["KEH_ContractExpirationDate"].ToString(), reader.GetDecimal(20), reader.GetDecimal(21), reader.GetString(29)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_ExportPackQty, KEH_ContractExpirationDate, KEH_ProvAdditionalRate, KEH_TotalProvAdditionalAmount, KEH_CargoManagementNumber", (33m, string.Empty, 0m, 0m, ""), result[entryPK1]);
						AssertEquals("KEH_ExportPackQty, KEH_ContractExpirationDate, KEH_ProvAdditionalRate, KEH_TotalProvAdditionalAmount, KEH_CargoManagementNumber", (132m, string.Empty, 0m, 0m, ""), result[entryPK2]);
						AssertEquals("KEH_ExportPackQty, KEH_ContractExpirationDate, KEH_ProvAdditionalRate, KEH_TotalProvAdditionalAmount, KEH_CargoManagementNumber", (143m, string.Empty, 0m, 0m, ""), result[entryPK3]);
						AssertEquals("KEH_ExportPackQty, KEH_ContractExpirationDate, KEH_ProvAdditionalRate, KEH_TotalProvAdditionalAmount, KEH_CargoManagementNumber", (287m, string.Empty, 0m, 0m, ""), result[entryPK4]);
						AssertEquals("KEH_ExportPackQty, KEH_ContractExpirationDate, KEH_ProvAdditionalRate, KEH_TotalProvAdditionalAmount, KEH_CargoManagementNumber", (2323m, new DateTime(2024, 3, 3).ToString(), 4.72m, 3m, "01KE0766SS200100003"), result[entryPK5]);
						AssertEquals("KEH_ExportPackQty, KEH_ContractExpirationDate, KEH_ProvAdditionalRate, KEH_TotalProvAdditionalAmount, KEH_CargoManagementNumber", (4242m, new DateTime(2024, 3, 3).ToString(), 4.72m, 12m, "NO"), result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromCusEntryHeader()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, (DateTime entryReleaseDate, decimal totalPaid, string billNum, decimal importPackQty, DateTime entryCreatedLocalTime)>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], (reader.GetDateTime(22), reader.GetDecimal(23), reader.GetString(30), reader.GetInt32(31), reader.GetDateTime(32)));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_EntryReleaseDate, KEH_TotalPaid, KEH_BillNum, KEH_ImportPackQty, KEH_EntryCreatedLocalTime", (new DateTime(2019, 01, 01), 1m, "", 0m, new DateTime(2019, 01, 01, 10, 0, 0)), result[entryPK1]);
						AssertEquals("KEH_EntryReleaseDate, KEH_TotalPaid, KEH_BillNum, KEH_ImportPackQty, KEH_EntryCreatedLocalTime", (new DateTime(2020, 01, 01), 2m, "", 0m, new DateTime(2020, 01, 01, 11, 0, 0)), result[entryPK2]);
						AssertEquals("KEH_EntryReleaseDate, KEH_TotalPaid, KEH_BillNum, KEH_ImportPackQty, KEH_EntryCreatedLocalTime", (new DateTime(2021, 01, 01), 3m, "", 0m, new DateTime(2021, 01, 01, 12, 0, 0)), result[entryPK3]);
						AssertEquals("KEH_EntryReleaseDate, KEH_TotalPaid, KEH_BillNum, KEH_ImportPackQty, KEH_EntryCreatedLocalTime", (new DateTime(2022, 01, 01), 4m, "", 0m, new DateTime(2022, 01, 01, 13, 0, 0)), result[entryPK4]);
						AssertEquals("KEH_EntryReleaseDate, KEH_TotalPaid, KEH_BillNum, KEH_ImportPackQty, KEH_EntryCreatedLocalTime", (new DateTime(2023, 01, 01), 5m, "BILL1", 10m, new DateTime(2023, 01, 01, 14, 0, 0)), result[entryPK5]);
						AssertEquals("KEH_EntryReleaseDate, KEH_TotalPaid, KEH_BillNum, KEH_ImportPackQty, KEH_EntryCreatedLocalTime", (new DateTime(2024, 01, 01), 6m, "BILL1", 20m, new DateTime(2024, 01, 01, 15, 0, 0)), result[entryPK6]);
					});
				}
			}
		}

		public void TestColumnsFromImporter()
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.KREntryHeaderDetailsView"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, string>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEH_PK"], reader.GetString(25));
					}
					CombineAssertions(() =>
					{
						AssertEquals("Record Count", 6, result.Count);
						AssertEquals("KEH_ImporterName", string.Empty, result[entryPK1]);
						AssertEquals("KEH_ImporterName", string.Empty, result[entryPK2]);
						AssertEquals("KEH_ImporterName", string.Empty, result[entryPK3]);
						AssertEquals("KEH_ImporterName", string.Empty, result[entryPK4]);
						AssertEquals("KEH_ImporterName", "Test Importer 1", result[entryPK5]);
						AssertEquals("KEH_ImporterName", "Test Importer 1", result[entryPK6]);
					});
				}
			}
		}

		public void TestSelectOnlyKRCompany()
		{
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM KREntryHeaderDetailsView")))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KR company data rows", 6, reader["RowCount"]);
				}
			}

			var clusterKey = 10;
			var otherCompanyPK = TestDataCreator.CreateCompany("AC1", "AU", "AUD");
			var otherBranchPK = TestDataCreator.CreateBranch(otherCompanyPK, "AB1", "AUZZZ");
			var declarationPK = CreateJobDeclaration("EXP", otherBranchPK, otherCompanyPK, clusterKey, "BG", "15306004");
			var entryPK = CreateCusEntryHeader(declarationPK, clusterKey, new DateTime(2025, 1, 1), new DateTime(2025, 1, 1), 1);
			var entryLinePK = CreateCusEntryLine(entryPK, clusterKey, "0101291000", 1);
			CreateCusEntryNum(entryPK, "6N00225000001X", "EXP");
			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK, clusterKey, 10m, 100m, "KG");
			(decimal, string)[] customsQuantities = { (0m, ""), (0m, ""), (0m, ""), (0m, "") };
			CreateJobComInvoiceLine(invoicePK1, clusterKey, entryLinePK, "0101291000", customsQuantities);
			using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM KREntryHeaderDetailsView")))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals("KR company data rows", 6, reader["RowCount"]);
				}
			}
		}

		Guid CreateJobDeclaration(string messageType, Guid branchPK, Guid companyPK, int clusterKey, string packType, string bondedAreaCode, Guid? supplierPK = null, Guid? supplierAddressPK = null, Guid? importerPK = null, string declarationReference = "")
		{
			var declarationPK = Guid.NewGuid();
			var declarationRef = string.IsNullOrEmpty(declarationReference) ? Guid.NewGuid().ToString("n") : declarationReference;
			var sql = @"
				INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_MessageType, JE_ClusterKey, JE_TotalNoOfPacksPackType, JE_LocationOtherInformation, JE_OH_Supplier, JE_OA_SupplierAddress, JE_OH_Importer, JE_SystemCreateUser, JE_SystemLastEditUser, JE_DeclarationReference)
					VALUES (@declarationPK, 'KR', @branchPK, @companyPK, @messageType, @clusterKey, @packType, @bondedAreaCode, @supplierPK, @supplierAddressPK, @importerPK, '~BP', '~BP', @declarationReference)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, JobDeclarationSchema.JE_MessageType.MaxLength, messageType);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@packType", SqlDbType.VarChar, packType);
				command.AddParameter("@bondedAreaCode", SqlDbType.VarChar, bondedAreaCode);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK == null ? DBNull.Value : supplierPK.GetValueOrDefault());
				command.AddParameter("@supplierAddressPK", SqlDbType.UniqueIdentifier, supplierAddressPK == null ? DBNull.Value : supplierAddressPK.GetValueOrDefault());
				command.AddParameter("@importerPK", SqlDbType.UniqueIdentifier, importerPK == null ? DBNull.Value : importerPK.GetValueOrDefault());
				command.AddParameter("@declarationReference", SqlDbType.VarChar, declarationRef);
				command.ExecuteNonQuery();
			}
			return declarationPK;
		}

		Guid CreateCusEntryHeader(Guid declarationPK, int clusterKey, DateTime entryReleaseDate, DateTime createTime, decimal totalPaid, Guid? instructionpk1 = null)
		{
			var entryHeaderPK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_ClusterKey, CH_EntryReleaseDate, CH_TotalPaid, CH_CEI_Instruction, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
					VALUES (@entryHeaderPK, 'KR', @declarationPK, @clusterKey, @entryReleaseDate, @totalPaid, @instructionpk1, @createTime, '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@entryReleaseDate", SqlDbType.DateTime, entryReleaseDate);
				command.AddParameter("@createTime", SqlDbType.DateTime, createTime);
				command.AddParameter("@totalPaid", SqlDbType.Int, totalPaid);
				command.AddParameter("@instructionpk1", SqlDbType.UniqueIdentifier, instructionpk1 == null ? DBNull.Value : instructionpk1);
				command.ExecuteNonQuery();
			}
			return entryHeaderPK;
		}

		Guid CreateCusEntryLine(Guid entryHeaderPK, int clusterKey, string hsCode, decimal customsValue)
		{
			var entryLinePK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_ClusterKey, CL_AdValoremTariff, CL_CustomsValue, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
					VALUES (@entryLinePK, 'KR', @entryHeaderPK, @clusterKey, @hsCode, @customsValue, getutcdate(), '~BP', getutcdate(), '~BP')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@entryHeaderPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@hsCode", SqlDbType.VarChar, hsCode);
				command.AddParameter("@customsValue", SqlDbType.Decimal, customsValue);
				command.ExecuteNonQuery();
			}
			return entryLinePK;
		}

		Guid CreateCusEntryNum(Guid entryHeaderPK, string entryNum, string entryType, DateTime? issueDate = null, DateTime? expiryDate = null)
		{
			var entryNumPK = TestDataCreator.CreateCusEntryNum(entryHeaderPK, "CusEntryHeader", entryNum, entryType, "CUS", "KR", issueDate);
			if (expiryDate != null)
			{
				var sql = $@"UPDATE [dbo].[CusEntryNum] SET CE_ExpiryDate = @expiryDate, CE_SystemLastEditTimeUtc = @systemLastEditTimeUtc, CE_SystemLastEditUser = '~BP' WHERE CE_PK = @entryNumPK";
				using (DbCommand command = Db.Connection.Command(sql))
				{
					command.AddParameter("@expiryDate", SqlDbType.DateTime, expiryDate.Value);
					command.AddParameter("@entryNumPK", SqlDbType.UniqueIdentifier, entryNumPK);
					command.AddParameter("@systemLastEditTimeUtc", SqlDbType.SmallDateTime, DateTime.UtcNow);
					command.ExecuteNonQuery();
				}
			}

			return entryNumPK;
		}

		Guid CreateJobComInvoiceHeader(Guid declarationPK, int clusterKey, decimal packQty, decimal weight, string weightUQ, string addInfo = "", Guid? billPK = null)
		{
			var invoiceHeaderPK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_DataModel, JZ_JE, JZ_ClusterKey, JZ_NoOfPacks, JZ_Weight, JZ_WeightUQ, JZ_AddInfo, JZ_CU_RelatedHouseBill)
					VALUES (@invoiceHeaderPK, 'KR', @declarationPK, @clusterKey, @packQty, @weight, @weightUQ, @addInfo, @billPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, invoiceHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@packQty", SqlDbType.Decimal, packQty);
				command.AddParameter("@weight", SqlDbType.Decimal, weight);
				command.AddParameter("@weightUQ", SqlDbType.VarChar, weightUQ);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@billPK", SqlDbType.UniqueIdentifier, billPK == null ? DBNull.Value : billPK);
				command.ExecuteNonQuery();
			}
			return invoiceHeaderPK;
		}

		Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, int clusterKey, Guid entryLinePK, string tariff, (decimal, string)[] customsQuantities)
		{
			var invoiceLinePK = Guid.NewGuid();
			var sql = @"
				INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_ClusterKey, JI_CL, JI_Tariff, JI_CustomsQuantity, JI_CustomsUnitQty, JI_CustomsSecondQuantity, JI_CustomsSecondUnitQty, JI_CustomsThirdQuantity, JI_CustomsThirdUnitQty, JI_CustomsFourthQuantity, JI_CustomsFourthUnitQty)
					VALUES (@invoiceLinePK, 'KR', @jobComInvoiceHeaderPK, @clusterKey, @entryLinePK, @tariff, @qty1, @uq1, @qty2, @uq2, @qty3, @uq3, @qty4, @uq4)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@entryLinePK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@tariff", SqlDbType.VarChar, tariff);
				command.AddParameter("@qty1", SqlDbType.Decimal, customsQuantities[0].Item1);
				command.AddParameter("@uq1", SqlDbType.VarChar, customsQuantities[0].Item2);
				command.AddParameter("@qty2", SqlDbType.Decimal, customsQuantities[1].Item1);
				command.AddParameter("@uq2", SqlDbType.VarChar, customsQuantities[1].Item2);
				command.AddParameter("@qty3", SqlDbType.Decimal, customsQuantities[2].Item1);
				command.AddParameter("@uq3", SqlDbType.VarChar, customsQuantities[2].Item2);
				command.AddParameter("@qty4", SqlDbType.Decimal, customsQuantities[3].Item1);
				command.AddParameter("@uq4", SqlDbType.VarChar, customsQuantities[3].Item2);
				command.ExecuteNonQuery();
			}
			return invoiceLinePK;
		}

		Guid companyPK;
		Guid branchPK;
		Guid declarationPK1;
		Guid declarationPK2;
		Guid declarationPK3;
		Guid entryPK1;
		Guid entryPK2;
		Guid entryPK3;
		Guid entryPK4;
		Guid entryPK5;
		Guid entryPK6;

		protected override void SetUp()
		{
			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
			branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRINC");

			int clusterKey1 = 1;
			var supplier1 = TestDataCreator.CreateOrganisation("SUP1", "Test Supplier 1");
			var supplierAddress1 = TestDataCreator.CreateAddress(supplier1, "CST", "TEST ADDRESS 111");
			declarationPK1 = CreateJobDeclaration("EXP", branchPK, companyPK, clusterKey1, "BG", "15306004", supplier1, supplierAddress1);
			entryPK1 = CreateCusEntryHeader(declarationPK1, clusterKey1, new DateTime(2019, 1, 1), new DateTime(2019, 1, 1, 1, 0, 0), 1);
			CreateCusEntryNum(entryPK1, "6N00220000051X", "EXP", new DateTime(2022, 1, 1));
			CreateCusEntryNum(entryPK1, "6N00220000052X", "EXP", new DateTime(2022, 1, 2));
			var entryLinePK1 = CreateCusEntryLine(entryPK1, clusterKey1, "0101291000", 115612);
			var entryLinePK2 = CreateCusEntryLine(entryPK1, clusterKey1, "2710209753", 1215321);
			entryPK2 = CreateCusEntryHeader(declarationPK1, clusterKey1, new DateTime(2020, 1, 1), new DateTime(2020, 1, 1, 2, 0, 0), 2);
			CreateCusEntryNum(entryPK2, "6N00220000061X", "EXP", new DateTime(2022, 2, 1));
			CreateCusEntryNum(entryPK2, "6N00220000062X", "EXP", new DateTime(2022, 2, 2));
			var entryLinePK3 = CreateCusEntryLine(entryPK2, clusterKey1, "2805120000", 5832128);
			var entryLinePK4 = CreateCusEntryLine(entryPK2, clusterKey1, "3202103000", 12342);

			(decimal, string)[] customsQuantities0 = { (0m, ""), (0m, ""), (0m, ""), (0m, "") };
			(decimal, string)[] customsQuantities1 = { (10000m, "U"), (2000m, "U"), (300m, "KG"), (40m, "U") };
			(decimal, string)[] customsQuantities2 = { (20000m, "U"), (3000m, "U"), (0m, ""), (0m, "") };

			var tariffTypePK = CreateRefCusTariffType();
			var tariffItems1 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "0101291000", "Other chromates and dichromates; peroxochromates", new DateTime(2020, 01, 01), new DateTime(2024, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems1);

			var tariffItems2 = GetItemList(new List<string> { "ZZ1_TariffCode", "ZZ1_Description", "ZZ1_StartDate", "ZZ1_EndDate" }, new List<object> { "2805120000", "Pneumatic mattresses, of cotton", new DateTime(2020, 01, 01), new DateTime(2024, 12, 31) });
			CreateRefCusTariff(tariffTypePK, tariffItems2);

			var invoicePK1 = CreateJobComInvoiceHeader(declarationPK1, clusterKey1, 11, 61m, "KG");
			CreateJobComInvoiceLine(invoicePK1, clusterKey1, entryLinePK1, "0101291000" , customsQuantities1);
			CreateJobComInvoiceLine(invoicePK1, clusterKey1, entryLinePK2, "0101291000", customsQuantities1);
			var invoicePK2 = CreateJobComInvoiceHeader(declarationPK1, clusterKey1, 22, 114m, "LB");
			CreateJobComInvoiceLine(invoicePK2, clusterKey1, entryLinePK1, "0101291000", customsQuantities0);
			CreateJobComInvoiceLine(invoicePK2, clusterKey1, entryLinePK2, "0101291000" , customsQuantities0);

			var invoicePK3 = CreateJobComInvoiceHeader(declarationPK1, clusterKey1, 33, 400m, "KG");
			CreateJobComInvoiceLine(invoicePK3, clusterKey1, entryLinePK3, "2805120000", customsQuantities1);
			CreateJobComInvoiceLine(invoicePK3, clusterKey1, entryLinePK3, "2805120000", customsQuantities1);
			var invoicePK4 = CreateJobComInvoiceHeader(declarationPK1, clusterKey1, 44, 2562m, "OZ");
			CreateJobComInvoiceLine(invoicePK4, clusterKey1, entryLinePK4, "2805120000" , customsQuantities0);
			CreateJobComInvoiceLine(invoicePK4, clusterKey1, entryLinePK4, "2805120000", customsQuantities0);
			var invoicePK5 = CreateJobComInvoiceHeader(declarationPK1, clusterKey1, 55, 43m, string.Empty);
			CreateJobComInvoiceLine(invoicePK5, clusterKey1, entryLinePK4, "2805120000" , customsQuantities0);
			CreateJobComInvoiceLine(invoicePK5, clusterKey1, entryLinePK4, "2805120000", customsQuantities0);

			int clusterKey2 = 2;
			var supplier2 = TestDataCreator.CreateOrganisation("SUP2", "Test Supplier 2");
			declarationPK2 = CreateJobDeclaration("EXP", branchPK, companyPK, clusterKey2, "CT", "15470001", supplier2);
			entryPK3 = CreateCusEntryHeader(declarationPK2, clusterKey2, new DateTime(2021, 1, 1), new DateTime(2021, 1, 1, 3, 0, 0), 3);
			CreateCusEntryNum(entryPK3, "6N00220000071X", "EXP", new DateTime(2022, 3, 1));
			CreateCusEntryNum(entryPK3, "6N00220000072X", "EXP", new DateTime(2022, 3, 2));
			var entryLinePK5 = CreateCusEntryLine(entryPK3, clusterKey2, "5206231000", 15812353);
			var entryLinePK6 = CreateCusEntryLine(entryPK3, clusterKey2, string.Empty, 456932);
			entryPK4 = CreateCusEntryHeader(declarationPK2, clusterKey2, new DateTime(2022, 1, 1), new DateTime(2022, 1, 1, 4, 0, 0), 4);
			CreateCusEntryNum(entryPK4, "6N00220000081X", "EXP", new DateTime(2022, 3, 15));
			CreateCusEntryNum(entryPK4, "6N00220000082X", "EXP", new DateTime(2022, 3, 16));
			var entryLinePK7 = CreateCusEntryLine(entryPK4, clusterKey2, string.Empty, 3929392);
			var entryLinePK8 = CreateCusEntryLine(entryPK4, clusterKey2, "8460241000", 192934);
			var invoicePK6 = CreateJobComInvoiceHeader(declarationPK2, clusterKey2, 66, 130m, "T");
			CreateJobComInvoiceLine(invoicePK6, clusterKey2, entryLinePK5, "0101291000" , customsQuantities1);
			CreateJobComInvoiceLine(invoicePK6, clusterKey2, entryLinePK6, "0101291000", customsQuantities2);
			var invoicePK7 = CreateJobComInvoiceHeader(declarationPK2, clusterKey2, 77, 1753m, "KG");
			CreateJobComInvoiceLine(invoicePK7, clusterKey2, entryLinePK6, "0101291000" , customsQuantities0);
			CreateJobComInvoiceLine(invoicePK7, clusterKey2, entryLinePK5, "0101291000", customsQuantities0);
			var invoicePK8 = CreateJobComInvoiceHeader(declarationPK2, clusterKey2, 88, 0m, "OZ");
			CreateJobComInvoiceLine(invoicePK8, clusterKey2, entryLinePK7, "2805120000" , customsQuantities0);
			CreateJobComInvoiceLine(invoicePK8, clusterKey2, entryLinePK7, "2805120000" , customsQuantities0);
			var invoicePK9 = CreateJobComInvoiceHeader(declarationPK2, clusterKey2, 99, 123m, "XX");
			CreateJobComInvoiceLine(invoicePK9, clusterKey2, entryLinePK8, "2805120000" , customsQuantities1);
			CreateJobComInvoiceLine(invoicePK9, clusterKey2, entryLinePK8, "2805120000" , customsQuantities2);
			var invoicePK10 = CreateJobComInvoiceHeader(declarationPK2, clusterKey2, 100, 40m, "KG");
			CreateJobComInvoiceLine(invoicePK10, clusterKey2, entryLinePK8, "2805120000" , customsQuantities0);
			CreateJobComInvoiceLine(invoicePK10, clusterKey2, entryLinePK8, "2805120000" , customsQuantities0);

			int clusterKey3 = 3;
			var importer = TestDataCreator.CreateOrganisation("IMP1", "Test Importer 1");
			declarationPK3 = CreateJobDeclaration("IMP", branchPK, companyPK, clusterKey3, "CL", "15420005", importerPK: importer);
			var instructionpk1 = TestDataCreator.CreateCusEntryInstruction(declarationPK3, "", "", DateTime.Now, clusterKey3, "PackQty=10");
			entryPK5 = CreateCusEntryHeader(declarationPK3, clusterKey3, new DateTime(2023, 1, 1), new DateTime(2023, 1, 1, 5, 0, 0), 5, instructionpk1);
			CreateCusEntryNum(entryPK5, "6N00224000051U", "IMP", new DateTime(2024, 3, 2));
			CreateCusEntryNum(entryPK5, "6N00224000052U", "IMP", new DateTime(2024, 3, 1));
			CreateCusEntryNum(entryPK5, "", "934", null, new DateTime(2024, 4, 1));
			CreateCusEntryNum(entryPK5, "", "934", null, new DateTime(2024, 4, 2));
			var entryLinePK9 = CreateCusEntryLine(entryPK5, clusterKey3, "4206332000", 15912353);
			var entryLinePK10 = CreateCusEntryLine(entryPK5, clusterKey3, string.Empty, 464932);
			var instructionpk2 = TestDataCreator.CreateCusEntryInstruction(declarationPK3, "", "", DateTime.Now, clusterKey3, "PackQty=20");
			entryPK6 = CreateCusEntryHeader(declarationPK3, clusterKey3, new DateTime(2024, 1, 1), new DateTime(2024, 1, 1, 6, 0, 0), 6, instructionpk2);
			CreateCusEntryNum(entryPK6, "6N00224000061X", "IMP", new DateTime(2024, 3, 15));
			CreateCusEntryNum(entryPK6, "6N00224000062X", "IMP", new DateTime(2024, 3, 16));
			CreateCusEntryNum(entryPK6, "", "934", null, new DateTime(2024, 5, 1));
			CreateCusEntryNum(entryPK6, "", "934", null, new DateTime(2024, 5, 2));
			var entryLinePK11 = CreateCusEntryLine(entryPK6, clusterKey3, string.Empty, 3929796);
			var entryLinePK12 = CreateCusEntryLine(entryPK6, clusterKey3, "8460241000", 242934);
			var billpk1 = TestDataCreator.CreateCusDecHouseBill(true, "", declarationPK3, clusterKey3, billNum: "BILL1", billType: "HB");
			var invoicePK11 = CreateJobComInvoiceHeader(declarationPK3, clusterKey3, 1111, 137m, "T", "ImpContractExpiryDate=2024-03-03 11:20:23*ProvAdditionalRate=4.72*ProvAdditionalAmount=1*ImportCargoManagementNumber=01KE0766SS200100003", billpk1);
			CreateJobComInvoiceLine(invoicePK11, clusterKey3, entryLinePK9, "" , customsQuantities2);
			CreateJobComInvoiceLine(invoicePK11, clusterKey3, entryLinePK10, "" , customsQuantities0);
			var invoicePK12 = CreateJobComInvoiceHeader(declarationPK3, clusterKey3, 1212, 1353m, "KG", "ImpContractExpiryDate=2024-03-03 11:20:23*ProvAdditionalRate=4.72*ProvAdditionalAmount=2*ImportCargoManagementNumber=01KE0766SS200100003", billpk1);
			CreateJobComInvoiceLine(invoicePK12, clusterKey3, entryLinePK10, "" , customsQuantities2);
			CreateJobComInvoiceLine(invoicePK12, clusterKey3, entryLinePK9, "" , customsQuantities0);
			var invoicePK13 = CreateJobComInvoiceHeader(declarationPK3, clusterKey3, 1313, 0m, "OZ", "ImpContractExpiryDate=2024-03-03 11:20:23*ProvAdditionalRate=4.72*ProvAdditionalAmount=3*ImportCargoManagementNumber=NO", billpk1);
			CreateJobComInvoiceLine(invoicePK13, clusterKey3, entryLinePK11, "" , customsQuantities0);
			CreateJobComInvoiceLine(invoicePK13, clusterKey3, entryLinePK11, "" , customsQuantities0);
			var invoicePK14 = CreateJobComInvoiceHeader(declarationPK3, clusterKey3, 1414, 133m, "XX", "ImpContractExpiryDate=2024-03-03 11:20:23*ProvAdditionalRate=4.72*ProvAdditionalAmount=4*ImportCargoManagementNumber=NO", billpk1);
			CreateJobComInvoiceLine(invoicePK14, clusterKey3, entryLinePK12, "" , customsQuantities2);
			CreateJobComInvoiceLine(invoicePK14, clusterKey3, entryLinePK12, "" , customsQuantities0);
			var invoicePK15 = CreateJobComInvoiceHeader(declarationPK3, clusterKey3, 1515, 42m, "KG", "ImpContractExpiryDate=2024-03-03 11:20:23*ProvAdditionalRate=4.72*ProvAdditionalAmount=5*ImportCargoManagementNumber=NO", billpk1);
			CreateJobComInvoiceLine(invoicePK15, clusterKey3, entryLinePK12, "" , customsQuantities2);
			CreateJobComInvoiceLine(invoicePK15, clusterKey3, entryLinePK12, "" , customsQuantities0);
		}
	}
}
