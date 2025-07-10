using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord))]
	public class MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecordTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord();
		}

		protected override void PrepareTestData()
		{
			CreateTestDataForSingleBatch(1, "12345678-24112401", new[] { "2024112412340001" });
			CreateTestDataForSingleBatch(2, "12345678-24112402", new[] { "2024112412340002" });
			CreateTestDataForSingleBatch(3, "12345678-24112403", new[] { "2024112412340003" });
			CreateTestDataForSingleBatch(4, "12345678-24112404", new[] { "2024112412340004" });
			CreateTestDataForSingleBatch(5, "12345678-24112405", new[] { "2024112412340005" });

			CreateTestDataForSingleBatch(6, "12345678-24112406", new[] { "2024112412340006", "2024112412340007" });
			CreateTestDataForSingleBatch(7, "12345678-24112407", new[] { "2024112412340008", "2024112412340009" });
			CreateTestDataForSingleBatch(8, "12345678-24112408", new[] { "2024112412340010", "2024112412340011" });

			CreateTestDataForSingleBatch(9, "12345678-24112409", new[] { "2024112412340012", "2024112412340013", "2024112412340014" });
			CreateTestDataForSingleBatch(10, "12345678-24112410", new[] { "2024112412340015", "2024112412340016", "2024112412340017" });
		}

		protected override void AssertTransformationResults()
		{
			var accTransactionAuthorisationRecordResult = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.AccTransactionHeaderAuthorisationRecord");

			AssertEquals("Should have 17 expectedAuthorisationRecordInfos.", 17, expectedAuthorisationRecordInfos.Count);
			AssertEquals("Result should have 17 rows.", 17, accTransactionAuthorisationRecordResult.Rows.Count);

			foreach (var (transactionPK, submitID, issueID) in expectedAuthorisationRecordInfos)
			{
				AssertAccTransactionAuthorisationRecordRow(accTransactionAuthorisationRecordResult.Rows.OfType<DataRow>(), transactionPK, submitID, issueID);
			}
		}

		public void TestOnlinePostUpgradeTransformation_Batching()
		{
			PrepareTestData();
			var batchSize = 3;
			var transformationInstance = new MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord(batchSize);
			transformationInstance.Initialise(manager: new DummyUpgradeManager());

			var dataRowsToVerify = new List<DataRow>();
			for (var i = 0; i < 4; i++)
			{
				AssertExceptionThrown<OperationCanceledException>(() => transformationInstance.Run(TransformationSection.OnlinePostUpgrade, new CancellationToken(true)));

				var accTransactionAuthorisationRecordResult = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM dbo.AccTransactionHeaderAuthorisationRecord");
				var newLoadedRows = accTransactionAuthorisationRecordResult.Rows.OfType<DataRow>()
					.Where(newRow => !dataRowsToVerify.Any(existedRow => Convert.ToString(existedRow["AHF_PK"]) == Convert.ToString(newRow["AHF_PK"])));
				dataRowsToVerify.AddRange(newLoadedRows);
			}

			foreach (var (transactionPK, submitID, issueID) in expectedAuthorisationRecordInfos)
			{
				AssertAccTransactionAuthorisationRecordRow(dataRowsToVerify, transactionPK, submitID, issueID);
			}
		}

		void CreateTestDataForSingleBatch(int batchNumber, string submitID, IEnumerable<string> issueIDs)
		{
			var batchPK = CreateEInvoicingBatch(companyPK, batchNumber);

			var genMessagePK = CreateEdiMessage("GEI", "GEN", "GEN", "TRX", branchPK, departmentPK, Encoding.UTF8.GetBytes(CreateGENXmlString(submitID)));
			var genStmALogPK = CreateStmALog("AccEInvoicingBatch", batchPK, "DEX");
			testDataCreator.CreateGenPivot("XEM", "SL", genStmALogPK, "EM", genMessagePK);

			var invoiceDocs = issueIDs.Select(x => CreateInvoiceDoc(x)).ToArray();
			var xutMessagePK = CreateEdiMessage("UDM", "XDC", "XUE", "RCV", branchPK, departmentPK, Encoding.UTF8.GetBytes(CreateXUTXmlString(invoiceDocs)));
			var xutStmALogPK = CreateStmALog("AccEInvoicingBatch", batchPK, "IAK");
			testDataCreator.CreateGenPivot("XEM", "SL", xutStmALogPK, "EM", xutMessagePK);

			foreach(var issueID in issueIDs)
			{
				var transactionHeaderPK = testDataCreator.CreateAccTransactionHeader("AR", "INV", branchPK, companyPK, departmentPK, issueID.Substring(issueID.Length - 3));
				var transactionPivotPK = CreateEInvoicingTransactionPivot(companyPK, batchPK, transactionHeaderPK);
				var transactionHeaderReference = CreateTransactionHeaderReference(transactionHeaderPK, issueID);

				expectedAuthorisationRecordInfos.Add((transactionHeaderPK, submitID, issueID));
			}
		}

		void AssertAccTransactionAuthorisationRecordRow(IEnumerable<DataRow> dataRows, Guid transactionPK, string submitID, string issueID)
		{
			var accTransactionAuthorisationRecordRow = dataRows.First(row =>
				new Guid(Convert.ToString(row["AHF_ParentId"])).Equals(transactionPK));

			AssertEquals("KRS", accTransactionAuthorisationRecordRow["AHF_RecordType"]);
			AssertEquals("AH", accTransactionAuthorisationRecordRow["AHF_ParentTableCode"]);

			var expectedInvoiceDoc = CreateInvoiceDoc(issueID);
			var storedInvoiceDoc = Encoding.UTF8.GetString(accTransactionAuthorisationRecordRow["AHF_AuthorisationData"] as byte[]);
			AssertEquals(expectedInvoiceDoc, storedInvoiceDoc);
		}

		#region Mock Data Creator

		Guid CreateEInvoicingBatch(Guid companyPK, int batchNumber)
		{
			var eInvoicingBatchPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.AccEInvoicingBatch (AIB_PK, AIB_GC, AIB_BatchNumber, AIB_Status, AIB_SystemCreateTimeUtc, AIB_SystemCreateUser, AIB_SystemLastEditTimeUtc, AIB_SystemLastEditUser)
							VALUES ('{eInvoicingBatchPK}', '{companyPK}', {batchNumber}, 'SNT', GETUTCDATE(),  'XXX', GETUTCDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();

			return eInvoicingBatchPK;
		}

		Guid CreateEdiMessage(string applicationCode, string messageType, string messageSubType, string receiveTransmit, Guid branchPK, Guid departmentPK, byte[] messageData)
		{
			var ediMessagePK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT dbo.EdiMessage (EM_PK, EM_ApplicationCode, EM_MessageType, EM_MessageSubType, EM_ReceiveTransmit, EM_GB, EM_GE, EM_MessageData, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser)
							VALUES	('{ediMessagePK}', '{applicationCode}', '{messageType}', '{messageSubType}', '{receiveTransmit}', '{branchPK}', '{departmentPK}', @messageData, GETUTCDATE(),  'XXX', GETUTCDATE(), 'XXX')");

			using ( var command = TestConnection.Command(script))
			{
				command.AddParameter("@messageData", SqlDbType.VarBinary, messageData);
				command.ExecuteNonQuery();
			}

			return ediMessagePK;
		}

		Guid CreateStmALog(string tableName, Guid parentPK, string eventCode)
		{
			var stmALogPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_GS_NKUser, SL_SE_NKEvent, SL_EventTime, SL_EventTimeUtc, SL_PostedTimeUtc)
							VALUES ('{stmALogPK}', '{tableName}', '{parentPK}', 'XXX', '{eventCode}', GETUTCDATE(), GETUTCDATE(), GETUTCDATE())");

			TestConnection.Command(script).ExecuteNonQuery();

			return stmALogPK;
		}

		Guid CreateEInvoicingTransactionPivot(Guid companyPK, Guid batchPK, Guid transactionPK)
		{
			var transactionPivotPK = Guid.NewGuid();

			var script = FormattableString.Invariant($@"INSERT INTO dbo.AccEInvoicingTransactionPivot (AIP_PK, AIP_GC, AIP_AIB, AIP_ParentID, AIP_ParentTableCode, AIP_Status, AIP_ActionType, AIP_RN_NKCountryCode, AIP_SystemCreateTimeUtc, AIP_SystemCreateUser, AIP_SystemLastEditTimeUtc, AIP_SystemLastEditUser)
							VALUES ('{transactionPivotPK}', '{companyPK}', '{batchPK}', '{transactionPK}', 'AH', 'SUC', 'SUB', 'KR', GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();

			return transactionPivotPK;
		}

		Guid CreateTransactionHeaderReference(Guid transactionHeaderPK, string reference)
		{
			var transactionHeaderReferencePK = Guid.NewGuid();		

			var script = FormattableString.Invariant($@"INSERT INTO dbo.AccTransactionHeaderReference (AH1_PK, AH1_AH, AH1_Type, AH1_Reference, AH1_SystemCreateTimeUtc, AH1_SystemCreateUser, AH1_SystemLastEditTimeUtc, AH1_SystemLastEditUser)
							VALUES ('{transactionHeaderReferencePK}', '{transactionHeaderPK}', 'KRI', '{reference}', GETUTCDATE(), 'XXX', GETUTCDATE(), 'XXX')");

			TestConnection.Command(script).ExecuteNonQuery();

			return transactionHeaderReferencePK;
		}

		#endregion

		#region xml template string

		string CreateGENXmlString(string submitId)
		{
			return $@"
<GlobalElectronicInvoicing xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing"">
	<Header>
		<ElectronicInvoiceBatchRequest>
			<AdditionalDataItems>
				<AdditionalDataItem>
					<Key>SubmitID</Key>
					<Value>{submitId}</Value>
				</AdditionalDataItem>
			</AdditionalDataItems>
		</ElectronicInvoiceBatchRequest>
	</Header>
</GlobalElectronicInvoicing>";
		}

		string CreateInvoiceDoc(string issueID)
		{
			return $@"
<TaxInvoice xmlns=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0 http://www.kec.or.kr/standard/Tax/TaxInvoiceSchemaModule_1.0.xsd"">
	<TaxInvoiceDocument>
		<IssueID>{issueID}</IssueID>
	</TaxInvoiceDocument>
</TaxInvoice>";
		}

		string CreateXUTXmlString(IEnumerable<string> invoiceDocs)
		{
			var contextNodes = invoiceDocs.Select(doc => $@"
<Context>
	<Type>EINV_DocTaxInvoice</Type>
	<Value>{Convert.ToBase64String(Encoding.UTF8.GetBytes(doc))}</Value>
</Context>");

			var xmlString = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
	<Event>
		<ContextCollection>
			{string.Join(Environment.NewLine, contextNodes)}
		</ContextCollection>
	</Event>
</UniversalEvent>";

			return xmlString;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			testDataCreator = new TransformationTestDataCreator();
			companyPK = testDataCreator.CreateGlbCompany("TKR", "KR");
			branchPK = testDataCreator.CreateGlbBranch("CHI", companyPK);
			departmentPK = testDataCreator.CreateGlbDepartment("AAA");
			expectedAuthorisationRecordInfos = new();
		}

		TransformationTestDataCreator testDataCreator;
		Guid companyPK;
		Guid branchPK;
		Guid departmentPK;
		List<(Guid transactionPK, string submitID, string issueID)> expectedAuthorisationRecordInfos;
	}
}
