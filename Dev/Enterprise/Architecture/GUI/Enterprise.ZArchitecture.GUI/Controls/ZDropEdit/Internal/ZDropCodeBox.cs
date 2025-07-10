using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Integration;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Modules;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI.Internal
{
	[ToolboxItem(false)]
	public partial class ZDropCodeBox : ZTextBox
	{
		public ZDropCodeBox() : base()
		{
			Hotkeys.RegisterHotKey(Keys.F3, WrapShowFormHotkey(), Res.GetString("4ff68099-fe12-4f25-ba7e-86816c142147", "Create new (if empty) / Show (if set)"));
		}

		#region Bare

		[ToolboxItem(false)]
		public new class Bare : ZDropCodeBox
		{
			protected override IControlExtensionCollection NewExtensionCollection()
			{
				return new ControlExtensionCollection(this);
			}
		}

		#endregion

#if WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Class name")]
		protected override string ClassName => "zdropcodebox";
#endif

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int MaxLength
		{
			get { return base.MaxLength; }
			set
			{
				var newValue = Math.Max(value, 0);
				if (MaxLength != newValue)
				{
					base.MaxLength = newValue;
				}
			}
		}

		public override bool ShouldAggressivelyTruncateText
		{
			get
			{
				return false;
			}
		}

		#region Implementation

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			return ProcessCmdKeyCore(keyData) || base.ProcessCmdKey(ref msg, keyData);
		}

		protected virtual bool ProcessCmdKeyCore(Keys keyData)
		{
			if (keyData == Keys.Escape || keyData == Keys.Tab)
			{
				var e = new KeyEventArgs(keyData);
				ParentDropEdit.HandleCommandKey(e);

				if (keyData != Keys.Tab && e.Handled)
				{
					return true;
				}
			}
			return false;
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (!ReadOnly && Parent != null)
			{
				((ZDropEdit)Parent).HandleCommandKey(e);
			}

			if (!e.Handled)
			{
				base.OnKeyDown(e);
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			OnTextChangedCore();
		}

		protected virtual void OnTextChangedCore()
		{
			if (ParentDropEdit != null && ParentDropEdit.IsBound && string.IsNullOrEmpty(ParentDropEdit.BindToForDescription))
			{
				UpdateSelectedIndexAndDescription();
			}
		}

		internal ModuleIdentifier ModuleID { get; set; }

		internal string MappingName { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1058:DoNotUseDebuggerIsAttached", Justification = "If the debugger is attached with breakpoints, work items will always be long running")]
		internal void UpdateSelectedIndexAndDescription()
		{
			if (ParentDropEdit != null && ParentDropEdit.IsBound && string.IsNullOrEmpty(ParentDropEdit.BindToForDescription))
			{
				if (ParentDropEdit.EnableTimeRecording)
				{
					var stopWatch = new Stopwatch();
					var currentListNumber = ParentDropEdit.ListNumberForLogging();
					var reportKey = string.Format(CultureInfo.InvariantCulture, "SlowMethodOnZDropEditFindItemExact.{0}.{1}.{2}", ParentDropEdit.ParentForm?.Name ?? "UNK_FORM", ParentDropEdit.Name, DataMember);
					if (ShouldReportLongRunningControls(reportKey))
					{
						ParentDropEdit.EnablePullListCostTimeRecording = true;
					}

					stopWatch.Start();
					var item = FindItem(Text, AutoCompleteStringComparisonType.Exact);
					var pullListCost = ParentDropEdit.PullListCost;
					ParentDropEdit.PullListCost = 0;
					stopWatch.Stop();

					ParentDropEdit.EnablePullListCostTimeRecording = false;
					if (stopWatch.Elapsed.TotalSeconds > 5 && !Debugger.IsAttached) // If the debugger is attached with breakpoints, work items will always be long running
					{
						var callstack = new StackTrace().ToString();
						ReportLongRunningControls(reportKey, ErrorMessageForLongtimeRunningMethod("FindItemExact", stopWatch.Elapsed.TotalSeconds, currentListNumber, pullListCost), callstack); // Error Report
					}

					stopWatch.Reset();
					currentListNumber = ParentDropEdit.ListNumberForLogging();
					if (item != null)
					{
						if (ParentDropEdit.ShowDescriptionBox)
						{
							UpdateDescription(item.Description);
						}
						stopWatch.Start();
						ParentDropEdit.OnSelectedIndexChanged(ParentDropEdit.List == null ? -1 : ParentDropEdit.List.IndexOf(item));
					}
					else
					{
						if (ParentDropEdit.ShowDescriptionBox)
						{
							ParentDropEdit.SetEmptyDescription();
						}
						stopWatch.Start();
						ParentDropEdit.OnSelectedIndexChanged(-1);
					}
					stopWatch.Stop();
					if (stopWatch.Elapsed.TotalSeconds > 5 && !Debugger.IsAttached) // If the debugger is attached with breakpoints, work items will always be long running
					{
						ReportLongRunningControls(string.Format(CultureInfo.InvariantCulture, "SlowMethodOnZDropEditSelectedIndexChangedEvent.{0}.{1}.{2}", ParentDropEdit.ParentForm?.Name ?? "UNK_FORM", ParentDropEdit.Name, DataMember), ErrorMessageForLongtimeRunningMethod("SelectedIndexChanged", stopWatch.Elapsed.TotalSeconds, currentListNumber, 0), ""); // Error Report
					}
				}
				else
				{
					var item = FindItem(Text, AutoCompleteStringComparisonType.Exact);

					if (item != null)
					{
						if (ParentDropEdit.ShowDescriptionBox)
						{
							UpdateDescription(item.Description);
						}
						ParentDropEdit.OnSelectedIndexChanged(ParentDropEdit.List == null ? -1 : ParentDropEdit.List.IndexOf(item));
					}
					else
					{
						if (ParentDropEdit.ShowDescriptionBox)
						{
							ParentDropEdit.SetEmptyDescription();
						}
						ParentDropEdit.OnSelectedIndexChanged(-1);
					}
				}
			}
		}

		protected virtual void UpdateDescription(string description)
		{
			ParentDropEdit.DescriptionBox.Text = description;
		}

		void ReportLongRunningControls(string key, string message, string callstack)
		{
			var nowUtc = ZDateTime.UtcNow;

			if (ShouldReportLongRunningControls(key))
			{
				var earliestFailureDate = nowUtc.AddMinutes(-1);
				var previousCallStack = ReportedLongRunningControls[key].Item2;
				message += $"Previous CallStack: \r\n {previousCallStack}";
				ErrorReporter.ReportOnce(key, message);
				ReportedLongRunningControls.Where(x => x.Value.Item1 < earliestFailureDate).ToList().ForEach(x => ReportedLongRunningControls.Remove(x.Key));
			}
			
			ReportedLongRunningControls[key] = (nowUtc, callstack);
		}

		bool ShouldReportLongRunningControls(string key)
		{
			(ZDateTime, string) value;
			var nowUtc = ZDateTime.UtcNow;

			if (ReportedLongRunningControls.TryGetValue(key, out value))
			{
				var lastReportedTime = value.Item1;
				var earliestFailureDate = nowUtc.AddMinutes(-1);
				if (lastReportedTime > earliestFailureDate)
				{
					return true;
				}
			}
			return false;
		}

#if DEBUG
		internal
#endif
		static Dictionary<string, (ZDateTime, string)> ReportedLongRunningControls
		{
			get { return reportedLongRunningControls ?? (reportedLongRunningControls = new Dictionary<string, (ZDateTime, string)>()); }
		}
		[ThreadStatic]
		static Dictionary<string, (ZDateTime, string)> reportedLongRunningControls;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception reporting")]
		string ErrorMessageForLongtimeRunningMethod(string methodName, double totalSeconds, int originListNumber, double pullListCost)
		{
			var message = string.Format(CultureInfo.InvariantCulture,
				@"UpdateSelectedIndexAndDescription took {0} seconds to process (twice within 60 seconds) {1}, including {2} seconds of ParentDropEdit.PullList.
The Name of ParentDropEdit is {3}.
The Name of bound property is {4}.
Current entered text is {5}.
ParentDropEdit.List: {6}.
ListNumberBeforeFindItem: {7}, ModuleID: {8}.
ParentDropEdit.BindToList: {9}.
MappingName: {10}
",
				totalSeconds,
				methodName,
				pullListCost,
				ParentDropEdit.Name,
				DataMember,
				Text,
				ParentDropEdit.List == null
					? "list is null"
					: string.Format(CultureInfo.InvariantCulture, "list type is {0}, and count is {1}",
						ParentDropEdit.List.GetType(),
						ParentDropEdit.List.Count),
				originListNumber,
				ModuleID?.Name,
				ParentDropEdit?.BindToList,
				MappingName);
			return message;
		}

#if !WINZOR
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			// need to check here whether or not we should continue
			if (!ReadOnly)
			{
				if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == ' ' || e.KeyChar == '-' || e.KeyChar == '.')
				{
					AutoCompleteText(e);
				}
				else if ((Keys)e.KeyChar == Keys.Back)
				{
					string newText;
					var newSelectionStart = Math.Max(SelectionStart - 1, 0);

					if (SelectionLength > 0)
					{
						newText = Text.Substring(0, SelectionStart) + Text.Substring(SelectionStart + SelectionLength);
						newSelectionStart = SelectionStart;
					}
					else if (SelectionStart > 0)
					{
						newText = Text.Substring(0, SelectionStart - 1) + Text.Substring(SelectionStart + SelectionLength);
					}
					else
					{
						newText = Text;
					}

					var item = FindItem(newText, AutoCompleteStringComparisonType.Exact);
					if (item != null)
					{
						Text = ParentDropEdit.GetMultilingualValue(item);
						ParentDropEdit.UpdateDropDown();
					}
					else
					{
						Text = newText;
					}

					SelectionStart = newSelectionStart;
					e.Handled = true;
				}
			}

			base.OnKeyPress(e);
		}
#endif
		ICodeDescription FindItem(string text, AutoCompleteStringComparisonType type)
		{
#if DEBUG
			if (ParentDropEdit.TestRunFindItemWithLongTime)
			{
				Thread.Sleep(TimeSpan.FromSeconds(6));
			}
#endif
			return AutocompleteSearchHelper.FindItem(ParentDropEdit, ParentDropEdit.List, text, type);
		}

		internal virtual void AutoCompleteText(KeyPressEventArgs e)
		{
			var oldSelectionStart = SelectionStart;

			var keyChar = new String(e.KeyChar, 1);
			if (CharacterCasing == CharacterCasing.Upper)
			{
				keyChar = keyChar.ToUpperInvariant();
			}
			else if (CharacterCasing == CharacterCasing.Lower)
			{
				keyChar = keyChar.ToLowerInvariant();
			}

			var startText = new ZString(Text).SubstringSafe(0, oldSelectionStart) + keyChar;
			string endText = new ZString(Text).SubstringSafe(SelectionStart + SelectionLength);
			var newText = startText + endText;

			if (MaxLength == -1 || MaxLength == 0 || newText.Length <= MaxLength)
			{
				var item = FindItem(startText, AutoCompleteStringComparisonType.StartsWith);
				if (item != null)
				{
					ParentDropEdit.SetSelection(item, oldSelectionStart + 1);
					ParentDropEdit.UpdateDropDown();
				}
				else
				{
					Text = newText;
					Select(oldSelectionStart + 1, 0);
				}
			}
			e.Handled = true;
		}

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if ((m.Msg == WindowsMessage.WM_CUT || m.Msg == WindowsMessage.WM_PASTE) && ParentDropEdit != null)
			{
				ParentDropEdit.UpdateDropDown();
			}
		}

#endif

		protected override void OnMaxLengthChanged()
		{
			base.OnMaxLengthChanged();
			if (ParentDropEdit.ShouldResizeByMaxLength)
			{
				ParentDropEdit.SetControlSize(MaxLength);
			}
		}

		HotKeyPressedProcessor WrapShowFormHotkey()
		{
			return (o, k) =>
			{
				return ShowFormHotkey();
			};
		}

		bool ShowFormHotkey()
		{
			var result = ParentDropEdit.ShowEditOrViewForm();
			if (result)
			{
				UserEventTracker.Instance.AddUserEvent(this, "KeyPress", "F3");
			}
			return result;
		}

		protected ZDropEdit ParentDropEdit
		{
			get { return (ZDropEdit)Parent; }
		}

		#endregion
	}
}
