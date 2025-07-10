using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Testing;

public class ConfigurationPreq<T> : Preq<T>
	where T : BusinessObject
{
	public ConfigurationPreq(Func<T, bool, IDisposable> setUpConfig)
	{
		this.setUpConfig = new Func<T, bool, IDisposable>((x, y) => { var dp = setUpConfig(x, y); disposables.Add(dp); return dp; });
	}

	readonly Func<T, bool, IDisposable> setUpConfig;
	bool validConfigValue;

	static Collection<IDisposable> disposables = new Collection<IDisposable>();

	public static void Dispose()
	{
		foreach (var disposable in disposables)
		{
			disposable.Dispose();
		}
		disposables = new Collection<IDisposable>();
	}

	protected override IEnumerable<Action<T>> GetSatisfactoryActionsCore()
	{
		return new List<Action<T>> { x => {
				var dp = setUpConfig(x, validConfigValue);
			}
		};
	}

	protected override IEnumerable<Action<T>> GetFailingActionsCore()
	{
		return new List<Action<T>> {
			x => {
				Dispose();
				setUpConfig(x, !validConfigValue);
			}
		};
	}

	public ConfigurationPreq<T> Values(bool configValue)
	{
		validConfigValue = configValue;
		return this;
	}
}
