using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderFIATAInboundMessages))]
	sealed class ForwarderFIATAInboundMessagesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => true;

		protected override void PrepareTestData()
		{
			string sqlText = $@"
DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JsPk07 UNIQUEIDENTIFIER = newid();

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
DECLARE @SlPk05 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk06 UNIQUEIDENTIFIER = newid();
DECLARE @SlPk07 UNIQUEIDENTIFIER = newid();

DECLARE @EiPk01 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk02 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk03 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk04 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk05 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk06 UNIQUEIDENTIFIER = newid();
DECLARE @EiPk07 UNIQUEIDENTIFIER = newid();

DECLARE @EmPk01 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk02 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk03 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk04 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk05 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk06 UNIQUEIDENTIFIER = newid();
DECLARE @EmPk07 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'SY1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'SY2', @GcPk01, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef) VALUES
	(@JsPk01, 'SHP01'),
	(@JsPk02, 'SHP02'),
	(@JsPk03, 'SHP03'),
	(@JsPk04, 'SHP04'),
	(@JsPk05, 'SHP05'),
	(@JsPk06, 'SHP06'),
	(@JsPk07, 'SHP07');

INSERT dbo.JobDocumentData(JDD_PK, JDD_ParentTableCode, JDD_ParentID, JDD_Name, JDD_SystemCreateTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditTimeUtc, JDD_SystemLastEditUser) VALUES
	(@JddPk01, 'JS', @JsPk01, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN'),
	(@JddPk02, 'JS', @JsPk02, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN'),
	(@JddPk03, 'JS', @JsPk03, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN'),
	(@JddPk04, 'JS', @JsPk04, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN'),
	(@JddPk05, 'JS', @JsPk05, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN'),
	(@JddPk06, 'JS', @JsPk06, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN'),
	(@JddPk07, 'JS', @JsPk07, 'BillOfLading', '2021-03-01', 'DNN', '2021-03-01', 'DNN');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(@SlPk01, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk01, 'ATH', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(@SlPk02, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk02, 'ATH', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(@SlPk03, 'JobDocumentData', getdate(), '2021-02-09 02:23:00', @JddPk03, 'ATH', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(@SlPk04, 'JobDocumentData', getdate(), '2021-04-09 02:23:00', @JddPk04, 'ATH', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(@SlPk05, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk05, 'ATH', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(@SlPk06, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk06, 'XXX', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(@SlPk07, 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk07, 'ATH', '|DEP=FIATA|TYP=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk01, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk02, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk03, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk04, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk05, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk06, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk07, 'MSN', '|DEP=CargoWise|MST=Bill Of Lading', 'SY2', 'N');

INSERT dbo.EDIInterchange (EI_PK, EI_From, EI_GB, EI_InterchangeNum, EI_SystemCreateTimeUtc, EI_SystemLastEditTimeUtc, EI_SystemCreateUser, EI_SystemLastEditUser) VALUES
	(@EiPk01, 'FIATA_HBL', @GbPk01, 'EI01', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk02, 'FIATA_HBL', @GbPk01, 'EI02', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk03, 'FIATA_HBL', @GbPk01, 'EI03', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk04, 'FIATA_HBL', @GbPk01, 'EI04', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk05, 'FIATA_HBL', @GbPk01, 'EI05', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk06, 'FIATA_HBL', @GbPk01, 'EI06', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD'),
	(@EiPk07, 'NOT_FIATA_HBL', @GbPk01, 'EI07', '2021-03-09 00:23:00', '2021-03-09 00:23:00', '~AD', '~AD');

INSERT dbo.EDIMessage (EM_PK, EM_EI, EM_GB, EM_GE, EM_SystemCreateTimeUtc, EM_SystemLastEditTimeUtc, EM_ReceiveTransmit, EM_MessageType, EM_ApplicationCode, EM_Status, EM_SystemCreateUser, EM_SystemLastEditUser, EM_IsActive, EM_MessageData) VALUES
	(@EmPk01, @EiPk01, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP01", "BOL01.pdf")}')),
	(@EmPk02, @EiPk02, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP02", "BOL02.pdf", eventBranch: string.Empty)}')),
	(@EmPk03, @EiPk03, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP03", "BOL03.pdf")}')),
	(@EmPk04, @EiPk04, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP04", "BOL04.pdf")}')),
	(@EmPk05, @EiPk05, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP05", "BOL05.pdf", paramsType: "Not BOL")}')),
	(@EmPk06, @EiPk06, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP06", "BOL06.pdf", eventType: "XXX")}')),
	(@EmPk07, @EiPk07, @GbPk01, @GePk01, '2021-03-09 00:23:00', '2021-03-09 00:23:00', 'TRX', 'USG', 'USG', 'CAP', '~AD', '~AD', 1, CONVERT(VARBINARY(max),'{CreateUniversalEvent("SHP07", "BOL07.pdf")}'));

INSERT dbo.GenPivot (XX_PK, XX_Relation1ID, XX_Relation2ID, XX_RelationType) VALUES
	(newid(), @SlPk01, @EmPk01, 'XEM'),
	(newid(), @SlPk02, @EmPk02, 'XEM'),
	(newid(), @SlPk03, @EmPk03, 'XEM'),
	(newid(), @SlPk04, @EmPk04, 'XEM'),
	(newid(), @SlPk05, @EmPk05, 'XEM'),
	(newid(), @SlPk06, @EmPk06, 'XEM'),
	(newid(), @SlPk07, @EmPk07, 'XEM');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = transactions.FirstOrDefault(t => t.Reference1.StartsWith("SHP01"));
			var transaction2 = transactions.FirstOrDefault(t => t.Reference1.StartsWith("SHP02"));

			AssertNotNull(transaction1);
			AssertNotNull(transaction2);

			Assert("SHP03 ISN event is before DateTime range", !transactions.Any(t => t.Reference1 == "SHP03"));
			Assert("SHP04 ISN event is after DateTime ranges", !transactions.Any(t => t.Reference1 == "SHP04"));
			Assert("SHP05 UE's Message Type is not 'Bill Of Lading'", !transactions.Any(t => t.Reference1 == "SHP05"));
			Assert("SHPO6 does not have ISN event", !transactions.Any(t => t.Reference1 == "SHP06"));
			Assert("SHPO7 UE is not from FIATA_HBL", !transactions.Any(t => t.Reference1 == "SHP07"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DAU", transaction1.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "SY2", transaction1.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), transaction1.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, transaction1.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "SHP01", transaction1.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "Bill Of Lading", transaction1.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "ATH", transaction1.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "BOL01", transaction1.Reference4);

				AssertEquals("[Row-1] CompanyCode", "DAU", transaction2.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "SY2", transaction2.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), transaction2.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, transaction2.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "SHP02", transaction2.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "Bill Of Lading", transaction2.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "ATH", transaction2.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "BOL02", transaction2.Reference4);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2021, 3);
			}
		}

		string CreateUniversalEvent(string shipmentUniqueConsignRef, string fileName, string eventType = "ATH", string eventBranch = "SY1", string paramsType = "Bill Of Lading")
		{
			string CreateContextElementIfValueIsNotEmpty(string type, string value)
			{
				return string.IsNullOrEmpty(value)
					? string.Empty
					: $@"<Context>
  <Type>{type}</Type>
  <Value>{value}</Value>
</Context>";
			}

			var attachedDocumentCollection = string.IsNullOrEmpty(fileName)
				? string.Empty
				: $@"<AttachedDocumentCollection>
  <AttachedDocument>
	<FileName>{fileName}</FileName>
  </AttachedDocument>
</AttachedDocumentCollection>
";

			return string.Format(@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<EventTime>10-MAR-2021 18:00</EventTime>
	<DataContext>
	  <DataTargetCollection>
	   <DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key>{0}</Key>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<EventParameters>
	  {1}
	</EventParameters>
	<ContextCollection>
		{2}
	</ContextCollection>
	<EventType>{3}</EventType>
	{4}
  </Event>
</UniversalEvent>",
				shipmentUniqueConsignRef,
				string.IsNullOrEmpty(paramsType) ? string.Empty : $"<Type>{paramsType}</Type>",
				CreateContextElementIfValueIsNotEmpty("EventBranch", eventBranch),
				eventType,
				attachedDocumentCollection);
		}
	}
}
