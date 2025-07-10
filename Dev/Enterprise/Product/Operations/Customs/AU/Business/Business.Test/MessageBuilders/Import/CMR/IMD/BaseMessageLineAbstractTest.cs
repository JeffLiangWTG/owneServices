using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSDEC;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class BaseMessageLineAbstractTest : TestCaseWithFactory
	{
		public abstract void TestPopulate();

		public void TestBasePopulateFTX()
		{
			invoiceLine.JI_Description = "TEST DESCRIPTION";
			messageLineToTest.PopulateFTX();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Goods Description", true, result.Contains("FTX+AAA+++TEST DESCRIPTION'"));
		}

		public void TestBasePopulateMEA()
		{
			invoiceLine.JI_CustomsUnitQty = "NO";
			invoiceLine.JI_CustomsQuantity = 123M;
			messageLineToTest.PopulateMEA();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Customs QTY & UQ", true, result.Contains("MEA+AAA++NO:123.00000'"));

			invoiceLine.AddInfo.ZA_QT2 = 543M;
			invoiceLine.AddInfo.ZA_UQ2 = "LA";
			messageLineToTest.PopulateMEA();
			result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Second Customs QTY & UQ", true, result.Contains("MEA+AAA++LA:543.00000'"));
		}

		public void TestBasePopulateMEAWith5Decimals()
		{
			invoiceLine.JI_CustomsUnitQty = "T";
			invoiceLine.JI_CustomsQuantity = 0.00311M;
			messageLineToTest.PopulateMEA();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Customs QTY & UQ", true, result.Contains("MEA+AAA++T:0.00311'"));
		}

		public abstract void TestSegmentGroup33();

		public void TestBasePopulateGroup35()
		{
			invoiceLine.JI_Tariff = "2003.30.40 05";
			invoiceLine.AddInfo.ZA_GSTE = "GSTE";
			invoiceLine.AddInfo.ZA_WETE = "WETE";
			messageLineToTest.PopulateGroup35();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Tariff Code", true, result.Contains("RFF+ABD:20033040'"));
			AssertEquals("Statistical Code", true, result.Contains("RFF+AED:05'"));
			AssertEquals("GST Exemption Code", true, result.Contains("RFF+ASA:GSTE"));
			AssertEquals("Wine Equalisation Tax Exemption Code", true, result.Contains("RFF+DA:WETE'"));
		}

		public void TestGSTEValueInGroup35()
		{
			invoiceHeader.AddInfo.ZA_GSTE = "GSTE";
			messageLineToTest.PopulateGroup35();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("GST Exemption Code", true, result.Contains("RFF+ASA:GSTE"));

			invoiceLine.AddInfo.ZA_GSTE = "INV";
			messageLineToTest.PopulateGroup35();
			result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("GST Exemption Code", true, result.Contains("RFF+ASA:INV"));
		}

		public void TestStatCode()
		{
			invoiceLine.JI_Tariff = "4901.99.90 5";
			messageLineToTest.PopulateGroup35();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());

			AssertEquals("Tariff Code", true, result.Contains("RFF+ABD:49019990'"));
			AssertEquals("Statistical Code", true, result.Contains("RFF+AED:05'"));
		}

		public void TestBasePopulateGroup40()
		{
			invoiceLine.AddInfo.ZA_WETQ = "Y";
			messageLineToTest.PopulateGroup40();
			var result = messageLineToTest.Group30.ToString(new Edifact.UNOCCMRCharacterSet());
			AssertEquals("Wine Equalisation Tax Quote Indicator", true, result.Contains("GIS+WET:109:95'"));
		}

		protected abstract BaseMessageLine GetMessageLineToTest { get; }

		protected abstract ZString Group30String { get; }

		protected override void SetUp()
		{
			SetUpDeclaration();
			base.SetUp();
		}

		void SetUpDeclaration()
		{
			testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			invoiceHeader = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var sender = new SendsMessagesToCustomsShutterUpperer();
			testDec.MessageInitiator = sender;
			testDec.DoMerge();

			entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];

			messageLineToTest = GetMessageLineToTest;
			messageLineToTest.Group30 = new SegmentGroup30();
		}

		protected JobDeclaration testDec;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryLine entryLine;
		protected BaseMessageLine messageLineToTest;
	}
}
