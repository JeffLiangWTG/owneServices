using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class SiscomexUsageFeeApportionManagerTest : TestCaseWithFactory
	{
		public void TestApportion_ZeroLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 400m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.FirstOrDefault();

			var apportionManager = new SiscomexUsageFeeApportionManager();
			AssertExceptionThrown(typeof(System.ArgumentNullException), () => apportionManager.Apportion(cusEntryHeader, 500.57m));
		}

		public void TestApportion_OneLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 400m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 400m;
			invoiceLine1.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			NewCusEntryLineForInvoiceLines(cusEntryHeader, invoice.InvoiceLines.Cast<JobComInvoiceLine>());

			var apportionManager = new SiscomexUsageFeeApportionManager();
			var apportionResult = apportionManager.Apportion(cusEntryHeader, 500.57m);

			CombineAssertions(() =>
			{
				AssertEquals("First apportion value should be", 500.57m, apportionResult.ElementAt(0).Item2);
				AssertEquals("Total SUF Fee should be equal", 500.57m, apportionResult.Sum(t => t.Item2));
			});
		}

		public void TestApportion_TwoLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 400m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			NewCusEntryLineForInvoiceLines(cusEntryHeader, invoice.InvoiceLines.Cast<JobComInvoiceLine>());

			var apportionManager = new SiscomexUsageFeeApportionManager();
			var apportionResult = apportionManager.Apportion(cusEntryHeader, 500.57m);

			CombineAssertions(() =>
			{
				AssertEquals("First apportion value should be", 200.23m, apportionResult.ElementAt(0).Item2);
				AssertEquals("Second apportion value should be", 300.34m, apportionResult.ElementAt(1).Item2);
				AssertEquals("Total SUF Fee should be equal", 500.57m, apportionResult.Sum(t => t.Item2));
			});
		}

		public void TestApportion_ThreeLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1600m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 400m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 600m;
			invoiceLine3.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			NewCusEntryLineForInvoiceLines(cusEntryHeader, invoice.InvoiceLines.Cast<JobComInvoiceLine>());

			var apportionManager = new SiscomexUsageFeeApportionManager();
			var apportionResult = apportionManager.Apportion(cusEntryHeader, 500.57m);

			CombineAssertions(() =>
			{
				AssertEquals("First apportion value should be", 187.72m, apportionResult.ElementAt(0).Item2);
				AssertEquals("Second apportion value should be", 125.14m, apportionResult.ElementAt(1).Item2);
				AssertEquals("Third apportion value should be", 187.71m, apportionResult.ElementAt(2).Item2);
				AssertEquals("Total SUF Fee should be equal", 500.57m, apportionResult.Sum(t => t.Item2));
			});
		}

		public void TestApportion_FiveLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 2200m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 400m;
			invoiceLine3.JI_NetWeight = 100m;

			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1";
			invoiceLine4.JI_LinePrice = 400m;
			invoiceLine4.JI_NetWeight = 100m;

			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "1";
			invoiceLine5.JI_LinePrice = 300m;
			invoiceLine5.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			NewCusEntryLineForInvoiceLines(cusEntryHeader, invoice.InvoiceLines.Cast<JobComInvoiceLine>());

			var apportionManager = new SiscomexUsageFeeApportionManager();
			var apportionResult = apportionManager.Apportion(cusEntryHeader, 500.57m);

			CombineAssertions(() =>
			{
				AssertEquals("First apportion value should be", 136.52m, apportionResult.ElementAt(0).Item2);
				AssertEquals("Second apportion value should be", 113.77m, apportionResult.ElementAt(1).Item2);
				AssertEquals("Third apportion value should be", 91.01m, apportionResult.ElementAt(2).Item2);
				AssertEquals("Fourth apportion value should be", 91.01m, apportionResult.ElementAt(3).Item2);
				AssertEquals("Fifth apportion value should be", 68.26m, apportionResult.ElementAt(4).Item2);
				AssertEquals("Total SUF Fee should be equal", 500.57m, apportionResult.Sum(t => t.Item2));
			});
		}

		public void TestApportion_EightLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 2800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 500m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 400m;
			invoiceLine3.JI_NetWeight = 100m;

			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1";
			invoiceLine4.JI_LinePrice = 400m;
			invoiceLine4.JI_NetWeight = 100m;

			var invoiceLine5 = invoice.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "1";
			invoiceLine5.JI_LinePrice = 300m;
			invoiceLine5.JI_NetWeight = 100m;

			var invoiceLine6 = invoice.InvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "1";
			invoiceLine6.JI_LinePrice = 200m;
			invoiceLine6.JI_NetWeight = 100m;

			var invoiceLine7 = invoice.InvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "1";
			invoiceLine7.JI_LinePrice = 300m;
			invoiceLine7.JI_NetWeight = 100m;

			var invoiceLine8 = invoice.InvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "1";
			invoiceLine8.JI_LinePrice = 100m;
			invoiceLine8.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			NewCusEntryLineForInvoiceLines(cusEntryHeader, invoice.InvoiceLines.Cast<JobComInvoiceLine>());

			var apportionManager = new SiscomexUsageFeeApportionManager();
			var apportionResult = apportionManager.Apportion(cusEntryHeader, 500.57m);

			CombineAssertions(() =>
			{
				AssertEquals("First apportion value should be", 107.26m, apportionResult.ElementAt(0).Item2);
				AssertEquals("Second apportion value should be", 89.39m, apportionResult.ElementAt(1).Item2);
				AssertEquals("Third apportion value should be", 71.51m, apportionResult.ElementAt(2).Item2);
				AssertEquals("Fourth apportion value should be", 71.51m, apportionResult.ElementAt(3).Item2);
				AssertEquals("Fifth apportion value should be", 53.63m, apportionResult.ElementAt(4).Item2);
				AssertEquals("Sixth apportion value should be", 35.76m, apportionResult.ElementAt(5).Item2);
				AssertEquals("Seventh apportion value should be", 53.63m, apportionResult.ElementAt(6).Item2);
				AssertEquals("Eighth apportion value should be", 17.88m, apportionResult.ElementAt(7).Item2);
				AssertEquals("Total SUF Fee should be equal", 500.57m, apportionResult.Sum(t => t.Item2));
			});
		}

		public void TestApportion_LineCustomsValueEqualZero()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 2800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;

			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_NetWeight = 100m;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 0m;
			invoiceLine2.JI_NetWeight = 100m;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 0m;
			invoiceLine3.JI_NetWeight = 100m;

			var invoiceLine4 = invoice.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "1";
			invoiceLine4.JI_LinePrice = 0m;
			invoiceLine4.JI_NetWeight = 100m;

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			NewCusEntryLineForInvoiceLines(cusEntryHeader, invoice.InvoiceLines.Cast<JobComInvoiceLine>());

			var apportionManager = new SiscomexUsageFeeApportionManager();
			var apportionResult = apportionManager.Apportion(cusEntryHeader, 500.57m);

			CombineAssertions(() =>
			{
				AssertEquals("First apportion value should be", 500.57m, apportionResult.ElementAt(0).Item2);
				AssertEquals("Second apportion value should be", 0m, apportionResult.ElementAt(1).Item2);
				AssertEquals("Third apportion value should be", 0m, apportionResult.ElementAt(2).Item2);
				AssertEquals("Fourth apportion value should be", 0m, apportionResult.ElementAt(3).Item2);
				AssertEquals("Total SUF Fee should be equal", 500.57m, apportionResult.Sum(t => t.Item2));
			});
		}

		void NewCusEntryLineForInvoiceLines(CusEntryHeader cusEntryHeader, IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			foreach (var invoiceLine in invoiceLines)
			{
				invoiceLine.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
				invoiceLine.CusEntryLine.CL_CustomsValue = invoiceLine.JI_LinePrice;
			}
		}
	}
}
