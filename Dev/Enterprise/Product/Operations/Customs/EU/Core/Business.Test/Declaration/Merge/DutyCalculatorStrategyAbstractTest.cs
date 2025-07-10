using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(DutyCalculatorStrategy))]
	public abstract class DutyCalculatorStrategyAbstractTest<T> : Customs.Business.Testing.DutyCalculatorStrategyAbstractTest<T>
		where T : DutyCalculatorStrategy
	{
		protected override bool ExpectedShouldCalculateDuties => ((JobDeclaration)Declaration).Configuration.UseUniversalFeeCalculation(Declaration);

		public override void TestCalculateDuties()
		{
			Assert("EU countries test CalculateDuties by its own", true);
		}

		public virtual void TestGetFeeCodeFromRateCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			AssertEquals("ABCDEF", entryLine.GetFeeCodeFromRateCode("ABCDEF"));
		}

		public virtual void TestCalculateDutiesAndVat()
		{
			var dtyTariffCode = "222";
			var stdPreferenceCode = "STD";

			lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

			var declaration = GetLineMergerTestHelper().CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, VATableAdditionChargeCode, taxType: "ORD");

			DoMerge(declaration);

			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
			AssertEquals("Entry Header count", 1, entryHeaders.Count());

			var entryHeader = entryHeaders.Single();
			AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

			if (declaration.Configuration.UseUniversalFeeCalculation(declaration))
			{
				AssertDutiesAndVatWithUniversalFeeCalculation(entryHeader);
			}
			else
			{
				AssertDutiesAndVatWithoutUniversalFeeCalculation(entryHeader);
			}
		}

		public virtual void TestCalculateValueForVAT()
		{
			var declaration = GetDeclarationForTest();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 1000;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invLine1 = invoice.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction.PK;
			invLine1.JI_Tariff = "1";

			var usdCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			var rate = usdCurrency.ExchangeRates.AddNew();
			rate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
			rate.RE_StartDate = ZDateTime.Now.AddDays(-2);
			rate.RE_ExpiryDate = ZDateTime.Now.AddDays(2);
			rate.RE_SellRate = 2;
			rate.RE_GC = GlbCompany.CurrentCompany.PK;

			DoMerge(declaration);

			AssertEquals("Entries count", 1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Entry Lines count", 1, entry.MergedLines.Count);
			var entryLine = entry.MergedLines[0];
			AssertEquals("CL_ValueForVAT", 0m, entryLine.CL_ValueForVAT);

			invLine1.JI_LinePrice = 1000.34M;
			DoMerge(declaration);
			AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedLocalCurrencyValue, entryLine.CL_ValueForVAT);

			invoice.JZ_RX_NKInvoice_Currency = "USD";
			DoMerge(declaration);
			AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue, entryLine.CL_ValueForVAT);

			if (NonVATableDeductionChargeCode != EmptyChargeCode)
			{
				var deduction = invLine1.Charges.AddNew(NonVATableDeductionChargeCode, 20, declaration.LocalCurrencyCode);
				deduction.J7_IsGSTApplicable = false;
				DoMerge(declaration);
				AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue - 20, entryLine.CL_ValueForVAT);
			}

			if (VATableAdditionChargeCode != EmptyChargeCode)
			{
				invLine1.Charges.RemoveAll();
				var addition = invLine1.Charges.AddNew(VATableAdditionChargeCode, 50);
				addition.J7_IsGSTApplicable = true;
				DoMerge(declaration);
				AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue + 25M, entryLine.CL_ValueForVAT);

				invLine1.Charges.RemoveAll();
				var additionInvoiceHeader = invoice.Charges.AddNew(VATableAdditionChargeCode, 200);
				additionInvoiceHeader.J7_IsGSTApplicable = true;
				DoMerge(declaration);
				AssertEquals("CL_ValueForVAT", CalculateValueForVAT_ExpectedUSDCurrencyValue + 100M, entryLine.CL_ValueForVAT);

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Tariff = "2";
				invoiceLine2.JI_LinePrice = 1000;
				DoMerge(declaration);
				AssertEquals("Entries count", 1, declaration.CustomsEntryHeaders.Count);
				var entryHeader = declaration.CustomsEntryHeaders[0];
				AssertEquals("Entry Lines count", 2, entryHeader.MergedLines.Count);
				var entryLines = entryHeader.MergedLines.Cast<CusEntryLine>();
				AssertEquals("CL_ValueForVAT for Entry Line 1", CalculateValueForVAT_ExpectedEntryLine1Value, entryLines.SingleOrDefault(x => x.Tariff == "1").CL_ValueForVAT);
				AssertEquals("CL_ValueForVAT for Entry Line 2", CalculateValueForVAT_ExpectedEntryLine2Value, entryLines.SingleOrDefault(x => x.Tariff == "2").CL_ValueForVAT);
			}
		}

		protected virtual ZDecimal CalculateValueForVAT_ExpectedEntryLine1Value => 550.18M;
		protected virtual ZDecimal CalculateValueForVAT_ExpectedEntryLine2Value => 549.99M;
		protected virtual ZDecimal CalculateValueForVAT_ExpectedLocalCurrencyValue => 1000.34M;
		protected ZDecimal CalculateValueForVAT_ExpectedUSDCurrencyValue => CalculateValueForVAT_ExpectedLocalCurrencyValue / 2;

		protected virtual JobDeclaration GetDeclarationForTest() => Factory.New<JobDeclaration>();

		void AssertDutiesAndVatWithUniversalFeeCalculation(CusEntryHeader entryHeader)
		{
			var entryLineA = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.ProcedureCode == "A");
			LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLineA, ExpectedVATChargeAmountA, ExpectedVATBaseValueA, "%", 22m, ExpectedChargeAmountDecimalPlaces);
			AssertEntryLineFees(entryLineA, EntryLineAExpectedFees);

			var entryLineB = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.ProcedureCode == "B");
			LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLineB, ExpectedVATChargeAmountB, ExpectedVATBaseValueB, "%", 22m, ExpectedChargeAmountDecimalPlaces);
			AssertEntryLineFees(entryLineB, EntryLineBExpectedFees);
		}

		void AssertDutiesAndVatWithoutUniversalFeeCalculation(CusEntryHeader entryHeader)
		{
			var entryLineA = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.ProcedureCode == "A");
			AssertEquals("No Fees expected", 0, entryLineA.Fees.Count);

			var entryLineB = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.ProcedureCode == "B");
			AssertEquals("No Fees expected", 0, entryLineB.Fees.Count);
		}

		protected void AssertEntryLineFees(CusEntryLine entryLine, IReadOnlyList<FeeAssertionObject> expectedFees) => LineMergerTestHelper.AssertEntryLineFees(entryLine, expectedFees, ExpectedChargeAmountDecimalPlaces);

		protected virtual void DoMerge(JobDeclaration declaration) => new LineMerger(declaration).DoMerge();

		protected virtual ZString VATableAdditionChargeCode => Common.CustomsChargeTypeList.Codes.AdditionCharge;
		protected virtual ZString NonVATableDeductionChargeCode => Common.CustomsChargeTypeList.Codes.DeductionCharge;
		protected ZString EmptyChargeCode => ZString.Empty;
		protected virtual ZInt ExpectedChargeAmountDecimalPlaces => 4;

		protected virtual ZDecimal ExpectedVATChargeAmountA => 79.97m;
		protected virtual ZDecimal ExpectedVATBaseValueA => 363.50m;
		protected virtual ZDecimal ExpectedVATChargeAmountB => 19.987m;
		protected virtual ZDecimal ExpectedVATBaseValueB => 90.85m;

		protected virtual IReadOnlyList<FeeAssertionObject> EntryLineAExpectedFees => new FeeAssertionObject[]
			{
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 14m, BaseValue = 70m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 60m, BaseValue = 500m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 150m, BaseValue = 500m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 3.5m, BaseValue = 7m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 21m, BaseValue = 70m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 30m, BaseValue = 500m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = ExpectedVATChargeAmountA, BaseValue = ExpectedVATBaseValueA, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

		protected virtual IReadOnlyList<FeeAssertionObject> EntryLineBExpectedFees => new FeeAssertionObject[]
			{
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 3.6m, BaseValue = 18m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 15.6m, BaseValue = 130m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 39m, BaseValue = 130m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 0.8m, BaseValue = 4m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 0.65m, BaseValue = 1.3m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 5.4m, BaseValue = 18m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 7.8m, BaseValue = 130m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
					new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = ExpectedVATChargeAmountB, BaseValue = ExpectedVATBaseValueB, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
			};

		protected LineMergerTestHelper lineMergerTestHelper
		{
			get
			{
				if (_lineMergerTestHelper == null)
				{
					_lineMergerTestHelper = GetLineMergerTestHelper();
					_lineMergerTestHelper.CreateDataGrouping();
				}

				return _lineMergerTestHelper;
			}
		}

		protected virtual LineMergerTestHelper GetLineMergerTestHelper() => new LineMergerTestHelper(Factory);

		LineMergerTestHelper _lineMergerTestHelper;
	}
}
