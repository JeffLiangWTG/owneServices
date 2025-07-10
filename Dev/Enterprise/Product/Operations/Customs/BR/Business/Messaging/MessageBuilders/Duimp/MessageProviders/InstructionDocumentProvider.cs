using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class InstructionDocumentProvider : IInstructionDocument
	{
		InstructionDocumentProvider(DispatchInstructionNumber instructionNumber)
		{
			this.instructionNumber = Argument.NotNull(instructionNumber, nameof(instructionNumber));
		}
		readonly DispatchInstructionNumber instructionNumber;

		public static InstructionDocumentProvider New(DispatchInstructionNumber instructionNumber) => instructionNumber == null ? null : new InstructionDocumentProvider(instructionNumber);

		public string Type => DispatchInstructionDocumentTypes.MapToCustomsCode(instructionNumber.CE_EntryType);

		public IEnumerable<IKeyWord> KeyWords => fKeyWords ??= new [] { KeyWordProvider.New(instructionNumber) };
		IKeyWord[] fKeyWords;
	}
}
