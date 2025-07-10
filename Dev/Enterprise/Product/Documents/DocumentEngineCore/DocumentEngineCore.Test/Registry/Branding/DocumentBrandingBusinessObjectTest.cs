using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	public abstract class DocumentBrandingBusinessObjectTest : ClientAndAgentBrandingBusinessObjectTestCase
	{
		public void TestBrandName()
		{
			AssertEquals("Precondition: Brand Name should be empty", true, BizObj.BrandName.IsEmpty);
			BizObj.BrandName = "test";
			AssertEquals("Brand Name:", "test", BizObj.BrandName);

			BizObj.BrandName = ZString.Empty;
			AssertEquals("Brand name should have errors", true, BizObj.BrandNameInfo.HasError("Please enter a Brand Name."));
		}

		public void TestBrandEmailAddress()
		{
			AssertEquals("Precondition: Brand EmailAddress should be empty", true, BizObj.BrandEmailAddress.IsEmpty);
			BizObj.BrandEmailAddress = "123";
			Assert("Should have errors", BizObj.BrandEmailAddressInfo.HasErrors());
			BizObj.BrandEmailAddress = "abc@edi.com.au";
			Assert("Should not have errors", !BizObj.BrandEmailAddressInfo.HasErrors());
			AssertEquals("Brand email address:", "abc@edi.com.au", BizObj.BrandEmailAddress);

			BizObj.BrandEmailAddress = ZString.Empty;
			AssertEquals("Brand email should have errors", true, BizObj.BrandEmailAddressInfo.HasError("Please enter a valid email address."));
		}

		public void TestUseGeneric()
		{
			AssertEquals("Precondition: Use Generic", false, BizObj.UseGeneric);
			BizObj.UseGeneric = ZBool.True;
			AssertEquals("Use Generic:", true, BizObj.UseGeneric);
		}

		protected new DocumentBrandingBusinessObject BizObj
		{
			get
			{
				return (DocumentBrandingBusinessObject)base.BizObj;
			}
		}
	}
}
