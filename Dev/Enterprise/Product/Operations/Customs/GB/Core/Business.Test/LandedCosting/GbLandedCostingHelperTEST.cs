using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Business.LandedCosting.Testing
{
	class GbLandedCostingHelperTEST : TestCaseWithFactory
	{
		public void TestHeaderDuties()
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var line1 = entryHeader.AllEntryLines.AddNew();
			var line2 = entryHeader.AllEntryLines.AddNew();
			line1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 100m;  // A00
			line1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts).CF_ChargeAmount = 200m; // A10
			line1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge).CF_ChargeAmount = 300m; // A20
			line1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty).CF_ChargeAmount = 400m; // A30
			line1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty).CF_ChargeAmount = 500m; // A40
			line2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 1m;  // A00
			line2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts).CF_ChargeAmount = 2m; // A10
			line2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge).CF_ChargeAmount = 3m; // A20
			line2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty).CF_ChargeAmount = 4m; // A30
			line2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty).CF_ChargeAmount = 5m; // A40

			var headerDuties = helper.GetTotalDutyTaxEntryFeeItems(declaration);
			AssertEquals(101m, headerDuties["TDT"]);
			AssertEquals(202m, headerDuties["OTH"]);
			AssertEquals(303m, headerDuties["ST1"]);
			AssertEquals(404m, headerDuties["ST2"]);
			AssertEquals(505m, headerDuties["ST3"]);
		}

		public void TestLineDuties()
		{
			var inv = declaration.Invoices.AddNew();
			inv.JZ_RX_NKInvoice_Currency = "GBP";
			var invLine = inv.InvoiceLines.AddNew();
			var taxA00 = invLine.Taxes.AddNew();
			var taxA10 = invLine.Taxes.AddNew();
			var taxA20 = invLine.Taxes.AddNew();
			var taxA30 = invLine.Taxes.AddNew();
			var taxA40 = invLine.Taxes.AddNew();
			taxA00.Data.G4_Amount = "100";
			taxA10.Data.G4_Amount = "200";
			taxA20.Data.G4_Amount = "300";
			taxA30.Data.G4_Amount = "400";
			taxA40.Data.G4_Amount = "500";
			taxA00.Data.G4_Type = "A00";
			taxA10.Data.G4_Type = "A10";
			taxA20.Data.G4_Type = "A20";
			taxA30.Data.G4_Type = "A30";
			taxA40.Data.G4_Type = "A40";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			_ = declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("Pre req", 1, entryHeader.MergedLines.Count);
			AssertEquals("Pre req", 1, entryHeader.MergedLines[0].InvoiceLines.Count);
			AssertEquals("Pre req", 5, entryHeader.MergedLines[0].Taxes.Count);

			var itemDuties = helper.GetLineDutyTaxEntryFeeItems(invLine);
			AssertEquals(100m, itemDuties["TDT"]);
			AssertEquals(200m, itemDuties["OTH"]);
			AssertEquals(300m, itemDuties["ST1"]);
			AssertEquals(400m, itemDuties["ST2"]);
			AssertEquals(500m, itemDuties["ST3"]);

			inv.JZ_RX_NKInvoice_Currency = "USD";
			AssertEquals("Pre-req, rates GBP vs USD is 1.39", 1.39m, inv.CurrencyConverter.GetExchangeRate(inv.Invoice_Currency));
			itemDuties = helper.GetLineDutyTaxEntryFeeItems(invLine);
			AssertEquals("Landed costing still reports in declaration currency, not invoice currency", 100m, itemDuties["TDT"]);
		}

		public void TestLineConfirmedFeesDuties()
		{
			CreateRefData();
			SetupJobDeclarationForFeesDutiesTest();
			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("Pre req", 1, entryHeader.MergedLines.Count);
			AssertEquals("Pre req", 2, entryLine.InvoiceLines.Count);

			AddConfirmedFees(entryLine);
			AssertEquals("EntryLine DutyAmount", 738m, entryLine.DutyAmount);
			AssertDutyTaxEntryFee(invoiceLine1, 246m, 111m, 50m);
			AssertDutyTaxEntryFee(invoiceLine2, 492m, 222m, 100m);

			entryLine.ConfirmedFees.DeleteAll();
			Factory.Save();
			AddConfirmedFees(entryLine, isNI: true);
			AssertEquals("EntryLine DutyAmount", 738m, entryLine.DutyAmount);
			AssertDutyTaxEntryFee(invoiceLine1, 246m, 111m, 50m);
			AssertDutyTaxEntryFee(invoiceLine2, 492m, 222m, 100m);
		}

		public void TestLineCalculatedFeesDuties()
		{
			CreateRefData();
			SetupJobDeclarationForFeesDutiesTest();
			var invoiceLine1 = declaration.InvoiceLines[0];
			var invoiceLine2 = declaration.InvoiceLines[1];
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("Pre req", 1, entryHeader.MergedLines.Count);
			AssertEquals("Pre req", 2, entryLine.InvoiceLines.Count);

			AddCalculatedFees(entryLine);
			AssertEquals("EntryLine DutyAmount", 738m, entryLine.DutyAmount);
			AssertDutyTaxEntryFee(invoiceLine1, 246m, 111m, 50m);
			AssertDutyTaxEntryFee(invoiceLine2, 492m, 222m, 100m);

			entryLine.Fees.Clear();
			AddCalculatedFees(entryLine, isNI: true);
			AssertEquals("EntryLine DutyAmount", 738m, entryLine.DutyAmount);
			AssertDutyTaxEntryFee(invoiceLine1, 246m, 111m, 50m);
			AssertDutyTaxEntryFee(invoiceLine2, 492m, 222m, 100m);
		}

		void CreateRefData()
		{
			var refHelper = new EU.NCTS.Business.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			var rateType = refHelper.CreateNewOrGetExistingRateType(dataGroupingCode: Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, rateType: Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, description: "Customs duties on industrial products");
			_ = refHelper.CreateNewOrGetExistingDataGrouping(Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, string.Empty, parent: dataGrouping);
			_ = refHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, rateType.PK);
			_ = refHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, rateType.PK);
			_ = refHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, rateType.PK);
			_ = refHelper.LoadOrCreateNewCusRateCode(Factory, GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty, rateType.PK);
			_ = refHelper.LoadOrCreateNewCusRateCode(Factory, GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty, rateType.PK);
			_ = refHelper.LoadOrCreateNewCusRateCode(Factory, GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty, rateType.PK);
			Factory.Save();
		}

		void SetupJobDeclarationForFeesDutiesTest()
		{
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedKingdom;
			invoice.JZ_InvoiceAmount = 3000m;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1;
			invoiceLine1.JI_LinePrice = 1000m;

			var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 1;
			invoiceLine2.JI_LinePrice = 2000m;

			_ = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		void AddConfirmedFees(CusEntryLine line, bool isNI = false)
		{
			_ = line.ConfirmedFees.AddOrUpdate(isNI ? GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty : UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 255m);
			_ = line.ConfirmedFees.AddOrUpdate(isNI ? GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty : UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 333m);
			_ = line.ConfirmedFees.AddOrUpdate(isNI ? GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty : UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, 150m);
			_ = line.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 5000m);
		}

		void AddCalculatedFees(CusEntryLine line, bool isNI = false)
		{
			_ = line.Fees.AddOrUpdate(isNI ? GBCommonConstants.NorthernIrelandDutyCodes.CustomsDuty : UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 255m);
			_ = line.Fees.AddOrUpdate(isNI ? GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveAntiDumpingDuty : UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 333m);
			_ = line.Fees.AddOrUpdate(isNI ? GBCommonConstants.NorthernIrelandDutyCodes.DefinitiveCountervailingDuty : UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, 150m);
		}

		void AssertDutyTaxEntryFee(JobComInvoiceLine invoiceLine, decimal totalDuty, decimal specialTax2, decimal specialTax3)
		{
			var itemDuties = helper.GetLineDutyTaxEntryFeeItems(invoiceLine);
			CombineAssertions(() =>
			{
				AssertEquals(CustomsDisbursementChargeCode.TotalDuty, totalDuty, itemDuties[CustomsDisbursementChargeCode.TotalDuty].Round(4));
				AssertEquals(CustomsDisbursementChargeCode.SpecialTax2, specialTax2, itemDuties[CustomsDisbursementChargeCode.SpecialTax2].Round(4));
				AssertEquals(CustomsDisbursementChargeCode.SpecialTax3, specialTax3, itemDuties[CustomsDisbursementChargeCode.SpecialTax3].Round(4));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			helper = new GbLandedCostingHelper();
		}

		JobDeclaration declaration;
		GbLandedCostingHelper helper;
	}
}
