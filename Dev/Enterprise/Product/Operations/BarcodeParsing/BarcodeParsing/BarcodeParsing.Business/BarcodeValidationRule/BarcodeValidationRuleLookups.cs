//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoBarcodeValidationRuleLookups
//
//    This class should be used for overriding collections in AutoBarcodeValidationRuleLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	public class BarcodeValidationRuleLookups : AutoBarcodeValidationRuleLookups
	{
		public BarcodeValidationRuleLookups(AutoBarcodeValidationRule parent) : base(parent)
		{
		}

		new BarcodeValidationRule Parent => (BarcodeValidationRule)base.Parent;

		#region LengthTypes

		public LengthTypes LengthTypes => Factory.GetCachedValue("BarcodeValidationRuleLookups|LengthTypes", () => new LengthTypes());

		#endregion

		#region FormatTypes

		public ReadOnlyCodeDescriptionPairList FormatTypes => Factory.GetCachedValue("BarcodeValidationRuleLookups|FormatTypes", () => BarcodeFormatHelper.GetFormatTypes(false));

		#endregion

		#region TargetFields

		public ReadOnlyCodeDescriptionPairList TargetFields => Factory.GetCachedValue("BarcodeValidationRuleLookups|TargetFields|" + Parent.Module, () => Factory.GetBarcodeParsingConsumerFromModuleCode(Parent.Module).TargetFields);

		#endregion
	}
}
