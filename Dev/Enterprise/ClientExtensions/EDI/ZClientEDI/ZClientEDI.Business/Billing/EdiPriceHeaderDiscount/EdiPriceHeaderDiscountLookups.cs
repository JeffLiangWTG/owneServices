//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceHeaderDiscountLookups
//
//    This class should be used for overriding collections in AutoEdiPriceHeaderDiscountLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceHeaderDiscountLookups : AutoEdiPriceHeaderDiscountLookups
	{
		public EdiPriceHeaderDiscountLookups(AutoEdiPriceHeaderDiscount parent) : base(parent)
		{
		}

		#region Names

		public CodeDescriptionPairList ActiveDiscountNames
		{
			get { return BuildDiscountNames(true); }
		}

		public CodeDescriptionPairList AllDiscountNames
		{
			get { return BuildDiscountNames(false); }
		}

		CodeDescriptionPairList BuildDiscountNames(bool activeOnly)
		{
			var result = new CodeDescriptionPairList();
			foreach (CodeDescriptionBool item in EDIDataRegistry.Instance.StlDiscountTypes.Value)
			{
				if (!activeOnly || item.Bool)
				{
					result.AddPair(item.Code, item.Description);
				}
			}
			return result;
		}

		#endregion

		#region Discount Types

		public CodeDescriptionPairList DiscountTypes
		{
			get { return GetStlDiscountCalculators(Factory); }
		}

		public static CodeDescriptionPairList GetStlDiscountCalculators(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("StlDiscountCalculators", () => { return BillingConstants.GetStlDiscountCalculatorList(); });
		}

		#endregion
	}
}

