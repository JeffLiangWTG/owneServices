using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth
{
	public abstract class KoreaSouthEInvoicingBatchCreator : EInvoicingBatchCreatorBase
	{
		public KoreaSouthEInvoicingBatchCreator(GlbCompany company) : base(company)
		{
		}

		public abstract string ActionType { get; }

		protected override DynamicBusinessObjectCollection GetAllTransactionsForCompany(string[] actionTypes = null, string[] actionTypesWithTransactionID = null, bool queryJoinWithHeadersOnly = false)
		{
			var selectQuery = $@"
	SELECT AIP_PK, AIP_ActionType, AH_GB, AIP_ParentID, AH_Ledger, AH_TransactionNum, AH_TransactionType, OH_Code
	FROM dbo.AccEInvoicingTransactionPivot
	JOIN dbo.AccTransactionHeader ON AH_PK = AIP_ParentID
	JOIN dbo.OrgHeader ON OH_PK = AH_OH 
	WHERE AIP_Status = @Status AND AIP_GC = @CompanyPK AND AIP_ParentTableCode = '{AccTransactionHeaderSchema.Constants.Prefix}'
	AND AIP_ActionType = @ActionType";

			var sqlParameters = new[]
			{
				ZSqlParameter.New("@Status", Core.Constants.EInvoicingPivotState.Queued, AccEInvoicingTransactionPivotSchema.AIP_Status),
				ZSqlParameter.New("@CompanyPK", CurrentCompany.PK, AccEInvoicingTransactionPivotSchema.AIP_GC),
				ZSqlParameter.New("@ActionType", ActionType, AccEInvoicingTransactionPivotSchema.AIP_ActionType),
			};

			var allTransactionsForCompany = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			allTransactionsForCompany.Load(selectQuery, sqlParameters);

			return allTransactionsForCompany;
		}

		protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
		{
			if (transactions == null)
			{
				return Enumerable.Empty<QueuedPivotPKs>();
			}

			var otherPivots = new List<DynamicBusinessObject>();
			var submitPivots = new List<DynamicBusinessObject>();
			foreach (DynamicBusinessObject pivot in transactions)
			{
				if (pivot[AccEInvoicingTransactionPivotSchema.Constants.AIP_ActionType]?.ToString() == EInvoicingPivotActionType.Submit)
				{
					submitPivots.Add(pivot);
				}
				else
				{
					otherPivots.Add(pivot);
				}
			}

			var batchSize = EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(CurrentCompany, AccountingElectronicMessagingRegistry.MaximumBatchSizeSupportedInXUEProcessor);
			var otherBatches = otherPivots.Select(x => QueuedPivotPKs.FromDynamicBizOCollection(new DynamicBusinessObject[] { x }));
			var submitBatches = submitPivots.GroupBy(x => x[AccTransactionHeaderSchema.Constants.AH_GB])
											.SelectMany(x => GroupTransactionByUniqueIdentifierForBatching(x).Batch(batchSize))
											.Select(z => QueuedPivotPKs.FromDynamicBizOCollection(z));

			return otherBatches.Concat(submitBatches);
		}
	}
}
