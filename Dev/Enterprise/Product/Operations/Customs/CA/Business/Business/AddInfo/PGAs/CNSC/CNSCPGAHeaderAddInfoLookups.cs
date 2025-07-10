//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCNSCPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoCNSCPGAHeaderAddInfoLookups
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
	public class CNSCPGAHeaderAddInfoLookups : AutoCNSCPGAHeaderAddInfoLookups
	{
		public CNSCPGAHeaderAddInfoLookups(AutoCNSCPGAHeaderAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList PGAIndicatorList
		{
			get { return Factory.GetCachedValue<YesNoList>(); }
		}

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<CNSCPGADepartmentCodes>(); }
		}

		public CodeDescriptionPairList Categories
		{
			get
			{
				return Factory.GetCachedValue<CNSCCategories>();
			}
		}

		public CodeDescriptionPairList PackUQList => Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);

		public UNDGSubstanceCollection UNDGCodeList => PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);
	}
}
