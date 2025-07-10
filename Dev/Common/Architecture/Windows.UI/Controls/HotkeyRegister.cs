using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Use to register hotkeys and shortcuts for your form. Can be used for single key with modifiers hotkeys, so for example 'Ctrl+A', 'Shift+F1', 'Alt+Shift+L' and 'F2' are all valid.
	/// However A+R, or F5+2 are not valid as they will cause collisions for Keys flags.
	/// 
	/// To use modifiers, OR the values like a bitmask. So for Ctrl+A use Keys.Control | Keys.A
	/// </summary>
	[ThreadSafe]
	public class HotkeyRegister
	{
		const Keys allControlKeys = Keys.Control | Keys.Shift | Keys.Alt;

		public void RegisterHotKey(Keys hotKey, Action processor, string description = null)
		{
			RegisterHotKey(hotKey, (sender, key) => { processor(); return true; }, description);
		}

		public void RegisterHotKey(Keys hotKey, HotKeyPressedProcessor processor, string description = null)
		{
			if (registeredKeys.TryAdd(hotKey, processor))
			{
				if (!string.IsNullOrEmpty(description))
				{
					AddDescription(hotKey, description);
				}
			}
			else
			{
				ReportAttemptToRegisterHotkeyTwice(hotKey, description);
			}
		}

		public void RegisterHotKeyRange(Keys from, Keys to, HotKeyPressedProcessor processor, string description = null)
		{
			if ((from & allControlKeys) != (to & allControlKeys))
			{
				throw new ArgumentException("'from' and 'to' keys most both have the same control characters");
			}
			else if (to <= from)
			{
				throw new ArgumentException("'to' parameter must be greater than 'from'");
			}

			var controlKeys = from & allControlKeys;
			from &= ~allControlKeys;
			to &= ~allControlKeys;

			if (!IsIterableRange(from, to))
			{
				throw new ArgumentException("The specified range is not valid");
			}

			for (var key = from; key <= to; key++)
			{
				RegisterHotKey(key | controlKeys, processor);
			}

			if (description != null)
			{
				AddDescription(GetKeyRangeAsString(controlKeys, from, to), description);
			}
		}

		bool IsIterableRange(Keys from, Keys to)
		{
			return (from >= Keys.D0 && to <= Keys.D9) ||
				(from >= Keys.A && to <= Keys.Z) ||
				(from >= Keys.F1 && to <= Keys.F12);
		}

		void ReportAttemptToRegisterHotkeyTwice(Keys hotkey, string description)
		{
			var keyDescription = GetKeyAsString(hotkey);
			var originalDescription = Descriptions.FirstOrDefault(kvp => kvp.Key == keyDescription).Value;
			var errorReport = string.Format(CultureInfo.InvariantCulture, (NoResString)"Attemped to register the same hotkey twice. Key: {0}, Original Description: '{1}', New Description: '{2}'", keyDescription, originalDescription ?? (NoResString)"<empty>", description ?? (NoResString)"<empty>");

			ErrorReporter.ReportOnce("DoubleRegisterKey|" + keyDescription, errorReport);
		}

		public void Clear()
		{
			registeredKeys.Clear();
			descriptions.Clear();
		}

		#region Descriptions

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "There is no benefit to creating a new type for this.")]
		public IEnumerable<KeyValuePair<string, string>> Descriptions
		{
			get
			{
				lock (descriptions)
				{
					return descriptions.ToArray();
				}
			}
		}

		public void AddDescription(Keys hotkey, string description)
		{
			AddDescription(GetKeyAsString(hotkey), description);
		}

		public void AddDescription(string hotkey, string description)
		{
			lock (descriptions)
			{
				descriptions.Add(new KeyValuePair<string, string>(hotkey, description));
			}
		}

		readonly IList<KeyValuePair<string, string>> descriptions = new List<KeyValuePair<string, string>>(5);

		string GetKeyRangeAsString(Keys controlKeys, Keys from, Keys to)
		{
			var controlKeyString = GetControlKeysString(controlKeys);
			var rangeString = GetKeyName(from) + " - " + GetKeyName(to);

			return string.IsNullOrEmpty(controlKeyString) ? rangeString : (controlKeyString + keyJoinString + rangeString);
		}

		string GetKeyAsString(Keys key)
		{
			var controlString = GetControlKeysString(key);
			var keyName = GetKeyName(key);
			return string.IsNullOrEmpty(controlString) ? keyName : (controlString + keyJoinString + keyName);
		}

		string GetKeyName(Keys key)
		{
			key &= ~allControlKeys;
			if (key >= Keys.D0 && key <= Keys.D9)
			{
				return (key - Keys.D0).ToString(CultureInfo.InvariantCulture);
			}

			return Enum.GetName(typeof(Keys), key & ~allControlKeys);
		}

		string GetControlKeysString(Keys key)
		{
			var result = string.Empty;
			if (key.HasFlag(Keys.Control))
			{
				result += keyJoinString + ControlKeyName;
			}

			if (key.HasFlag(Keys.Shift))
			{
				result += keyJoinString + ShiftKeyName;
			}

			if (key.HasFlag(Keys.Alt))
			{
				result += keyJoinString + AltKeyName;
			}

			return string.IsNullOrEmpty(result) ? string.Empty : result.Substring(keyJoinString.Length);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key names")]
		const string ControlKeyName = "Ctrl";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key names")]
		const string ShiftKeyName = "Shift";
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key names")]
		const string AltKeyName = "Alt";

		const string keyJoinString = " + ";

		#endregion

		#region Implementation

		readonly ConcurrentDictionary<Keys, HotKeyPressedProcessor> registeredKeys = new ConcurrentDictionary<Keys, HotKeyPressedProcessor>();

		public bool ProcessCmdKey(object sender, Keys keyData)
		{
			var hotKeyMonitor = HotKeyMonitorProvider.GetHotKeyMonitor();
			try
			{
				var processed = registeredKeys.TryGetValue(keyData, out var processor) && processor(sender, keyData);
				if (hotKeyMonitor.Enabled)
				{
					var keyName = GetKeyAsString(keyData);
					var registeredKeysSet = registeredKeys.Keys.Select(k => k.ToString()).ToHashSet();
					var processorName = processor == null ? "" : processor.Method.Name;
					hotKeyMonitor.ProcessCmdKey(sender, keyName, registeredKeysSet, processorName, processed);
				}
				return processed;
			}
			catch (Exception ex)
			{
				var controlPath = string.Empty;
				if (sender is Control control)
				{
					controlPath = ControlDescription.GetControlPath(control);
				}
				var keyDataString = GetKeyAsString(keyData);

				Globals.Message.ShowInformation(Res.GetString("4c23c9d7-54f8-40af-a5e1-28e388149a69", "There has been an error processing the ({0}) hot key.", keyDataString));
				ErrorReporter.ReportOnce("HotkeyProcessCmdKeyError", $"HotkeyProcessCmdKeyError: Control:{controlPath}, keyData:({keyDataString})", ex);
				if (hotKeyMonitor.Enabled)
				{
					hotKeyMonitor.ProcessExceptions(controlPath, keyDataString, ex.ToString());
				}
				throw;
			}
		}

		public bool IsRegistered(Keys keys)
		{
			return registeredKeys.ContainsKey(keys);
		}

		#endregion

		#region Global Hotkeys

#if DEBUG
		public static HotkeyRegister GlobalHotkeys => globalHotkeys.Value ?? (globalHotkeys.Value = new HotkeyRegister());

		static readonly Overridable<HotkeyRegister> globalHotkeys = new Overridable<HotkeyRegister>(null);

#else
		public static HotkeyRegister GlobalHotkeys { get; } = new HotkeyRegister();
#endif

		#endregion
	}

	/// <summary>
	/// The delegate to be run when a hotkey is pressed
	/// </summary>
	/// <param name="sender">The active form that the keypress message was sent to</param>
	/// <param name="keyPressed">The bitmask of the key that was pressed and its modifiers (Keys.Shift, Keys.Control or Keys.Alt)</param>
	/// <returns>Return true if you processed this hotkey and no more processing should be performed, otherwise return false.</returns>
	public delegate bool HotKeyPressedProcessor(object sender, Keys keyPressed);
}
