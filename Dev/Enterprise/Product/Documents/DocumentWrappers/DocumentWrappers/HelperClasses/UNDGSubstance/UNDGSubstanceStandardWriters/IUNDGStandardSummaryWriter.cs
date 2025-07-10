using System.Collections.Generic;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	interface IUNDGStandardSummaryWriter
	{
		bool StandardConditionApplies(UNDGSubstanceWrapper wrapper);
		IReadOnlyCollection<IUNDGSummaryWriterComponent> Components { get; }
	}
}
