using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ZFormCreatingStrategy : IDisposable
	{
		internal ZFormCreatingStrategy()
		{
			KForm.FormCreated += NewFormCreated;
		}

		static void NewFormCreated(object sender, EventArgs e)
		{
			var form = sender as KForm;
			if (form != null)
			{
				if (!ZFormStrategy.CanFormBeCreatedDuringDbTransaction(form.GetType()))
				{
					NotifyOpeningFormDuringDbTransactions(form);
				}
			}
		}

		static void NotifyOpeningFormDuringDbTransactions(KForm form)
		{
			if (!Db.IsUpgradeWorkingInProgress)
			{
				try
				{
					var validTransactionCount =
#if DEBUG
					TransactionedTestCase.InTransactionedTestCase ? 1 : // In TransactionedTestCases transaction level is expected to 1, otherwise 0 is the limit.
#endif
					0;

					if (Db.Connection.AppTransactionCount > validTransactionCount)
					{
						if (!hasShownFormInThisTransaction)
						{
							hasShownFormInThisTransaction = true;

							var description = Res.GetString("2a5f8332-f0a3-4a47-b13f-ddab36f3106c", "Opening a form during a transaction [{0}] - Caption: {1} - Form type: {2}", form.Name, form.Text, form.GetType().FullName);

							var messageBox = form as ZMessageBox;
							if (messageBox != null && messageBox.TextBox != null)
							{
								description += " " + Res.GetString("cb843411-df09-4220-9d53-3a15ed65a418", "- Message: {0}", messageBox.TextBox.Text);
							}

							ErrorReporter.ReportOnce(description);
						}
					}
					else
					{
						hasShownFormInThisTransaction = false;
					}
				}
				catch (DatabaseUpgradeInProgressException)
				{
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("ZFormCreatingStrategy.NotifyOpeningFormDuringDbTransactions", ex.Message);
				}
			}
		}

		[ThreadStatic]
		static bool hasShownFormInThisTransaction;

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				KForm.FormCreated -= NewFormCreated;
			}
		}
	}
}
