using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Services.OperationalActions.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Services.OperationalActions.GUI
{
	public sealed partial class OperationalActionRunnerForm : ZChildForm
	{
		public OperationalActionRunnerForm(OperationalActionRunner runner)
			: base(runner)
		{
			InitializeComponent();

			Runner.RunOnAllMatchingRecords = Runner.SelectedGridRowCount == 0;
			Runner.RunOnSelectedRecords = Runner.SelectedGridRowCount > 0;

			Runner.HasChanges = false;

			if (runner.IsFilterRecordsSelection)
			{
				RunOnAllMatchingRecordsRadioButton.Click += RunOnAllMatchingRecordsRadioButton_Click;
				RunOnSelectedRecordsRadioButton.Click += RunOnSelectedRecordsRadioButton_Click;
			}
			else
			{
				RunOnAllMatchingRecordsRadioButton.Visible = false;
				RunOnSelectedRecordsRadioButton.Visible = false;
			}

			bool supportsBulkUpdates = Runner.Action.Context.Supporter.SupportsBulkUpdates && Runner.Fields.Count > 0;
			bool supportsDocuments = Runner.Action.Context.SupportsDocuments && Runner.Action.DocumentPivotsView.Count > 0;

			progressControl.ShowMainProgressBar = Runner.MethodApplicators.Count > 1 || Runner.RunOnAllMatchingRecords;
			progressControl.ShowSectionProgressBar = !Runner.RunOnAllMatchingRecords;
			fieldsTabPage.TabVisible = supportsBulkUpdates;
			documentsTabPage.TabVisible = supportsDocuments;

			int insertIndex = (supportsBulkUpdates ? 1 : 0) + (supportsDocuments ? 1 : 0);
			TabPage firstMethodTabPage = null;
			int minMethodWidth = 0;
			int minMethodHeight = 0;

			foreach (var applicator in Runner.MethodApplicators)
			{
				if (applicator is IOverrideSelectionCount overrideSelection)
				{
					Runner.OverrideSelectionCount = overrideSelection;
					Runner.RunOnSelectedRecords = !overrideSelection.SupportRunningOnAllMatchedRecords;
					Runner.RunOnAllMatchingRecords = overrideSelection.SupportRunningOnAllMatchedRecords;
					RunOnAllMatchingRecordsRadioButton.Visible = false;
					RunOnSelectedRecordsRadioButton.Visible = false;
				}
			}

			foreach (OperationalActionMethod method in Runner.MethodApplicators.GetMethods())
			{
				if (method.HasControl)
				{
					ActionMethodTabPage page = new ActionMethodTabPage(method);
					runnerTabControl.TabPages.Insert(page, insertIndex);
					insertIndex++;

					Size methodSize = page.MinimumContentSize;

					minMethodHeight = Math.Max(minMethodHeight, methodSize.Height);
					minMethodWidth = Math.Max(minMethodWidth, methodSize.Width);

					if (firstMethodTabPage == null)
					{
						firstMethodTabPage = page;
					}
				}
				this.IsRunAgainDisabled |= method.IsRunAgainDisabled;
				this.IsResultLoggingDisabled |= method.RunWithoutResultLogging;
			}

			Size padding = Size - progressControl.Size - progressControl.Padding.Size;
			Size absoluteMinSize = MinimumSize;
			Size minFormSize = ControlDpiScalingHelper.NewScaledSize(
				Math.Min(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(Math.Max(absoluteMinSize.Width, padding.Width + minMethodWidth)), 900),
				Math.Min(ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Math.Max(absoluteMinSize.Height, padding.Height + minMethodHeight)), 680)
			);

			MinimumSize = minFormSize;
		}

		void RunOnAllMatchingRecordsRadioButton_Click(object sender, EventArgs e)
		{
			ToggleShowProgressBars(showBar: true);
		}

		void RunOnSelectedRecordsRadioButton_Click(object sender, EventArgs e)
		{
			ToggleShowProgressBars(showBar: false);
		}

		void ToggleShowProgressBars(bool showBar)
		{
			RefreshCaption();
			progressControl.ShowMainProgressBar = showBar;
			progressControl.ShowSectionProgressBar = !showBar;
		}

		#region Show

		public static void Show(OperationalActionRunner runner)
		{
			Argument.NotNull(runner, "runner");

			using (OperationalActionRunnerForm form = new OperationalActionRunnerForm(runner))
			{
				if (CheckRequiredLicenceCheckpoints(runner, form))
				{
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
		}

		static bool CheckRequiredLicenceCheckpoints(OperationalActionRunner runner, ILicensedComponent component)
		{
			var deniedLicences = new List<LicenceCheckpoint>();
			var requiredLicences = runner.ExtractUniqueLicenceCheckpoints();

			foreach (LicenceCheckpoint checkpoint in requiredLicences)
			{
				if (checkpoint.Login(component) == LicenceLoginResponse.Denied)
				{
					deniedLicences.Add(checkpoint);
				}
			}

			if (deniedLicences.Count == 0)
			{
				return true;
			}
			else
			{
				StringBuilder builder = new StringBuilder();
				foreach (var checkpoint in deniedLicences)
				{
					builder.Append(deniedLicences[0].LastReasonForNotAllowing);
				}

				string caption = Res.GetString("OperationalActionsRunnerForm|Licence|Caption", "License Check Failed");
				Globals.Message.ShowError(builder.ToString(), caption);

				return false;
			}
		}

		#endregion

		#region Properties

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool EnableDebugLogging
		{
			get { return progressControl.EnableDebugLogging; }
			set { progressControl.EnableDebugLogging = value; }
		}

		readonly bool IsRunAgainDisabled;
		readonly bool IsResultLoggingDisabled;

		#endregion

		#region Overrides

		public override string FormHeading
		{
			get
			{
				if (Runner.OverrideSelectionCount != null)
				{
					return Res.GetString("OperationalActionRunnerForm|CaptionWithoutCount", "Run '{0}'", Runner.HumanReadableName);
				}
				else
				{
					return Res.GetString("OperationalActionRunnerForm|Caption", "Run '{0}' on {1} {2}",
					Runner.HumanReadableName,
					Runner.RunOnSelectedRecords ? (int)Runner.SelectedGridRowCount : Runner.FilterRowCount,
					Runner.RunOnSelectedRecords ? Runner.SelectedGridRowNoun : Runner.FilterRowNoun);
				}
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(
				businessEntityForValidation,
				Res.GetString("OperationalActionRunnerForm|EntityName", "action"),
				Res.GetString("OperationalActionRunnerForm|PresentVerb", "run"),
				Res.GetString("OperationalActionRunnerForm|PastVerb", "run"),
				includeIgnoreOption);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (cancelButton.Enabled)
			{
				base.OnClosing(e);
			}
			else
			{
				e.Cancel = true;
			}
		}

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new DevTools.ToggleDebugPanelTool());
		}

		#endregion

		#region PerformClick

		public void PerformClickOk()
		{
			this.okButton.PerformClick();
		}

		public void PerformClickCancel()
		{
			this.cancelButton.PerformClick();
		}

		#endregion

		#region Implementation

		bool ValidateAndRunAction()
		{
			if (this.IsDisposed || Runner == null)
			{
				return false;
			}

			Runner.RunPreSaveValidation();
			TabPageNotificationsExposer.ExposeTabPageNotifications(this, BusinessEntity);

			if (Runner.HasErrors)
			{
				ShowErrorsDialog();
				return false;
			}
			else if (Runner.RunOnSelectedRecords && Runner.SelectedGridRowCount == 0)
			{
				Globals.Message.Show(Res.GetString("e8c5467a-6e75-4c1a-b203-41c5e0ec6d50", "Please select at least one record in the module screen, or select 'Run on all matching records'."), Runner.HumanReadableName, MessageBoxButtons.OK, DialogResult.OK);
				return false;
			}
			else if (!QueryUserIfShouldContinue())
			{
				return false;
			}
			else
			{
				try
				{
					okButton.Enabled = cancelButton.Enabled = false;
					Runner.SetReadOnlyIncludingChildren(true);
					runnerTabControl.SelectTab(progressTabPage);
					return RunAction(progressControl);
				}
				finally
				{
					Runner.SetReadOnlyIncludingChildren(false);
					okButton.Text = Res.GetString("OperationalActionRunnerForm|OkPostRun", "Run Again");
					okButton.Enabled = okButton.Visible = !IsRunAgainDisabled;
					cancelButton.Text = Res.GetString("OperationalActionRunnerForm|CancelPostRun", "Close");
					cancelButton.Enabled = true;
				}
			}
		}

		bool QueryUserIfShouldContinue()
		{
			string message = Res.GetString(
				"OperationalActionRunnerForm|Confirm",
				"Are you sure you want to run this action on {0} {1}?",
				Runner.RunOnSelectedRecords ? (int)Runner.SelectedGridRowCount : Runner.FilterRowCount,
				Runner.RunOnSelectedRecords ? Runner.SelectedGridRowNoun : Runner.FilterRowNoun);

			return DialogResult.OK == Globals.Message.Show(message, Runner.HumanReadableName, MessageBoxButtons.OKCancel, DialogResult.OK);
		}

		bool RunAction(IOperationalActionLog log)
		{
			bool shouldClose = false;

			if (Runner.RunOnSelectedRecords)
			{
				var factoryForChanges = new BusinessObjectFactory { NameForDebugging = "OperationalActions factory for changes" };
				using (factoryForChanges.AddDisposableService())
				{
					Runner.Run(log, factoryForChanges, OperationalActionMethodUIMode.UIOnly);
					SaveAndLog(log, factoryForChanges);
				}
			}
			else if (Runner.RunOnAllMatchingRecords)
			{
				Action<IOperationalActionLog, BusinessObjectFactory> saveAndLogAction = SaveAndLog;
				Runner.BatchRun(log, saveAndLogAction, OperationalActionMethodUIMode.UIOnly);
			}

			if (log.HighestErrorLevelEncountered < OperationalActionLogErrorLevel.Warning)
			{
				shouldClose = Runner.CloseOnCompletion;
			}

			return shouldClose;
		}

		void LogResult(IOperationalActionLog log, string message, OperationalActionLogErrorLevel logErrorLevel = OperationalActionLogErrorLevel.Informational)
		{
			if (!IsResultLoggingDisabled)
			{
				log.Notify(logErrorLevel, message);
			}
		}

		void SaveAndLog(IOperationalActionLog log, BusinessObjectFactory factoryForChanges)
		{
			if (log.HighestErrorLevelEncountered < OperationalActionLogErrorLevel.Error)
			{
				if (((IBusinessObjectFactoryInternals)factoryForChanges).AllBusinessObjects.Any(x => x.HasChanges))
				{
					LogResult(log, Res.GetString("OperationalActionRunnerForm|Log|Saving", "Saving ..."));
					if (SaveWithBestGuessErrorHandling(factoryForChanges))
					{
						LogResult(log, Res.GetString("OperationalActionRunnerForm|Log|Saved", "Saved."));
						Runner.SummaryLog(log);
						LogResult(log, Res.GetString("OperationalActionRunnerForm|Log|Done", "Done."));
					}
					else
					{
						LogResult(log, Res.GetString("OperationalActionRunnerForm|Log|Failed", "Failed."), OperationalActionLogErrorLevel.Error);
					}
				}
				else
				{
					Runner.SummaryLog(log);
					LogResult(log, Res.GetString("OperationalActionRunnerForm|Log|Done", "Done."));
				}
			}
			else
			{
				LogResult(log, Res.GetString("OperationalActionRunnerForm|Log|Aborted", "Aborted."), OperationalActionLogErrorLevel.Error);
			}
		}

		bool SaveWithBestGuessErrorHandling(BusinessObjectFactory factoryForChanges)
		{
			try
			{
				factoryForChanges.Save();
				return true;
			}
			catch (ZSaveException ex)
			{
				// Normally ZSaveException's are handled by ZExceptionReporting.HandleSaveException
				// but in this case there is nothing it can do to auto-resolve/recover/resume so we
				// need to give some other (generic) user-friendly message.

				if (ex.ShouldBeReportedToEDI)
				{
					throw new RunActionException(Runner.GetDebugInfo(), ex);
				}

				string message = null;

				if (ex is ZSaveConcurrencyException)
				{
					message = Res.GetString(
						"OperationalActionRunnerForm|SaveConflict",
						"There was a conflict with another users changes and the changes could not be saved. You may need to run the action again.");
				}
				else
				{
					message = Res.GetString(
						"OperationalActionRunnerForm|SaveException",
						"An exception occurred while saving: {0}",
						string.IsNullOrEmpty(ex.FriendlyMessage) ? ex.GetInnermostException().Message : ex.FriendlyMessage);
				}

				Globals.Message.ShowError(message);
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw new RunActionException(Runner.GetDebugInfo(), ex);
				}

				try
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				catch (Exception)
				{
					// was not able to handle the exception so rethrow with extra debugging information.
					// Note: we rethrow the original execption because the second exception does not add
					// anything useful.
					throw new RunActionException(Runner.GetDebugInfo(), ex);
				}
			}

			return false;
		}

		OperationalActionRunner Runner
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (OperationalActionRunner)DataSource; }
		}

		#endregion

		#region Events

		void okButton_Click(object sender, EventArgs e)
		{
			if (this.ValidateAndRunAction())
			{
				Close();
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
