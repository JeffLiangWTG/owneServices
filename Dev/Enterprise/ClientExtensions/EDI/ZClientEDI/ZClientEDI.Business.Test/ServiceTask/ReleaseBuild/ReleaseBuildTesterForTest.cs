using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTask.Test
{
	internal class ReleaseBuildTesterForTest : ReleaseBuildTester
	{
		public ReleaseBuildTesterForTest(ReleaseBuild build, ILogger logger) : base(build, logger, CancellationToken.None)
		{
		}

		public string BuildSchemaVersionOverride { get; set; }
		public string BuildScriptVersionOverride { get; set; }
		public string BuildDataVersionOverride { get; set; }
		public string BuildTransformationVersionOverride { get; set; }

		public string DbSchemaVersionOverride { get; set; }
		public string DbScriptVersionOverride { get; set; }
		public string DbDataVersionOverride { get; set; }
		public string DbTransformationVersionOverride { get; set; }

		public int DbVersionMajorOverride { get; set; }
		public int DbVersionMinorOverride { get; set; }
		public int DbVersionReleaseOverride { get; set; }
		public int DbVersionPatchOverride { get; set; }

		protected override int RetryIntervalInSeconds => 1;
		protected override int MaxRetryTimes => 1;

		public bool TriggerBuildUploadFailure { get; set; }
		public bool TriggerUpgradeCommandFailure { get; set; }
		public bool TriggerRemoteUpgradeFailure { get; set; }

		public HttpStatusCode WebAppHealthCheckStatusCodeOverride { get; set; } = HttpStatusCode.OK;
		public string WebAppHealthCheckContentOverride { get; set; }

		protected override Versions GetVersionsFromBuildCore()
		{
			return new Versions()
			{
				SchemaVerion = BuildSchemaVersionOverride,
				ScriptVerion = BuildScriptVersionOverride,
				DataVerion = BuildDataVersionOverride,
				TransformationVerion = BuildTransformationVersionOverride,
			};
		}

		protected override Versions GetVersionsFromDbCore(string serverName, string dbName)
		{
			return new Versions()
			{
				SchemaVerion = DbSchemaVersionOverride,
				ScriptVerion = DbScriptVersionOverride,
				DataVerion = DbDataVersionOverride,
				TransformationVerion = DbTransformationVersionOverride,
			};
		}

		protected override Version GetDbCurrentVersionNumber(string serverName, string dbName)
		{
			return new Version(DbVersionMajorOverride, DbVersionMinorOverride, DbVersionReleaseOverride, DbVersionPatchOverride);
		}

		protected override bool UploadBuildToTestDatabaseCore(string serverName, string dbName, Version buildVersion)
		{
			return !TriggerBuildUploadFailure;
		}

		protected override ProcessStartInfo GetUpgradeCommandToRemote(string serverName, string dbName, string targetHost)
		{
			return new ProcessStartInfo("TestCommand.exe", "-Params");
		}

		protected override bool RunRemoteUpgradeCore(ProcessStartInfo processInfo)
		{
			if (TriggerUpgradeCommandFailure)
			{
				return false;
			}
			else
			{
				if (!TriggerRemoteUpgradeFailure)
				{
					DbSchemaVersionOverride = BuildSchemaVersionOverride;
					DbScriptVersionOverride = BuildScriptVersionOverride;
					DbDataVersionOverride = BuildDataVersionOverride;
					DbTransformationVersionOverride = BuildTransformationVersionOverride;
				}
				return true;
			}
		}

		protected override (HttpStatusCode StatusCode, string Content) RunWebAppHealthCheck(string webAppUrl)
		{
			var statusCode = WebAppHealthCheckStatusCodeOverride;
			var content = string.IsNullOrEmpty(WebAppHealthCheckContentOverride) ? EDIConstants.DatabaseConnectionStatus.Ok : WebAppHealthCheckContentOverride;
			return (statusCode, content);
		}
	}
}
