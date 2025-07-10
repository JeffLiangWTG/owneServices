using System.IO;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Web.Http;
using CargoWise.BuildTools;
using CargoWise.Data;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.Bi.BusinessIntelligence.Testing.AnalysisServices
{
	public abstract class BaseDAXAnalysisServicesTest : BaseBusinessIntelligenceTest
	{
		public string DaxQuery => daxQuery ?? (daxQuery = File.ReadAllText($"{Path.Combine(DaxPath, DaxFile)}"));
		string daxQuery;

		protected string ExpectedResults => expectedResults ?? (expectedResults = File.ReadAllText($"{Path.Combine(DaxPath, ExpectedResultsFile)}"));
		string expectedResults;

		protected virtual bool ShouldCompareRows
		{
			get
			{
				return true;
			}
		}

		protected abstract string CubeSuffix { get; }
		protected string DaxPath => Path.Combine(BuildConstants.LocalEnterprisePath, "BusinessIntelligence", "BiIntegration", "Deployment", "CargoWiseBiDeployment", "BusinessIntelligence.Testing", "DaxStatements");
		protected abstract string DaxFile { get; }
		protected abstract string ExpectedResultsFile { get; }
		protected virtual bool ResultsTruncated => false;

		protected override string CubeName
		{
			get
			{
				return Db.DatabaseName + "_" + CubeSuffix;
			}
		}

		public virtual void TestEvaluateDax()
		{
			if (SsasServerConnection.DatabaseExists(CubeName))
			{
				using (var cubeDataTable = SsasHelper.ExecuteDaxQuery(SsasHelper.AnalysisServerName, CubeName, DaxQuery))
				{
					if (cubeDataTable.Columns.Count == 0)
					{
						Fail($"Model Table result set for '{DaxFile}' is empty. Check the configuration and try again.");
					}
					else
					{
						if (ResultsTruncated)
						{
							var result = JsonConvert.SerializeObject(cubeDataTable, Formatting.Indented);
							var truncatedResult = Regex.Replace(result, @"[\d-]", string.Empty).Replace("\"", string.Empty).Replace(".", string.Empty);
							var truncatedExpected = Regex.Replace(ExpectedResults, @"[\d-]", string.Empty).Replace("\"", string.Empty).Replace(".", string.Empty);
							AssertContains(truncatedExpected, truncatedResult);
						}
						else
						{
							AssertEquals(ExpectedResults, JsonConvert.SerializeObject(cubeDataTable, Formatting.Indented));
						}
					}
				}
			}
			else
			{
				Fail($"Could not find cube [{CubeName}] on Analysis Server.");
			}
		}

		[SnailTest]
		public virtual void TestEvaluateDaxApi()
		{
			var path = Path.Combine(DaxPath, ExpectedResultsFile);
			DaxApiTestHelpers.AssertDaxQueryReturnsSameResultAsJsonFile(CubeName, DaxQuery, path);
		}

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		const string CubeBackupFilePath = @"BusinessIntelligence\BiIntegration\Deployment\CargoWiseBiDeployment\BusinessIntelligence.Testing\ModelBackup";

		#endregion

		public class DummyController : ApiController
		{
			public DummyController(IIdentity identity)
			{
				User = new GenericPrincipal(identity, null);
			}
		}
	}
}
