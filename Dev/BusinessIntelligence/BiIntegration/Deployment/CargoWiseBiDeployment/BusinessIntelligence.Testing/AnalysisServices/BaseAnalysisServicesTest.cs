using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Bi.Configuration;
using CargoWise.BuildTools;
using CargoWise.Data;

namespace CargoWise.Bi.BusinessIntelligence.Testing.AnalysisServices
{
	public abstract class BaseAnalysisServicesTest : BaseBusinessIntelligenceTest
	{
		protected virtual bool ShouldCompareRows
		{
			get
			{
				return true;
			}
		}

		protected virtual string CubeSuffix { get; }
		public virtual string ModelTableName { get; }

		protected override string CubeName
		{
			get
			{
				return Db.DatabaseName + "_" + CubeSuffix;
			}
		}

		public void TestEvaluateModelTable()
		{
			if (SsasServerConnection.DatabaseExists(CubeName))
			{
				var executeCommand = $"EVALUATE('{ModelTableName}')";
				var cubeDataTable = SsasHelper.ExecuteDaxQuery(SsasHelper.AnalysisServerName, CubeName, executeCommand);

				var xmlDataTable = LoadDataTableFromXml();
				if (xmlDataTable == null)
				{
					Fail($"Model Table result set xml for '{ModelTableName}' is empty. Check the configuration and try again.");
				}
				else
				{
					CompareDataTables(xmlDataTable, cubeDataTable);
				}
			}
			else
			{
				Fail($"Could not find cube [{CubeName}] on Analysis Server.");
			}
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		DataTable LoadDataTableFromXml()
		{
			var cubeNamespace = GetType().Namespace.Split('.').Last();
			var filePath = Path.Combine(BuildConstants.LocalEnterprisePath, ResultSetFilePath, cubeNamespace, ModelTableName + ".xml");
			if (File.Exists(filePath))
			{
				var dataSet = new DataSet();
				BiConfiguration.LoadConfigurationXml(dataSet, filePath);
				return dataSet.Tables[ModelTableName];
			}
			else
			{
				return null;
			}
		}

		void CompareDataTables(DataTable xmlDataTable, DataTable cubeDataTable)
		{
			var xmlDataColumns = GetColumnNamesFromDataTable(xmlDataTable);
			var cubeDataColumns = GetColumnNamesFromDataTable(cubeDataTable);

			CompareColumns(xmlDataColumns, cubeDataColumns);
			CompareRows(xmlDataTable, cubeDataTable, cubeDataColumns);
		}

		void CompareColumns(List<string> xmlDataColumns, List<string> cubeDataColumns)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Column count", xmlDataColumns.Count, cubeDataColumns.Count);
				AssertContainsExactElementsInAnyOrder("Column names", xmlDataColumns, cubeDataColumns);
			});
		}

		void CompareRows(DataTable xmlDataColumns, DataTable cubeDataColumns, List<string> dataColumns)
		{
			if (ShouldCompareRows)
			{
				var xmlDataRows = GetDataRows(xmlDataColumns, dataColumns);
				var cubeDataRows = GetDataRows(cubeDataColumns, dataColumns);
				AssertContainsExactElementsInAnyOrder("Data rows", xmlDataRows, cubeDataRows);
			}
		}

		List<string> GetColumnNamesFromDataTable(DataTable dataTable)
		{
			var result = new List<string>();
			foreach (DataColumn column in dataTable.Columns)
			{
				result.Add(column.ColumnName);
			}
			return result;
		}

		List<string> GetDataRows(DataTable dataTable, List<string> dataColumns)
		{
			var result = new List<string>();
			foreach (DataRow dataRow in dataTable.Rows)
			{
				var stringBuilder = new StringBuilder();
				foreach (var columnName in dataColumns)
				{
					if (!string.IsNullOrEmpty(stringBuilder.ToString()))
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(dataRow[columnName]);
				}
				result.Add(stringBuilder.ToString());
			}
			return result;
		}

		const string ResultSetFilePath = @"BusinessIntelligence\BiIntegration\Deployment\CargoWiseBiDeployment\BusinessIntelligence.Testing\AnalysisServices\ModelTableResultSets";

		#endregion
	}
}
