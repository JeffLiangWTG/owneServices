using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderSealCollection
{
	readonly IEnumerable<ZString> seals;

	public ETHeaderSealCollection(IEnumerable<ZString> seals)
	{
		this.seals = Argument.NotNull(seals, "seals");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt NumberOfOccurences => seals.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public IEnumerable<ETHeaderSeal> SealsIdentity
	{
		get
		{
			foreach (var seal in seals)
			{
				yield return new ETHeaderSeal(seal);
			}
		}
	}
}
