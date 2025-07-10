using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class SupportingDocumentValidation : CusSupportingInfoValidation
{
	public SupportingDocumentValidation(SupportingDocument parent)
		: base(parent)
	{
	}

	new SupportingDocument Parent => (SupportingDocument)base.Parent;

	ICusSupportingInfoParent SupportingDocumentParent => Parent.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New(SupportingDocumentParent));
	PlausiValidation plausiValidation;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		if (SupportingDocumentParent?.JobDeclaration?.IsImport ?? false)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		PlausiValidation.CheckR290(Parent.CSI_CodeInfo, Parent);
	}

	protected override void CheckCSI_DateOfIssue()
	{
		base.CheckCSI_DateOfIssue();
		PlausiValidation.CheckR227a(Parent.CSI_DateOfIssueInfo, Parent);
		PlausiValidation.CheckE001_DateOfIssue(Parent.CSI_DateOfIssueInfo, Parent);
	}
}
