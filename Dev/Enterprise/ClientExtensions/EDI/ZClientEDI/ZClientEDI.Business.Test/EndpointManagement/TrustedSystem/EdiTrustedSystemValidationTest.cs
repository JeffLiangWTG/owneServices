namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class EdiTrustedSystemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckETS_Product()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Product = "XXX";
			AssertHasError(sys.ETS_ProductInfo, "Enter a valid selection.");
			sys.ETS_Product = "";
			AssertHasError(sys.ETS_ProductInfo, "Please enter a value.");
			sys.ETS_Product = "CW1";
			AssertNoErrors(sys.ETS_ProductInfo);
		}

		public void TestETS_AccessTokenExpiryOverride()
		{
			var sys = Factory.New<EdiTrustedSystem>();
			sys.ETS_Product = "CW1";
			sys.ETS_AccessTokenExpiryOverride = 0;
			AssertNoErrors(sys.ETS_AccessTokenExpiryOverrideInfo);
			sys.ETS_AccessTokenExpiryOverride = -10;
			AssertHasError(sys.ETS_AccessTokenExpiryOverrideInfo, "value cannot be negative.");
		}
	}
}
