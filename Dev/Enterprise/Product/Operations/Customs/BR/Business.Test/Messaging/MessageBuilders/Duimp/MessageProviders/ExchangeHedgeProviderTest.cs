using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class ExchangeHedgeProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertNull(ExchangeHedgeProvider.New(null));

			var declaration = Factory.New<JobDeclaration>();
			AssertType<ExchangeHedgeProvider>(ExchangeHedgeProvider.New(declaration.Invoices.AddNew()));
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			var dataProvider = ExchangeHedgeProvider.New(invoice);
			CombineAssertions(() =>
			{
				Assert("Type should be Empty", dataProvider.Type.IsEmpty());
				Assert("ROFNumber should be Empty", dataProvider.ROFNumber.IsEmpty());
				AssertNull("FinancialInstitution should be Null", dataProvider.FinancialInstitution);
				AssertEquals("Value should be Zero", 0d, dataProvider.Value);
				AssertNull("ReasonCode should be Null", dataProvider.ReasonCode);
			});

			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._1;
			invoice.ExchangeHedgeROFBACENNumber = "BDGA96";
			invoice.ExchangeHedgeFinancialInstitution = "99";
			invoice.ExchangeHedgeValue = 200.33;
			invoice.ExchangeHedgeReason = "52";

			dataProvider = ExchangeHedgeProvider.New(invoice);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be ATE_180_DIAS", "ATE_180_DIAS", dataProvider.Type);
				AssertEquals("ROFNumber should be BDGA96", "BDGA96", dataProvider.ROFNumber);
				AssertNull("FinancialInstitution should be Null", dataProvider.FinancialInstitution);
				AssertEquals("Value should be 200.33", 200.33, dataProvider.Value);
				AssertNull("ReasonCode should be Null", dataProvider.ReasonCode);
			});

			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._2;
			invoice.ExchangeHedgeROFBACENNumber = "BDGA96";
			invoice.ExchangeHedgeFinancialInstitution = "99";
			invoice.ExchangeHedgeValue = 200.33;
			invoice.ExchangeHedgeReason = "52";

			dataProvider = ExchangeHedgeProvider.New(invoice);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be DE_181_ATE_360", "DE_181_ATE_360", dataProvider.Type);
				AssertEquals("ROFNumber should be BDGA96", "BDGA96", dataProvider.ROFNumber);
				AssertEquals("FinancialInstitution should be 99", 99, dataProvider.FinancialInstitution);
				AssertEquals("Value should be 200.33", 200.33, dataProvider.Value);
				AssertEquals("ReasonCode should be 52", 52, dataProvider.ReasonCode);
			});

			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._3;
			invoice.ExchangeHedgeROFBACENNumber = "BDGA96";
			invoice.ExchangeHedgeFinancialInstitution = "99";
			invoice.ExchangeHedgeValue = 200.33;
			invoice.ExchangeHedgeReason = "52";

			dataProvider = ExchangeHedgeProvider.New(invoice);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be ACIMA_360", "ACIMA_360", dataProvider.Type);
				AssertEquals("ROFNumber should be BDGA96", "BDGA96", dataProvider.ROFNumber);
				AssertEquals("FinancialInstitution should be 99", 99, dataProvider.FinancialInstitution);
				AssertEquals("Value should be 200.33", 200.33, dataProvider.Value);
				AssertEquals("ReasonCode should be 52", 52, dataProvider.ReasonCode);
			});

			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._4;
			invoice.ExchangeHedgeROFBACENNumber = "BDGA96";
			invoice.ExchangeHedgeFinancialInstitution = "99";
			invoice.ExchangeHedgeValue = 200.33;
			invoice.ExchangeHedgeReason = "52";

			dataProvider = ExchangeHedgeProvider.New(invoice);
			CombineAssertions(() =>
			{
				AssertEquals("Type should be SEM_COBERTURA", "SEM_COBERTURA", dataProvider.Type);
				AssertEquals("ROFNumber should be BDGA96", "BDGA96", dataProvider.ROFNumber);
				AssertEquals("FinancialInstitution should be 99", 99, dataProvider.FinancialInstitution);
				AssertEquals("Value should be 200.33", 200.33, dataProvider.Value);
				AssertEquals("ReasonCode should be 52", 52, dataProvider.ReasonCode);
			});
		}
	}
}
