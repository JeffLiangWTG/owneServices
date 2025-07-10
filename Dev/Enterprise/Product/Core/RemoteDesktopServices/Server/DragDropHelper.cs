using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class DragDropHelper
	{
		protected DragDropHelper()
		{ }

		public static DragDropHelper Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new DragDropHelper();
				}
				return instance;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static DragDropHelper instance;
		public readonly Dictionary<string, Form> associatedForms = new Dictionary<string, Form>();

		// Steps to find the target window: 
		// 1. If the hwnd exists in AssociatedForms use it.
		// 2. Search the window matching the title in all opened windows. Save it in AssociatedForms if found.
		// 3. If the title has no match and starts with '(' use active form if not null but do NOT save it in AssociatedForms.
		// 4. Otherwise if active form is null, show 'SelectionForm' and save user's choice to AssociatedForms.
		// 5. If the title has no match and does NOT start with '(', it means the message is a broadcast and this CW1 instance is not the expected receiver therefore save the hwnd with a null value in AssociatedForms.
		public Form HandleRemoteWindowTitleAndHandleMessage(string message, bool shouldShowSelectionFormIfNotFound = true, bool useCache = true)
		{
			Form waitingActiveForm = null;
			try
			{
				currentActiveForm = GetCurrentActiveForm();
				logBuilder = TrackingInfoLogger.Instance.HasListener ? new StringBuilder() : null;
				(var clientFormHwnd, var clientFormText) = GetFormHandleAndText(message);
				if (shouldShowSelectionFormIfNotFound && EnvProxy.Instance.CurrentUser.IsController && EnvProxy.Instance.Registry.RemoteAppAlwaysShowSelectionFormOnFileDrop)
				{
					waitingActiveForm = ShowSelectionForm();
				}
				else
				{
					waitingActiveForm = FindAndSetSuitableActiveForm(clientFormHwnd, clientFormText, shouldShowSelectionFormIfNotFound, useCache);
				}

				if (waitingActiveForm != null && !waitingActiveForm.IsDisposed)
				{
					Application.UseWaitCursor = true;
					return waitingActiveForm;
				}
				return null;
			}
			finally
			{
				DisplayLog(waitingActiveForm);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		(string, string) GetFormHandleAndText(string message)
		{
			string[] parsedMessages = null;
			try
			{
				parsedMessages = WindowCaptionUtils.Base64DecodeCaptionAndHandle(message);
			}
			catch (FormatException)
			{
				parsedMessages = message.Split(new string[] { "|||" }, StringSplitOptions.None);
			}

			var text = parsedMessages[0];
			var hwnd = string.Empty;
			if (parsedMessages.Length != 2 && InitializationMessageHandler.RemoteVersion >= messageWithHwndEarliestSupportedVersion)
			{
				var clientInfo = (ObjectFactory.Get<TerminalService>().IsCitrixICA ? "Citrix " : "RDP ") + InitializationMessageHandler.RemoteVersion;
				ErrorReporter.ReportOnce("StartDropMessageMissingFormHandle", $"Failed to parse. The message is from {clientInfo}, and the content is: " + message);
			}
			else
			{
				hwnd = parsedMessages[1];
			}
			return (hwnd, text);
		}

		Form FindAndSetSuitableActiveForm(string hwnd, string text, bool shouldShowSelectionFormIfNotFound, bool useCache)
		{
			logBuilder?.AppendLine("##################################");

			if (!useCache || !associatedForms.TryGetValue(hwnd, out var waitingActiveForm))
			{
				waitingActiveForm = NewFormRequest(hwnd, text, shouldShowSelectionFormIfNotFound);
			}
			else
			{
				logBuilder?.AppendLine($"Use cached form.\r\n	Hwnd: {hwnd}\r\n	Text: {waitingActiveForm?.Text}");
			}
			return waitingActiveForm;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		Form NewFormRequest(string hwnd, string text, bool shouldShowSelectionFormIfNotFound)
		{
			logBuilder?.AppendLine($"New Hwnd Detected.\r\n	Hwnd: {hwnd}\r\n	Text: {text}");

			//reset shouldRemember
			shouldRemember = false;

			var waitingActiveForm = GetOpenFormMatchingText(formText => !string.IsNullOrWhiteSpace(formText) && text.StartsWith(formText, StringComparison.Ordinal));

			if (waitingActiveForm == null && !string.IsNullOrWhiteSpace(text) && ObjectFactory.Get<TerminalService>().IsCitrixICA)
			{
				const string citrixSuffix = @"- \\Remote";
				int lastIndexOfSuffix = text.LastIndexOf(citrixSuffix, StringComparison.OrdinalIgnoreCase);
				if (lastIndexOfSuffix > 0 && lastIndexOfSuffix < text.Length)
				{
					text = text.Remove(lastIndexOfSuffix).TrimEnd();
				}

				waitingActiveForm = GetOpenFormMatchingText(formText => !string.IsNullOrWhiteSpace(formText) && formText.StartsWith(text, StringComparison.Ordinal));
			}

			if (waitingActiveForm == null)
			{
				var emptyFormText = string.IsNullOrWhiteSpace(text) || text.First(c => !char.IsWhiteSpace(c)).Equals('(');
				if (emptyFormText)
				{
					waitingActiveForm = FormNotFound(shouldShowSelectionFormIfNotFound);
				}
				else
				{
					shouldRemember = true;
					logBuilder?.AppendLine("Cannot find a current form matching this form text.");
				}
			}

			RememberResult(hwnd, waitingActiveForm);
			return waitingActiveForm;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		Form FormNotFound(bool shouldShowSelectionFormIfNotFound)
		{
			logBuilder?.AppendLine("Cannot find waitingActiveForm because form text is empty.");
			Form waitingActiveForm = null;

			if (currentActiveForm != null)
			{
				waitingActiveForm = UseCurrentActiveForm();
			}
			else if (shouldShowSelectionFormIfNotFound)
			{
				waitingActiveForm = ShowSelectionForm();
			}
			return waitingActiveForm;
		}

		Form GetOpenFormMatchingText(Func<string, bool> checkIsMatch)
		{
			foreach (var form in ZApplication.GetOpenForms())
			{
				try
				{
					var windowTitle = form.Text;
					if (checkIsMatch(windowTitle))
					{
						shouldRemember = true;
						return form;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					logBuilder?.AppendLine($"Ignored an error getting window title to find a matching form: {ex.Message}");
				}
			}

			return null;
		}

		Form UseCurrentActiveForm()
		{
			logBuilder?.AppendLine($"Use currentActiveForm: {currentActiveForm.Text}.");
			return currentActiveForm;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		Form ShowSelectionForm()
		{
			logBuilder?.AppendLine("Show selection form.");
			using (var selectionForm = new CurrentOpenedJobsSelectionForm(ZApplication.GetOpenForms()))
			{
				selectionForm.TopMost = true;
				if (HasActionOnSelectionFormShown)
				{
					selectionForm.Shown += SelectionForm_Shown;
				}
				selectionForm.ShowDialog();
				shouldRemember = true;
				return selectionForm.SelectedForm;
			}
		}

		void RememberResult(string hwnd, Form waitingActiveForm)
		{
			if (shouldRemember && !string.IsNullOrWhiteSpace(hwnd))
			{
				logBuilder?.AppendLine($"Save form to cache. Handle: {hwnd}.");
				associatedForms.Remove(hwnd);
				associatedForms.Add(hwnd, waitingActiveForm);

				if (waitingActiveForm != null)
				{
					waitingActiveForm.Disposed += (s, e) => associatedForms.Remove(hwnd);
				}
			}
		}

		void DisplayLog(Form waitingActiveForm)
		{
			logBuilder?.AppendLine($"Final result: {waitingActiveForm?.Text}.");

			TrackingInfoLogger.Instance.NewLog(() => logBuilder?.ToString());
			TrackingInfoLogger.Instance.ShowAll();
		}

		void SelectionForm_Shown(object sender, EventArgs e)
		{
			var form = (CurrentOpenedJobsSelectionForm)sender;
			SelectionForm_ShownCore(form.okButton);
		}

		protected virtual void SelectionForm_ShownCore(Button okButton)
		{
			throw new InvalidOperationException();
		}

		StringBuilder logBuilder;
		bool shouldRemember;
		Form currentActiveForm;

		protected virtual bool HasActionOnSelectionFormShown => false;

		protected virtual Form GetCurrentActiveForm() => Form.ActiveForm;

		readonly Version messageWithHwndEarliestSupportedVersion = ObjectFactory.Get<TerminalService>().IsCitrixICA ? new Version(1, 4, 0) : new Version(4, 9, 3);
	}
}
