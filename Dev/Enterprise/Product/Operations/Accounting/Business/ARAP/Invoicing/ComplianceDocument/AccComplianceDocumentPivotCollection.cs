using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentPivotCollection : BusinessObjectCollection<AccComplianceDocumentPivot>
	{
		public AccComplianceDocumentPivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public AccComplianceDocumentPivotCollection(BusinessObjectFactory factory, ZQuery query) : base(factory, query)
		{ }
	}
}
