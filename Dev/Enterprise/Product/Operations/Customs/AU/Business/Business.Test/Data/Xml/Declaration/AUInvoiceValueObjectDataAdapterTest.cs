using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUInvoiceValueObjectDataAdapter))]
	class AUInvoiceValueObjectDataAdapterTest : DataTransfer.Testing.InvoiceValueObjectDataAdapterTest
	{
		public void TestSetInvoiceHeaderAdditionalInfo()
		{
			ToolTest.TestSetInvoiceHeaderAdditionalInfo();
		}

		public void TestSetInvoiceLinesAdditionalInfo()
		{
			ToolTest.TestSetInvoiceLinesAdditionalInfo();
		}

		public void TestSetXmlInvoiceHeaderAdditionalInfo()
		{
			ToolTest.TestSetXmlInvoiceHeaderAdditionalInfo();
		}

		public void TestSetXmlInvoiceLinesAdditionalInfo()
		{
			ToolTest.TestSetXmlInvoiceLinesAdditionalInfo();
		}

		public void TestAUSetInvoiceLineSummary()
		{
			ToolTest.TestAUSetInvoiceLineSummary();
		}

		public void TestAUSetInvoiceLineDetail()
		{
			ToolTest.TestAUSetInvoiceLineDetail();
		}

		public void TestAUSetInvoiceLineDetail_Bond()
		{
			ToolTest.TestAUSetInvoiceLineDetail_Bond();
		}

		public void TestNewBusinessObject()
		{
			ToolTest.TestNewBusinessObject();
		}

		public void TestSetXmlInvoiceLineDetails_ClassInfo()
		{
			ToolTest.TestSetXmlInvoiceLineDetails_ClassInfo();
		}

		public void TestSetXmlInvoiceLineDetails_Bond()
		{
			ToolTest.TestSetXmlInvoiceLineDetails_Bond();
		}

		protected override ZInt GSTVATRate
		{
			get
			{
				return 10;
			}
		}

		protected override void TestExportLandedCostingValuesCore()
		{
			var localCurrencyConstantCode = Enterprise.Core.Constants.CurrencyCodes.Australia;
			JobComInvoiceLine invoiceLine = (JobComInvoiceLine)SetupLandedCostHistoryForInvoiceLine();
			NotificationBuffer notify = new NotificationBuffer();
			Xsd.InvoiceHeader xmlInvoiceHeader = InvoiceDataAdapter.ExportToValueObject(invoiceLine.InvoiceHeader, new ValueObjectExportContext(notify));
			Xsd.InvoiceLine xmlInvoiceLine = xmlInvoiceHeader.InvoiceLines[0];
			Xsd.LandedCostingInfo landedCostingXml = xmlInvoiceLine.LandedCosting;
			AssertEquals("LandedCosting is specified", true, xmlInvoiceLine.LandedCosting.IsSpecified);
			AssertEquals("Invoice Line Type", "ACT", landedCostingXml.LineType);
			AssertEquals("Excise shouldn't be specified", false, landedCostingXml.Excise.IsSpecified);
			AsserteFinancialValue("Total Cost Per Unit", landedCostingXml.TotalCostPerUnit, 118.38m, localCurrencyConstantCode);
			AsserteFinancialValue("Unit Price in Local Currency", landedCostingXml.UnitPriceInLocalCurrency, 100m, localCurrencyConstantCode);
			AsserteFinancialValue("Duty And Taxes Per Unit", landedCostingXml.DutiesAndTaxesPerUnit, 3.38m, localCurrencyConstantCode);
			AsserteFinancialValue("Entry Fees", landedCostingXml.EntryFees, 20m, localCurrencyConstantCode);
			AsserteFinancialValue("Landing Cost Per Unit", landedCostingXml.LandingCostPerUnit, 15m, localCurrencyConstantCode);
			AssertEquals("selldetails is specified", true, landedCostingXml.SellDetails.IsSpecified);
			AssertEquals("lcGroupCharges is specified", true, landedCostingXml.LCGroupCharges.IsSpecified);
			AssertEquals("lcMisc is not specified", false, landedCostingXml.LCMisc.IsSpecified);
			//group charge
			Assert("Group 1", AssertGroupCharges(1, landedCostingXml.LCGroupCharges, 100m, false, localCurrencyConstantCode));
			Assert("Group 2", AssertGroupCharges(2, landedCostingXml.LCGroupCharges, 200m, false, localCurrencyConstantCode));
			Assert("Group 3", AssertGroupCharges(3, landedCostingXml.LCGroupCharges, 300m, false, localCurrencyConstantCode));
			Assert("Group 4", AssertGroupCharges(4, landedCostingXml.LCGroupCharges, 400m, false, localCurrencyConstantCode));
			Assert("Group 5", AssertGroupCharges(5, landedCostingXml.LCGroupCharges, 500m, false, localCurrencyConstantCode));
			Assert("Group 6", AssertGroupCharges(6, landedCostingXml.LCGroupCharges, 0m, true, localCurrencyConstantCode));
			//sell Details
			AssertEquals("Sell Details", true, AssertSellDetails(1, landedCostingXml.SellDetails, 127.85m, 140.64m, 8m, 8m, localCurrencyConstantCode));
			AssertEquals("Sell Details", true, AssertSellDetails(2, landedCostingXml.SellDetails, 129.03m, 141.93m, 9m, 9m, localCurrencyConstantCode));
			AssertEquals("Sell Details", true, AssertSellDetails(3, landedCostingXml.SellDetails, 130.22m, 143.24m, 10m, 10m, localCurrencyConstantCode));
			AssertEquals("Other Duty", false, landedCostingXml.OtherDuty.IsSpecified);
			//special Tax
			AssertEquals("Special Tax", true, AssertSpecialTax(1, landedCostingXml.SpecialTax, 5m, localCurrencyConstantCode));
			AssertEquals("Special Tax", true, AssertSpecialTax(2, landedCostingXml.SpecialTax, 6m, localCurrencyConstantCode));
			AssertEquals("Special Tax", true, AssertSpecialTax(3, landedCostingXml.SpecialTax, 7m, localCurrencyConstantCode));
		}

		protected override void AssertRightNumberOfAddCustomsDetailsAreGeneratedFromAnEmptyInvoiceHeader(int count)
		{
			AssertEquals("AddCustomsDetails.Count", 17, count);
		}

		#region Overrides for base test
		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			BaseJobComInvoiceHeader result = base.GetInvoiceHeader();
			result.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			result.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			return result;
		}

		protected override BaseCusClassification CreateTestCusClassification()
		{
			BaseCusClassification result = base.CreateTestCusClassification();
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			result.CC_TariffNum = TariffNumber;
			return result;
		}

		protected override ZString TariffNumber
		{
			get
			{
				return "9999.99.99 99";
			}
		}

		protected override ValueObjectDataAdapter<BaseJobComInvoiceHeader, Xsd.InvoiceHeader> GetNewBizObjXmlDataAdapter()
		{
			return new AUInvoiceValueObjectDataAdapter(Factory.New<BaseJobDeclaration>());
		}

		protected override BaseJobComInvoiceHeader NewBusinessObject()
		{
			JobDeclaration jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			return jobDec.Invoices.AddNew();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			string outputFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUEmptyInvoice.xml");
			return new BusinessObjectAndExpectedOutputFileName(GetEmptyInvoiceHeader(), outputFileFullPath, ValidationKind.None, "Empty Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			string outputFileFullPath = FileReader.ExtractEmbeddedResourceToFile(TestFilesPath, TempDir.DirectoryName, "AUPopulatedInvoice.xml");
			return new BusinessObjectAndExpectedOutputFileName(GetInvoiceHeaderWithTestData(), outputFileFullPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated Invoice");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return System.Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected new BaseJobComInvoiceHeader GetEmptyInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)NewBusinessObject();
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			return invoiceHeader;
		}

		protected new BaseJobComInvoiceHeader GetInvoiceHeaderWithTestData()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			organisation.OH_FullName = "Supplier";
			organisation.OH_IsConsignee = true;
			organisation.OH_IsConsignor = true;
			organisation.OH_RL_NKClosestPort = "AUMEL";
			OrgAddress address = organisation.Addresses.MainAddress;
			address.OA_Address1 = "7J1B7RFZ4WSH3LJTGUG7P55D1ABG3TMYMKFCPHQ2VSK4UDXQF0";
			organisation.OH_Code = "L1FZUT4BYOD8";
			JobComInvoiceHeader invoiceHeader = (JobComInvoiceHeader)NewBusinessObject();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Enterprise.Core.Constants.CurrencyCodes.Australia;
			invoiceHeader.JZ_OH_Supplier = organisation.PK;
			invoiceHeader.JZ_InvoiceNumber = "INVOICENUMBER";
			invoiceHeader.JZ_InvoiceAmount = 57;
			invoiceHeader.JZ_ValuationDateOverride = new ZDateTime(1998, 3, 4);
			invoiceHeader.JZ_Volume = 159;
			invoiceHeader.JZ_VolumeUQ = "Vo";
			invoiceHeader.JZ_Weight = 168;
			invoiceHeader.JZ_WeightUQ = "We";
			invoiceHeader.JZ_InvoiceDate = new ZDateTime(2005, 3, 14);
			invoiceHeader.JZ_IncoTerm = "Inc";
			invoiceHeader.AddInfo.ZA_ORG = "AU";
			invoiceHeader.AddInfo.ZA_VALB_Hidden = "TV";
			invoiceHeader.AddInfo.ZA_HeaderREL_Hidden = "N";
			InvoiceCharge invoiceCharge = invoiceHeader.Charges.AddNew();
			invoiceCharge.J7_ChargeType = "Cha";
			invoiceCharge.J7_Amount = 32;
			invoiceCharge.J7_RX_NKCurrency = "AUD";
			invoiceCharge.J7_IsDutiable = false;
			invoiceCharge.J7_IsGSTApplicable = false;
			invoiceCharge.J7_IsIncludedInITOT = false;
			return invoiceHeader;
		}

		#endregion
		#region Setup
		protected override IValueObjectDataAdapter GetInvoiceValueObjectDataAdapter()
		{
			return new AUInvoiceValueObjectDataAdapter(JobDec);
		}

		AUInvoiceValueObjectDataAdapter InvoiceDataAdapter
		{
			get
			{
				return (AUInvoiceValueObjectDataAdapter)invoiceDataAdapter;
			}
		}

		AUInvoiceDataTransferToolTest ToolTest
		{
			get
			{
				return fToolTest ?? (fToolTest = new AUInvoiceDataTransferToolTest(InvoiceDataAdapter, GetInvoiceHeader, Factory));
			}
		}
		AUInvoiceDataTransferToolTest fToolTest;

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(AUInvoiceValueObjectDataAdapterTest)));
		TestFileReader fileReader;

		TempDirectory TempDir => tempDir ?? (tempDir = new TempDirectory());
		TempDirectory tempDir;

		protected override void TearDown()
		{
			base.TearDown();
			tempDir?.Dispose();
		}

		const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.TestFiles";

		#endregion
	}
}
