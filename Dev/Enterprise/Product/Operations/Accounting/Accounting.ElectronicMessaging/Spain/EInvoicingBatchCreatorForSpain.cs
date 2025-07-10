using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Spain
{
	public sealed class EInvoicingBatchCreatorForSpain : EInvoicingBatchCreatorBase
	{
		public EInvoicingBatchCreatorForSpain(GlbCompany company, int maximumBatchSize = 1)
			: base(company)
		{
			MaximumBatchSize = EInvoicingBatchCreator.CalculateMaximumBatchSizeWithArchitecturalLimitation(maximumBatchSize);
		}

		protected override DynamicBusinessObjectCollection GetAllTransactionsForCompany(
			string[] actionTypes = null,
			string[] actionTypesJoiningTransactionHeader = null,
			bool queryJoinWithHeadersOnly = false)
		{
			return base.GetAllTransactionsForCompany(
				actionTypes: null,
				actionTypesJoiningTransactionHeader: new[] { EInvoicingPivotActionType.Submit, EInvoicingPivotActionType.Cancel },
				queryJoinWithHeadersOnly: true);
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			if (transactions == null || transactions.Count == 0)
			{
				yield break;
			}

			var pivotPKsByBranchAndLedger = transactions.GroupBy(x => new { GB_PK = (ZGuid)x["AH_GB"], AH_Ledger = (ZString)x["AH_Ledger"] });

			foreach (var g in pivotPKsByBranchAndLedger)
			{
				var groupedTransactions = GroupTransactionByUniqueIdentifierForBatching(g);
				var oneBatch = new QueuedPivotPKs();
				foreach (var x in groupedTransactions)
				{
					oneBatch.Items.Add(((ZGuid)x["AIP_PK"]).ToGuid());

					if (oneBatch.Items.Count >= MaximumBatchSize)
					{
						yield return oneBatch;
						oneBatch = new QueuedPivotPKs();
					}
				}
				if (oneBatch.Items.Count > 0)
				{
					yield return oneBatch;
				}
			}
		}

		public int MaximumBatchSize { get; }
	}
}
