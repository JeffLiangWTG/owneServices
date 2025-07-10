using CargoWise.Types;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IDocumentDeliveryDefaultLanguages
	{
		ZString Fallback { get; set; }
		ZInt Order { get; set; }
	}
}
