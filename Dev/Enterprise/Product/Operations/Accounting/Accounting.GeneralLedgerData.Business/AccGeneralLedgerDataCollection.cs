using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class AccGeneralLedgerDataCollection : BusinessObjectCollection<AccGeneralLedgerData>
	{
		public AccGeneralLedgerDataCollection(BusinessObjectFactory factory) : base(factory)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery relationshipFilter = base.CreateRelationshipFilter();
			relationshipFilter.AddToFilter(AccGeneralLedgerDataSchema.GLD_GC_Company, GlbCompany.CurrentCompany.PK);
			return relationshipFilter;
		}
	}
}
