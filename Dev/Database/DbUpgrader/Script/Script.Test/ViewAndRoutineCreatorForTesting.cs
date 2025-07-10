using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.Common.Data;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Script.Test
{
	sealed class ViewAndRoutineCreatorForTesting : ViewAndRoutineCreator
	{
		public ViewAndRoutineCreatorForTesting(string upgradingDb, DbConnection connection)
			: base(new ViewAndRoutineCreatorManager(), connection, upgradingDb)
		{
		}

		public DbRoutineScriptCollection ExpectedList;

		public ViewAndRoutineCreatorManager Manager => manager as ViewAndRoutineCreatorManager;

		protected override DbRoutineScriptCollection GetViewAndRoutineScriptCollection()
		{
			return ExpectedList ?? (ExpectedList = base.GetViewAndRoutineScriptCollection());
		}

		protected override List<string> SkipObjectNameList
		{
			get
			{
				var result = base.SkipObjectNameList;
				result.Add(DatabaseConstants.TriggerNameToBlockInsertUpdateDeleteForDocManager);
				return result;
			}
		}

		public DbRoutineScriptCollection GetViewAndRoutineScriptCollection_Exposed()
		{
			return GetViewAndRoutineScriptCollection();
		}

		public void CompareViewsAndRoutinesExposed(IEnumerable<IDbScript> expectedList, out IEnumerable<IDbScript> dropList, out IEnumerable<IDbScript> createList, out IEnumerable<IDbScript> refreshList)
		{
			CompareViewsAndRoutines(expectedList, out dropList, out createList, out refreshList);
		}

		public bool ObjectExists(string objName, string objSchema = "dbo")
		{
			var sql = string.Format("SELECT CONVERT(bit, CASE WHEN OBJECT_ID('[{0}].[{1}]') is NULL AND NOT EXISTS (SELECT NULL FROM sys.triggers WHERE name = '{1}') THEN 0 ELSE 1 END);", objSchema, objName);
			return (bool)upgConnection.ExecuteScalar(sql);
		}

		public bool IncludeDevelopmentOnlyScriptsOverride { get; set; }
		protected override bool IncludeDevelopmentOnlyScripts => IncludeDevelopmentOnlyScriptsOverride;

		public MockLogFullnessProvider LogFullnessProvider = new MockLogFullnessProvider();
		protected override IBacklogInfoProvider CreateBacklogInfoProvider() => LogFullnessProvider;

		public class ViewAndRoutineCreatorManager : DummyUpgradeManager
		{
			public override void ShowInfoMessage(string infoMessage)
			{
				InfoMessages.AppendLine(infoMessage);
			}

			public StringBuilder InfoMessages = new StringBuilder();
		}

		public class MockLogFullnessProvider : IBacklogInfoProvider
		{
			public long BacklogSizeForTesting { get; set; }

			public long AcceptableBacklog => 80L;

			public TimeSpan[] Timespans { get; } = new TimeSpan[5]
			{
			TimeSpan.FromMilliseconds(10.0),
			TimeSpan.FromMilliseconds(10.0),
			TimeSpan.FromMilliseconds(10.0),
			TimeSpan.FromMilliseconds(10.0),
			TimeSpan.FromMilliseconds(10.0)
			};

			public AttemptInGettingBacklog GetCurrentBacklog()
			{
				var backlogResult = new BacklogResult()
				{
					BacklogSize = BacklogSizeForTesting,
					BacklogDescription = string.Format(CultureInfo.InvariantCulture, "transaction log fullness (current: {0}%, acceptable: {1}%)", BacklogSizeForTesting, AcceptableBacklog)
				};

				AfterGetCurrentBacklog?.Invoke();
				return new AttemptInGettingBacklog(success: true, string.Empty, backlogResult);
			}

			public Action AfterGetCurrentBacklog;
		}
	}
}
