using System;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// The purpose of this class is to store ZPropertyInfo state
	/// </summary>
	internal class ZPropertyInfoStorage
	{
		internal bool ValueChangedIsActive
		{
			get { return valueChangedDictionary != null; }
		}

		internal HumanReadableName GetHumanReadableName(ZPropertyInfo propertyInfo)
		{
			HumanReadableName result;
			HumanReadableNameDictionary.TryGetValue(propertyInfo, out result);
			return result ?? new HumanReadableName();
		}

		internal void SetHumanReadableName(ZPropertyInfo propertyInfo, ZString value, bool guiCaption = false)
		{
			if (value.IsEmpty)
			{
				HumanReadableNameDictionary.Remove(propertyInfo);
			}
			else
			{
				HumanReadableNameDictionary[propertyInfo] = new HumanReadableName { Value = value, IsGuiCaption = guiCaption };
			}
		}

		internal DictionaryKeyedByZPropertyInfo<EventHandler> ValueChangedDictionary
		{
			get { return valueChangedDictionary ?? (valueChangedDictionary = new DictionaryKeyedByZPropertyInfo<EventHandler>()); }
		}

		internal DictionaryKeyedByZPropertyInfo<EventHandler> ConcurrencyMergedDictionary => concurrencyMergedDictionary ?? (concurrencyMergedDictionary = new DictionaryKeyedByZPropertyInfo<EventHandler>());

		internal DictionaryKeyedByZPropertyInfo<RunValidationInvoker> AdditionalValidationDictionary
		{
			get { return additionalValidationDictionary ?? (additionalValidationDictionary = new DictionaryKeyedByZPropertyInfo<RunValidationInvoker>()); }
		}

		internal DictionaryKeyedByZPropertyInfo<string> InvalidTextDictionary = new DictionaryKeyedByZPropertyInfo<string>();
		internal DictionaryKeyedByZPropertyInfo<NotificationCollection> NotificationDictionary = new DictionaryKeyedByZPropertyInfo<NotificationCollection>();
		internal DictionaryKeyedByZPropertyInfo<bool> ReadOnlyDictionary = new DictionaryKeyedByZPropertyInfo<bool>();
		internal DictionaryKeyedByZPropertyInfo<HumanReadableName> HumanReadableNameDictionary = new DictionaryKeyedByZPropertyInfo<HumanReadableName>();

		DictionaryKeyedByZPropertyInfo<EventHandler> valueChangedDictionary;
		DictionaryKeyedByZPropertyInfo<EventHandler> concurrencyMergedDictionary;
		DictionaryKeyedByZPropertyInfo<RunValidationInvoker> additionalValidationDictionary;

		internal class HumanReadableName
		{
			internal ZString Value { get; set; }
			internal bool IsGuiCaption { get; set; }
		}
	}
}
