using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public partial class ConsolidationBatchExporter
	{
		public ConsolidationBatchExporter(IEnumerable<ZGuid> consolidationBatchPKs, bool includeManualEliminationJournals)
		{
			this.ConsolidationBatchPKs = new List<ZGuid>(consolidationBatchPKs);
			this.includeManualEliminationJournals = includeManualEliminationJournals;
		}

		readonly List<ZGuid> ConsolidationBatchPKs;
		readonly bool includeManualEliminationJournals;
		const int TimeOut = 1800;

		public IEnumerable<ConsolidationBatchDetailsRow> Export()
		{
			var batchRows = new List<ConsolidationBatchDetailsRow>();

			using (var command = GetCommandForReadingBatchContents())
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var batchRow = new ConsolidationBatchDetailsRow();

					batchRow.Period = (int)reader["Period"];
					batchRow.GLAccount = (string)reader["GLAccount"];
					batchRow.BranchCode = (string)reader["BranchCode"];
					batchRow.DepartmentCode = (string)reader["DepartmentCode"];
					batchRow.ConsolidationGroupCode = (string)reader["ConsolidationGroupCode"];
					batchRow.PostingCompanyCode = (string)reader["PostingCompanyCode"];
					batchRow.TransactionOrganisationCode = (string)reader["TransactionOrganisationCode"];
					batchRow.PostingCompanyCurrency = (string)reader["PostingCompanyCurrency"];
					batchRow.AmountInPostingCompanyCurrency = (decimal)reader["AmountInPostingCompanyCurrency"];
					batchRow.TransactionCurrency = (string)reader["TransactionCurrency"];
					batchRow.AmountInTransactionCurrency = (decimal)reader["AmountInTransactionCurrency"];

					batchRows.Add(batchRow);
				}
			}

			return batchRows;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		DbCommand GetCommandForReadingBatchContents()
		{
			var command = Db.Connection.Command("EXEC GLConsolidationsExportData @ConsolidationBatchPKs, @IncludeManualEliminationJournals;", TimeOut); // This is a call to a stored proc, not something we're showing to the user.
			command.AddParameter("@ConsolidationBatchPKs", SqlDbType.VarChar, int.MaxValue, string.Join(",", ConsolidationBatchPKs.Select(x => x.ToString())));
			command.AddParameter("@IncludeManualEliminationJournals", SqlDbType.Char, 1, includeManualEliminationJournals ? "Y" : "N");
			return command;
		}
	}
}
