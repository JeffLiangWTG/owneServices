using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSRequestedValidityOfTheProofWrapper : IT2LPOUSRequestedValidityOfTheProof
{
	public T2LPOUSRequestedValidityOfTheProofWrapper(CusEntryInstruction cusEntryInstruction)
	{
		entryInstruction = Argument.NotNull(cusEntryInstruction, nameof(cusEntryInstruction));
	}
	protected readonly CusEntryInstruction entryInstruction;

	public ZInt NumberOfDays => entryInstruction.ZG_NumberOfDays;

	public ZString Justification => entryInstruction.ZG_Justification;
}
