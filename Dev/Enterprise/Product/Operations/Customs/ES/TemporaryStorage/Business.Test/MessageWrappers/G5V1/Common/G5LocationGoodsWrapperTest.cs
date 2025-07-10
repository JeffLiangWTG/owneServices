using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5LocationGoodsWrapperTest : WrapperHelperTest<G5LocationGoodsWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if location is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "location"), () => GetWrapper("", null));
		}

		public void TestNationalLocation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled NationalLocation when AuthorisationNumber is filled in and customsOffice given starts with ES", "ES08123456", wrapper.NationalLocation);

				wrapper = GetWrapper("FR009999", goodsLocation);
				AssertEquals("Expected empty NationalLocation when AuthorisationNumber is filled in but customsOffice given doesn't start with ES", ZString.Empty, wrapper.NationalLocation);
			});
		}

		public void TestGenericLocation()
		{
			CombineAssertions(() =>
			{
				AssertNull("Expected empty GenericLocation when customsOffice given starts with ES", wrapper.GenericLocation);

				wrapper = GetWrapper("FR009999", goodsLocation);
				var genericLocation = wrapper.GenericLocation;
				AssertNotNull("Expected filled GenericLocation when customsOffice given doesn't start with ES", genericLocation);
				AssertSame("Cached GenericLocation", wrapper.GenericLocation, genericLocation);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsLocation = Factory.New<CusGoodsLocation>();
			goodsLocation.CGL_Qualifier = "Y";
			goodsLocation.Address.AuthorisationNumber = "ES08123456";

			wrapper = GetWrapper("ES009999", goodsLocation);
		}
		CusGoodsLocation goodsLocation;
		G5LocationGoodsWrapper wrapper;

		G5LocationGoodsWrapper GetWrapper(ZString customsOffice, CusGoodsLocation location) => new G5LocationGoodsWrapper(customsOffice, location);

		protected override G5LocationGoodsWrapper GetProvider() => wrapper;
	}
}
