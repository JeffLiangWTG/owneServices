using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentLineCollection : BusinessObjectCollection<AccComplianceDocumentLine>
	{
		public AccComplianceDocumentLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override bool AllowNewCore => false;
	}
}
