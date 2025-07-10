using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusBondDetailValidation : CommonCusBondDetailValidation
	{
		public CusBondDetailValidation(CusBondDetail parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateRemaining();
		}

		public void ValidateRemaining()
		{
			ValidateCalculatedProperty(Parent.RemainingInfo);
		}

		protected void CheckRemaining()
		{
			MandatoryValidation.CheckNotNegative(Parent.RemainingInfo);
		}

		protected override void CheckPW_BondAmount()
		{
			base.CheckPW_BondAmount();
			if (!Parent.PW_BondType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondAmountInfo);
			}
		}

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			if (Parent.PW_BondAmount > 0)
			{
				MandatoryValidation.CheckEntered(Parent.PW_BondTypeInfo);
			}
		}

		protected override void CheckPW_ActivityCode()
		{
			base.CheckPW_ActivityCode();
			CheckInvoiceLineProcedureByActivityCode();
		}

		void CheckInvoiceLineProcedureByActivityCode()
		{
			if (!Parent.PW_BondType.IsEmpty
				&& Parent.IsConsumed
				&& !Parent.HasReleaseGuarantees
				&& Parent.Instruction is CusEntryInstruction instruction
				&& !instruction.AllInvoiceLinesUseConsumeNotReleaseGuaranteeProcedure)
			{
				Parent.PW_ActivityCodeInfo.AddMessageError(Res.GetString("429bee08-e60a-44d2-bbb5-094a5ab16041", "All invoice lines must have a Procedure and all of these Procedures should consume a guarantee and not release a guarantee."));
			}
		}

		protected new CusBondDetail Parent => (CusBondDetail)base.Parent;
	}
}
