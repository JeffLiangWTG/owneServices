using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMUnitOfMeasureCollection
{
	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	public ZInt? NumberOfOccurences => null;

	[MessageLayout(Order = 1)]
	public IEnumerable<IMUnitOfMeasure> UnitOfMeasures => System.Array.Empty<IMUnitOfMeasure>();
}
