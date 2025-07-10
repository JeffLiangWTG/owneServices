using System;

namespace CargoWise.Data
{
	public interface IDbConnectionGuiPlugin
	{
		IDisposable NewConnectingSplashFormManager();
		bool GetUserConfirmation(string caption, string yesNoQuestion);
		void CheckVersionUpgraded();
		void HandleDatabaseUpgradeException(DatabaseUpgradeException ex);
		void HandleDbConcurrencyException(Exception ex);
	}

	class BaseDbConnectionGuiPlugin : IDbConnectionGuiPlugin
	{
		#region IDbConnectionGuiPlugin Members

		public IDisposable NewConnectingSplashFormManager()
		{
			return null;
		}

		public bool GetUserConfirmation(string caption, string yesNoQuestion)
		{
			return true;
		}

		public void CheckVersionUpgraded()
		{
		}

		public void HandleDatabaseUpgradeException(DatabaseUpgradeException ex)
		{
		}

		public void HandleDbConcurrencyException(Exception ex)
		{
		}

		#endregion
	}
}
