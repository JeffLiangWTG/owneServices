using System;
using CargoWise.Common;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine
{
	public class PrintEngineJob
	{
		public PrintEngineJob(SerialisablePrintJob job, SerialisablePrintQueue queue, Watermark watermark, decimal lineSpacing = 1m)
		{
			SerialisablePrintJob = Argument.NotNull(job, nameof(job));
			SerialisablePrintQueue = Argument.NotNull(queue, nameof(queue));
			Argument.NotNullOrEmpty(job.BlobType, nameof(job.BlobType));
			Argument.NotNull(job.Contents, nameof(job.Contents));

			Watermark = watermark;
			LineSpacing = lineSpacing;
		}

		#region Properties

		public int Copies => SerialisablePrintJob.Copies;

		public string BlobType => SerialisablePrintJob.BlobType;

		public byte[] Contents => SerialisablePrintJob.Contents;

		public string DocumentName => SerialisablePrintJob.EmailSubjectLine;

		public string PrinterName => SerialisablePrintQueue.Name;

		public int LeftMargin => SerialisablePrintQueue.LeftMargin;

		public int TopMargin => SerialisablePrintQueue.TopMargin;

		public decimal Scale => SerialisablePrintQueue.Scale;

		public decimal VerticalScale => SerialisablePrintQueue.RowScale;

		public decimal HorizontalScale => SerialisablePrintQueue.ColumnScale;

		public byte[] PrinterDriverTemplate => SerialisablePrintQueue.XlsTemplate;

		public bool DeleteCompanyLogo => SerialisablePrintQueue.SuppressLetterhead;

		public byte[] EscapeSequence => SerialisablePrintJob.EscapeSequence;

		public bool IsRollPaper => SerialisablePrintQueue.IsRollPaper;

		public decimal LineSpacing { get; }

		public Guid JobPk => SerialisablePrintJob.JobPk;

		#endregion

		public readonly Watermark Watermark;

		readonly SerialisablePrintJob SerialisablePrintJob;
		readonly SerialisablePrintQueue SerialisablePrintQueue;
	}
}
