using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationInstructionDocumentProvider : IDeclarationInstructionsDocument
	{
		public DeclarationInstructionDocumentProvider(CusEntryNumber entryNumber)
		{
			this.entryNumber = Argument.NotNull(entryNumber, nameof(entryNumber));
		}

		public static DeclarationInstructionDocumentProvider New(CusEntryNumber entryNumber) => entryNumber == null ? null : new DeclarationInstructionDocumentProvider(entryNumber);

		readonly CusEntryNumber entryNumber;

		public string ReferenceTypeCode => entryNumber.CE_EntryType;
		public string ReferenceNumber => entryNumber.CE_EntryNum;
	}
}

