using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CustomsPortMessagingOutboundDataRecord))]
	sealed class CustomsPortMessagingOutboundDataRecordTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2021, 3);
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(3, transactions.Count());

			var row0 = transactions.First(t => t.Reference1.StartsWith("JS_TESTID"));
			var row1 = transactions.First(t => t.Reference2.StartsWith("DOS2"));
			var row2 = transactions.First(t => t.Reference2.StartsWith("DOS7"));

			Assert("DOS3 ISN event is before DateTime range", !transactions.Any(t => t.Reference2.StartsWith("DOS3")));
			Assert("DOS4 ISN event is after DateTime ranges", !transactions.Any(t => t.Reference2.StartsWith("DOS4")));
			Assert("DOS6 does not have ISN event", !transactions.Any(t => t.Reference2.StartsWith("DOS6")));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JS_TESTID", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "File Creation Request (DOS) - Import", row0.Reference2);
				AssertNull("[Row-0] TransactionReference03", row0.Reference3);
				AssertNull("[Row-0] TransactionReference04", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "[BLANK]", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "DOS2 - File Creation Request (DOS) - Export", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "Amendment", row1.Reference3);
				AssertNull("[Row-1] TransactionReference04", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB3", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 1), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "S000001", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "DOS7 - File Creation Request (DOS) - Import", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "Original", row2.Reference3);
				AssertNull("[Row-2] TransactionReference04", row2.Reference4);
			});
		}

		string CreateUniversalEvent(string messageType, string operationalPortCode, string forwardingConsolKey = "C0000001", string forwardingShipmentKey = "S000001", string eventBranchValue = "GB1", string purpose = "ORG", string eventType = "ISN")
		{
			var forwardingConsol = forwardingConsolKey == null ? "" : $@"
		<DataTarget>
		  <Type>ForwardingConsol</Type>
		  <Key>{forwardingConsolKey}</Key>
		</DataTarget>";

			var forwardingShipment = forwardingShipmentKey == null ? "" : $@"
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key>{forwardingShipmentKey}</Key>
		</DataTarget>";

			var eventBranch = string.IsNullOrEmpty(eventBranchValue) ? "" : $@"
		 <Context>
			<Type>EventBranch</Type>
			<Value>{eventBranchValue}</Value>
		</Context>";

			return $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<EventTime>10-JUL-2010 18:00</EventTime>
	<EventType>{eventType}</EventType>
	<EventParameters>
	  <Department>CargoWise</Department>
	  <MessageType>{messageType}</MessageType>
	</EventParameters>
	<DataContext>
	  <DataTargetCollection>
		{forwardingConsol}
		{forwardingShipment}
	  </DataTargetCollection>
	</DataContext>

	<ContextCollection>
		<Context>
			<Type>OperationalPortCode</Type>
			<Value>{operationalPortCode}</Value>
		</Context>
		<Context>
			<Type>Purpose</Type>
			<Value>{purpose}</Value>
		</Context>
		{eventBranch}
	</ContextCollection>
  </Event>
</UniversalEvent>
";
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

DECLARE @SlPk01 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk02 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk03 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk04 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk06 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk07 UNIQUEIDENTIFIER = newid();

DECLARE @EiPk01 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk02 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk03 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk04 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk06 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk07 UNIQUEIDENTIFIER = newid();

DECLARE @EmPk01 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk02 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk03 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk04 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk06 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk07 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk03, 'GB3', @GcPk01, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(@SlPk01, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk01, 'ISN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),
	(@SlPk02, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk02, 'ISN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Export', 'GB2', 'N'),
	(@SlPk03, 'JobDocumentData', getdate(), '2021-02-09 02:23:00', @JddPk03, 'ISN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),
	(@SlPk04, 'JobDocumentData', getdate(), '2021-04-09 02:23:00', @JddPk04, 'ISN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Export', 'GB2', 'N'),
	(@SlPk06, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk06, 'XXX', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),
	(@SlPk07, 'JobDocumentData', getdate(), '2021-03-09 02:23:01', @JddPk07, 'ISN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk02, 'MSN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:01', @JddPk07, 'MSN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB3', 'N');

INSERT dbo.EDIInterchange (EI_PK, EI_From, EI_GB, EI_InterchangeNum, EI_SystemCreateTimeUtc, EI_SystemLastEditTimeUtc, EI_SystemCreateUser, EI_SystemLastEditUser) VALUES
	(@EiPk01, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI01', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk02, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI02', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk03, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI03', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk04, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI04', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk06, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI06', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk07, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI07', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD');

INSERT dbo.EDIMessage (EM_PK, EM_EI, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemLastEditTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_Status, EM_SystemCreateUser, EM_SystemLastEditUser, EM_IsActive, EM_MessageData) VALUES
	(@EmPk01, @EiPk01, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Import", string.Empty, string.Empty, "JS_TESTID", purpose: string.Empty)}')),
	(@EmPk02, @EiPk02, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Export", "DOS2", string.Empty, string.Empty, eventBranchValue: string.Empty, purpose: "AMD")}')),
	(@EmPk03, @EiPk03, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Import", "DOS3")}')),
	(@EmPk04, @EiPk04, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Export", "DOS4")}')),
	(@EmPk06, @EiPk06, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Import", "DOS6", eventType: "XXX")}')),
	(@EmPk07, @EiPk07, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Import", "DOS7", null, eventBranchValue: string.Empty)}'));

INSERT dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation2ID, XX_RelationType) VALUES
	(newid(), @SlPk01, @EmPk01, 'XEM'),
	(newid(), @SlPk02, @EmPk02, 'XEM'),
	(newid(), @SlPk03, @EmPk03, 'XEM'),
	(newid(), @SlPk04, @EmPk04, 'XEM'),
	(newid(), @SlPk06, @EmPk06, 'XEM'),
	(newid(), @SlPk07, @EmPk07, 'XEM');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
