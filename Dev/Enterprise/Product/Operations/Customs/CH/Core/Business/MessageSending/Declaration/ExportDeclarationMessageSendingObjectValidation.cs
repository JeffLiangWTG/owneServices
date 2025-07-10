using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class ExportDeclarationMessageSendingObjectValidation : DeclarationMessageSendingObjectValidation
{
	public ExportDeclarationMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	protected new ExportDeclarationMessageSendingObject Parent => (ExportDeclarationMessageSendingObject)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateNextProcedure();
	}

	protected override void CheckReasonText()
	{
		if (Parent.ShouldSend && !Parent.IsVOCReason_ReadOnly && Parent.VOCReason == UniversalReferenceConstants.PassarReasonCodes.Others && Parent.ReasonText.IsEmpty)
		{
			MandatoryValidation.CheckEntered(Parent.ReasonTextInfo);
		}
	}

	public void ValidateNextProcedure()
	{
		ValidateCalculatedProperty(Parent.NextProcedureInfo);
	}

	protected void CheckNextProcedure()
	{
		if (Parent.Header.IsExport && Parent.IsNC123)
		{
			ListValidation.ErrorIfInvalidCode(Parent.NextProcedureInfo);
		}
	}
}
