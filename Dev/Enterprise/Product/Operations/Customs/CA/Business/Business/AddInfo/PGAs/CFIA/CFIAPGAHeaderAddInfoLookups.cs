//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCFIAPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoCFIAPGAHeaderAddInfoLookups
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
	public class CFIAPGAHeaderAddInfoLookups : AutoCFIAPGAHeaderAddInfoLookups
	{
		public CFIAPGAHeaderAddInfoLookups(AutoCFIAPGAHeaderAddInfo parent)
			: base(parent)
		{
		}

		CFIAPGAHeader PGAHeader => (CFIAPGAHeader)((CFIAPGAHeaderAddInfo)Parent).Parent;

		public CACFIAEndUseCodesCollection EndUseCodes
		{
			get { return new CACFIAEndUseCodesCollection(Factory); }
		}

		public CACFIAMiscCodesCollection AirsMiscellaneous
		{
			get { return new CACFIAMiscCodesCollection(Factory); }
		}

		public CodeDescriptionPairList PGAIndicatorList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<CFIAPGADepartmentCodes>(); }
		}

		public RefCountryCollection CountryOfSourceLookup
		{
			get
			{
				if (PGAHeader is CFIAPGAHeader pgaHeader)
				{
					if (pgaHeader.RequirementsParent is IHasPGARequirements requirementsParent)
					{
						return requirementsParent.CountryOfSourceLookup;
					}
				}
				return new RefCountryCollection(Factory);
			}
		}

		public CodeDescriptionPairList CountryOfSourceStateLookup
		{
			get
			{
				if (PGAHeader is CFIAPGAHeader pgaHeader)
				{
					if (pgaHeader.RequirementsParent is IHasPGARequirements requirementsParent)
					{
						return requirementsParent.CountryOfSourceStateLookup;
					}
				}
				return new CodeDescriptionPairList();
			}
		}
	}
}
