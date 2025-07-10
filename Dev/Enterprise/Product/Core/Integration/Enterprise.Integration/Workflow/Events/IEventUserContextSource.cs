using CargoWise.Types;
using Enterprise.DocumentEngineIntegration.DocumentParsing;

namespace Enterprise.Integration
{
	public interface IEventUserContextSource
	{
		[DocumentFieldExcludeFromMap]
		ZString StaffCode { get; }
		ZString UserCode { get; }
	}
}
