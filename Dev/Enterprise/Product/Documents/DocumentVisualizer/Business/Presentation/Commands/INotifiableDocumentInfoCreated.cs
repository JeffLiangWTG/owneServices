using Enterprise.DocumentVisualizer.Business;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public interface INotifiableDocumentInfoCreated
	{
		void NotifyDocumentInfoCreated(IDocumentInfo documentInfo);
	}
}
