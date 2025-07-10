using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineContainerCollection
{
	readonly IEnumerable<ZString> containers;

	public IMLineContainerCollection(IEnumerable<ZString> containers)
	{
		this.containers = Argument.NotNull(containers, nameof(containers));
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldImportRules("D", "C55")]
	[MessageFieldDepositoRules("D", "C55")]
	public ZInt NumberOfOccurences => containers.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldImportRules("D", "C55")]
	[MessageFieldDepositoRules("D", "C55")]
	public IEnumerable<IMLineContainer> Containers
	{
		get
		{
			foreach (var container in containers)
			{
				yield return new IMLineContainer(container);
			}
		}
	}
}
