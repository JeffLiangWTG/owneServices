using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.eServices.Testing
{
	sealed class ApplicationCodeObjCollectionMergeManagerTest : TestCase
	{
		public void TestMergeApplicationCodePurgeTypeWithoutPurgeTime()
		{
			var primaryCollection = GetAllPurgeSettings(new ApplicationCodeSettingDefault());
			var secondaryCollection = GetAllPurgeSettings(new ApplicationCodeSettingWithoutPurgeTime());

			var defaultTwc = primaryCollection.GetApplicationCodeObj("TWC");
			AssertNotNull(defaultTwc);
			AssertEquals((short)12, defaultTwc.PurgeTime);
			AssertEquals(TimeUnit.Month, defaultTwc.PurgeTimeUnit);

			var secondaryTwc = secondaryCollection.GetApplicationCodeObj("TWC");
			var messageType = (MessageTypeObj)secondaryTwc.MessageTypes.FirstOrDefault();
			messageType.Selected = false;

			var mergeManager = new ApplicationCodeObjCollectionMergeManager();
			var mergedCollection = mergeManager.MergeApplicationCodeObjCollections(primaryCollection, secondaryCollection);

			var mergedTwc = mergedCollection.GetApplicationCodeObj("TWC");
			AssertNotNull(mergedTwc);
			AssertEquals(PurgeTypeList.ApplicationCode, defaultTwc.PurgeType);
			AssertEquals((short)3, mergedTwc.PurgeTime);
			AssertEquals(TimeUnit.Week, mergedTwc.PurgeTimeUnit);
			Assert(!mergedTwc.Selected);
			AssertEquals(0, mergedTwc.MessageTypes.Count);
			Assert(!mergedTwc.PurgeTimeInfo.HasNotifications());
		}

		public void TestMergeApplicationCodePurgeTypeWithPurgeTime()
		{
			var primaryCollection = GetAllPurgeSettings(new ApplicationCodeSettingDefault());
			var secondaryCollection = GetAllPurgeSettings(new ApplicationCodeSettingWithPurgeTime());
			var defaultTwc = primaryCollection.GetApplicationCodeObj("TWC");
			AssertNotNull(defaultTwc);
			AssertEquals((short)12, defaultTwc.PurgeTime);
			AssertEquals(TimeUnit.Month, defaultTwc.PurgeTimeUnit);

			var mergeManager = new ApplicationCodeObjCollectionMergeManager();
			var mergedCollection = mergeManager.MergeApplicationCodeObjCollections(primaryCollection, secondaryCollection);

			var mergedTwc = mergedCollection.GetApplicationCodeObj("TWC");
			AssertNotNull(mergedTwc);
			AssertEquals(PurgeTypeList.ApplicationCode, defaultTwc.PurgeType);
			AssertEquals((short)2, mergedTwc.PurgeTime);
			AssertEquals(TimeUnit.Year, mergedTwc.PurgeTimeUnit);
			AssertEquals(0, mergedTwc.MessageTypes.Count);
			Assert(!mergedTwc.PurgeTimeInfo.HasNotifications());
		}

		public void TestMergeApplicationCodePurgeTypeDoesNotMergeUnpurgableApplicationCodes()
		{
			var primaryCollection = GetAllPurgeSettings(new ApplicationCodeSettingWithUnpurgableCodes());
			var secondaryCollection = GetAllPurgeSettings(new ApplicationCodeSettingDefault());

			var defaultJpc = primaryCollection.GetApplicationCodeObj("JPC");
			AssertNotNull(defaultJpc);
			Assert(defaultJpc.IsUnpurgable);
			Assert(!defaultJpc.Selected);
			AssertEquals(defaultJpc.PurgeTime, ZShort.Zero);
			AssertEquals(defaultJpc.PurgeTimeUnit, ZGuid.Empty);

			var mergeManager = new ApplicationCodeObjCollectionMergeManager();
			var mergedCollection = mergeManager.MergeApplicationCodeObjCollections(primaryCollection, secondaryCollection);

			var mergedJpc = mergedCollection.GetApplicationCodeObj("JPC");
			AssertNotNull(mergedJpc);
			Assert(mergedJpc.IsUnpurgable);
			Assert(!mergedJpc.Selected);
			AssertEquals(ZShort.Zero, mergedJpc.PurgeTime);
			AssertEquals(ZGuid.Empty, mergedJpc.PurgeTimeUnit);
			Assert(mergedJpc.MessageTypes.IsNullOrEmpty());
			Assert(!mergedJpc.PurgeTimeInfo.HasNotifications());
		}

		class ApplicationCodeSettingWithPurgeTime : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodePurgeType("TWC", new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, 2, TimeUnit.Year);
			}
		}

		class ApplicationCodeSettingWithoutPurgeTime : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodeMessageSubTypePurgeType("TWC", new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
					.Add("", "", 3, TimeUnit.Week)
				);
			}
		}

		class ApplicationCodeSettingDefault : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodePurgeType("JPC", new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, 12, TimeUnit.Month);
				yield return AddApplicationCodePurgeType("TWC", new InterchangeObjCollection() { NewInterchangeConfigObj(12, TimeUnit.Month) }, 12, TimeUnit.Month);
			}
		}

		class ApplicationCodeSettingWithUnpurgableCodes : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddUnpurgableApplicationCode("JPC");
			}
		}

		ApplicationCodeObjCollection GetAllPurgeSettings(PurgeSettingsConfig config)
		{
			var result = new ApplicationCodeObjCollection();
			result.AddRange(config.GetPurgeSettings());
			return result;
		}
	}
}
