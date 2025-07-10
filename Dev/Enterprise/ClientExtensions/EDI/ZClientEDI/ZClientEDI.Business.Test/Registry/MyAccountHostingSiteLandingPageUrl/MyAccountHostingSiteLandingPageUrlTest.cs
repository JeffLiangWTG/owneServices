using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(MyAccountHostingSiteLandingPageUrl))]
	public class MyAccountHostingSiteLandingPageUrlTest : RegistryBusinessObjectTemplateTestCase<MyAccountHostingSiteLandingPageUrl>
	{
		public void TestBaseValidation()
		{
			var collection = new MyAccountHostingSiteLandingPageUrlCollection();
			var rule_1 = collection.AddNew(ZString.Empty, ZString.Empty);
			var rule_2 = collection.AddNew("AAA", "www.myaccount.com/#test");
			var rule_3 = collection.AddNew(ProductTypes.Codes.CargoWiseOne, "www.myaccount.com/#CargoWiseOne");
			rule_1.ValidateEverything();
			AssertHasError(rule_1.ProductCodeInfo, "Please enter a Product.");
			AssertHasError(rule_1.LandingPageAbsoluteUrlInfo, "Please enter a valid URL");
			AssertHasError(rule_2.ProductCodeInfo, "Enter a valid Product.");
			AssertNoNotifications(rule_2.LandingPageAbsoluteUrlInfo);
			AssertNoNotifications(rule_3);
			var rule_4 = collection.AddNew(ProductTypes.Codes.CargoWiseOne, "www.myaccount.com/#CargoWiseOne");
			rule_4.ValidateEverything();
			AssertHasError(rule_3.ProductCodeInfo, "Duplicated products are not allowed");
			AssertHasError(rule_4.ProductCodeInfo, "Duplicated products are not allowed");
			AssertNoNotifications(rule_3.LandingPageAbsoluteUrlInfo);
			AssertNoNotifications(rule_4.LandingPageAbsoluteUrlInfo);
		}

		#region Implementation
		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override bool RequiresFactory
		{
			get
			{
				return true;
			}
		}

		protected override MyAccountHostingSiteLandingPageUrl GetBusinessObjectToClone()
		{
			return new MyAccountHostingSiteLandingPageUrl(null, new BusinessObjectFactory(), new MyAccountHostingSiteLandingPageUrlCollection());
		}

		protected override MyAccountHostingSiteLandingPageUrl GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
		#endregion
	}
}
