using System.Collections.Generic;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveableBusinessObjectProviderBase
	{
		IEnumerable<string> TableNamesSupported { get; }
		IEnumerable<ReferenceKeyType> ReferenceKeyTypesSupported { get; }
	}
}
