using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineContainerCollection
{
	readonly IEnumerable<ZString> containers;

	public ETLineContainerCollection(IEnumerable<ZString> containers)
	{
		this.containers = Argument.NotNull(containers, "containers");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("D", "C55")]
	[MessageFieldExportWithTransitRules("D", "C55")]
	[MessageFieldTransitRules("D", "C55")]
	[MessageFieldInternationalRoadTransportsRules("D", "C55")]
	public ZInt NumberOfOccurences => containers.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldExportRules("D", "C55")]
	[MessageFieldExportWithTransitRules("D", "C55")]
	[MessageFieldTransitRules("D", "C55")]
	[MessageFieldInternationalRoadTransportsRules("D", "C55")]
	public IEnumerable<ETLineContainer> Containers
	{
		get
		{
			foreach (var container in containers)
			{
				yield return new ETLineContainer(container);
			}
		}
	}
}
