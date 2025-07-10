using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentHeaderCollection : BusinessObjectCollection<AccComplianceDocumentHeader>
	{
		public AccComplianceDocumentHeaderCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		public AccComplianceDocumentHeaderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(AccComplianceDocumentHeaderSchema.ADH_GC_Company, GlbCompany.CurrentCompany.PK);
		}

		protected override bool AllowNewCore => false;
	}
}
