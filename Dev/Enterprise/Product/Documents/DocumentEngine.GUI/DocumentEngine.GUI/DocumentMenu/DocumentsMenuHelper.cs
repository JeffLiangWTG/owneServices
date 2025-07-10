using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.GUI
{
	public abstract class DocumentsMenuHelper
	{
		public abstract IDocumentSupportable GetDocumentSupportable();
		public abstract List<DocumentsMenuItem> GetAvailableDocuments();
		public abstract ZGuid PKForBizOCreation { get; }
		public virtual void SetupDocumentPack(DocumentPack pack)
		{
		}
	}
}
