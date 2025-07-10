using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public class RomaniaEInvoicingBatchCreator : EInvoicingBatchCreatorBase
	{
		public RomaniaEInvoicingBatchCreator(GlbCompany company) : base(company)
		{
		}

		public override ZBool PerformBatching(ILogger serviceLogger)
		{
			serviceLogger.Log(LogType.Debug, "Batching sub task started.");

			var deliveredTransactionForCompany = GetDeliveredRecordsForCompany();
			if (deliveredTransactionForCompany.Any())
			{
				CreateBatchesAndUpdatePivotsForCompany(deliveredTransactionForCompany, serviceLogger, EInvoicingPivotState.Delivered);
				serviceLogger.Log(LogType.Information, "Batching sub task: Reset Status to Delivered completed.");
				return true;
			}

			var queuedTransactionsForCompany = GetAllTransactionsForCompany();
			if (queuedTransactionsForCompany.Any())
			{
				CreateBatchesAndUpdatePivotsForCompany(queuedTransactionsForCompany, serviceLogger);
				serviceLogger.Log(LogType.Information, "Batching sub task completed.");
				return true;
			}

			var batchedTransactionsForCompany = GetBatchedRecordsForCompany();
			if (batchedTransactionsForCompany.Any())
			{
				serviceLogger.Log(LogType.Information, "Batching sub task completed.");
				return true;
			}

			serviceLogger.Log(LogType.Debug, "Batching sub task completed - No transactions were available for batching.");
			return false;
		}

		DynamicBusinessObjectCollection GetBatchedRecordsForCompany()
		{
			var selectQuery = $@"SELECT AIP_PK FROM dbo.AccEInvoicingTransactionPivot
JOIN dbo.AccEInvoicingBatch ON AIB_PK = AIP_AIB
WHERE
	AIP_Status = '{EInvoicingPivotState.Delivered}'
	AND AIP_LastResponseReceivedUtc < @RestrictReceivedUtc
	AND AIB_Status = '{EInvoicingBatchState.Ready}'
	AND AIB_GC = @CompanyPK
";

			var sqlParameters = new List<ZSqlParameter>
			{
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
				ZSqlParameter.New("@RestrictReceivedUtc", ZDateTime.UtcNow.AddMinutes(AccountingElectronicMessagingRegistry.Instance.RomaniaEReportingDelayTimeForQueryInvoiceRequest.Value * -1), AccEInvoicingTransactionPivotSchema.AIP_LastResponseReceivedUtc),
			};

			var allWaitingPivotsForCompany = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			allWaitingPivotsForCompany.Load(selectQuery, sqlParameters.ToArray());

			return allWaitingPivotsForCompany;
		}

		DynamicBusinessObjectCollection GetDeliveredRecordsForCompany()
		{
			var query = $@"SELECT AIP_PK
FROM AccEInvoicingTransactionPivot
WHERE
	AIP_AIB IS NULL
	AND AIP_GC = @CompanyPK
	AND AIP_Status = '{EInvoicingPivotState.Delivered}'";
			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
			};

			var pivots = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			pivots.Load(query, sqlParameters);

			return pivots;
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?.Select(t => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { t })) ?? Enumerable.Empty<QueuedPivotPKs>();
		}
	}
}
