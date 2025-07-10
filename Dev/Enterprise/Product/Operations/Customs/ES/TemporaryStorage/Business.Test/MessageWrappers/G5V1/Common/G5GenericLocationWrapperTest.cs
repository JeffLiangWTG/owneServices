using System;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers.Testing
{
	public class G5GenericLocationWrapperTest : WrapperHelperTest<G5GenericLocationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if location is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "location"), () => GetWrapper(null));
		}

		public void TestType()
		{
			goodsLocation.CGL_Type = "A";
			AssertEquals("Expected filled Type", "A", wrapper.Type);
		}

		public void TestQualifier()
		{
			goodsLocation.CGL_Qualifier = "T";
			AssertEquals("Expected filled Qualifier", "T", wrapper.Qualifier);
		}

		public void TestCoded()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "U";
				goodsLocation.Unlocode = "ESMAD";
				wrapper = GetWrapper(goodsLocation);
				var coded = wrapper.Coded;
				AssertNotNull("Expected filled Coded", coded);
				AssertSame("Cached Coded", wrapper.Coded, coded);

				goodsLocation.CGL_Qualifier = "Z";
				wrapper = GetWrapper(goodsLocation);
				AssertNull("Expected empty Coded when qualifier is Z (even if the fields are not empty)", wrapper.Coded);
			});
		}

		public void TestAddress()
		{
			CombineAssertions(() =>
			{
				goodsLocation.CGL_Qualifier = "Z";
				goodsLocation.Address.E2_Address1 = "Street Name";
				wrapper = GetWrapper(goodsLocation);
				var address = wrapper.Address;
				AssertNotNull("Expected filled Address", address);
				AssertSame("Cached Address", wrapper.Address, address);

				goodsLocation.CGL_Qualifier = "Y";
				wrapper = GetWrapper(goodsLocation);
				AssertNull("Expected empty Address when qualifier is not Z (even if the fields are not empty)", wrapper.Address);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			goodsLocation = Factory.New<CusGoodsLocation>();

			wrapper = GetWrapper(goodsLocation);
		}
		CusGoodsLocation goodsLocation;
		G5GenericLocationWrapper wrapper;

		G5GenericLocationWrapper GetWrapper(CusGoodsLocation location) => new G5GenericLocationWrapper(location);

		protected override G5GenericLocationWrapper GetProvider() => wrapper;
	}
}
