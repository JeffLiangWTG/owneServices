//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPayableOrderLineLookups
//
//    This class should be used for overriding collections in AutoAccPayableOrderLineLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.PayableOrder
{
	public class AccPayableOrderLineLookups : AutoAccPayableOrderLineLookups
	{
		public AccPayableOrderLineLookups(AutoAccPayableOrderLine parent) : base(parent)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public override GlbBranchCollection Branches
		{
			get
			{
				return Factory.GetCachedValue("Accounting" + GlbCompany.CurrentCompany.PK.ToStringKey() + nameof(GlbBranchCollection) + "ActiveCurrentCompany", () =>
				{
					ZQuery branchesQuery = new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
					branchesQuery.AddToFilter(GlbBranchSchema.GB_IsActive, ZBool.True);
					return new GlbBranchCollection(Factory, branchesQuery);
				});
			}
		}
	}
}

