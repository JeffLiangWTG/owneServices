using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[ImmutableObject(true)]
	public class HotKeyMonitor : IHotKeyMonitor
	{
		#region Enabled

		bool IHotKeyMonitor.Enabled
		{
			get { return enabled; }
			set
			{
				enabled = value; 
			}
		}

		bool enabled;

		#endregion

		#region ProcessCmdKey

		readonly SortedDictionary<string, KeyPressSpan> keyPressSpans = new ();

		public void ProcessCmdKey(object sender, string keyName, HashSet<string> registeredKeys, string processorName, bool processed)
		{
			if (enabled)
			{
				InsertCmdKeyRecord(sender, keyName, registeredKeys, processorName, processed);
			}
		}

		public void ProcessExceptions(string controlName, string keyName, string callStacks)
		{
			if (enabled)
			{
				var newEvent = new KeyPressEvent(controlName, keyName, callStacks);
				LastSpan.AddKeyPressEvent(newEvent);
			}
		}

		public string GetEnvironmentInfo()
		{
			var env = EnvProxy.Instance;
			var builder = new StringBuilder();
			builder.AppendLine((NoResString)"*** Environment Info ***");
			builder.AppendLine();
			builder.AppendLine($"LoginName: [{EnvProxy.Instance.CurrentUser?.LoginName}]");
			builder.AppendLine($"Company: [{env.CurrentCompany.Name}]");
			builder.AppendLine($"Branch: [{env.CurrentBranch.Name}]");
			builder.AppendLine($"Department: [{env.CurrentDepartment.Description}]");
			builder.AppendLine($"MachineName: [{System.Environment.MachineName}]");
			builder.AppendLine($"VersionNumber: [{ReleaseInfo.Instance.VersionNumber.ToString()}]");
			builder.AppendLine("OpenedForms");
			builder.AppendLine($"{PrintOpenedForms(ZApplication.GetOpenForms())}");
			builder.AppendLine("GlobalHotKeys");
			builder.AppendLine($"{PrintGlobalHotKeys(HotkeyRegister.GlobalHotkeys.Descriptions)}");
			return builder.ToString();
		}

		void InsertCmdKeyRecord(object sender, string keyName, HashSet<string> registeredKeys, string processorName, bool processed)
		{
			if (sender is Control control)
			{
				var foundForm = control.FindForm();
				var normalizedName = NormalizeName(foundForm);
				var lastSpan = GetOrCreateLastSpan(normalizedName, registeredKeys);
				var newEvent = new KeyPressEvent(control.Name, keyName, processorName, processed);
				lastSpan.AddKeyPressEvent(newEvent);
			}
		}

		string NormalizeName(Form form)
		{
			var formName = form.Name;
			if (form is IMainForm mainForm)
			{
				var currentModule = mainForm.CurrentModule;
				return currentModule == null ? formName : $"{formName}-{currentModule.Description}";
			}

			return formName;
		}

		KeyPressSpan LastSpan => keyPressSpans.Last().Value;

		KeyPressSpan GetOrCreateLastSpan(string normalizedName, HashSet<string> registeredKeys)
		{
			if (!keyPressSpans.Any() || !LastSpan.ControlName.Equals(normalizedName))
			{
				var newSpan = new KeyPressSpan(normalizedName, registeredKeys);
				keyPressSpans.Add(GetCurrentDateTime(), newSpan);
				return newSpan;
			}

			LastSpan.AddHotKeys(registeredKeys);
			return LastSpan;
		}

		#endregion

		#region Get Key Press Spans

		public SortedDictionary<string, string> GetAllSpans()
		{
			var result = new SortedDictionary<string, string>();
			foreach (var kvp in keyPressSpans)
			{
				result[$"{kvp.Key} | {kvp.Value.ControlName}"] = kvp.Value.ToString();
			}
			return result;
		}

		public SortedDictionary<string, string> GetFilteredSpans()
		{
			var result = new SortedDictionary<string, string>();
			foreach (var kvp in keyPressSpans)
			{
				var cloned = kvp.Value.FilteredClone();
				if (!cloned.KeyPressEvents.IsNullOrEmpty())
				{
					result[$"{kvp.Key} | {kvp.Value.ControlName}"] = cloned.ToString();
				}
			}
			return result;
		}

		#endregion

		#region Clear All

		public void ClearAll()
		{
			keyPressSpans.Clear();
		}

		#endregion

		#region Helper

		public static string GetCurrentDateTime()
		{
			return ZDateTime.Now.ToString("O", CultureInfo.CurrentCulture);
		}

		public string GetLatestRecordTime()
		{
			if (keyPressSpans.Any())
			{
				var lastKeyPressEvents = LastSpan.KeyPressEvents;
				return lastKeyPressEvents.Any() ? lastKeyPressEvents.Last().CurrentDateTime : string.Empty;
			}

			return string.Empty;
		}

		public string PrintOpenedForms(Form[] forms)
		{
			var builder = new StringBuilder();
			forms.ForEach(e => builder.AppendLine($"\t- {e.Name}"));
			return builder.ToString();
		}

		public string PrintGlobalHotKeys(IEnumerable<KeyValuePair<string, string>> descriptions)
		{
			var builder = new StringBuilder();
			descriptions.ForEach(e => builder.AppendLine($"\t- {e.ToString()}"));
			return builder.ToString();
		}

		#endregion
	}
}
