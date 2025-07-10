//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPayableOrderHeaderLookups
//
//    This class should be used for overriding collections in AutoAccPayableOrderHeaderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderHeaderLookups : AutoAccPayableOrderHeaderLookups
	{
		public AccPayableOrderHeaderLookups(AutoAccPayableOrderHeader parent) : base(parent)
		{
		}

		public OrgHeaderCollection OrderedBy_OrgList
		{
			get
			{
				ZDBOnlyQuery orgQuery = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.GC_OH_OrgProxy);
				companyQuery.AddToFilter(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK);
				orgQuery.AddSubQuery(OrgHeaderSchema.PK, companyQuery, JoinCondition.And);

				ZDBOnlySubQuery branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_OH_OrgProxy);
				branchQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
				orgQuery.AddSubQuery(OrgHeaderSchema.PK, branchQuery, JoinCondition.Or);

				return new OrganisationsFindBoxCollection(Factory, orgQuery);
			}
		}

		public OrganisationsFindBoxCollection Supplier_OrgList
		{
			get
			{
				return new CreditorCollection(Factory);
			}
		}
	}
}

