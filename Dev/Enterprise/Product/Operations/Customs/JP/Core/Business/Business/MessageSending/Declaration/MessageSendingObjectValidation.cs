using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class MessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public MessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
		{
		}

		public new MessageSendingObject Parent => base.Parent as MessageSendingObject;

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateAction();
			ValidateDeclarationCorrectionCopyRequest();
		}

		public void ValidateDeclarationCorrectionCopyRequest()
		{
			ValidateCalculatedProperty(Parent.DeclarationCorrectionCopyRequestInfo);
		}

		protected void CheckDeclarationCorrectionCopyRequest()
		{
			var procedureCode = Parent.ProcedureCode;
			if (Parent.DeclarationCorrectionCopyRequest && procedureCode != JPProcedureCodeList.Codes.IDC && procedureCode != JPProcedureCodeList.Codes.EDC && procedureCode != JPProcedureCodeList.Codes.EAC)
			{
				Parent.DeclarationCorrectionCopyRequestInfo.AddMessageError(Res.GetString("56BE3F6D-93A6-4896-BC26-1C9C354C9DEB", "Declaration Correction Copy Request can only be true when Procedure Code is IDC or EDC or EAC."));
			}
		}

		public void ValidateAction()
		{
			ValidateCalculatedProperty(Parent.ActionInfo);
		}

		protected void CheckAction()
		{
			if (Parent.ShouldSend && Parent.ProcedureCode == JPProcedureCodeList.Codes.ECR)
			{
				ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.ActionInfo);
			}
		}
	}
}
