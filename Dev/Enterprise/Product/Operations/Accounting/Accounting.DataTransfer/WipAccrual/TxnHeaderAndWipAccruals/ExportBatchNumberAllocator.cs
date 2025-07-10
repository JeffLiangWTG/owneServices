using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer
{
	public class ExportBatchNumberAllocator : SaveInTransactionActionWithMainConnection
	{
		public ExportBatchNumberAllocator(AccountingTransactionsDataExporter exporter)
		{
			this.FilterProvider = exporter.FilterProvider;
			this.Exporter = exporter;
		}

		readonly AccountingTransactionsDataExporter Exporter;
		readonly TransactionExportFilterProvider FilterProvider;

		int NewBatchNumber;

		protected override IChangedTableNames SaveInTransaction()
		{
			if (FilterProvider.CurrentBatchNo == AccountingTransactionsDataExporter.EmptyBatchNumber && Exporter.IsTransactionsExistInBatch)
			{
				AccountingNumberFountainNonVoucher numberFountainWrapper = AccountingNumberFountainWrapperFactory.Instance.TransactionExportBatchNo;
				NewBatchNumber = ZInt.Parse(numberFountainWrapper.GetNext(this.connected));

#if DEBUG
				if (isDeleteTestInvoice)
				{
					var factory = new BusinessObjectFactory();
					ZQuery query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "TEST");
					query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					var header = factory.LoadTop1<AccTransactionHeader>(query);
					header.Delete();
					factory.Save();
				}
#endif

				AllocateBatchNumber();
			}

			return new ChangedTableNames(new[] { GenExportBatchSequenceSchema.Constants.TableName });
		}

		public int GetBatchNumberAndAllocateToTransactionsToBeExported()
		{
			BusinessObjectFactory.SaveTogether(this);
			return NewBatchNumber;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AllocateBatchNumber()
		{
			using (DbCommand cmd = this.connected.Connection.Command(
				"exec AccountingTransactionExportBatchCreation @CompanyCode,@BatchNumber,@HeadersBatchFilterPks,@WIPAccrualPostBatchFilterPKs,@WIPAccrualReverseBatchFilterPKs")
			) // May be a part of SQL expression.
			{
				cmd.AddParameter("@CompanyCode", System.Data.SqlDbType.VarChar, 3, Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_Code.ToString());
				cmd.AddParameter("@BatchNumber", System.Data.SqlDbType.VarChar, int.MaxValue, NewBatchNumber.ToString());

				cmd.AddParameter("@HeadersBatchFilterPks", System.Data.SqlDbType.VarChar, int.MaxValue, GetCommaDelimitedString(Exporter.InvoiceBatchFilterPks) + "," + GetCommaDelimitedString(Exporter.UnallocatedTransactionBatchFilterPks));
				cmd.AddParameter("@WIPAccrualPostBatchFilterPKs", System.Data.SqlDbType.VarChar, int.MaxValue, GetCommaDelimitedString(Exporter.WIPAccPostBatchFilterPks));
				cmd.AddParameter("@WIPAccrualReverseBatchFilterPKs", System.Data.SqlDbType.VarChar, int.MaxValue, GetCommaDelimitedString(Exporter.WIPAccReverseBatchFilterPks));

				int result = ((IDbCommand)cmd).ExecuteNonQuery();
				if (result == 0)
				{
					NewBatchNumber = AccountingTransactionsDataExporter.EmptyBatchNumber;
					throw new Exception("Allocate Batch Number failed.");
				}
			}
		}

		string GetCommaDelimitedString(List<ZGuid> batch)
		{
			if (batch.Count == 0)
			{
				return ZGuid.Empty.ToString();
			}

			StringBuilder builder = new StringBuilder();

			builder.Append(batch[0].ToString());
			for (int i = 1; i < batch.Count; i++)
			{
				builder.Append("," + batch[i].ToString());
			}
			return builder.ToString();
		}

#if DEBUG
		internal bool isDeleteTestInvoice;
#endif
	}
}
