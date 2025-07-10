using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USImportEntryLine))]
	class USImportEntryLineTest : DbCreateScriptTest
	{
		public void TestEndToEnd()
		{
			var usCompanyPK = Guid.NewGuid();
			var usBranchPK = CreateGlbBranch("US", usCompanyPK);
			var usDeclarationACEIMP = CreateData(usBranchPK, usCompanyPK,  "US", "IMP", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 1);
			var usDeclarationACSIMP = CreateData(usBranchPK, usCompanyPK,  "US", "IMP", "ACS", new DateTime(2020, 4, 9), "EntryFilerCode=SV8*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 2);
			var usDeclarationACEIMX = CreateData(usBranchPK, usCompanyPK,  "US", "IMX", "ACE", DateTime.MinValue, "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 3);
			var usDeclarationACEMSC = CreateData(usBranchPK, usCompanyPK,  "US", "MSC", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 4);
			var usDeclarationACEFTZ = CreateData(usBranchPK, usCompanyPK,  "US", "FTZ", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6010*EntryType=01", 5);
			var usDeclarationACEIMPFTZConsumption = CreateData(usBranchPK, usCompanyPK,  "USC", "IMP", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=06", 6);
			var usDeclarationACEMSCFTZConsumption = CreateData(usBranchPK, usCompanyPK,  "USC", "MSC", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=06", 7);
			var usDeclarationACEEXP = CreateData(usBranchPK, usCompanyPK,  "US", "EXP", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 8);

			var prCompanyPK = Guid.NewGuid();
			var prBranchPK = CreateGlbBranch("PR", prCompanyPK);
			var prDeclarationACEIMP = CreateData(prBranchPK, prCompanyPK, "PR", "IMP", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 9);

			var auCompanyPK = Guid.NewGuid();
			var auBranchPK = CreateGlbBranch("AU", auCompanyPK);
			var auDeclarationACEIMP = CreateData(auBranchPK, auCompanyPK, "AU", "IMP", "ACE", new DateTime(2020, 4, 10), "EntryFilerCode=SV9*EntryDate=2020-04-15*SchDEntry=6009*EntryType=01", 10);

			var sql = @"SELECT * FROM dbo.USImportEntryLine ORDER BY USE_EntryNum, USE_LineNumber, USE_AdValoremTariff";
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				var actualColumns = USImportEntryLineSchema.All.OfType<SchemaColumn>().Select(x => x.Name).ToList();
				AssertEquals("reader.FieldCount", actualColumns.Count, reader.FieldCount);
				var columns = new[]
				{
					USImportEntryLineSchema.Constants.PK, USImportEntryLineSchema.Constants.USE_AdValoremTariff, USImportEntryLineSchema.Constants.USE_CH,
					USImportEntryLineSchema.Constants.USE_ChildLineNum, USImportEntryLineSchema.Constants.USE_CL_ParentLine, USImportEntryLineSchema.Constants.USE_CustomsValue,
					USImportEntryLineSchema.Constants.USE_Description, USImportEntryLineSchema.Constants.USE_DutyRateDesc, USImportEntryLineSchema.Constants.USE_EntryDate,
					USImportEntryLineSchema.Constants.USE_EntryFilerCode, USImportEntryLineSchema.Constants.USE_EntryNum, USImportEntryLineSchema.Constants.USE_EntryPort,
					USImportEntryLineSchema.Constants.USE_HasMPF, USImportEntryLineSchema.Constants.USE_HMFAmountForEntry, USImportEntryLineSchema.Constants.USE_IsACE,
					USImportEntryLineSchema.Constants.USE_IsConsumptionFTZ, USImportEntryLineSchema.Constants.USE_IsFTZAdmission, USImportEntryLineSchema.Constants.USE_JE,
					USImportEntryLineSchema.Constants.USE_LineNumber, USImportEntryLineSchema.Constants.USE_MPFAmountForEntry, USImportEntryLineSchema.Constants.USE_SupLine
				};
				AssertEquals("columns.Length", actualColumns.Count, columns.Length);
				foreach (var column in columns)
				{
					Assert($"Removing column {column}", actualColumns.Remove(column));
				}
				if (actualColumns.Count > 0)
				{
					Fail("These columns are not tested:\r\n" + string.Join("\r\n", actualColumns));
				}
				CombineAssertions(() =>
				{
					AssertData("prDeclarationACEIMP 1", reader, prDeclarationACEIMP.DeclarationPK, prDeclarationACEIMP.EntryPK, prDeclarationACEIMP.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "PRACEIMP002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("prDeclarationACEIMP 2", reader, prDeclarationACEIMP.DeclarationPK, prDeclarationACEIMP.EntryPK, prDeclarationACEIMP.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "PRACEIMP002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("prDeclarationACEIMP 3", reader, prDeclarationACEIMP.DeclarationPK, prDeclarationACEIMP.EntryPK, prDeclarationACEIMP.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, prDeclarationACEIMP.EntryLine1PK, 4, "", 500m, 550m, "SV9", "PRACEIMP002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACEFTZ 1", reader, usDeclarationACEFTZ.DeclarationPK, usDeclarationACEFTZ.EntryPK, usDeclarationACEFTZ.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "USACEFTZ002", new DateTime(2020, 4, 10), "6010", true, true, false);
					AssertData("usDeclarationACEFTZ 2", reader, usDeclarationACEFTZ.DeclarationPK, usDeclarationACEFTZ.EntryPK, usDeclarationACEFTZ.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "USACEFTZ002", new DateTime(2020, 4, 10), "6010", true, true, false);
					AssertData("usDeclarationACEFTZ 3", reader, usDeclarationACEFTZ.DeclarationPK, usDeclarationACEFTZ.EntryPK, usDeclarationACEFTZ.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACEFTZ.EntryLine1PK, 4, "", 500m, 550m, "SV9", "USACEFTZ002", new DateTime(2020, 4, 10), "6010", true, true, false);
					AssertData("usDeclarationACEIMP 1", reader, usDeclarationACEIMP.DeclarationPK, usDeclarationACEIMP.EntryPK, usDeclarationACEIMP.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "USACEIMP002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACEIMP 2", reader, usDeclarationACEIMP.DeclarationPK, usDeclarationACEIMP.EntryPK, usDeclarationACEIMP.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "USACEIMP002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACEIMP 3", reader, usDeclarationACEIMP.DeclarationPK, usDeclarationACEIMP.EntryPK, usDeclarationACEIMP.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACEIMP.EntryLine1PK, 4, "", 500m, 550m, "SV9", "USACEIMP002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACEIMX 1", reader, usDeclarationACEIMX.DeclarationPK, usDeclarationACEIMX.EntryPK, usDeclarationACEIMX.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "USACEIMX002", new DateTime(2020, 4, 15), "6009", false, true, false);
					AssertData("usDeclarationACEIMX 2", reader, usDeclarationACEIMX.DeclarationPK, usDeclarationACEIMX.EntryPK, usDeclarationACEIMX.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "USACEIMX002", new DateTime(2020, 4, 15), "6009", false, true, false);
					AssertData("usDeclarationACEIMX 3", reader, usDeclarationACEIMX.DeclarationPK, usDeclarationACEIMX.EntryPK, usDeclarationACEIMX.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACEIMX.EntryLine1PK, 4, "", 500m, 550m, "SV9", "USACEIMX002", new DateTime(2020, 4, 15), "6009", false, true, false);
					AssertData("usDeclarationACEMSC 1", reader, usDeclarationACEMSC.DeclarationPK, usDeclarationACEMSC.EntryPK, usDeclarationACEMSC.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "USACEMSC002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACEMSC 2", reader, usDeclarationACEMSC.DeclarationPK, usDeclarationACEMSC.EntryPK, usDeclarationACEMSC.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "USACEMSC002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACEMSC 3", reader, usDeclarationACEMSC.DeclarationPK, usDeclarationACEMSC.EntryPK, usDeclarationACEMSC.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACEMSC.EntryLine1PK, 4, "", 500m, 550m, "SV9", "USACEMSC002", new DateTime(2020, 4, 10), "6009", false, true, false);
					AssertData("usDeclarationACSIMP 1", reader, usDeclarationACSIMP.DeclarationPK, usDeclarationACSIMP.EntryPK, usDeclarationACSIMP.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV8", "USACSIMP002", new DateTime(2020, 4, 9), "6009", false, false, false);
					AssertData("usDeclarationACSIMP 2", reader, usDeclarationACSIMP.DeclarationPK, usDeclarationACSIMP.EntryPK, usDeclarationACSIMP.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV8", "USACSIMP002", new DateTime(2020, 4, 9), "6009", false, false, false);
					AssertData("usDeclarationACSIMP 3", reader, usDeclarationACSIMP.DeclarationPK, usDeclarationACSIMP.EntryPK, usDeclarationACSIMP.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACSIMP.EntryLine1PK, 4, "", 500m, 550m, "SV8", "USACSIMP002", new DateTime(2020, 4, 9), "6009", false, false, false);
					AssertData("usDeclarationACEIMPFTZConsumption 1", reader, usDeclarationACEIMPFTZConsumption.DeclarationPK, usDeclarationACEIMPFTZConsumption.EntryPK, usDeclarationACEIMPFTZConsumption.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "USCACEIMP02", new DateTime(2020, 4, 10), "6009", false, true, true);
					AssertData("usDeclarationACEIMPFTZConsumption 2", reader, usDeclarationACEIMPFTZConsumption.DeclarationPK, usDeclarationACEIMPFTZConsumption.EntryPK, usDeclarationACEIMPFTZConsumption.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "USCACEIMP02", new DateTime(2020, 4, 10), "6009", false, true, true);
					AssertData("usDeclarationACEIMPFTZConsumption 3", reader, usDeclarationACEIMPFTZConsumption.DeclarationPK, usDeclarationACEIMPFTZConsumption.EntryPK, usDeclarationACEIMPFTZConsumption.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACEIMPFTZConsumption.EntryLine1PK, 4, "", 500m, 550m, "SV9", "USCACEIMP02", new DateTime(2020, 4, 10), "6009", false, true, true);
					AssertData("usDeclarationACEMSCFTZConsumption 1", reader, usDeclarationACEMSCFTZConsumption.DeclarationPK, usDeclarationACEMSCFTZConsumption.EntryPK, usDeclarationACEMSCFTZConsumption.EntryLine1PK, 1, "10000000", 1000m, "DESC 1", true, false, Guid.Empty, 3, "DUTY RATE DESC 1", 500m, 550m, "SV9", "USCACEMSC02", new DateTime(2020, 4, 10), "6009", false, true, true);
					AssertData("usDeclarationACEMSCFTZConsumption 2", reader, usDeclarationACEMSCFTZConsumption.DeclarationPK, usDeclarationACEMSCFTZConsumption.EntryPK, usDeclarationACEMSCFTZConsumption.EntryLine2PK, 2, "20000000", 2000m, "DESC 2", false, true, Guid.Empty, 0, "DUTY RATE DESC 2", 500m, 550m, "SV9", "USCACEMSC02", new DateTime(2020, 4, 10), "6009", false, true, true);
					AssertData("usDeclarationACEMSCFTZConsumption 3", reader, usDeclarationACEMSCFTZConsumption.DeclarationPK, usDeclarationACEMSCFTZConsumption.EntryPK, usDeclarationACEMSCFTZConsumption.EntryLine3PK, 3, "30000000", 3000m, "DESC 3", false, false, usDeclarationACEMSCFTZConsumption.EntryLine1PK, 4, "", 500m, 550m, "SV9", "USCACEMSC02", new DateTime(2020, 4, 10), "6009", false, true, true);
					AssertEquals("No more records", false, reader.Read());
				});
			}
		}

		void AssertData(string indicator, IDataReader reader, Guid declarationPK, Guid entryPK, Guid entryLinePK, short lineNumber, string adValoremTariff, decimal customsValue, string description,
			bool supLine, bool hasMPF, Guid parentEntryLinePK, int childLineNum, string dutyRateDesc,
			decimal mpfAmountForEntry, decimal hmfAmountForEntry,
			string entryFilerCode, string entryNum, DateTime entryDate, string entryPort, bool isFTZAdmission, bool isACE, bool isConsumptionFTZ)
		{
			Assert(indicator + " has row", reader.Read());
			var actualEntryNum = (string)reader[USImportEntryLineSchema.Constants.USE_EntryNum];
			var actualLineNumber = (short)reader[USImportEntryLineSchema.Constants.USE_LineNumber];
			indicator += $" {actualEntryNum}-{actualLineNumber} ";
			AssertEquals(indicator + USImportEntryLineSchema.Constants.PK, entryLinePK, (Guid)reader[USImportEntryLineSchema.Constants.PK]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_AdValoremTariff, adValoremTariff, (string)reader[USImportEntryLineSchema.Constants.USE_AdValoremTariff]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_CH, entryPK, (Guid)reader[USImportEntryLineSchema.Constants.USE_CH]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_ChildLineNum, childLineNum, (int)reader[USImportEntryLineSchema.Constants.USE_ChildLineNum]);
			var use_CL_ParentLine = reader[USImportEntryLineSchema.Constants.USE_CL_ParentLine];
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_CL_ParentLine, parentEntryLinePK, use_CL_ParentLine == DBNull.Value ? Guid.Empty : (Guid)use_CL_ParentLine);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_CustomsValue, customsValue, (decimal)reader[USImportEntryLineSchema.Constants.USE_CustomsValue]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_Description, description, (string)reader[USImportEntryLineSchema.Constants.USE_Description]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_EntryDate, entryDate, (DateTime)reader[USImportEntryLineSchema.Constants.USE_EntryDate]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_EntryFilerCode, entryFilerCode, (string)reader[USImportEntryLineSchema.Constants.USE_EntryFilerCode]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_EntryNum, entryNum, actualEntryNum);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_EntryPort, entryPort, (string)reader[USImportEntryLineSchema.Constants.USE_EntryPort]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_HasMPF, hasMPF, (bool)reader[USImportEntryLineSchema.Constants.USE_HasMPF]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_HMFAmountForEntry, hmfAmountForEntry, (decimal)reader[USImportEntryLineSchema.Constants.USE_HMFAmountForEntry]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_IsACE, isACE, (bool)reader[USImportEntryLineSchema.Constants.USE_IsACE]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_IsConsumptionFTZ, isConsumptionFTZ, (bool)reader[USImportEntryLineSchema.Constants.USE_IsConsumptionFTZ]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_IsFTZAdmission, isFTZAdmission, (bool)reader[USImportEntryLineSchema.Constants.USE_IsFTZAdmission]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_JE, declarationPK, (Guid)reader[USImportEntryLineSchema.Constants.USE_JE]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_LineNumber, lineNumber, actualLineNumber);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_MPFAmountForEntry, mpfAmountForEntry, (decimal)reader[USImportEntryLineSchema.Constants.USE_MPFAmountForEntry]);
			AssertEquals(indicator + USImportEntryLineSchema.Constants.USE_SupLine, supLine, (bool)reader[USImportEntryLineSchema.Constants.USE_SupLine]);
		}

		Guid CreateGlbBranch(string countryCode, Guid companyPK)
		{
			var branchPK = Guid.NewGuid();
			var branchSql = @"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) 
VALUES (@CompanyPK, @CountryCode + 'C', 'US company', @CountryCode, @CountryCode + 'D')

INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC)
VALUES (@BranchPK, @CountryCode + 'B', @CompanyPK)
";

			using (var command = Db.Connection.Command(branchSql))
			{
				command.AddParameter("@CountryCode", SqlDbType.Char, countryCode);
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}
			return branchPK;
		}

		(Guid DeclarationPK, Guid EntryPK, Guid EntryLine1PK, Guid EntryLine2PK, Guid EntryLine3PK) CreateData(Guid branchPK, Guid companyPK, string prefix, string messageType, string applicationCode, DateTime dateOfArrival, string addInfo, int clusterKey)
		{
			var declarationPK = Guid.NewGuid();
			var entryPK = Guid.NewGuid();
			var entryLine1PK = Guid.NewGuid();
			var entryLine2PK = Guid.NewGuid();
			var entryLine3PK = Guid.NewGuid();
			var declarationReference = $"{prefix}{applicationCode}{messageType}".PadRight(10, '0');
			var creationSql = @"
INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_GB, JE_GC, JE_ApplicationCode, JE_MessageType, JE_DateOfArrival, JE_AddInfo, JE_ClusterKey)
VALUES (@DeclarationPK, 'US', @DeclarationReference, @BranchPK, @CompanyPK, @ApplicationCode, @MessageType, @DateOfArrival, @AddInfo, @ClusterKey)

INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_RN_NKCountryCode, CE_EntryType, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES (NEWID(), @DeclarationPK, 'JobDeclaration', 'US', 'ENS', @DeclarationReference + '2', getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_RN_NKCountryCode, CE_EntryType, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES (NEWID(), @DeclarationPK, 'JobDeclaration', 'U1', 'ENS', @DeclarationReference + '1', getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_RN_NKCountryCode, CE_EntryType, CE_EntryNum, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser)
VALUES (NEWID(), @DeclarationPK, 'JobDeclaration', 'US', 'EN1', @DeclarationReference + '3', getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@EntryPK, 'US', @DeclarationPK, 'ENS', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeType, C1_ChargeAmount, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser)
VALUES (NEWID(), @EntryPK, '499', 400, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
INSERT dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeType, C1_ChargeAmount, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser)
VALUES (NEWID(), @EntryPK, '499', 500, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
INSERT dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeType, C1_ChargeAmount, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser)
VALUES (NEWID(), @EntryPK, '501', 450, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
INSERT dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeType, C1_ChargeAmount, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser)
VALUES (NEWID(), @EntryPK, '501', 550, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_AdValoremTariff, CL_CustomsValue, CL_Description, CL_AddInfo, CL_CustomsPostedStatus, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@EntryLine1PK, 'US', @EntryPK, 1, '10000000', 1000, 'DESC 1', 'SupLine=Y*ChildLineNum=3*DutyRateDesc=DUTY RATE DESC 1', 'ACT', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_AdValoremTariff, CL_CustomsValue, CL_Description, CL_AddInfo, CL_CustomsPostedStatus, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@EntryLine2PK, 'US', @EntryPK, 2, '20000000', 2000, 'DESC 2', 'SupLine=N*HasMPF=Y*DutyRateDesc=DUTY RATE DESC 2', 'ACT', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_AdValoremTariff, CL_CustomsValue, CL_Description, CL_AddInfo, CL_CustomsPostedStatus, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@EntryLine3PK, 'US', @EntryPK, 3, '30000000', 3000, 'DESC 3', 'SupLine=N*HasMPF=N*CL_ParentLine=' + CAST(@EntryLine1PK AS VARCHAR(36)) + '*ChildLineNum=4', 'ACT', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

DECLARE @EntryLine4NonActivePK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_AdValoremTariff, CL_CustomsValue, CL_Description, CL_AddInfo, CL_CustomsPostedStatus, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@EntryLine4NonActivePK, 'US', @EntryPK, 4, '40000000', 4000, 'DESC 4', 'SupLine=N*HasMPF=Y*CL_ParentLine=' + CAST(@EntryLine1PK AS VARCHAR(36)) + '*ChildLineNum=5*DutyRateDesc=DUTY RATE DESC 4', 'NCT', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

DECLARE @EntryNonENSPK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
VALUES (@EntryNonENSPK, 'US', @DeclarationPK, 'EN1', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

INSERT dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeType, C1_ChargeAmount, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser)
VALUES (NEWID(), @EntryNonENSPK, '499', 700, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
INSERT dbo.CusEntryHeaderCharges (C1_PK, C1_CH, C1_ChargeType, C1_ChargeAmount, C1_ClusterKey, C1_SystemCreateTimeUtc, C1_SystemCreateUser, C1_SystemLastEditTimeUtc, C1_SystemLastEditUser)
VALUES (NEWID(), @EntryNonENSPK, '501', 800, @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')

DECLARE @EntryLineNonENSPK UNIQUEIDENTIFIER = NEWID()
INSERT dbo.CusEntryLine (CL_PK, CL_DataModel, CL_CH, CL_LineNumber, CL_AdValoremTariff, CL_CustomsValue, CL_Description, CL_AddInfo, CL_CustomsPostedStatus, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser)
VALUES (@EntryLineNonENSPK, 'US', @EntryNonENSPK, 1, '50000000', 5000, 'DESC 5', 'SupLine=Y*HasMPF=Y*ChildLineNum=5*DutyRateDesc=DUTY RATE DESC 5', 'ACT', @ClusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
";

			using (var command = Db.Connection.Command(creationSql))
			{
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@DeclarationReference", SqlDbType.VarChar, declarationReference);
				command.AddParameter("@ApplicationCode", SqlDbType.Char, applicationCode);
				command.AddParameter("@MessageType", SqlDbType.Char, messageType);
				if (dateOfArrival == DateTime.MinValue)
				{
					command.AddParameter("@DateOfArrival", SqlDbType.DateTime, DBNull.Value);
				}
				else
				{
					command.AddParameter("@DateOfArrival", SqlDbType.DateTime, dateOfArrival);
				}
				command.AddParameter("@AddInfo", SqlDbType.VarChar, addInfo);
				command.AddParameter("@EntryLine1PK", SqlDbType.UniqueIdentifier, entryLine1PK);
				command.AddParameter("@EntryLine2PK", SqlDbType.UniqueIdentifier, entryLine2PK);
				command.AddParameter("@EntryLine3PK", SqlDbType.UniqueIdentifier, entryLine3PK);
				command.AddParameter("@DeclarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@EntryPK", SqlDbType.UniqueIdentifier, entryPK);
				command.AddParameter("@ClusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
			return (declarationPK, entryPK, entryLine1PK, entryLine2PK, entryLine3PK);
		}
	}
}
