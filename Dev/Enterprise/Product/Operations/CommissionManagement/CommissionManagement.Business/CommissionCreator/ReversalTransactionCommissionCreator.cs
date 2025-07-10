using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class ReversalTransactionCommissionCreator : CommissionCreator, IReversalTransactionCommissionCreator
	{
		#region Constructor

		public ReversalTransactionCommissionCreator(ICommissionableTransaction transaction)
			: base(transaction.Factory)
		{
			Argument.NotNull(transaction, "transaction");

			this.Transaction = transaction;
		}

		protected readonly ICommissionableTransaction Transaction;

		#endregion

		#region CreateCommissions

		public override void CreateCommissions(CreateCommissionContext context)
		{
			var existingCommissionHeaderQuery = new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Transaction.PK);
			var existingCommissionHeader = Factory.LoadTop1<AccCommissionHeader>(existingCommissionHeaderQuery);
			if (existingCommissionHeader != null)
			{
				return;
			}

			var originalTransaction = Factory.Load<ITransactionHeader>(Transaction.AH_TransactionBelongsToGroup) as ICommissionableTransaction;
			if (originalTransaction == null)
			{
				return;
			}

			var commissionHeadersForOriginalTransactionQuery = new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, originalTransaction.PK);
			commissionHeadersForOriginalTransactionQuery.AddToFilter(AccCommissionHeaderSchema.CH0_OverridenDateTimeUtc, null);
			var commissionHeadersForOriginalTransaction = Factory.Load<AccCommissionHeader>(commissionHeadersForOriginalTransactionQuery);
			foreach (var commissionHeaderForOriginalTransaction in commissionHeadersForOriginalTransaction)
			{
				CreateReversalCommissions(commissionHeaderForOriginalTransaction);
			}
		}

		public void CreateReversalCommissions(IAccCommissionHeader originalCommissionHeader)
		{
			var reversalCommissionHeader = Factory.New<AccCommissionHeader>();
			reversalCommissionHeader.CH0_GC = originalCommissionHeader.CH0_GC;
			reversalCommissionHeader.CH0_AH_Source = Transaction.PK;
			reversalCommissionHeader.CH0_GroupingSourceTableCode = originalCommissionHeader.CH0_GroupingSourceTableCode;
			reversalCommissionHeader.CH0_GroupingSourceID = originalCommissionHeader.CH0_GroupingSourceID;
			reversalCommissionHeader.CH0_CA0 = originalCommissionHeader.CH0_CA0;
			reversalCommissionHeader.CH0_OH_Debtor = originalCommissionHeader.CH0_OH_Debtor;
			reversalCommissionHeader.CH0_OH_Customer = originalCommissionHeader.CH0_OH_Customer;
			reversalCommissionHeader.CH0_Product = originalCommissionHeader.CH0_Product;
			reversalCommissionHeader.CH0_Service = originalCommissionHeader.CH0_Service;
			reversalCommissionHeader.CH0_SubModule = originalCommissionHeader.CH0_SubModule;
			reversalCommissionHeader.CH0_CommissionDate = originalCommissionHeader.CH0_CommissionDate;
			reversalCommissionHeader.CH0_Mode = originalCommissionHeader.CH0_Mode;
			reversalCommissionHeader.CH0_NKOrigin = originalCommissionHeader.CH0_NKOrigin;
			reversalCommissionHeader.CH0_NKDestination = originalCommissionHeader.CH0_NKDestination;
			reversalCommissionHeader.CH0_JobNumber = originalCommissionHeader.CH0_JobNumber;

			reversalCommissionHeader.CH0_SnapshotDateTime = Transaction.AH_PostDate;
			reversalCommissionHeader.CH0_SnapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;

			CreateReversalLineGroups(reversalCommissionHeader, originalCommissionHeader);
			CreateReversalLines(reversalCommissionHeader, originalCommissionHeader);
		}

		void CreateReversalLineGroups(AccCommissionHeader reversalCommissionHeader, IAccCommissionHeader originalCommissionHeader)
		{
			foreach (var originalLineGroup in originalCommissionHeader.LineGroups)
			{
				var reversalLineGroup = reversalCommissionHeader.LineGroups.AddNew();
				reversalLineGroup.CLG_AC = originalLineGroup.CLG_AC;
				reversalLineGroup.CLG_RX_NKTransactionCurrency = originalLineGroup.CLG_RX_NKTransactionCurrency;
				reversalLineGroup.CLG_TransactionAmount = -originalLineGroup.CLG_TransactionAmount;
				reversalLineGroup.CLG_RX_NKCommissionCurrency = originalLineGroup.CLG_RX_NKCommissionCurrency;
				reversalLineGroup.CLG_TotalCommissionableAmount = -originalLineGroup.CLG_TotalCommissionableAmount;

				CreateReversalLines(reversalLineGroup, originalLineGroup);
			}
		}

		void CreateReversalLines(AccCommissionLineGroup reversalLineGroup, IAccCommissionLineGroup originalLineGroup)
		{
			foreach (var originalLine in originalLineGroup.Lines)
			{
				var reversalLine = reversalLineGroup.Lines.AddNew();
				AccCommissionLine.PopulateReversalLine(reversalLine, originalLine);
			}
		}

		void CreateReversalLines(AccCommissionHeader reversalCommissionHeader, IAccCommissionHeader originalCommissionHeader)
		{
			foreach (var originalLine in originalCommissionHeader.Lines)
			{
				var reversalLine = reversalCommissionHeader.Lines.AddNew();
				AccCommissionLine.PopulateReversalLine(reversalLine, originalLine);
			}
		}

		#endregion

		#region Static

		public static void CreateReversalTransactionCommissionsIfRequired(ICommissionableTransaction transaction)
		{
			if (transaction.AH_IsCancelled)
			{
				var factory = transaction.Factory;
				var reversalQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.PK);
				reversalQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, true);
				reversalQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
				var reversalTransaction = factory.LoadTop1<TransactionHeader>(reversalQuery) as ICommissionableTransaction;

				if (reversalTransaction != null)
				{
					new ReversalTransactionCommissionCreator(reversalTransaction).CreateCommissions();
				}
			}
		}

		public static void AddReversalTransactionFetchHints(BusinessObjectFactory factory, IEnumerable<ICommissionableTransaction> originalTransactions)
		{
			foreach (var transaction in originalTransactions)
			{
				if (transaction.AH_IsCancelled)
				{
					var reversalQuery = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transaction.PK);
					reversalQuery.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, true);
					reversalQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, transaction.AH_GC);
					factory.AddFetchHint(typeof(TransactionHeader), reversalQuery);
				}
			}
		}

		public static ZQuery GetIsNotReversalTransactionQuery()
		{
			var result = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, null);
			result.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_IsCancelled, false);
			return result;
		}

		#endregion
	}
}
