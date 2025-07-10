using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	public interface IUNDGSubstanceCollectionSummaryWriter
	{
		ZString GetSummary(IReadOnlyCollection<UNDGSubstanceWrapper> collection);
	}
}
