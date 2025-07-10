using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderTransitCustomsOfficeCollection
{
	readonly IEnumerable<IETHeaderTransitCustomsOffice> iETHeaderTransitCustomsOffices;

	public ETHeaderTransitCustomsOfficeCollection(IEnumerable<IETHeaderTransitCustomsOffice> iETHeaderTransitCustomsOffices)
	{
		this.iETHeaderTransitCustomsOffices = Argument.NotNull(iETHeaderTransitCustomsOffices, "iETHeaderTransitCustomsOffices");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZInt? NumberOfOccurences => iETHeaderTransitCustomsOffices.GetCountOrNullIfEmpty();

	[MessageLayout(Order = 1)]
	[MessageFieldExportWithTransitRules("R", "R906", "R907", "R908", "R910")]
	[MessageFieldTransitRules("R", "R906", "R907", "R908", "R910")]
	public IEnumerable<ETHeaderTransitCustomsOffice> TransitCustomsOffices
	{
		get
		{
			foreach (var eTHeaderTransitCustomsOffice in iETHeaderTransitCustomsOffices)
			{
				yield return new ETHeaderTransitCustomsOffice(eTHeaderTransitCustomsOffice);
			}
		}
	}
}
