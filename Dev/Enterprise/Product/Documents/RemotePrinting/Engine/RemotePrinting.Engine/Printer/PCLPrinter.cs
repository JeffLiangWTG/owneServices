namespace Enterprise.RemotePrinting.Engine
{
	public class PCLPrinter : BasePrinter
	{
		public PCLPrinter(PrintEngineJob printJob)
			: base(printJob)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override void ProcessAndPrintDocument()
		{
			Log("Sending document content to printer");
			RawPrinterHelper.SendBytesToPrinter(PrinterName, Contents);
		}
	}
}
