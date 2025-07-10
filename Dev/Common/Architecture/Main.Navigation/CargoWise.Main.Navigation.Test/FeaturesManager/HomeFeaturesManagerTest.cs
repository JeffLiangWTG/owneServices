using System;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test;

public class HomeFeaturesManagerTest : TestCase
{
	public void TestFeaturesManager_WhenNoFeatureToggleAdded()
	{
		var uut = new HomeFeaturesManager();

		foreach (HomeFeature feature in Enum.GetValues(typeof(HomeFeature)))
		{
			Assert($"'{feature}' should be enabled.", uut.IsEnabled(feature));
		}
	}

	public void TestFeaturesManager_WhenFeatureToggleAdded_WhenExpressionIsNull()
	{
		var uut = new HomeFeaturesManager();

		AssertExceptionThrown<ArgumentNullException>(() => uut.AddFeatureToggle(HomeFeature.Snapshots, null));
	}

	public void TestFeaturesManager_WhenFeatureToggleAdded()
	{
		var uut = new HomeFeaturesManager();

		foreach (HomeFeature feature in Enum.GetValues(typeof(HomeFeature)))
		{
			uut.AddFeatureToggle(feature, () => false);
			Assert($"'{feature}' should NOT be enabled.", !uut.IsEnabled(feature));
		}

		foreach (HomeFeature feature in Enum.GetValues(typeof(HomeFeature)))
		{
			uut.AddFeatureToggle(feature, () => true);
			Assert($"'{feature}' should be enabled.", uut.IsEnabled(feature));
		}
	}
}
