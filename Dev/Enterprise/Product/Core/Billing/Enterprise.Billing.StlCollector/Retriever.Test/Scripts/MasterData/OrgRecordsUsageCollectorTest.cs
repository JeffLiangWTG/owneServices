using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(OrgRecordsUsageCollector))]
	class OrgRecordsUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = @"
		DECLARE @OrgHeaderPK UNIQUEIDENTIFIER = 'C3F842EF-3BE5-448C-BED3-0017B232C623';
		DECLARE @OrgBrandOrRelatedPK UNIQUEIDENTIFIER = 'C3F842EF-3BE5-448C-BED3-0017B232C633';
		DECLARE @OrgAddressPK UNIQUEIDENTIFIER = 'C3F842EF-3BE5-448C-BED3-0017B232C643';
		DECLARE @OrgAddressPK1 UNIQUEIDENTIFIER = 'A3F842EF-3BE5-448C-BED3-0017B232C643';
		DECLARE @OrgCusCodePK UNIQUEIDENTIFIER = 'C3F842EF-3BE5-448C-BED3-0017B232C683';

		DISABLE TRIGGER ALL ON dbo.OrgBrandOrRelatedName;

		IF EXISTS(SELECT 1 FROM dbo.OrgHeader WHERE OH_Code = 'Test_02')
		BEGIN
			UPDATE dbo.OrgHeader SET OH_IsActive = 0, OH_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE OH_Code = 'Test_02';
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgHeader(OH_PK, OH_IsValid, OH_Code, OH_IsActive, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser) VALUES
			(NEWID(), 1, 'Test_02', 1, '2024-03-29 05:29:00', 'KOO', '2024-03-29 05:29:00', 'MKO');
		END
		IF EXISTS(SELECT 1 FROM dbo.OrgHeader WHERE OH_PK = @OrgHeaderPK)
		BEGIN
			UPDATE dbo.OrgHeader SET OH_IsActive = 1, OH_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE OH_PK = @OrgHeaderPK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgHeader(OH_PK, OH_IsValid, OH_Code, OH_IsActive, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser, OH_SystemCreateTimeUtc, OH_SystemCreateUser) VALUES
			(@OrgHeaderPK, 1, 'Test_01', 1, '2024-03-29 05:29:00', 'KOO', '2024-03-29 05:29:00', 'MKO');
		END
		IF EXISTS(SELECT 1 FROM dbo.OrgBrandOrRelatedName WHERE P1_PK = @OrgBrandOrRelatedPK)
		BEGIN
			UPDATE dbo.OrgBrandOrRelatedName SET P1_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE P1_PK = @OrgBrandOrRelatedPK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgBrandOrRelatedName(P1_PK, P1_IsValid, P1_RelatedName, P1_OH, P1_SystemLastEditTimeUtc, P1_SystemCreateTimeUtc) VALUES
			(@OrgBrandOrRelatedPK, 1, 'Brand', @OrgHeaderPK, '2024-03-29 05:29:00', '2024-03-29 05:29:00');
		END;

		ENABLE TRIGGER ALL ON dbo.OrgBrandOrRelatedName;

		IF EXISTS(SELECT 1 FROM dbo.OrgAddress WHERE OA_PK = @OrgAddressPK)
		BEGIN
			UPDATE dbo.OrgAddress SET OA_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE OA_PK = @OrgAddressPK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgAddress(OA_PK, OA_IsActive, OA_Code, OA_CompanyNameOverride, OA_Address1, OA_OH, OA_SystemLastEditTimeUtc, OA_SystemCreateTimeUtc) VALUES
			(@OrgAddressPK, 1, 'Semi', 'Test', 'Sharon', @OrgHeaderPK, '2024-03-29 05:29:00', '2024-03-29 05:29:00');
		END
		IF EXISTS(SELECT 1 FROM dbo.OrgAddress WHERE OA_PK = @OrgAddressPK1)
		BEGIN
			UPDATE dbo.OrgAddress SET OA_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE OA_PK = @OrgAddressPK1;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgAddress(OA_PK, OA_IsActive, OA_Code, OA_CompanyNameOverride, OA_Address1, OA_OH, OA_SystemLastEditTimeUtc, OA_SystemCreateTimeUtc) VALUES
			(@OrgAddressPK1, 0, 'Femi', 'Telt', 'Sharnn', @OrgHeaderPK, '2024-03-29 05:29:00', '2024-03-29 05:29:00');
		END
		IF EXISTS(SELECT 1 FROM dbo.OrgCusCode WHERE OK_PK = @OrgCusCodePK)
		BEGIN
			UPDATE dbo.OrgCusCode SET OK_SystemLastEditTimeUtc = '2024-03-29 05:29:00' WHERE OK_PK = @OrgCusCodePK;
		END
		ELSE
		BEGIN
			INSERT INTO dbo.OrgCusCode(OK_PK, OK_CodeType, OK_OA_PremisesAddress, OK_RN_NKCodeCountry, OK_OH, OK_SystemLastEditTimeUtc, OK_SystemCreateTimeUtc) VALUES
			(@OrgCusCodePK, 'DUN', @OrgAddressPK, 'US', @OrgHeaderPK, '2024-03-29 05:29:00', '2024-03-29 05:29:00');
		END
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var orgActiveRecords = TestConnection.ExecuteScalar<int>("SELECT COUNT(OH_PK) FROM dbo.OrgHeader WHERE OH_IsActive = 1 AND OH_Code != 'DEMORG'");
			var orgInactiveRecords = TestConnection.ExecuteScalar<int>("SELECT COUNT(OH_PK) FROM dbo.OrgHeader WHERE OH_IsActive = 0 AND OH_Code != 'DEMORG'");
			var orgAddressCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM OrgHeader oh JOIN OrgAddress oa ON oh.OH_PK = oa.OA_OH WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG' AND OA_IsActive = 1");
			var orgIdCount = TestConnection.ExecuteScalar<int>("SELECT COUNT(*) FROM OrgCusCode oc JOIN OrgHeader oh ON oh.OH_PK = oc.OK_OH WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG' AND (oc.OK_CodeType = 'DUN' OR oc.OK_CodeType = 'PAS')");
			var orgNameCount = TestConnection.ExecuteScalar<int>(@"SELECT 
				(SELECT COUNT(*)
				 FROM OrgHeader oh 
				 JOIN OrgBrandOrRelatedName ob ON oh.OH_PK = ob.P1_OH
				 WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG') +
				(SELECT COUNT(*)
				 FROM OrgHeader oh 
				 JOIN OrgAddress oa ON oh.OH_PK = oa.OA_OH
				 WHERE oh.OH_IsActive = 1 AND oh.OH_Code != 'DEMORG' AND OA_CompanyNameOverride != '' AND OA_IsActive = 1) +
				(SELECT COUNT(*)
				 FROM OrgHeader
				 WHERE OH_IsActive = 1 AND OH_Code != 'DEMORG')");

			var expectedAdditionalRefs = "{\"OrgNameCount\":" + orgNameCount + ",\"OrgAddressCount\":" + orgAddressCount + ",\"OrgIdCount\":" + orgIdCount + ",\"OrgActiveRecords\":" + orgActiveRecords + ",\"OrgInactiveRecords\":" + orgInactiveRecords + "}";

			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertNotNull(transaction1.Reference1);
			AssertEquals("Additional Reference value", expectedAdditionalRefs, transaction1.AdditionalRefs);
		}
	}
}

