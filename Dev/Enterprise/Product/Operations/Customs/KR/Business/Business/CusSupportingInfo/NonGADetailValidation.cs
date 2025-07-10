using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class NonGADetailValidation : CusSupportingInfoValidation
	{
		public NonGADetailValidation(NonGADetail parent)
			: base(parent)
		{
		}

		public new NonGADetail Parent => (NonGADetail)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNonGAReasonType();
		}

		protected override void CheckCSI_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_Procedure()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_ProcedureInfo);
		}

		protected override void CheckCSI_Status()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_StatusInfo);
		}

		public void ValidateNonGAReasonType()
		{
			ValidateCalculatedProperty(Parent.NonGAReasonTypeInfo);
		}
		protected void CheckNonGAReasonType()
		{
			if (!Parent.NonGAReasonTypeInfo.ReadOnly)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.NonGAReasonTypeInfo);
			}
		}

		protected override void CheckCSI_Description()
		{
			if (!Parent.ImportNonGAMandatoryDocument.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}
	}
}
