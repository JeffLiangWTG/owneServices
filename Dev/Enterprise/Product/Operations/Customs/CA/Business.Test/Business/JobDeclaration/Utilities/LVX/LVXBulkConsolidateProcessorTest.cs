using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXBulkConsolidateProcessorTest : TestCaseWithFactory
	{
		#region TestBulkUpdate

		public void TestBulkUpdate_SingleDeclaration()
		{
			importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_FullName = "Importer2";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "GBB";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "GC2";
			branch3 = company2.Branches.AddNew();
			branch3.GB_Code = "GBC";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1111", "1111", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PA");

			var officeCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2222", "2222", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(officeCode2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province, "PB");

			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20001", companyPk: company1.PK.ToGuid());
			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20002", companyPk: company2.PK.ToGuid());
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20001");
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20002");
			Factory.Save();
			var log = new NotificationsLogWrapper(new Notifications());

			using (DisposableEnvironment.ForCompany("GC1"))
			{
				var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 4, importer2.PK, branch1.PK, "1111", "A");
				var declaration4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 4, importer1.PK, branch2.PK, "1111", "A");
				var declaration6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000006", 2015, 3, importer1.PK, branch1.PK, "2222", "A");
				var declaration7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2015, 3, importer1.PK, branch1.PK, "1111", "B");
				var declaration8 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000008", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
				var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration7.LVXInvoiceHeader, lvs1);
				Factory.Save();
				runner = new LVXJobsConsolidateRunner(log, Factory);

				var selectionCriteria = new LVXSelectionCriteriaBO(Factory);
				selectionCriteria.PeriodYear = 2015;
				selectionCriteria.PeriodMonth = 5;
				lVXDeclarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertEquals("No LVX declarations filtered out", 0, lVXDeclarationPKs.Count());

				var processor = new LVXBulkConsolidateProcessor(lVXDeclarationPKs);
				var dummybo = new DummyBusinessObjectForTest(processor);
				processor.StartProcessing();
				Assert(dummybo.messages.Contains("Process started, total count : 0"));
				AssertProcessCompleted(dummybo);

				selectionCriteria.PeriodMonth = 4;
				lVXDeclarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertEquals("No LVX declarations filtered out", 5, lVXDeclarationPKs.Count());
				dummybo.Reset();
				processor = new LVXBulkConsolidateProcessor(lVXDeclarationPKs);
				dummybo = new DummyBusinessObjectForTest(processor);
				processor.StartProcessing();
				Assert(dummybo.messages.Contains("Process started, total count : 5"));
				Assert(dummybo.messages.Contains("Process stopped by user, 4 records processed."));
				AssertProcessCompleted(dummybo);

				selectionCriteria.PeriodMonth = 3;
				lVXDeclarationPKs = runner.SelectLVXJobPKs(selectionCriteria);
				AssertEquals("No LVX declarations filtered out", 2, lVXDeclarationPKs.Count());
				dummybo.Reset();
				processor = new LVXBulkConsolidateProcessor(lVXDeclarationPKs);
				dummybo = new DummyBusinessObjectForTest(processor);
				processor.StartProcessing();
				Assert(dummybo.messages.Contains("Process started, total count : 2"));
				AssertProcessCompleted(dummybo);
			}
		}

		void AssertProcessCompleted(DummyBusinessObjectForTest dummybo)
		{
			Assert(dummybo.messages.Contains("Process completed."));
			AssertEquals(100, dummybo.processValue);
		}

		#endregion

		OrgHeader importer1;
		OrgHeader importer2;
		GlbBranch branch1;
		GlbBranch branch2;
		GlbBranch branch3;

		LVXJobsConsolidateRunner runner;
		IEnumerable<ZGuid> lVXDeclarationPKs;
	}
}
