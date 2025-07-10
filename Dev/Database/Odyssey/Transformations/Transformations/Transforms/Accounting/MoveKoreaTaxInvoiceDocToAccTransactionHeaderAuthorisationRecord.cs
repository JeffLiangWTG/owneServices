using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public class MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord : DataTransformation
	{
		public override string UserDescription => "Move the Korea E-Invoice TaxInvoice Documents to the table AccTransactionHeaderAuthorisationRecord";

		public MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord() : this(1000)
		{
		}

		internal MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord(int batchSize) : base()
		{
			BatchSize = batchSize;
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedEInvoicingBatchPK);
			var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, AccEInvoicingBatchSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(chunkSize: BatchSize, Math.Max(1, rowCount), lastProcessedPK);

			var stopWatch = Stopwatch.StartNew();
			foreach(var chunk in chunks)
			{
				ProcessBatch(chunk.LowerBound, chunk.UpperBound);

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, LastProcessedEInvoicingBatchPK, chunk.UpperBound.ToString());
					manager.ShowInfoMessage($"Last processed batch chunk: {chunk.UpperBound}.");

					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedEInvoicingBatchPK);
		}

		void ProcessBatch(Guid startGuid, Guid endGuid)
		{
			const string transformationSql = $@"
WITH XMLNAMESPACES(
	'http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing' AS gei,
	'http://www.cargowise.com/Schemas/Universal/2012/11' AS xut,
	'urn:kr:or:kec:standard:Tax:ReusableAggregateBusinessInformationEntitySchemaModule:1:0' AS inv
),
Batch_EDIMessage AS(
	SELECT AIB_PK, AIB_BatchNumber, TRY_CAST(dbo.CLRUncompressAsString(EM_MessageData) AS XML) AS EM_MessageData_XML, SL_SE_NKEvent, SL_EventTimeUtc
	FROM dbo.AccEInvoicingBatch
		JOIN dbo.GlbCompany on AIB_GC = GC_PK
		JOIN dbo.StmALog on SL_Parent = AIB_PK
		JOIN dbo.GenPivot on XX_Relation1ID = SL_PK
		JOIN dbo.EDIMessage on XX_Relation2ID = EM_PK
	WHERE AIB_Status = 'SNT'
		AND GC_RN_NKCountryCode = 'KR'
		AND SL_SE_NKEvent in ('IAK', 'IRJ', 'DEX')
		AND XX_RelationType = 'XEM'
		AND SL_Table = 'AccEInvoicingBatch'
		AND AIB_PK BETWEEN @startGuid AND @endGuid
),
Pivot_Transaction AS(
	SELECT 
		AIP_PK,
		AIP_AIB,
		AH_PK,
		AH1_Reference
	From dbo.AccEInvoicingTransactionPivot
		JOIN dbo.AccTransactionHeader on AH_PK = AIP_ParentID
		JOIN dbo.AccTransactionHeaderReference on AH1_AH = AH_PK
	WHERE AIP_RN_NKCountryCode = 'KR'
		AND AIP_Status = 'SUC'
		AND AH1_Type = 'KRI'
		AND AIP_AIB BETWEEN @startGuid AND @endGuid
		AND NOT EXISTS(
			SELECT 1 
			FROM dbo.AccTransactionHeaderAuthorisationRecord 
			WHERE AHF_ParentId = AH_PK 
			AND AHF_ParentTableCode = 'AH'
			AND AHF_RecordType = 'KRS'
		)
),
Batch_SubmitID AS(
	SELECT
		AIB_PK,
		AIB_BatchNumber,
		SL_SE_NKEvent,
		SL_EventTimeUtc,
		c.value('(gei:Value)[1]', 'varchar(50)') AS SubmitID
	FROM Batch_EDIMessage
		CROSS APPLY EM_MessageData_XML.nodes('/gei:GlobalElectronicInvoicing/gei:Header/gei:ElectronicInvoiceBatchRequest/gei:AdditionalDataItems/gei:AdditionalDataItem') AS t(c)
	WHERE SL_SE_NKEvent = 'DEX'
		AND EM_MessageData_XML.exist('/gei:GlobalElectronicInvoicing/gei:Header/gei:ElectronicInvoiceBatchRequest/gei:AdditionalDataItems/gei:AdditionalDataItem[gei:Key=""SubmitID""]') = 1
		AND c.value('(gei:Key)[1]', 'varchar(50)') = 'SubmitID'
),
Pivot_DocTaxInvoices AS(
	SELECT
		AIB_PK,
		AIB_BatchNumber,
		AIP_PK,
		AH_PK,
		AH1_Reference,
		SL_SE_NKEvent,
		SL_EventTimeUtc,
		c.value('(xut:Value)[1]', 'varbinary(max)') AS invoiceDoc,
		TRY_CAST(dbo.CLRUncompressAsString(c.value('(xut:Value)[1]', 'varbinary(max)')) AS XML) AS InvoiceDoc_XML
	FROM Batch_EDIMessage
		JOIN Pivot_Transaction PT on AIB_PK = AIP_AIB
		CROSS APPLY EM_MessageData_XML.nodes('/xut:UniversalEvent/xut:Event/xut:ContextCollection/xut:Context') AS t(c)
	WHERE SL_SE_NKEvent in ('IAK', 'IRJ')
		AND EM_MessageData_XML.exist('/xut:UniversalEvent/xut:Event/xut:ContextCollection/xut:Context[xut:Type=""EINV_DocTaxInvoice""]') = 1
		AND c.value('(xut:Type)[1]', 'varchar(50)') = 'EINV_DocTaxInvoice'
),
Pivot_MatchedTaxInvoice AS(
	SELECT
		AIB_PK,
		AIP_PK,
		AH_PK,
		AH1_Reference,
		SL_SE_NKEvent,
		SL_EventTimeUtc,
		InvoiceDoc
	FROM Pivot_DocTaxInvoices
		CROSS APPLY InvoiceDoc_XML.nodes('/inv:TaxInvoice/inv:TaxInvoiceDocument/inv:IssueID') AS t(c)
	WHERE c.value('(.)', 'varchar(50)') = AH1_Reference
)

INSERT INTO dbo.AccTransactionHeaderAuthorisationRecord (
	AHF_PK,
	AHF_RecordType,
	AHF_ParentId,
	AHF_ParentTableCode,
	AHF_Number,
	AHF_AuthorisationData,
	AHF_SystemCreateTimeUtc,
	AHF_SystemCreateUser,
	AHF_SystemLastEditTimeUtc,
	AHF_SystemLastEditUser
)
SELECT
	NEWID(),
	'KRS',
	p.AH_PK,
	'AH',
	b.SubmitID,
	InvoiceDoc,
	GETUTCDATE(),
	'E',
	GETUTCDATE(),
	'E'
FROM Batch_SubmitID b
	JOIN Pivot_MatchedTaxInvoice p
	ON b.AIB_PK = p.AIB_PK";

			using (var command = Db.Connection.Command(transformationSql))
			{
				command.AddParameter("@startGuid", SqlDbType.UniqueIdentifier, startGuid);
				command.AddParameter("@endGuid", SqlDbType.UniqueIdentifier, endGuid);

				command.ExecuteNonQuery();
			}
		}

		readonly int BatchSize;
		const string LastProcessedEInvoicingBatchPK = "MoveKoreaTaxInvoiceDocToAccTransactionHeaderAuthorisationRecord.LastProcessedEInvoicingBatchPK";
	}
}
