using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class KeyWordProvider : IKeyWord
	{
		KeyWordProvider(DispatchInstructionNumber instructionNumber)
		{
			this.instructionNumber = Argument.NotNull(instructionNumber, nameof(instructionNumber));
		}
		readonly DispatchInstructionNumber instructionNumber;

		public static KeyWordProvider New(DispatchInstructionNumber instructionNumber) => instructionNumber == null ? null : new KeyWordProvider(instructionNumber);

		public int Code => 1;

		public string Value => instructionNumber.CE_EntryNum;
	}
}
