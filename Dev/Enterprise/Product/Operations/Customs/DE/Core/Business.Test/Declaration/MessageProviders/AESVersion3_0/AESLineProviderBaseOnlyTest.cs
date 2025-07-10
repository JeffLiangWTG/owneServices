using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.AESVersion3_0.Testing
{
	sealed class AESLineProviderBaseOnlyTest : Customs.Business.Testing.DataProviderTestCase<AESLineProvider>
	{
		public void TestLineNumber()
		{
			AssertEquals(2, Provider.LineNumber);
		}

		public void TestStatisticalValue_IsZero()
		{
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_LinePrice = 0.00m;
			AssertEquals("Normalize", "0", Provider.StatisticalValue.ToString());
		}

		public void TestSupplementaryQuantity0()
		{
			AssertEquals("Expect 0", 0m, Provider.SupplementaryQuantity);
		}

		public void TestSupplementaryQuantity_OneLine()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.12345;
			AssertEquals("1 line", 1.123m, Provider.SupplementaryQuantity);
		}

		public void TestSupplementaryQuantity_TwoLines()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.12345;
			var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			secondInvoiceLine.JI_CustomsSecondQuantity = 0.87655m;
			secondInvoiceLine.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.ReloadFromLocalCache();
			AssertEquals("2 lines", 2m, Provider.SupplementaryQuantity);
		}

		public void TestSupplementaryQuantity_Round3()
		{
			invoiceLine.JI_CustomsSecondQuantity = 4.2514m;
			AssertEquals(4.251m, Provider.SupplementaryQuantity);
		}

		public void TestSupplementaryQuantity_Normalize()
		{
			invoiceLine.JI_CustomsSecondQuantity = 1.1000;
			AssertEquals("1.1", Provider.SupplementaryQuantity.ToString());
		}

		public void TestStatisticalValue()
		{
			invoice.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_LinePrice = 1.22m;
			var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			secondInvoiceLine.JI_CL = entryLine.PK;
			secondInvoiceLine.JI_LinePrice = 2.33m;
			AssertEquals("IsTransitionPeriod is false", "3.55", Provider.StatisticalValue.ToString());
		}

		public void TestStatisticalValue_IsTransitionPeriod_Style4thDigitIs4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000400;
				invoice.JZ_RX_NKInvoice_Currency = "EUR";
				invoiceLine.JI_LinePrice = 1.10m;
				var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
				secondInvoiceLine.JI_CL = entryLine.PK;
				secondInvoiceLine.JI_LinePrice = 2.20m;
				AssertEquals("Normalize", "3.3", Provider.StatisticalValue.ToString());
			}
		}

		public void TestStatisticalValue_IsTransitionPeriod_Style4thDigitIsNot4()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				invoice.JZ_RX_NKInvoice_Currency = "EUR";
				invoiceLine.JI_LinePrice = 1.10m;
				var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
				secondInvoiceLine.JI_CL = entryLine.PK;
				secondInvoiceLine.JI_LinePrice = 2.20m;
				AssertEquals("Normalize", "3", Provider.StatisticalValue.ToString());
			}
		}

		public void TestStatisticalValue_IsTransitionPeriod_Style4thDigitIsNot4_ValueUnder1()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
				invoice.JZ_RX_NKInvoice_Currency = "EUR";
				invoiceLine.JI_LinePrice = 0.1m;
				var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
				secondInvoiceLine.JI_CL = entryLine.PK;
				secondInvoiceLine.JI_LinePrice = 0.2m;
				AssertEquals("Normalize", "1", Provider.StatisticalValue.ToString());
			}
		}

		public void TestStatisticalValueSpecified()
		{
			CombineAssertions(() =>
			{
				InvoiceLineCharge invoiceLineCharge = invoiceLine.Charges.AddNew();
				invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
				invoiceLineCharge.J7_ChargeType = ZString.Empty;
				AssertEquals("No curency or charges", false, Provider.StatisticalValueSpecified);

				invoice.JZ_RX_NKInvoice_Currency = "EUR";
				AssertEquals("Only currency", true, Provider.StatisticalValueSpecified);

				invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
				invoiceLineCharge.J7_ChargeType = EU.Business.ChargeTypeList.Codes.StatisticalValue;
				AssertEquals("only charges", true, Provider.StatisticalValueSpecified);
			});
		}

		public void TestStatisticalValueSpecified_MultipleLines()
		{
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000100;
			invoice.JZ_RX_NKInvoice_Currency = ZString.Empty;
			var secondInvoiceLine = invoice.JobComInvoiceLines.AddNew();
			secondInvoiceLine.JI_CL = entryLine.PK;
			CombineAssertions(() =>
			{
				AssertEquals("No line meets the condition", false, Provider.StatisticalValueSpecified);

				InvoiceLineCharge invoiceLineCharge = secondInvoiceLine.Charges.AddNew();
				invoiceLineCharge.J7_ChargeType = EU.Business.ChargeTypeList.Codes.StatisticalValue;
				AssertEquals("One line meets the condition", true, Provider.StatisticalValueSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = Factory.New<CusEntryInstruction>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 2;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		protected override AESLineProvider GetProvider() => new AESLineProviderForTest(entryLine);

		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;

		class AESLineProviderForTest : AESLineProvider
		{
			public AESLineProviderForTest(CusEntryLine entryLine)
				: base(entryLine)
			{
			}
		}
	}
}
