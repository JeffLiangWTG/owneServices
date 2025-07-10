using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ImportEntryInstructionCusFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
	{
		public ImportEntryInstructionCusFiscalReferenceValidation(CusFiscalReference parent)
			: base(parent)
		{
		}

		protected override void CheckCFR_Reference()
		{
			base.CheckCFR_Reference();
			var parent = Parent;
			CheckCFR_Reference_BR8063_NoAmend(parent.CFR_Reference, parent.CFR_ReferenceInfo);
		}

		void CheckCFR_Reference_BR8063_NoAmend(ZString subStyle, ZPropertyInfo info)
		{
			if (Parent is CusFiscalReference fiscalReference && fiscalReference.Instruction.IsAmendmentValidationMode && !fiscalReference.OriginalCFR_Reference.IsEmpty && fiscalReference.OriginalCFR_Reference != subStyle)
			{
				info.AddMessageError(CommonResStrings.ShouldNotAmendThisValue);
			}
		}
	}
}
