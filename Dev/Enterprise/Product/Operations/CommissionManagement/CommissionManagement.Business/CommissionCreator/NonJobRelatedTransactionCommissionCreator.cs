using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class NonJobRelatedTransactionCommissionCreator : TransactionCommissionCreator
	{
		#region New

		public static NonJobRelatedTransactionCommissionCreator New(ICommissionableTransaction transaction, DataTable effectiveDateCacheTable = null)
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden(transaction) : new NonJobRelatedTransactionCommissionCreator(transaction, effectiveDateCacheTable);
		}

		protected delegate NonJobRelatedTransactionCommissionCreator NewDelegate(ICommissionableTransaction transaction);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#endregion

		#region Constructor

		protected NonJobRelatedTransactionCommissionCreator(ICommissionableTransaction transaction, DataTable effectiveDateCacheTable = null)
			: base(transaction, effectiveDateCacheTable)
		{
			Argument.NotNull(transaction, "transaction");

			if (transaction.IsJobRelated)
			{
				throw new ArgumentException(ZString.Format("Must be non job-related transaction, but passed in transaction (PK:{0}) with AH_JH:{1}", transaction.PK, transaction.AH_JH));
			}
			else if (transaction.AH_TransactionBelongsToGroup.IsValid && transaction.AH_IsCancelled)
			{
				throw new ArgumentException(ZString.Format("Passed in transaction (PK:{0}) which is a reversal. Use {1} to create commission for reversal transactions", transaction.PK, nameof(ReversalTransactionCommissionCreator)));
			}
		}

		#endregion

		public sealed override void CreateCommissions(CreateCommissionContext context)
		{
			if (!Transaction.AH_IsCancelled)
			{
				var customerPk = Transaction.AH_OH;
				var product = OrgCommissionAgreementItemLookups.AllProductsCode;
				var service = OrgCommissionAgreementItemLookups.AllServicesCode;
				var subModule = OrgCommissionAgreementItemLookups.AllSubModulesCode;
				var mode = OrgCommissionAgreementItemLookups.AllModesCode;
				var commissionDate = MainTransaction.AH_PostDate.Date;
				var commissionDateByChargeDictionary = new Dictionary<ZGuid, ZDate>();
				commissionDateByChargeDictionary[ZGuid.Empty] = commissionDate;
				var snapshotDateTime = Transaction.AH_PostDate;
				var snapshotEventCode = context.RegeneratingCommissions ? AccCommissionHeaderSnapshotEventList.Codes.Regenerated : AccCommissionHeaderSnapshotEventList.Codes.Posted;
				PopulateEffectiveDateForTriggerType(customerPk);

				var builder = GetNewCommissionHeaderBuilder(context);
				var buildItemArgs = new CommissionHeaderBuildItemArgs(Transaction, MainTransaction as BusinessObject, customerPk, product, service, subModule, commissionDateByChargeDictionary, snapshotDateTime, snapshotEventCode, mode, "", "");
				var buildItem = GetNewCommissionHeaderBuildItem(context, buildItemArgs);
				builder.Create(buildItem);
			}
			else
			{
				ReversalTransactionCommissionCreator.CreateReversalTransactionCommissionsIfRequired(Transaction);
			}
		}

		protected virtual CommissionHeaderBuilder GetNewCommissionHeaderBuilder(CreateCommissionContext context)
		{
			return new CommissionHeaderBuilder(context.OverrideFactory ?? Factory, context);
		}

		protected virtual CommissionHeaderBuildItem GetNewCommissionHeaderBuildItem(CreateCommissionContext context, CommissionHeaderBuildItemArgs itemArgs)
		{
			return new CommissionHeaderBuildItem(context, itemArgs, GetCreatePercentageCommissionLineGroupsDelegate(Transaction.Lines.Cast<TransactionLine>(), itemArgs.CommissionDateByChargeDictionary), effectiveDateCacheTable);
		}

		public static ZDBOnlyQuery GetNonJobRelatedTransactionsQuery()
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddToFilter(TransactionCommissionCreator.GetIsCommissionableTransactionQuery());
			result.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
			var doesNotContainJobApportionedLineSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH, true);
			doesNotContainJobApportionedLineSubQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, SQLComparisonOperator.NotEqual, null);
			result.AddSubQuery(doesNotContainJobApportionedLineSubQuery, JoinCondition.And);

			return result;
		}
	}
}
