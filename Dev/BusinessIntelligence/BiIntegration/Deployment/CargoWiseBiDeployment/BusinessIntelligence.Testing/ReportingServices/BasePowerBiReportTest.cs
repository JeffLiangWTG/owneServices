namespace CargoWise.Bi.BusinessIntelligence.Testing.ReportingServices
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Xml;
	using CargoWise.Bi.Deployment.ReportingServices;
	using CargoWise.Bi.Registration;
	using CargoWise.Bi.Registration.PowerBi;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.Data.Testing;
	using Enterprise.Registry.Business;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
	using OpenQA.Selenium.Chrome;

	public abstract class BasePowerBiReportTest : BaseBusinessIntelligenceTest
	{
		#region UI Tests

		//[RequiresSoftware(RequiredSoftware.Sql2016OrLater | RequiredSoftware.SsasTabular2016OrLater | RequiredSoftware.PowerBi | RequiredSoftware.IsVM)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:BaseSourcePath", Justification = "Not running true test on DAT")]
		[UseSnapshotProtection]
		public void TestReportUIQueries()
		{
			if (TestingState.IsRunningOnDAT)
			{
				Assert(true);
			}
			else
			{
				var tempDir = Directory.CreateDirectory(Path.Combine(TestCase.ExecutableDirectory, "Power BI Trace"));
				try
				{
					using (SetupModelAndReport())
					using (CreateTrace(tempDir.FullName))
					using (var driver = new ChromeDriver(ChromeDriverPath))
					{
						driver.Navigate().GoToUrl(ReportUrl);
						Thread.Sleep(TimeSpan.FromSeconds(5));
					}

					IEnumerable<string> queries = GetTraceEventData(tempDir.FullName);
					CombineAssertions(() =>
					{
						AssertReportUIQueries(queries);
					});
				}
				finally
				{
					CleanupTraceFiles(tempDir.FullName);
				}
			}
		}

		#region Implementation

		protected abstract void AssertReportUIQueries(IEnumerable<string> queries);

		protected void AssertMeasureQuery(IEnumerable<string> queries, string tableName, string measureName)
		{
			var regex = new Regex(string.Format(@"MEASURE\s+'{0}'\s*\[{1}\]\s*=", tableName, measureName), RegexOptions.IgnoreCase);
			Assert($"Missing measure query for '{tableName}'[{measureName}].", queries.Any(q => regex.IsMatch(q)));
		}

		protected void AssertColumnQuery(IEnumerable<string> queries, string tableName, string columnName)
		{
			var regex = new Regex(string.Format(@"EVALUATE\s+TOPN\s*\((.|\s)*'{0}'\s*\[{1}\]\s*,\s*(0|1)", tableName, columnName), RegexOptions.IgnoreCase);
			Assert($"Missing column query for '{tableName}'[{columnName}].", queries.Any(q => regex.IsMatch(q)));
		}

		protected void AssertFilter(IEnumerable<string> queries, string tableName, string columnName, string expression)
		{
			var regex = new Regex(string.Format(@"FILTER\s*\(\s+KEEPFILTERS\s*\((.|\s)*?'{0}'\s*\[{1}\]\s*{2}\s*\)", tableName, columnName, expression.Replace(" ", @"\s+")), RegexOptions.IgnoreCase);
			Assert($"Missing filter expression query for \"'{tableName}'[{columnName}]{expression}\".", queries.Any(q => regex.IsMatch(q)));
		}

		IDisposable SetupModelAndReport()
		{
			SetBiServerRegistryItems();
			var disposableCube = SsasHelper.RestoreModelBackupIfRequiredForSingleTest(SsasServerConnection, $"{Db.DatabaseName}_{TabularModelSuffix}");
			PowerBiHelper.DeployReport(ReportName, ResourceName, ReportType, ReportTestFolderPath, TabularModelSuffix);

			disposableCube.AddDisposable(new DisposableAction(() =>
			{
				var deployer = new AnalyticsReportDeployer(null);
				deployer.DeleteCatalogItem(ReportTestFolderPath);
			}));
			return disposableCube;
		}

		void SetBiServerRegistryItems()
		{
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName);
			SystemDataRegistry.Instance.BiAnalysisServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName);
			SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReportBrowserUrl);
		}

		IDisposable CreateTrace(string directoryName)
		{
			const string traceId = "Power BI Trace";
			var cmd = string.Format(@"
<Create xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
  <ObjectDefinition>
    <Trace>
      <ID>{0}</ID>
      <Name>{0}</Name>
      <XEvent xmlns=""http://schemas.microsoft.com/analysisservices/2011/engine/300/300"">
        <event_session name=""{0}"" dispatchLatency=""0"" maxEventSize=""0"" maxMemory=""4"" memoryPartition=""none"" eventRetentionMode=""AllowSingleEventLoss"" trackCausality=""true"" xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
          <event package=""AS"" name=""QueryEnd"" />
          <target package=""package0"" name=""event_file"">
            <parameter name=""filename"" value=""{1}\{0}.xel"" />
            <parameter name=""max_file_size"" value=""10"" />
            <parameter name=""max_rollover_files"" value=""10"" />
            <parameter name=""increment"" value=""1"" />
          </target>
        </event_session>
      </XEvent>
    </Trace>
  </ObjectDefinition>
</Create>", traceId, directoryName);
			SsasHelper.ExecuteDaxQuery(Db.ServerName, TabularModelDatabaseName, cmd);

			return new DisposableAction(() =>
			{
				cmd = string.Format(@"
<Delete xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
  <Object>
    <TraceID>{0}</TraceID>
  </Object>
</Delete>", traceId);
				SsasHelper.ExecuteDaxQuery(Db.ServerName, TabularModelDatabaseName, cmd);
			});
		}

		IEnumerable<string> GetTraceEventData(string directoryName)
		{
			var commands = new List<string>();
			var eventFiles = Directory.GetFiles(directoryName, "*.xel").OrderBy(n => n);
			foreach (var eventFile in eventFiles)
			{
				var events = GetTraceEventDataFromFile(eventFile);
				foreach (var eventData in events)
				{
					if (eventData.DatabaseName == TabularModelDatabaseName && eventData.EventSubclass == 3)
					{
						commands.Add(eventData.TextData);
					}
				}
			}
			return commands.AsEnumerable();
		}

		List<TraceEvent> GetTraceEventDataFromFile(string filePath)
		{
			var traceEvents = new List<TraceEvent>();
			var sqlText = $"SELECT convert(xml, event_data) AS event_data FROM sys.fn_xe_file_target_read_file(N'{filePath}', null, null, null);";

			using (var adminConnection = Db.NewAdminConnection())
			{
				var events = DataUtils.GetListOfValuesFromQuery(adminConnection, sqlText);
				foreach (var eventXml in events)
				{
					traceEvents.Add(new TraceEvent(eventXml));
				}
			}

			return traceEvents;
		}

		void CleanupTraceFiles(string directoryName)
		{
			foreach (var traceFile in Directory.GetFiles(directoryName, "*.xel"))
			{
				File.Delete(traceFile);
			}
			Directory.Delete(directoryName);
		}

		protected abstract string TabularModelSuffix { get; }

		string TabularModelDatabaseName
		{
			get
			{
				return $"{Db.DatabaseName}_{TabularModelSuffix}";
			}
		}

		protected override string CubeName => "Model";

		const string ReportTestFolderPath = @"/TEST";

		string ReportUrl
		{
			get
			{
				return string.Format(@"{0}/powerbi{1}/{2}?rs:embed=true", ReportBrowserUrl, ReportTestFolderPath, ReportName.Replace(" ", "%20"));
			}
		}

		string ReportBrowserUrl
		{
			get
			{
				var reportBrowserUrl = SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value;
				if (!string.IsNullOrEmpty(reportBrowserUrl))
				{
					return reportBrowserUrl;
				}
				else
				{
					return string.Format(@"http://{0}/PBIRS", Environment.MachineName);
				}
			}
		}

		string ResourceName
		{
			get
			{
				return "CargoWise.Bi.Registration.PowerBi.ReportFiles." + BusinessArea + "." + ReportName + ".pbix";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		string ChromeDriverPath
		{
			get
			{
				return Path.Combine(BaseSourcePath, "bin");
			}
		}

		class TraceEvent
		{
			public TraceEvent(string eventXml)
			{
				var document = new XmlDocument();
				document.LoadXml(eventXml);

				DatabaseName = document.SelectSingleNode(@"//data[@name=""DatabaseName""]/value").InnerText;
				EventSubclass = Convert.ToInt32(document.SelectSingleNode(@"//data[@name=""EventSubclass""]/value").InnerText);
				TextData = document.SelectSingleNode(@"//data[@name=""TextData""]/value").InnerText;
			}

			public string DatabaseName { get; }
			public int EventSubclass { get; }
			public string TextData { get; }
		}

		#endregion

		#endregion

		#region Measures Tests

		#region Test Cases

		public void TestHasTestsForAllMeasureGroups()
		{
			var reportTestType = GetType();
			var actualMeasureGroupTests = reportTestType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(m => m.Name.StartsWith("Test") && m.DeclaringType.FullName == reportTestType.FullName)
				.Select(m => m.Name).OrderBy(x => x);

			var expectedMeasureGroupTests = ExpectedMeasureGroups.MeasureGroupNames.Select(x => $"Test{x.Replace(" ", "_")}MeasureGroup").OrderBy(x => x);

			AssertMultilineASCIIEquals("Measure Group tests:", string.Join("\r\n", expectedMeasureGroupTests), string.Join("\r\n", actualMeasureGroupTests));
		}

		public void TestMeasureGroupCount()
		{
			AssertMultilineASCIIEquals("Measures:", string.Join("\r\n", ExpectedMeasureGroups.MeasureGroupNames), string.Join("\r\n", GetAllMeasureGroupNames()));
		}

		#endregion

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		int GetMeasureGroupCount()
		{
			var measures = AllMeasureGroups;
			return measures.Count;
		}

		JToken GetMeasureGroup(string measureGroupName)
		{
			Argument.NotNullOrEmpty(measureGroupName, nameof(measureGroupName));
			var measureGroups = AllMeasureGroups;
			return measureGroups.FirstOrDefault(x => x.SelectToken("name").Value<string>().Equals(measureGroupName, StringComparison.OrdinalIgnoreCase));
		}

		JArray GetAllMeasures(JToken measureGroup)
		{
			Argument.NotNull(measureGroup, nameof(measureGroup));

			var measures = (measureGroup.SelectToken("measures") as JArray);
			return measures;
		}

		string GetMeasureExpression(JToken measureGroup, string measureName)
		{
			string expression = null;
			var measure = measureGroup.SelectToken("measures").FirstOrDefault(x => x.SelectToken("name").Value<string>().Equals(measureName, StringComparison.OrdinalIgnoreCase));
			if (measure != null)
			{
				expression = measure.SelectToken("expression").Value<string>();
			}
			return expression;
		}

		JArray GetAllMeasureGroups()
		{
			string reportLayout = new DeploymentFileLoader().LoadReportLayoutFilesFromEmbeddedResource(BusinessArea, ReportName);
			var entireJSON = JsonConvert.DeserializeObject<JObject>(reportLayout);
			var configJSON = JsonConvert.DeserializeObject(entireJSON.SelectToken("config").ToString()) as JObject;
			var entities = (configJSON.SelectToken("modelExtensions")[0].SelectToken("entities") as JArray);

			return entities;
		}

		IEnumerable<string> GetAllMeasureGroupNames()
		{
			var measureGroups = GetAllMeasureGroups();
			var measureGroupNames = measureGroups.Select(x => x.SelectToken("name").Value<string>()).OrderBy(x => x);

			return measureGroupNames;
		}

		#region Assertions

		protected void AssertMeasureGroup(string measureGroupName)
		{
			AssertMeasureGroup(measureGroupName, ExpectedMeasureGroups[measureGroupName]);
		}

		void AssertMeasureGroup(string measureGroupName, MeasureGroup expectedMeasureGroup)
		{
			CombineAssertions($"Measure Group name: {measureGroupName}", () =>
			{
				if (expectedMeasureGroup != null)
				{
					var actualMeasureGroup = GetMeasureGroup(measureGroupName);
					if (actualMeasureGroup != null)
					{
						var actualMeasureNames = GetAllMeasures(actualMeasureGroup).Select(x => x.SelectToken("name").Value<string>()).OrderBy(x => x);

						if (expectedMeasureGroup.MeasureNames.SequenceEqual(actualMeasureNames))
						{
							foreach (Measure measure in expectedMeasureGroup)
							{
								var measureName = measure.Name;
								var expectedMeasureExpression = measure.Expression;
								var actualMeasureExpression = GetMeasureExpression(actualMeasureGroup, measureName);

								AssertMeasureExpressionsAreEqual(measureName, expectedMeasureExpression, actualMeasureExpression);
							}
						}
						else
						{
							AssertMultilineASCIIEquals("Measures:", string.Join("\r\n", expectedMeasureGroup.MeasureNames), string.Join("\r\n", actualMeasureNames));
						}
					}
					else
					{
						Fail($"Actual measure groups does not contain '{measureGroupName}'.");
					}
				}
				else
				{
					Fail($"Expected measure groups does not contain '{measureGroupName}'.");
				}
			});
		}

		void AssertMeasureExpressionsAreEqual(string measureName, string expectedExpression, string actualExpression)
		{
			Argument.NotNull(expectedExpression, nameof(expectedExpression));

			if (!string.IsNullOrEmpty(actualExpression))
			{
				Assert($"Measure name: {measureName}\r\nExpected expression:\r\n{expectedExpression}\r\n\r\nActual expression:\r\n{FormatExpression(actualExpression)}", FormatExpression(expectedExpression).Equals(FormatExpression(actualExpression), StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				Fail($"Measure '{measureName}' not found.");
			}
		}

		string FormatExpression(string expression)
		{
			if (!string.IsNullOrEmpty(expression))
			{
				var formattedExpression = expression.Trim()
					.Replace("\r", " ")
					.Replace("\n", " ")
					.Replace("\t", " ");

				return new Regex("[ ]{2,}", RegexOptions.None).Replace(formattedExpression, " ");
			}
			else
			{
				return expression;
			}
		}

		#endregion

		#region Constants

		protected MeasureGroups ExpectedMeasureGroups
		{
			get
			{
				return measureGroupsToTest ?? (measureGroupsToTest = GetExpectedMeasureGroups());
			}
		}
		MeasureGroups measureGroupsToTest;

		protected JArray AllMeasureGroups
		{
			get
			{
				return allMeasureGroups ?? (allMeasureGroups = GetAllMeasureGroups());
			}
		}
		JArray allMeasureGroups;

		protected abstract MeasureGroups GetExpectedMeasureGroups();

		protected abstract string BusinessArea { get; }

		protected abstract string ReportName { get; }

		protected abstract BiReportType ReportType { get; }

		#endregion

		#endregion

		#endregion
	}
}
