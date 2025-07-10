using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public class DepositBatchCollection : TransactionHeaderCollection
	{
		public DepositBatchCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Overrides

		public new DepositBatch this[int i]
		{
			get { return (DepositBatch)Elements[i]; }
		}

		public new DepositBatch AddNew()
		{
			return (DepositBatch)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Create Deposit Batches

		public void CreateDepositBatches(DepositBatchParent depositBatchParent)
		{
			RemoveAndDeleteAll();

			if (depositBatchParent != null)
			{
				string sql = ConstructSQLString(depositBatchParent);

				ZSqlParameterCollection @params = new ZSqlParameterCollection();
				@params.Add(ZSqlParameter.New("@ReceiptBatchNo", "", AccTransactionHeaderSchema.AH_ReceiptBatchNo));
				@params.Add(ZSqlParameter.New("@IsCancelled", true, AccTransactionHeaderSchema.AH_IsCancelled));
				@params.Add(ZSqlParameter.New("@Receipt", TransactionTypes.Receipt, AccTransactionHeaderSchema.AH_TransactionType));
				@params.Add(ZSqlParameter.New("@DirectReceipt", TransactionTypes.DirectReceipt, AccTransactionHeaderSchema.AH_TransactionType));
				@params.Add(ZSqlParameter.New("@CompanyPK", GlbCompany.CurrentCompany.PK, AccBankAccountSchema.AB_GC));

				if (depositBatchParent.FilterByBranch)
				{
					@params.Add(ZSqlParameter.New("@BranchPK", depositBatchParent.BranchFilterPK, AccTransactionHeaderSchema.AH_GB));
				}

				if (depositBatchParent.FilterByForeignCurrency || depositBatchParent.FilterByLocalCurrency)
				{
					@params.Add(ZSqlParameter.New("@CurrentCurrencyNK", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, AccBankAccountSchema.AB_RX_NKAccountCurrency));
				}

				DynamicBusinessObjectCollection banks = new DynamicBusinessObjectCollection(Factory);
				banks.Load(sql, @params);

				for (int loop = 0; loop < banks.Count; loop++)
				{
					DepositBatch newLine = Factory.New<DepositBatch>();
					newLine.AH_AB = (ZGuid)banks[loop][AccBankAccountSchema.PK.Name];
					newLine.LoadTransactions(depositBatchParent.FilterByBranch ? depositBatchParent.BranchFilterPK : ZGuid.Empty);
					newLine.AH_PostDate = depositBatchParent.DepositPostDate;
					newLine.AH_InvoiceDate = depositBatchParent.DepositDate;
					this.Add(newLine);
				}
			}
		}

		#endregion

		#region Implementation

		ZString ConstructSQLString(DepositBatchParent depositBatchParent)
		{
			string sQL = @"SELECT " + AccBankAccountSchema.PK.Name +
					" FROM " + AccBankAccountSchema.Constants.SqlSchemaName + "." + AccBankAccountSchema.Constants.TableName +
					" WHERE " + AccBankAccountSchema.PK.Name + " IN" +
					" (SELECT DISTINCT " + AccTransactionHeaderSchema.AH_AB.Name +
					" FROM " + AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName +
					" WHERE (" + AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name + " = @ReceiptBatchNo" +
					" AND NOT(" + AccTransactionHeaderSchema.AH_ReceiptBatchNo.Name + " = @ReceiptBatchNo" +
					" AND " + AccTransactionHeaderSchema.AH_IsCancelled.Name + " = @IsCancelled))" +
					" AND (" + AccTransactionHeaderSchema.AH_TransactionType.Name + " IN (@Receipt, @DirectReceipt))";

			if (depositBatchParent.FilterByBranch)
			{
				sQL += " AND " + AccTransactionHeaderSchema.AH_GB.Name + " = @BranchPK";
			}

			sQL += ") AND " + AccBankAccountSchema.AB_GC.Name + " = @CompanyPK";

			if (depositBatchParent.FilterByForeignCurrency)
			{
				sQL += " AND " + AccBankAccountSchema.AB_RX_NKAccountCurrency.Name + " <> @CurrentCurrencyNK";
			}
			else if (depositBatchParent.FilterByLocalCurrency)
			{
				sQL += " AND " + AccBankAccountSchema.AB_RX_NKAccountCurrency.Name + " = @CurrentCurrencyNK";
			}

			return sQL;
		}

		#endregion
	}
}
