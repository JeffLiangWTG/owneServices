//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNRCanPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoNRCanPGAHeaderAddInfoLookups
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
	public class NRCanPGAHeaderAddInfoLookups : AutoNRCanPGAHeaderAddInfoLookups
	{
		public NRCanPGAHeaderAddInfoLookups(AutoNRCanPGAHeaderAddInfo parent) : base(parent)
		{
		}

		NRCanPGAHeader PGAHeader => (NRCanPGAHeader)((NRCanPGAHeaderAddInfo)Parent).Parent;

		public CodeDescriptionPairList IntendedUseCodeList => Factory.GetCachedValue("NRCanIntendedUseCodes", () =>
																													{
																														var result = new NRCanIntendedUseCodes();
																														result.RemoveCode(NRCanIntendedUseCodes.Codes.NR04);
																														return (CodeDescriptionPairList)result;
																													});

		public LPCOHolderPartyTypeCodes AuthorizedPartyTypeCodes => Factory.GetCachedValue("NRCanHolderPartyTypeCodeRemoved", () =>
																														{
																															var result = new LPCOHolderPartyTypeCodes();
																															result.RemoveCode(LPCOHolderPartyTypeCodes.Codes.Exporter);
																															result.RemoveCode(LPCOHolderPartyTypeCodes.Codes.Other);
																															return result;
																														});

		public CodeDescriptionPairList CustomsUQList => Universal.RefCusCodeListTypes.GetCachedListValidBeforeDate(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, UniversalReferenceConstants.UNPackTypeStartDate);

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<NRCanPGADepartmentCodes>(); }
		}

		public UNDGSubstanceCollection UNDGCodeList => PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);

		public RefCountryCollection CountryOfOriginsLookup
		{
			get
			{
				if (PGAHeader is NRCanPGAHeader pgaHeader)
				{
					if (pgaHeader.RequirementsParent is IHasPGARequirements requirementsParent)
					{
						return requirementsParent.CountryOfOriginsLookup;
					}
				}
				return new RefCountryCollection(Factory);
			}
		}

		public CodeDescriptionPairList StateCodeListLookup
		{
			get
			{
				if (PGAHeader is NRCanPGAHeader pgaHeader)
				{
					if (pgaHeader.RequirementsParent is IHasPGARequirements requirementsParent)
					{
						return requirementsParent.StateCodeListLookup;
					}
				}
				return new CodeDescriptionPairList();
			}
		}

		public CodeDescriptionPairList PGAIndicatorList => Factory.GetCachedValue<YesNoList>();
	}
}
