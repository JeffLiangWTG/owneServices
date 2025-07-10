//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoDFOPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoDFOPGAHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DFOPGAHeaderAddInfoLookups : AutoDFOPGAHeaderAddInfoLookups
	{
		public DFOPGAHeaderAddInfoLookups(AutoDFOPGAHeaderAddInfo parent) : base(parent)
		{
		}

		DFOPGAHeader PGAHeader => (DFOPGAHeader)((DFOPGAHeaderAddInfo)Parent).Parent;

		public CodeDescriptionPairList ProgramCodesList
		{
			get { return Factory.GetCachedValue<DFOPGADepartmentCodes>(); }
		}

		public OrgHeaderCollection HarvestingParties
		{
			get { return Factory.GetCachedValue("DFOHarvestingParties", () => new OrgHeaderCollection(Factory)); }
		}

		public OrgHeaderCollection Processors
		{
			get { return Factory.GetCachedValue("DFOProcessors", () => new OrgHeaderCollection(Factory)); }
		}

		public CodeDescriptionPairList ScientificNames
		{
			get { return Factory.GetCachedValue("DFOAISScientificNames", () => new DFOScientificNames()); }
		}

		public CodeDescriptionPairList CategoryList
		{
			get { return Factory.GetCachedValue("DFOCategoryList", () => new DFOProductCategories()); }
		}

		public CodeDescriptionPairList DirectionList
		{
			get { return Factory.GetCachedValue("DFODirectionList", () => new DFOPGADirections()); }
		}

		public CodeDescriptionPairList CommonNameCodes
		{
			get { return Factory.GetCachedValue("DFOCommonNameCodes", () => new DFOCommonNameCodes()); }
		}

		public CodeDescriptionPairList CommissionList
		{
			get { return Factory.GetCachedValue("DFOCommissionList", () => new DFOCommissionList()); }
		}

		public RefCountryCollection CountryOfOriginsLookup
		{
			get
			{
				if (PGAHeader is DFOPGAHeader pgaHeader)
				{
					if (pgaHeader.RequirementsParent is IHasPGARequirements requirementsParent)
					{
						return requirementsParent.CountryOfOriginsLookup;
					}
				}
				return new RefCountryCollection(Factory);
			}
		}
	}
}
