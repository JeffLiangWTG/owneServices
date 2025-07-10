using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Web.Common
{
	public class WebDbConnectionGuiPlugin : IDbConnectionGuiPlugin
	{
		public IDisposable NewConnectingSplashFormManager() => null;
		public bool GetUserConfirmation(string caption, string yesNoQuestion) => true;
		public void HandleDbConcurrencyException(Exception ex) { }

		public void HandleDatabaseUpgradeException(DatabaseUpgradeException ex)
		{
			TopLevelWebExceptionHandler.HandleUnhandledException(ex);
		}

		public void CheckVersionUpgraded() { }
	}
}
