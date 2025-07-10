using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class JobComInvoiceHeaderDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneExchangeHedge()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var header = declaration.Invoices.AddNew();
			header.ExchangeHedgeReason = "1";
			header.ExchangeHedgeROFBACENNumber = "1";
			header.ExchangeHedgeFinancialInstitution = "1";
			header.ExchangeHedgeValue = 1;
			header.ExchangeHedgeType = ExchangeHedgeList.Codes._3;

			var clonedInvoice = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(header, CloneType.TemplateCopy, declaration, null).Clone();
			CombineAssertions(() =>
			{
				AssertEquals("ExchangeHedgeFinancialInstitution should be the same", "1", clonedInvoice?.ExchangeHedgeFinancialInstitution);
				AssertEquals("ExchangeHedgeROFBACENNumber should be the same", "1", clonedInvoice?.ExchangeHedgeROFBACENNumber);
				AssertEquals("ExchangeHedgeType should be the same", ExchangeHedgeList.Codes._3, clonedInvoice?.ExchangeHedgeType);
				AssertEquals("ExchangeHedgeValue should be the same", 1m, clonedInvoice?.ExchangeHedgeValue);
			});

			header.ExchangeHedgeReason = "1";
			header.ExchangeHedgeType = ExchangeHedgeList.Codes._4;

			clonedInvoice = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(header, CloneType.TemplateCopy, declaration, null).Clone();

			CombineAssertions(() =>
			{
				AssertEquals("ExchangeHedgeReason should be the same", "1", clonedInvoice?.ExchangeHedgeReason);
				AssertEquals("ExchangeHedgeType should be the same", ExchangeHedgeList.Codes._4, clonedInvoice?.ExchangeHedgeType);
			});
		}

		public void TestCloneInvoiceWeight()
		{
			var declarationImp = Factory.NewWithValidTestData<JobDeclaration>();
			declarationImp.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var headerImp = declarationImp.Invoices.AddNew();
			headerImp.JZ_Weight = 10m;
			headerImp.JZ_NetWeight = 8m;

			var clonedInvoiceImp = (JobComInvoiceHeader)new JobComInvoiceHeaderDeepCloneStrategy(headerImp, CloneType.TemplateCopy, declarationImp, null).Clone();
			CombineAssertions(() =>
			{
				AssertEquals("JZ_Weight should be the same", headerImp.JZ_Weight, clonedInvoiceImp.JZ_Weight);
				AssertEquals("JZ_NetWeight should be the same", headerImp.JZ_NetWeight, clonedInvoiceImp.JZ_NetWeight);
			});
		}
	}
}
