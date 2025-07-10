using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class KROrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZO_BankCode()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organisation);
			wrapper.ZO_BankCode = "";
			AssertNoMessageErrors(wrapper.ZO_BankCodeInfo);

			wrapper.ZO_BankCode = "Z";
			AssertHasMessageErrorContaining(wrapper.ZO_BankCodeInfo, ListValidation.InvalidCodeMessageError);

			wrapper.ZO_BankCode = BankTypeList.Codes._002;
			AssertNoMessageErrors(wrapper.ZO_BankCodeInfo);

			wrapper.ZO_BankCode = BankTypeList.Codes._020;
			AssertNoMessageErrors(wrapper.ZO_BankCodeInfo);
		}

		public void TestCheckZO_VATDeferment()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = OrgHeaderWrapper.New(organisation);
			wrapper.ZO_VATDeferment = "";
			AssertNoMessageErrors(wrapper.ZO_VATDefermentInfo);

			wrapper.ZO_VATDeferment = "Z";
			AssertHasMessageErrorContaining(wrapper.ZO_VATDefermentInfo, ListValidation.InvalidCodeMessageError);

			wrapper.ZO_VATDeferment = VATDefermentType.Codes.Y;
			AssertNoMessageErrors(wrapper.ZO_VATDefermentInfo);

			wrapper.ZO_VATDeferment = VATDefermentType.Codes.Y1;
			AssertNoMessageErrors(wrapper.ZO_VATDefermentInfo);
		}
	}
}
