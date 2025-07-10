using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.ProductCatalog.Testing
{
	class MultivaluedAttributeProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(MultivaluedAttributeProvider.New(null));
			AssertType<MultivaluedAttributeProvider>(MultivaluedAttributeProvider.New(Factory.New<AttributeCusCodeData>()));
		}

		public void TestProperties()
		{
			var question1 = Factory.New<RefCusProfileQuestion>();
			question1.XQ2_Code = "ATT_1";
			question1.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			question1.XQ2_AllowMultipleAnswers = true;

			var catalog = Factory.New<CusGoodsCatalog>();
			var attribute = catalog.Attributes.AddNew("ATT_1");
			attribute.TariffProfileQuestion = TariffProfileQuestion.New(question1);
			var provider = MultivaluedAttributeProvider.New(attribute);
			CombineAssertions(() =>
			{
				AssertEquals("Attribute", "ATT_1", provider.Attribute);
				AssertEquals("Values", 0, provider.Values.Count());
			});

			attribute.Content = "123\r\n\r\n234\r\n";

			provider = MultivaluedAttributeProvider.New(attribute);
			CombineAssertions(() =>
			{
				AssertEquals("Attribute", "ATT_1", provider.Attribute);
				AssertContainsExactElementsInExactOrder(new[] { "123", "234" }, provider.Values);
			});
		}
	}
}
