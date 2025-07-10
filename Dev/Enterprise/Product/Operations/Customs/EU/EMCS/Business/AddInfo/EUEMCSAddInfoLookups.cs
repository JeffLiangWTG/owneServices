//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEUEMCSAddInfoLookups
//
//    This class should be used for overriding collections in AutoEUEMCSAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EUEMCSAddInfoLookups : AutoEUEMCSAddInfoLookups
	{
		public EUEMCSAddInfoLookups(AutoEUEMCSAddInfo parent)
			: base(parent)
		{
		}

		protected new AddInfo Parent
		{
			get { return (AddInfo)base.Parent; }
		}
	}
}
