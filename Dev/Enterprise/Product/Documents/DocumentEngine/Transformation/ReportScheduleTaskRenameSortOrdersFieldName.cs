using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.DocumentEngine.Transformation
{
	[CodeAlive("online transformation")]
	public class ReportScheduleTaskRenameSortOrdersFieldName
	{
		const string ReleaseDate = "ReleaseDate";
		internal const string SDName = "StmMenuItemPKList";

		#region IOnlineTransformation

		public string Description => (NoResString)"Rename SortOrders Name for Report Schedule Task";

		public void Run(Action<string> logAction, CancellationToken token)
		{
			if (!ShouldRun())
			{
				return;
			}

			using (EnvProxy.Instance.SuspendBranchAccessError())
			{
				RenameSortOrdersFieldName(logAction, token);
			}
			DeleteStmMenuItemPKListIfExists();
		}

		internal bool ShouldRun()
		{
			var script = $@"IF EXISTS (SELECT 1 FROM dbo.StmData WHERE SD_Name = '{SDName}') SELECT CAST(1 AS BIT) ELSE SELECT CAST(0 AS BIT);";
			return Db.Connection.ExecuteScalar<bool>(script);
		}

		#endregion

		void RenameSortOrdersFieldName(Action<string> logAction, CancellationToken token)
		{
			logAction(Description);

			var renamer = new ReportScheduleTaskFilterAndSortNamesRenamer(Db.Connection);
			var sortOrdersFieldListChanges = new Dictionary<string, string>() {
				{ ReleaseDate, JobDeclarationSchema.Constants.JE_EntryAuthorisationDate }
			};

			var list = ReadReportMenuItemPKs();
			if (list.Any())
			{
				token.ThrowIfCancellationRequested();

				renamer.Rename(GetReportsFilter(list), null, null, null, null, sortOrdersFieldListChanges, null, null,
					null, null, null, null);
			}
		}

		List<Guid> ReadReportMenuItemPKs()
		{
			var result = new List<Guid>();
			var sqlText = $@"SELECT CONVERT(NVARCHAR(MAX), SD_BinaryValue) FROM dbo.StmData WHERE SD_Name = '{SDName}'";
			var value = Db.Connection.ExecuteScalar(sqlText).ToString();

			var stringList = value.Split(';').ToList();
			return stringList.Select(ParseGuidSafe).ToList();
		}

		static Guid ParseGuidSafe(string input)
		{
			if (Guid.TryParse(input, out Guid result))
			{
				return result;
			}
			else
			{
				return Guid.Empty;
			}
		}

		IEnumerable<Tuple<SchemaColumn, object>> GetReportsFilter(List<Guid> list)
		{
			return new[]
			{
				Tuple.Create<SchemaColumn, object>(StmScheduleTaskSchema.S5_ParentTableCode, "SU"),
				Tuple.Create<SchemaColumn, object>(StmScheduleTaskSchema.S5_ParentID, list),
				Tuple.Create<SchemaColumn, object>(StmScheduleTaskSchema.S5_ScheduleType, "REP"),
			};
		}

		void DeleteStmMenuItemPKListIfExists()
		{
			var script = $@"DELETE FROM dbo.StmData WHERE SD_Name = '{SDName}'";
			Db.Connection.ExecuteNonQuery(script);
		}
	}
}
