using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ServiceManager.Host;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;
using WTG.ApplicationLogging.Abstractions;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	class ServiceManagerApplicationInitializerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestExistingLocksAreReleasedDuringInitialization()
		{
			var services = CompositionRoot.AddRegistrations(new ServiceCollection(), new[] { Db.ServerName, Db.DatabaseName });
			Enterprise.ServiceManager.Runner.CompositionRoot.AddRegistrations(services, Mock.Of<IApplicationLoggerFactory>())
				.AddSingleton(Mock.Of<IRunnerLogger>())
				.RemoveAll<IControllerUpgrade>()
				.AddSingleton(Mock.Of<IControllerUpgrade>());

			using (var provider = services.BuildServiceProvider())
			{
				// Arrange
				var serviceTaskLocker = provider.GetRequiredService<IServiceTaskLocker>();
				var serviceTaskCodes = new[] { "~~1", "~~2", "~~3", "~~4", "~~5" };
				var disposableLocks = new List<IDisposable>();
				serviceTaskCodes.ForEach(code =>
				{
					serviceTaskLocker.TryAcquireLock(code, out var sqlMutexLock);
					if (sqlMutexLock != null)
					{
						disposableLocks.Add(sqlMutexLock);
					}
				});
				AssertEquals("SqlMutex have been acquired", serviceTaskCodes.Length, GetActiveLocksCount(serviceTaskCodes));

				var initializer = provider.GetRequiredService<IServiceManagerApplicationInitializer>();

				try
				{
					// Act
					initializer.Initialize();

					// Assert
					AssertEquals("Acquired SqlMutex locks have been released", 0, GetActiveLocksCount(serviceTaskCodes));
				}
				finally
				{
					disposableLocks.ForEach(x => x.Dispose());
				}
			}
		}

		int GetActiveLocksCount(IEnumerable<string> serviceTaskCodes)
		{
			const string sql = @"
SELECT count(*)
FROM [dbo].[StmServiceMutex]
WHERE 1=1
	AND SMX_LockInfo in (SELECT value FROM STRING_SPLIT(@serviceTaskCodes, ','))
	AND SMX_ExpiresAtUtc > GETUTCDATE()
;
";

			return Db.Connection.ExecuteScalar<int>(sql, cmd =>
			{
				cmd.AddParameter("@serviceTaskCodes", SqlDbType.VarChar, string.Join(",", serviceTaskCodes));
			});
		}
	}
}
