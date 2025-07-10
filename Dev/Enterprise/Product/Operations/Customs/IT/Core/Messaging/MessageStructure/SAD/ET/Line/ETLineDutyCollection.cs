using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.MessageFieldAttributes;

namespace Enterprise.Customs.IT.Messaging.SAD;

public class ETLineDutyCollection
{
	readonly IEnumerable<IDutyTaxFee> iDutyTaxFees;

	public ETLineDutyCollection(IEnumerable<IDutyTaxFee> iDutyTaxFees)
	{
		this.iDutyTaxFees = Argument.NotNull(iDutyTaxFees, "iDutyTaxFees");
	}

	[MessageLayout(Order = 0)]
	[MessageFieldIntegerRepresentation(2, false)]
	[MessageFieldExportRules("R")]
	[MessageFieldExportWithTransitRules("R")]
	[MessageFieldTransitRules("R")]
	[MessageFieldInternationalRoadTransportsRules("R")]
	public ZInt NumberOfOccurences => iDutyTaxFees.Count();

	[MessageLayout(Order = 1)]
	public IEnumerable<ETLineDuty> Duties
	{
		get
		{
			foreach (var duty in iDutyTaxFees)
			{
				yield return new ETLineDuty(duty);
			}
		}
	}
}
