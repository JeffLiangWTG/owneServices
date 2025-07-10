using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	sealed class CustomsDataReportingTest : TransactionedTestCase
	{
		public void TestZADataReporting()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllStlScriptFactory.SetTemporaryFeatureCodeFilterForTest(new[] { "MSC", "IFB", "EFI", "IFI", "EF2", "IFL", "IF2" }))
			{
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = ZDateTime.UtcToday.AddMonths(-1).AddDays(-3).ToDateTime();
				var sqlText = @"
DECLARE @ZACompanyPK UNIQUEIDENTIFIER = NEWID()
DECLARE @ZABranchPK UNIQUEIDENTIFIER = NEWID()

INSERT INTO dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (@ZACompanyPK, 'ZA', 'ZA$', 'ZA company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (@ZABranchPK, @ZACompanyPK, 'ZA#')

DECLARE @MessageTypeCreateDecScript NVARCHAR(MAX), @ApplicationCodeCreateDecScript NVARCHAR(MAX), @CreateDecScript NVARCHAR(MAX)
SET @CreateDecScript = '
SET @DecPK = NEWID();
SET @ClusterKey = (SELECT ISNULL(MAX(JE_ClusterKey), 0) FROM dbo.JobDeclaration) + 1;
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ApplicationCode, JE_TransportMode, JE_SystemCreateTimeUtc, JE_ClusterKey) VALUES(@DecPK, ''ZA'', @ZABranchPK, @ZACompanyPK, ''<0><1><2>1'', ''<0>'', ''<1>'', ''<2>'', DATEADD(MONTH, -1, GETUTCDATE()), @ClusterKey);
SET @EntryInstructionPK = NEWID();
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@EntryInstructionPK, ''ZA'', @DecPK, @ClusterKey, getdate(), ''~BP'', getdate(), ''~BP'');
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_CEI_Instruction, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES (NEWID(), ''ZA'', @DecPK, @EntryInstructionPK, DATEADD(MONTH, -1, GETUTCDATE()), @ClusterKey, getdate(), ''~BP'', getdate(), ''~BP'');
SET @DecPK = NEWID();
SET @ClusterKey += 1;
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ApplicationCode, JE_TransportMode, JE_SystemCreateTimeUtc, JE_ClusterKey) VALUES(@DecPK, ''ZA'', @ZABranchPK, @ZACompanyPK, ''<0><1><2>2'', ''<0>'', ''<1>'', ''<2>'', DATEADD(MONTH, -1, GETUTCDATE()), @ClusterKey);
SET @DecPK = NEWID();
SET @ClusterKey += 1;
INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_GB, JE_GC, JE_DeclarationReference, JE_MessageType, JE_ApplicationCode, JE_TransportMode, JE_SystemCreateTimeUtc, JE_ClusterKey) VALUES(@DecPK, ''ZA'', @ZABranchPK, @ZACompanyPK, ''<0><1><2>3'', ''<0>'', ''<1>'', ''<2>'', DATEADD(MONTH, -1, GETUTCDATE()), @ClusterKey);
SET @EntryInstructionPK = NEWID();
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@EntryInstructionPK, ''ZA'', @DecPK, @ClusterKey, getdate(), ''~BP'', getdate(), ''~BP'');
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_CEI_Instruction, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES (NEWID(), ''ZA'', @DecPK, @EntryInstructionPK, DATEADD(MONTH, -1, GETUTCDATE()), @ClusterKey, getdate(), ''~BP'', getdate(), ''~BP'');
SET @EntryInstructionPK = NEWID();
INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_JE, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@EntryInstructionPK, ''ZA'', @DecPK, @ClusterKey, getdate(), ''~BP'', getdate(), ''~BP'');
INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_CEI_Instruction, CH_EntrySubmittedDate, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES (NEWID(), ''ZA'', @DecPK, @EntryInstructionPK, DATEADD(MONTH, -1, GETUTCDATE()), @ClusterKey, getdate(), ''~BP'', getdate(), ''~BP'');
'
DECLARE MessageTypeCursor CURSOR FAST_FORWARD READ_ONLY FOR
SELECT Code
FROM
(
	SELECT 'MSC' AS Code
	UNION ALL
	SELECT 'IMX' AS Code
	UNION ALL
	SELECT 'EXP' AS Code
	UNION ALL
	SELECT 'EXW' AS Code
	UNION ALL
	SELECT 'IMP' AS Code
	UNION ALL
	SELECT 'OTH' AS Code
) AS Data

DECLARE @Code VARCHAR(3)

OPEN MessageTypeCursor
FETCH NEXT FROM MessageTypeCursor INTO @Code
WHILE @@FETCH_STATUS = 0
BEGIN
--	SET @Code = 'MSC'
	SET @MessageTypeCreateDecScript = REPLACE(@CreateDecScript, '<0>', @Code)
	SET @ApplicationCodeCreateDecScript = REPLACE(@MessageTypeCreateDecScript, '<1>', 'BLT')
	DECLARE @UpdateQuery NVARCHAR(MAX) =
		'DECLARE @EntryInstructionPK UNIQUEIDENTIFIER, @DecPK UNIQUEIDENTIFIER = NEWID(), @ClusterKey INT, @ZABranchPK UNIQUEIDENTIFIER, @ZACompanyPK UNIQUEIDENTIFIER;'
		+ 'SELECT @ZABranchPK = GB_PK, @ZACompanyPK = GB_GC FROM dbo.GlbBranch WHERE GB_Code = ''ZA#'';'
		+ REPLACE(@ApplicationCodeCreateDecScript, '<2>', 'ROA')
		+ REPLACE(@ApplicationCodeCreateDecScript, '<2>', 'RAI')
		+ REPLACE(@ApplicationCodeCreateDecScript, '<2>', 'SEA')

	SET @ApplicationCodeCreateDecScript = REPLACE(@MessageTypeCreateDecScript, '<1>', 'ITF')
	SET @UpdateQuery = @UpdateQuery + REPLACE(@ApplicationCodeCreateDecScript, '<2>', 'ROA') + REPLACE(@ApplicationCodeCreateDecScript, '<2>', 'RAI') + REPLACE(@ApplicationCodeCreateDecScript, '<2>', 'SEA')
	EXECUTE sp_executesql @UpdateQuery

	FETCH NEXT FROM MessageTypeCursor INTO @Code
END

CLOSE MessageTypeCursor;
DEALLOCATE MessageTypeCursor;
";
				TestConnection.ExecuteNonQuery(sqlText);
				TestConnection.ExecuteNonQuery("TRUNCATE TABLE StmUsageData");
				var scriptLoader = new ScriptLoader(new CollectionTimeProvider(), new LoggerForTest());
				var retriever = new StlRetrieverForTest(scriptLoader);
				retriever.CollectAndSend(CancellationToken.None);

				var billingTransactions = new List<BillingTransaction>();

				using (var cmd = TestConnection.Command("SELECT SUD_Data FROM dbo.StmUsageData WHERE SUD_Code IN ('MSC', 'IFB', 'EFC', 'IFG', 'EF2', 'IFL', 'IF2', 'EFI', 'IFI')"))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						billingTransactions.Add(BillingManager.DecryptTransaction(reader[0].ToString(), BillingManager.CurrentSchemaVersion));
					}
				}

				CombineAssertions(() =>
				{
					AssertBillingTransaction(billingTransactions, "MSC", 18);
					AssertBillingTransaction(billingTransactions, "IFB", 18);
					AssertBillingTransaction(billingTransactions, "EFI", 9);
					AssertBillingTransaction(billingTransactions, "IFI", 9);
					AssertBillingTransaction(billingTransactions, "EF2", 12);
					AssertBillingTransaction(billingTransactions, "IFL", 34);
					AssertBillingTransaction(billingTransactions, "IF2", 8);
				});
			}
		}

		public void TestDataReportingForAUCustoms()
		{
			AssertDataReporting(TestConnection.SetupAUData,
				("DRW", 72),
				("EFC", 72),
				("IFG", 72),
				("IFL", 72),
				("MSC", 72),
				("RFP", 72));
		}

		public void TestDataReportingForCACustoms()
		{
			AssertDataReporting(TestConnection.SetupCAData,
				("CB2", 18),
				("EFC", 18),
				("IFG", 9),
				("IFL", 9),
				("LVI", 18),
				("LVS", 18),
				("MSC", 18));
		}

		public void TestDataReportingForDECustoms()
		{
			AssertDataReporting(TestConnection.SetupDEData,
				("EF2", 24),
				("EFI", 36),
				("IF2", 60),
				("IFI", 90),
				("IFL", 195));
		}

		public void TestDataReportingForGBCustoms()
		{
			AssertDataReporting(TestConnection.SetupGBData,
				("EFC", 72),
				("IFG", 36),
				("IFL", 36),
				("MSC", 72));
		}

		public void TestDataReportingForNZCustoms()
		{
			AssertDataReporting(TestConnection.SetupNZData,
				("ECE", 54),
				("ECM", 54),
				("EFC", 108),
				("IFG", 189),
				("IFL", 189),
				("MSC", 810));
		}

		public void TestDataReportingForPRCustoms()
		{
			AssertDataReporting(TestConnection.SetupPRData,
				("DRW", 54),
				("EFC", 54),
				("FTZ", 54),
				("IFB", 54),
				("IFG", 27),
				("IFL", 27),
				("MSC", 54),
				("URC", 54),
				("USP", 54));
		}

		public void TestDataReportingForSGCustoms()
		{
			AssertDataReporting(TestConnection.SetupSGData,
				("COO", 72),
				("EFC", 72),
				("IFG", 72),
				("IFL", 72),
				("TNP", 72));
		}

		public void TestDataReportingForUSCustoms()
		{
			AssertDataReporting(TestConnection.SetupUSData,
				("DRW", 54),
				("EFC", 54),
				("FTZ", 54),
				("IFB", 54),
				("IFG", 27),
				("IFL", 27),
				("MSC", 54),
				("URC", 54),
				("USP", 54));
		}

		public void TestDataReportingForZACustoms()
		{
			AssertDataReporting(TestConnection.SetupZAData,
				("EF2", 24),
				("EFI", 36),
				("IF2", 24),
				("IFB", 54),
				("IFI", 72),
				("IFL", 96),
				("MSC", 54));
		}

		void AssertDataReporting(Action<string> setupData, params (string code, int expectedBillableCount)[] priceCodeDetails)
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTestingWithALPVersionBasedOffDateNow()))
			using (RetrieverTestHelper.MockProductRegistration(DatabaseTypes.Codes.Production, isInternalSystem: false))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllStlScriptFactory.SetTemporaryFeatureCodeFilterForTest(priceCodeDetails.Select(s => s.code)))
			{
				var dataDate = ZDateTime.UtcToday.AddDays(-1);
				BillingTestHelper.StlCollectorHighWaterMarkRegistry = dataDate.AddDays(-3).ToDateTime();
				setupData(dataDate.ToString("yyyy-MM-dd"));
				TestConnection.ExecuteNonQuery("TRUNCATE TABLE StmUsageData");
				var scriptLoader = new ScriptLoader(new CollectionTimeProvider(), new LoggerForTest());
				var retriever = new StlRetrieverForTest(scriptLoader);
				retriever.CollectAndSend(CancellationToken.None);

				var billingTransactions = new List<BillingTransaction>();
				using (var cmd = TestConnection.Command($"SELECT SUD_Data FROM dbo.StmUsageData WHERE SUD_Category = 'STL' AND SUD_Code NOT IN ('STL', 'STS')"))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						billingTransactions.Add(BillingManager.DecryptTransaction(reader[0].ToString(), BillingManager.CurrentSchemaVersion));
					}
				}

				var reference1Dictionary = new SortedDictionary<string, SortedDictionary<string, List<BillingTransaction>>>();
				var priceItemCodeDictionary = new SortedDictionary<string, List<BillingTransaction>>();
				foreach (var billingTransaction in billingTransactions)
				{
					if (!reference1Dictionary.TryGetValue(billingTransaction.Reference1, out var reference1PriceItemCodeDictionary))
					{
						reference1PriceItemCodeDictionary = new SortedDictionary<string, List<BillingTransaction>>();
						reference1Dictionary.Add(billingTransaction.Reference1, reference1PriceItemCodeDictionary);
					}
					AddToPriceItemCodeDictionary(reference1PriceItemCodeDictionary, billingTransaction);
					AddToPriceItemCodeDictionary(priceItemCodeDictionary, billingTransaction);
				}

				var jobAppearsInDifferentPriceCode = new ZStringBuilder();
				foreach (var reference1Data in reference1Dictionary.Where(x => x.Value.Count > 1))
				{
					jobAppearsInDifferentPriceCode.Append($"{reference1Data.Key} ({string.Join(", ", reference1Data.Value.Keys)})");
				}
				var priceCodeDictionary = priceCodeDetails.OrderBy(x => x.code).ToDictionary(x => x.code);
				CombineAssertions(() =>
				{
					AssertMultilineASCIIEquals("Jobs should not appear in different PriceCode", "", jobAppearsInDifferentPriceCode.ToStringWithNewLineBetweenAppends());
					foreach (var priceItemCodeData in priceItemCodeDictionary)
					{
						var expectedBillableCount = 0;
						if (priceCodeDictionary.TryGetValue(priceItemCodeData.Key, out var data))
						{
							expectedBillableCount = data.expectedBillableCount;
							priceCodeDictionary.Remove(priceItemCodeData.Key);
						}
						AssertEquals(priceItemCodeData.Key, expectedBillableCount, priceItemCodeData.Value.Sum(x => x.BillableCount));
					}
					foreach (var priceCodeData in priceCodeDictionary)
					{
						Fail($"There are no data for code '{priceCodeData.Key}'");
					}
				});
			}
		}

		void AddToPriceItemCodeDictionary(SortedDictionary<string, List<BillingTransaction>> dictionary, BillingTransaction billingTransaction)
		{
			var key = billingTransaction.PriceItemCode;
			if (!dictionary.TryGetValue(key, out var list))
			{
				list = new List<BillingTransaction>();
				dictionary.Add(key, list);
			}
			list.Add(billingTransaction);
		}

		void AssertBillingTransaction(List<BillingTransaction> billingTransactions, ZString code, int count)
		{
			AssertEquals(code, count, billingTransactions.Where(x => x.PriceItemCode == code).Sum(x => x.BillableCount));
		}
	}
}
