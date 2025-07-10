using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(CusEntryLine))]
	sealed class CusEntryLineTest : Customs.Business.Testing.CusEntryLineTest<CusEntryLine, JobComInvoiceLine>
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(entryLine,
				"KRCusEntryLine",
				schemaTypeName: nameof(AutoCusEntryLine.Schema));
		}

		[TestDate(2005, 6, 2)]
		public override void TestMoneyInLocalCurrency()
		{
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345"))
			{
				var newCurrency = RefCurrency.New(Factory);
				newCurrency.RX_Code = "MDD";
				newCurrency.SetCustomsRate(new ZDateTime(2005, 6, 1), new ZDateTime(2005, 6, 5), 2m).RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_RX_NKInvoice_Currency = newCurrency.RX_Code;

				var line1 = invoiceHeader.JobComInvoiceLines.AddNew();
				var line2 = invoiceHeader.JobComInvoiceLines.AddNew();
				line1.JI_CEI = instruction.PK;
				line2.JI_CEI = instruction.PK;
				line1.JI_Tariff = "2203.10.10 10";
				line2.JI_Tariff = "2203.10.10 10";
				line1.JI_LinePrice = 100.0m;
				line2.JI_LinePrice = 200.0m;

				var line1ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
				line1ONS.J7_Amount = 5.0m;
				var line1OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
				line1OFT.J7_Amount = 10.0m;
				var line2ONS = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance);
				line2ONS.J7_Amount = 10.0m;
				var line2OFT = line1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
				line2OFT.J7_Amount = 20.0m;

				DoMerge(declaration);

				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				CombineAssertions(() =>
				{
					AssertEquals("FOB", 600.0m, entryLine.FOBInLocalCurrency.Amount);
					AssertEquals("CIF", 690.0m, entryLine.CIFInLocalCurrency.Amount);
					AssertEquals("Overseas Freight", 60.0m, entryLine.OverseasFreightInLocalCurrency.Amount);
					AssertEquals("Overseas Insurance", 30.0m, entryLine.OverseasInsuranceInLocalCurrency.Amount);
					AssertEquals("T and I", 90.0m, entryLine.TAndIInLocalCurrency.Amount);
				});
			}
		}

		public void TestNetWeightInGrams()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine.PK;
			invoiceLine1_1.JI_SequenceNumber = 1;
			invoiceLine1_1.JI_NetWeight = 1m;
			invoiceLine1_1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine.PK;
			invoiceLine1_2.JI_SequenceNumber = 2;
			invoiceLine1_2.JI_NetWeight = 100m;
			invoiceLine1_2.JI_NetWeightUQ = Core.Constants.Weight.Grams;

			AssertEquals(1100m, entryLine.NetWeightInGrams);
		}

		public void TestFirstInvoiceLine()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine.PK;
			invoiceLine1_2.JI_SequenceNumber = 2;

			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine.PK;
			invoiceLine1_1.JI_SequenceNumber = 1;

			AssertEquals(invoiceLine1_1, entryLine.FirstInvoiceLine);
		}

		public void TestNetWeightInKG()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine.PK;
			invoiceLine1_1.JI_SequenceNumber = 1;
			invoiceLine1_1.JI_NetWeight = 1m;
			invoiceLine1_1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;

			var invoiceLine1_2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine.PK;
			invoiceLine1_2.JI_SequenceNumber = 2;
			invoiceLine1_2.JI_NetWeight = 1000m;
			invoiceLine1_2.JI_NetWeightUQ = Core.Constants.Weight.Grams;

			AssertEquals(2m, entryLine.NetWeightInKG);
		}

		public void TestCustomsValueUSD()
		{
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			entryLine.CL_CustomsValue = 100m;
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			RefCurrency uSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.UnitedStates);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRate, ZDateTime.Today, ZDateTime.Today, 0.8573m, uSD);
			SetExchangeRate(GlbCompany.CurrentCompany, Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary, ZDateTime.Today, ZDateTime.Today, 0.75m, uSD);

			AssertEquals(116m, entryLine.CustomsValueUSD);
		}
		void SetExchangeRate(GlbCompany company, ZString rateType, ZDateTime startDate, ZDateTime endDate, ZDecimal rate, RefCurrency foreignCurrency)
		{
			RefExchangeRate result = Factory.New<RefExchangeRate>();

			result.RE_GC = company.PK;
			result.RE_RX_NKExCurrency = foreignCurrency.RX_Code;
			result.RE_StartDate = startDate;
			result.RE_ExpiryDate = endDate;
			result.RE_SellRate = rate;
			result.RE_ExRateType = rateType;

			Factory.Save();
		}
		public void TestZPropertyInfoChange()
		{
			var entryLine = Factory.New<CusEntryLine>();

			AssertEquals(true, entryLine.CL_LineNumberInfo.ReadOnly);
		}

		public void TestTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0102399000", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1), "Tariff Description 1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "8429521022", ZDateTime.Today.AddDays(-7), ZDateTime.Today.AddDays(1), "Tariff Description 2");

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0102399000";
			invoiceLine1.JI_CL = entryLine.PK;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "8429521022";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8429521022";
			invoiceLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_AdValoremTariff = "XXXXXXXXXX";
			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "XXXXXXXXXX";
			invoiceLine3.JI_CL = entryLine3.PK;

			AssertEquals("Tariff Description 1", entryLine.TariffDescription);
			AssertEquals("Tariff Description 2", entryLine2.TariffDescription);
			AssertEquals(ZString.Empty, entryLine3.TariffDescription);
		}

		public override void TestICusEntryLineInterface()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "A";
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1.1m;
			invoiceLine1.JI_Tariff = "1234567890";
			invoiceLine1.JI_Description = "DESCRIPTION";
			invoiceLine1.JI_InvoiceUQ = "ZZ";
			invoiceLine1.JI_CustomsUnitQty = "PK";
			invoiceLine1.JI_CustomsQuantity = 3.2m;

			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1.0m;
			invoiceLine2.JI_Tariff = "1234567890";
			invoiceLine2.JI_Description = "DESCRIPTION";
			invoiceLine2.JI_InvoiceUQ = "ZZ";
			invoiceLine2.JI_CustomsUnitQty = "PK";
			invoiceLine2.JI_CustomsQuantity = 2.1m;

			Assert("Precondition: !InvoiceLine1.JI_Tariff.IsEmpty", !invoiceLine1.JI_Tariff.IsEmpty);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = invoiceLine1.JI_Tariff;

			ICusEntryLine iEntryLine = entryLine;
			AssertEquals("IEntryLine.CL_LineNumber", (short)1, iEntryLine.CL_LineNumber);
			AssertEquals("IEntryLine.Description", "DESCRIPTION", iEntryLine.Description);
			AssertEquals("IEntryLine.Tariff", invoiceLine1.JI_Tariff, iEntryLine.Tariff);
			AssertEquals("IEntryLine.CustomsQuantity", 5m, iEntryLine.CustomsQuantity);
			AssertEquals("IEntryLine.CustomsUnitQty", "PK", iEntryLine.CustomsUnitQty);
			AssertEquals("IEntryLine.BondedWarehouseQuantity", 2.1m, iEntryLine.BondedWarehouseQuantity);
			AssertEquals("IEntryLine.CustomsUnitQty", "ZZ", iEntryLine.BondedWarehouseUnitQuantity);
		}

		public void TestLocalExportSupportingDocumentData()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SequenceNumber = 1;

			AssertEquals("", entryLine.SupportingDocumentNo);
			AssertEquals("", entryLine.SupportingDocumentType);
			AssertEquals("", entryLine.SupportingDocumentTypeDescription);

			invoiceLine.SupportingDocumentReferenceNumber = "123456789012345";
			invoiceLine.SupportingDocumentCode = "XX";
			AssertEquals("123456789012345", entryLine.SupportingDocumentNo);
			AssertEquals("XX", entryLine.SupportingDocumentType);
			AssertEquals("", entryLine.SupportingDocumentTypeDescription);

			invoiceLine.SupportingDocumentCode = LocalExportDocumentTypeList.Codes._99;
			AssertEquals(LocalExportDocumentTypeList.Codes._99, entryLine.SupportingDocumentType);
			AssertEquals(LocalExportDocumentTypeList.Descriptions._99, entryLine.SupportingDocumentTypeDescription);
		}

		public void TestItemsOfEntryLineFeeAndRate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();

			var duty = entryLine.Fees.AddNew();
			duty.CF_Rate = 1000m;
			duty.CF_ChargeAmount = 2000m;
			duty.CF_ChargeType = ChargeTypeList.Codes.Duty;
			duty = entryLine.Fees.AddNew();
			duty.CF_ChargeAmount = 200m;
			duty.CF_ChargeType = ChargeTypeList.Codes.Duty;

			var specialConsumptionTax = entryLine.Fees.AddNew();
			specialConsumptionTax.CF_ChargeAmount = 1000m;
			specialConsumptionTax.CF_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
			specialConsumptionTax = entryLine.Fees.AddNew();
			specialConsumptionTax.CF_ChargeAmount = 100m;
			specialConsumptionTax.CF_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;

			var liquor = entryLine.Fees.AddNew();
			liquor.CF_ChargeAmount = 1000m;
			liquor.CF_ChargeType = ChargeTypeList.Codes.LiquorTax;
			liquor = entryLine.Fees.AddNew();
			liquor.CF_ChargeAmount = 100m;
			liquor.CF_ChargeType = ChargeTypeList.Codes.LiquorTax;

			var transportationTax = entryLine.Fees.AddNew();
			transportationTax.CF_ChargeAmount = 1000m;
			transportationTax.CF_ChargeType = ChargeTypeList.Codes.TransportationTax;
			transportationTax = entryLine.Fees.AddNew();
			transportationTax.CF_ChargeAmount = 100m;
			transportationTax.CF_ChargeType = ChargeTypeList.Codes.TransportationTax;

			var education = entryLine.Fees.AddNew();
			education.CF_ChargeAmount = 4000m;
			education.CF_ChargeType = ChargeTypeList.Codes.EducationTax;
			education = entryLine.Fees.AddNew();
			education.CF_ChargeAmount = 400m;
			education.CF_ChargeType = ChargeTypeList.Codes.EducationTax;

			var agriculture = entryLine.Fees.AddNew();
			agriculture.CF_ChargeAmount = 5000m;
			agriculture.CF_ChargeType = ChargeTypeList.Codes.AgricultureTax;
			agriculture = entryLine.Fees.AddNew();
			agriculture.CF_ChargeAmount = 500m;
			agriculture.CF_ChargeType = ChargeTypeList.Codes.AgricultureTax;

			var vat = entryLine.Fees.AddNew();
			vat.CF_ChargeAmount = 6000m;
			vat.CF_ChargeType = ChargeTypeList.Codes.VAT;
			vat = entryLine.Fees.AddNew();
			vat.CF_ChargeAmount = 600m;
			vat.CF_ChargeType = ChargeTypeList.Codes.VAT;
			vat = entryLine.Fees.AddNew();
			vat.CF_ChargeAmount = 60m;
			vat.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount;

			AssertEquals(1000m, entryLine.DutyRate);
			AssertEquals(2200m, entryLine.DutyAmount);
			AssertEquals(3300m, entryLine.DomesticTaxAmount);
			AssertEquals(4400m, entryLine.EducationTaxAmount);
			AssertEquals(5500m, entryLine.AgricultureTaxAmount);
			AssertEquals(6600m, entryLine.VATAmount);
		}

		public void TestPreferenceCodeDescription()
		{
			var preference = "A1";
			var preferenceDesc = "기본세율(선택1)";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry(preference, "기본세율(선택1)", Core.Constants.CountryCodes.KoreaSouth);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = entryLine.InvoiceLines.AddNew();

			invoiceLine.JI_PrimaryPreference = "LDC";
			AssertNullOrEmpty(entryLine.PreferenceCodeDescription);

			invoiceLine.JI_PrimaryPreference = preference;
			AssertEquals(preferenceDesc, entryLine.PreferenceCodeDescription);
		}

		public void TestLineOrProductIsValidationEnabled()
		{
			Assert(!((ILineOrProduct)entryLine).IsValidationEnabled);
		}

		public void TestResetFTASequenceOnEntryChanged()
		{
			entryLine.CL_FTASequenceNumber = 1;
			entryLine.CL_CH = declaration.CustomsEntryHeaders.AddNew().PK;
			AssertEquals((ZShort)0, entryLine.CL_FTASequenceNumber);
		}
		protected override void SetUp()
		{
			setUNIPASSDeclarantID = KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "12345");
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = declaration.JE_MessageType;
			entry.EntryNumber = "1234567890I";
			var entryNum = entry.CusEntryNumber;
			entryNum.CE_IssueDate = ZDateTime.Today;
			entryLine = entry.MergedLines.AddNew();
			entryLine.CL_AdValoremTariff = "0102399000";
		}

		protected override Type ExpectedTypeOfFees => typeof(CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>);

		protected override void TearDown()
		{
			setUNIPASSDeclarantID.Dispose();
		}

		IDisposable setUNIPASSDeclarantID;
		JobDeclaration declaration;
		CusEntryHeader entry;
		CusEntryLine entryLine;
	}
}
