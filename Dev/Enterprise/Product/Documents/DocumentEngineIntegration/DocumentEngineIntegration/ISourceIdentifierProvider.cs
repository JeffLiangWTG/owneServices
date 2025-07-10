using CargoWise.Types;

namespace Enterprise.DocumentEngineIntegration
{
	public interface ISourceIdentifierProvider
	{
		ZGuid SourceIdentifier { get; }
	}
}
