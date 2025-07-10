using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusEntryLine))]
	sealed class DocCusEntryLineTest : DocBaseCusEntryLineAbstractTest<CusEntryLine, DocCusEntryLine>
	{
		#region ZString Fields

		public void TestSupplierCustomsClientID()
		{
			var supplier = Factory.New<OrgHeader>();
			var cusCode = supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456");
			InvoiceLine.JI_OH_Supplier = supplier.PK;

			AssertEquals("123456", EntryLineWrapper.SupplierCustomsClientID);

			var newAddress = Factory.New<OrgAddress>();
			InvoiceHeader.JZ_OA_SupplierAddress = newAddress.PK;
			var cusCode2 = supplier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "654321");
			cusCode2.OK_OA_PremisesAddress = newAddress.PK;
			AssertEquals("654321", EntryLineWrapper.SupplierCustomsClientID);
		}

		public void TestFormattedDutyRateIncludingWHEstimate()
		{
			EntryLine.CL_DutyPercent = 15.1515m;
			EntryLine.CL_FlatAmount = 20.151515m;
			EntryLine.CL_FlatAmountUQ = "USD";
			AssertEquals("FormattedDutyRateIncludingWHEstimate", "15.15%+20.15151/USD", EntryLineWrapper.FormattedDutyRateIncludingWHEstimate);
		}

		public void TestFormattedGST()
		{
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATDeferred, 23.45m);
			AssertEquals("FormattedGST", 23.45m, EntryLineWrapper.GSTAmount);
			EntryLineInternal.Fees.AddOrUpdate(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 12.34m);
			AssertEquals("FormattedGST", 12.34m, EntryLineWrapper.GSTAmount);
		}

		public void TestIsNature20()
		{
			EntryLineInternal.RandomLine.JI_IsPackToBondForLine = false;
			AssertEquals(false, EntryLineWrapper.IsNature20);
			EntryLineInternal.RandomLine.JI_IsPackToBondForLine = true;
			AssertEquals(true, EntryLineWrapper.IsNature20);
		}

		public void TestVOTILine()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			JobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			JobComInvoiceHeader invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			InvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var mockEntry = Factory.New<DummyCusEntryHeader>();
			mockEntry.NatureReturns = new ZString("N20");
			mockEntry.CH_JE = declaration.PK;
			EntryLine = mockEntry.MergedLines.AddNew();
			InvoiceLine.JI_CL = EntryLine.PK;
			DocCusEntryLine linewrapper = DocCusEntryLine.New(EntryLine, Factory);
			EntryLine.CL_CustomsValue = 15.00m;
			EntryLine.EntryLineAddInfo.ZA_TILV = "123";
#pragma warning disable IDE0002
			EntryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.WetAmount, 23.45m);
			EntryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 34.55m);
			EntryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 15.66m);
			EntryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 16.66m);
			EntryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityConcession, 18.76m);
			EntryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.SecurityLiability, 28.56m);
#pragma warning restore IDE0002
			AssertEquals("VOTI=      170.32      T&I=      123.00      WET=       23.45      LCT=       34.55      CVD=       15.66      DMP=       16.66      Security=       18.76      Security Uncollected=       28.56", linewrapper.VOTILine);

			EntryLine = mockEntry.MergedLines.AddNew();
			linewrapper = DocCusEntryLine.New(EntryLine, Factory);
			AssertEquals("VOTI=        0.00      T&I=        0.00", linewrapper.VOTILine);
		}

		sealed class DummyCusEntryHeader : CusEntryHeader
		{
			public DummyCusEntryHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString NatureReturns { get; set; } = string.Empty;

			public override ZString Nature => NatureReturns;
		}

		public void TestTariffNumber()
		{
			AssertEquals("TariffNumber is empty", ZString.Empty, EntryLineWrapper.TariffNumber);

			InvoiceLine.JI_Tariff = "2300.00.00 DD";
			AssertEquals("TariffNumber is not empty", "23000000", EntryLineWrapper.TariffNumber);
		}

		public void TestStatCode()
		{
			AssertEquals("Stat code is empty", ZString.Empty, EntryLineWrapper.StatCode);
		}

		public void TestTreatmentCode()
		{
			AssertEquals("Treatment code is empty", ZString.Empty, EntryLineWrapper.TreatmentCode);

			EntryLine.InvoiceLineAddInfo.ZA_TreatmentCode_Hidden = "CCC";
			AssertEquals("Treatment code is not empty", "CCC", EntryLineWrapper.TreatmentCode);
		}

		public override void TestLinePricesWithCurrency()
		{
			AssertEquals(DocEntryLineMergeOfTwoInvoiceLines.LinePricesWithCurrency, "200.05 ZAR");
		}

		#endregion

		#region ZDecimal Fields

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

		public void TestFlatDutyPortion()
		{
			EntryLine.Fees.AddOrUpdate(Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes.FlatDutyPortion, 12.34m);
			AssertEquals("FlatDutyPortion", EntryLine.FlatDutyPortion, EntryLineWrapper.FlatDutyPortion);
		}

		public void TestInterimAntiDumpingDuty()
		{
			EntryLine.Fees.AddOrUpdate(Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty, 12.34m);
			AssertEquals("InterimAntiDumpingDuty", EntryLine.InterimAntiDumpingDuty, EntryLineWrapper.InterimAntiDumpingDuty);
		}

		public void TestInterimCountervailingDuty()
		{
			EntryLine.Fees.AddOrUpdate(Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes.InterimCountervailingDuty, 12.34m);
			AssertEquals("InterimCountervailingDuty", EntryLine.InterimCountervailingDuty, EntryLineWrapper.InterimCountervailingDuty);
		}

		public void TestLCTAmount()
		{
			EntryLine.Fees.AddOrUpdate(Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes.LCTAmount, 12.34m);
			AssertEquals("LCTAmount", EntryLine.LCTAmount, EntryLineWrapper.LCTAmount);
		}

		public void TestWETAmount()
		{
			EntryLine.Fees.AddOrUpdate(Enterprise.Customs.AU.Declaration.Business.CusEntryChargeTypeList.Codes.WetAmount, 12.34m);
			AssertEquals("WETAmount", EntryLine.WETAmount, EntryLineWrapper.WETAmount);
		}

		public void TestWarehouseQuantityForATDAndWarehouseQuantityUnitForATD()
		{
			InvoiceLine.JI_CustomsQuantity = 567M;
			AssertEquals("WarehouseQuantityForATD", 567M, EntryLineWrapper.WarehouseQuantityForATD);
			AssertEquals("WarehouseQuantityUnitForATD", ZString.Empty, EntryLineWrapper.WarehouseQuantityUnitForATD);

			InvoiceLine.AddInfo.ZA_WRQ = 123M;
			InvoiceLine.AddInfo.ZA_WRU = "KG";
			AssertEquals("WarehouseQuantityForATD", 123M, EntryLineWrapper.WarehouseQuantityForATD);
			AssertEquals("WarehouseQuantityUnitForATD", "KG", EntryLineWrapper.WarehouseQuantityUnitForATD);

			InvoiceLine.JI_CustomsUnitQty = "KG";
			InvoiceLine.JI_CustomsQuantity = 567M;
			AssertEquals("WarehouseQuantityForATD", 567M, EntryLineWrapper.WarehouseQuantityForATD);
			AssertEquals("WarehouseQuantityUnitForATD", "KG", EntryLineWrapper.WarehouseQuantityUnitForATD);
		}

		#endregion

		#region Wrapper Fields

		public void TestCusEntryHeader()
		{
			AssertNotNull("CusEntryHeader", EntryLineWrapper.CusEntryHeader);
			AssertEquals("CusEntryHeader is of type DocCusEntryHeader", typeof(DocCusEntryHeader), EntryLineWrapper.CusEntryHeader.GetType());
		}

		#endregion

		#region Collections

		public void TestQuestions()
		{
			// confirm questions are sorted by ID.
			CMRLodgementQuestion question50000 = Factory.New<CMRLodgementQuestion>();
			question50000.CQ_LodgementQuestionIdentifier = 50000;
			question50000.CQ_LodgementQuestionStartDate = new ZDateTime(2021, 1, 1);
			question50000.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			question50000.CQ_LodgementQuestionName = "Q50000";
			question50000.CQ_LodgementQuestionText = "Question50000";

			CMRLodgementQuestion question50010 = Factory.New<CMRLodgementQuestion>();
			question50010.CQ_LodgementQuestionIdentifier = 50010;
			question50010.CQ_LodgementQuestionStartDate = new ZDateTime(2021, 1, 1);
			question50010.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			question50010.CQ_LodgementQuestionName = "Q50010";
			question50010.CQ_LodgementQuestionText = "Question50010";

			CMRLodgementQuestion question5100 = Factory.New<CMRLodgementQuestion>();
			question5100.CQ_LodgementQuestionIdentifier = 5100;
			question5100.CQ_LodgementQuestionStartDate = new ZDateTime(2021, 1, 1);
			question5100.CQ_LodgementQuestionEndDate = ZDateTime.Empty;
			question5100.CQ_LodgementQuestionName = "Q5100";
			question5100.CQ_LodgementQuestionText = "Question5100";

			var question1 = EntryLine.Questions.AddNew();
			question1.QuestionID = "50000";
			var question2 = EntryLine.Questions.AddNew();
			question2.QuestionID = "50010";
			var question3 = EntryLine.Questions.AddNew();
			question3.QuestionID = "5100";

			var docQuestions = EntryLineWrapper.Questions;
			AssertEquals("Question5100", docQuestions[0].Question);
			AssertEquals("Question50000", docQuestions[1].Question);
			AssertEquals("Question50010", docQuestions[2].Question);
		}

		#endregion

		#region Implementation

		CusEntryLine EntryLine;
		JobComInvoiceHeader InvoiceHeader;
		JobComInvoiceLine InvoiceLine;

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Australia; }
		}

		protected override CusEntryLine GetNewEntryLine()
		{
			if (EntryLine == null)
			{
				JobDeclaration declaration = JobDeclaration.New(Factory);
				JobComInvoiceGroupHeader invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
				InvoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
				InvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
				EntryLine = entryHeader.MergedLines.AddNew();
				InvoiceLine.JI_CL = EntryLine.PK;

				return EntryLine;
			}

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

					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
					declaration.JE_MessageType = "IMP";
					declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 5, 5);
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

		#endregion
	}
}
