//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRFPNumberAddInfoLookups
//
//    This class should be used for overriding collections in AutoRFPNumberAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPNumberAddInfoLookups : AutoRFPNumberAddInfoLookups
	{
		public RFPNumberAddInfoLookups(AutoRFPNumberAddInfo parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList NetQuantityUnits => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EXDOCSUnitOfMeasurement, ZDate.Today);

		public EXDOCPacakgeTypeCodes PackageTypes
		{
			get { return new EXDOCPacakgeTypeCodes(); }
		}
	}
}
