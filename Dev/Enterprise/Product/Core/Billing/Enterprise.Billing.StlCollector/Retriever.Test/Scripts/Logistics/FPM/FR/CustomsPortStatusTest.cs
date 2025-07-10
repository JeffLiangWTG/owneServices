using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CustomsPortStatus))]
	sealed class CustomsPortStatusTest : RefStlScriptWithDefaultsTest
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
			AssertEquals(16, transactions.Count());

			var row0 = transactions.First(t => t.Reference4.StartsWith("CRESA1"));
			var row1 = transactions.First(t => t.Reference4.StartsWith("CRESA2"));

			var row2 = transactions.First(t => t.Reference4.StartsWith("AMQ"));
			var row3 = transactions.First(t => t.Reference4.StartsWith("LPD"));
			var row4 = transactions.First(t => t.Reference4.StartsWith("LDE"));
			var row5 = transactions.First(t => t.Reference4.StartsWith("CDM"));
			var row6 = transactions.First(t => t.Reference4.StartsWith("TRC1"));
			var row7 = transactions.First(t => t.Reference4.StartsWith("TRC2"));
			var row8 = transactions.First(t => t.Reference4.StartsWith("DOS1"));
			var row9 = transactions.First(t => t.Reference4.StartsWith("DOS2"));

			var row10 = transactions.First(t => t.Reference4.StartsWith("NAMQ"));
			var row11 = transactions.First(t => t.Reference4.StartsWith("NDTE"));
			var row12 = transactions.First(t => t.Reference4.StartsWith("NLDE"));
			var row13 = transactions.First(t => t.Reference4.StartsWith("GIN"));

			var row14 = transactions.First(t => t.Reference4.StartsWith("CRESAX1"));
			var row15 = transactions.First(t => t.Reference4.StartsWith("CRESAX2"));

			Assert("CRESA3 MAA event is before DateTime range", !transactions.Any(t => t.Reference4.StartsWith("CRESA3")));
			Assert("CRESA4 MAA event is after DateTime ranges", !transactions.Any(t => t.Reference4.StartsWith("CRESA4")));
			Assert("CRESA6 is IRA event", !transactions.Any(t => t.Reference4.StartsWith("CRESA6")));
			Assert("CRESA8 is IRJ event", !transactions.Any(t => t.Reference4.StartsWith("CRESA8")));
			Assert("CRESA9 is ISN event", !transactions.Any(t => t.Reference4.StartsWith("CRESA9")));
			Assert("CAV Universal Event Message is not from SOGET or MGI", !transactions.Any(t => t.Reference4.StartsWith("CAV")));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB2", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "C0000001", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "Goods Received (CRESA)", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "MAA", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "CRESA1 - ERN01143237", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB1", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "C0000001", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "FRLEH - Goods Received (CRESA)", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "MAA", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "CRESA2 - ERN01143237", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB2", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "C0000001", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "AUSYD - Container Advice to Booking (AMQ)", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "MAA", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "AMQ - ERN01143237", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "DAU", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB2", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "C0000001", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "FRLEH - Provisional Unpacking List (LPD)", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "MAA", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "LPD - ERN01143237", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "DAU", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB2", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "S000001", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "FRLEH - Final Container Manifest (LDE)", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "MAA", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "LDE - ERN01143237", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "DAU", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "GB2", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "C0000001", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "FRLEH - Outturn Report (CDM)", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "MAA", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "CDM - ERN01143237", row5.Reference4);

				AssertEquals("[Row-6] CompanyCode", "DAU", row6.GetCompanyCode());
				AssertEquals("[Row-6] BranchCode", "GB2", row6.GetBranchCode());
				AssertEquals("[Row-6] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row6.ServiceOccuredUTC);
				AssertEquals("[Row-6] ItemCount", 1, row6.BillableCount);
				AssertEquals("[Row-6] TransactionReference01", "T000001", row6.Reference1);
				AssertEquals("[Row-6] TransactionReference02", "FRLEH - Tracing Request (TRC) - Export", row6.Reference2);
				AssertEquals("[Row-6] TransactionReference03", "MAA", row6.Reference3);
				AssertEquals("[Row-6] TransactionReference04", "TRC1 - ERN01143237", row6.Reference4);

				AssertEquals("[Row-7] CompanyCode", "DAU", row7.GetCompanyCode());
				AssertEquals("[Row-7] BranchCode", "GB2", row7.GetBranchCode());
				AssertEquals("[Row-7] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row7.ServiceOccuredUTC);
				AssertEquals("[Row-7] ItemCount", 1, row7.BillableCount);
				AssertEquals("[Row-7] TransactionReference01", "[BLANK]", row7.Reference1);
				AssertEquals("[Row-7] TransactionReference02", "FRLEH - Tracing Request (TRC) - Import", row7.Reference2);
				AssertEquals("[Row-7] TransactionReference03", "MAA", row7.Reference3);
				AssertEquals("[Row-7] TransactionReference04", "TRC2", row7.Reference4);

				AssertEquals("[Row-8] CompanyCode", "DAU", row8.GetCompanyCode());
				AssertEquals("[Row-8] BranchCode", "GB2", row8.GetBranchCode());
				AssertEquals("[Row-8] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row8.ServiceOccuredUTC);
				AssertEquals("[Row-8] ItemCount", 1, row8.BillableCount);
				AssertEquals("[Row-8] TransactionReference01", "C0000001", row8.Reference1);
				AssertEquals("[Row-8] TransactionReference02", "FRLEH - File Creation Request (DOS) - Export", row8.Reference2);
				AssertEquals("[Row-8] TransactionReference03", "MAA", row8.Reference3);
				AssertEquals("[Row-8] TransactionReference04", "DOS1", row8.Reference4);

				AssertEquals("[Row-9] CompanyCode", "DAU", row9.GetCompanyCode());
				AssertEquals("[Row-9] BranchCode", "GB2", row9.GetBranchCode());
				AssertEquals("[Row-9] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row9.ServiceOccuredUTC);
				AssertEquals("[Row-9] ItemCount", 1, row9.BillableCount);
				AssertEquals("[Row-9] TransactionReference01", "C0000001", row9.Reference1);
				AssertEquals("[Row-9] TransactionReference02", "FRLEH - File Creation Request (DOS) - Import", row9.Reference2);
				AssertEquals("[Row-9] TransactionReference03", "MRJ", row9.Reference3);
				AssertEquals("[Row-9] TransactionReference04", "DOS2 - ERF00!@@!", row9.Reference4);

				AssertEquals("[Row-10] CompanyCode", "DAU", row10.GetCompanyCode());
				AssertEquals("[Row-10] BranchCode", "GB1", row10.GetBranchCode());
				AssertEquals("[Row-10] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row10.ServiceOccuredUTC);
				AssertEquals("[Row-10] ItemCount", 1, row10.BillableCount);
				AssertEquals("[Row-10] TransactionReference01", "C0000001", row10.Reference1);
				AssertNull("[Row-10] TransactionReference02", row10.Reference2);
				AssertEquals("[Row-10] TransactionReference03", "STU", row10.Reference3);
				AssertEquals("[Row-10] TransactionReference04", "NAMQ - 20221008", row10.Reference4);

				AssertEquals("[Row-11] CompanyCode", "DAU", row11.GetCompanyCode());
				AssertEquals("[Row-11] BranchCode", "GB1", row11.GetBranchCode());
				AssertEquals("[Row-11] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row11.ServiceOccuredUTC);
				AssertEquals("[Row-11] ItemCount", 1, row11.BillableCount);
				AssertEquals("[Row-11] TransactionReference01", "C0000001", row11.Reference1);
				AssertNull("[Row-11] TransactionReference02", row11.Reference2);
				AssertEquals("[Row-11] TransactionReference03", "STU", row11.Reference3);
				AssertEquals("[Row-11] TransactionReference04", "NDTE - 20221009", row11.Reference4);

				AssertEquals("[Row-12] CompanyCode", "DAU", row12.GetCompanyCode());
				AssertEquals("[Row-12] BranchCode", "GB3", row12.GetBranchCode());
				AssertEquals("[Row-12] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row12.ServiceOccuredUTC);
				AssertEquals("[Row-12] ItemCount", 1, row12.BillableCount);
				AssertEquals("[Row-12] TransactionReference01", "C0000001", row12.Reference1);
				AssertEquals("[Row-12] TransactionReference02", "FRLEH", row12.Reference2);
				AssertEquals("[Row-12] TransactionReference03", "STU", row12.Reference3);
				AssertEquals("[Row-12] TransactionReference04", "NLDE - 20221010", row12.Reference4);

				AssertEquals("[Row-13] CompanyCode", "DAU", row13.GetCompanyCode());
				AssertEquals("[Row-13] BranchCode", "GB3", row13.GetBranchCode());
				AssertEquals("[Row-13] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), row13.ServiceOccuredUTC);
				AssertEquals("[Row-13] ItemCount", 1, row13.BillableCount);
				AssertEquals("[Row-13] TransactionReference01", "C0000001", row13.Reference1);
				AssertEquals("[Row-13] TransactionReference02", "FRLEH", row13.Reference2);
				AssertEquals("[Row-13] TransactionReference03", "GIN", row13.Reference3);
				AssertEquals("[Row-13] TransactionReference04", "GIN - 20221011", row13.Reference4);

				AssertEquals("[Row-14] CompanyCode", "DAU", row14.GetCompanyCode());
				AssertEquals("[Row-14] BranchCode", "GB5", row14.GetBranchCode());
				AssertEquals("[Row-14] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 1), row14.ServiceOccuredUTC);
				AssertEquals("[Row-14] ItemCount", 1, row14.BillableCount);
				AssertEquals("[Row-14] TransactionReference01", "S000001", row14.Reference1);
				AssertEquals("[Row-14] TransactionReference02", "Goods Received (CRESA)", row14.Reference2);
				AssertEquals("[Row-14] TransactionReference03", "MAA", row14.Reference3);
				AssertEquals("[Row-14] TransactionReference04", "CRESAX1 - ERN01143237", row14.Reference4);

				AssertEquals("[Row-15] CompanyCode", "DAU", row15.GetCompanyCode());
				AssertEquals("[Row-15] BranchCode", "GB4", row15.GetBranchCode());
				AssertEquals("[Row-15] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 2), row15.ServiceOccuredUTC);
				AssertEquals("[Row-15] ItemCount", 1, row15.BillableCount);
				AssertEquals("[Row-15] TransactionReference01", "C0000001", row15.Reference1);
				AssertEquals("[Row-15] TransactionReference02", "Goods Received (CRESA)", row15.Reference2);
				AssertEquals("[Row-15] TransactionReference03", "MRJ", row15.Reference3);
				AssertEquals("[Row-15] TransactionReference04", "CRESAX2 - ERN01143237", row15.Reference4);
			});
		}

		string CreateUniversalEvent(string messageTypeValue, string customsReferenceNumberKey, string forwardingConsolKey = "C0000001", string forwardingShipmentKey = "S000001", string transitReceiveKey = "T000001", string eventType = "MAA", string locationValue = "FRLEH", string equipmentReferenceNumberKey = "ERN01143237")
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

			var messageType = string.IsNullOrEmpty(messageTypeValue) ? "" : $"<MessageType>{messageTypeValue}</MessageType>";
			var location = string.IsNullOrEmpty(locationValue) ? "" : $"<Location>{locationValue}</Location>";

			var customsReferenceNumber = string.IsNullOrEmpty(customsReferenceNumberKey) ? "" : $"<CustomsReferenceNumber>{customsReferenceNumberKey}</CustomsReferenceNumber>";
			var equipmentReferenceNumber = string.IsNullOrEmpty(equipmentReferenceNumberKey) ? "" : $"<EquipmentReferenceNumber>{equipmentReferenceNumberKey}</EquipmentReferenceNumber>";

			return $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DataTargetCollection>
                {forwardingConsol}
                {forwardingShipment}
                {transitReceive}
            </DataTargetCollection>
        </DataContext>
        <EventTime>2021-02-05T12:22:00</EventTime>
        <EventType>{eventType}</EventType>
        <EventParameters>
            <Department>Terminal</Department>
            {messageType}
            {location}
            <ReferenceNumber>CRE03830751</ReferenceNumber>
            {customsReferenceNumber}>
            {equipmentReferenceNumber}
        </EventParameters>
        <EventReference />
        <ContextCollection>
            <Context>
                <Type>Message Reference</Type>
                <Value>SOG0000001072</Value>
            </Context>
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
DECLARE @JddPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk11 UNIQUEIDENTIFIER = newid();
DECLARE @JkPK01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk14 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk15 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk16 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk17 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @WrcPk01 UNIQUEIDENTIFIER = newid();
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
DECLARE @SlPk16 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk17 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk20 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk21 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk22 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk23 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk24 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk25 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk26 UNIQUEIDENTIFIER = newid();

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
DECLARE @EiPk16 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk17 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk20 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk21 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk22 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk23 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk24 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk25 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk26 UNIQUEIDENTIFIER = newid();

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
DECLARE @EmPk16 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk17 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk20 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk21 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk22 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk23 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk24 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk25 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk26 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk03, 'GB3', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk04, 'GB4', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk05, 'GB5', @GcPk01, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(@SlPk01, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk01, 'MAA', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk02, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MAA', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk03, 'JobDocumentData', getdate(), '2021-02-09 02:23:00', @JddPk03, 'MAA', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk04, 'JobDocumentData', getdate(), '2021-04-09 02:23:00', @JddPk04, 'MAA', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk06, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk06, 'IRA', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk16, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk16, 'IRJ', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk17, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk17, 'ISN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk01, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB1', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk16, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk17, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),

	(@SlPk08, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk08, 'MAA', '|DEP=CargoWise|MST=Container Advice to Booking (AMQ)', 'GB2', 'N'),
	(@SlPk09, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk09, 'MAA', '|DEP=CargoWise|MST=Provisional Unpacking List (LPD)', 'GB2', 'N'),
	(@SlPk10, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk10, 'MAA', '|DEP=CargoWise|MST=Final Container Manifest (LDE)', 'GB2', 'N'),
	(@SlPk11, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk11, 'MAA', '|DEP=CargoWise|MST=Outturn Report (CDM)', 'GB2', 'N'),
	(@SlPk12, 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPK01, 'MAA', '|DEP=CargoWise|MST=Tracing Request (TRC) - Export', 'GB2', 'N'),
	(@SlPk13, 'JobConsol', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MAA', '|DEP=CargoWise|MST=Tracing Request (TRC) - Import', 'GB2', 'N'),
	(@SlPk14, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk14, 'MAA', '|DEP=CargoWise|MST=File Creation Request (DOS) - Export', 'GB2', 'N'),
	(@SlPk15, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk15, 'MRJ', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),

	(@SlPk20, 'JobContainer', getdate(), '2021-03-09 02:23:00', @JcPk01, 'STU', '|DEP=Terminal|TYP=AMQ Notification', 'GB2', 'N'),
	(@SlPk21, 'JobContainer', getdate(), '2021-03-09 02:23:00', @JcPk01, 'STU', '|DEP=Terminal|TYP=DTE Notification', 'GB2', 'N'),

	(@SlPk22, 'JobContainer', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'STU', '|DEP=Terminal|TYP=LDE Notification', 'GB2', 'N'),
	(@SlPk23, 'JobContainer', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'GIN', '|FAC=CTO|LOC=FRLEH', 'GB2', 'N'),
	(@SlPk24, 'JobContainer', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'CAV', '|FAC=CTO|LOC=FRLEH', 'GB2', 'N'),

	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk08, 'MSN', '|DEP=CargoWise|MST=Container Advice to Booking (AMQ)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk09, 'MSN', '|DEP=CargoWise|MST=Provisional Unpacking List (LPD)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk10, 'MSN', '|DEP=CargoWise|MST=Final Container Manifest (LDE)', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk11, 'MSN', '|DEP=CargoWise|MST=Outturn Report (CDM)', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPK01, 'MSN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Export', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Import', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk14, 'MSN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Export', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk15, 'MSN', '|DEP=CargoWise|MST=File Creation Request (DOS) - Import', 'GB2', 'N'),

	(newid(), 'JobContainer', getdate(), '2021-03-09 01:23:00', @JcPk01, 'MSN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Export', 'GB1', 'N'),
	(newid(), 'JobContainer', getdate(), '2021-03-09 01:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Tracing Request (TRC) - Import', 'GB3', 'N'),

	(@SlPk25, 'WhsItemReceiveConsignment', getdate(), '2021-03-09 02:23:01', @WrcPk01, 'MAA', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),
	(@SlPk26, 'WhsItemReceiveConsignment', getdate(), '2021-03-09 02:23:02', @DuplicatePK, 'MRJ', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB2', 'N'),

	(newid(), 'WhsItemReceiveConsignment', getdate(), '2021-03-09 01:23:00', @WrcPk01, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB5', 'N'),
	(newid(), 'WhsItemReceiveConsignment', getdate(), '2021-03-09 01:23:00', @DuplicatePK, 'MSN', '|DEP=CargoWise|MST=Goods Received (CRESA)', 'GB4', 'N');

INSERT dbo.EDIInterchange (EI_PK, EI_From, EI_GB, EI_InterchangeNum, EI_SystemCreateTimeUtc, EI_SystemLastEditTimeUtc, EI_SystemCreateUser, EI_SystemLastEditUser) VALUES
	(@EiPk01, 'SOGET', @GbPk01, 'EI01', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk02, 'MGI', @GbPk01, 'EI02', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk03, 'SOGET', @GbPk01, 'EI03', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk04, 'SOGET', @GbPk01, 'EI04', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk06, 'SOGET', @GbPk01, 'EI06', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk16, 'SOGET', @GbPk01, 'EI16', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk17, 'SOGET', @GbPk01, 'EI17', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),

	(@EiPk08, 'SOGET', @GbPk01, 'EI08', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk09, 'SOGET', @GbPk01, 'EI09', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk10, 'SOGET', @GbPk01, 'EI10', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk11, 'SOGET', @GbPk01, 'EI11', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk12, 'MGI', @GbPk01, 'EI12', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk13, 'MGI', @GbPk01, 'EI13', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk14, 'MGI', @GbPk01, 'EI14', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk15, 'MGI', @GbPk01, 'EI15', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),

	(@EiPk20, 'MGI', @GbPk01, 'EI20', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk21, 'MGI', @GbPk01, 'EI21', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk22, 'MGI', @GbPk01, 'EI22', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk23, 'MGI', @GbPk01, 'EI23', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk24, 'ABC', @GbPk01, 'EI24', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),

	(@EiPk25, 'SOGET', @GbPk01, 'EI25', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk26, 'SOGET', @GbPk01, 'EI26', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD');

INSERT dbo.EDIMessage (EM_PK, EM_EI, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemLastEditTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_Status, EM_SystemCreateUser, EM_SystemLastEditUser, EM_IsActive, EM_MessageData) VALUES
	(@EmPk01, @EiPk01, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA1", locationValue: string.Empty)}')),
	(@EmPk02, @EiPk02, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA2")}')),
	(@EmPk03, @EiPk03, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA3")}')),
	(@EmPk04, @EiPk04, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA4")}')),
	(@EmPk06, @EiPk06, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA6", eventType: "IRA")}')),
	(@EmPk16, @EiPk16, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA8", eventType: "IRJ")}')),
	(@EmPk17, @EiPk17, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESA9", eventType: "ISN")}')),

	(@EmPk08, @EiPk08, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Container Advice to Booking (AMQ)", "AMQ", locationValue: "AUSYD")}')),
	(@EmPk09, @EiPk09, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Provisional Unpacking List (LPD)", "LPD")}')),
	(@EmPk10, @EiPk10, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Final Container Manifest (LDE)", "LDE", forwardingConsolKey: null)}')),
	(@EmPk11, @EiPk11, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Outturn Report (CDM)", "CDM", forwardingShipmentKey: "JS)))!")}')),
	(@EmPk12, @EiPk12, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Tracing Request (TRC) - Export", "TRC1", forwardingConsolKey: string.Empty, forwardingShipmentKey: string.Empty)}')),
	(@EmPk13, @EiPk13, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Tracing Request (TRC) - Import", string.Empty, string.Empty, string.Empty, string.Empty, equipmentReferenceNumberKey: "TRC2")}')),
	(@EmPk14, @EiPk14, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Export", "DOS1", equipmentReferenceNumberKey: string.Empty)}')),
	(@EmPk15, @EiPk15, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("File Creation Request (DOS) - Import", "DOS2", equipmentReferenceNumberKey: "ERF00!@@!", eventType: "MRJ")}')),

	(@EmPk20, @EiPk20, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent(string.Empty, "NAMQ", equipmentReferenceNumberKey: "20221008", eventType: "STU", locationValue: string.Empty)}')),
	(@EmPk21, @EiPk21, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent(string.Empty, "NDTE", equipmentReferenceNumberKey: "20221009", eventType: "STU", locationValue: string.Empty)}')),
	(@EmPk22, @EiPk22, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent(string.Empty, "NLDE", equipmentReferenceNumberKey: "20221010", eventType: "STU")}')),
	(@EmPk23, @EiPk23, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent(string.Empty, "GIN", equipmentReferenceNumberKey: "20221011", eventType: "GIN")}')),
	(@EmPk24, @EiPk24, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent(string.Empty, "CAV", equipmentReferenceNumberKey: "20221012", eventType: "CAV")}')),

	(@EmPk25, @EiPk25, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESAX1", "", locationValue: string.Empty)}')),
	(@EmPk26, @EiPk26, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("Goods Received (CRESA)", "CRESAX2", locationValue: string.Empty)}'));

INSERT dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation2ID, XX_RelationType) VALUES
	(newid(), @SlPk01, @EmPk01, 'XEM'),
	(newid(), @SlPk02, @EmPk02, 'XEM'),
	(newid(), @SlPk03, @EmPk03, 'XEM'),
	(newid(), @SlPk04, @EmPk04, 'XEM'),
	(newid(), @SlPk06, @EmPk06, 'XEM'),
	(newid(), @SlPk16, @EmPk16, 'XEM'),
	(newid(), @SlPk17, @EmPk17, 'XEM'),

	(newid(), @SlPk08, @EmPk08, 'XEM'),
	(newid(), @SlPk09, @EmPk09, 'XEM'),
	(newid(), @SlPk10, @EmPk10, 'XEM'),
	(newid(), @SlPk11, @EmPk11, 'XEM'),
	(newid(), @SlPk12, @EmPk12, 'XEM'),
	(newid(), @SlPk13, @EmPk13, 'XEM'),
	(newid(), @SlPk14, @EmPk14, 'XEM'),
	(newid(), @SlPk15, @EmPk15, 'XEM'),

	(newid(), @SlPk20, @EmPk20, 'XEM'),
	(newid(), @SlPk21, @EmPk21, 'XEM'),
	(newid(), @SlPk22, @EmPk22, 'XEM'),
	(newid(), @SlPk23, @EmPk23, 'XEM'),
	(newid(), @SlPk24, @EmPk24, 'XEM'),

	(newid(), @SlPk25, @EmPk25, 'XEM'),
	(newid(), @SlPk26, @EmPk26, 'XEM');
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
