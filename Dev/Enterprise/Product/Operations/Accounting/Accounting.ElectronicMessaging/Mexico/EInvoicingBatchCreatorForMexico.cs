using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public class EInvoicingBatchCreatorForMexico : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForMexico(GlbCompany company)
			: base(company)
		{
		}

		protected override bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
					return true;
				case Constants.EInvoicingPivotState.Discarded:
				case "":
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				case Constants.EInvoicingPivotState.BatchedWithError:
				case Constants.EInvoicingPivotState.Failed:
					UpdatePivotStateAndBatchStateByTransactionDate(pivotId, parentId);
					return false;
				default:
					return false;
			}
		}

		protected override bool AmendingPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);
			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
					return true;
				case Constants.EInvoicingPivotState.Discarded:
				case "":
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				case Constants.EInvoicingPivotState.BatchedWithError:
				case Constants.EInvoicingPivotState.Failed:
					UpdatePivotStateAndBatchStateByTransactionDate(pivotId, parentId);
					return false;
				default:
					return false;
			}
		}

		void UpdatePivotStateAndBatchStateByTransactionDate(Guid pivotId, Guid parentId)
		{
			var transactionDate = DateTime.MinValue;
			var transactionBranchPk = Guid.Empty;

			var sqlCommand = "SELECT AH_InvoiceDate, AH_GB FROM dbo.AccTransactionHeader WHERE AH_PK = @parentId";
			using (var selectCommand = Db.Connection.Command(sqlCommand)) // Not using factories because of performance for batch operations
			{
				selectCommand.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId);

				using (var reader = selectCommand.ExecuteReader())
				{
					while (reader.Read())
					{
						transactionDate = (DateTime)reader["AH_InvoiceDate"];
						transactionBranchPk = (Guid)reader["AH_GB"];
					}
				}
			}

			var transBranch = CurrentCompany.Factory.Load<IGlbBranch>(transactionBranchPk);
			var transactionDateUtc = transBranch.HomeTimeZone?.ToUniversalTime(transactionDate) ?? ZDateTime.Empty;

			if ((ZDateTime.UtcNow - transactionDateUtc).TotalDays > 4)
			{
				UpdatePivotStateAndBatchState(pivotId);
			}
		}
	}
}
