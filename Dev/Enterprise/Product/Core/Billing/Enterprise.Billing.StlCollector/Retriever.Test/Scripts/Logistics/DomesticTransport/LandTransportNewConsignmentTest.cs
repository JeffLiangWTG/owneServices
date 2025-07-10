using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportNewConsignment))]
	sealed class LandTransportNewConsignmentTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override bool IsMandatoryForMilestones => true;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'
DECLARE @oh1PK UNIQUEIDENTIFIER = '04451e63-733f-48bb-b32b-79585f63b3ae'
DECLARE @oh2PK UNIQUEIDENTIFIER = '46708197-3541-41b5-9772-eacf8b6d19c1'
DECLARE @oh3PK UNIQUEIDENTIFIER = '336b5392-51e3-41ac-8847-ab9479e8282f'

DECLARE @cn1PK UNIQUEIDENTIFIER = '6b581b3f-1b20-4f77-aa2f-8dae1b466614'
DECLARE @cn2PK UNIQUEIDENTIFIER = 'ca9779ec-fe67-4508-8d54-fb99934668cb'
DECLARE @cnAddr1PK UNIQUEIDENTIFIER = '8d3ffe41-a97f-4d48-aa22-e38f416b4826'
DECLARE @cnAddr2PK UNIQUEIDENTIFIER = 'aa6fa00b-44dc-472d-b02a-e0204981fad6'
DECLARE @cnAddr3PK UNIQUEIDENTIFIER = 'fac7c39e-dbfc-4f82-9c74-dcae3955d28a'
DECLARE @cnAddr4PK UNIQUEIDENTIFIER = 'e578aa9a-5012-44a9-8ed5-da159e44af94'
DECLARE @cnAddr5PK UNIQUEIDENTIFIER = '6427f8a4-6407-45e5-b1a6-f62281896af4'
DECLARE @cnAddr6PK UNIQUEIDENTIFIER = 'bc84cea4-34f8-4046-bf37-7a551dea256a'
DECLARE @cnAddr7PK UNIQUEIDENTIFIER = 'fc0fe3fd-190b-46f7-a261-5d292fc9a4a6'

DECLARE @oaAddr1PK UNIQUEIDENTIFIER = '1e0c1026-7901-46d0-ae71-70438ddd1474'
DECLARE @oaAddr2PK UNIQUEIDENTIFIER = '45e085a2-84e8-47cc-8719-5f353cf63808'
DECLARE @oaAddr3PK UNIQUEIDENTIFIER = 'b6ecd480-7d6d-439a-8eb6-f1d1d724ca58'

DECLARE @jobDocAddr1PK UNIQUEIDENTIFIER = 'b5ca7936-af96-4d2c-998d-1e86af415ada'
DECLARE @jobDocAddr2PK UNIQUEIDENTIFIER = 'e295c210-dfe2-4497-a360-a83e52348b84'
DECLARE @jobDocAddr3PK UNIQUEIDENTIFIER = '0f296d0e-4554-4916-985d-3457fc25191b'
DECLARE @jobDocAddr4PK UNIQUEIDENTIFIER = '83c2b6d7-c2dd-4975-a10d-413761624fe1'
DECLARE @jobDocAddr5PK UNIQUEIDENTIFIER = '65f93b69-73f4-4acb-9fbd-bcba1b77ff75'
DECLARE @jobDocAddr6PK UNIQUEIDENTIFIER = 'e3bf147f-6587-4dbd-846e-5a1384c2e845'
DECLARE @jobDocAddr7PK UNIQUEIDENTIFIER = '051ced72-d6b7-45fa-8285-238285db2a4a'

INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES
(@oh1PK, 'ARORG1'),
(@oh2PK, 'ARORG2'),
(@oh3PK, 'ARORG3');

INSERT INTO dbo.DtbConsignment(LTC_PK,LTC_JobID,LTC_ConsignmentType,LTC_ConnoteNumber,LTC_JobType,LTC_Direction,LTC_GB_Branch,LTC_SystemCreateTimeUtc,LTC_SystemCreateUser,LTC_SystemLastEditTimeUtc,LTC_SystemLastEditUser) VALUES 
(@cn1PK, 'CN0001', 'LTC', 'CNNote001','LTL','LOC',@SYDBranchPk,'2022-08-11 02:23:00','TST',GETUTCDATE(),'TST'),
(@cn2PK, 'CN0002', 'LTC', 'CNNote002','LTL','LOC',@SYDBranchPk,'2022-08-11 02:33:00','TST',GETUTCDATE(),'TST');

INSERT INTO dbo.DtbConsignmentAddress(LTS_PK, LTS_LTC_Consignment,LTS_InstructionType,LTS_Sequence,LTS_Status,LTS_SystemCreateTimeUtc,LTS_SystemCreateUser,LTS_SystemLastEditTimeUtc,LTS_SystemLastEditUser) VALUES
(@cnAddr1PK, @cn1PK, 'PIC',1,'INC',GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@cnAddr7PK, @cn1PK, 'MLT',2,'INC',GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@cnAddr2PK, @cn1PK, 'DLV',3,'INC',GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@cnAddr3PK, @cn2PK, 'PIC',1,'INC','2022-08-10 01:23:00','TST','2022-08-10 01:23:00','TST'),
(@cnAddr4PK, @cn2PK, 'PIC',2,'INC','2022-08-11 01:23:00','TST','2022-08-11 01:23:00','TST'),
(@cnAddr5PK, @cn2PK, 'DLV',3,'INC','2022-08-11 01:23:00','TST','2022-08-11 01:23:00','TST'),
(@cnAddr6PK, @cn2PK, 'DLV',4,'INC','2022-08-12 01:23:00','TST','2022-08-12 01:23:00','TST');

INSERT INTO dbo.OrgAddress(OA_PK, OA_Address1, OA_City, OA_State, OA_RN_NKCountryCode, OA_GeoLocation,OA_OH, OA_SystemCreateTimeUtc,OA_SystemCreateUser,OA_SystemLastEditTimeUtc,OA_SystemLastEditUser) VALUES
(@oaAddr1PK,'test address 1', 'DINGLEY','VIC','AU',geography::Point(47.651,-122.362, 4326),@oh1PK,GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@oaAddr2PK,'test address 2', 'MELBOURNE','VIC','AU',geography::Point(47.652, -122.363, 4326),@oh2PK,GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@oaAddr3PK,'test address 3', 'MOUNT GAMBIER','SA','AU',geography::Point(47.653, -122.364, 4326),@oh3PK,GETUTCDATE(),'TST',GETUTCDATE(),'TST');

INSERT INTO dbo.jobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_City, E2_State, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address,E2_AddressOverride,E2_ValidationStatus, E2_SystemCreateTimeUtc,E2_SystemCreateUser,E2_SystemLastEditTimeUtc,E2_SystemLastEditUser) VALUES
(@jobDocAddr1PK, @cnAddr1PK, 'LTS','ALEXANDRIA','NSW','AU',geography::Point(47.616, -122.360, 4326), NULL, 0,'NRQ', GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@jobDocAddr2PK, @cnAddr2PK, 'LTS','DUTTON PARK','QLD','AU',geography::Point(47.626, -122.360, 4326), @oaAddr1PK, 1, 'NYV',GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@jobDocAddr3PK, @cnAddr3PK, 'LTS','','','',geography::Point(47.636, -122.360, 4326), @oaAddr2PK, 0, 'NRQ',GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@jobDocAddr4PK, @cnAddr4PK, 'LTS','ADELAIDE','SA','AU',geography::Point(47.646, -122.360, 4326),NULL, 0,'NRQ', GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@jobDocAddr5PK, @cnAddr5PK, 'LTS','UNLEY','SA','AU',geography::Point(47.656, -122.360, 4326),NULL, 0, 'NRQ',GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@jobDocAddr6PK, @cnAddr6PK, 'LTS','STAWELL','VIC','AU',geography::Point(47.656, -122.360, 4326), @oaAddr3PK, 1, 'NYV', GETUTCDATE(),'TST',GETUTCDATE(),'TST'),
(@jobDocAddr7PK, @cnAddr7PK, 'LTS','Marden','SA','AU',geography::Point(47.756, -122.360, 4326), NULL, 1, 'NYV', GETUTCDATE(),'TST',GETUTCDATE(),'TST');
			";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Consignment Usage Statistics Records", 2, transactions.Count());
			AssertRowResult(transactions, "EDI", "SYD", new DateTime(2022, 8, 11, 2, 23, 0), "6b581b3f-1b20-4f77-aa2f-8dae1b466614", "LTL", "CNNote001", "CN0001", "TST", "{\"PCity\":\"ALEXANDRIA\",\"PState\":\"NSW\",\"PCountry\":\"AU\",\"PGeoloc\":\"POINT (-122.36 47.616)\",\"DCity\":\"DUTTON PARK\",\"DState\":\"QLD\",\"DCountry\":\"AU\",\"DGeoloc\":\"POINT (-122.36 47.626)\"}");
			AssertRowResult(transactions, "EDI", "SYD", new DateTime(2022, 8, 11, 2, 33, 0), "ca9779ec-fe67-4508-8d54-fb99934668cb", "LTL", "CNNote002", "CN0002", "TST", "{\"PCity\":\"MELBOURNE\",\"PState\":\"VIC\",\"PCountry\":\"AU\",\"PGeoloc\":\"POINT (-122.363 47.652)\",\"DCity\":\"STAWELL\",\"DState\":\"VIC\",\"DCountry\":\"AU\",\"DGeoloc\":\"POINT (-122.36 47.656)\"}");
		}

		void AssertRowResult(IEnumerable<IStlTransaction> transactions, string companyCode, string branchCode, DateTime transactionDate, string referenceGuid, string reference1, string reference2, string reference3, string userCode, string additonalRefs)
		{
			var transaction = transactions.Single(t => t.Reference5.ToLower() == referenceGuid.ToLower());
			AssertEquals($"Company Code should be {companyCode}", companyCode, transaction.GetCompanyCode());
			AssertEquals($"Branch Code should be {branchCode}", branchCode, transaction.GetBranchCode());
			AssertEquals($"TransactionDateUtc should be {transactionDate:dd/MM/yyyy HH:mm:ss}", transactionDate, transaction.ServiceOccuredUTC);
			AssertEquals($"Billing Reference 1 should be {reference1}", reference1, transaction.Reference3);
			AssertEquals($"Billing Reference 2 should be {reference2}", reference2, transaction.Reference2);
			AssertEquals($"Billing Reference 3 should be {reference3}", reference3, transaction.Reference1);
			AssertEquals($"Creating User Code should be {userCode}", userCode, transaction.ClientStaffCode);
			AssertEquals("AdditionalRefs should be as expected", additonalRefs, transaction.AdditionalRefs);
		}
	}
}
