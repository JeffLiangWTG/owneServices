using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class CreditControlledDocumentsApprovalDocumentCollection : NonPersistentBusinessObjectCollection<CreditControlledDocumentsApprovalDocument>
	{
		public CreditControlledDocumentsApprovalDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CreditControlledDocumentsApprovalDocument("");
		}
	}
}