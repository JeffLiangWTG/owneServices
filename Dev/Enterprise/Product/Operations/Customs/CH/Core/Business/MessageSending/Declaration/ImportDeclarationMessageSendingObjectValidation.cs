using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class ImportDeclarationMessageSendingObjectValidation : DeclarationMessageSendingObjectValidation
{
	public ImportDeclarationMessageSendingObjectValidation(AutoJobDeclarationMessageSendingObject parent) : base(parent)
	{
	}

	public new ImportDeclarationMessageSendingObject Parent => (ImportDeclarationMessageSendingObject)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ?? (plausiValidation = PlausiValidation.New((JobDeclaration)Parent.Header.Declaration));
	PlausiValidation plausiValidation;

	protected override void CheckVOCReason()
	{
		base.CheckVOCReason();

		if (Parent.ShouldSend && !Parent.IsVOCReason_ReadOnly)
		{
			PlausiValidation.CheckR292(Parent.VOCReasonInfo, Parent);
		}
	}
}
