using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Billing.Collectors;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts
{
	[TestedType(typeof(RefStlScriptRetriever))]
	sealed class RefStlScriptRetrieverTest : StlScriptTest
	{
		public void TestCodeAndNameFields()
		{
			var bizo = Factory.New<RefStlScript>();
			bizo.STL_FeatureCode = "TST";
			bizo.STL_FeatureName = "TestFeatureName";
			bizo.STL_FunctionName = "TestFunctionName";
			bizo.STL_ModuleName = "TestModuleName";
			bizo.STL_RoleName = "TestRoleName";
			bizo.STL_ActiveOn = "TST";
			bizo.STL_MinCW1Version = "20.1.23.0";
			bizo.STL_MaxCW1Version = "22.5.7.6";

			var script = (IStlScript)new RefStlScriptRetriever(bizo);
			AssertEquals("Wrong code", "TST", script.Code);
			AssertEquals("Wrong feature", "TestFeatureName", script.Feature);
			AssertEquals("Wrong function", "TestFunctionName", script.Function);
			AssertEquals("Wrong module", "TestModuleName", script.Module);
			AssertEquals("Wrong role", "TestRoleName", script.Role);
			AssertEquals("Wrong ActiveOn", "TST", script.ActiveOn);
			AssertEquals("Wrong MinCW1Version", "20.1.23.0", script.MinCW1Version);
			AssertEquals("Wrong MaxCW1Version", "22.5.7.6", script.MaxCW1Version);
		}

		public void TestIsActiveReportsExceptionsAndReturnsFalse()
		{
			var bizo = Factory.New<RefStlScript>();
			bizo.STL_FeatureCode = "TST";
			bizo.STL_FeatureName = "TestFeatureName";
			bizo.STL_FunctionName = "TestFunctionName";
			bizo.STL_ModuleName = "TestModuleName";
			bizo.STL_RoleName = "TestRoleName";
			bizo.STL_MinCW1Version = "A";

			var script = (IStlScript)new RefStlScriptRetriever(bizo);
			AssertEquals("Wrong code", "TST", script.Code);
			AssertEquals("Wrong feature", "TestFeatureName", script.Feature);
			AssertEquals("Wrong function", "TestFunctionName", script.Function);
			AssertEquals("Wrong module", "TestModuleName", script.Module);
			AssertEquals("Wrong role", "TestRoleName", script.Role);
			AssertEquals("Is Active is false", false, script.IsActive);
			AssertEquals("Error was reported as developer exception", 1, ExceptionReporterTestListener.Instance.Count);
			AssertType<FormatException>(ExceptionReporterTestListener.Instance[0]);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public override void TestCollectorType()
		{
			var item = ScriptToTest;
			AssertEquals("Incorrect value for property CollectorType", StlCollectorType.Dynamic, item.CollectorType);
		}

		public override void TestScriptTextContainsComment()
		{
			var bizo = Factory.New<RefStlScript>();
			bizo.STL_FeatureCode = "TST";
			bizo.STL_FeatureName = "TestFeatureName";
			bizo.STL_FunctionName = "TestFunctionName";
			bizo.STL_ModuleName = "TestModuleName";
			bizo.STL_RoleName = "TestRoleName";

			var script = (IStlScript)new RefStlScriptRetriever(bizo);
			Assert("Comment with collector details not found in script", script.ScriptText.Contains($"-- STL Collector query for FeatureCode=TST, CollectorType=Dynamic"));
		}

		public void TestIsCollectionActiveFollowsActiveOn()
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationProdMock = new Mock<IProductRegistration>();
			productRegistrationProdMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationProdMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
			prodKeyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);

			using (ObjectFactory.Substitute(productRegistrationProdMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL"));
				AssertEquals("Wrong IsActive for ALL Prod", true, retriever.IsCollectionActive);

				retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "NON"));
				AssertEquals("Wrong IsActive for NON Prod", false, retriever.IsCollectionActive);

				retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "PRD"));
				AssertEquals("Wrong IsActive for PRD Prod", true, retriever.IsCollectionActive);

				retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "TST"));
				AssertEquals("Wrong IsActive for TST Prod", false, retriever.IsCollectionActive);
			}

			var testKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationTestMock = new Mock<IProductRegistration>();
			productRegistrationTestMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(false);
			productRegistrationTestMock.Setup(m => m.Key).Returns(testKeyMock.Object);
			testKeyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Test);

			using (ObjectFactory.Substitute(productRegistrationTestMock.Object))
			using (Globals.TemporaryOverrideForIsTest(false))
			using (Globals.TemporaryOverrideForIsDebugMode(false))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL"));
				AssertEquals("Wrong IsActive for ALL Test", true, retriever.IsCollectionActive);

				retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "NON"));
				AssertEquals("Wrong IsActive for NON Test", false, retriever.IsCollectionActive);

				retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "PRD"));
				AssertEquals("Wrong IsActive for PRD Test", false, retriever.IsCollectionActive);

				retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "TST"));
				AssertEquals("Wrong IsActive for TST Test", true, retriever.IsCollectionActive);
			}
		}

		public void TestIsActiveFollowsMinMaxCW1Version()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("13.7.4.33", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: "14.5.23.333", maxVersion: "20.3.2.785"));
				AssertEquals("Wrong IsActive for below minimum", false, retriever.IsCollectionActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("22.1.6.444", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: "14.5.23.333", maxVersion: "20.3.2.785"));
				AssertEquals("Wrong IsActive for Above maximum", false, retriever.IsCollectionActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("16.4.44.777", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: "14.5.23.333", maxVersion: "20.3.2.785"));
				AssertEquals("Wrong IsActive for within range", true, retriever.IsCollectionActive);
			}
		}

		public void TestIsActiveOnlyMinCW1VersionProvided()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("13.7.4.33", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: "14.5.23.333", maxVersion: string.Empty));
				AssertEquals("Wrong IsActive for below minimum", false, retriever.IsCollectionActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("16.4.44.777", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: "14.5.23.333", maxVersion: string.Empty));
				AssertEquals("Wrong IsActive for above minimum", true, retriever.IsCollectionActive);
			}
		}

		public void TestIsActiveOnlyMaxCW1VersionProvided()
		{
			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("13.7.4.33", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: string.Empty, maxVersion: "20.3.2.785"));
				AssertEquals("Wrong IsActive for below maximum", true, retriever.IsCollectionActive);
			}

			using (ReleaseInfo.SetTemporaryInstanceForTesting(ReleaseInfo.CreateNewInstanceForTesting("22.1.6.444", DateTime.Now, "GP1")))
			{
				var retriever = new RefStlScriptRetriever(CreateScriptBizo(activeOn: "ALL", minVersion: string.Empty, maxVersion: "20.3.2.785"));
				AssertEquals("Wrong IsActive for Above maximum", false, retriever.IsCollectionActive);
			}
		}

		public void TestStlGrain()
		{
			var retriever1 = new RefStlScriptRetriever(CreateScriptBizo(dataGranularity: "TRN"));
			AssertEquals("Wrong StlGrain for TRN", StlDataGrain.Transactional, ((IStlItem)retriever1).StlGrain);

			var retriever2 = new RefStlScriptRetriever(CreateScriptBizo(dataGranularity: "MAH"));
			AssertEquals("Wrong StlGrain for MAH", StlDataGrain.MonthlyAllowHistoricalData, ((IStlItem)retriever2).StlGrain);

			var retriever3 = new RefStlScriptRetriever(CreateScriptBizo(dataGranularity: "MCO"));
			AssertEquals("Wrong StlGrain for MCO", StlDataGrain.MonthlyCurrentDataOnly, ((IStlItem)retriever3).StlGrain);
		}

		public void TestName()
		{
			var bizo = new DummyRefStlScript();
			var retriever = new RefStlScriptRetriever(bizo);
			AssertEquals("Name should return bizo type name", bizo.GetType().Name, retriever.Name);
		}

		IRefStlScript CreateScriptBizo(string activeOn = "ALL", string minVersion = "", string maxVersion = "", string dataGranularity = "TRN")
		{
			var mockScript = new Mock<IRefStlScript>();
			mockScript.Setup(m => m.ActiveOn).Returns(activeOn);
			mockScript.Setup(m => m.MinCW1Version).Returns(minVersion);
			mockScript.Setup(m => m.MaxCW1Version).Returns(maxVersion);
			mockScript.Setup(m => m.DataGranularity).Returns(dataGranularity);
			return mockScript.Object;
		}

		#region override

		protected override bool IsMandatoryForMilestones => false;

		protected override sealed IDateTimeRange TestDateTimeRange
		{
			get
			{
				var startDate = new DateTime(2021, 7, 1);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = transactions.Single(t => t.Reference5 == "00000001-0000-0000-0000-000000000000");
			AssertEquals("[T1] CompanyCode", "WTG", transaction1.GetCompanyCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2021, 7, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] TransactionReference01", CollectedAsUsageTransaction ? null : "[BLANK]", transaction1.Reference1);
			AssertEquals("[T1] TransactionReference02", null, transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", null, transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", null, transaction1.Reference4);
			AssertEquals("[T1] AdditionalRefs", "{\"UsageId\": \"Usage1\"}", transaction1.AdditionalRefs);
			AssertEquals("[T1] BranchCode", "CBR", transaction1.GetBranchCode());
			AssertEquals("[T1] UserCode", "DAT", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);

			var transaction2 = transactions.Single(t => t.Reference5 == "00000002-0000-0000-0000-000000000000");
			AssertEquals("[T2] CompanyCode", "WTG", transaction2.GetCompanyCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2021, 7, 1, 23, 58, 30), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] TransactionReference01", CollectedAsUsageTransaction ? null : "[BLANK]", transaction2.Reference1);
			AssertEquals("[T2] TransactionReference02", null, transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", null, transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", null, transaction2.Reference4);
			AssertEquals("[T2] AdditionalRefs", "{\"UsageId\": \"Usage2\"}", transaction2.AdditionalRefs);
			AssertEquals("[T2] BranchCode", "CBR", transaction2.GetBranchCode());
			AssertEquals("[T2] UserCode", "DAT", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
		}

		protected override void PrepareTestData()
		{
			const string sqlText = @"
--Insert a company whose country code is not GB
INSERT dbo.GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_Code, GC_Name) VALUES (0x1, 'AU', 'WTG', 'AU company')
INSERT dbo.GlbBranch (GB_PK, GB_GC, GB_Code) VALUES (0x1, 0x1, 'CBR')
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (0x1, 'DEP')

INSERT dbo.EDIMessage (EM_PK, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemLastEditTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_Status, EM_SystemCreateUser, EM_SystemLastEditUser, EM_IsActive, EM_MessageData) VALUES
	(0x1, 0x1, 0x1, '2021-07-01 00:00:00', '2021-07-01 00:00:00', 'TRX', 'USG', 'USG', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage1""}')), -- all valid data, minimum allowed EM_SystemCreateTimeUtc
	(0x2, 0x1, 0x1, '2021-07-01 23:58:30', '2021-07-01 23:58:30', 'TRX', 'USG', 'USG', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage2""}')), -- all valid data, maximum allowed EM_SystemCreateTimeUtc
	(0x3, 0x1, 0x1, '2021-07-01 23:58:30', '2021-07-01 23:58:30', 'TRX', 'USG', 'USG', 'CAP', 'DAT', 'DAT', 0, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage3""}')), -- inactive
	(0x4, 0x1, 0x1, '2021-07-01 23:58:30', '2021-07-01 23:58:30', 'TRX', 'USG', 'UNK', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage4""}')), -- different application code
	(0x5, 0x1, 0x1, '2021-07-01 23:58:30', '2021-07-01 23:58:30', 'TRX', 'UNK', 'USG', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage5""}')), -- different message type
	(0x6, 0x1, 0x1, '2021-07-01 23:58:30', '2021-07-01 23:58:30', 'TRX', 'USG', 'USG', 'SNT', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage6""}')), -- different status
	(0x7, 0x1, 0x1, '2021-07-01 23:58:30', '2021-07-01 23:58:30', 'RCV', 'USG', 'USG', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage7""}')), -- receive
	(0x8, 0x1, 0x1, '2021-06-30 23:58:30', '2021-06-30 23:58:30', 'TRX', 'USG', 'USG', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage8""}')), -- before range
	(0x9, 0x1, 0x1, '2021-07-02 00:00:00', '2021-07-02 00:00:00', 'TRX', 'USG', 'USG', 'CAP', 'DAT', 'DAT', 1, CONVERT(VARBINARY(max),'{""UsageId"": ""Usage9""}')) -- after range";
			TestConnection.Command(sqlText).ExecuteNonQuery();
		}

		protected override IStlScript ScriptToTest
		{
			get
			{
				var bizo = Factory.New<RefStlScript>();

				bizo.STL_UsedInBilling = ZBool.False;
				bizo.STL_FeatureCode = "USG";
				bizo.STL_RoleName = "eServices";
				bizo.STL_ModuleName = "Usage Data";
				bizo.STL_FunctionName = "Usage Data Messages";
				bizo.STL_FeatureName = "Usage Data Messages";
				bizo.STL_CompanyCode = "gc.GC_Code";
				bizo.STL_BranchCode = "gb.GB_Code";
				bizo.STL_TransactionDateUtc = "em.EM_SystemCreateTimeUtc";
				bizo.STL_GuidReference = "em.EM_PK";
				bizo.STL_AdditionalRefs = "em.EM_MessageData";
				bizo.STL_CreatingUserCode = "em.EM_SystemCreateUser";
				bizo.STL_ActiveOn = "ALL";
				bizo.STL_FromClause = @"
							EDIMessage em
							INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = em.EM_GB
							INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
				bizo.STL_WhereClause = @"
							(
								(em.EM_ApplicationCode = 'USG')
								AND (em.EM_ReceiveTransmit = 'TRX')
								AND (em.EM_IsActive = 1)
								AND (em.EM_Status = 'CAP')
								AND (em.EM_MessageType = 'USG')
							)";
				return new RefStlScriptRetriever(bizo);
			}
		}
		#endregion
	}
}
