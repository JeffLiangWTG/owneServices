using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.EMCS;

public class IE815ImportSadContainer
{
	public IE815ImportSadContainer(IImportSadContainer importSadContainer)
	{
		this.importSadContainer = Argument.NotNull(importSadContainer, "importSadContainer");
	}
	readonly IImportSadContainer importSadContainer;

	[MessageLayout(Order = 16)]
	[MessageFieldIntegerRepresentation(3, true)]
	[MessageFieldRules("R", "R053")]
	public ZInt TotalImportSadIterations => ImportSads.Count();

	[MessageLayout(Order = 17)]
	public IEnumerable<IE815ImportSad> ImportSads
	{
		get
		{
			var i = 1;
			foreach (var importSad in importSadContainer.ImportSads)
			{
				yield return new IE815ImportSad(importSad, i++);
			}
		}
	}
}
