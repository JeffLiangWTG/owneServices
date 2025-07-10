using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public sealed class Notifier
	{
		readonly Configuration configuration;

		public Notifier(Configuration configuration)
		{
			Argument.NotNull(configuration, nameof(configuration));
			this.configuration = configuration;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception notification message")]
		public void ShowError(string text)
		{
			ShowError(text, "Error");
		}

		public void ShowError(string text, string caption)
		{
			ShowError(null, text, caption);
		}

		public void ShowError(IWin32Window owner, string text, string caption)
		{
			if (NoUI)
			{
				configuration.Services.EventLog.WriteEntry(configuration.ApplicationName, text, EventLogEntryType.Error);
			}
			else
			{
				configuration.Services.MessageBox.Show(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		bool NoUI
		{
			get { return (configuration.UILevel == UILevel.AutomatedWithNoUI); }
		}
	}
}
