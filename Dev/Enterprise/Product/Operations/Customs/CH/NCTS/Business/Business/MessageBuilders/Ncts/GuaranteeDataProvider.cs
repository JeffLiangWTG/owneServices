using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class GuaranteeDataProvider : IGuarantee
{
	public static IEnumerable<GuaranteeDataProvider> NewCollection(INctsGuaranteeCollection<NctsGuarantee> guarantees) => guarantees != null ? guarantees.Where(x => x.IsGuaranteeTypeWithAmountNR0067).Select((x, i) =>  new GuaranteeDataProvider(x, i + 1)) : Enumerable.Empty<GuaranteeDataProvider>();

	GuaranteeDataProvider(NctsGuarantee guarantee, int sequenceNumber)
	{
		this.guarantee = guarantee;
		SequenceNumber = sequenceNumber;
	}
	readonly NctsGuarantee guarantee;

	public int SequenceNumber { get; }

	public string GuaranteeType => guarantee.PW_BondType;

	public string OtherGuaranteeReference => guarantee.PW_BondNumber2;

	public IReadOnlyCollection<IGuaranteeReference> GuaranteeReferences => guaranteeReferences ?? (guaranteeReferences = new[] { GuaranteeReferenceDataProvider.New(guarantee, 1) });
	IReadOnlyCollection<IGuaranteeReference> guaranteeReferences;
}
