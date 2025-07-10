//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCACargoControlNumberAddInfoLookups
//
//    This class should be used for overriding collections in AutoCACargoControlNumberAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	public class CACargoControlNumberAddInfoLookups : AutoCACargoControlNumberAddInfoLookups
	{
		public CACargoControlNumberAddInfoLookups(AutoCACargoControlNumberAddInfo parent) : base(parent)
		{
		}

		public BillCollection Bills
		{
			get { return ((JobDeclaration)Parent).Bills; }
		}
	}
}
