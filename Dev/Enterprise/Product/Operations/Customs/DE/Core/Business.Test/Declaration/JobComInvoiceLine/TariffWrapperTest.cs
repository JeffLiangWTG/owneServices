using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class TariffWrapperTest : TestCaseWithFactory
	{
		public void TestInvoiceLine()
		{
			AssertExceptionThrown<ArgumentNullException>("Null InvoiceLine", "Value cannot be null.\r\nParameter name: invoiceLine",
				() => new TariffWrapper(null));
		}

		public void TestSetTariffCode()
		{
			CombineAssertions("Tariff code should be set", () =>
			{
				AssertEquals("IMP", "0000000021", importTariff.Code);
				AssertEquals("EXP", "0000000020", exportTariff.Code);
				AssertEquals("NULL", ZString.Empty, nonTariff.Code);
			});
		}

		public void TestSetTariffDescription()
		{
			CombineAssertions("Tariff Description should be set", () =>
			{
				AssertEquals("IMP", "This is IMP!", importTariff.Description);
				AssertEquals("EXP", "This is EXP!", exportTariff.Description);
				AssertEquals("NULL", ZString.Empty, nonTariff.Description);
			});
		}

		public void TestSetTariffUQ1()
		{
			CombineAssertions("Tariff UQ1 should be set", () =>
			{
				AssertEquals("IMP", "002", importTariff.UQ1);
				AssertEquals("EXP", "001", exportTariff.UQ1);
				AssertEquals("NULL", ZString.Empty, nonTariff.UQ1);
			});
		}

		public void TestSetTariffUQ2()
		{
			CombineAssertions("Tariff UQ2 should be set", () =>
			{
				AssertEquals("IMP", "036", importTariff.UQ2);
				AssertEquals("EXP", "035", exportTariff.UQ2);
				AssertEquals("NULL", ZString.Empty, nonTariff.UQ2);
			});
		}

		public void TestSetTariffUQ3()
		{
			CombineAssertions("Tariff UQ3 should be set", () =>
			{
				AssertEquals("IMP", "112", importTariff.UQ3);
				AssertEquals("EXP", "111", exportTariff.UQ3);
				AssertEquals("NULL", ZString.Empty, nonTariff.UQ3);
			});
		}

		public void TestSetTariffUQ4()
		{
			CombineAssertions("Tariff UQ4 should be set", () =>
			{
				AssertEquals("IMP", "223", importTariff.UQ4);
				AssertEquals("EXP", ZString.Empty, exportTariff.UQ4);
				AssertEquals("NULL", ZString.Empty, nonTariff.UQ4);
			});
		}

		public void TestSetTariffUQ5()
		{
			CombineAssertions("Tariff UQ5 should be set", () =>
			{
				AssertEquals("IMP", ZString.Empty, importTariff.UQ5);
				AssertEquals("EXP", ZString.Empty, exportTariff.UQ5);
				AssertEquals("NULL", ZString.Empty, nonTariff.UQ5);
			});
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Germany, "EXP");
			var impTariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Germany, "IMP");
			Factory.Save();

			var expTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, expTariffType.PK, "0000000020", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "This is EXP!");
			helper.CreateTariffUOM(expTariff, "CU1", "001");
			helper.CreateTariffUOM(expTariff, "CU2", "035");
			helper.CreateTariffUOM(expTariff, "CU3", "111");
			helper.CreateTariffUOM(expTariff, "CU3", "222");

			var impTariff = helper.CreateTariff(Core.Constants.CountryCodes.Germany, impTariffType.PK, "0000000021", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "This is IMP!");
			helper.CreateTariffUOM(impTariff, "CU1", "002");
			helper.CreateTariffUOM(impTariff, "CU2", "036");
			helper.CreateTariffUOM(impTariff, "CU3", "112");
			helper.CreateTariffUOM(impTariff, "CU3", "223");
			Factory.Save();

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importInvoice = importDeclaration.Invoices.AddNew();
			var importInvoiceLine = importInvoice.InvoiceLines.AddNew();
			importInvoiceLine.JI_Tariff = "0000000021";

			importTariff = new TariffWrapper(importInvoiceLine);

			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var exportInvoice = exportDeclaration.Invoices.AddNew();
			var exportInvoiceLine = exportInvoice.InvoiceLines.AddNew();
			exportInvoiceLine.JI_Tariff = "0000000020";

			exportTariff = new TariffWrapper(exportInvoiceLine);

			var nonTariffDeclaration = Factory.New<JobDeclaration>();
			nonTariffDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var nonTariffInvoice = nonTariffDeclaration.Invoices.AddNew();
			var nonTariffInvoiceLine = nonTariffInvoice.InvoiceLines.AddNew();
			nonTariffInvoiceLine.JI_Tariff = "0000000022";

			nonTariff = new TariffWrapper(nonTariffInvoiceLine);
		}

		ITariff importTariff;
		ITariff exportTariff;
		ITariff nonTariff;
	}
}
