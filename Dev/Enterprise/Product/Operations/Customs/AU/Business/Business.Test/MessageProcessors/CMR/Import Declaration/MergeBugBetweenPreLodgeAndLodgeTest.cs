using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class MergeBugBetweenPreLodgeAndLodgeTest : TestCaseWithFactory
	{
		public void TestProblemWithMergeNumbersOnPreLodge()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			declaration.JE_DeclarationReference = "B00122382";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.CustomsEntryHeaders[0].CH_BGMReference = "B00122382/1";

			AssertEquals("Line number", (ZShort)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			var iMDRMessage = declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.PreLodgementProcessing;
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);

			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0901.11.00 01";
			declaration.DoMerge();
			AssertEquals("Line number", (ZShort)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			AssertEquals("Line number", (ZShort)2, declaration.CustomsEntryHeaders[0].MergedLines[1].CL_LineNumber);
		}

		public void TestProblemWithMergeNumbersOnAmendments()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			declaration.JE_DeclarationReference = "B00122382";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.CustomsEntryHeaders[0].CH_BGMReference = "B00122382/1";

			AssertEquals("Line number", (ZShort)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			var iMDMessage = declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(CMRIMDMessage));
			iMDMessage.EM_MessageText = CMRImportDeclarationTestData.IMD;
			var iMDRMessage = declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(CMRIMDRMessage));
			iMDRMessage.EM_MessageText = CMRImportDeclarationTestData.IMRHeld;
			new IMDRMessageProcessor(new LoggingInformation()).ProcessMessage(iMDRMessage);
			var calculator = new IMDStatusCalculator(declaration.CustomsEntryHeaders[0]);
			calculator.DeriveStatusNow();

			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "0901.11.00 01";
			declaration.DoMerge();
			AssertEquals("Line number", (ZShort)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			AssertEquals("Line number", (ZShort)2, declaration.CustomsEntryHeaders[0].MergedLines[1].CL_LineNumber);

			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "4901.10.00 01";
			declaration.DoMerge();
			AssertEquals("Line number", (ZShort)1, declaration.CustomsEntryHeaders[0].MergedLines[0].CL_LineNumber);
			AssertEquals("Line number", (ZShort)2, declaration.CustomsEntryHeaders[0].MergedLines[1].CL_LineNumber);
			AssertEquals("Line number", (ZShort)3, declaration.CustomsEntryHeaders[0].MergedLines[2].CL_LineNumber);
		}
	}
}
