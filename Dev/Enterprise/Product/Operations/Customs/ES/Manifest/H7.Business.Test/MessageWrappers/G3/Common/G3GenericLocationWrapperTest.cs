
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3GenericLocationWrapperTest : DataProviderTestCase<G3GenericLocationWrapper>
	{
		public void TestLocType()
		{
			AssertEquals("Expected filled Type", "B", wrapper.Type);
		}

		public void TestQualifier()
		{
			AssertEquals("Expected filled Qualifier", "Y", wrapper.Qualifier);
		}

		public void TestCoded()
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Coded", wrapper.Coded);
				AssertCached("Expected cached Coded", () => wrapper.Coded);
			});
		}

		public void TestAddress()
		{
			AssertNull("Expected null Address", wrapper.Address);
		}

		public void TestType()
		{
			AssertEquals("Expected type", CusGoodsLocationTypeList.Codes.AuthorizedPlace, wrapper.Type);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			wrapper = new G3GenericLocationWrapper(header);
		}

		protected override G3GenericLocationWrapper GetProvider()
		{
			return wrapper;
		}

		G3GenericLocationWrapper wrapper;
	}
}
