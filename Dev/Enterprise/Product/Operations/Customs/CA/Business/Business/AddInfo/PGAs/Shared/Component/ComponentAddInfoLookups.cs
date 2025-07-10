//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoComponentAddInfoLookups
//
//    This class should be used for overriding collections in AutoComponentAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ComponentAddInfoLookups : AutoComponentAddInfoLookups
	{
		public ComponentAddInfoLookups(AutoComponentAddInfo parent) : base(parent)
		{
		}

		public Component Component
		{
			get { return (Component)((ComponentAddInfo)Parent).Parent; }
		}

		public CodeDescriptionPairList UnitList
		{
			get
			{
				var pgaHeader = Component.Parent as IPGAHeader;
				var result = new CodeDescriptionPairList();
				if (pgaHeader != null)
				{
					switch (pgaHeader.GovAgencyIDCode)
					{
						case PGACodes.Codes.HC:
						case PGACodes.Codes.ECCC:
							result = Factory.GetCachedValue<UnitOfIngredientQuantity>();
							break;
						case PGACodes.Codes.CNSC:
							result = GetCNSCUniqCodeList(Component.Parent);
							break;
						default:
							result = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);
							break;
					}
				}
				return result;
			}
		}

		public CodeDescriptionPairList ComponentTypes
		{
			get
			{
				var pgaHeader = Component?.Parent as ECCCPGAHeader;
				if (pgaHeader != null && pgaHeader.CA_WENProgramInd == YesNoList.Codes.Yes)
				{
					return Factory.GetCachedValue("ECCCComponentTypes", () =>
					{
						var result = new IDTypeCodes();
						result.RemoveCode(IDTypeCodes.Codes.BN);
						result.RemoveCode(IDTypeCodes.Codes.EE);
						result.RemoveCode(IDTypeCodes.Codes.VV);

						return result;
					});
				}

				return new CodeDescriptionPairList();
			}
		}

		CodeDescriptionPairList GetCNSCUniqCodeList(BusinessObject parent)
		{
			var cnscHeader = parent as CNSCPGAHeader;
			var category = cnscHeader != null ? cnscHeader.CA_Category : ZString.Empty;
			return Factory.GetCachedValue("CA_ComponentVolumnUnitList" + category, delegate
			{
				if (category.IsEmpty)
				{
					return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);
				}
				return new ComponentVolumnUnitList(category);
			});
		}

		public CodeDescriptionPairList SpecificationList
		{
			get { return Factory.GetCachedValue<NuclearSubstanceSpecificationCodes>(); }
		}

		public virtual RefCountryCollection Countries => new RefCountryCollection(Factory);
	}
}
