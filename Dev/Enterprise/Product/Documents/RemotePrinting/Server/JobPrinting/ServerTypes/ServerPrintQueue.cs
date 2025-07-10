using System;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	public class ServerPrintQueue
	{
		public ServerPrintQueue()
		{
		}

		#region Properties

		public Guid QueuePk { get; set; }

		public string Name { get; set; }

		public string DisplayName { get; set; }

		public int LeftMargin { get; set; }

		public int TopMargin { get; set; }

		public string PrintLanguage { get; set; }

		public decimal Scale { get; set; }

		public decimal RowScale { get; set; }

		public decimal ColumnScale { get; set; }

		public byte[] XlsTemplate { get; set; }

		public Guid StateChangedStamp { get; set; }

		public bool SuppressLetterhead { get; set; }

		public bool IsRollPaper { get; set; }

		#endregion
	}
}
