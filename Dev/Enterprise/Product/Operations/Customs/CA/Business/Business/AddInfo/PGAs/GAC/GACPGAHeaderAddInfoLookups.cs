//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGACPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoGACPGAHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class GACPGAHeaderAddInfoLookups : AutoGACPGAHeaderAddInfoLookups
	{
		public GACPGAHeaderAddInfoLookups(AutoGACPGAHeaderAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PGAIndicatorList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<GACPGADepartmentCodes>(); }
		}

		public CodeDescriptionPairList FTAProcessingCodes
		{
			get { return Factory.GetCachedValue<FTAProcessingCodes>(); }
		}

		public RefCountryCollection FibreOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}

		public RefCountryCollection YarnOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}

		public RefCountryCollection FabricOrigins
		{
			get { return new RefCountryCollection(Factory); }
		}
	}
}
