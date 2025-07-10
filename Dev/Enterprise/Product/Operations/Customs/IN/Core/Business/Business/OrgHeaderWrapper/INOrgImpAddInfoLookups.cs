//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoINOrgImpAddInfoLookups
//
//    This class should be used for overriding collections in AutoINOrgImpAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business
{
	public class INOrgImpAddInfoLookups : AutoINOrgImpAddInfoLookups
	{
		public INOrgImpAddInfoLookups(AutoINOrgImpAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ExporterTypeList => Factory.GetCachedValue<ExporterTypeList>();

		public CodeDescriptionPairList ImporterTypeList => Factory.GetCachedValue<ImporterTypeList>();
	}
}
