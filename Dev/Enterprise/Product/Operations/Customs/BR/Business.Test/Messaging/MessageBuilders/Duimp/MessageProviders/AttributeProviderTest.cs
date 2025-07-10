using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
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
			var dataProvider = AttributeProvider.New(Factory.New<AttributeCusCodeData>());
			CombineAssertions(() =>
			{
				Assert("Code should be empty", dataProvider.Code.IsEmpty());
				Assert("Value should be empty", dataProvider.Value.IsEmpty());
			});

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var attribute = declaration.Invoices.AddNew().InvoiceLines.AddNew().Attributes.AddNew();
			attribute.CY_Code = "2024";
			attribute.CY_Data = "1996";

			dataProvider = AttributeProvider.New(attribute);

			CombineAssertions(() =>
			{
				AssertEquals("Code should be", "2024", dataProvider.Code);
				AssertEquals("Value should be", "1996", dataProvider.Value);
			});
		}
	}
}
