using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface ISetterSuspenderSupporter
	{
		IEnumerable<string> SupportedFields { get; }
		SetterSuspender SetterSuspender { get; }
	}

	public sealed class SetterSuspender
	{
		public IDisposable SuspendSetting(params ZString[] propertyNames) => LockDictionary(SetterSuspenderDictionary, propertyNames);
		public IDisposable SuspendEnforceSetting(params ZString[] propertyNames) => LockDictionary(EnforceSetterSuspenderDictionary, propertyNames);

		public IDisposable ResumeSetting(params ZString[] propertyNames) => LockDictionary(OverridenSetterSuspenderDictionary, propertyNames);

		public bool IsSetterSuspended(ZString propertyName)
		{
			var result = IsSuspended(enforceSetterSuspenderDictionary, propertyName);
			if (!result)
			{
				result = IsSuspended(setterSuspenderDictionary, propertyName) && !IsSuspended(overridenSetterSuspenderDictionary, propertyName);
			}
			return result;
		}

		#region Implement

		IDisposable LockDictionary(Dictionary<ZString, byte> dictionary, params ZString[] propertyNames)
		{
			return propertyNames.All(c => c.IsEmpty)
				? DisposableAction.NoAction
				: new DisposableAction
				(
					() =>
					{
						foreach (var propertyName in propertyNames)
						{
							dictionary.TryGetValue(propertyName, out var index);
							dictionary[propertyName] = ++index;
						}
					},
					() =>
					{
						foreach (var propertyName in propertyNames)
						{
							if (dictionary.TryGetValue(propertyName, out var index))
							{
								--index;

								if (index <= 0)
								{
									dictionary.Remove(propertyName);
								}
								else
								{
									dictionary[propertyName] = index;
								}
							}
						}
					}
				);
		}

		bool IsSuspended(Dictionary<ZString, byte> dictionary, ZString propertyName)
		{
			return dictionary != null && dictionary.TryGetValue(propertyName, out var index) && index > 0;
		}

		Dictionary<ZString, byte> OverridenSetterSuspenderDictionary => overridenSetterSuspenderDictionary ?? (overridenSetterSuspenderDictionary = new Dictionary<ZString, byte>());
		Dictionary<ZString, byte> overridenSetterSuspenderDictionary;

		Dictionary<ZString, byte> SetterSuspenderDictionary => setterSuspenderDictionary ?? (setterSuspenderDictionary = new Dictionary<ZString, byte>());
		Dictionary<ZString, byte> setterSuspenderDictionary;

		Dictionary<ZString, byte> EnforceSetterSuspenderDictionary => enforceSetterSuspenderDictionary ?? (enforceSetterSuspenderDictionary = new Dictionary<ZString, byte>());
		Dictionary<ZString, byte> enforceSetterSuspenderDictionary;

		#endregion
	}
}
