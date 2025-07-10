using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class IMPGAApprovalValidation : GAApprovalValidation
	{
		public IMPGAApprovalValidation(GAApproval parent)
			: base(parent)
		{
		}

		new GAApproval Parent => (GAApproval)base.Parent;

		protected override void CheckCSI_DateOfIssue()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);

			ZDateTime declarationDate = Parent.Parent.DeclarationDate.IsEmpty ? ZDateTime.Today : Parent.Parent.DeclarationDate;

			if (Parent.CSI_DateOfIssue > declarationDate)
			{
				Parent.CSI_DateOfIssueInfo.AddMessageError(ApprovalDateMessageErr);
			}
		}

		protected override void CheckCSI_Code()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
		public static ZString ApprovalDateMessageErr => Res.GetString("86DAC026-73EA-413F-8870-19A29132B21E", "Please enter an Approval date earlier than or equal to Declaration Date.");
	}
}
