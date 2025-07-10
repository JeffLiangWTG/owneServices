using System.Globalization;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	public class PrinterFactory : IPrinterFactory
	{
		public BasePrinter GetPrinter(PrintEngineJob job)
		{
			Argument.NotNull(job, nameof(job));

			var blobType = job.BlobType?.ToUpper(CultureInfo.InvariantCulture) ?? string.Empty;

			switch (blobType)
			{
				case Constants.PDF:
					return new PdfPrinter(job);
				case Constants.TIF:
					return new GraphicPrinter(job);
				case Constants.PCL:
				case Constants.ZPL:
					return new PCLPrinter(job);
				case Constants.XLS:
				case Constants.XLSX:
					return new FlexCelPrinter(job);
				default: return null;
			}
		}
	}
}
