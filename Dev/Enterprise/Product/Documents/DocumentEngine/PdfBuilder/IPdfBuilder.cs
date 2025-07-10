using System.IO;

namespace Enterprise.DocumentEngine.PdfBuilder
{
	public interface IPdfBuilder
	{
		public void Build(PdfElement[] elements, Stream output);
	}
}
