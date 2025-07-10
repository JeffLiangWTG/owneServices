//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceDiscountGroupMemberLookups
//
//    This class should be used for overriding collections in AutoEdiPriceDiscountGroupMemberLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiPriceDiscountGroupMemberLookups : AutoEdiPriceDiscountGroupMemberLookups
	{
		public EdiPriceDiscountGroupMemberLookups(AutoEdiPriceDiscountGroupMember parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList DiscountNames
		{
			get
			{
				var parent = (EdiPriceDiscountGroupMember)Parent;
				var headerDiscount = parent != null ? parent.HeaderDiscount : null;
				var result = new CodeDescriptionPairList();
				if (headerDiscount != null)
				{
					var availableDiscounts = EdiPriceHeaderDiscountCollection.GetCachedValue(Factory, headerDiscount.PHD_Version);

					foreach (EdiPriceHeaderDiscount discount in availableDiscounts.OrderBy(x => x.PHD_Name))
					{
						result.AddPairIfNotExist(discount.PHD_Name, discount.NameDescription);
					}
				}
				return result;
			}
		}
	}
}

