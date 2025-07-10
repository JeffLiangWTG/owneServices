using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AmnestyReporter.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	AmnestiesProcessorServiceTask.Code,
	"Amnesty Errors Manager Processor",
	"CSP",
	typeof(AmnestiesProcessorServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "5Minutes",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.ServiceTask
{
	class AmnestiesProcessorServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			var amnestyFailures = Download();
			var filtered = FilterAndTranformWorkItems(amnestyFailures);
			Process(filtered, token);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal IEnumerable<DatAmnestyFailure> Download()
		{
			var result = new List<DatAmnestyFailure>();
			// read from Crikey DB
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				if (connection == null)
				{
					ServiceLogger.Error("Crikey db server not configured, this is probably not the production instance of ediProd");
					return Enumerable.Empty<DatAmnestyFailure>();
				}

				using (var cmd = connection.Command(DownloadSQL))
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new DatAmnestyFailure
						{
							AF_PK = reader.GetValue<Guid>("AF_PK"),
							AF_StartDate = reader.GetValue<DateTime>("AF_StartDate"),
							AF_IM = reader.GetValue<Guid?>("AF_IM"),
							EDI_IM = reader.GetValue<Guid?>("EDI_IM"),
							Grouping = reader.GetValue<string>("Grouping_Node"),
							E6_PK = reader.GetValue<Guid>("E6_PK"),
							E6_MethodName = reader.GetValue<string>("E6_MethodName"),
							E2_TestClass = reader.GetValue<string>("E2_TestClass"),

							E8_AssemblyName = reader.GetValue<string>("E8_AssemblyName"),
							ST_ResponsibleUser = reader.GetValue<string>("ST_User_Responsible"),
							ST_Product = reader.GetValue<string>("ST_Product"),
							ST_ProductArea = reader.GetValue<string>("ST_Product_Area"),
							ST_Module = reader.GetValue<string>("ST_Module")
						});
					}
				}
			}
			return result;
		}

		internal IEnumerable<DatAmnestyFailure> FilterAndTranformWorkItems(IEnumerable<DatAmnestyFailure> downloaded)
		{
			var filtered = new List<DatAmnestyFailure>();
			var failuresToVerify = new List<DatAmnestyFailure>();
			var wiPks = new HashSet<ZGuid>();
			foreach (var af in downloaded)
			{
				if (!af.EDI_IM.HasValue)
				{
					filtered.Add(af);
				}
				else
				{
					failuresToVerify.Add(af);
					wiPks.Add(new ZGuid(af.EDI_IM.Value));
				}
			}
			var workItems = OpenOrAssignedWorkItems(new BusinessObjectFactory(), wiPks);
			var verified = failuresToVerify.GroupBy(af => af.AF_PK).Select(g => VerifyWorkItem(g.ToList(), workItems));
			return filtered.Concat(verified).ToList();
		}

		IDictionary<Guid, NewWorkItem> OpenOrAssignedWorkItems(BusinessObjectFactory factory, IEnumerable<ZGuid> wiPks)
		{
			var query = new ZQuery(WorkItemSchema.PK, wiPks)
				.AddToFilter(WorkItemSchema.WKI_Status, new[]
					{
						ProcessTaskStatusCodeList.Codes.Open,
						ProcessTaskStatusCodeList.Codes.Assigned
					});
			return factory.Load<NewWorkItem>(query).ToDictionary(wki => wki.PK.ToGuid());
		}

		DatAmnestyFailure VerifyWorkItem(IEnumerable<DatAmnestyFailure> amnestyFailures, IDictionary<Guid, NewWorkItem> openOrAssigned)
		{
			DatAmnestyFailure clone = null;
			foreach (var amnestyFailure in amnestyFailures)
			{
				clone = amnestyFailure.Clone();
				clone.EDI_IM = openOrAssigned.TryGetValue(amnestyFailure.EDI_IM.Value, out NewWorkItem result) ? result.PK.ToGuid() : null;
				if (clone.EDI_IM.HasValue)
				{
					return clone;
				}
			}
			return clone;
		}

		internal static string JoinGuidsToSafeSqlInClause<T>(IEnumerable<T> source, Func<T, Guid> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}

			return string.Join(",", source.Select(selector).Concat(new[] { Guid.Empty }).Select(guid => string.Format(CultureInfo.InvariantCulture, "'{0}'", guid.ToString())));
		}

		void Process(IEnumerable<DatAmnestyFailure> datBundle, CancellationToken token) => Process(datBundle, TimeSpan.FromSeconds(5), token);

		internal void Process(IEnumerable<DatAmnestyFailure> datBundle, TimeSpan saveEvery, CancellationToken token)
		{
			if (datBundle == null)
			{
				throw new ArgumentNullException(nameof(datBundle));
			}

			var processData = InitProcess();
			var workItemToAmnesties = new Dictionary<ZGuid, List<Guid>>();
			var groupingToWorkItems = new Dictionary<string, ZGuid>();

			foreach (var datAmnestyFailure in datBundle.Where(datAmnestyFailure => datAmnestyFailure != null))
			{
				token.ThrowIfCancellationRequested();

				ServiceLogger?.Log(LogType.Information, string.Format(CultureInfo.InvariantCulture, "Processing Amnesty Failure '{0}'.", datAmnestyFailure.AF_PK));

				if (string.IsNullOrEmpty(datAmnestyFailure.ST_Product))
				{
					ServiceLogger?.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Missing product code for Amnesty Failure {0}/{1}/{2}).", datAmnestyFailure.E8_AssemblyName, datAmnestyFailure.E2_TestClass, datAmnestyFailure.E6_MethodName));
					continue;
				}

				if (string.IsNullOrEmpty(datAmnestyFailure.ST_Module))
				{
					ServiceLogger?.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Missing product module for Amnesty Failure {0}/{1}/{2}).", datAmnestyFailure.E8_AssemblyName, datAmnestyFailure.E2_TestClass, datAmnestyFailure.E6_MethodName));
				}

				if (string.IsNullOrEmpty(datAmnestyFailure.ST_ProductArea))
				{
					ServiceLogger?.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Missing product area for Amnesty Failure {0}/{1}/{2}).", datAmnestyFailure.E8_AssemblyName, datAmnestyFailure.E2_TestClass, datAmnestyFailure.E6_MethodName));
				}

				groupingToWorkItems.TryGetValue(datAmnestyFailure.Grouping, out var workItemPKHint);
				// Create a work item and get its PK
				var workItemPk = new AmnestyException(processData.Factory, datAmnestyFailure, workItemPKHint).Process();
				if (workItemPk.IsValid)
				{
					if (workItemToAmnesties.ContainsKey(workItemPk))
					{
						workItemToAmnesties[workItemPk].Add(datAmnestyFailure.AF_PK);
					}
					else
					{
						workItemToAmnesties.Add(workItemPk, new List<Guid>(new[] { datAmnestyFailure.AF_PK }));
					}
					groupingToWorkItems[datAmnestyFailure.Grouping] = workItemPk;
					processData.ProcessedItemsCount++;
				}

				if (processData.StartingTime.Elapsed > saveEvery)
				{
					SaveProcess(processData, workItemToAmnesties);
					processData = InitProcess();
				}
			}

			SaveProcess(processData, workItemToAmnesties);
		}

		/// <summary>
		/// Init new fabric, log, etc. for <see cref="Process" />.
		/// </summary>
		/// <returns>Newly created data</returns>
		static ProcessData InitProcess()
		{
			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			return new ProcessData
			{
				Factory = factory,
				StartingTime = stopwatch,
				ProcessedItemsCount = 0
			};
		}

		/// <summary>
		/// Save current process data from <see cref="Process" />
		/// </summary>
		/// <param name="processData">Processing data to save to main DB</param>
		/// <param name="workItemToAmnesties">Data to save to Crikey DB</param>
		void SaveProcess(ProcessData processData, Dictionary<ZGuid, List<Guid>> workItemToAmnesties)
		{
			var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(Factory);
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var processTime = processData.StartingTime.Elapsed;
				ServiceLogger?.Log(LogType.Information,
					string.Format(CultureInfo.InvariantCulture, "Processed amnesty failures: {0} items in {1} second(s).",
						processData.ProcessedItemsCount.ToString(CultureInfo.InvariantCulture),
						processTime.TotalSeconds.ToString(CultureInfo.InvariantCulture)));

				var start = ZDateTime.UtcNow;

				// saving logs and workitems
				processData.Factory.Save();

				// saving to Crikey
				SaveWorkItemsToAmnesties(workItemToAmnesties);

				ServiceLogger?.Log(LogType.Information,
					string.Format(CultureInfo.InvariantCulture, "Saving amnesty failures: {0} items in {1} second(s).",
						workItemToAmnesties.Select(pair => pair.Value?.Count ?? 0).Sum().ToString(CultureInfo.InvariantCulture),
						(ZDateTime.UtcNow - start).TotalSeconds.ToString(CultureInfo.InvariantCulture)));
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}

		BusinessObjectFactory fFactory;

		/// <summary>
		/// Save results of assigned WorkItems to Crikey DB
		/// </summary>
		/// <param name="workItemToAmnestiesDictionary">WorkItems to Amnesty Fails</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		internal static void SaveWorkItemsToAmnesties(Dictionary<ZGuid, List<Guid>> workItemToAmnestiesDictionary)
		{
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			{
				foreach (var pair in workItemToAmnestiesDictionary)
				{
					using (var cmd = connection.Command(string.Format(CultureInfo.InvariantCulture, @"
UPDATE [dbo].[AmnestyFailures]
SET [AF_IM] = '{0}'
WHERE [AF_PK] IN ({1})", pair.Key, JoinGuidsToSafeSqlInClause(pair.Value, guid => guid))))
					{
						cmd.ExecuteNonQuery();
					}
				}
			}
		}

		/// <summary>
		/// Amnesty Error Processor
		/// </summary>
		public const string Code = "AEP";

		public readonly string DownloadSQL = @"
WITH [Tests] AS (
	SELECT 
		TestsMappedToSourceTree.* , 
		ST.[ST_User_Responsible],
		ST.[ST_Product],
		ST.[ST_Product_Area],
		ST.[ST_Module],
		CASE ISNULL(ST.ST_Amnesty_WI_Grouping, 'DLL')
			WHEN 'MET' THEN CAST(TestsMappedToSourceTree.[E6_PK] AS VARCHAR(512))
			WHEN 'CLS' THEN CAST(TestsMappedToSourceTree.[E2_PK] AS VARCHAR(512))
			WHEN 'DLL' THEN CAST(TestsMappedToSourceTree.[E8_PK] AS VARCHAR(512))
			WHEN 'STN' THEN ST.[ST_Path]
		END AS Grouping_Node
	FROM 
		(
			SELECT 
				TM.[E6_PK],
				TM.[E6_MethodName],
				TC.[E2_PK], 
				TC.[E2_TestClass], 
				TA.[E8_PK], 
				TA.[E8_AssemblyName], 
				TA.[E8_SourcePath],
				MAX([ST_Path]) AS ST_Path
			FROM [dbo].[TestMethod] TM	
			LEFT JOIN [dbo].[TestClass] TC ON TM.E6_E2 = TC.E2_PK
			LEFT JOIN [dbo].[Assembly] TA ON TC.E2_E8 = TA.E8_PK
			LEFT JOIN [dbo].[SourceTreeResponsibility] ON 
				TA.E8_SourcePath LIKE ST_Path + '/%' 
				OR TA.E8_SourcePath LIKE ST_Path + '?path=%' 
				OR TA.E8_SourcePath = ST_Path
			GROUP BY 
				TM.[E6_PK],
				TM.[E6_MethodName],
				TC.[E2_PK], 
				TC.[E2_TestClass], 
				TA.[E8_PK], 
				TA.[E8_AssemblyName], 
				TA.[E8_SourcePath]
		) TestsMappedToSourceTree
	LEFT JOIN [dbo].[SourceTreeResponsibility] ST ON TestsMappedToSourceTree.ST_Path = ST.ST_Path
) 

SELECT 
	Amnesties.[AF_PK], 
	T.Grouping_Node,
	NonExpiredExistingAmnesties.AF_IM AS EDI_IM, 
	Amnesties.[AF_StartDate], 
	Amnesties.[AF_ExpiryDate], 
	Amnesties.[AF_IM],
	T.[E6_PK], 
	T.[E6_MethodName],
	T.[E2_TestClass],
	T.[E8_AssemblyName],
	T.[E8_SourcePath],
	T.[ST_User_Responsible],
	T.[ST_Product],
	T.[ST_Product_Area],
	T.[ST_Module]
FROM [dbo].[AmnestyFailures] Amnesties
LEFT JOIN [Tests] T ON Amnesties.AF_E6 = T.E6_PK
LEFT JOIN 
	(
		SELECT 
			DISTINCT
			Grouping_Node, 
			AF_IM			
		FROM [dbo].[AmnestyFailures]
		LEFT JOIN [Tests] ON 
			AF_E6 = E6_PK
		WHERE 
			AF_IM IS NOT NULL 
			AND AF_ExpiryDate IS NULL			
	) NonExpiredExistingAmnesties ON 

	T.Grouping_Node = NonExpiredExistingAmnesties.Grouping_Node

WHERE
	Amnesties.[AF_IM] IS NULL
	AND Amnesties.[AF_ExpiryDate] IS NULL
";

		/// <summary>
		/// Small class with internal data to <see cref="AmnestiesProcessorServiceTask.Process" /> method. Created only in
		/// <see cref="InitProcess" />.
		/// </summary>
		class ProcessData
		{
			public BusinessObjectFactory Factory { get; set; }
			public Stopwatch StartingTime { get; set; }
			public int ProcessedItemsCount { get; set; }
		}
	}
}
