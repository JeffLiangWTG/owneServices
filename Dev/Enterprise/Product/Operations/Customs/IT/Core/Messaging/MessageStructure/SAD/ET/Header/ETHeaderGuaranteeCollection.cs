using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETHeaderGuaranteeCollection
{
	readonly IEnumerable<IETHeaderGuarantee> iETHeaderGuarantees;

	public ETHeaderGuaranteeCollection(IEnumerable<IETHeaderGuarantee> iETHeaderGuarantees)
	{
		this.iETHeaderGuarantees = Argument.NotNull(iETHeaderGuarantees, "iETHeaderGuarantees");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	public ZInt? NumberOfOccurences => iETHeaderGuarantees.GetCountOrNullIfEmpty();

	[MessageLayout(Order = 1)]
	[MessageFieldExportWithTransitRules("D", "C85")]
	[MessageFieldTransitRules("D", "C85")]
	public IEnumerable<ETHeaderGuarantee> Guarantees
	{
		get
		{
			foreach (var eTHeaderGuarantee in iETHeaderGuarantees)
			{
				yield return new ETHeaderGuarantee(eTHeaderGuarantee);
			}
		}
	}
}
