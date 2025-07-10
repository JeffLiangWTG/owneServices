using System.Threading;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.Environment
{
	public class DbUpgradingWarningManager : IDbUpgradingWarningManager
	{
		public void ShowWarningAndWait()
		{
			ApplicationDispatcher.Current.Send(_ => ShowForm(), null);
			formShowingSignal.WaitOne();
		}

		protected void ShowForm()
		{
			if (!formShowingSignal.WaitOne(NoWaitTimeOut))
			{
				return;
			}

			formShowingSignal.Reset();

			try
			{
				ShowFormCore();
			}
			finally
			{
				formShowingSignal.Set();
			}

			void ShowFormCore()
			{
				using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
				using (var upgradeInProgressForm = new UpgradeInProgressForm())
				{
					ShowDialog(upgradeInProgressForm);
				}
			}
		}

		protected virtual void ShowDialog(UpgradeInProgressForm upgradeInProgressForm)
		{
			upgradeInProgressForm.ShowDialog();
		}

		readonly ManualResetEvent formShowingSignal = new ManualResetEvent(true);
		const int NoWaitTimeOut = 0;
	}
}
