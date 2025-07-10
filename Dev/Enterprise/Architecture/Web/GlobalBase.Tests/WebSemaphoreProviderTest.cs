using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	class WebSemaphoreProviderTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCreateSemaphoreHandle()
		{
			const int heartbeatDurationSeconds = 10;
			const int expectedKeepAliveIntervalMs = (int)(heartbeatDurationSeconds * 1e3 * 0.4);

			Db.Connection.ExecuteNonQuery($@"
BEGIN TRANSACTION;

UPDATE dbo.StmData
SET
	SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), {heartbeatDurationSeconds}))
WHERE 1=1
	AND SD_Name = 'HeartbeatDuration'
;

IF @@ROWCOUNT = 0
BEGIN
	INSERT
	dbo.StmData	(SD_PK, SD_Name , SD_BinaryValue)
	VALUES (NEWID(), 'HeartbeatDuration', CONVERT(varbinary(max), CONVERT(nvarchar(max), {heartbeatDurationSeconds})))
;
END

COMMIT TRANSACTION;
");
			var semaphoreTypeMock = new Mock<ISemaphoreType>();
			var remoteLogOffHandlerMock = new Mock<IHeartBeatRemoteLogoff>();
			var webSemaphoreProvider = new WebSemaphoreProvider(remoteLogOffHandlerMock.Object);
			var semaphoreProvider = (ISemaphoreProvider)webSemaphoreProvider;
			var heartBeat = webSemaphoreProvider.InternalHeartbeat;

			semaphoreTypeMock.Setup(x => x.LockInfo).Returns("test");
			semaphoreTypeMock.Setup(x => x.Category).Returns("LGN");

			using (var semaphoreHandle = semaphoreProvider.CreateSemaphoreHandle(semaphoreTypeMock.Object))
			{
				Assert(semaphoreHandle.Success);
			}

			Assert(Db.Connection.Exists("FROM dbo.StmServiceHeartBeat WHERE SV_ParentId = @userPk", command =>
			{
				command.AddParameter("@userPk", SqlDbType.UniqueIdentifier, EnvProxy.Instance.CurrentUser.PK);
			}));

			AssertEquals(heartbeatDurationSeconds, Env.Registry.HeartbeatDurationSeconds);
			AssertEquals(expectedKeepAliveIntervalMs, heartBeat.KeepAliveIntervalMs);
		}
	}
}
