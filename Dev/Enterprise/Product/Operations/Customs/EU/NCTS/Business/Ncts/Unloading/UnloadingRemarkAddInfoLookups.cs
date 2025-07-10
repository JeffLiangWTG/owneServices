//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUnloadingRemarkAddInfoLookups
//
//    This class should be used for overriding collections in AutoUnloadingRemarkAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;
namespace Enterprise.Customs.EU.NCTS.Business
{
	public class UnloadingRemarkAddInfoLookups : AutoUnloadingRemarkAddInfoLookups
	{
		public UnloadingRemarkAddInfoLookups(AutoUnloadingRemarkAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList YesNoCodeList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();

		public CodeDescriptionPairList YesNoEmptyCodeList => Factory.GetCachedValue<YesNoEmpty>();
	}
}
