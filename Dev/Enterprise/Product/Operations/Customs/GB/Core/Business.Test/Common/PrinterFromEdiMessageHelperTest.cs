using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	class PrinterFromEdiMessageHelperTest : TestCaseWithFactory
	{
		public void TestCanFindCorrectPrinter_CCSUK()
		{
			RunPrinterTest(GBCustomsDataRegistry.Instance.PrinterCcsuk);
		}

		public void TestCanFindCorrectPrinter_Chief()
		{
			RunPrinterTest(GBCustomsDataRegistry.Instance.PrinterChief);
		}

		void RunPrinterTest(IRegistryItem rego)
		{
			var documentSupporter = Factory.New<JobDeclaration>();
			var printerSharedComp1 = Factory.New<StmPrintQueue>();
			printerSharedComp1.SQ_QueueName = "Shared1X";
			var printerApplicationCompanyComp1 = Factory.New<StmPrintQueue>();
			printerApplicationCompanyComp1.SQ_QueueName = "CCSUK companyX";
			var printerApplicationBranchBranch12 = Factory.New<StmPrintQueue>();
			printerApplicationBranchBranch12.SQ_QueueName = "CCSUK branchX";
			var printerSharedComp2 = Factory.New<StmPrintQueue>();
			printerSharedComp2.SQ_QueueName = "Shared2X";
			Factory.Save();

			var company1 = Factory.New<GlbCompany>();
			var branch11 = company1.Branches.AddNew();
			branch11.GB_Code = "11";
			branch11.GB_RL_NKHomePort = "GBLHR";
			var branch12 = company1.Branches.AddNew();
			branch12.GB_Code = "12";
			branch12.GB_RL_NKHomePort = "GBLHR";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "C2";
			company2.GC_RN_NKCountryCode = "GB";
			var branch21 = company2.Branches.AddNew();
			branch21.GB_RL_NKHomePort = "GBLHR";
			branch21.GB_Code = "21";
			Factory.Save();

			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, printerSharedComp1.PK.ToGuid());
			GBCustomsDataRegistry.Instance.PrinterShared.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, printerSharedComp2.PK.ToGuid());
			rego.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, printerApplicationCompanyComp1.PK.ToGuid());
			rego.SetValue(Guid.Empty, branch12.PK.ToGuid(), Guid.Empty, printerApplicationBranchBranch12.PK.ToGuid());

			var docCommand = Factory.LoadTop1<StmMenuItem>(new ZQuery());

			var message = Factory.New<GbEDIMessage>();
			message.EM_GB = branch11.PK;
			var printerHelper = new PrinterFromEdiMessageHelperForTest(message.Factory);
			var selectedPrinter = printerHelper.GetPrinterFor_PrintToEdocsAndPaper(docCommand.PK, message, documentSupporter, rego, false);
			AssertEquals("Branch 11 does not have its own printer, see parent company's", printerApplicationCompanyComp1.PK, selectedPrinter);

			message.EM_GB = branch12.PK;
			selectedPrinter = printerHelper.GetPrinterFor_PrintToEdocsAndPaper(docCommand.PK, message, documentSupporter, rego, false);
			AssertEquals("This branch has its own printer defined", printerApplicationBranchBranch12.PK, selectedPrinter);

			message.EM_GB = branch21.PK;
			selectedPrinter = printerHelper.GetPrinterFor_PrintToEdocsAndPaper(docCommand.PK, message, documentSupporter, rego, false);
			AssertEquals("Branch 21 has no printer of its own, see parent's", printerSharedComp2.PK, selectedPrinter);
		}

		class PrinterFromEdiMessageHelperForTest : PrinterFromEdiMessageHelper
		{
			public PrinterFromEdiMessageHelperForTest(BusinessObjectFactory businessObjectFactory)
				: base(businessObjectFactory, null)
			{
			}

			public ZGuid GetPrinterFor_PrintToEdocsAndPaper(ZGuid guidOfMenuItemYouWantToPrint, IBranchProvider branchProviderForDeterminingPrinter, IDocumentSupportable documentSupportableBizOFromWhichEdocWillHang,
				IRegistryItem registryForPaperPrinter, bool printToEdocsToo = true)
			{
				base.PrintToEdocsAndPaper(guidOfMenuItemYouWantToPrint, branchProviderForDeterminingPrinter, documentSupportableBizOFromWhichEdocWillHang, registryForPaperPrinter, printToEdocsToo);
				return base.pkOfSelectedPaperPrinter;
			}
		}
	}
}
