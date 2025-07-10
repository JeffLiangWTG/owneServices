using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class DocumentAvailabilityValidation : CusSupportingInfoValidation
{
	public DocumentAvailabilityValidation(DocumentAvailability parent)
		: base(parent)
	{
	}

	public new DocumentAvailability Parent => (DocumentAvailability)base.Parent;
}
