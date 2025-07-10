using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.Customs.CA.Business
{
	public class BulkConsolidateProgressEventArgs : EventArgs
	{
		public BulkConsolidateProgressEventArgs(int current, int total, string message) : this((current * 100) / total, message)
		{
		}

		public BulkConsolidateProgressEventArgs(int percentComplete, string message)
		{
			this.percentComplete = percentComplete;
			this.message = message;
		}

		public int percentComplete;
		public string message;
	}

	[SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public delegate void BulkConsolidateProgressEventHandler(object sender, BulkConsolidateProgressEventArgs e);
}
