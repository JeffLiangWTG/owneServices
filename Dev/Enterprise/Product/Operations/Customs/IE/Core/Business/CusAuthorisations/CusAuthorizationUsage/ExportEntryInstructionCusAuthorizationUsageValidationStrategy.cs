using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using static Enterprise.Customs.IE.Business.ExportCusAuthorizationUsageValidation;

namespace Enterprise.Customs.IE.Business
{
	public sealed class ExportEntryInstructionCusAuthorizationUsageValidationStrategy : IExportCusAuthorizationUsageValidationStrategy
	{
		public ExportEntryInstructionCusAuthorizationUsageValidationStrategy(CusEntryInstruction instruction)
		{
			this.instruction = Argument.NotNull(instruction, nameof(instruction));
		}
		readonly CusEntryInstruction instruction;

		public string[] GetRulesApplicableToCode(ZString agc_Code)
		{
			if (agc_Code.EqualsIgnoringCase(Customs.Business.CusAuthorizationHeaderTypeList.Codes.CentralizedClearance))
			{
				switch (instruction.CEI_SubStyle.ToUpperInvariant())
				{
					case EU.Business.EntrySubStyleList.Codes.IncompleteDeclaration:
					case EU.Business.EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB:
						return new string[] { MessageError.CodeCclNotAllowedForSubtypeBOrE };
				}
			}
			return Array.Empty<string>();
		}
	}
}
