using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	internal class ZFormCaptionStrategy : IDisposable
	{
		internal ZFormCaptionStrategy()
		{
			KForm.QueryFormCaptionSuffix += QueryFormCaptionSuffix;
		}

		static void QueryFormCaptionSuffix(object sender, QueryFormCaptionEventArgs e)
		{
			e.GlobalFormTopCaption = GetGlobalFormTopCaption((Form)sender);
		}

		static string GetGlobalFormTopCaption(Form form)
		{
			try
			{
				IEnvironment env;
				return (form.IsDesignMode() || Db.IsUpgradeWorkingInProgress || (env = EnvProxy.Instance).CurrentUser == null) ? string.Empty : env.GlobalFormTopCaption;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				if (ex is DatabaseUpgradeInProgressException || ex is System.Data.Common.DbException)
				{
					// do nothing
				}
				else
				{
					ErrorReporter.ReportOnce("ZFormCaptionStrategyGetGlobalFormTopCaption", ex.Message);
				}
				return string.Empty;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				KForm.QueryFormCaptionSuffix -= QueryFormCaptionSuffix;
			}
		}

#if DEBUG

		internal static string GetGlobalFormTopCaption_ForTest(Form form) => GetGlobalFormTopCaption(form);

#endif
	}
}
