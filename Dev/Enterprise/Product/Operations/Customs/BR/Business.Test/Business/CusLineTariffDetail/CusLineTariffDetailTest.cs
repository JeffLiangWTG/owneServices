using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(Customs.Business.CusLineTariffDetail))]
	class CusLineTariffDetailTest : Customs.Business.Testing.CusLineTariffDetailTest
	{
		public void TestLookups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";

			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			AssertType<CusLineTariffDetailLookups>(tariffDetail.Lookups);
		}

		public void TestValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";

			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			AssertType<CusLineTariffDetailValidation>(tariffDetail.Validation);
		}

		public void TestExNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "09022000";

			var tariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			AssertEquals("ExNumber", ZString.Empty, tariffDetail.ExNumber);

			tariffDetail.BZ_Tariff = "09022000_001";
			AssertEquals("ExNumber", "001", tariffDetail.ExNumber);

			tariffDetail.ExNumber = "002";
			AssertEquals("ExNumber", "002", tariffDetail.ExNumber);
			AssertEquals("BZ_Tariff", "09022000_002", tariffDetail.BZ_Tariff);

			tariffDetail.ExNumber = ZString.Empty;
			AssertEquals("ExNumber", ZString.Empty, tariffDetail.ExNumber);
			AssertEquals("BZ_Tariff", ZString.Empty, tariffDetail.BZ_Tariff);
		}

		public void TestIsSavedByFactory()
		{
			var cusLineTariffDetail = Factory.New<CusLineTariffDetail>();
			cusLineTariffDetail.BZ_Tariff = "1";
			cusLineTariffDetail.BZ_Type = "1";

			AssertEquals("IsSavedByFactory", true, cusLineTariffDetail.IsSavedByFactory);

			cusLineTariffDetail.BZ_Tariff = "";
			AssertEquals("IsSavedByFactory", true, cusLineTariffDetail.IsSavedByFactory);

			cusLineTariffDetail.BZ_Type = "";
			AssertEquals("IsSavedByFactory", false, cusLineTariffDetail.IsSavedByFactory);
		}

		public void TestOnSaving()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			Factory.Save();
			AssertEquals("Should not be saved if empty", false, cusLineTariffDetail.IsInDatabase);
			cusLineTariffDetail.BZ_Tariff = "1";
			cusLineTariffDetail.BZ_Type = "1";
			Factory.Save();
			AssertEquals("Should be saved if not empty", true, cusLineTariffDetail.IsInDatabase);
			cusLineTariffDetail.BZ_Tariff = ZString.Empty;
			cusLineTariffDetail.BZ_Type = ZString.Empty;
			Factory.Save();
			AssertEquals("Should be deleted if not empty", true, cusLineTariffDetail.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			cusLineTariffDetail.FillWithValidTestData();
			return cusLineTariffDetail;
		}
	}
}
