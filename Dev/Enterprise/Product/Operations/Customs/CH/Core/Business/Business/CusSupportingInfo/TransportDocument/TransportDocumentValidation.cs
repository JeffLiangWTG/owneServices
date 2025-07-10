using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class TransportDocumentValidation : CusSupportingInfoValidation
{
	public TransportDocumentValidation(TransportDocument parent)
		: base(parent)
	{
	}

	new TransportDocument Parent => (TransportDocument)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ??= (plausiValidation = PlausiValidation.New(Parent.JobDeclaration));
	PlausiValidation plausiValidation;

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		PlausiValidation.CheckNS30065(Parent.CSI_ReferenceNumberInfo, Parent);
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
	}
}
