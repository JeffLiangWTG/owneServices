using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class AlternativeEvidenceCollection : NonPersistentBusinessObjectCollection<AlternativeEvidence>
	{
		public AlternativeEvidenceCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AlternativeEvidence(Factory);
		}
	}
}
