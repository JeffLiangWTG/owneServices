using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(EManifestSentFromShipments))]
	sealed class EManifestSentFromShipmentsTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(2, transactions.Count());

			AssertNull("ISN event is before DateTime range", transactions.FirstOrDefault(x => x.Reference3 == "sld003"));
			AssertNull("ISN event is after DateTime range", transactions.FirstOrDefault(x => x.Reference3 == "sld004"));
			AssertNull("This is XX event", transactions.FirstOrDefault(x => x.Reference3 == "sld005"));
			AssertNull("SL_Table is not JobDocumentData", transactions.FirstOrDefault(x => x.Reference3 == "sld006"));
			AssertNull("DEP is not CargoWise", transactions.FirstOrDefault(x => x.Reference3 == "sld007"));

			var row0 = transactions.Single(x => x.Reference3 == "sld001");
			var row1 = transactions.Single(x => x.Reference3 == "sld002");

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 11, 2, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JsUniqueConsignRef01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "C1H1", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "sld001", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "CNNBO - eManifest[ORG]", row0.Reference4);
				AssertEquals("[Row-0] AdditionalRefs should be as expected",
					"{\"EventReference\":\"|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNNBO|RFN=sld001\"," +
					"\"ShipmentType\":\"STD\"," +
					"\"TransportMode\":\"SEA\"," +
					"\"ContainerMode\":\"FCL\"," +
					"\"Origin\":\"CNSAA\"," +
					"\"Destination\":\"AUS2E\"}"
					, row0.AdditionalRefs);

				AssertEquals("[Row-1] CompanyCode", "SZG", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB3", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 11, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JsUniqueConsignRef02", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "C2H2", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "sld002", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "CNNBO - eManifest[ORG]", row1.Reference4);
				AssertEquals("[Row-2] AdditionalRefs should be as expected",
					"{\"EventReference\":\"|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNNBO|RFN=sld002\"," +
					"\"ShipmentType\":\"CLD\"," +
					"\"TransportMode\":\"SEA\"," +
					"\"ContainerMode\":\"LCL\"," +
					"\"Origin\":\"CNSAC\"," +
					"\"Destination\":\"AUS2M\"}"
					, row1.AdditionalRefs);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @JddPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk07 UNIQUEIDENTIFIER = newid();

DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();

DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'SHA', 'CN company1', 'CNY', 'CN');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk03, 'SZG', 'CN company2', 'CNY', 'CN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk00, 'GB0', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk02, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk03, 'GB3', @GcPk03, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobDocumentData', getdate(), '2022-11-02 12:01:00', @JddPk01, 'ISN', '|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNNBO|RFN=sld001', 'GB0', 'N'), -- 01
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 12:02:00', @JddPk02, 'ISN', '|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNNBO|RFN=sld002', 'GB0', 'N'),  -- 02
	(newid(), 'JobDocumentData', getdate(), '2022-10-01 12:03:00', @JddPk03, 'ISN', '|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNSHA|RFN=sld003', 'GB0', 'N'), -- Before date range
	(newid(), 'JobDocumentData', getdate(), '2022-12-01 12:04:00', @JddPk04, 'ISN', '|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNSHA|RFN=sld004', 'GB0', 'N'), -- After date range
	(newid(), 'JobDocumentData', getdate(), '2022-11-01 12:05:00', @JddPk05, 'XXX', '|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNSHA|RFN=sld005', 'GB0', 'N'), -- SL_SE_NKEvent is not ISN
	(newid(), 'JobShipment', getdate(), '2022-11-12 12:06:00', @JddPk06, 'ISN', '|DEP=CargoWise|MST=eManifest|STA=ORG|LOC=CNNBO|RFN=sld006', 'GB0', 'N'),  -- SL_Table is not JobDocumentData
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 12:06:00', @JddPk07, 'ISN', '|DEP=CargoSmart|MST=eManifest|STA=ORG|LOC=CNNBO|RFN=sld007', 'GB0', 'N'),  -- DEP is not CargoWise

	(newid(), 'JobDocumentData', getdate(), '2022-11-01 10:00:00', @JddPk01, 'MSN', '|MST=eManifest', 'GB1', 'N'), -- 01 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-11-10 10:00:00', @JddPk01, 'MWR', '|MST=eManifest', 'GB2', 'N'), -- 01 MWR
	(newid(), 'JobDocumentData', getdate(), '2022-11-12 10:00:00', @JddPk02, 'MSN', '|MST=eManifest', 'GB3', 'N'), -- 02 MSN
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk02, 'MSN', '|MST=eManifest', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk03, 'MSN', '|MST=eManifest', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk04, 'MSN', '|MST=eManifest', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-10 10:00:00', @JddPk05, 'MSN', '|MST=eManifest', 'GB2', 'N');

INSERT dbo.JobDocumentData (JDD_PK, JDD_ParentID, JDD_ParentTableCode, JDD_SystemCreateTimeUtc, JDD_SystemLastEditTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditUser) VALUES
	(@JddPk01, @JsPk01, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk02, @JsPk02, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk03, @JsPk03, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk04, @JsPk04, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E'),
	(@JddPk05, @JsPk05, 'JS', '2022-01-01 00:00:00', '2022-01-01 00:00:00', 'E', 'E');

INSERT INTO dbo.JobShipment (JS_PK,JS_UniqueConsignRef,JS_ShipmentType,JS_TransportMode,JS_PackingMode,JS_RL_NKOrigin,JS_RL_NKDestination) VALUES
	(@JsPk01,N'JsUniqueConsignRef01',N'STD',N'SEA',N'FCL',N'CNSAA',N'AUS2E'),
	(@JsPk02,N'JsUniqueConsignRef02',N'CLD',N'SEA',N'LCL',N'CNSAC',N'AUS2M')

INSERT dbo.JobConsol (JK_PK,JK_UniqueConsignRef) VALUES
	(@JkPk01,N'JkUniqueConsignRef01'),
	(@JkPk02,N'JkUniqueConsignRef02');

INSERT INTO dbo.JobConShipLink (JN_PK,JN_JK,JN_JS) VALUES
	(newid(),@JkPk01,@JsPk01),
	(newid(),@JkPk02,@JsPk02);

INSERT INTO dbo.JobConsolTransport (JW_PK,JW_ParentGUID,JW_TransportMode,JW_RL_NKLoadPort,JW_RL_NKDiscPort) VALUES
	(newid(),@JkPk01,N'SEA',N'CNSHA',N'AUSYD'),
	(newid(),@JkPk02,N'SEA',N'CNSHA',N'AUSYD');

DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk02 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.OrgHeader (OH_PK,OH_Code) VALUES
	(@OhPk01,N'OhCode1'),
	(@OhPk02,N'OhCode2');

DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk02 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.OrgAddress (OA_PK,OA_OH,OA_Address1) VALUES
	(@OaPk01,@OhPk01,'address1 01'),
	(@OaPk02,@OhPk02,'address1 02');

INSERT INTO dbo.JobDocAddress (E2_PK,E2_AddressOverride,E2_OA_Address,E2_ParentID,E2_AddressType,E2_ParentTableCode) VALUES
	(newid(),0,@OaPk01,@JkPk01,N'CHA','JK'),
	(newid(),0,@OaPk02,@JkPk02,N'CHA','JK');

INSERT INTO dbo.OrgCusCode (OK_PK,OK_CustomsRegNo,OK_CodeType,OK_OH) VALUES
	(newid(),N'C1H1',N'C1C',@OhPk01),
	(newid(),N'C2H2',N'C1C',@OhPk02);
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
