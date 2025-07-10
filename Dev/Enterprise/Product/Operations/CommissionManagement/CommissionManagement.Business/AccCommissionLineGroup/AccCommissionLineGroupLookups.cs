//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccCommissionLineGroupLookups
//
//    This class should be used for overriding collections in AutoAccCommissionLineGroupLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLineGroupLookups : AutoAccCommissionLineGroupLookups
	{
		public AccCommissionLineGroupLookups(AutoAccCommissionLineGroup parent) : base(parent)
		{
		}

		#region CommissionHeaders

		public virtual AccCommissionHeaderCollection CommissionHeaders
		{
			get { return new AccCommissionHeaderCollection(Factory); }
		}

		#endregion
	}
}
