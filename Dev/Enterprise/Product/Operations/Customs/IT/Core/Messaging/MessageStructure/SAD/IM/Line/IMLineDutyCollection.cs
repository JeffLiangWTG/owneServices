using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class IMLineDutyCollection
{
	public IMLineDutyCollection(IEnumerable<IDutyTaxFee> duties)
	{
		this.duties = Argument.NotNull(duties, nameof(duties));
	}

	readonly IEnumerable<IDutyTaxFee> duties;

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldImportRules("R")]
	[MessageFieldDepositoRules("R")]
	public ZInt NumberOfOccurences => duties.Count();

	[MessageLayout(Order = 1)]
	[MessageFieldImportRules("O")]
	[MessageFieldDepositoRules("O")]
	public IEnumerable<IMLineDuty> Duties => duties.Select(duty => new IMLineDuty(duty));
}
