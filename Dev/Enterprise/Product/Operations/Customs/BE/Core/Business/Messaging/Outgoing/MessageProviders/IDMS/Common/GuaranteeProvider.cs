using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public class GuaranteeProvider : IGuarantee
{
	readonly CusEntryInstruction entryInstruction;
	public GuaranteeProvider(CusEntryInstruction entryInstruction, int sequenceNumber, string guaranteeType)
	{
		this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
		this.GuaranteeType = Argument.NotNullOrEmpty(guaranteeType, nameof(guaranteeType));
		this.SequenceNumber = sequenceNumber;
	}

	public int SequenceNumber { get; }

	public string GuaranteeType { get; }

	public string OtherGuaranteeReference => null;

	public IReadOnlyCollection<IGuaranteeReference> GuaranteeReferences => guaranteeReferences ??= entryInstruction.Guarantees.Where(g => g.PW_BondType == GuaranteeType).Select((x, i) => new GuaranteeReferenceProvider(x, i + 1)).ToArray<IGuaranteeReference>();
	IReadOnlyCollection<IGuaranteeReference> guaranteeReferences;
}
