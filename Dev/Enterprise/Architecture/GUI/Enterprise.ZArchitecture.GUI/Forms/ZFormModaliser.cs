using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	#region IModalDialog Interface

	public interface INoActivateModalForm
	{
	}

	#endregion

	public class ZFormModaliser
	{
		#region Constructor

		protected ZFormModaliser()
		{
			activeMessageBoxStack = new Stack<Form>();
			activeMessageBoxParentFormStack = new Stack<Form>();
			closingMessageBoxList = new List<Form>();
		}

		protected static internal ZFormModaliser Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new ZFormModaliser();
				}
				return instance;
			}
		}
		[ThreadStatic]
		static ZFormModaliser instance;

#if DEBUG
		/// <summary>
		/// Suspends disposal of all forms shown using any of the Show??WithDispose methods
		/// </summary>
		/// <returns></returns>
		public static IDisposable SuspendDispose()
		{
			Instance.DisposeSuspendCount++;
			return new DisposableAction(new Action(() => Instance.DisposeSuspendCount--));
		}

		int disposeSuspendCount;
		internal int DisposeSuspendCount
		{
			get
			{
				return disposeSuspendCount;
			}
			set
			{
				disposeSuspendCount = value;
				if (disposeSuspendCount == 0)
				{
					foreach (var disposable in suspendedDisposables)
					{
						disposable.Dispose();
					}
					suspendedDisposables.Clear();
				}
			}
		}
		readonly List<IDisposable> suspendedDisposables = new List<IDisposable>();

		internal static void SetInstanceForTest(ZFormModaliser newInstance)
		{
			instance = newInstance;
		}

		public static Form LastFormShownDialogForTest
		{
			get => lastFormShownDialogForTest.Value;
			set => lastFormShownDialogForTest.Value = value;
		}
		readonly static ThreadLocalOverridable<Form> lastFormShownDialogForTest = new ThreadLocalOverridable<Form>(disposeIfIDisposable: false);

		public static IBusiness LastIBusinessShownOnDialogForTest
		{
			get => lastIBusinessShownOnDialogForTest.Value;
			set => lastIBusinessShownOnDialogForTest.Value = value;
		}
		readonly static ThreadLocalOverridable<IBusiness> lastIBusinessShownOnDialogForTest = new ThreadLocalOverridable<IBusiness>();

		public static Form LastFormShownForTest
		{
			get { return lastFormShownForTest == null ? null : lastFormShownForTest.Target as Form; }
			set { lastFormShownForTest = new WeakReference(value); }
		}
		[ThreadStatic]
		static WeakReference lastFormShownForTest;

		/// <summary>
		/// Same as LastFormShownForTest, not the actual active form, for UnitTest only
		/// </summary>
		public static Form ActiveForm
		{
			get { return (LastFormShownForTest != null && LastFormShownForTest.Owner != null) ? LastFormShownForTest : null; }
		}

		public static DialogResult ResultToReturnFromShowDialog
		{
			get => resultToReturnFromShowDialog.Value;
			set => resultToReturnFromShowDialog.Value = value;
		}
		static readonly Overridable<DialogResult> resultToReturnFromShowDialog = new Overridable<DialogResult>(DialogResult.None);

		public static string FileNameToSelectInShowCommonDialog
		{
			get { return getFileNameToSelectInShowCommonDialog.Value == null ? fileNameToSelectInShowCommonDialog.Value : getFileNameToSelectInShowCommonDialog.Value(); }
			set
			{
				getFileNameToSelectInShowCommonDialog.Value = null;
				fileNameToSelectInShowCommonDialog.Value = value;
			}
		}
		static readonly Overridable<Func<string>> getFileNameToSelectInShowCommonDialog = new Overridable<Func<string>>();
		static readonly Overridable<string> fileNameToSelectInShowCommonDialog = new Overridable<string>();

		[SuppressThreadStaticFieldMessage]
		public static string PathToSelectInShowCommonDialog;

		public static bool ShowDialogsInTest
		{
			get => showDialogsInTest.Value;
			set => showDialogsInTest.Value = value;
		}
		static readonly Overridable<bool> showDialogsInTest = new Overridable<bool>(false);

		public static CommonDialog LastCommonDialogShownDialogForTest
		{
			get => lastCommonDialogShownDialogForTest.Value;
			set => lastCommonDialogShownDialogForTest.Value = value;
		}
		static readonly Overridable<CommonDialog> lastCommonDialogShownDialogForTest = new Overridable<CommonDialog>();

#endif
		#endregion

		#region DelegateToCallBeforeShowingFormsOrDialogs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer error message")]
		public static void SetDelegateToCallBeforeShowingFormsOrDialogs(PreShowInvoker delegateToCall)
		{
			if (Instance.delegateToCallBeforeShowingFormsOrDialogs.Value != null)
			{
				if (Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.Value.Count() >= MaximumDelegatesToStack)
				{
					var message = "SetDelegateToCallBeforeShowingFormsOrDialogs called when delegate already set too many times.";
					Globals.Message.ShowDeveloperErrorOnce("SetDelegateToCallBeforeShowingFormsOrDialogs", message, "");
				}
				Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.Value = Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.Value.Push(Instance.delegateToCallBeforeShowingFormsOrDialogs.Value);
			}
			Instance.delegateToCallBeforeShowingFormsOrDialogs.Value = delegateToCall;
		}

		public static IDisposable SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(PreShowInvoker invoker)
		{
			return new DisposableAction(
				() => Instance.temporaryDelegateToCallBeforeShowingFormsOrDialogs.Value = invoker,
#pragma warning disable IDE0200 // Lambda expression can be removed
				() => Instance.temporaryDelegateToCallBeforeShowingFormsOrDialogs.ResetValue()
#pragma warning restore IDE0200
				);
		}

		public static void SetDelegateToCallOnFormShown(PreShowInvoker delegateToCall)
		{
			SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var form = dialog as Form;

				if (form != null)
				{
					form.Shown += (sender, args) => delegateToCall.Invoke(dialog);
				}
			});
		}

		public static void SetDelegateToCallOnFormClosing(PreShowInvoker delegateToCall)
		{
			SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
			{
				var form = dialog as Form;

				if (form != null)
				{
					form.Closing += (sender, args) => delegateToCall.Invoke(dialog);
				}
			});
		}

#if DEBUG
		public static void ClearDelegateToCallBeforeShowingFormsOrDialogsAndClearStackForTest()
		{
			Instance.delegateToCallBeforeShowingFormsOrDialogs.ResetValue();
			Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.ResetValue();
		}
#endif
		public static void ClearDelegateToCallBeforeShowingFormsOrDialogs()
		{
			if (!Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.Value.IsEmpty)
			{
				Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.Value = Instance.stackedDelegatesToCallBeforeShowingFormsOrDialogs.Value.Pop(out var value);
				Instance.delegateToCallBeforeShowingFormsOrDialogs.Value = value;
			}
			else
			{
				Instance.delegateToCallBeforeShowingFormsOrDialogs.Value = null;
			}
		}

		static void CallPreShowDelegateIfActive(object form)
		{
			Instance.delegateToCallBeforeShowingFormsOrDialogs.Value?.Invoke(form);
			Instance.temporaryDelegateToCallBeforeShowingFormsOrDialogs.Value?.Invoke(form);
		}

		readonly Overridable<PreShowInvoker> delegateToCallBeforeShowingFormsOrDialogs = new Overridable<PreShowInvoker>();
		readonly Overridable<PreShowInvoker> temporaryDelegateToCallBeforeShowingFormsOrDialogs = new Overridable<PreShowInvoker>();

		public delegate void PreShowInvoker(object formOrDialog);

		readonly Overridable<ImmutableStack<PreShowInvoker>> stackedDelegatesToCallBeforeShowingFormsOrDialogs = new Overridable<ImmutableStack<PreShowInvoker>>(ImmutableStack<PreShowInvoker>.Empty);

#if DEBUG
		internal
#endif
		const int MaximumDelegatesToStack = 3;

		#endregion

		#region FormsModalityRelationCollection

		public static Form GetActiveChildFormForParentForm(Form parentForm)
		{
			return parentForm.OwnedForms.Length > 0 ? parentForm.OwnedForms[0] : null;
		}

		public static Form GetParentFormForModalForm(Form modalForm)
		{
			return modalForm.Owner;
		}

		#endregion

		#region Show

		public static void Show(Form formToShowModally, Form parentForm)
		{
			if (formToShowModally != null)
			{
				CallPreShowDelegateIfActive(formToShowModally);
				Instance.ShowInternal(formToShowModally, parentForm);
			}
		}

		void ShowInternal(Form formToShowModally, Form parentForm)
		{
			if (parentForm == null)
			{
				parentForm = GetBestParentFormForModalShow();
			}

			new ParentChildTracker(formToShowModally, parentForm);
#if DEBUG
			LastFormShownForTest = formToShowModally;
#endif
		}

#if DEBUG
		internal
#endif
		Form GetBestParentFormForModalShow()
		{
			if (ApplicationActiveForm != null)
			{
				return ApplicationActiveForm;
			}
			else
			{
				foreach (var form in ZApplication.GetOpenForms())
				{
					if (form is IMainForm && !(form is ZChildForm))
					{
						return form;
					}
				}
			}

			throw new Exception("Could not find a parent form for modal show");
		}

		#region Enable/Disable Form

		public static void EnableForm(ZForm form, bool enable)
		{
			if (!form.IsDisposed)
			{
				if (enable)
				{
					form.UpdateEnableCount();
					if (form.IsEnabledCore)
					{
						EnableWindow(form, true);
					}
				}
				else
				{
					if (form.IsEnabledCore)
					{
						EnableWindow(form, false);
					}
					form.UpdateDisableCount();
				}
			}
		}

		internal static IDisposable DisableAllFormsButOne(ZForm form)
		{
			var disabledForms = new List<Form>();

			foreach (var openForm in ZApplication.GetOpenForms())
			{
				if (openForm != form && !openForm.IsDisposed && openForm.IsHandleCreated && !openForm.InvokeRequired && openForm.Visible && IsWindowEnabled(openForm))
				{
					var zForm = openForm as ZForm;
					if (zForm != null)
					{
						if (zForm.IsEnabledCore)
						{
							EnableForm(zForm, false);
							disabledForms.Add(openForm);
						}
					}
					else
					{
						EnableWindow(openForm, false);
						disabledForms.Add(openForm);
					}
				}
			}

			return new DisposableAction(() =>
			{
				foreach (var disabledForm in disabledForms)
				{
					if (!disabledForm.IsDisposed)
					{
						var zForm = disabledForm as ZForm;
						if (zForm != null)
						{
							EnableForm(zForm, true);
						}
						else
						{
							EnableWindow(disabledForm, true);
						}
					}
				}
			});
		}

#endregion

				#region ParentChildTracker

		/// <summary>
		/// Handles events on a modal form that require the owner to be disabled while the form is shown.
		/// It ensures the owner form can always be found.
		/// The Form.Owner property is insufficient for this since it is set to null before the Disposed event is called.
		/// </summary>
#if DEBUG
		public
#endif
		class ParentChildTracker
		{
			internal ParentChildTracker(Form formToShowModally, Form parentForm)
			{
				if (formToShowModally != null && parentForm != null
					&& !formToShowModally.IsDisposed && !formToShowModally.Disposing && !parentForm.IsDisposed && !parentForm.Disposing)
				{
					form = formToShowModally;
					owner = parentForm;
					form.Owner = owner;

#if !WINZOR
					// Owner should be set to null during formToShowModally closing in case of OutOfMemoryException.
					// So try to use ParentFormInZFormModaliser to activate parentForm
					if (formToShowModally is ZForm zformToShowModally)
					{
						zformToShowModally.ParentFormInZFormModaliser = parentForm;
					}
#endif

					form.Disposed += form_Disposed;

					// In case the form is hidden rather than closing it...
					form.VisibleChanged += form_VisibleChanged;

					// Note, the doco says Form.Close() disposes the form, but just in case it doesn't
					form.FormClosing += form_FormClosing;
					ZForm.QuietlyShowForm(form);
					form.Focus();
					var zOwner = owner as ZForm;
					if (zOwner != null)
					{
						EnableForm(zOwner, false);
					}
					else
					{
						if (owner != null)
						{
							EnableWindow(owner, false);
						}
					}
				}
			}

			Form form;
			Form owner;

			void form_Disposed(object sender, EventArgs e)
			{
				UnModalise(true);
			}

			void form_VisibleChanged(object sender, EventArgs e)
			{
				if (!form.Visible)
				{
					UnModalise(false);
				}
			}

			void form_FormClosing(object sender, FormClosingEventArgs e)
			{
				if (!e.Cancel)
				{
					UnModalise(false);
				}
			}

			void UnModalise(bool isDisposed)
			{
				if (form != null)
				{
					// Don't access event handlers if we're in the Dispose handler
					// since it prevents child controls from being garbage collected
					if (!isDisposed)
					{
						form.Disposed -= form_Disposed;
						form.VisibleChanged -= form_VisibleChanged;
						form.FormClosing -= form_FormClosing;
						form.Owner = null;
					}
					if (owner != null)
					{
						if (!owner.IsDisposed)
						{
							var zOwner = owner as ZForm;
							if (zOwner != null)
							{
								EnableForm(zOwner, true);
							}
							else
							{
								EnableWindow(owner, true);
							}
#if !WINZOR
							SetForegroundWindow(owner.Handle);
#endif
						}
					}
					form = null;
					owner = null;
				}
			}
		}

#endregion

#endregion

		#region ShowMessageBox

		public static void ActivateMessageBoxIfShowing()
		{
			var dialog = instance.GetActiveMessageBox();
			if (dialog != null)
			{
				dialog.Activate();
			}
		}

		public static DialogResult ShowMessageBoxWithoutDispose(Form messageBox, Form parentForm = null)
		{
			CallPreShowDelegateIfActive(messageBox);
			return Instance.ShowMessageBoxInternalWithoutDispose(messageBox, parentForm);
		}

		DialogResult ShowMessageBoxInternalWithoutDispose(Form messageBox, Form parentForm)
		{
			var result = DialogResult.None;

			if (messageBox != null)
			{
				activeMessageBoxParentFormStack.Push(ApplicationActiveForm);
				CancelEventHandler closingHandler = delegate
				{ closingMessageBoxList.Add(messageBox); };
				messageBox.Closing += closingHandler;

				try
				{
					activeMessageBoxStack.Push(messageBox);

					try
					{
#if DEBUG
						var zMessageBox = messageBox as ZMessageBox;
						if (Globals.IsTest && zMessageBox != null)
						{
							LastFormShownDialogForTest = messageBox;
							result = zMessageBox.ShowDialogForTest();
						}
#if !WINZOR
						else if (NUnit.Framework.TestingState.IsRunningOnDAT && zMessageBox != null)
						{
							throw new InvalidOperationException("Attempted to show a message box on DAT outside of test:\r\n" + zMessageBox.Message + "\r\n" + System.Environment.StackTrace);
						}
#endif
						else
#endif
						{
							result = ShowDialogInternalWithoutDispose(messageBox, parentForm);
						}
					}
					finally
					{
						activeMessageBoxStack.Pop();
					}
				}
				finally
				{
					messageBox.Closing -= closingHandler;
					if (closingMessageBoxList.Contains(messageBox))
					{
						closingMessageBoxList.Remove(messageBox);
					}
					activeMessageBoxParentFormStack.Pop();
				}
			}
			else
			{
				Globals.Message.ShowDeveloperErrorAlways("Null parameter passed to ZFormModaliser. Cannot show modally with null parameters.", "");
			}

			return result;
		}

		#endregion

		#region ShowCommonDialog

		public static DialogResult ShowCommonDialogWithoutDispose(CommonDialog dialog)
		{
			return Instance.ShowCommonDialogInternalWithoutDispose(dialog);
		}

		protected virtual DialogResult ShowCommonDialogInternalWithoutDispose(CommonDialog dialog)
		{
			using (PerformanceStatisticsCollector.Exclude())
			{
				var result = DialogResult.OK;

				if (!Globals.IsTest)
				{
					result = dialog.ShowDialog();
				}
#if DEBUG
				else
				{
					LastCommonDialogShownDialogForTest = dialog;
					result = ResultToReturnFromShowDialog;
					if (dialog is FileDialog)
					{
						((FileDialog)dialog).FileName = FileNameToSelectInShowCommonDialog;
					}
					else if (dialog is FolderBrowserDialog)
					{
						((FolderBrowserDialog)dialog).SelectedPath = PathToSelectInShowCommonDialog;
					}

					CallPreShowDelegateIfActive(dialog);
				}
#endif
				return result;
			}
		}

		public static DialogResult ShowCommonDialogWithoutDispose(IFileDialog dialog, IWin32Window owner = null)
		{
			return Instance.ShowCommonDialogInternalWithoutDispose(dialog, owner);
		}

		protected virtual DialogResult ShowCommonDialogInternalWithoutDispose(IFileDialog dialog, IWin32Window owner)
		{
			using (PerformanceStatisticsCollector.Exclude())
			{
				var result = DialogResult.OK;
				if (!Globals.IsTest)
				{
					result = dialog.ShowDialog(owner);
				}
#if DEBUG
				else
				{
					result = ShowCommonDialogInternalWithoutDispose(dialog.Dialog);
				}
#endif
				return result;
			}
		}

		public static DialogResult ShowCommonDialogWithoutDispose(ZFolderBrowserDialog dialog)
		{
			using (PerformanceStatisticsCollector.Exclude())
			{
				var result = DialogResult.OK;
				if (!Globals.IsTest)
				{
					result = dialog.ShowDialog();
				}
#if DEBUG
				else
				{
					result = Instance.ShowCommonDialogInternalWithoutDispose(dialog.Dialog);
				}
#endif
				return result;
			}
		}

		#endregion

		#region ShowDialog

		public static DialogResult ShowDialogAndDispose(Form form, Form parentForm = null)
		{
			try
			{
#if DEBUG
				if (Instance.DisposeSuspendCount > 0)
				{
					Instance.suspendedDisposables.Add(form);
				}
#endif
				return ShowDialogWithoutDispose(form, parentForm);
			}
			finally
			{
#if DEBUG
				if (Instance.DisposeSuspendCount == 0)
#endif
				{
					form.Dispose();
				}
			}
		}

		public static DialogResult ShowDialogWithoutDispose(Form form, Form parentForm = null)
		{
			if (!Globals.CanShowDialogs)
			{
				ErrorReporter.ReportOnce("ShowModalFormInNonUI_" + form.GetType().Name,
					string.Format("Showing a modal form when the application is not running in UserInteractive mode is not a valid operation.\r\nForm type: {0}\r\nForm caption: {1}",
					form.GetType().FullName, form.Text));
				return DialogResult.Cancel;
			}

			CallPreShowDelegateIfActive(form);
			return Instance.ShowDialogInternalWithoutDispose(form, parentForm);
		}

		DialogResult ShowDialogInternalWithoutDispose(Form form, Form parentForm = null)
		{
			if (form is ZForm zform)
			{
				//normally done by QuietlyShowForm, but we're using ShowDialog, so just do it now
				zform.FormInitialSize();
			}

			using (PerformanceStatisticsCollector.Exclude())
			{
				var result = DialogResult.OK;
				if (!Globals.IsTest)
				{
					result = parentForm == null ? form.ShowDialog() : form.ShowDialog(parentForm);
				}
#if DEBUG
				else
				{
					if (ShowDialogsInTest)
					{
						form.Shown += (sender, e) =>
						{
							form.DialogResult = DialogResult.OK;
							form.Close();
						};
						if (parentForm == null)
						{ form.ShowDialog(); }
						else
						{ form.ShowDialog(parentForm); }
					}

					LastFormShownDialogForTest = form;
					var zForm = form as ZForm;
					LastIBusinessShownOnDialogForTest = (zForm != null) ? zForm.BusinessEntity : null;
					result = ResultToReturnFromShowDialog;
				}
#endif
				return result;
			}
		}

		#endregion

		#region LastActiveForm

		internal static Form GetActiveMessageBoxParentForm()
		{
			return (Instance.activeMessageBoxParentFormStack.Count > 0) ? Instance.activeMessageBoxParentFormStack.Peek() : null;
		}

		readonly Stack<Form> activeMessageBoxParentFormStack;

		protected internal Form ApplicationActiveForm
		{
			get { return applicationActiveFormOverride ?? Form.ActiveForm; }
		}
		Form applicationActiveFormOverride;

		public static void SetApplicationActiveForm(Form form)
		{
			Instance.applicationActiveFormOverride = form;
		}

		public static void RemoveAsApplicationActiveFormIfStillActive(Form form)
		{
			if (Instance.applicationActiveFormOverride == form)
			{
				Instance.applicationActiveFormOverride = null;
			}
		}

		public class ActiveFormOverride : Disposable
		{
			public ActiveFormOverride(Form currentActiveForm)
			{
				Instance.applicationActiveFormOverride = currentActiveForm;
			}

			protected override void Dispose(bool isDisposing)
			{
				Instance.applicationActiveFormOverride = null;
			}
		}

		#endregion

		Form GetActiveMessageBox()
		{
			Form result = null;
			while (result == null && activeMessageBoxStack.Count > 0)
			{
				result = activeMessageBoxStack.Peek();
				if (result != null && result.IsDisposed)
				{
					result = null;
				}
			}
			return result;
		}

		readonly Stack<Form> activeMessageBoxStack;
		readonly List<Form> closingMessageBoxList;

		static internal bool IsWindowEnabled(Form form)
#if !WINZOR
			=> IsWindowEnabled(form.Handle);
#else
			=> form.WindowEnabled && form.Enabled;
#endif

		static internal bool EnableWindow(Form form, bool enable)
#if !WINZOR
			=> EnableWindow(form.Handle, enable);
#else
			=> form.WindowEnabled = enable;
#endif

		#region Native Methods

#if !WINZOR

		[DllImport("user32.dll")]
		static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		static extern bool IsWindowEnabled(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetForegroundWindow(IntPtr hWnd);

#endif

		#endregion
	}
}
