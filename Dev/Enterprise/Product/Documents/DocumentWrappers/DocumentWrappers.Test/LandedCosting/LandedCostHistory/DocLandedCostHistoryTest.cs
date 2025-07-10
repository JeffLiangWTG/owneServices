using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.LandedCosting.Business;
using Enterprise.LandedCosting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocLandedCostHistory))]
	sealed class DocLandedCostHistoryTest : DocumentWrapperTestCase
	{
		public void TestExGSTSellPrice1To4DP()
		{
			LCHistory.SetLineValue(1000m, "TDT");
			LCHistory.SetLineValue(20m, "ST1");
			LCHistory.SetLineValue(30m, "ST2");
			LCHistory.SetLineValue(50m, "ST3");

			LCHistory.LH_LandedCostGroup1 = 500m;
			LCHistory.LH_LandedCostGroup2 = 600m;

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 14;
			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			LCHistory.LH_LandedCostMarginPercent1 = 10;

			AssertEquals("LCHistory.SellPrice1ExGST", 958.57146m, LCHistory.SellPrice1ExGST); // sell price is ZDecimal
			AssertEquals("DocLCHistory.ExGSTSellPrice1To4DP", 958.5715m, DocLCHistory.ExGSTSellPrice1To4DP);
		}

		public void TestTotalCostWithMarkup1Applied()
		{
			LCHistory.SetLineValue(1000m, "TDT");
			LCHistory.SetLineValue(20m, "ST1");
			LCHistory.SetLineValue(30m, "ST2");
			LCHistory.SetLineValue(50m, "ST3");

			LCHistory.LH_LandedCostGroup1 = 500m;
			LCHistory.LH_LandedCostGroup2 = 600m;

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			LCHistory.LH_LandedCostMarginPercent1 = 10;
			AssertEquals("DocLCHistory.TotalCostWithMarkup1Applied", 13420.00m, DocLCHistory.TotalCostWithMarkup1Applied);
		}

		public void TestNew()
		{
			AssertNull("Created with null", DocLandedCostHistory.New(null, Factory));
			AssertNotNull("Created with a valid object", DocLCHistory);
		}

		public void TestUnitPriceInInvoiceCurrencyWithFourDecimals()
		{
			var testDec = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = (Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.LocalCurrency.PK))).RX_Code;
			invoice.JZ_InvoiceCurrLandedCostExRate = 0.5m;

			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;
			invoiceLine.JI_InvoiceQuantity = 30m;

			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			AssertEquals("Line price in invoice currency from wrapper", 100.00m, DocLCHistory.LinePriceInInvoiceCurrency);
			AssertEquals("Unit price in invoice currency from Invoice Line", 3.3333m, invoiceLine.UnitPrice, 0.01m);
			AssertEquals("Unit price in invoice currency from wrapper", 3.3333m, DocLCHistory.UnitPriceInInvoiceCurrency);
			AssertEquals("Unit price in local currency from wrapper", 6.6667m, DocLCHistory.UnitPriceInLocalCurrency);
		}

		public void TestProxiedProperties()
		{
			LCHistory[LandedCostHistorySchema.LH_DutyPercent.Name] = 100.585m;
			AssertEquals("DutyPercent", 100.585m, DocLCHistory["DutyPercent"]);

			AssertProxy(LandedLineCostType.Codes.LandedCostGroup1, "LandedCostGroup1");
			AssertProxy(LandedLineCostType.Codes.LandedCostGroup2, "LandedCostGroup2");
			AssertProxy(LandedLineCostType.Codes.LandedCostGroup3, "LandedCostGroup3");
			AssertProxy(LandedLineCostType.Codes.LandedCostGroup4, "LandedCostGroup4");
			AssertProxy(LandedLineCostType.Codes.LandedCostGroup5, "LandedCostGroup5");
			AssertProxy(LandedLineCostType.Codes.LandedCostGroup6, "LandedCostGroup6");
			AssertProxy(LandedLineCostType.Codes.LandedCostGroupMisc, "LandedCostGroupMisc");
		}

		void AssertProxy(string costTypeCode, string propertyNameInDoc)
		{
			LCHistory.SetLineValue(100.585m, costTypeCode);
			AssertEquals(propertyNameInDoc, 100.59m, DocLCHistory[propertyNameInDoc]);
			LCHistory.SetLineValue(100.584m, costTypeCode);
			AssertEquals(propertyNameInDoc, 100.58m, DocLCHistory[propertyNameInDoc]);
		}

		public void TestDocCustomsDisbursementCharges()
		{
			using (LCHistory.LCHeader.Company.TemporarilySetCountry("CA"))
			{
				LCHistory.SetLineValue(10m, "TDT");
				LCHistory.SetLineValue(20m, "OTH");

				AssertEquals("DocCustomsDisbursementCharges.Count", 3, DocLCHistory.DocCustomsDisbursementCharges.Count);

				var duty = DocLCHistory.DocCustomsDisbursementCharges["TDT"];
				AssertEquals("TDT Amount", 10m, duty.Amount);
				AssertEquals("TDT Label", "Total Duty", duty.Label);
				AssertEquals("TDT DocumentCustomLabelCode", "DutyAmount", duty.DocumentCustomLabelCode);

				var otherDuty = DocLCHistory.DocCustomsDisbursementCharges["OTH"];
				AssertEquals("OTH Amount", 20m, otherDuty.Amount);
				AssertEquals("OTH Label", "OTH Duty", otherDuty.Label);
				AssertEquals("OTH DocumentCustomLabelCode", "FlatOrOtherDutyAmount", otherDuty.DocumentCustomLabelCode);

				var excise = DocLCHistory.DocCustomsDisbursementCharges["EXC"];
				AssertEquals("EXC Amount", 0m, excise.Amount);
				AssertEquals("EXC Label", "Excise", excise.Label);
				AssertEquals("EXC DocumentCustomLabelCode", "ExciseTax", excise.DocumentCustomLabelCode);
			}
		}

		public void TestCustomsDisbursementCharges()
		{
			LCHistory.SetLineValue(1000m, "TDT");
			LCHistory.SetLineValue(20m, "ST1");
			LCHistory.SetLineValue(30m, "ST2");
			LCHistory.SetLineValue(40m, "ST3");
			LCHistory.SetLineValue(50m, "QUA");
			AssertEquals("Duties/Taxes/EntryFee/QuarantineFee", 1140.00m, DocLCHistory.CustomsDisbursementCharges);
		}

		public void TestUnitCustomsDisbursementCharges()
		{
			LCHistory.SetLineValue(1000m, "TDT");
			LCHistory.SetLineValue(20m, "ST1");
			LCHistory.SetLineValue(30m, "ST2");
			LCHistory.SetLineValue(50m, "ST3");
			LCHistory.SetLineValue(60m, "QUA");

			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_InvoiceQuantity = 11m;
			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			AssertEquals("PerUnitCustomsDisbursementCharges", 105.4545m, DocLCHistory.PerUnitCustomsDisbursementCharges);
		}

		public void TestPerUnitLandingCost()
		{
			LCHistory.LH_LandedCostGroup1 = 500m;
			LCHistory.LH_LandedCostGroup2 = 1500m;

			var invoiceLine = Factory.New<BaseJobComInvoiceLine>();
			invoiceLine.JI_InvoiceQuantity = 10m;
			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			AssertEquals("Unit Duties and Taxed shown in four decimals", 200.0000m, DocLCHistory.PerUnitLandingCost);
		}

		public void TestTotalCostThisLineAndLineLandedCost()
		{
			LCHistory.SetLineValue(1000m, "TDT");
			LCHistory.SetLineValue(20m, "ST1");
			LCHistory.SetLineValue(30m, "ST2");
			LCHistory.SetLineValue(50m, "ST3");

			LCHistory.LH_LandedCostGroup1 = 500m;
			LCHistory.LH_LandedCostGroup2 = 600m;

			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			AssertEquals("Total Cost this line", 12200.00m, DocLCHistory.TotalCost);

			invoiceLine.JI_InvoiceQuantity = 122m;
			AssertEquals("LineLandedCost shown in four decimals", 100.0000m, DocLCHistory.PerUnitTotalCost);
		}

		public void TestCustomsValue()
		{
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee.CustomsValueExposed = 1000.595m;

			LCHistory.UltimateDistributee = dummyDistributee;
			AssertEquals("Customs Value", 1000.60m, DocLCHistory.CustomsValue);
		}

		public void TestEntryGSTVATAmount()
		{
			DummyIUltimateDistributee dummyDistributee = Factory.New<DummyIUltimateDistributee>();
			dummyDistributee.GSTVATAmountExposed = 10.595m;

			LCHistory.UltimateDistributee = dummyDistributee;
			AssertEquals("EntryGSTVATAmount", 10.60m, DocLCHistory.EntryGSTVATAmount);
		}

		public void TestAggregatedFieldsAddRoundedResult()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 2;
			invoiceLine.JI_InvoiceUQ = "NO";
			LCHistory.LH_ParentID = invoiceLine.PK;
			LCHistory.LH_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			LCHistory.SetLineValue(200.033m, "TDT");//200.03
			LCHistory.SetLineValue(10.242m, "ST1");//10.24
			AssertEquals("PreCondition:TotalDuty", 200.03m, DocLCHistory.DocCustomsDisbursementCharges["TDT"].Amount);
			AssertEquals("PreCondition:SpecialTax1", 10.24m, DocLCHistory.DocCustomsDisbursementCharges["ST1"].Amount);
			AssertEquals("CustomsDisbursementCharges is aggregated after each item is rounded", 210.27m, DocLCHistory.CustomsDisbursementCharges);
			AssertEquals("PerUnitCustomsDisbursementCharges is aggregated after each item is rounded", 105.1350m, DocLCHistory.PerUnitCustomsDisbursementCharges);

			LCHistory.LH_LandedCostGroup1 = 200.033m;
			LCHistory.LH_LandedCostGroup2 = 10.242m;
			AssertEquals("PreCondition:LandedCostGroup1", 200.03m, DocLCHistory.LandedCostGroup1);
			AssertEquals("PreCondition:LandedCostGroup1", 10.24m, DocLCHistory.LandedCostGroup2);
			AssertEquals("TotalLandingCost is aggregated after each item is rounded", 210.27m, DocLCHistory.TotalLandingCost);
			AssertEquals("PerUnitTotalLandingCost is aggregated after each item is rounded", 105.1350m, DocLCHistory.PerUnitLandingCost);

			LCHistory.SetLineValue(200.033m, "TDT");
			LCHistory.SetLineValue(0m, "ST1");
			LCHistory.LH_LandedCostGroup1 = 0m;
			LCHistory.LH_LandedCostGroup2 = 10.242m;
			AssertEquals("TotalCost is aggregated after each item is rounded", 10210.27m, DocLCHistory.TotalCost);
			AssertEquals("PerUnitTotalCost is aggregated after each item is rounded", 5105.1350m, DocLCHistory.PerUnitTotalCost);
		}

		public void TestLandedCostPercentage()
		{
			var lCHistoryMock = Factory.NewMoq<LandedCostHistory>();
			lCHistoryMock.Setup(m => m.LandedCostPercentage).Returns(new ZDecimal(1050.116m));
			LandedCostHistory lCHistory = lCHistoryMock.Object;
			DocLandedCostHistory docLCHistory = DocLandedCostHistory.New(lCHistory, Factory);
			AssertEquals(1050.12m, docLCHistory.LandedCostPercentage);
		}

		#region Implementation

		LandedCostHeader LCHeader
		{
			get
			{
				if (fLCHeader == null)
				{
					fLCHeader = Factory.New<LandedCostHeader>();
				}
				return fLCHeader;
			}
		}
		LandedCostHeader fLCHeader;

		LandedCostHistory LCHistory
		{
			get
			{
				if (fLCHistory == null)
				{
					fLCHistory = LCHeader.Histories.AddNew();
				}
				return fLCHistory;
			}
		}
		LandedCostHistory fLCHistory;

		DocLandedCostHistory DocLCHistory
		{
			get
			{
				if (fDocLCHistory == null)
				{
					fDocLCHistory = DocLandedCostHistory.New(LCHistory, Factory);
				}
				return fDocLCHistory;
			}
		}
		DocLandedCostHistory fDocLCHistory;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocLCHistory };
		}

		#endregion
	}
}
