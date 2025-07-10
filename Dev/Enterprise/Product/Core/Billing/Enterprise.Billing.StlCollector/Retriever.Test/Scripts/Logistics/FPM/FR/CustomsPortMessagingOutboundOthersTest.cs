using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CustomsPortMessagingOutboundOthers))]
	sealed class CustomsPortMessagingOutboundOthersTest : RefStlScriptWithDefaultsTest
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
			AssertEquals(10, transactions.Count());

			var row0 = transactions.First(t => t.Reference2.StartsWith("CRESA1"));
			var row1 = transactions.First(t => t.Reference2.StartsWith("CRESA2"));

			var row2 = transactions.First(t => t.Reference2.StartsWith("AMQ"));
			var row3 = transactions.First(t => t.Reference2.StartsWith("LPD"));
			var row4 = transactions.First(t => t.Reference2.StartsWith("LDE"));
			var row5 = transactions.First(t => t.Reference2.StartsWith("CDM"));
			var row6 = transactions.First(t => t.Reference2.StartsWith("TRC1"));
			var row7 = transactions.First(t => t.Reference2.StartsWith("TRC2"));

			var row8 = transactions.First(t => t.Reference2.StartsWith("CRESA7"));
			var row9 = transactions.First(t => t.Reference2.StartsWith("CRESA8"));

			Assert("CRESA3 ISN event is before DateTime range", !transactions.Any(t => t.Reference2.StartsWith("CRESA3")));
			Assert("CRESA4 ISN event is after DateTime ranges", !transactions.Any(t => t.Reference2.StartsWith("CRESA4")));
			Assert("CRESA6 does not have ISN event", !transactions.Any(t => t.Reference2.StartsWith("CRESA6")));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "T000001", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "CRESA1 - Goods Received (CRESA)", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "Original", row0.Reference3);
				AssertNull("[Row-0] TransactionReference04", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB1", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "S000001", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CRESA2 - Goods Received (CRESA)", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "Amendment", row1.Reference3);
				AssertNull("[Row-1] TransactionReference04", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB1", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "[BLANK]", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "AMQ - Container Advice to Booking (AMQ)", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "Original", row2.Reference3);
				AssertNull("[Row-2] TransactionReference04", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "DAU", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB1", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "[BLANK]", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "LPD - Provisional Unpacking List (LPD)", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "Original", row3.Reference3);
				AssertNull("[Row-3] TransactionReference04", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "DAU", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB1", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "C0000001", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "LDE - Final Container Manifest (LDE)", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "Original", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "CC00001", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "DAU", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB1", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "C0000001", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "CDM - Outturn Report (CDM)", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "Original", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "CC00001", row5.Reference4);

				AssertEquals("[Row-6] CompanyCode", "DAU", row6.GetCompanyCode());
				AssertEquals("[Row-6] BranchCode", "GB1", row6.GetBranchCode());
				AssertEquals("[Row-6] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row6.ServiceOccuredUTC);
				AssertEquals("[Row-6] ItemCount", 1, row6.BillableCount);
				AssertEquals("[Row-6] TransactionReference01", "C0000001", row6.Reference1);
				AssertEquals("[Row-6] TransactionReference02", "TRC1 - Tracing Request (TRC) - Export", row6.Reference2);
				AssertEquals("[Row-6] TransactionReference03", "Original", row6.Reference3);
				AssertEquals("[Row-6] TransactionReference04", "CC00001", row6.Reference4);

				AssertEquals("[Row-7] CompanyCode", "DAU", row7.GetCompanyCode());
				AssertEquals("[Row-7] BranchCode", "GB2", row7.GetBranchCode());
				AssertEquals("[Row-7] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row7.ServiceOccuredUTC);
				AssertEquals("[Row-7] ItemCount", 1, row7.BillableCount);
				AssertEquals("[Row-7] TransactionReference01", "C0000001", row7.Reference1);
				AssertEquals("[Row-7] TransactionReference02", "TRC2 - Tracing Request (TRC) - Import", row7.Reference2);
				AssertNull("[Row-7] TransactionReference03", row7.Reference3);
				AssertEquals("[Row-7] TransactionReference04", "CC00001", row7.Reference4);

				AssertEquals("[Row-8] CompanyCode", "DAU", row8.GetCompanyCode());
				AssertEquals("[Row-8] BranchCode", "GB2", row8.GetBranchCode());
				AssertEquals("[Row-8] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 1), row8.ServiceOccuredUTC);
				AssertEquals("[Row-8] ItemCount", 1, row8.BillableCount);
				AssertEquals("[Row-8] TransactionReference01", "[BLANK]", row8.Reference1);
				AssertEquals("[Row-8] TransactionReference02", "CRESA7 - Goods Received (CRESA)", row8.Reference2);
				AssertEquals("[Row-8] TransactionReference03", "Original", row8.Reference3);
				AssertNull("[Row-8] TransactionReference04", row8.Reference4);

				AssertEquals("[Row-9] CompanyCode", "DAU", row9.GetCompanyCode());
				AssertEquals("[Row-9] BranchCode", "GB3", row9.GetBranchCode());
				AssertEquals("[Row-9] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 2), row9.ServiceOccuredUTC);
				AssertEquals("[Row-9] ItemCount", 1, row9.BillableCount);
				AssertEquals("[Row-9] TransactionReference01", "S000001", row9.Reference1);
				AssertEquals("[Row-9] TransactionReference02", "CRESA8 - Goods Received (CRESA)", row9.Reference2);
				AssertEquals("[Row-9] TransactionReference03", "Amendment", row9.Reference3);
				AssertNull("[Row-9] TransactionReference04", row9.Reference4);
			});
		}

		string CreateUniversalEvent(string messageType, string operationalPortCode = "PCD", string forwardingConsolKey = "C0000001", string forwardingShipmentKey = "S000001", string transitReceiveKey = "T000001", string eventBranchValue = "GB1", string purpose = "ORG", string containerNumberValue = "CC00001", string eventType = "ISN")
		{
			var forwardingConsol = forwardingConsolKey == null ? "" : $@"
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>{forwardingConsolKey}</Key>
        </DataTarget>";

			var forwardingShipment = string.IsNullOrEmpty(forwardingShipmentKey) ? "" : $@"
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{forwardingShipmentKey}</Key>
        </DataTarget>";

			var transitReceive = string.IsNullOrEmpty(transitReceiveKey) ? "" : $@"
        <DataTarget>
          <Type>TransitReceive</Type>
          <Key>{transitReceiveKey}</Key>
        </DataTarget>";

			var eventBranch = string.IsNullOrEmpty(eventBranchValue) ? "" : $@"
        <Context>
          <Type>EventBranch</Type>
          <Value>{eventBranchValue}</Value>
        </Context>";

			var containerNumber = containerNumberValue == null ? "" : $@"
        <Context>
            <Type>ContainerNumber</Type>
            <Value>{containerNumberValue}</Value>
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
        {transitReceive}
      </DataTargetCollection>
    </DataContext>

    <ContextCollection>
        {containerNumber}
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
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk11 UNIQUEIDENTIFIER = newid();
DECLARE @JcPK01 UNIQUEIDENTIFIER = newid();
DECLARE @WrcPK01 UNIQUEIDENTIFIER = newid();
DECLARE @DuplicatePK UNIQUEIDENTIFIER = newid();

DECLARE @SlPk01 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk02 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk03 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk04 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk06 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk08 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk09 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk10 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk11 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk12 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk13 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk14 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk15 UNIQUEIDENTIFIER = newid();

DECLARE @EiPk01 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk02 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk03 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk04 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk06 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk08 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk09 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk10 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk11 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk12 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk13 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk14 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk15 UNIQUEIDENTIFIER = newid();

DECLARE @EmPk01 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk02 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk03 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk04 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk06 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk08 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk09 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk10 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk11 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk12 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk13 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk14 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk15 UNIQUEIDENTIFIER = newid();

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
	(@SlPk01, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk01, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk02, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk03, 'JobDocumentData', getdate(), '2021-02-09 02:23:00', @JddPk03, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk04, 'JobDocumentData', getdate(), '2021-04-09 02:23:00', @JddPk04, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk06, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk06, 'XXX', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB1', 'N'),

	(@SlPk08, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk08, 'ISN', '|DEP=CargoWise|MST=Container Advice to Booking (AMQ)', 'GB2', 'N'),
	(@SlPk09, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk09, 'ISN', '|DEP=CargoWise|MST=Provisional Unpacking List (LPD)', 'GB2', 'N'),
	(@SlPk10, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk10, 'ISN', '|DEP=CargoWise|MST=Final Container Manifest (LDE)', 'GB2', 'N'),
	(@SlPk11, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk11, 'ISN', '|DEP=CargoWise|MST=Outturn Report (CDM)', 'GB2', 'N'),
	(@SlPk12, 'JobContainer', getdate(), '2021-03-09 02:23:00', @JcPK01, 'ISN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Export', 'GB2', 'N'),
	(@SlPk13, 'JobContainer', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'ISN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Import', 'GB2', 'N'),

	(newid(), 'JobContainer', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Import', 'GB2', 'N'),

	(@SlPk14, 'WhsItemReceiveConsignment', getdate(), '2021-03-09 02:23:01', @WrcPK01, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk15, 'WhsItemReceiveConsignment', getdate(), '2021-03-09 02:23:02', @DuplicatePK, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),

	(newid(), 'WhsItemReceiveConsignment', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB3', 'N');

INSERT dbo.EDIInterchange (EI_PK, EI_From, EI_GB, EI_InterchangeNum, EI_SystemCreateTimeUtc, EI_SystemLastEditTimeUtc, EI_SystemCreateUser, EI_SystemLastEditUser) VALUES
	(@EiPk01, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI01', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk02, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI02', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk03, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI03', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk04, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI04', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk06, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI06', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),

	(@EiPk08, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI08', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk09, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI09', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk10, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI10', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk11, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI11', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk12, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI12', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk13, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI13', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),

	(@EiPk14, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI14', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk15, 'FORWARDING_PORT_MESSAGE', @GbPk01, 'EI15', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD');

INSERT dbo.EDIMessage (EM_PK, EM_EI, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemLastEditTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_Status, EM_SystemCreateUser, EM_SystemLastEditUser, EM_IsActive, EM_MessageData) VALUES
	(@EmPk01, @EiPk01, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA1", forwardingShipmentKey: string.Empty)}')),
	(@EmPk02, @EiPk02, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA2", eventBranchValue: string.Empty, purpose: "AMD", containerNumberValue: string.Empty)}')),
	(@EmPk03, @EiPk03, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA3")}')),
	(@EmPk04, @EiPk04, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA4")}')),
	(@EmPk06, @EiPk06, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA6", eventType: "XXX")}')),

	(@EmPk08, @EiPk08, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Container Advice to Booking (AMQ)", "AMQ", null, containerNumberValue: string.Empty)}')),
	(@EmPk09, @EiPk09, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Provisional Unpacking List (LPD)", "LPD", string.Empty, containerNumberValue: null)}')),
	(@EmPk10, @EiPk10, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Final Container Manifest (LDE)", "LDE")}')),
	(@EmPk11, @EiPk11, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Outturn Report (CDM)", "CDM")}')),
	(@EmPk12, @EiPk12, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Tracing Request (TRC) - Export", "TRC1")}')),
	(@EmPk13, @EiPk13, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Tracing Request (TRC) - Import", "TRC2", eventBranchValue: string.Empty, purpose: string.Empty)}')),

	(@EmPk14, @EiPk14, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA7", string.Empty, string.Empty, string.Empty, "GB2")}')),
	(@EmPk15, @EiPk15, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA8", eventBranchValue: string.Empty, purpose: "AMD")}'));

INSERT dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation2ID, XX_RelationType) VALUES
	(newid(), @SlPk01, @EmPk01, 'XEM'),
	(newid(), @SlPk02, @EmPk02, 'XEM'),
	(newid(), @SlPk03, @EmPk03, 'XEM'),
	(newid(), @SlPk04, @EmPk04, 'XEM'),
	(newid(), @SlPk06, @EmPk06, 'XEM'),

	(newid(), @SlPk08, @EmPk08, 'XEM'),
	(newid(), @SlPk09, @EmPk09, 'XEM'),
	(newid(), @SlPk10, @EmPk10, 'XEM'),
	(newid(), @SlPk11, @EmPk11, 'XEM'),
	(newid(), @SlPk12, @EmPk12, 'XEM'),
	(newid(), @SlPk13, @EmPk13, 'XEM'),

	(newid(), @SlPk14, @EmPk14, 'XEM'),
	(newid(), @SlPk15, @EmPk15, 'XEM');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
