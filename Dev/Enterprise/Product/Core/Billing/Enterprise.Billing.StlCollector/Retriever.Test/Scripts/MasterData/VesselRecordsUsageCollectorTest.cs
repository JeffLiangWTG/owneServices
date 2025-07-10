using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(VesselRecordsUsageCollector))]
	class VesselRecordsUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = @"
		DECLARE @OrgHeaderPK UNIQUEIDENTIFIER = 'C3F842EF-3BE5-448C-BED3-0017B232C623';
		DECLARE @RefVesselPK UNIQUEIDENTIFIER = 'A3F842EF-3BE1-448C-BED3-0017B232C623';
		DECLARE @RefVesselPK1 UNIQUEIDENTIFIER = 'A3F842EF-3BE5-448C-BED3-0017B232C623';

		IF EXISTS(SELECT 1 FROM dbo.OrgHeader WHERE OH_PK = @OrgHeaderPK)
		BEGIN
			UPDATE dbo.OrgHeader SET OH_IsActive = 1, OH_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE OH_PK = @OrgHeaderPK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgHeader(OH_PK, OH_IsValid, OH_Code, OH_IsActive, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser) VALUES
			(@OrgHeaderPK, 1, 'Test_01', 1, '2024-03-29 05:29:00', 'KOO', '2024-03-29 05:29:00', 'MKO');
		END
		IF EXISTS(SELECT 1 FROM dbo.[RefVessel] WHERE RV_PK = @RefVesselPK)
		BEGIN
			UPDATE dbo.[RefVessel] SET RV_IsActive = 0, RV_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE RV_PK = @RefVesselPK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.[RefVessel](RV_PK, RV_IsActive, RV_Code, RV_LloydsNumber, RV_OH, RV_SystemLastEditTimeUtc, RV_SystemCreateTimeUtc) VALUES
			(@RefVesselPK, 0, 'Test03', 1234567, @OrgHeaderPK, '2024-03-29 05:29:00', '2024-03-29 05:29:00');
		END
		IF EXISTS(SELECT 1 FROM dbo.[RefVessel] WHERE RV_PK = @RefVesselPK1)
		BEGIN
			UPDATE dbo.[RefVessel] SET RV_IsActive = 1, RV_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE RV_PK = @RefVesselPK1;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.[RefVessel](RV_PK, RV_IsActive, RV_Code, RV_LloydsNumber, RV_OH, RV_SystemLastEditTimeUtc, RV_SystemCreateTimeUtc) VALUES
			(@RefVesselPK1, 1, 'Test02', 1234567, @OrgHeaderPK, '2024-03-29 05:29:00', '2024-03-29 05:29:00');
		END
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var vesselActiveRecords = TestConnection.ExecuteScalar<int>("SELECT COUNT(RV_PK) FROM dbo.RefVessel WHERE RV_IsActive = 1");
			var vesselInactiveRecords = TestConnection.ExecuteScalar<int>("SELECT COUNT(RV_PK) FROM dbo.RefVessel WHERE RV_IsActive = 0");
			var vesselIdCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM RefVessel WHERE RV_LloydsNumber != '' AND RV_IsActive = 1");
			var vesselNameCount = TestConnection.ExecuteScalar<int>("Select COUNT(*) AS VesselNameCount FROM RefVessel WHERE RV_IsActive = 1");

			var expectedAdditionalRefs = "{\"VesselNameCount\":" + vesselNameCount + ",\"VesselIdCount\":" + vesselIdCount + ",\"VesselActiveRecords\":" + vesselActiveRecords + ",\"VesselInactiveRecords\":" + vesselInactiveRecords + "}";

			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertNotNull(transaction1.Reference1);
			AssertEquals("Additional Reference value", expectedAdditionalRefs, transaction1.AdditionalRefs);
		}
	}
}

