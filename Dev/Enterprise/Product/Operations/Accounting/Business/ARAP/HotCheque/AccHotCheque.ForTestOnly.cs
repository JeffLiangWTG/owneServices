#if DEBUG

using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Accounting.Business.ARAP.HotCheque
{
	public partial class AccHotCheque
	{
		public void DocumentEventSource_DocumentPrinted_ForTestOnly(object sender, DocumentPrintedEventArgs e)
		{
			DocumentEventSource_DocumentPrinted(sender, e);
		}
	}
}

#endif
