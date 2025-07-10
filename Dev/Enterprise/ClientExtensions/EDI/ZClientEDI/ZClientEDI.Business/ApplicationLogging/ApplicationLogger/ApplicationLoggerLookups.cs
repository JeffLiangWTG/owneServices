//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoApplicationLoggerLookups
//
//    This class should be used for overriding collections in AutoApplicationLoggerLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ApplicationLogging.Business
{
	public class ApplicationLoggerLookups : AutoApplicationLoggerLookups
	{
		public ApplicationLoggerLookups(AutoApplicationLogger parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Products
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(ApplicationLoggerProducts.CargoWise);
				return result;
			}
		}

		public static class ApplicationLoggerProducts
		{
			public const string CargoWise = "CargoWise";
		}
	}
}
