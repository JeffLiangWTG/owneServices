using System;
using CargoWise.Common;
using CargoWise.Common.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public class CurrentLogSubscriberTracker : ICurrentLogSubscriberTracker
	{
		public string CurrentName => Name.Value;

		internal static IDisposable SetCurrentCode(LogSubscriber subscriber)
		{
			var oldValue = Name.Value;
			DisposableAction action = null;
			action = new DisposableAction(() =>
			{
				DisposableLeakListener.Instance.UnRegisterDisposable(action);
				Name.Value = oldValue;
			});

			DisposableLeakListener.Instance.RegisterDisposable(action);
			Name.Value = subscriber.Name;
			return action;
		}

		static Overridable<string> Name { get; } = new Overridable<string>(null);
	}
}
