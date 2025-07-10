using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class ESGuaranteeValidation : EU.Business.Declaration.GuaranteeForDeclarationValidation
	{
		public ESGuaranteeValidation(ESGuarantee guarantee)
		: base(guarantee)
		{
		}
		protected new ESGuarantee Parent => (ESGuarantee)base.Parent;

		protected override void CheckEntryInstructionID()
		{
			if (Parent.Declaration?.IsImport ?? false)
			{
				base.CheckEntryInstructionID();
				TypeValidation.CheckValidGuid(Parent.EntryInstructionIDInfo);
			}
		}

		protected override void CheckPW_BondFiledPort()
		{
			if (Parent.Declaration?.IsImport ?? false)
			{
				base.CheckPW_BondFiledPort();
				if (!Parent.PW_BondNumber.IsEmpty && Parent.PW_BondFiledPort.IsEmpty && IsEntryInstructionH2)
				{
					Parent.PW_BondFiledPortInfo.AddMessageError(Res.GetString("84DC0C8F-1E2B-4809-82DA-36F2B3A4FB98", "You have not entered a guarantee Office"));
				}
			}
		}

		bool IsEntryInstructionH2 => Parent.EntryInstruction != null && Parent.EntryInstruction.CEI_Style == IMPDeclarationTypeList.Codes.H2;
	}
}
