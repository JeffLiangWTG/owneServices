using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine
{
	public interface IPrintEngineProcessor
	{
		void SetPrinter(SerialisablePrintQueue changedQueue);
		void UpdatePrinters(IEnumerable<string> installedPrinters);
		bool HasQueueChanged(string queueName, Guid queueStateChangedStamp);

		PrintResult Print(SerialisablePrintJob jobToPrint, Watermark watermark);

		Task<PrintResult> PrintAsync(SerialisablePrintJob jobToPrint, Watermark watermark);

		event EventHandler<LogEventArgs> Logged;
		bool EnableVerboseLogging { get; set; }
	}
}
