using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public sealed class EInvoicingBatchCreatorForSaudiArabia : EInvoicingBatchCreatorBase
	{
		public EInvoicingBatchCreatorForSaudiArabia(GlbCompany company) : base(company)
		{
		}

		protected override DynamicBusinessObjectCollection GetAllTransactionsForCompany(
			string[] actionTypes = null,
			string[] actionTypesJoiningTransactionHeader = null,
			bool queryJoinWithHeadersOnly = false)
		{
			var topTranText = "";
			if (IsThrottlingEnabledForCompany)
			{
				topTranText = FormattableString.Invariant($"TOP {MaximumNumberOfInvoicesThatCanBeProcessedSimultaneously}");
			}

			var sqlParameters = new List<ZSqlParameter>()
			{
				ZSqlParameter.New("@Status", EInvoicingPivotState.Queued, AccEInvoicingTransactionPivotSchema.AIP_Status),
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
			};

			var collectedQuery = FormattableString.Invariant($@"SELECT {topTranText} AIP_PK, AIP_GC, AIP_ActionType, AIP_ParentID, AH_GovernmentAllocatedID = '', AH_GB = CONVERT(uniqueidentifier, 0x), AH_Ledger = '', AH_TransactionType = ''
FROM dbo.AccEInvoicingTransactionPivot WHERE AIP_Status = @Status AND AIP_GC = @CompanyPK"); // SQL query

			var allTransactionsForCompany = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			allTransactionsForCompany.Load(collectedQuery, sqlParameters.ToArray());
			return allTransactionsForCompany;
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			return transactions?.Select(t => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { t })) ?? Enumerable.Empty<QueuedPivotPKs>();
		}
	}
}
