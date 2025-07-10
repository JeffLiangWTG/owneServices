using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	interface IUNDGSummaryWriterComponent
	{
		ZString Write(UNDGSubstanceWrapper wrapper);
		bool IsDefault { get; }
	}
}
