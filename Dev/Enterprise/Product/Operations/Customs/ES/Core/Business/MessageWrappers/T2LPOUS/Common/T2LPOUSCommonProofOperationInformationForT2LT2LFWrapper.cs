using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper : IT2LPOUSCommonProofOperationInformationForT2LT2LF
{
	public T2LPOUSCommonProofOperationInformationForT2LT2LFWrapper(CusEntryHeader cusEntryHeader, ZBool isReception)
	{
		entryHeader = Argument.NotNull(cusEntryHeader, nameof(cusEntryHeader));
		declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		entryInstruction = entryHeader.EntryInstruction;
		this.isReception = isReception;
	}
	protected readonly CusEntryHeader entryHeader;
	readonly JobDeclaration declaration;
	readonly CusEntryInstruction entryInstruction;
	readonly ZBool isReception;

	const int RequestMinNumberOfDays = 90;

	public ZString DeclarationType => declaration.ZG_CTStatusID;

	public IT2LPOUSRequestedValidityOfTheProof RequestedValidityOfTheProof => requestedValidityOfTheProof ?? (requestedValidityOfTheProof = entryInstruction.ZG_NumberOfDays <= RequestMinNumberOfDays ? null : new T2LPOUSRequestedValidityOfTheProofWrapper(entryInstruction));
	T2LPOUSRequestedValidityOfTheProofWrapper requestedValidityOfTheProof;

	public ZString RequestType => entryInstruction.ZG_RequestType;

	public ZBool NationalOnlyRequest => isReception ? true : entryInstruction.ZG_National;
}
