using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public interface IAEODocumentMessageBoxProvider
	{
		bool AskIfShouldRemoveAEODocument(ZString documentTypeForMessage);
	}
}
