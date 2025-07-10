using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	class NullPrinter : BasePrinter
	{
		public NullPrinter(PrintEngineJob job)
			: base(job)
		{
			PrintJob = Argument.NotNull(job, nameof(job));
		}

		public PrintEngineJob PrintJob { get; }

		protected override void ProcessAndPrintDocument()
		{
			Log("Processing and printing");
			PrintCount++;
		}

		protected override void PrintTrailingEscapeSequenceCore()
		{
			PrintTrailingEscapeSequenceCount++;
		}

		public int PrintCount { get; private set; }
		public int PrintTrailingEscapeSequenceCount { get; private set; }
	}
}
