using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Transformation
{
	public class ReportScheduleTaskRenameFilterFieldName
	{
		const string ReleaseDate = "ReleaseDate";
		const string FTZAdmissionDate = "FTZAdmissionDate";
		internal const string TempTableName = "ReportRenameFilterFieldNameList";

		#region IOnlineTransformation

		public string Description => (NoResString)"Rename Filter Name for Report Schedule Task";

		public void Run(Action<string> logAction, CancellationToken token)
		{
			if (!ShouldRun)
			{
				return;
			}

			using (EnvProxy.Instance.SuspendBranchAccessError())
			{
				RenameFilterFieldName(logAction, token);
			}
			DropTempTableIfExists();
		}

		internal bool ShouldRun
		{
			get { return DbObjectCreator.TableExists(Db.Connection, TempTableName); }
		}

		#endregion

		public void RenameFilterFieldName(Action<string> logAction, CancellationToken token)
		{
			logAction(Description);

			var renamer = new ReportScheduleTaskFilterAndSortNamesRenamer(Db.Connection);
			var filterFieldNameChanges = new Dictionary<string, string>() {
				{ ReleaseDate, JobDeclarationSchema.Constants.JE_EntryAuthorisationDate },
				{ FTZAdmissionDate, JobDeclarationSchema.Constants.JE_EntryAuthorisationDate }
			};

			var list = ReadReportMenuItemPKs();
			if (list.Any())
			{
				token.ThrowIfCancellationRequested();

				renamer.Rename(GetReportsFilter(list), null, null, filterFieldNameChanges, null, null, null, null,
					null, null, null, null);
			}
		}

		List<Guid> ReadReportMenuItemPKs()
		{
			var result = new List<Guid>();
			var factory = new BusinessObjectFactory();
			var sql = $@"SELECT PK FROM dbo.{TempTableName}";
			var menuItemPKs = new DynamicBusinessObjectCollection(factory);
			menuItemPKs.Load(sql);

			return menuItemPKs.Select(x => Guid.Parse(x["PK"].ToString())).ToList();
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

		void DropTempTableIfExists()
		{
			DbObjectCreator.DropTableIfExists(Db.Connection, TempTableName);
		}
	}
}
