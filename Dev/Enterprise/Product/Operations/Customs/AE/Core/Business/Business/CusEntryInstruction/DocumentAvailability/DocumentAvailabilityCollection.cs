using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class DocumentAvailabilityCollection : CusSupportingInfoCollection<DocumentAvailability>
{
	public DocumentAvailabilityCollection(CusEntryInstruction parent) : base(parent, AEConstants.CusSupportingInfoTypes.Codes.DocumentAvailability)
	{
	}
}
