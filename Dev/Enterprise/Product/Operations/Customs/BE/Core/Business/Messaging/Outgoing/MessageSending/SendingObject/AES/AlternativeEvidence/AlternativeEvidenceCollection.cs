using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

public class AlternativeEvidenceCollection : NonPersistentBusinessObjectCollection<AlternativeEvidence>
{
	public AlternativeEvidenceCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override BusinessObject CreateNonPersistentBusinessObject() => new AlternativeEvidence(Factory);
}
