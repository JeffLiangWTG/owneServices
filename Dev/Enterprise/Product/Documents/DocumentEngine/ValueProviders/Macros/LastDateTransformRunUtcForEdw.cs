using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class LastDateTransformRunUtcForEdw : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<LastDateTransformRunUtcForEdw>",
				ResString.GetMultilingualString("8ea31648-dc6f-4513-ba86-458afd7se5cd", "Return the Last Date Transform Run UTC For EDW Report."),
				new List<(string example, object expectedResult)> { ("<LastDateTransformRunUtcForEdw>", "2024-01-01 00:00") });
		}

		#region GetReplacement

		protected override object GetReplacementCore(string macro, Report report)
		{
			return GetLastDateTransformRunUtcForEdw();
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public string GetLastDateTransformRunUtcForEdw()
		{
			var edwServerName = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			var edWConnection = !string.IsNullOrEmpty(edwServerName) ? Db.NewExtraConnectionWithMainDbCredentials(edwServerName, Db.EdwDatabaseName) : null;
			if(edWConnection == null || !IsDataWarehouseServiceTaskActiveAndEnabled())
			{
				return string.Empty;
			}

			var lastDateTransformRunUtcForEdwSql = string.Format($"SELECT CONVERT(varchar(16), [ParamValue], 120) AS LastDateTransformRunUtc FROM [{Db.EdwDatabaseName}].[biadmin].[MasterState] WHERE [ParamName] = 'LAST_DATE_TRANSFORM_RUN_UTC'");
			var cmd = edWConnection.Command(lastDateTransformRunUtcForEdwSql);
			var result = cmd.ExecuteScalar();
			return result == null ? string.Empty : (string)result;
		}

		bool IsDataWarehouseServiceTaskActiveAndEnabled()
		{
			const string EDWExecutionTaskCode = "EET";
			var serviceTaskBizo = new BusinessObjectFactory().LoadTop1<StmScheduleTask>(new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, EDWExecutionTaskCode));
			var forcefullyDisabledTasksSet = new HashSet<string>();
			forcefullyDisabledTasksSet = new HashSet<string>(
				  SystemDataRegistry.Instance.ForcefullyDisabledTasks.Value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
				   ?? Array.Empty<string>());

			return serviceTaskBizo != null && serviceTaskBizo.S5_IsActive && !forcefullyDisabledTasksSet.Contains(EDWExecutionTaskCode);
		}

		#endregion

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)Last(?:[\s]*)Date(?:[\s]*)Transform(?:[\s]*)Run(?:[\s]*)Utc(?:[\s]*)For(?:[\s]*)Edw(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
