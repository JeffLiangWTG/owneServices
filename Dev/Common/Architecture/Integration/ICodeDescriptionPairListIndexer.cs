using System;

namespace CargoWise.Integration
{
	public interface ICodeDescriptionPairListIndexer
	{
		ICodeDescription this[Guid pk] { get; }
	}
}
