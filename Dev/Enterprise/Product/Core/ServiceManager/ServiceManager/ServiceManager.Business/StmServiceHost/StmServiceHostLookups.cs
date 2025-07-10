//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoStmServiceHostLookups
//
//    This class should be used for overriding collections in AutoStmServiceHostLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.ServiceManager.Business
{
	public class StmServiceHostLookups : AutoStmServiceHostLookups
	{
		public StmServiceHostLookups(AutoStmServiceHost parent) : base(parent)
		{
		}

		public CodeDescriptionPairList StmServiceHostStatusTypes
		{
			get { return new StmServiceHostStatusTypes(); }
		}
	}
}

