using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineTest : EU.Business.Declaration.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		public void TestTypeOfHeader()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.InvoiceLines.AddNew();

			DoMerge(declaration);
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];

			AssertType<CusEntryHeader>(entryLine.Header);
		}

		public void TestCL_StatisticalValueOnMerge()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine = invoice.InvoiceLines.AddNew();

			DoMerge(declaration);
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];

			var lineAndStatisticalValues = new KeyValuePair<ZDecimal, ZDecimal>[]
			{
					new KeyValuePair<ZDecimal, ZDecimal>(0m, 1m),
					new KeyValuePair<ZDecimal, ZDecimal>(0.01m, 1m),
					new KeyValuePair<ZDecimal, ZDecimal>(0.49m, 1m),
					new KeyValuePair<ZDecimal, ZDecimal>(0.73m, 1m),
					new KeyValuePair<ZDecimal, ZDecimal>(10.49m, 10m),
					new KeyValuePair<ZDecimal, ZDecimal>(10.50m, 11m),
					new KeyValuePair<ZDecimal, ZDecimal>(10.51m, 11m)
			};

			CombineAssertions(() =>
			{
				foreach (var lineAndStatisticalValue in lineAndStatisticalValues)
				{
					var linePrice = lineAndStatisticalValue.Key;
					var statisticalValue = lineAndStatisticalValue.Value;
					invoiceLine.JI_LinePrice = linePrice;
					DoMerge(declaration);
					AssertEquals($"{linePrice} rounds to statistical {statisticalValue}", statisticalValue, entryLine.CL_StatisticalValue);
				}
			});
		}

		public void TestCL_ValueForVATOnMerge()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 87.49m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 34.67m;

			DoMerge(declaration);
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
			AssertEquals("VAT equals total lines price", 122.16m, entryLine.CL_ValueForVAT);
		}

		public void TestEffectiveGrossWeightApplicable()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Weight = 87.49m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Weight = 34.67m;
			invoiceLine1.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			DoMerge(declaration);
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];

			AssertEquals(122.16m, entryLine.EffectiveGrossWeight.Amount);
		}

		public void TestZG_CustomsStatus()
		{
			var declaration = (JobDeclaration)ImportJobDeclaration;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			DoMerge(declaration);
			var entryLine = ((CusEntryHeader)declaration.ActiveEntryHeaders[0]).AllEntryLines[0];
			entryLine.CL_AddInfo = "CustomsStatus=RL1";

			AssertEquals("RL1", entryLine.ZG_CustomsStatus);
		}

		public void TestCustomsStatusDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "RL1", "RL1 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = (JobDeclaration)ImportJobDeclaration;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.AllEntryLines.AddNew();

			CombineAssertions(() =>
			{
				entryLine.ZG_CustomsStatus = "RL1";
				AssertEquals("ZG_CustomsStatus is valid", "RL1 DESC", entryLine.CustomsStatusDescription);
				entryLine.ZG_CustomsStatus = "XX";
				AssertEquals("ZG_CustomsStatus is invalid", ZString.Empty, entryLine.CustomsStatusDescription);
			});
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = "MDD";
			var from = new ZDateTime(2005, 6, 1);
			var to = new ZDateTime(2005, 6, 5);
			newCurrency.SetCustomsRate(from, to, RatesAreReciprocal ? 2m : 0.5m);
			var declaration = SetUpDeclarationAndInvLinesForMoneyTest(newCurrency);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			// Do not assert FOB.  FOB in base is wrong. 
			AssertEquals("CIF", 660.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 60.0m, entryLine.TAndIInLocalCurrency.Amount);
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrencyWhenAllChargesAreInLocalCurrency()
		{
			var declaration = SetUpDeclarationAndInvLinesForMoneyTest(GlbCompany.CurrentCompany.LocalCurrency);
			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			// Do not assert FOB.  FOB in base is wrong. 
			AssertEquals("CIF", 330.0m, entryLine.CIFInLocalCurrency.Amount);
			AssertEquals("OFT", 30.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
			AssertEquals("ONS", 0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
			AssertEquals("T&I", 30.0m, entryLine.TAndIInLocalCurrency.Amount);
		}

		public void TestAtLeastOneInvoiceLineHasPackingDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader1 = declaration.Invoices.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoiceHeader1.InvoiceLines.AddNew();
			var invoiceLine2_1 = invoiceHeader2.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "10";
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1_1.JI_CL = entryLine.PK;
			invoiceLine2_1.JI_CL = entryLine.PK;

			CombineAssertions(() =>
			{
				AssertEquals("No linked InvoiceLine has PackingDetails", false, entryLine.AtLeastOneInvoiceLineHasPackingDetails);

				declaration.Packages.AddNew();
				var baseCusLinkPackage = invoiceLine2_1.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First();
				baseCusLinkPackage.IsLinked = true;
				AssertEquals("At Least 1 linked InvoiceLine has PackingDetails", true, entryLine.AtLeastOneInvoiceLineHasPackingDetails);
			});
		}

		public void TestConfirmedFeesReadOnlyType()
		{
			AssertType<CusEntryLineConfirmedFeeWrapperCollection>(Factory.New<CusEntryLine>().ConfirmedFeesReadOnly);
		}

		public void TestEntryLineVatCalculatorType()
		{
			AssertType<EntryLineVatCalculator>(Factory.New<CusEntryLine>().GetEntryLineVatCalculator());
		}

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = base.ImportJobDeclaration;
				result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				result.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
				return result;
			}
		}

		protected override string OverseasFreightCode => ImportChargeCodeList.Codes._011;

		protected override string StatisticalValueApplicableCharge => ChargeCodeList.Codes.AdditionCharge;

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			foreach (var invoiceLine in declaration.InvoiceLines.Cast<EU.Business.Declaration.JobComInvoiceLine>())
			{
				invoiceLine.JI_CEI = entryInstruction.PK;
			}
			base.DoMerge(declaration);
		}

		protected override BaseJobDeclaration SetUpDeclarationAndInvLinesForMoneyTest(RefCurrency currency)
		{
			var declaration = SetUpDeclarationForMoneyTest();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = currency.RX_Code;

			var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "2203.10.10 10";
			line2.JI_Tariff = "2203.10.10 10";
			line1.JI_LinePrice = 100.0m;
			line2.JI_LinePrice = 200.0m;

			var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line1ONS.J7_Amount = 5.0m;
			line1ONS.J7_IsDutiable = true;
			var line1OFT = line1.Charges.AddNew(OverseasFreightCode);
			line1OFT.J7_Amount = 10.0m;
			line1OFT.J7_IsDutiable = true;
			var line2ONS = line2.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
			line2ONS.J7_Amount = 10.0m;
			line2ONS.J7_IsDutiable = true;
			var line2OFT = line2.Charges.AddNew(OverseasFreightCode);
			line2OFT.J7_IsDutiable = true;
			line2OFT.J7_Amount = 20.0m;

			AssertEquals("PreReq, line 1 CIF is 115", 110.0m, line1.JI_CIF.Amount);
			AssertEquals("PreReq, line 2 CIF is 230", 220.0m, line2.JI_CIF.Amount);
			AssertEquals("PreReq, line 1 CIF currency is invoice currency", currency.Code, line1.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 2 CIF currency is invoice currency", currency.Code, line2.JI_CIF.Currency.Code);
			AssertEquals("PreReq, line 1 CIF is 115", 110.0m, line1.JI_Calc_CIF);
			AssertEquals("PreReq, line 2 CIF is 230", 220.0m, line2.JI_Calc_CIF);
			DoMerge(declaration);
			return declaration;
		}

		protected override Type GetExpectedTaxBoxSupporterType() => typeof(CusEntryLineFee);

		protected override Type ExpectedTypeOfFees => typeof(EU.Business.Declaration.CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);
	}
}
