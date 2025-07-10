using CargoWise.Types;
using Enterprise.Customs.Common.IE;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class EntryInstructionValidationModesCalculator : EU.Business.Declaration.EntryInstructionValidationModesCalculator
	{
		public EntryInstructionValidationModesCalculator(CusEntryInstruction instruction)
			: base(instruction)
		{ }

		protected override EU.Business.Declaration.ValidationModes RecalculateValidationModesCore()
		{
			var result = EU.Business.Declaration.ValidationModes.None;

			switch (supporter.JobDeclaration?.JE_MessageType.ToUpperInvariant() ?? ZString.Empty)
			{
				case IEJobMessageTypeList.Codes.Export:
				case IEJobMessageTypeList.Codes.Import:
					if (supporter.FirstActiveEntry is CusEntryHeader entry && !entry.MovementReferenceNumber.IsEmpty)
					{
						result |= EU.Business.Declaration.ValidationModes.Amendment;
					}
					break;
			}

			return result;
		}

		protected new CusEntryInstruction supporter => (CusEntryInstruction)base.supporter;
	}
}
