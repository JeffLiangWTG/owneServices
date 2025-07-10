using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Tasks.ScheduledUpgrader.Testing
{
	[TestedType(typeof(ScheduledUpgraderConfig))]
	sealed class ScheduledUpgraderConfigTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSaveAndLoad()
		{
			var taskSchedule = Factory.New<IServiceTaskSchedule>();
			var config = new ScheduledUpgraderConfig(taskSchedule);
			AssertEquals(false, config.AutoDownload);
			AssertEquals(false, config.PatchOnly);
			AssertEquals(false, config.NotifyOnSuccess);
			AssertEquals(Constants.Groups.PostMastersGroupPK, config.NotificationGroup_PK);
			config.AutoDownload = true;
			config.PatchOnly = true;
			config.NotifyOnSuccess = true;
			Factory.Save();
			config = new ScheduledUpgraderConfig(taskSchedule);
			AssertEquals(true, config.AutoDownload);
			AssertEquals(true, config.PatchOnly);
			AssertEquals(true, config.NotifyOnSuccess);
			AssertEquals(Constants.Groups.PostMastersGroupPK, config.NotificationGroup_PK);
			config.AutoDownload = false;
			config.PatchOnly = true;
			config.NotificationGroup_PK = Constants.Groups.AllPK;
			Factory.Save();
			config = new ScheduledUpgraderConfig(taskSchedule);
			AssertEquals(false, config.AutoDownload);
			AssertEquals(true, config.PatchOnly);
			AssertEquals(true, config.NotifyOnSuccess);
			AssertEquals(Constants.Groups.AllPK, config.NotificationGroup_PK);
			config.NotificationGroup_PK = Constants.Groups.PostMastersGroupPK;
			Factory.Save();
			config = new ScheduledUpgraderConfig(taskSchedule);

			GlbGroup pMG = Factory.Load<GlbGroup>(Constants.Groups.PostMastersGroupPK);
			AssertEquals(pMG, config.NotificationGroup);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ScheduledUpgraderConfig(Factory.New<IServiceTaskSchedule>());
		}

		#endregion
	}
}
