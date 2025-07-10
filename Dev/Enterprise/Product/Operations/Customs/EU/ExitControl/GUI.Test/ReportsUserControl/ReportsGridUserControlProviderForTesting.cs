using System;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	public class ReportsGridUserControlProviderForTesting : IReportsGridUserControlProvider, IDisposable
	{
		public ReportsGridUserControl UserControl => userControl ?? (userControl = new ReportsGridUserControl());
		ReportsGridUserControl userControl;

		void IDisposable.Dispose()
		{
			userControl?.Dispose();
		}

		IReportsGridUserControl IReportsGridUserControlProvider.UserControl => UserControl;
	}
}
