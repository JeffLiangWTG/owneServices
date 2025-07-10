using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Client.UPE.Business
{
	public interface IUPEDocumentSupportable : IBusiness, IDocumentSupportable
	{
		void OnPrintBatchItemQueued(PrintBatchItemQueuedEventArgs e);
	}
}
