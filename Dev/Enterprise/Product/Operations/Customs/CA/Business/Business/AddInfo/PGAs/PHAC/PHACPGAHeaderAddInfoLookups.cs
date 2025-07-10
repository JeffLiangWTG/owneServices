//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPHACPGAHeaderAddInfoLookups
//
//    This class should be used for overriding collections in AutoPHACPGAHeaderAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Globalization;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PHACPGAHeaderAddInfoLookups : AutoPHACPGAHeaderAddInfoLookups
	{
		public PHACPGAHeaderAddInfoLookups(AutoPHACPGAHeaderAddInfo parent) : base(parent)
		{
		}

		PHACPGAHeader PGAHeader => (PHACPGAHeader)((PHACPGAHeaderAddInfo)Parent).Parent;

		public CodeDescriptionPairList IntendedUseCodes => Factory.GetCachedValue<PHACEndUseCodes>();

		CodeDescriptionPairList CategoryCodes => Factory.GetCachedValue<PHACCategories>();

		public CodeDescriptionPairList Categories
		{
			get
			{
				if (!IntendedUseCodes.ContainsCode(PGAHeader.CA_IntendedUseCode))
				{
					return CategoryCodes;
				}

				return Factory.GetCachedValue(
					string.Format(CultureInfo.InvariantCulture, "PHACCategories_{0}", PGAHeader.CA_IntendedUseCode),
					() =>
					{
						var result = new CodeDescriptionPairList();
						foreach (CodeDescriptionPair category in CategoryCodes)
						{
							if (PHACEndUseCodes.IsValidCategoryCode(PGAHeader.CA_IntendedUseCode, category.Code))
							{
								result.Add(category);
							}
						}

						return result;
					});
			}
		}

		public CodeDescriptionPairList PGAIndicatorList => Factory.GetCachedValue<YesNoList>();

		public CodeDescriptionPairList ProgramCodesList => Factory.GetCachedValue<PHACPGADepartmentCodes>();

		public UNDGSubstanceCollection UNDGCodeList => PGAHeaderExtensions.GetCachedUNDGSubstanceCollection(Factory);
	}
}
