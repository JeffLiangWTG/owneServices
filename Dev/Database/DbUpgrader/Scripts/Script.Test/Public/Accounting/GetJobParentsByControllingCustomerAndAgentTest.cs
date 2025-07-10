using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(GetJobParentsByControllingCustomerAndAgent))]
	class GetJobParentsByControllingCustomerAndAgentTest : EdwHashTest
	{
		public void TestSampleCall()
		{
			var org1 = TestDataCreator.CreateOrganisation("O1", "Org 1");
			var org2 = TestDataCreator.CreateOrganisation("O2", "Org 2");
			var org3 = TestDataCreator.CreateOrganisation("O3", "Org 3");

			var org1Address = TestDataCreator.CreateAddress(org1, "ADD1", "25 Ocean St, Alexandria, NSW, Australia");
			var org2Address = TestDataCreator.CreateAddress(org2, "ADD2", "58 Frederick St, Mascot, NSW, Australia");
			var org3Address = TestDataCreator.CreateAddress(org3, "ADD3", "55 Oxford St, Newtown, NSW, Australia");

			var shipment1 = TestDataCreator.CreateShipment("S00001");
			var shipment2 = TestDataCreator.CreateShipment("S00002");
			var shipment3 = TestDataCreator.CreateShipment("S00003");

			var scpDocAddress1 = TestDataCreator.CreateDocAddress(org1Address, "AU COMPANY", shipment1, "JS", "SCP");
			var cagDocAddress1 = TestDataCreator.CreateDocAddress(org2Address, "AU COMPANY", shipment1, "JS", "CAG");

			var cagDocAddress2 = TestDataCreator.CreateDocAddress(org3Address, "AU COMPANY", shipment2, "JS", "CAG");

			var scpDocAddress3 = TestDataCreator.CreateDocAddress(org2Address, "AU COMPANY", shipment3, "JS", "SCP");

			var sqlCmd = @"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
DECLARE @JEPk AS UNIQUEIDENTIFIER = 'e691ccaf-ae24-4471-b7c8-7bc812e54634'
DECLARE @ControllingAgentJE AS UNIQUEIDENTIFIER = '1cef7c3e-e891-4979-9d1d-37c17c0c5a73'
DECLARE @ControllingCustomerJE AS UNIQUEIDENTIFIER = 'da361802-2425-4b03-9bf1-08e73c5eb6ff'

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'DAN', 'AU company', 'AU', 'AUD')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@ControllingAgentJE, 'TESTORGADL2')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES (@ControllingCustomerJE, 'TESTORGADL3')

INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel,JE_GB, JE_GC, JE_OH_ControllingAgent, JE_OH_ControllingCustomer, JE_ClusterKey) VALUES (@JEPk, 'AU', @BranchPk, @CompanyPk, @ControllingAgentJE, @ControllingCustomerJE, 1)
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM GetJobParentsByControllingCustomerAndAgent()");
			AssertEquals("Result should have one row", 4, result.Rows.Count);
		}
	
		protected override string expectedMainDbFunctionHash => "411C541D3D57DAF7641332F661B3B3FA65F2B0C8D05AC94C8991C887D4D08FB0";
		protected override string expectedEdwDbFunctionHash => "09C6C1F72B2DDC5DD52EC17128FA74FE95513E6A36E33398A407CE7A3D9AFCB5";

		protected override string edwScriptPath => "ReportFunctions/Accounting/GetJobParentsByControllingCustomerAndAgent.sql";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new GetJobParentsByControllingCustomerAndAgent();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Accounting.GetJobParentsByControllingCustomerAndAgent();
		}
	}
}

