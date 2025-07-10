using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiLicenceSettingFlattened))]
	public class EdiLicenceSettingFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsValueProvided()
		{
			var obj1 = new EdiLicenceSettingFlattened();
			AssertEquals(false, obj1.IsBWPurchasedLicencesProvided);
			AssertEquals(false, obj1.IsDiscountActiveProvided);
			AssertEquals(false, obj1.IsPriceProvided);
			AssertEquals(false, obj1.IsDiscountPercentProvided);

			obj1.BWPurchasedLicencesAsText = "1";
			AssertEquals(true, obj1.IsBWPurchasedLicencesProvided);
			AssertEquals(1m, obj1.BWPurchasedLicences);
			obj1.DiscountActiveAsText = "Y";
			AssertEquals(true, obj1.IsDiscountActiveProvided);
			AssertEquals(true, obj1.DiscountActive);
			obj1.PriceAsText = "2";
			AssertEquals(true, obj1.IsPriceProvided);
			AssertEquals(2m, obj1.Price);
			obj1.DiscountPercentAsText = "3";
			AssertEquals(true, obj1.IsDiscountPercentProvided);
			AssertEquals(3m, obj1.DiscountPercent);
			AssertEquals(false, obj1.HasErrors);

			var obj2 = new EdiLicenceSettingFlattened();
			obj2.BWPurchasedLicencesAsText = "aaa";
			AssertEquals(false, obj2.IsBWPurchasedLicencesProvided);
			AssertEquals(0m, obj2.BWPurchasedLicences);
			obj2.DiscountActiveAsText = "bbb";
			AssertEquals(false, obj2.IsDiscountActiveProvided);
			AssertEquals(false, obj2.DiscountActive);
			obj2.PriceAsText = "ccc";
			AssertEquals(false, obj2.IsPriceProvided);
			AssertEquals(0m, obj2.Price);
			obj2.DiscountPercentAsText = "ddd";
			AssertEquals(false, obj2.IsDiscountPercentProvided);
			AssertEquals(0m, obj2.DiscountPercent);
			AssertEquals(true, obj2.HasErrors);
			var errMsg = string.Join("\r\n", obj2.Notifications.Where(x => x.Type.IsFatal).Select(x => x.Message));
			AssertEquals(@"Error - DiscountPercent: ddd is not a valid number
Error - DiscountActive: bbb is not a valid boolean value
Error - Price: ccc is not a valid number
Error - BWPurchasedLicences: aaa is not a valid number", errMsg);
		}
	}
}
