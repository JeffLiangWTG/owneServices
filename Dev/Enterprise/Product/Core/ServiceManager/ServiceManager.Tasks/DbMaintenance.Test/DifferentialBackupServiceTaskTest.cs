using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbBackup.Engine;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(DifferentialBackupServiceTask))]
	sealed class DifferentialBackupServiceTaskTest : ServiceTaskTestCase<DifferentialBackupServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestDifferentialBackupRunsUnconditionally()
		{
			CombineAssertions(() =>
			{
				Test(DatabaseTypes.Codes.Test, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Test, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Production, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Production, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Demo, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Demo, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Education, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Education, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Training, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Training, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.WisecloudTrial, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.WisecloudTrial, hostedOnWiseCloud: false);
			});

			void Test(string databaseType, bool hostedOnWiseCloud)
			{
				// Arrange
				var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var savedData = keyForTest.DatabaseTypeForTest;
				var savedIsWiseTechGlobalDatabaseServerForTest = DataUtils.IsWiseTechGlobalDatabaseServerForTest;

				var loggerMock = new Mock<ILogger>();
				var dbMaintenanceProxyMock = new Mock<IDbMaintenanceProxy>(MockBehavior.Strict);
				dbMaintenanceProxyMock.Setup(x => x
					.RunDifferentialBackup(It.IsAny<ILogger>(), It.IsAny<IEmailNotificationSender>())).Verifiable();

				using (ObjectFactory.Substitute(dbMaintenanceProxyMock.Object))
				using (new DisposableAction(() =>
				{
					keyForTest.DatabaseTypeForTest = savedData;
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = savedIsWiseTechGlobalDatabaseServerForTest;
				}))
				{
					keyForTest.DatabaseTypeForTest = databaseType;
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = hostedOnWiseCloud;

					var backupTask = new DifferentialBackupServiceTask { ServiceLogger = loggerMock.Object };

					// Act
					backupTask.RunTask();

					// Assert
					AssertNoExceptionThrown(() =>
					{
						dbMaintenanceProxyMock.Verify(x => x
							.RunDifferentialBackup(It.IsAny<ILogger>(), It.IsAny<IEmailNotificationSender>()), Times.Once);
						dbMaintenanceProxyMock.VerifyNoOtherCalls();
					});
				}
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
