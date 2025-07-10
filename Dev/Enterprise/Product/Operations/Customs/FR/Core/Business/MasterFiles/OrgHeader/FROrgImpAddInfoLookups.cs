//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFROrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoFROrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class FROrgImpAddInfoLookups : AutoFROrgImpAddInfoLookups
	{
		public FROrgImpAddInfoLookups(AutoFROrgImpAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList VATProcedureList => Factory.GetCachedValue<VATProcedureList>();

		public CodeDescriptionPairList DeltaG1SubProcedureList => Factory.GetCachedValue<DeltaG1SubProcedureList>();
	}
}
