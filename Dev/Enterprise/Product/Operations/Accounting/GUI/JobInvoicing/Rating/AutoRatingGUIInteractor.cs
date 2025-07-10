#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public class AutoRatingGUIInteractor : IAutoRatingGUIInteractor
	{
		public AutoRatingGUIInteractor(Control control)
			: this((KForm)control.FindForm())
		{
			jobChargeBoundGrid = (control as JobInvoicingUserControl)?.JobChargeUserControl?.JobChargeBoundGrid;
		}

		public AutoRatingGUIInteractor(KForm hostForm)
		{
			this.hostForm = hostForm;
		}

		readonly KForm hostForm;
		readonly ZGrid jobChargeBoundGrid;
		IDisposable suspendChargeListChanged = DisposableAction.NoAction;

		public void SuspendLayout()
		{
			if (hostForm != null)
			{
				hostForm.SuspendLayout();
			}

			suspendChargeListChanged = (jobChargeBoundGrid?.List as BusinessObjectCollection)?.SuspendListChanged() ?? DisposableAction.NoAction;
		}

		public void ResumeLayout()
		{
			if (hostForm != null)
			{
				hostForm.ResumeLayout(true);
			}

			suspendChargeListChanged.Dispose();
			suspendChargeListChanged = DisposableAction.NoAction;
		}

		public void ShowPossibleMatchesDialog(AutoRater.AdditionalRatesNotifications notifications)
		{
			ZFormModaliser.ShowDialogAndDispose(new PossibleMatchesForm(notifications.PossibleMatchesWrapper));
		}

		public Quote SelectQuote(QuoteCollection possibleMatches)
		{
			var model = new SimpleOneOffQuoteCollectionWrapper(possibleMatches);
			ZFormModaliser.ShowDialogAndDispose(new PossibleOneOffQuoteMatchesForm(model));
			return model.SelectedQuote;
		}

		public ZDialogResult ShowNamedAccountMessageBox(string jobNamedAccount, string ratesNamedAccount)
		{
			var message = Res.GetString("9c140420-48aa-43cd-a3c6-abf01571a58d",
				"During Autorating operation, Named Account '{0}' is found on the rates to be applied, which is different from the Named Account '{1}' input in the job. How would you like to proceed?",
				ratesNamedAccount,
				jobNamedAccount);
			return (ZDialogResult)ZFormModaliser.ShowDialogAndDispose(new Rating.GUI.AutoRating.NamedAccountMessageBox(message));
		}

		public void SetJobInvoicingSecurityOverrideProvider(JobHeader job)
		{
			SecurityOverrideProviderSource.Get(job).Provider = new JobInvoicingSecurityOverrideProvider();
		}

		public void ReportProgress(string currentProcessName, int totalItems, int done, TimeSpan eta, decimal speedPerTick)
		{
			UpdateProgress?.Invoke(currentProcessName, totalItems, done, eta, speedPerTick);
		}

		#region DeferErrorPopup

		public IDisposable DeferErrorPopup()
		{
			return new ErrorPopupDeferrer(this);
		}

		class ErrorPopupDeferrer : Disposable
		{
			public ErrorPopupDeferrer(AutoRatingGUIInteractor parent)
			{
				Parent = parent;
				Parent.ErrorPopupDeferredCount++;
			}

			protected override void Dispose(bool isDisposing)
			{
				if (isDisposing)
				{
					Parent.ErrorPopupDeferredCount--;
				}
			}

			readonly AutoRatingGUIInteractor Parent;
		}

		int ErrorPopupDeferredCount;

		#endregion

		public Action<string, int, int, TimeSpan, decimal> UpdateProgress { get; set; }
		public Action<string> ShowCurrentOperation { get; set; }

		public IDisposable StartRatingSession()
		{
			return new RatingSession(this);
		}

		public bool IsCancelled { get; private set; }

		protected class RatingSession : IDisposable
		{
			public RatingSession(AutoRatingGUIInteractor interactor)
			{
				this.interactor = interactor;

				if (interactor.hostForm != null)
				{
					waitCursorChanger = new ZWaitCursorChanger(interactor.hostForm);
				}
				else
				{
					waitCursorChanger = new ZWaitCursorChanger();
				}
			}

			AutoRatingGUIInteractor interactor;
			readonly ZWaitCursorChanger waitCursorChanger;

			public void Dispose()
			{
				if (waitCursorChanger != null)
				{
					waitCursorChanger.Dispose();
				}

				if (interactor.IsCancelled)
				{
					// do nothing
				}
				else if (interactor.ErrorsEncountered.Any())
				{
					var sb = new ZStringBuilder(interactor.ErrorsEncountered);
					Globals.Message.ShowError(sb.ToStringWithNewLineBetweenAppends(), Res.GetString("6ad271b7-36f0-4e12-8a6c-8ca51434a144", "AutoRating Error"));
				}
				else if (interactor.WarningsEncountered.Any())
				{
					var sb = new ZStringBuilder();
					sb.AppendLine(Res.GetString("77f2038b-ee54-4bee-bc56-9ac5f198a3d3", "AutoRating has been completed.\r\nPlease review the following warnings:"));
					foreach (var message in interactor.WarningsEncountered.Where(x => !string.IsNullOrEmpty(x)))
					{
						sb.AppendLine(message);
					}

					sb.Append(Res.GetString("2df61a54-e08a-45f4-9241-cc9255a594db", "Please refer to Notes->{0} for more information.", PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description));
					Globals.Message.ShowInformation(sb.ToString(), Res.GetString("ff81607c-aa81-4168-9130-58b8194d6a41", "AutoRating Completed"));
					interactor.WarningsEncountered.Clear();
				}

				interactor = null;
			}
		}

		public List<string> WarningsEncountered
		{
			get { return warningsEncountered ?? (warningsEncountered = new List<string>()); }
		}
		List<string> warningsEncountered;

		List<string> ErrorsEncountered
		{
			get { return errorsEncountered ?? (errorsEncountered = new List<string>()); }
		}
		List<string> errorsEncountered;

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule")]
		public bool YesNoWarning(string message)
		{
			var result = Globals.Message.Show(message, Res.GetString("54e424ab-f091-4406-9cc3-c5cbdb8f0a1c", "AutoRating Warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
			var userSelected = result ? Res.GetString("e64d00c9-d14a-4010-90ca-f9f65433a663", "Yes") : Res.GetString("8fd38667-6eca-4933-84fd-628bb32cf0c3", "No");
			var messageToLog = Res.GetString("77ef9b1b-6c03-4546-97dc-adf84e6eb24d", "{0}\r\nUser selected: {1}", message, userSelected);
			Log(LogType.Information, messageToLog);

			return result;
		}

		public void ShowException(Exception ex)
		{
			var message = new StringBuilder();
			message.AppendLine(Res.GetString("6D712DE6-7923-4D75-85D8-1E0BB9A1F8D7", "Autorating has encountered an error:"));
			message.Append(ex.Message);
			Globals.Message.ShowError(message.ToString());
		}

		public void Log(LogType type, string message) => Log(type, message, null);

		public void Log(LogType type, string message, Exception ex)
		{
			ShowCurrentOperation?.Invoke(message);

			switch (type)
			{
				case LogType.Error:
					if (ex != null)
					{
						ShowException(ex);
					}
					else if (ErrorPopupDeferredCount > 0)
					{
						if (!ErrorsEncountered.Contains(message))
						{
							ErrorsEncountered.Add(message);
						}
					}
					else
					{
						Globals.Message.ShowError(message, Res.GetString("6ad271b7-36f0-4e12-8a6c-8ca51434a144", "AutoRating Error"));
					}

					break;

				case LogType.Warning:
					WarningsEncountered.Add(message);
					break;
			}
		}

		public void ShowRatesAdditionResult(AutoRatesAdditionResult additionResult, string entityName, CostSell costOrSell)
		{
		}

		public IEnumerable<AutoRateInfo> SelectRate(IRatingContext ratingContext, RatingCriteria criteria)
		{
			var initialCursor = Cursor.Current;
			try
			{
				return new RateSelectorCommand().SelectRate(ratingContext, criteria, hostForm);
			}
			catch (AutoRater.RatingCancelledException)
			{
				IsCancelled = true;
				throw;
			}
			finally
			{
				Cursor.Current = initialCursor;
			}
		}
	}
}
