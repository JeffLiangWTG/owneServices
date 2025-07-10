using System.Linq;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.ProductCatalog.Testing
{
	class CompositeAttributeProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var question1 = Factory.New<RefCusProfileQuestion>();
			question1.XQ2_Code = "ATT_1";
			question1.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Compound;

			var question11 = Factory.New<RefCusProfileQuestion>();
			question11.XQ2_Code = "ATT_11";
			question11.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;
			question11.XQ2_StartDate = ZDateTime.Now.AddDays(-10);

			var question12 = Factory.New<RefCusProfileQuestion>();
			question12.XQ2_Code = "ATT_12";
			question12.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;
			question12.XQ2_StartDate = ZDateTime.Now.AddDays(-10);

			var question13 = Factory.New<RefCusProfileQuestion>();
			question13.XQ2_Code = "ATT_13";
			question13.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.Number;
			question13.XQ2_StartDate = ZDateTime.Now.AddDays(10);

			var pathway1 = Factory.New<RefCusProfileQuestionPathway>();
			pathway1.XQP_XQ2_QuestionParent = question1.PK;
			pathway1.XQP_XQ2_QuestionChild = question11.PK;

			var pathway2 = Factory.New<RefCusProfileQuestionPathway>();
			pathway2.XQP_XQ2_QuestionParent = question1.PK;
			pathway2.XQP_XQ2_QuestionChild = question12.PK;

			var pathway3 = Factory.New<RefCusProfileQuestionPathway>();
			pathway3.XQP_XQ2_QuestionParent = question1.PK;
			pathway3.XQP_XQ2_QuestionChild = question13.PK;

			AssertNull(CompositeAttributeProvider.New(null));

			var catalog = Factory.New<CusGoodsCatalog>();
			var att1 = catalog.Attributes.AddNew("ATT_1");
			att1.TariffProfileQuestion = TariffProfileQuestion.New(question1);
			AssertNull(CompositeAttributeProvider.New(att1));

			var att11 = catalog.Attributes.AddNew("ATT_11");
			att11.TariffProfileQuestion = TariffProfileQuestion.New(question11);
			att11.QuestionPathway = pathway1;
			var att12 = catalog.Attributes.AddNew("ATT_12");
			att12.TariffProfileQuestion = TariffProfileQuestion.New(question12);
			att12.QuestionPathway = pathway2;
			var att13 = catalog.Attributes.AddNew("ATT_13");
			att13.TariffProfileQuestion = TariffProfileQuestion.New(question13);
			att13.QuestionPathway = pathway3;

			AssertNull(CompositeAttributeProvider.New(att1));
			AssertNull(CompositeAttributeProvider.New(att11));
			AssertNull(CompositeAttributeProvider.New(att12));
			AssertNull(CompositeAttributeProvider.New(att12));

			att11.CY_Data = "123";

			var provider = CompositeAttributeProvider.New(att1);
			CombineAssertions(() =>
			{
				AssertEquals("Attribute", "ATT_1", provider.Attribute);
				AssertEquals("Values", 1, provider.Values.Count());
				AssertAttribute(provider.Values.ElementAt(0), "ATT_11", "123");
			});

			att12.CY_Data = "456";
			provider = CompositeAttributeProvider.New(att1);
			CombineAssertions(() =>
			{
				AssertEquals("Attribute", "ATT_1", provider.Attribute);
				AssertEquals("Values", 2, provider.Values.Count());
				AssertAttribute(provider.Values.ElementAt(0), "ATT_11", "123");
				AssertAttribute(provider.Values.ElementAt(1), "ATT_12", "456");
			});

			att13.CY_Data = "789";
			provider = CompositeAttributeProvider.New(att1);
			CombineAssertions(() =>
			{
				AssertEquals("Attribute", "ATT_1", provider.Attribute);
				AssertEquals("Values", 2, provider.Values.Count());
				AssertAttribute(provider.Values.ElementAt(0), "ATT_11", "123");
				AssertAttribute(provider.Values.ElementAt(1), "ATT_12", "456");
			});

			void AssertAttribute(IAttribute attribute, string expectedAttribute, string expectedValue)
			{
				AssertEquals("Attribute", expectedAttribute, attribute.Attribute);
				AssertEquals("Value", expectedValue, attribute.Value);
			}
		}
	}
}
