using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Security.Core.Testing
{
	[UseSnapshotProtection]
	class SecurityCoreThreadSentryTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestThreadSentry()
		{
			var envProxy = EnvProxy.Instance;
			var securityCore = new SecurityForTest(null, envProxy.CurrentUser.PK, envProxy.CurrentBranch.PK, envProxy.CurrentDepartment.PK, envProxy.CurrentCompany.PK);

			var securityCheckpoint = securityCore.ZSecurityInstance.FindCheckPoint(new CheckpointLookupKey("TagAdd"));

			AutoResetEvent threadCompletedEvent = new AutoResetEvent(false);

			new Thread(() =>
			{
				Thread.CurrentThread.IsBackground = true;

				using (Db.DisposableActionForDbConnection())
				{
					securityCheckpoint = securityCore.ZSecurityInstance.FindCheckPoint(new CheckpointLookupKey("TagAdd"));
					securityCheckpoint = securityCore.ZSecurityInstance.FindCheckPoint(new CheckpointLookupKey("BadDebtWriteOffReceivablesTransaction"));
					securityCheckpoint = securityCore.ZSecurityInstance.FindCheckPoint(new CheckpointLookupKey("CAB2AdjustmentsView"));
					securityCheckpoint = securityCore.ZSecurityInstance.FindCheckPoint(new CheckpointLookupKey("TagDefinitionEdit"));
					securityCheckpoint = securityCore.ZSecurityInstance.FindCheckPoint(new CheckpointLookupKey("VesselConsortiumModify"));

					securityCore.PrepareForSearching();
					securityCore.ZSecurityInstance.GetGroupRightsWithImplicitRights(securityCheckpoint);
				}

				threadCompletedEvent.Set();
			}).Start();

			threadCompletedEvent.WaitOne();
		}
	}
}
