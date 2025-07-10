//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGenericTransactionLookups
//
//    This class should be used for overriding collections in AutoGenericTransactionLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	public class GenericTransactionLookups : AutoGenericTransactionLookups
	{
		public GenericTransactionLookups(AutoGenericTransaction parent)
			: base(parent)
		{
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public JobHeaderCollection Jobs
		{
			get { return new JobHeaderCollection(Factory); }
		}
	}
}

