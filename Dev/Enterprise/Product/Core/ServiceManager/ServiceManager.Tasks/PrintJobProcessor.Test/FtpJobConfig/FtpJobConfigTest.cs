using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	[TestedType(typeof(FtpJobConfig))]
	sealed class FtpJobConfigTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFtpJobConfig()
		{
			var ftpConfig = GetNewBusinessObject() as FtpJobConfig;
			Assert(!ftpConfig.NotifyOnFailure);
			Assert(!ftpConfig.NotifyOnSuccess);
			Assert(!ftpConfig.NotifyPrintUser);
			AssertEquals(ZGuid.Empty, ftpConfig.NotificationGroup_PK);
			Assert(!ftpConfig.NotificationGroup_PKInfo.HasErrors());

			ftpConfig.NotifyOnSuccess = true;
			Assert(ftpConfig.NotificationGroup_PKInfo.HasErrors());

			ftpConfig.NotificationGroup_PK = ZGuid.NewZGuid();
			Assert(!ftpConfig.NotificationGroup_PKInfo.HasErrors());

			ftpConfig.NotifyOnSuccess = false;
			ftpConfig.NotificationGroup_PK = ZGuid.Empty;

			ftpConfig.NotifyOnFailure = true;
			Assert(ftpConfig.NotificationGroup_PKInfo.HasErrors());

			ftpConfig.NotificationGroup_PK = ZGuid.NewZGuid();
			Assert(!ftpConfig.NotificationGroup_PKInfo.HasErrors());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				return new FtpJobConfig((BusinessObject)Factory.New<IStmServiceTaskBranchValidationAdapter>());
			}
			return new FtpJobConfig((BusinessObject)Factory.New<IServiceTaskSchedule>());
		}

		#endregion
	}
}
