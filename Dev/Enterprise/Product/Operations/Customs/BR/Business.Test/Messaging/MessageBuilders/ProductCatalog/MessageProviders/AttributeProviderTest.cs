using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.ProductCatalog.Testing
{
	class AttributeProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(AttributeProvider.New(null));
			AssertType<AttributeProvider>(AttributeProvider.New(Factory.New<AttributeCusCodeData>()));
		}

		public void TestProperties()
		{
			var attribute = Factory.New<AttributeCusCodeData>();
			var dataProvider = AttributeProvider.New(attribute);

			CombineAssertions(() =>
			{
				AssertEquals("Attribute", string.Empty, dataProvider.Attribute);
				AssertEquals("Value", string.Empty, dataProvider.Value);
			});

			attribute.CY_Code = "2024";
			attribute.CY_Data = "1996";
			dataProvider = AttributeProvider.New(attribute);

			CombineAssertions(() =>
			{
				AssertEquals("Attribute", "2024", dataProvider.Attribute);
				AssertEquals("Value", "1996", dataProvider.Value);
			});
		}
	}
}
