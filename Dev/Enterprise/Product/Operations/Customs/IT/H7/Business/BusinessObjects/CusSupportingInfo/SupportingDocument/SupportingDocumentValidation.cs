using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.H7.Business;

public class SupportingDocumentValidation : EU.H7.Business.SupportingDocumentValidation
{
	public SupportingDocumentValidation(AutoCusSupportingInfo parent)
		: base(parent)
	{
	}

	protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

	protected override void CheckCSI_Code()
	{
	}
}
