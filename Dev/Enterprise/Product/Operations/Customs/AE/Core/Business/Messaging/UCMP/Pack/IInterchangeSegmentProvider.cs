using CargoWise.EntityFramework;
using Enterprise.Edifact.Generic.V4;

namespace Enterprise.Customs.AE.Business;

public interface IInterchangeSegmentProvider
{
	bool TryGetHeaderSegment(BusinessObject messageParent, out UNBSegment headerSegment);
}
