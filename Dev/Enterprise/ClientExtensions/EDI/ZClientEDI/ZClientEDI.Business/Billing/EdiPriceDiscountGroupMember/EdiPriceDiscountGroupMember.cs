using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	/// <summary>
	/// An member of a Discount Group (called Discount Structure on the Excel pricelist)
	/// E.g. if a discount version had 10 discounts defined, but only 2 of those discounts were available
	/// for some price item. Those two discounts would be added to a discount group, identified by PGM_GroupCode.
	/// The group code would then be assigned to the price item.
	/// </summary>
	[CodeProperty(EdiPriceDiscountGroupMember.Schema.PGM_GroupCode)]
	public class EdiPriceDiscountGroupMember : AutoEdiPriceDiscountGroupMember
	{
		public EdiPriceDiscountGroupMember(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public EdiPriceHeaderDiscount HeaderDiscount
		{
			get { return Factory.Load<EdiPriceHeaderDiscount>(PGM_PHD); }
		}

		[RelatedBusinessObject("HeaderDiscount")]
		public override ZGuid PGM_PHD
		{
			get
			{
				return base.PGM_PHD;
			}
			set
			{
				base.PGM_PHD = value;
				discountName = null;
			}
		}

		[List("Lookups.DiscountNames")]
		public ZString DiscountName
		{
			get
			{
				return discountName ?? (discountName = HeaderDiscount?.PHD_Name ?? "");
			}
			set
			{
				var availableDiscounts = EdiPriceHeaderDiscountCollection.GetCachedValue(Factory, HeaderDiscount.PHD_Version);
				var matchByName = availableDiscounts.FirstOrDefault(x => x.PHD_Name.EqualsIgnoringCase(value));
				if (matchByName != null)
				{
					PGM_PHD = matchByName.PK;
				}

				discountName = value;
				PGM_PHDInfo.RefreshBinding();
			}
		}
		string discountName;

		public ZPropertyInfo DiscountNameInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DiscountName), x => PGM_PHDInfo); }
		}

		public ZString DiscountNameDesc
		{
			get { return Lookups.DiscountNames.GetDescriptionFromCode(DiscountName); }
		}
	}
}

