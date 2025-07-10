using Enterprise.DocumentScanning.Integration;

namespace Enterprise.DocumentScanning.Business
{
	public interface IDragDropSupport : IDragDropSupportBase
	{
		void Add(SerializableEDocCollection collection);
	}
}
