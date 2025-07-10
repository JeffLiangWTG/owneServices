using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA
{
	[TestedType(typeof(Report_ZAInvoiceLines))]
	class Report_ZAInvoiceLinesTest : DbCreateScriptTest
	{
		public void TestReport_ZAInvoiceLines()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "ZA", "ZAR");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			Db.Connection.ExecuteNonQuery(@"
IF NOT EXISTS (SELECT NULL FROM RefDatabase_RefDataGrouping where ZZZ_DataGrouping = 'ZA')
BEGIN
	INSERT INTO RefDatabase_RefDataGrouping(ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping)
	VALUES('595F5657-9AC9-4381-B990-F37D1940FCBE', 'ZA', 'South Africa', NULL)
END
IF NOT EXISTS (SELECT 1 FROM RefDatabase_RefCusProcedure where ZZ6_ProcedureCode ='12' and ZZ6_PreviousProcedureCode ='00')
BEGIN
	INSERT INTO RefDatabase_RefCusProcedure (ZZ6_PK,ZZ6_Category,ZZ6_ProcedureCode,ZZ6_PreviousProcedureCode,ZZ6_Concession,ZZ6_Description,ZZ6_ZZZ_NKDataGrouping,ZZ6_ShipmentType,ZZ6_CalculateDuty,ZZ6_Group,ZZ6_LandedCost,ZZ6_IntoWarehouse,ZZ6_OutOfWarehouse,ZZ6_StartDate,ZZ6_EndDate)
	VALUES (NEWID(), 'A','12','00','13A, 13B, 13C, 13D, 4','Home Use'' and payment of VAT, on goods received from the BLNS states.','ZA','IMP',1,'BLNS',0,'N','N', '1900-01-01', '2076-01-01')
END
if not exists(select null from RefDatabase_RefCusRateType where ZZR_RateType = 'REB' and ZZR_ZZZ_NKDataGrouping = 'ZA')
begin
	insert into RefDatabase_RefCusRateType(ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_IsPayable, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula)
	values('56DE3B48-A6B7-4BD2-88A0-FBFEF65CAEFF', 'REB', 'Rebate', 0, 'ZA', '')
end
if not exists (select null from RefDatabase_RefCusRateCode where ZY1_RateCode = '4P2' and ZY1_Description = 'Schedule 4 Part 2')
begin
	insert into RefDatabase_RefCusRateCode(ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
	values ('7C869805-BCEF-403B-9485-7A948C6AEFE3', '4P2', '56DE3B48-A6B7-4BD2-88A0-FBFEF65CAEFF', 'Schedule 4 Part 2')
end");

			var declarationPK = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, dataModel: "ZA");
			var invoiceGroupPK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, true, 1, dataModel: "ZA");
			var invoicePK = TestDataCreator.CreateJobComInvoiceHeader(declarationPK, false, 1, dataModel: "ZA");
			var invoiceLinePK = CreateJobComInvoiceLine(invoicePK, AddCusInstruction(declarationPK, "12", 1), "00", 1);
			_ = CreateCusVehicle(invoiceLinePK, 1);

			var reportSql = @"select CPC, PPC, RefundRebateCode, RefundRebateValue, ROOCert, AssessmentDate from Report_ZAInvoiceLines(@companyPK, @messageType, null, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("CPC", "12", reader.GetString(0));
					AssertEquals("PPC", "00", reader.GetString(1));
					AssertEquals("RefundRebateCode", "", reader.GetString(2));
					AssertEquals("RefundRebateValue", 0m, reader.GetDecimal(3));
					AssertEquals("AssessmentDate", new DateTime(2012, 5, 31, 23, 59, 0), reader.GetDateTime(5));
				}
			}

			AddCusLineTariffDetail(invoiceLinePK, "4P2", "460110100", "12.3", "ZA");

			AddRebateCharge(declarationPK, invoiceLinePK, "4P2", "23.45", 1);

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("CPC", "12", reader.GetString(0));
					AssertEquals("PPC", "00", reader.GetString(1));
					AssertEquals("RefundRebateCode", "460110100", reader.GetString(2));
					AssertEquals("RefundRebateValue", 23.45m, reader.GetDecimal(3));
				}
			}

			UpdateCusLineTariffDetail(invoiceLinePK, "1P2");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("CPC", "12", reader.GetString(0));
					AssertEquals("PPC", "00", reader.GetString(1));
					AssertEquals("RefundRebateCode", "", reader.GetString(2));
					AssertEquals("RefundRebateValue", 0m, reader.GetDecimal(3));
				}
			}

			UpdateDeclarationAdditionalInfo(declarationPK, "ROOCert=DECLARATION");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("ROOCert", "DECLARATION", reader.GetString(4));
				}
			}

			UpdateInvoiceAdditionalInfo(invoicePK, "ROOCert=INVOICE-HEAD");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("ROOCert", "INVOICE-HEAD", reader.GetString(4));
				}
			}

			UpdateInvoiceLineAdditionalInfo(invoiceLinePK, "ROOCert=INVOICE-LINE");

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals("ROOCert", "INVOICE-LINE", reader.GetString(4));
				}
			}

			var additionalInfoHelper = new AdditionalInfoHelper(new[]
			{
				new AdditionalInfoConfig("ActualPrice", 1111.11m),
				new AdditionalInfoConfig("CommissionNumber", "CN1234"),
				new AdditionalInfoConfig("CustomsValueCurrencyOverride", "RX_NKCustomsValueCurrencyOverride", "ABC"),
				new AdditionalInfoConfig("CustomsValueOverride", 2222.22m),
				new AdditionalInfoConfig("DiamondBeneficiaryLicense", "L1234"),
				new AdditionalInfoConfig("DiamondDealerLicense", "L2345"),
				new AdditionalInfoConfig("DiamondLevyValue", 123456m),
				new AdditionalInfoConfig("DiamondProducerRegistration", "R1234"),
				new AdditionalInfoConfig("DiamondProducerExemption", "E1234"),
				new AdditionalInfoConfig("ElectionsExemptionsLevy", "EE1234"),
				new AdditionalInfoConfig("NewUsed", "S"),
				new AdditionalInfoConfig("PermitNumber", "P1234"),
				new AdditionalInfoConfig("TakeUpInTradeStatistics", false),
				new AdditionalInfoConfig("TargetEntryLineNumber", (short)123),
			});
			UpdateInvoiceLineAdditionalInfo(invoiceLinePK, additionalInfoHelper.AdditionalInfoText);

			var updateSQL = @"
UPDATE dbo.JobComInvoiceLine
SET
	JI_Model = @model,
	JI_ValuationMarkup = @valuationMarkup,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = @invoiceLinePK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@model", SqlDbType.VarChar, "A MODEL");
				command.AddParameter("@valuationMarkup", SqlDbType.Decimal, 1234.56m);
				command.ExecuteNonQuery();
			}

			reportSql = $"select {additionalInfoHelper.SelectList} from Report_ZAInvoiceLines(@companyPK, @messageType, null, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageType", SqlDbType.VarChar, "IMP");

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("only one record", reader.Read());
						foreach (var config in additionalInfoHelper.Configurations)
						{
							AssertEquals($"Additional info \"{config.ColumnName}\" from ModelView", config.Value, reader[config.ColumnName]);
						}
					});
				}
			}
		}

		Guid CreateJobComInvoiceLine(Guid jobComInvoiceHeaderPK, Guid instructionPK, string ppc, int clusterKey)
		{
			var jobComInvoiceLinePK = Guid.NewGuid();
			var sql = @"
INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_DataModel, JI_JZ, JI_CEI, JI_Procedure, JI_ClusterKey)
VALUES(@jobComInvoiceLinePK, 'ZA', @jobComInvoiceHeaderPK, @instructionPK, @ppc, @clusterKey)
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.AddParameter("@jobComInvoiceHeaderPK", SqlDbType.UniqueIdentifier, jobComInvoiceHeaderPK);
				command.AddParameter("@instructionPK", SqlDbType.UniqueIdentifier, instructionPK);
				command.AddParameter("@ppc", SqlDbType.VarChar, "  " + ppc);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return jobComInvoiceLinePK;
		}

		Guid CreateCusVehicle(Guid jobComInvoiceLinePK, int clusterKey)
		{
			var cusVehiclePK = Guid.NewGuid();
			var sql = @"
INSERT INTO [dbo].[CusVehicle] ([CVH_PK],[CVH_AutoVersion],[CVH_ClusterKey],[CVH_SerialNumber],[CVH_ModelName],[CVH_VehicleIdentificationNumber],[CVH_ManufacturedDate],[CVH_ParentID],[CVH_ParentTableCode],[CVH_SystemCreateTimeUtc],[CVH_SystemCreateUser],[CVH_SystemLastEditTimeUtc],[CVH_SystemLastEditUser],[CVH_CarType],[CVH_DataModel],[CVH_EngineCapacity],[CVH_EngineCapacityUQ],[CVH_Color],[CVH_SupplyMethod])
VALUES(@cusVehiclePK, 1, @clusterKey, 'EN-123456789', 'ModelName', 'VIN-1234', '2023-01-01', @jobComInvoiceLinePK, 'JI', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'A VEHICLE TYPE', 'ZA', 3000, 'CC', 'WHITE', 'OTH')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@cusVehiclePK", SqlDbType.UniqueIdentifier, cusVehiclePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@jobComInvoiceLinePK", SqlDbType.UniqueIdentifier, jobComInvoiceLinePK);
				command.ExecuteNonQuery();
			}
			return cusVehiclePK;
		}

		Guid AddCusInstruction(Guid declarationPK, string cpc, int clusterKey)
		{
			var cusInstructionPK = Guid.NewGuid();

			var cusEntryHeaderSQL = @"
			insert into dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_Style, CEI_JE, CEI_ClusterKey, CEI_DateForDuty, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) values (@cusIntructionPK, 'ZA', @procedureCode, @declarationPK, @clusterKey, DATETIMEFROMPARTS(2012, 5, 31, 23, 59, 0, 0), getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@cusIntructionPK", SqlDbType.UniqueIdentifier, cusInstructionPK);
				command.AddParameter("@procedureCode", SqlDbType.VarChar, cpc);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			return cusInstructionPK;
		}

		void AddCusLineTariffDetail(Guid invoiceLinePK, string part, string code, string value, string datamodel)
		{
			var tariffDetailPK = Guid.NewGuid();

			var cusEntryHeaderSQL = @"
insert into dbo.CusLineTariffDetail (BZ_PK, BZ_ParentTableCode, BZ_ParentID, BZ_Type, BZ_Tariff, BZ_Value, BZ_DataModel) values (@tariffDetailPK, 'JI', @invoiceLinePK, @part, @code, @value, @datamodel)";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@tariffDetailPK", SqlDbType.UniqueIdentifier, tariffDetailPK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@part", SqlDbType.VarChar, part);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@value", SqlDbType.Decimal, value);
				command.AddParameter("@datamodel", SqlDbType.VarChar, datamodel);

				command.ExecuteNonQuery();
			}
		}

		void UpdateCusLineTariffDetail(Guid invoiceLinePK, string part)
		{
			var updateSQL = @"
UPDATE dbo.CusLineTariffDetail 
SET 
    BZ_Type = @part, 
    BZ_SystemLastEditTimeUtc = GETUTCDATE(), 
    BZ_SystemLastEditUser = '~BP' 
WHERE 
    BZ_ParentTableCode = 'JI' 
    AND BZ_ParentID = @invoiceLinePK;";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@part", SqlDbType.VarChar, part);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.ExecuteNonQuery();
			}
		}

		void AddRebateCharge(Guid declarationPK, Guid invoiceLinePK, string part, string value, int clusterKey)
		{
			var cusEntryHeaderPK = Guid.NewGuid();

			var cusEntryHeaderSQL = @"
insert into dbo.CusEntryHeader(CH_PK, CH_DataModel, CH_IsValid, CH_MessageType, CH_JE, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) values (@cusEntryHeaderPK, 'ZA', 1, 'IMP', @declarationPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var cusEntryLinePK = Guid.NewGuid();

			var cusEntryLineSQL = @"
insert into dbo.CusEntryLine(CL_PK, CL_DataModel, CL_IsValid, CL_LineNumber, CL_CustomsValue, CL_Description, CL_CH, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) values (@cusEntryLinePK, 'ZA', 1, 1, 200, 'LineDesc', @cusEntryHeaderPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryLineSQL))
			{
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var cusEntryLineFeePK = Guid.NewGuid();

			var cusEntryLineFeeSQL = @"
insert into dbo.CusEntryLineFee (CF_PK, CF_IsValid, CF_ChargeAmount, CF_CL, CF_ChargeType, CF_ClusterKey, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser) values (@cusEntryLineFeePK, 1, @value, @cusEntryLinePK, @part, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryLineFeeSQL))
			{
				command.AddParameter("@cusEntryLineFeePK", SqlDbType.UniqueIdentifier, cusEntryLineFeePK);
				command.AddParameter("@value", SqlDbType.Decimal, value);
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@part", SqlDbType.VarChar, part);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			var updateSQL = @"
UPDATE dbo.JobComInvoiceLine
SET
	JI_CL = @cusEntryLinePK,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = @invoiceLinePK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@cusEntryLinePK", SqlDbType.UniqueIdentifier, cusEntryLinePK);
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.ExecuteNonQuery();
			}
		}

		void UpdateDeclarationAdditionalInfo(Guid declarationPK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_AddInfo = @additionalInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
				command.ExecuteNonQuery();
			}
		}

		void UpdateInvoiceAdditionalInfo(Guid invoicePK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobComInvoiceHeader
SET
	JZ_AddInfo = @additionalInfo,
	JZ_SystemLastEditTimeUtc = GETUTCDATE(),
	JZ_SystemLastEditUser = '~BP'
WHERE
	JZ_PK = @invoicePK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@invoicePK", SqlDbType.UniqueIdentifier, invoicePK);
				command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
				command.ExecuteNonQuery();
			}
		}

		void UpdateInvoiceLineAdditionalInfo(Guid invoiceLinePK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobComInvoiceLine
SET
	JI_AddInfo = @additionalInfo,
	JI_SystemLastEditTimeUtc = GETUTCDATE(),
	JI_SystemLastEditUser = '~BP'
WHERE
	JI_PK = @invoiceLinePK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, invoiceLinePK);
				command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
				command.ExecuteNonQuery();
			}
		}
	}
}

