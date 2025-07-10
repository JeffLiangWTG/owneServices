using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public class GLJournalLineGLDDeleter : SaveInTransactionActionWithMainConnection
	{
		public GLJournalLineGLDDeleter(ZGuid companyPK)
		{
			this.companyPK = companyPK;
		}

		static string TVPToDeleteGLJournalGLD => "@toDeleteGLDGLJournalLinePKs";
		static string UniqueidentifierTVPName => "dbo.TVP_uniqueidentifier";

		readonly ZGuid companyPK;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "<Pending>")]
		protected override IChangedTableNames SaveInTransaction()
		{
			var companyParameter = (NoResString)"@company";
			var sql = $"DELETE dbo.AccGeneralLedgerData WHERE GLD_GC_Company = {companyParameter} AND GLD_AL_TransactionLine IN (SELECT Value FROM {TVPToDeleteGLJournalGLD})";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddTableValuedParameter(TVPToDeleteGLJournalGLD, UniqueidentifierTVPName, ToDeleteGLDTransactionLinePKs);
				cmd.AddParameter(companyParameter, SqlDbType.UniqueIdentifier, companyPK.ToGuid());

				cmd.ExecuteNonQuery();
			}

			return new ChangedTableNames(new[] { "AccGeneralLedgerData" });
		}

		public bool HasDeletedGLJournalLine => ToDeleteGLDTransactionLinePKs.Any();

		public void AddDeletedGLJournalLinePK(ZGuid linePK)
		{
			ToDeleteGLDTransactionLinePKs.Add(linePK.ToGuid());
		}

		HashSet<Guid> ToDeleteGLDTransactionLinePKs => fToDeleteGLDTransactionLinePKs ?? (fToDeleteGLDTransactionLinePKs = new HashSet<Guid>());
		HashSet<Guid> fToDeleteGLDTransactionLinePKs;
	}
}
