using System;

namespace Enterprise.Client.UPE.Business
{
	public delegate void PrintBatchItemQueuedEventHandler(object sender, PrintBatchItemQueuedEventArgs e);
	public class PrintBatchItemQueuedEventArgs : EventArgs
	{
		public PrintBatchItemQueuedEventArgs(UPEPrintBatchItem printItem, bool notifyUser)
		{
			this.PrintItem = printItem;
			this.NotifyUser = notifyUser;
		}

		public readonly UPEPrintBatchItem PrintItem;
		public readonly bool NotifyUser;
	}
}
