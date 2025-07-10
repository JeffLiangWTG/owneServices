namespace Enterprise.RemotePrinting.Engine
{
	public interface IPrinterFactory
	{
		BasePrinter GetPrinter(PrintEngineJob job);
	}
}
