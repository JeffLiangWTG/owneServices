using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.FormalEntry.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		public override void TestDutyAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.DutyAmountRounded);
		}

		public override void TestGSTVATAmountRounded()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12.3453M);
			AssertEquals("DutyAmount", 12.35M, EntryLineWrapperInternal.GSTVATAmountRounded);
		}

		public void TestMiscSuppliers()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ZGuid miscOrgPK = declaration.CachedMiscOrgPK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_OH_Supplier = miscOrgPK;
			declaration.MiscSupplierName = "MY CAT'S MISC SUPPLIER";
			declaration.JE_OH_Importer = miscOrgPK;
			declaration.MiscImporterName = "MY DOG'S MISC IMPORTER";

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 1;

			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			DocCusEntryLine entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);

			AssertEquals("", entryLineWrapper.SupplierCode);
			AssertEquals("MY CAT'S MISC SUPPLIER", entryLineWrapper.SupplierName);

			OrgHeader anotherOrg = Factory.New<OrgHeader>();
			anotherOrg.OH_Code = "NOTHING";
			anotherOrg.OH_FullName = "NOTHING COMPARES, TOO YOU.";
			anotherOrg.LocalCustomsSupplierCode = "87878787A";

			invoiceHeader.JZ_OH_Supplier = anotherOrg.PK;

			entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);

			AssertEquals("87878787A", entryLineWrapper.SupplierCode);
			AssertEquals("NOTHING COMPARES, TOO YOU.", entryLineWrapper.SupplierName);
		}

		public void TestExchangeRateIndicator()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 1;

			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			DocCusEntryLine entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);
			AssertEquals("EntryLineWrapper.ExchangeRateIndicator", ExchangeRateIndicatorList.Descriptions.ForwardCover, entryLineWrapper.ExchangeRateIndicator);
		}

		public void TestVFDForeign()
		{
			var declaration = CreateExportAirJob();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Invoice01";
			invoiceHeader.JZ_FOBValue = 0;
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_InvoiceAmount = 15300.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			invoiceHeader.JZ_InvoiceCurrExRate = 1m;

			var freightCharge = invoiceHeader.GroupHeader.Charges.AddNew();
			freightCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge.J7_DistributeBy = "WGT";
			freightCharge.J7_FullOrPartialApportionment = "PAA";
			freightCharge.J7_Amount = 1291.24m;
			freightCharge.J7_IsApportionedCharge = true;
			freightCharge.J7_IsGSTApplicable = true;
			freightCharge.J7_IsIncludedInITOT = true;
			freightCharge.J7_IsDutiable = false;
			freightCharge.J7_RX_NKCurrency = "NZD";

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_LinePrice = 15300.00m;
			invoiceLine.JI_Tariff = "1902194000";
			invoiceLine.JI_CustomsQuantity = 900m;
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLine.JI_CountryOfOrigin = "NZ";

			Factory.Save();
			declaration.DoMerge(new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			AssertEquals("CL_CustomsValue should have been calculated", 15300.00m, entryLine.CL_CustomsValue);

			var entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);
			AssertEquals("EntryLineWrapper.VFDWholeNZ", "15300", entryLineWrapper.VFDWholeNZ);
			AssertEquals("EntryLineWrapper.VFDForeign returns NZD value for line 1", "15300.00", entryLineWrapper.VFDForeign);
			AssertEquals("EntryLineWrapper.CurrencyCode", "NZD", entryLineWrapper.CurrencyCode);
			AssertEquals("EntryLineWrapper.ExchangeRate", "1.00", entryLineWrapper.ExchangeRate);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "Invoice02";
			invoiceHeader2.JZ_FOBValue = 0;
			invoiceHeader2.JZ_IncoTerm = "CIF";
			invoiceHeader2.JZ_InvoiceAmount = 17691.80m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "AUD";
			invoiceHeader2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.ForwardCover;
			invoiceHeader2.JZ_InvoiceCurrExRate = 0.93m;

			var invoiceLine2 = invoiceHeader2.InvoiceLines.AddNew();
			invoiceLine2.JI_LineNo = 2;
			invoiceLine2.JI_LinePrice = 17691.80m;
			invoiceLine2.JI_Tariff = "5106100101B";
			invoiceLine2.JI_CustomsQuantity = 1252.3m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine2.JI_CountryOfOrigin = "NZ";

			Factory.Save();
			declaration.DoMerge(new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine2 = entryHeader.MergedLines[1];
			AssertEquals("CL_CustomsValue should have been calculated", 19023.44m, entryLine2.CL_CustomsValue);

			entryLineWrapper = DocCusEntryLine.New(entryLine2, Factory);
			AssertEquals("EntryLineWrapper.VFDWholeNZ", "19023", entryLineWrapper.VFDWholeNZ);
			AssertEquals("EntryLineWrapper.VFDForeign", "17691.80", entryLineWrapper.VFDForeign);
			AssertEquals("EntryLineWrapper.CurrencyCode", "AUD", entryLineWrapper.CurrencyCode);
			AssertEquals("EntryLineWrapper.ExchangeRate", "0.93", entryLineWrapper.ExchangeRate);
		}

		public void TestVFDForeignOnMergedLines()
		{
			var declaration = CreateExportAirJob();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "Invoice01";
			invoiceHeader.JZ_FOBValue = 0;
			invoiceHeader.JZ_IncoTerm = "CIF";
			invoiceHeader.JZ_InvoiceAmount = 1015.00m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			invoiceHeader.JZ_InvoiceCurrExRate = 1m;

			var freightCharge = invoiceHeader.GroupHeader.Charges.AddNew();
			freightCharge.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			freightCharge.J7_DistributeBy = "WGT";
			freightCharge.J7_FullOrPartialApportionment = "PAA";
			freightCharge.J7_Amount = 91.24m;
			freightCharge.J7_IsApportionedCharge = true;
			freightCharge.J7_IsGSTApplicable = true;
			freightCharge.J7_IsIncludedInITOT = true;
			freightCharge.J7_IsDutiable = false;
			freightCharge.J7_RX_NKCurrency = "NZD";

			var inv1Line1 = invoiceHeader.InvoiceLines.AddNew();
			inv1Line1.JI_LineNo = 1;
			inv1Line1.JI_LinePrice = 125.00m;
			inv1Line1.JI_Tariff = "1902194000";
			inv1Line1.JI_CustomsQuantity = 100m;
			inv1Line1.JI_CustomsUnitQty = "KGM";
			inv1Line1.JI_CountryOfOrigin = "NZ";

			var inv1Line2 = invoiceHeader.InvoiceLines.AddNew();
			inv1Line2.JI_LineNo = 2;
			inv1Line2.JI_LinePrice = 320.00m;
			inv1Line2.JI_Tariff = "1902194000";
			inv1Line2.JI_CustomsQuantity = 400m;
			inv1Line2.JI_CustomsUnitQty = "KGM";
			inv1Line2.JI_CountryOfOrigin = "NZ";

			var inv1Line3 = invoiceHeader.InvoiceLines.AddNew();
			inv1Line3.JI_LineNo = 3;
			inv1Line3.JI_LinePrice = 670.00m;
			inv1Line3.JI_Tariff = "1902194000";
			inv1Line3.JI_CustomsQuantity = 900m;
			inv1Line3.JI_CustomsUnitQty = "KGM";
			inv1Line3.JI_CountryOfOrigin = "NZ";

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "Invoice02";
			invoiceHeader2.JZ_FOBValue = 0;
			invoiceHeader2.JZ_IncoTerm = "CIF";
			invoiceHeader2.JZ_InvoiceAmount = 500.00m;
			invoiceHeader2.JZ_RX_NKInvoice_Currency = "NZD";
			invoiceHeader2.JZ_ExchangeRateIndicator = ExchangeRateIndicatorList.Codes.NZD;
			invoiceHeader2.JZ_InvoiceCurrExRate = 1m;

			var inv1Line4 = invoiceHeader2.InvoiceLines.AddNew();
			inv1Line4.JI_LineNo = 4;
			inv1Line4.JI_LinePrice = 500.00m;
			inv1Line4.JI_Tariff = "1902194000";
			inv1Line4.JI_CustomsQuantity = 100m;
			inv1Line4.JI_CustomsUnitQty = "KGM";
			inv1Line4.JI_CountryOfOrigin = "NZ";

			Factory.Save();
			declaration.DoMerge(new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryLine = entryHeader.MergedLines[0];
			var entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);
			AssertEquals("EntryLineWrapper.VFDForeign returns summed NZD value for lines 1, 2, 3 & 4 as an output string", "1615.00", entryLineWrapper.VFDForeign);
		}

		public void TestCanGetPararmeterlessConstructorGoing()
		{
			DocCusEntryLine docEntryLine = DocCusEntryLine.New(Factory);
			AssertEquals("CountryOfExport Object", null, docEntryLine.CountryOfExport);
			AssertEquals("CurrencyCode ZString Field", "", docEntryLine.CurrencyCode);
		}

		#region New Properties for Customs Certificate
		public void TestCIFWholeNZ()
		{
			JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			header.JZ_InvoiceNumber = "1";
			header.JZ_RX_NKInvoice_Currency = "NZD";

			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.JI_Tariff = "123";
			invoiceLine.JI_LineNo = 1;

			CusEntryLine entryLine = declaration.CusEntryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			DocCusEntryLine entryLineWrapper = DocCusEntryLine.New(entryLine, Factory);

			AssertEquals("EntryLineWrapper.CIFWholeNZ", "0", entryLineWrapper.CIFWholeNZ);
		}

		public void TestLevyAmount()
		{
			EntryLine.ALACLevyAmount = 23.45m;
			AssertEquals("EntryLineWrapper.LevyAmount", "23.45", EntryLineWrapper.LevyAmount);
		}

		public void TestLevyAmount_SGG()
		{
			EntryLine.SyntheticGreenhouseGasesLevyAmount = 99.99m;
			AssertEquals("EntryLineWrapper.LevyAmount", "99.99", EntryLineWrapper.LevyAmount);
		}

		[ExpectNoExceptions]
		public void TestTotalInvoiceLinesValueAndCurrency()
		{
			InvoiceLine.JI_LinePrice = 101.32m;
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "NZD";
			AssertEquals("EntryLineWrapper.TotalInvoiceLinesValueAndCurrency", "101.32 NZD", EntryLineWrapper.TotalInvoiceLinesValueAndCurrency);
			InvoiceHeader.JZ_RX_NKInvoice_Currency = "";
			AssertEquals("EntryLineWrapper.TotalInvoiceLinesValueAndCurrency", "", EntryLineWrapper.TotalInvoiceLinesValueAndCurrency);
		}

		public void TestStatQtyAndUnitIncludingDescription()
		{
			InvoiceLine.JI_CustomsUnitQty = "XX";
			InvoiceLine.JI_CustomsQuantity = 101.32m;
			AssertEquals("EntryLineWrapper.StatQtyAndUnitIncludingDescription", "Stat Qty: 101.320 XX", EntryLineWrapper.StatQtyAndUnitIncludingDescription);
		}

		public void TestSuppQtyAndUnitIncludingDescription()
		{
			InvoiceLine.JI_SupplementaryQty = 102.32m;
			InvoiceLine.JI_SupplementaryUQ = "YY";
			AssertEquals("EntryLineWrapper.SuppQtyAndUnitIncludingDescription", "Supp Qty: 102.320 YY", EntryLineWrapper.SuppQtyAndUnitIncludingDescription);
		}

		[TestDate(2006, 01, 01)]
		public void TestDutyAndLevyRate()
		{
			AssertEquals("EntryLineWrapper.DutyAndLevyRate", "", EntryLineWrapper.DutyAndLevyRate);
			EntryLine.CL_AdValoremTariff = "7007.21.02.01K";
			EntryLine.Declaration?.ResumeApportionment();
			AssertEquals("EntryLineWrapper.DutyAndLevyRate", "17.50%", EntryLineWrapper.DutyAndLevyRate);
		}

		[TestDate(2006, 01, 01)]
		public void TestDutyAndLevyRateIncludingDescription()
		{
			AssertEquals("EntryLineWrapper.DutyAndLevyRate", "", EntryLineWrapper.DutyAndLevyRate);
			EntryLine.CL_AdValoremTariff = "7007.21.02.01K";
			EntryLine.Declaration?.ResumeApportionment();
			AssertEquals("EntryLineWrapper.DutyAndLevyRateIncludingDescription", "Duty Rate: 17.50%", EntryLineWrapper.DutyAndLevyRateIncludingDescription);
			Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("EntryLineWrapper.DutyAndLevyRateIncludingDescription", "", EntryLineWrapper.DutyAndLevyRateIncludingDescription);
		}
		#endregion

		public void TestMiscValuesDontDuplicateAcrossLines()
		{
			EntryLine.ALACLevyAmount = 10m;
			DocCusEntryLine docEntryLine = DocCusEntryLine.New(EntryLine, Factory);
			AssertEquals("DocEntryLine.MiscAmountNZ", "10.00", docEntryLine.MiscAmountNZ);
			AssertEquals("DocEntryLine.MiscReasonCode", "ALAC", docEntryLine.MiscReasonCode);

			EntryLine.ALACLevyAmount = 0m;
			docEntryLine = DocCusEntryLine.New(EntryLine, Factory);
			AssertEquals("DocEntryLine.MiscAmountNZ", "", docEntryLine.MiscAmountNZ);
			AssertEquals("DocEntryLine.MiscReasonCode", "", docEntryLine.MiscReasonCode);
		}

		public override void TestLinePricesWithCurrency()
		{
			AssertEquals(DocEntryLineMergeOfTwoInvoiceLines.LinePricesWithCurrency, "200.05 ZAR");
		}

		public void TestProductBrandName()
		{
			DocCusEntryLine docEntryLine = DocCusEntryLine.New(EntryLine, Factory);
			AssertEquals(ZString.Empty, docEntryLine.ProductBrandName);

			var part = Factory.NewWithValidTestData<Enterprise.Customs.Business.OrgSupplierPart>();
			part.OP_PartNum = "ProductName";
			part.OP_Brand = "Brand X";
			EntryLine.RandomLine.JI_OP = part.PK;
			EntryLine.RandomLine.PartSyncManager.ReloadPart = true;

			docEntryLine = DocCusEntryLine.New(EntryLine, Factory);
			AssertEquals("DocEntryLine.ProductBrandName", "Brand X", docEntryLine.ProductBrandName);
		}

		#region Implementation
		JobDeclaration Declaration;
		CusEntryLine EntryLine
		{
			get
			{
				if (fEntryLine == null)
				{
					Declaration = Factory.New<JobDeclaration>();
					Declaration.JE_MessageType = Enterprise.Customs.NZ.Business.JobMessageTypeList.Codes.Import;
					Declaration.JE_MessageSubType = Enterprise.Customs.NZ.Business.JobMessageSubTypeList.Codes.Normal;
					JobComInvoiceGroupHeader invoiceGroupHeader = Declaration.JobComInvoiceGroupHeaders[0];
					InvoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
					InvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
					fEntryLine = Declaration.CusEntryHeader.MergedLines.AddNew();
					InvoiceLine.JI_CL = fEntryLine.PK;
				}
				return fEntryLine;
			}
		}
		CusEntryLine fEntryLine;
		JobComInvoiceHeader InvoiceHeader;
		JobComInvoiceLine InvoiceLine;

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.NewZealand; }
		}

		protected override CusEntryLine GetNewEntryLine()
		{
			return EntryLine;
		}

		DocCusEntryLine EntryLineWrapper
		{
			get { return EntryLineWrapperInternal; }
		}

		protected override DocCusEntryLine CreateEntryLineWrapper(Enterprise.Customs.Business.ICusEntryLine entryLineInternal)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineInternal, Factory);
		}

		Enterprise.Customs.Business.CusEntryLine fEntryLineMergeOfTwoInvoiceLines;
		protected override Enterprise.Customs.Business.ICusEntryLine EntryLineMergeOfTwoInvoiceLines
		{
			get
			{
				if (fEntryLineMergeOfTwoInvoiceLines == null)
				{
					JobDeclaration declaration = (JobDeclaration)GetNewDeclaration();
					JobComInvoiceHeader header = declaration.Invoices.AddNew();
					header.JZ_InvoiceNumber = "1";
					header.JZ_RX_NKInvoice_Currency = "ZAR";

					JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
					invoiceLine.JI_LinePrice = 200.05m;
					invoiceLine.JI_Tariff = "123";
					invoiceLine.JI_LineNo = 1;

					declaration.MessageInitiator = new Enterprise.Customs.Business.SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = "IMP";
					declaration.DoMerge();
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders.Count > 0);
					Assert("Must have at least one CustomsEntryHeader", declaration.CustomsEntryHeaders[0].MergedLines.Count > 0);
					fEntryLineMergeOfTwoInvoiceLines = declaration.CustomsEntryHeaders[0].MergedLines[0];
				}
				return fEntryLineMergeOfTwoInvoiceLines;
			}
		}

		protected override DocCusEntryLine DocEntryLineMergeOfTwoInvoiceLines
		{
			get { return CreateEntryLineWrapper(EntryLineMergeOfTwoInvoiceLines); }
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCusEntryLine.New(Factory),
				DocCusEntryLine.New(EntryLine, Factory)
			};
		}

		JobDeclaration CreateExportAirJob()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS";
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			jobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			jobDeclaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			jobDeclaration.JE_TotalWeight = 15m;
			jobDeclaration.JE_TotalWeightUnit = "KG";
			jobDeclaration.JE_VoyageFlightNo = "QF108";
			jobDeclaration.JE_ExportDate = ZDateTime.Today;
			jobDeclaration.JE_DateOfArrival = ZDateTime.Today;
			jobDeclaration.JE_DeclarationReference = "BEX0042709";
			jobDeclaration.JE_GoodsDescription = "NEWS PAPER";
			jobDeclaration.JE_GS_NKCusAgent = "JKS";
			jobDeclaration.JE_HouseBill = "HB92027";
			jobDeclaration.JE_MasterBill = "08100342948";
			jobDeclaration.JE_OH_Supplier = Factory.NewWithValidTestData<OrgHeader>().PK;
			jobDeclaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			jobDeclaration.JE_OH_ShippingLine = shippingLine.PK;
			jobDeclaration.JE_RL_NKFinalDestination = "AUSYD";
			jobDeclaration.JE_RL_NKOrigin = "NZAKL";
			jobDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			jobDeclaration.JE_RL_NKPortOfLoading = "NZAKL";
			jobDeclaration.JE_TotalNoOfPacks = 15;
			jobDeclaration.JE_TotalNoOfPacksPackType = "PKT";
			return jobDeclaration;
		}

		#endregion
	}
}
