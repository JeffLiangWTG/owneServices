//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeRuleLookups
//
//    This class should be used for overriding collections in AutoBarcodeRuleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeRuleLookups : AutoBarcodeRuleLookups
	{
		public BarcodeRuleLookups(AutoBarcodeRule parent)
			: base(parent)
		{
		}

		#region TerminatorTypes

		public TerminatorTypes TerminatorTypes
		{
			get { return Factory.GetCachedValue("BarcodeRuleLookups|TerminatorTypes", () => new TerminatorTypes()); }
		}

		#endregion
	}
}
