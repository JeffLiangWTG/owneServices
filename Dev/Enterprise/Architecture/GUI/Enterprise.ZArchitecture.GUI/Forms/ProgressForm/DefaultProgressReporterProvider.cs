using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public class DefaultProgressReporterProvider : IProgressReporterProvider
	{
		public DefaultProgressReporterProvider(Form parentForm)
		{
			this.parentForm = parentForm;
		}

		readonly Form parentForm;

		#region IProgressReporterProvider Members

		IProgressReporter IProgressReporterProvider.CreateProgressReporter(string message, int numberOfItemsToProcess, bool allowCancel)
		{
			var reporter = new LinearProgressReporter(numberOfItemsToProcess, itemsBetweenUpdates: 1, allowCancel);
			reporter.ShowForm(parentForm, message);

			return reporter;
		}
		#endregion
	}
}
