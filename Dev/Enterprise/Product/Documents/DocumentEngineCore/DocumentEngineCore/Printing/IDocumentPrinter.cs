using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.Integration
{
	public interface IDocumentPrinter
	{
		void Print(ZGuid menuPK, ZGuid printer, IDocumentSupportable parent);
		void Print(ZGuid menuPK, ZGuid printer, IDocumentSupportable parent, int numberOfCopies);
	}
}
