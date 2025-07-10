using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocLandedCostHeader))]
	[CountrySpecificTest("AU")]
	sealed class DocLandedCostHeaderTest : DocumentWrapperTestCase
	{
		public void TestDocDeclarationDefaults()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001243";
			ILandedCostHeader lcParent = declaration;
			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.LT_ParentID = lcParent.PK;
			lcHeader.LT_ParentTableCode = lcParent.TableCode;

			declaration.JE_OwnerRef = "LIKEFRIESWITHTHAT";
			DocLandedCostHeader docLCHeader = DocLandedCostHeader.New(lcHeader, Factory);
			AssertEquals("docLCHeader.DocDeclaration.OwnerRef", "LIKEFRIESWITHTHAT", docLCHeader.DocDeclaration.OwnerRef);
			AssertEquals("docLCHeader.ToString()", "B00001243", docLCHeader.ToString());
		}

		public void TestTotalCostWithMarkup1Applied()
		{
			LandedCostHistory lCHistory1 = LCHeader.Histories.AddNew();
			lCHistory1.SetLineValue(1000m, "TDT");
			lCHistory1.SetLineValue(20m, "ST1");
			lCHistory1.SetLineValue(30m, "ST2");
			lCHistory1.SetLineValue(50m, "ST3");
			lCHistory1.LH_LandedCostGroup1 = 500m;
			lCHistory1.LH_LandedCostGroup2 = 600m;

			LandedCostHistory lCHistory2 = LCHeader.Histories.AddNew();
			lCHistory2.SetLineValue(2000m, "TDT");
			lCHistory2.SetLineValue(20m, "ST1");
			lCHistory2.SetLineValue(30m, "ST2");
			lCHistory2.SetLineValue(50m, "ST3");
			lCHistory2.LH_LandedCostGroup1 = 500m;
			lCHistory2.LH_LandedCostGroup2 = 600m;

			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			BaseJobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			lCHistory1.LH_ParentID = invoiceLine1.PK;
			lCHistory1.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			lCHistory1.LH_LandedCostMarginPercent1 = 10;

			BaseJobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			lCHistory2.LH_ParentID = invoiceLine2.PK;
			lCHistory2.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			lCHistory2.LH_LandedCostMarginPercent1 = 10;

			AssertEquals("DocLCHeader.TotalCostWithMarkup1Applied", 27940.00m, DocLCHeader.TotalCostWithMarkup1Applied);
		}

		public void TestRateEntryHeaders()
		{
			var docLCHeader = DocLandedCostHeader.New(LCHeader, Factory);
			AssertEquals("DocLCHeader.BaseEntryHeaders when not on a Declaration.", typeof(DocLandedCostHeader.EmptyDocCusEntryHeaderCollection), docLCHeader.BaseEntryHeaders.GetType());

			docLCHeader = DocLandedCostHeader.New(LCHeader, Factory);
			var declaration = Factory.New<BaseJobDeclaration>();
			LCHeader.LT_ParentID = declaration.PK;
			LCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			AssertNotEquals("DocLCHeader.BaseEntryHeaders when on a Declaration.", typeof(DocLandedCostHeader.EmptyDocCusEntryHeaderCollection), docLCHeader.BaseEntryHeaders.GetType());
		}

		public void TestDutyRatesIsNotNullAndIsOfRightType()
		{
			AssertEquals("DocLCHeader.DutyRates.GetType()", typeof(LandedCostDutyRateSummaryCollection), DocLCHeader.DutyRates.GetType());
		}

		public void TestBusinessObjectToLogAgainst()
		{
			AssertEquals(DummyHost, ((IBODocDataProvider)DocLCHeader).BusinessObjectToLogAgainst);
		}

		public void TestConsigneeName()
		{
			//return LCHeader.Consignee == null ? ZString.Empty : LCHeader.Consignee.OH_FullName;
			OrgHeader consignee = OrgHeader.New(Factory);
			consignee.OH_FullName = "Test";
			DummyHost.ConsigneeExposed = consignee;
			AssertEquals("Test", DocLCHeader.ConsigneeName);
		}

		public void TestTotalCustomsDisbursementChargesAndLinePrice()
		{
			DutyTaxEntryFee total = new DutyTaxEntryFee();
			total["TDT"] = 82m;
			total["ST1"] = 22m;
			total["ST2"] = 42m;
			total["ST3"] = 62m;
			total["ENT"] = 3m;
			total["QUA"] = 10m;
			DummyHost.TotalDutyTaxEntryFeeItemsExposed = total;

			AssertEquals("TotalDuties", 82.00m, DocLCHeader.DocCustomsDisbursementCharges["TDT"].Amount);
			AssertEquals("SpecialTax1", 22.00m, DocLCHeader.DocCustomsDisbursementCharges["ST1"].Amount);
			AssertEquals("SpecialTax2", 42.00m, DocLCHeader.DocCustomsDisbursementCharges["ST2"].Amount);
			AssertEquals("SpecialTax3", 62.00m, DocLCHeader.DocCustomsDisbursementCharges["ST3"].Amount);
			AssertEquals("QuarantineFee", 10.00m, DocLCHeader.DocCustomsDisbursementCharges["QUA"].Amount);
			AssertEquals("Total Duties & Taxes & Entry Fees & Quarantine Fee", 221.00m, DocLCHeader.TotalCustomsDisbursementCharges);

			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee1.CostInLocalCurrencyExposed = 100m;

			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee2.CostInLocalCurrencyExposed = 50m;

			DummyIUltimateDistributee dummyDistributee3 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee3.CostInLocalCurrencyExposed = 1.23m;

			DummyHost.UltimateDistributeesExposed = new IUltimateDistributee[] { dummyDistributee1, dummyDistributee2, dummyDistributee3 };
			LandedCostHistory lCHistory1 = LCHeader.Histories.AddNew();
			LandedCostHistory lCHistory2 = LCHeader.Histories.AddNew();
			LandedCostHistory lCHistory3 = LCHeader.Histories.AddNew();

			lCHistory1.UltimateDistributee = dummyDistributee1;
			lCHistory2.UltimateDistributee = dummyDistributee2;
			lCHistory3.UltimateDistributee = dummyDistributee3;

			AssertEquals("TotalInvoiceCostInLocalCurrency", 151.23m, DocLCHeader.TotalInvoiceCost);

			var testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice1 = testDec.Invoices.AddNew();
			BaseJobComInvoiceHeader invoice2 = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			lCHistory1.LH_ParentID = invoiceLine1.PK;
			lCHistory1.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			lCHistory1.UltimateDistributee = invoiceLine1;

			BaseJobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			lCHistory2.LH_ParentID = invoiceLine2.PK;
			lCHistory2.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			lCHistory2.UltimateDistributee = invoiceLine2;

			BaseJobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			lCHistory3.LH_ParentID = invoiceLine3.PK;
			lCHistory3.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			lCHistory3.UltimateDistributee = invoiceLine3;

			invoiceLine1.JI_LinePrice = 123.45m;
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine3.JI_LinePrice = 400m;

			AssertEquals("TotalInvoicePriceInInvoiceCurrency", 723.45m, DocLCHeader.TotalLinePrice);
		}

		public void TestMultiInvoices()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Invoices.AddNew();

			LandedCostHeader landedCostHeader = LandedCostHeader.New(declaration);
			DocLandedCostHeader docLCHeader = DocLandedCostHeader.New(landedCostHeader, Factory);
			AssertEquals("N", docLCHeader.MultipleInvoices);

			declaration.Invoices.AddNew();
			AssertEquals("Y", docLCHeader.MultipleInvoices);
		}

		public void TestTotalCIFandJobDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			BaseJobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			BaseInvoiceCharge fIFT = invoiceHeader.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 200m;
			fIFT.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			fIFT.J7_IsDutiable = false;
			fIFT.J7_IsIncludedInITOT = true;

			BaseInvoiceCharge oFT = invoiceHeader.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 500m;
			oFT.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;

			LandedCostHeader landedCostHeader = LandedCostHeader.New(declaration);

			LandedCostHistory lCHistory1 = landedCostHeader.Histories.AddNew();
			lCHistory1.SetLineValue(1000m, "TDT");
			lCHistory1.SetLineValue(20m, "ST1");
			lCHistory1.LH_LandedCostGroup1 = 500m;
			lCHistory1.LH_ParentID = invoiceLine.PK;
			lCHistory1.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			//LCHistory1.LH_LandedCostMarginPercent1 = 10;

			LandedCostHistory lCHistory2 = landedCostHeader.Histories.AddNew();
			lCHistory2.SetLineValue(2000m, "TDT");
			lCHistory2.SetLineValue(20m, "ST1");
			lCHistory2.LH_LandedCostGroup1 = 500m;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 20000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20000m;

			fIFT = invoiceHeader.Charges.AddNew();
			fIFT.J7_ChargeType = CustomsChargeTypeList.Codes.ForeignInlandFreight;
			fIFT.J7_Amount = 300m;
			fIFT.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
			fIFT.J7_IsDutiable = false;
			fIFT.J7_IsIncludedInITOT = true;

			oFT = invoiceHeader.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oFT.J7_Amount = 600m;
			oFT.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;

			lCHistory2.LH_ParentID = invoiceLine.PK;
			lCHistory2.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			//LCHistory2.LH_LandedCostMarginPercent1 = 20;

			SendsMessagesToCustomsShutterUpperer mergeResult = new SendsMessagesToCustomsShutterUpperer(false);
			mergeResult.AnswerToContinueWithAction = true;
			declaration.DoMerge(mergeResult);

			DocLandedCostHeader docLCHeader = DocLandedCostHeader.New(landedCostHeader, Factory);
			AssertEquals("Total CIF", 30600m, docLCHeader.TotalCIFAmount);
			AssertEquals("total CIF in local currency", 30600m, docLCHeader.TotalCIFLocalAmount);
			AssertEquals("Parent declaration is exposed", declaration.PK, ((BaseJobDeclaration)docLCHeader.JobDeclaration.WrappedObject).PK);
		}

		public void TestTotals()
		{
			LandCostInput lCInput1 = LCHeader.CostInputs.AddNew();
			lCInput1.LI_LandedCostGroup = 1;
			lCInput1.LI_CostAmount = 100.027m;
			lCInput1.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput lCInput2 = LCHeader.CostInputs.AddNew();
			lCInput2.LI_LandedCostGroup = 2;
			lCInput2.LI_CostAmount = 200.089m;
			lCInput2.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput lCInput3 = LCHeader.CostInputs.AddNew();
			lCInput3.LI_LandedCostGroup = 3;
			lCInput3.LI_CostAmount = 300.024m;
			lCInput3.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput lCInput4 = LCHeader.CostInputs.AddNew();
			lCInput4.LI_LandedCostGroup = 4;
			lCInput4.LI_CostAmount = 400.054m;
			lCInput4.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput lCInput5 = LCHeader.CostInputs.AddNew();
			lCInput5.LI_LandedCostGroup = 5;
			lCInput5.LI_CostAmount = 500.037m;
			lCInput5.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput lCInput6 = LCHeader.CostInputs.AddNew();
			lCInput6.LI_LandedCostGroup = 6;
			lCInput6.LI_CostAmount = 600.871m;
			lCInput6.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			LandCostInput lCInputMisc = LCHeader.CostInputs.AddNew();
			lCInputMisc.LI_LandedCostGroup = 7;
			lCInputMisc.LI_CostAmount = 700.281m;
			lCInputMisc.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AssertEquals("GroupTotal", 2801.38m, DocLCHeader.TotalLandingCost);
			AssertEquals("GroupTotal", 100.03m, DocLCHeader.TotalGroup1);
			AssertEquals("GroupTotal", 200.09m, DocLCHeader.TotalGroup2);
			AssertEquals("GroupTotal", 300.02m, DocLCHeader.TotalGroup3);
			AssertEquals("GroupTotal", 400.05m, DocLCHeader.TotalGroup4);
			AssertEquals("GroupTotal", 500.04m, DocLCHeader.TotalGroup5);
			AssertEquals("GroupTotal", 600.87m, DocLCHeader.TotalGroup6);
			AssertEquals("GroupTotal", 700.28m, DocLCHeader.TotalGroupMisc);
		}

		public void TestCostInputs()
		{
			AssertNotNull("Cost Inputs Collection", DocLCHeader.CostInputs);
		}

		public void TestHistories()
		{
			AssertNotNull("Histories Collection", DocLCHeader.Histories);
		}

		public void TestNew()
		{
			AssertNull("Create with Null", DocLandedCostHeader.New(null, Factory));
			AssertNotNull("Create with a valid object", DocLCHeader);
		}

		public void TestDateOfProcessing()
		{
			LCHeader.LT_DateOfProcessing = new ZDateTime(2005, 10, 25);
			AssertEquals("Date of Processing", LCHeader.LT_DateOfProcessing, DocLCHeader.DateOfProcessing);
		}

		public void TestUniqueReferenceNumber()
		{
			var dummyHost = Factory.New<DummyLandedCostHeader>();
			dummyHost.UniqueReferenceNumberExposed = "XC0000nnn";

			LCHeader.LT_ParentID = dummyHost.PK;
			LCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertEquals("UniqueReference Number", dummyHost.UniqueReferenceNumber, DocLCHeader.UniqueReferenceNumber);
		}

		public void TestLandedCostGroupLabels()
		{
			var mockLCHeader = Factory.NewMoq<LandedCostHeader>();
			mockLCHeader.Setup(m => m.LandedCostGroup1Label).Returns(new ZString("GROUP1"));
			mockLCHeader.Setup(m => m.LandedCostGroup2Label).Returns(new ZString("GROUP2"));
			mockLCHeader.Setup(m => m.LandedCostGroup3Label).Returns(new ZString("GROUP3"));
			mockLCHeader.Setup(m => m.LandedCostGroup4Label).Returns(new ZString("GROUP4"));
			mockLCHeader.Setup(m => m.LandedCostGroup5Label).Returns(new ZString("GROUP5"));
			mockLCHeader.Setup(m => m.LandedCostGroup6Label).Returns(new ZString("GROUP6"));

			var docLCHeader = DocLandedCostHeader.New(mockLCHeader.Object, Factory);
			AssertEquals("LCGroup1Label", "GROUP1", docLCHeader.LandedCostGroup1Label);
			AssertEquals("LCGroup2Label", "GROUP2", docLCHeader.LandedCostGroup2Label);
			AssertEquals("LCGroup3Label", "GROUP3", docLCHeader.LandedCostGroup3Label);
			AssertEquals("LCGroup4Label", "GROUP4", docLCHeader.LandedCostGroup4Label);
			AssertEquals("LCGroup5Label", "GROUP5", docLCHeader.LandedCostGroup5Label);
			AssertEquals("LCGroup6Label", "GROUP6", docLCHeader.LandedCostGroup6Label);
		}

		public void TestLandedCostGroupMiscLabel()
		{
			AssertEquals("LC Misc Group", "Misc Charges", DocLCHeader.LandedCostGroupMiscLabel);
		}

		public void TestDocCustomsDisbursementCharges()
		{
			using (LCHeader.Company.TemporarilySetCountry("CA"))
			{
				DummyHost.TotalDutyTaxEntryFeeItemsExposed["TDT"] = 10m;
				DummyHost.TotalDutyTaxEntryFeeItemsExposed["OTH"] = 20m;

				AssertEquals("DocCustomsDisbursementCharges.Count", 3, DocLCHeader.DocCustomsDisbursementCharges.Count);

				var duty = DocLCHeader.DocCustomsDisbursementCharges["TDT"];
				AssertEquals("TDT Amount", 10m, duty.Amount);
				AssertEquals("TDT Label", "Total Duty", duty.Label);
				AssertEquals("TDT DocumentCustomLabelCode", "DutyAmount", duty.DocumentCustomLabelCode);

				var otherDuty = DocLCHeader.DocCustomsDisbursementCharges["OTH"];
				AssertEquals("OTH Amount", 20m, otherDuty.Amount);
				AssertEquals("OTH Label", "OTH Duty", otherDuty.Label);
				AssertEquals("OTH DocumentCustomLabelCode", "FlatOrOtherDutyAmount", otherDuty.DocumentCustomLabelCode);

				var excise = DocLCHeader.DocCustomsDisbursementCharges["EXC"];
				AssertEquals("EXC Amount", 0m, excise.Amount);
				AssertEquals("EXC Label", "Excise", excise.Label);
				AssertEquals("EXC DocumentCustomLabelCode", "ExciseTax", excise.DocumentCustomLabelCode);
			}
		}

		public void TestDisclaimer()
		{
			DocumentsDataRegistry.Instance.LandedCostingClosingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid(), "Testing");
			AssertEquals("Disclaimer in the registry has not been read correctly", "Testing", DocLCHeader.Disclaimer);
		}

		public void TestLandedCostPercentageLabel()
		{
			var lCHeaderMock = Factory.NewMoq<LandedCostHeader>();
			lCHeaderMock.Setup(m => m.LandedCostPercentageLabel).Returns(new ZString("Total Import Cost %"));
			var docLCHeader = DocLandedCostHeader.New(lCHeaderMock.Object, Factory);
			AssertEquals("Total Import Cost %", docLCHeader.LandedCostPercentageLabel);
			AssertEquals("Total Cost %", docLCHeader.LandedTotalCostPercentageLabel);
		}

		public void TestHistoriesAreSorted()
		{
			var dummyHost = Factory.New<DummyLandedCostHeader>();
			dummyHost.LineComparerExposed = new DistributeeComparerTest.DummyLineComparer();

			DummyIUltimateDistributee dummyDistributee1 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee1.HumanReadableCodeExposed = "ZZZ";

			DummyIUltimateDistributee dummyDistributee2 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee2.HumanReadableCodeExposed = "AAA";

			DummyIUltimateDistributee dummyDistributee3 = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee3.HumanReadableCodeExposed = "CCC";

			var lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHost.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LandedCostHistory lCLine1 = lCHeader.Histories.AddNew();
			lCLine1.UltimateDistributee = dummyDistributee1;

			LandedCostHistory lCLine2 = lCHeader.Histories.AddNew();
			lCLine2.UltimateDistributee = dummyDistributee2;

			LandedCostHistory lCLine3 = lCHeader.Histories.AddNew();
			lCLine3.UltimateDistributee = dummyDistributee3;

			DocLandedCostHeader docLCHeader = DocLandedCostHeader.New(lCHeader, Factory);
			AssertEquals("First DocLCHistory after sorted", lCLine2, docLCHeader.Histories[0].LCHistory);
			AssertEquals("Second DocLCHistory after sorted", lCLine3, docLCHeader.Histories[1].LCHistory);
			AssertEquals("Third DocLCHistory after sorted", lCLine1, docLCHeader.Histories[2].LCHistory);
		}

		#region Implementation

		string InitialLandedCostingClosingText;
		protected override void SetUp()
		{
			base.SetUp();
			InitialLandedCostingClosingText = DocumentsDataRegistry.Instance.LandedCostingClosingText.Value;
		}

		protected override void TearDown()
		{
			DocumentsDataRegistry.Instance.LandedCostingClosingText.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, GlbDepartment.CurrentDepartment.PK.ToGuid(), InitialLandedCostingClosingText);
			base.TearDown();
		}

		DummyLandedCostHeader DummyHost
		{
			get
			{
				if (fDummyHost == null)
				{
					fDummyHost = Factory.New<DummyLandedCostHeader>();
				}
				return fDummyHost;
			}
		}
		DummyLandedCostHeader fDummyHost;

		LandedCostHeader LCHeader
		{
			get
			{
				if (fLCHeader == null)
				{
					fLCHeader = Factory.New<LandedCostHeader>();
					fLCHeader.LT_ParentID = DummyHost.PK;
					fLCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
				}
				return fLCHeader;
			}
		}
		LandedCostHeader fLCHeader;

		DocLandedCostHeader DocLCHeader
		{
			get
			{
				if (fDocLCHeader == null)
				{
					fDocLCHeader = DocLandedCostHeader.New(LCHeader, Factory);
				}
				return fDocLCHeader;
			}
		}
		DocLandedCostHeader fDocLCHeader;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocLCHeader };
		}

		#endregion
	}
}
