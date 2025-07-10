using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(CusEntryLineDocumentWrapper))]
	class CusEntryLineDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor_EntryLineIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusEntryLineDocumentWrapper(null));
		}

		public void TestEntryLine()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			AssertEquals(entryLine, wrapper.EntryLine);
		}

		public void TestEntryLineNo()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.CL_LineNumber = 1;
			AssertEquals((ZShort)1, wrapper.EntryLineNo);
		}

		public void TestFormattedTariff()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.CL_AdValoremTariff = "1001.00.01";
			AssertEquals("1001.00.01", wrapper.FormattedTariff);
		}

		public void TestGoodsDescription()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_Description = "Goods Desc";
			AssertEquals("Goods Desc", wrapper.GoodsDescription);
		}

		public void TestItemPrice()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.CL_CustomsValue = 36.123m;
			AssertEquals(36.12m, wrapper.ItemPrice);
		}

		public void TestCountryOfOrigin()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertEquals(Core.Constants.CountryCodes.Australia, wrapper.CountryOfOrigin);
		}

		public void TestPrimaryPreference()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_PrimaryPreference = "PRE";
			AssertEquals("PRE", wrapper.PrimaryPreference);
		}

		public void TestProcedure()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_Procedure = "P1";
			AssertEquals("P1", wrapper.Procedure);
		}

		public void TestQuota()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_ConcessionOrder = "Ord1";
			AssertEquals("Ord1", wrapper.Quota);
		}

		public void TestPreviousEntryNumber()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_PreviousEntryNumber = "Entry1";
			AssertEquals("Entry1", wrapper.PreviousEntryNumber);
		}

		public void TestPreviousEntryLineNumber()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_PreviousEntryLineNumber = 1;
			AssertEquals((ZShort)1, wrapper.PreviousEntryLineNumber);
		}

		public void TestCustomsQuantity()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_CustomsQuantity = 50.11111m;
			entryLine.InvoiceLines[1].JI_CustomsQuantity = 50.11111m;
			entryLine.InvoiceLines[2].JI_CustomsQuantity = 50.11111m;
			AssertEquals(150.33333m, wrapper.CustomsQuantity);
		}

		public void TestCustomsUnitQty()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_CustomsUnitQty = "UNI";
			AssertEquals("UNI", wrapper.CustomsUnitQty);
		}

		public void TestCustomsSecondQuantity()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_CustomsSecondQuantity = 50.11111m;
			entryLine.InvoiceLines[1].JI_CustomsSecondQuantity = 50.11111m;
			entryLine.InvoiceLines[2].JI_CustomsSecondQuantity = 50.11111m;
			AssertEquals(150.33333m, wrapper.CustomsSecondQuantity);
		}

		public void TestCustomsSecondQuantityUnitQty()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_CustomsSecondUnitQty = "UNI";
			AssertEquals("UNI", wrapper.CustomsSecondUnitQty);
		}

		public void TestGrossWeightInKG()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_Weight = 10.123m;
			entryLine.InvoiceLines[0].JI_WeightUQ = "KG";
			entryLine.InvoiceLines[1].JI_Weight = 10m;
			entryLine.InvoiceLines[1].JI_WeightUQ = "";
			entryLine.InvoiceLines[2].JI_Weight = 10m;
			entryLine.InvoiceLines[2].JI_WeightUQ = "T"; // equals 10000KG
			AssertEquals(10010.123m, wrapper.GrossWeightInKG);
		}

		public void TestLinePrice()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			entryLine.InvoiceLines[0].JI_LinePrice = 10.123m;
			entryLine.InvoiceLines[1].JI_LinePrice = 10m;
			entryLine.InvoiceLines[2].JI_LinePrice = 10m;
			AssertEquals(30.12m, wrapper.LinePrice);
		}

		public void TestLinePriceCurrency()
		{
			var entryLine = CreateEntryLine();
			var invoiceLine = entryLine.InvoiceLines[0];
			var invoice = invoiceLine.InvoiceHeader;
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			invoice.JZ_RX_NKInvoice_Currency = "XXX";
			AssertEquals("", invoiceLine.JI_RX_NKLinePriceCurr);
			AssertEquals("", wrapper.LinePriceCurrency);
			invoice.JZ_RX_NKInvoice_Currency = "XAF";
			AssertEquals("XAF", invoiceLine.JI_RX_NKLinePriceCurr);
			AssertEquals("XAF", wrapper.LinePriceCurrency);
			invoice.Delete();
			AssertEquals("", invoiceLine.JI_RX_NKLinePriceCurr);
			AssertEquals("", wrapper.LinePriceCurrency);
		}

		public void TestFreight()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
			{
				var entryLine = CreateEntryLine();
				var declaration = entryLine.Declaration;
				var invoiceLine = entryLine.InvoiceLines[0];
				invoiceLine.JI_LinePrice = 10000m;
				var invoice = invoiceLine.InvoiceHeader;
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, declaration.LocalCurrencyCode);
				var invoiceLine2 = entryLine.InvoiceLines[1];
				invoiceLine2.JI_LinePrice = 10000m;
				var invoice2 = invoiceLine2.InvoiceHeader;
				invoice2.JZ_InvoiceAmount = 10000m;
				invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice2.JZ_IncoTerm = "FOB";
				invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 200m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("Overseas Freight", 100m, invoiceLine.JI_Calc_FreightInInvoiceCurr);
				AssertEquals("Overseas Freight", 200m, invoiceLine2.JI_Calc_FreightInInvoiceCurr);
				var wrapper = new CusEntryLineDocumentWrapper(entryLine);
				AssertEquals(300.00m, wrapper.Freight);
			}
		}

		public void TestInsurance()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
			{
				var entryLine = CreateEntryLine();
				var declaration = entryLine.Declaration;
				var invoiceLine = entryLine.InvoiceLines[0];
				invoiceLine.JI_LinePrice = 10000m;
				var invoice = invoiceLine.InvoiceHeader;
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "FOB";
				invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 10m, declaration.LocalCurrencyCode);
				var invoiceLine2 = entryLine.InvoiceLines[1];
				invoiceLine2.JI_LinePrice = 10000m;
				var invoice2 = invoiceLine2.InvoiceHeader;
				invoice2.JZ_InvoiceAmount = 10000m;
				invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice2.JZ_IncoTerm = "FOB";
				invoice2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 20m, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("Overseas Insurance", 10m, invoiceLine.JI_Calc_InsuranceInInvoiceCurr);
				AssertEquals("Overseas Insurance", 20m, invoiceLine2.JI_Calc_InsuranceInInvoiceCurr);
				var wrapper = new CusEntryLineDocumentWrapper(entryLine);
				AssertEquals(30.00m, wrapper.Insurance);
			}
		}

		public void TestVATGSTBaseValue()
		{
			var entryLine = CreateEntryLine();
			var declaration = entryLine.Declaration;
			var invoiceLine = entryLine.InvoiceLines[0];
			invoiceLine.JI_LinePrice = 100m;
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "CIF";
			declaration.ResumeApportionment();
			AssertEquals("Line CIF", 100m, invoiceLine.JI_Calc_CIF);
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			AssertEquals(100.00m, wrapper.VATGSTBaseValue);
		}

		public void TestTotalDuty()
		{
			var entryLine = CreateEntryLine();
			var declaration = entryLine.Declaration;
			var invoiceLine = entryLine.InvoiceLines[0];
			invoiceLine.JI_LinePrice = 1000m;
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
			var invoiceLine2 = entryLine.InvoiceLines[1];
			invoiceLine2.JI_LinePrice = 3000m;
			var invoice2 = invoiceLine2.InvoiceHeader;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = "CIF";
			entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 100.444m;
			declaration.ResumeApportionment();
			AssertEquals("Duty apportioned Amount for invoice line", 25.111m, invoiceLine.JI_Calc_DutyAmountIncludingWHEstimate);
			AssertEquals("Duty apportioned Amount for invoice line2", 75.333m, invoiceLine2.JI_Calc_DutyAmountIncludingWHEstimate);
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			AssertEquals(100.44m, wrapper.TotalDuty);
		}

		public void TestTotalVATGST()
		{
			var entryLine = CreateEntryLine();
			var declaration = entryLine.Declaration;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoiceLine = entryLine.InvoiceLines[0];
			invoiceLine.JI_LinePrice = 100m;
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_IncoTerm = "FOB";
			var invoiceLine2 = entryLine.InvoiceLines[1];
			invoiceLine2.JI_LinePrice = 100m;
			var invoice2 = invoiceLine2.InvoiceHeader;
			invoice2.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice2.JZ_IncoTerm = "FOB";
			entryLine.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 400.444m);
			AssertEquals("invoiceLine.GSTVATAmount", 200.222m, invoiceLine.JI_Calc_GSTVATAmountIncludingWHEstimate);
			AssertEquals("invoiceLine.GSTVATAmount", 200.222m, invoiceLine2.JI_Calc_GSTVATAmountIncludingWHEstimate);
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			AssertEquals(400.44m, wrapper.TotalVATGST);
		}

		public void TestLocalCurrency()
		{
			var entryLine = CreateEntryLine();
			var declaration = entryLine.Declaration;
			var invoiceLine = entryLine.InvoiceLines[0];
			var invoice = invoiceLine.InvoiceHeader;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals(declaration.LocalCurrencyCode, invoiceLine.LocalCurrency.RX_Code);
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			AssertEquals(declaration.LocalCurrencyCode, wrapper.LocalCurrency);
		}

		public void TestDutyAndFeesAmount()
		{
			var entryLine = CreateEntryLine();
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeAmount = 5.123m;
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeAmount = 4m;
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			AssertEquals(9.12m, wrapper.DutyAndFeesAmount);
		}

		public void TestDutyAndFeesCollection()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = "VAT";
			fee1.CF_ChargeAmount = 5m;
			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "DTY";
			fee2.CF_ChargeAmount = 4m;
			CombineAssertions(() =>
			{
				AssertEquals(2, wrapper.DutyAndFeesCollection.Count);
				var wrapperFee = wrapper.DutyAndFeesCollection[0];
				AssertEquals(fee1.CF_ChargeType, wrapperFee.Type);
				AssertEquals(fee1.CF_ChargeAmount, wrapperFee.Amount);
				var wrapperFee2 = wrapper.DutyAndFeesCollection[1];
				AssertEquals(fee2.CF_ChargeType, wrapperFee2.Type);
				AssertEquals(fee2.CF_ChargeAmount, wrapperFee2.Amount);
			});
		}

		public void TestSupportingDocuments()
		{
			var entryLine = CreateEntryLine();
			var wrapper = new CusEntryLineDocumentWrapper(entryLine);
			var invoiceLine = (JobComInvoiceLine)entryLine.InvoiceLines[0];
			var document1 = invoiceLine.SupportingDocuments.AddNew();
			document1.CSI_Code = "INL";
			document1.CSI_ReferenceNumber = "111";
			document1.CSI_AdditionalDescription = "invoiceline 1 supporting doc";
			var document2 = invoiceLine.InvoiceHeader.SupportingDocuments.AddNew();
			document2.CSI_Code = "INV";
			document2.CSI_ReferenceNumber = "222";
			document2.CSI_AdditionalDescription = "invoice 1 supporting doc";
			CombineAssertions(() =>
			{
				AssertEquals(2, wrapper.SupportingDocuments.Count);
				var wapperDoc1 = wrapper.SupportingDocuments[0];
				AssertEquals(document1.CSI_Code, wapperDoc1.Code);
				AssertEquals(document1.CSI_ReferenceNumber, wapperDoc1.Reference);
				AssertEquals(document1.CSI_AdditionalDescription, wapperDoc1.Comments);
				var wapperDoc2 = wrapper.SupportingDocuments[1];
				AssertEquals(document2.CSI_Code, wapperDoc2.Code);
				AssertEquals(document2.CSI_ReferenceNumber, wapperDoc2.Reference);
				AssertEquals(document2.CSI_AdditionalDescription, wapperDoc2.Comments);
			});
		}

		CusEntryLine CreateEntryLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			var invoiceLine3 = invoice2.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine3.JI_CL = entryLine.PK;
			return entryLine;
		}

		protected override BusinessObject GetNewBusinessObject() => new CusEntryLineDocumentWrapper(CreateEntryLine());
	}
}
