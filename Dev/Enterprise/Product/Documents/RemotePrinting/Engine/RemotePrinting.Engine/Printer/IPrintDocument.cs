using System;
using System.Drawing.Printing;
using FlexCel.Render;

namespace Enterprise.RemotePrinting.Engine
{
	public interface IPrintDocument : IDisposable
	{
		PageSettings DefaultPageSettings { get; set; }
		PaperSize DefaultPaperSize { get; set; }
		string DocumentName { get; set; }
		PrintController PrintController { get; set; }
		PrinterSettings PrinterSettings { get; set; }

		event PrintPageEventHandler BeforePrintPage;
		event PrintHardMarginsEventHandler GetPrinterHardMargins;
		event PrintPageEventHandler PrintPage;

		void Print();
	}
}
