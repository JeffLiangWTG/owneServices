using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class AFRMMEntryFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateAFRMMSameNetWeight_ISW()
		{
			AssertCalculateAFRMMSameNetWeight(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestCalculateAFRMMSameNetWeight_IMP()
		{
			AssertCalculateAFRMMSameNetWeight(BRJobMessageTypeList.Codes.Import);
		}

		void AssertCalculateAFRMMSameNetWeight(ZString messageType)
		{
			var refCurrecny = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			refCurrecny.SetCustomsRate(new ZDateTime(2000, 1, 1), ZDateTime.MaxSmallDateTimeValue, 0.2m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = messageType;

			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;

			var groupHeader = declaration.AllGroupHeaders.FirstOrDefault() as JobComInvoiceGroupHeader;

			var freightPrepaid = groupHeader.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 40m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var freightCollect = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollect.J7_Amount = 20m;
			freightCollect.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var freightComponents = invoice.Charges.AddNew(ImportChargesProvider.FreightComponents.Code);
			freightComponents.J7_Amount = 100m;
			freightComponents.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 100m;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 100m;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 600m;
			invoiceLine3.JI_NetWeight = 100m;
			invoiceLine3.JI_CEI = entryInstruction.PK;

			declaration.ResumeApportionment();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine1.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine3.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			new AFRMMEntryFeeCalculator(cusEntryHeader).UpdateFeeOnEntryLines();

			var entryLineFee1 = cusEntryHeader.MergedLines[0].Fees.Cast<CusEntryLineFee>();
			var entryLineFee2 = cusEntryHeader.MergedLines[1].Fees.Cast<CusEntryLineFee>();
			var entryLineFee3 = cusEntryHeader.MergedLines[2].Fees.Cast<CusEntryLineFee>();
			CombineAssertions(() =>
			{
				AssertEquals("First Line Total fee", 11.34m, cusEntryHeader.MergedLines[0].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Second Line Total fee", 11.33m, cusEntryHeader.MergedLines[1].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Third Line Total fee", 11.33m, cusEntryHeader.MergedLines[2].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Total FMM Fee should be equal", 34m, cusEntryHeader.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax)));
			});
		}

		public void TestCalculateAFRMMDifferentNetWeight_ISW()
		{
			AssertCalculateAFRMMDifferentNetWeight(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestCalculateAFRMMDifferentNetWeight_IMP()
		{
			AssertCalculateAFRMMDifferentNetWeight(BRJobMessageTypeList.Codes.Import);
		}

		void AssertCalculateAFRMMDifferentNetWeight(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = messageType;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var freightPrepaid = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 30m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightCollect = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollect.J7_Amount = 20m;
			freightCollect.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightComponents = invoice.Charges.AddNew(ImportChargesProvider.FreightComponents.Code);
			freightComponents.J7_Amount = 10m;
			freightComponents.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 30m;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 600m;
			invoiceLine3.JI_NetWeight = 20m;
			invoiceLine3.JI_CEI = entryInstruction.PK;

			declaration.ResumeApportionment();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine1.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine3.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			new AFRMMEntryFeeCalculator(cusEntryHeader).UpdateFeeOnEntryLines();

			CombineAssertions(() =>
			{
				AssertEquals("First Line Total fee", 8m, cusEntryHeader.MergedLines[0].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Second Line Total fee", 4.8m, cusEntryHeader.MergedLines[1].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Third Line Total fee", 3.2m, cusEntryHeader.MergedLines[2].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Total FMM Fee should be equal", 16m, cusEntryHeader.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax)));
			});

			cusEntryHeader.MergedLines.RemoveAndDeleteAll();
			invoiceLine1.JI_CL = invoiceLine2.JI_CL = invoiceLine3.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			new AFRMMEntryFeeCalculator(cusEntryHeader).UpdateFeeOnEntryLines();

			CombineAssertions(() =>
			{
				AssertEquals("MergedLines Total fee", 16m, cusEntryHeader.MergedLines[0].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Total FMM Fee should be equal", 16m, cusEntryHeader.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax)));
			});

			cusEntryHeader.MergedLines.RemoveAndDeleteAll();
			invoiceLine1.JI_CL = invoiceLine2.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine3.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			declaration.ResumeApportionment();
			new AFRMMEntryFeeCalculator(cusEntryHeader).UpdateFeeOnEntryLines();

			CombineAssertions(() =>
			{
				AssertEquals("First MergedLines Total fee", 12.8m, cusEntryHeader.MergedLines[0].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Second MergedLines Total fee", 3.2m, cusEntryHeader.MergedLines[1].Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax));
				AssertEquals("Total FMM Fee should be equal", 16m, cusEntryHeader.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax)));
			});
		}

		public void TestCalculateAFRMMWithAllLinesBeingExemption_ISW()
		{
			AssertCalculateAFRMMWithAllLinesBeingExemption(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestCalculateAFRMMWithAllLinesBeingExemption_IMP()
		{
			AssertCalculateAFRMMWithAllLinesBeingExemption(BRJobMessageTypeList.Codes.Import);
		}

		void AssertCalculateAFRMMWithAllLinesBeingExemption(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = messageType;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 1800m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var freightPrepaid = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 30m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightCollect = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollect.J7_Amount = 20m;
			freightCollect.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightComponents = invoice.Charges.AddNew(ImportChargesProvider.FreightComponents.Code);
			freightComponents.J7_Amount = 10m;
			freightComponents.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 30m;
			invoiceLine2.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "1";
			invoiceLine3.JI_LinePrice = 600m;
			invoiceLine3.JI_NetWeight = 20m;
			invoiceLine3.FMMBenefit = FMMBenefitTypeList.Codes.Exemption;
			invoiceLine3.JI_CEI = entryInstruction.PK;

			declaration.ResumeApportionment();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine1.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine3.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			new AFRMMEntryFeeCalculator(cusEntryHeader).UpdateFeeOnEntryLines();

			AssertEquals("Total FMM Fee should be equal", ZDecimal.Zero, cusEntryHeader.MergedLines.Sum(t => t.Fees.GetAmount(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax)));
		}

		public void TestEntryLineFeeProperties_ISW()
		{
			AssertEntryLineFeeProperties(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestEntryLineFeeProperties_IMP()
		{
			AssertEntryLineFeeProperties(BRJobMessageTypeList.Codes.Import);
		}

		void AssertEntryLineFeeProperties(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = messageType;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var freightPrepaid = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 30m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightCollect = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollect.J7_Amount = 20m;
			freightCollect.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightComponents = invoice.Charges.AddNew(ImportChargesProvider.FreightComponents.Code);
			freightComponents.J7_Amount = 10m;
			freightComponents.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.JI_LinePrice = 600m;
			invoiceLine1.JI_NetWeight = 50m;
			invoiceLine1.JI_CEI = entryInstruction.PK;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "1";
			invoiceLine2.JI_LinePrice = 600m;
			invoiceLine2.JI_NetWeight = 100m;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			declaration.ResumeApportionment();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine1.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;
			invoiceLine2.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			new AFRMMEntryFeeCalculator(cusEntryHeader).UpdateFeeOnEntryLines();

			AssertEquals(2, cusEntryHeader.MergedLines.Count);

			var fee1 = cusEntryHeader.MergedLines.FirstOrDefault().Fees.Where(x => x.CF_ChargeType == Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax).FirstOrDefault();
			var fee2 = cusEntryHeader.MergedLines.LastOrDefault().Fees.Where(x => x.CF_ChargeType == Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax).FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("fee1.CF_ChargeAmount should be equal to 5.33", 5.33m, fee1.CF_ChargeAmount);
				AssertEquals("fee1.CF_BaseValue should be equal to 70", 70m, fee1.CF_BaseValue);
				AssertEquals("fee1.CF_Rate should be equal to 10", 10m, fee1.CF_Rate);
				AssertEquals("fee1.CF_MethodOfCalculation should be equal to %", Constants.MethodOfCalculation.Percentage, fee1.CF_MethodOfCalculation);

				AssertEquals("fee2.CF_ChargeAmount should be equal to 10.67", 10.67m, fee2.CF_ChargeAmount);
				AssertEquals("fee2.CF_BaseValue should be equal to 70", 70m, fee2.CF_BaseValue);
				AssertEquals("fee2.CF_Rate should be equal to 10", 10m, fee2.CF_Rate);
				AssertEquals("fee2.CF_MethodOfCalculation should be equal to %", Constants.MethodOfCalculation.Percentage, fee2.CF_MethodOfCalculation);
			});
		}

		public void TestCalculateTotalFee_ISW()
		{
			AssertCalculateTotalFee(BRJobMessageTypeList.Codes.ImportSiscomex);
		}

		public void TestCalculateTotalFee_IMP()
		{
			AssertCalculateTotalFee(BRJobMessageTypeList.Codes.Import);
		}

		void AssertCalculateTotalFee(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_MessageType = messageType;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Description = "TEST_ENTRY";
			entryInstruction.IsAFRMMRateOverridden = true;
			entryInstruction.CEI_AFRMMRateOverride = 10m;
			entryInstruction.CEI_UtilizationFeeOverride = 10m;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "TEST1";
			invoice.JZ_InvoiceAmount = 600m;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_NoOfPacks = 55;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var freightPrepaid = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code);
			freightPrepaid.J7_Amount = 30m;
			freightPrepaid.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightCollect = invoice.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code);
			freightCollect.J7_Amount = 20m;
			freightCollect.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
			var freightComponents = invoice.Charges.AddNew(ImportChargesProvider.FreightComponents.Code);
			freightComponents.J7_Amount = 10m;
			freightComponents.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1";
			invoiceLine.JI_LinePrice = 600m;
			invoiceLine.JI_NetWeight = 100m;
			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.ResumeApportionment();

			var cusEntryHeader = declaration.ActiveEntryHeaders.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			invoiceLine.JI_CL = cusEntryHeader.MergedLines.AddNew().PK;

			var calculator = new AFRMMEntryFeeCalculator(cusEntryHeader);
			AssertEquals("Total FMM ", 16m, calculator.CalculateTotalFee());
		}
	}
}
