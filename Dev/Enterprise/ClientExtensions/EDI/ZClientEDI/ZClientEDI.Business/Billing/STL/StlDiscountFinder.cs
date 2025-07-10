using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class StlDiscountFinder
	{
		public void AddDiscounts(IEnumerable<IStlDiscount> discounts)
		{
			foreach (var discount in discounts)
			{
				var masterOrgPk = discount.MonthlyUsage.Database.LD_OH_WebAccessOrg;
				var productCode = discount.MonthlyUsage.Database.LD_Product;
				var discountType = discount.HeaderDiscount.PHD_Type;

				var key = CreateKey(masterOrgPk, productCode, discountType);
				Discounts[key] = discount;
			}
		}

		public IStlDiscount GetDiscount(ZGuid masterOrgPk, string productCode, string discountType)
		{
			var key = CreateKey(masterOrgPk, productCode, discountType);
			return Discounts.TryGetValue(key, out var result) ? result  : null ;
		}

		public IStlDiscount GetCargoWiseOneDevelopingCountryDiscount(ZGuid masterOrgPk)
			=> GetDiscount(masterOrgPk, ProductTypes.Codes.CargoWiseOne, BillingConstants.DiscountCalculator.DevelopingCountry);

		readonly Dictionary<string, IStlDiscount> Discounts = new Dictionary<string, IStlDiscount>();

		string CreateKey(ZGuid masterOrgPk, string productCode, string discountType)
			=> $"{KeyTypeOrgProductDiscount}:{masterOrgPk}:{productCode}:{discountType}";

		const string KeyTypeOrgProductDiscount = "OPD";
	}
}
