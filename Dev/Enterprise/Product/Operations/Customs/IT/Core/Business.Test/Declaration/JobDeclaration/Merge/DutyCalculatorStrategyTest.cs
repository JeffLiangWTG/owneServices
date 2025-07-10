using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(DutyCalculatorStrategy))]
sealed class DutyCalculatorStrategyTest : DutyCalculatorStrategyAbstractTest<DutyCalculatorStrategy>
{
	protected override DutyCalculatorStrategy GetDutyCalculatorStrategy() => new DutyCalculatorStrategy((JobDeclaration)Declaration);

	public void TestCalculateDutiesAndVatWithSpecialRounding()
	{
		var dtyTariffCode = "222";
		var stdPreferenceCode = "STD";

		lineMergerTestHelper.PopulateTestReferenceDataAndSave(dtyTariffCode, stdPreferenceCode);

		var declaration = lineMergerTestHelper.CreateTestDeclarationAndInvoiceLines(dtyTariffCode, stdPreferenceCode, VATableAdditionChargeCode, taxType: "ORD");
		var orderedInvoiceLines = declaration.InvoiceLines.Cast<JobComInvoiceLine>().OrderBy(x => x.InvoiceHeader.JZ_InvoiceNumber);

		AssertEquals("[PRE-CONDITION] Invoice Lines count", orderedInvoiceLines.Count(), 4);

		var invoiceLine1 = orderedInvoiceLines.ElementAt(0);
		invoiceLine1.JI_CustomsQuantity = 0.004;
		invoiceLine1.JI_LinePrice = 0.005;
		invoiceLine1.JI_CustomsSecondQuantity = 0.3;

		var invoiceLine2 = orderedInvoiceLines.ElementAt(1);
		invoiceLine2.JI_CustomsQuantity = 0.0005;
		invoiceLine2.JI_LinePrice = 0.0008;
		invoiceLine2.JI_CustomsSecondQuantity = 0.2;

		var invoiceLine3 = orderedInvoiceLines.ElementAt(2);
		invoiceLine3.JI_CustomsQuantity = 0.0025;
		invoiceLine3.JI_LinePrice = 0.0008;
		invoiceLine3.JI_CustomsSecondQuantity = 0.03;

		var invoiceLine4 = orderedInvoiceLines.ElementAt(3);
		invoiceLine4.JI_CustomsQuantity = 0.0005;
		invoiceLine4.JI_LinePrice = 0.0008;
		invoiceLine4.JI_CustomsSecondQuantity = 0.001;

		new LineMerger(declaration).DoMerge();

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());

		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 2, entryHeader.MergedLines.Count);

		var entryLineA = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.ProcedureCode == "A");
		LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLineA, 3.36m, 15.28m, "%", 22m);

		var entryLineAExpectedFees = new FeeAssertionObject[]
		{
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 0.01m, BaseValue = 0.01m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 15m, BaseValue = 3m, Rate = 5m, MethodOfCalculation = "HLT", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 0.06m, BaseValue = 0.5m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 0.15m, BaseValue = 0.5m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 0.01m, BaseValue = 0.0045m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 0.01m, BaseValue = 0.01m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 0.03m, BaseValue = 0.5m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = 3.36m, BaseValue = 15.28m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
		};
		AssertEntryLineFees(entryLineA, entryLineAExpectedFees);

		var entryLineB = entryHeader.MergedLines.Cast<CusEntryLine>().Single(x => x.ProcedureCode == "B");
		LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLineB, 3.31m, 15.05m, "%", 22m);

		var entryLineBExpectedFees = new FeeAssertionObject[]
		{
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 2m, BaseValue = 10m, Rate = 20m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A00RateCode, ChargeAmount = 0.01m, BaseValue = 0.003m, Rate = 0.12m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 0.01m, BaseValue = 0.003m, Rate = 0.3m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A20RateCode, ChargeAmount = 0.01m, BaseValue = 0.031m, Rate = 0.2m, MethodOfCalculation = "LTR", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A30RateCode, ChargeAmount = 0.01m, BaseValue = 0.00003m, Rate = 0.5m, MethodOfCalculation = "DTN", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 3m, BaseValue = 10m, Rate = 30m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.A40RateCode, ChargeAmount = 0.01m, BaseValue = 0.003m, Rate = 0.06m, MethodOfCalculation = "KGM", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = LineMergerTestHelper.RateCode.VatRateCode, ChargeAmount = 3.31m, BaseValue = 15.05m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" },
		};
		AssertEntryLineFees(entryLineB, entryLineBExpectedFees);
	}

	public void TestPortTaxesCalculation()
	{
		SetUpHarbourRates();

		ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TransportMode = "SEA";
		declaration.JE_MessageType = "IMP";
		declaration.JE_RL_NKPortOfArrival = "ITVCE";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Tariff = "80";
		invoiceLine.ZG_PortTaxRate = "A3";
		invoiceLine.JI_Weight = 10;
		invoiceLine.JI_WeightUQ = "T";

		declaration.DoMerge();

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());

		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);

		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();
		var expectedPortTaxFee = new FeeAssertionObject() { ChargeType = "9AA", ChargeAmount = 5m, BaseValue = 10m, Rate = 0.5m, MethodOfCalculation = "TNE", OverrideReason = "" };
		LineMergerTestHelper.AssertEntryLineFees(entryLine, [expectedPortTaxFee], ExpectedChargeAmountDecimalPlaces);

		declaration.JE_RL_NKPortOfArrival = "USLAX";
		invoiceLine.ZG_PortTaxRate = "A4";
		declaration.DoMerge();
		expectedPortTaxFee = new FeeAssertionObject() { ChargeType = "9AB", ChargeAmount = 1m, BaseValue = 10m, Rate = 0.1m, MethodOfCalculation = "TNE", OverrideReason = "" };
		LineMergerTestHelper.AssertEntryLineFees(entryLine, [expectedPortTaxFee], ExpectedChargeAmountDecimalPlaces);

		invoiceLine.ZG_PortTaxRate = "";
		declaration.DoMerge();
		AssertEquals("No Fees expected", 0, entryLine.Fees.Count);
	}

	public void TestVatCalculationForItaly()
	{
		SetUpAndSaveRefDataForVatCalculation();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		var invLine = invoice.InvoiceLines.AddNew();
		invLine.JI_ZZF_NKTaxType = "ORD";

		declaration.DoMerge();

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());
		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLine, 0m, 0m, "%", 22m);

		// Add VATable and non-VATable entry line fees and merge again
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "900", "%", 1m, 1000m, 10m, "ADD", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "300", "%", 5m, 1000m, 50m, "ADD", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "100", "%", 7m, 1000m, 70m, "ADD", false);
		declaration.DoMerge();

		LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLine, 17.6m, 80m, "%", 22m);
	}

	public void TestVatExemptionCalculation()
	{
		SetUpAndSaveRefDataForVatCalculation();
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		declaration.DoMerge();
		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());
		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLine, 0m, 0m, "%", 22m);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "900", "%", 1m, 1000m, 10m, "ADD", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "300", "%", 5m, 1000m, 50m, "ADD", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "100", "%", 7m, 1000m, 70m, "ADD", false);

		Assert("VAT Exemption is not required", !entryLine.RequiresVATExemption);
		declaration.DoMerge();
		LineMergerTestHelper.AssertSystemVatEntryLineFee(entryLine, 17.6m, 80m, "%", 22m);
		var vatExemptionFee = GetFee(UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406);
		AssertNull("VAT Exemption is not calculated", vatExemptionFee);

		invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		Assert("VAT Exemption is required", entryLine.RequiresVATExemption);
		declaration.DoMerge();
		var vatFee = GetFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
		vatExemptionFee = GetFee(UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406);
		AssertNotNull("VAT Exemption is calculated (adjusted rate)", vatExemptionFee);
		CombineAssertions(() =>
		{
			AssertEquals("BaseValue", -80m, vatExemptionFee.CF_BaseValue);
			AssertEquals("Amount", -17.6m, vatExemptionFee.CF_ChargeAmount);
			AssertEquals("Type", UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406, vatExemptionFee.CF_ChargeType);
			AssertEquals("MethodOfCalculation", "%", vatExemptionFee.CF_MethodOfCalculation);
			AssertEquals("VAT Exemption MethodOfPayment", "A", vatExemptionFee.CF_MethodOfPayment);
			AssertEquals("VAT and VAT Exemption have same MethodOfPayment", vatFee.CF_MethodOfPayment, vatExemptionFee.CF_MethodOfPayment);
			AssertEquals("Rate", 22m, vatExemptionFee.CF_Rate);
		});

		vatFee = GetFee(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat);
		vatFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
		declaration.DoMerge();
		vatExemptionFee = GetFee(UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406);
		AssertNotNull("VAT Exemption is calculated", vatExemptionFee);
		CombineAssertions(() =>
		{
			AssertEquals("BaseValue", -80m, vatExemptionFee.CF_BaseValue);
			AssertEquals("Amount", -17.6m, vatExemptionFee.CF_ChargeAmount);
			AssertEquals("Type", UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406, vatExemptionFee.CF_ChargeType);
			AssertEquals("MethodOfCalculation", "%", vatExemptionFee.CF_MethodOfCalculation);
			AssertEquals("VAT Exemption MethodOfPayment", "A", vatExemptionFee.CF_MethodOfPayment);
			AssertEquals("VAT and VAT Exemption have same MethodOfPayment", vatFee.CF_MethodOfPayment, vatExemptionFee.CF_MethodOfPayment);
			AssertEquals("Rate", 22m, vatExemptionFee.CF_Rate);
		});

		Customs.Business.CusEntryLineFee GetFee(ZString feeType)
		{
			return entryLine.Fees.GetElementWithThisCode(feeType);
		}
	}

	public void TestCalculateNationalDuties()
	{
		SetupRatesAndTariff();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Tariff = "808088";
		invoiceLine.JI_LinePrice = 1000m;

		declaration.DoMerge();

		var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		var expectedFees = new FeeAssertionObject[]
		{
				new FeeAssertionObject() { ChargeType = "000", ChargeAmount = 80m, BaseValue = 1000m, Rate = 8m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "100", ChargeAmount = 80m, BaseValue = 1000m, Rate = 8m, MethodOfCalculation = "%", OverrideReason = "" },
				new FeeAssertionObject() { ChargeType = "900", ChargeAmount = 80m, BaseValue = 1000m, Rate = 8m, MethodOfCalculation = "%", OverrideReason = "" },
		};

		AssertEntryLineFees(entryLine, expectedFees);
	}

	public void TestPortTaxesAreIncludedInVat()
	{
		SetUpAndSaveRefDataForVatCalculation();
		SetUpHarbourRates();

		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryValue: true))
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKPortOfArrival = "ITVCE";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Tariff = "80";
			invoiceLine.ZG_PortTaxRate = "A3";
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = "T";
			invoiceLine.JI_ZZF_NKTaxType = "ORD";

			declaration.DoMerge();

			var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
			AssertEquals("Entry Header count", 1, entryHeaders.Count());

			var entryHeader = entryHeaders.Single();
			AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);

			var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();
			var expectedPortTaxFee = new FeeAssertionObject() { ChargeType = "9AA", ChargeAmount = 5m, BaseValue = 10m, Rate = 0.5m, MethodOfCalculation = "TNE", OverrideReason = "" };
			var expectedVatFee = new FeeAssertionObject() { ChargeType = "B00", ChargeAmount = 1.1m, BaseValue = 5m, Rate = 22m, MethodOfCalculation = "%", OverrideReason = "" };
			LineMergerTestHelper.AssertEntryLineFees(entryLine, [expectedPortTaxFee, expectedVatFee], ExpectedChargeAmountDecimalPlaces);
		}
	}

	public void TestCalculateDuties_NotExecutedForEntryInstructionWithAllowDutiesAndFeeCalculationAsFalse()
	{
		var dtyTariffCode = "222";

		DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTariffType("EXP")
			.AddTariff(dtyTariffCode)
			.AddRateCode(RateTypeEnum.Duty, "A00", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Antidumping, "A35", "0.5 * [VFD]", preference: "")
			.AddRateCode(RateTypeEnum.CounterVailing, "A45", "0.5 * [VFD]", preference: "")
			.Configure();

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = "";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		var invLine1 = invoice1.InvoiceLines.AddNew();
		invLine1.JI_CEI = entryInstruction.PK;
		invLine1.JI_Procedure = "A";
		invLine1.JI_Tariff = dtyTariffCode;
		invLine1.JI_CustomsQuantity = 5;
		invLine1.JI_CustomsUnitQty = "DTN";
		invLine1.JI_CustomsSecondQuantity = 300;
		invLine1.JI_CustomsSecondUnitQty = "KGM";
		invLine1.JI_CustomsThirdQuantity = 0.1;
		invLine1.JI_CustomsThirdUnitQty = "KLT";
		invLine1.JI_SupplementaryCode1 = "7000";
		invLine1.JI_LinePrice = 50;

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			SetCeiStyleAndAssertFeesCount(ExportUCC6DeclarationTypeList.Codes.DichiarazioneRiesportazioneB1, declaration, entryInstruction, 4);
			SetCeiStyleAndAssertFeesCount(ExportUCC6DeclarationTypeList.Codes.DichiarazionePerTerritoriFiscaliSpecialiB4, declaration, entryInstruction, 0);
			SetCeiStyleAndAssertFeesCount(ExportUCC6DeclarationTypeList.Codes.DichiarazioneSemplificataC1, declaration, entryInstruction, 0);
			SetCeiStyleAndAssertFeesCount(ExportUCC6DeclarationTypeList.Codes.RegimeSpecialeDichiarazioneB2, declaration, entryInstruction, 4);
		}

		void SetCeiStyleAndAssertFeesCount(string ceiStyle, JobDeclaration jobDeclaration, CusEntryInstruction cusEntryInstruction,
			int expectedFeesCount)
		{
			cusEntryInstruction.CEI_Style = ceiStyle;
			jobDeclaration.DoMerge();
			AssertEquals("Entry Headers", 1, jobDeclaration.CustomsEntryHeaders.Count);
			var entryHeader = jobDeclaration.CustomsEntryHeaders[0];
			AssertEquals("Entry Lines", 1, entryHeader.MergedLines.Count);
			var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();
			var fees = entryLine.Fees.OfType<CusEntryLineFee>();
			AssertEquals($"Fees Count for CEI_Style={ceiStyle}", expectedFeesCount, fees.Count());
		}
	}

	public void TestCalculateEntryLineFees_ExcludesFeesWithActionEXC()
	{
		SetupRatesAndTariff();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "808088";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		declaration.DoMerge();
		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());
		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);

		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "900", "%", 1m, 1000m, 10m, "ADD", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "000", "%", 5m, 0m, 50m, "EXC", false);
		VatCalculationTestHelper.CreateEntryLineFee(entryLine, "100", "%", 7m, 1000m, 70m, "OVR", false);

		declaration.DoMerge();

		var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>().ToArray();
		var feeWithActionEXCCount = entryLineFees.Count(f => f.CF_ChargeType == "000");
		var feeWithActionOVRCount = entryLineFees.Count(f => f.CF_ChargeType == "100");
		CombineAssertions(() =>
		{
			AssertEquals("Entry Line fee with Code=000 is skipped", 1, feeWithActionEXCCount);
			AssertEquals("Entry Line fee with Code=100 is skipped", 1, feeWithActionOVRCount);
		});
	}

	public void TestCalculateEntryLineFees_ExcludesExtraFeesWithActionEXC()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateTaxOrFee("ORD", 0.2m, "IT", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_TransportMode = "SEA";

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		invoiceLine.JI_Tariff = "808088";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		declaration.DoMerge();
		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());
		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);

		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		ExcludeFeeOfType("406", entryLine);

		declaration.DoMerge();

		var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>().ToArray();
		var extraFees = entryLineFees.Count(f => f.CF_ChargeType == "406");

		AssertEquals("fee 406 must not be duplicated after merge being marked as EXC", 1, extraFees);
	}

	public void TestCalculateEntryLineFees_ExcludesVATFeesWithActionEXC()
	{
		SetupRatesAndTariff();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";

		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.ZG_PortTaxRate = "A1";
		invoiceLine.JI_Tariff = "808088";
		invoiceLine.JI_ZZF_NKTaxType = "ORD";

		declaration.DoMerge();

		var entryHeaders = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>();
		AssertEquals("Entry Header count", 1, entryHeaders.Count());
		var entryHeader = entryHeaders.Single();
		AssertEquals("Entry Line count", 1, entryHeader.MergedLines.Count);

		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		ExcludeFeeOfType("B00", entryLine);

		declaration.DoMerge();

		var entryLineFees = entryLine.Fees.Cast<CusEntryLineFee>().ToArray();
		var vatFees = entryLineFees.Count(f => f.CF_ChargeType == "B00");

		AssertEquals("fee B00 must not be duplicated after merge being marked as EXC", 1, vatFees);
	}

	public void TestSetUserEditableValues_WhenActionIsBlank()
	{
		var testDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure4500 = testDataHelper.CreateRefCusProcedure("IT", "IM", "45", "00", "", "Procedure 4500", "IMP");
		testDataHelper.CreateRefCusProcedureAttribute(procedure4500.PK, "DTYPaymentMethod", "D");

		DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddTariffType("IMP")
			.AddTariff("222")
			.AddRateCode(RateTypeEnum.Duty, "A00", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "A20", "0.3 * [KGM]", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "270", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.AddRateCode(RateTypeEnum.Duty, "275", "0.12 * [KGM] + 0.2 * VFD", preference: "")
			.Configure();

		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = "BLT";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "45";
		var invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = "EUR";
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_Procedure = "4500";
		invoiceLine.JI_Tariff = "222";
		invoiceLine.JI_CustomsQuantity = 5;
		invoiceLine.JI_CustomsUnitQty = "DTN";
		invoiceLine.JI_CustomsSecondQuantity = 300;
		invoiceLine.JI_CustomsSecondUnitQty = "KGM";
		invoiceLine.JI_CustomsThirdQuantity = 0.1;
		invoiceLine.JI_CustomsThirdUnitQty = "KLT";
		invoiceLine.JI_SupplementaryCode1 = "7000";
		invoiceLine.JI_LinePrice = 50;

		declaration.DoMerge();

		AssertEquals("[PRE-CONDITION] Entry Headers", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("[PRE-CONDITION] Entry Lines", 1, entryHeader.MergedLines.Count);
		var entryLine = entryHeader.MergedLines.Cast<CusEntryLine>().Single();

		var addFee = entryLine.Fees.AddNew();
		addFee.CF_RateOverrideReasonCode = "ADD";
		addFee.CF_MethodOfPayment = "X";

		var fees = entryLine.Fees.OfType<CusEntryLineFee>();
		AssertEquals("[PRE-CONDITION] Entry Line fees", 8, fees.Count());

		AssertEquals("Fee Method of Payments", "D, D, D, D, D, D, D, X", GetActualMopAsString());

		invoiceLine.JI_Procedure = "4522";
		declaration.DoMerge();
		AssertEquals("Fee Method of Payments", "A, A, A, A, A, A, A, X", GetActualMopAsString());

		string GetActualMopAsString()
		{
			return string.Join(", ", fees.Select(x => x.CF_MethodOfPayment).OrderBy(x => x)).Trim();
		}
	}

	void SetUpAndSaveRefDataForVatCalculation()
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(Factory);

		refDataHelper.CreateNewOrGetExistingDataGrouping("IT");

		var levRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "LEV", description: "Levies");
		var intRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "INT", description: "Interest");
		var mieRateType = refDataHelper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MIE", description: "Miscellaneous Import Export");

		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "100", levRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "900", levRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "300", intRateType.PK);
		refDataHelper.CreateCusRateCode(Factory, zy1RateCode: "9AA", mieRateType.PK);

		var configurationBuilder = DutyReferenceDataConfigurationBuilder.New(Factory)
			.AddPreferences("STD")
			.AddTaxOrFee("ORD", taxRate: 0.22m);
		configurationBuilder.Configure();
	}

	void SetUpHarbourRates()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();
	}

	void SetupRatesAndTariff()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var rateFormula = "VFD * 0.080";

		var euDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping("IT", parent: euDataGrouping);
		helper.CreateNewOrGetExistingDataGrouping("FR", parent: euDataGrouping);

		var dtyRateType = helper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "DTY", description: "Duty");
		var levRateType = helper.CreateCusRateType(dataGroupingCode: "IT", rateType: "LEV", description: "Levies");
		var mieRateType = helper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MIE", description: "Miscellaneous Import Export");
		var intRateType = helper.CreateCusRateType(dataGroupingCode: "FR", rateType: "MIE", description: "Interest");

		var rate000 = helper.CreateCusRateCode(Factory, zy1RateCode: "000", dtyRateType.PK);
		var rate100 = helper.CreateCusRateCode(Factory, zy1RateCode: "100", levRateType.PK);
		var rate900 = helper.CreateCusRateCode(Factory, zy1RateCode: "900", mieRateType.PK);
		var rate300 = helper.CreateCusRateCode(Factory, zy1RateCode: "300", intRateType.PK);

		var tariffType = helper.CreateNewOrGetExistingTariffType("IT", "IMP");
		var tariff = helper.LoadOrCreateNewTariff("IT", tariffType.PK, "808088", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		var rateView000 = helper.CreateRate(tariff, rate000.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula,
			dataGrouping: "EUN");
		var rateView100 = helper.CreateRate(tariff, rate100.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula,
			dataGrouping: "IT");
		var rateView900 = helper.CreateRate(tariff, rate900.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula,
			dataGrouping: "IT");
		var rateView300 = helper.CreateRate(tariff, rate300.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula,
			dataGrouping: "FR");
		var tradeGroup = helper.CreateTradeGroup("IT", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "ERGA OMNES");

		helper.CreateCusApplicability(rateView000, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusApplicability(rateView100, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusApplicability(rateView900, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusApplicability(rateView300, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		helper.CreateTaxOrFee("ORD", 0.2m, "IT", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));

		Factory.Save();
	}

	void ExcludeFeeOfType(string type, CusEntryLine entryLine)
	{
		var vatFee = entryLine.Fees.Where(f => f.CF_ChargeType == type).Single();
		vatFee.CF_RateOverrideReasonCode = "EXC";
	}

	protected override ZString VATableAdditionChargeCode => UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge;

	protected override ZString NonVATableDeductionChargeCode => ZString.Empty;

	protected override ZInt ExpectedChargeAmountDecimalPlaces => 2;
}
