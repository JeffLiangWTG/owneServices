using System;
using System.Collections.Generic;

namespace CargoWise.Main.Navigation;

public class HomeFeaturesManager
{
	readonly Dictionary<HomeFeature, Func<bool>> _featureToggles = new();

	public HomeFeaturesManager AddFeatureToggle(HomeFeature feature, Func<bool> isEnabled)
	{
		if (isEnabled == null)
		{
			throw new ArgumentNullException(nameof(isEnabled), "expression cannot be null");
		}

		_featureToggles[feature] = isEnabled;
		return this;
	}

	public bool IsEnabled(HomeFeature feature)
	{
		if (_featureToggles.TryGetValue(feature, out var expression))
		{
			return expression();
		}

		return true;
	}
}
