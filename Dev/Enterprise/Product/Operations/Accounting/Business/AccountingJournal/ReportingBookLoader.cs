using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ReportingBookLoader
{
	public static class AccountingJournalReportingBookLoader
	{
		public static DataTable LoadReportingBook(Guid reportingBookPK, ZDateTime startPostDate, ZDateTime endPostDate, IEnumerable<ZGuid> transactionHeaderPKs = null, IEnumerable<ZGuid> transactionLinePKs = null)
		{
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				var transactionHeaderPKsTVP = transactionHeaderPKs != null && transactionHeaderPKs.Any() ? transactionHeaderPKs.Select(x => x.ToGuid()).Distinct().ToArray() : Array.Empty<Guid>();
				var transactionLinePKsTVP = transactionLinePKs != null && transactionLinePKs.Any() ? transactionLinePKs.Select(x => x.ToGuid()).Distinct().ToArray() : Array.Empty<Guid>();
				var dbCommand = connection.Command($"SELECT * FROM [dbo].GetGeneralLedgerTransactionData('{GlbCompany.CurrentCompany.PK.ToGuid()}', '{reportingBookPK}', '{startPostDate}', '{endPostDate}', '', '', @TransactionHeaderPKsTVP, @TransactionLinePKsTVP)");
				dbCommand.AddTableValuedParameter("@TransactionHeaderPKsTVP", "dbo.TVP_uniqueidentifier", transactionHeaderPKsTVP);
				dbCommand.AddTableValuedParameter("@TransactionLinePKsTVP", "dbo.TVP_uniqueidentifier", transactionLinePKsTVP);
				return DataUtils.GetDataTableFromCommand(dbCommand);
			}
		}
	}
}
