using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.DevTools.Definitions;

[assembly: HostedService(Enterprise.Client.EDI.ServiceTask.ReleaseBuildTestServiceTask.Code,
	Enterprise.Client.EDI.ServiceTask.ReleaseBuildTestServiceTask.Description,
	"CSP",
	typeof(Enterprise.Client.EDI.ServiceTask.ReleaseBuildTestServiceTask),
	CanRunInAnyBranch = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "15Minutes",
	DefaultScheduleRunEvery = "1hour"
	)]

namespace Enterprise.Client.EDI.ServiceTask
{
	public class ReleaseBuildTestServiceTask : ServiceProviderImpl
	{
		public const string Code = "RBT";
		public const string Description = "Release Build Test Service Task";

		public override void RunTask(CancellationToken cancellationToken)
		{
			var build = GetBuildToTest();
			if (build != null)
			{
				var result = false;
				try
				{
					var tester = GetTester(build, ServiceLogger, cancellationToken);
					var deploymentResult = tester.RunDeployment();
					if (deploymentResult)
					{
						result = tester.RunTest();
						build.HL_TestDateUtc = ZDateTime.UtcNow;
						build.HL_IsTestPassed = result;
						build.Factory.Save();
					}
				}
				catch (OperationCanceledException)
				{
					ServiceLogger.Warning("Build test has been cancelled");
				}
				catch (Exception ex)
				{
					ServiceLogger.Error(ex.Message);
					ErrorReporter.ReportOnce(ex.Message, ex);
				}

				SendNotificationEmail(build, result);
			}
		}

		protected virtual ReleaseBuildTester GetTester(ReleaseBuild build, ILogger logger, CancellationToken cancellationToken)
		{
			return new ReleaseBuildTester(build, logger, cancellationToken);
		}

		ReleaseBuild GetBuildToTest()
		{
			var query = new ZQuery(ReleaseBuildSchema.HL_ReleaseStatus, ReleaseRings.Codes.ALP);
			query.AddToFilter(ReleaseBuildSchema.HL_IsActive, true);
			query.AddToFilter(ReleaseBuildSchema.HL_Superceded, false);
			query.AddToFilter(ReleaseBuildSchema.HL_TestDateUtc, null);
			query.OrderBy = FormattableString.Invariant($"{ReleaseBuildSchema.Constants.HL_MajorVersion},{ReleaseBuildSchema.Constants.HL_MinorVersion},{ReleaseBuildSchema.Constants.HL_Release},{ReleaseBuildSchema.Constants.HL_Patch}");

			return new BusinessObjectFactory().LoadTop1<ReleaseBuild>(query);
		}

		void SendNotificationEmail(ReleaseBuild build, bool result)
		{
			var resultAsText = result ? "Passed" : "Failed";
			var subject = $"Release Build {build.VersionNumber} Test {resultAsText}";

			var regItem = EDIDataRegistry.Instance.ReleaseBuildTestResultNotificationGroup;
			var regFullName = string.Join(" > ", regItem.Categories) + " > " + regItem.Caption;

			var mail = new EmailDef()
			{
				Subject = subject,
				Body = $"You are receiving this email because you are a member of the group in registry {regFullName}",
			};
			try
			{
				Env.OutgoingMailManager.CreateAndSave(mail, regItem.Value, GroupSourceLocator.GetFromRegistryItem(regItem));
			}
			catch (EmailSendFailedException)
			{
			}
		}
	}
}
