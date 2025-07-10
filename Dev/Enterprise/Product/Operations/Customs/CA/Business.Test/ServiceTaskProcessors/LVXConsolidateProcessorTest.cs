using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXConsolidateProcessorTest : TestCaseWithFactory
	{
		[TestDate(2018, 8, 8)]
		public void TestProcess()
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

			using (DisposableEnvironment.ForCompany("GC1"))
			{
				var declaration7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2015, 3, importer1.PK, branch1.PK, "1111", "B");
				var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration7.LVXInvoiceHeader, lvs1);
				Factory.Save();
				Assert(!declaration7.LVXInvoiceHeader.CA_ReadyForConsolidation);

				var logger = new SimpleLogger();
				var processor = new LVXConsolidateProcessor(logger);
				processor.Process(new List<ZGuid> { declaration7.PK });
				AssertEquals(@"Start Courier LVS Declaration processing
Warning: B00000007 is not ready for Consolidation.
Successfully processed 1 Courier LVS Declarations.
", logger.ToString());
				declaration7.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
				Factory.Save();
				logger = new SimpleLogger();
				processor = new LVXConsolidateProcessor(logger);
				processor.Process(new List<ZGuid> { declaration7.PK });
				AssertEquals(@"Start Courier LVS Declaration processing
Warning: B00000007 has already been attached to Consolidated LVS Declaration B00008001, it is ignored.
Successfully processed 1 Courier LVS Declarations.
", logger.ToString());

				var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 4, importer2.PK, branch1.PK, "1111", "A");
				var declaration4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 4, importer1.PK, branch2.PK, "1111", "A");
				var declaration6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000006", 2015, 3, importer1.PK, branch1.PK, "2222", "A");
				var declaration8 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000008", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
				Factory.Save();
				var invoice1 = (JobComInvoiceHeader)declaration1.Invoices.FirstOrDefault();
				invoice1.CA_ReadyForConsolidation = true;
				var invoice2 = (JobComInvoiceHeader)declaration2.Invoices.FirstOrDefault();
				invoice2.CA_ReadyForConsolidation = true;
				var invoice3 = (JobComInvoiceHeader)declaration3.Invoices.FirstOrDefault();
				invoice3.CA_ReadyForConsolidation = true;
				var invoice4 = (JobComInvoiceHeader)declaration4.Invoices.FirstOrDefault();
				invoice4.CA_ReadyForConsolidation = true;
				var invoice5 = (JobComInvoiceHeader)declaration5.Invoices.FirstOrDefault();
				invoice5.CA_ReadyForConsolidation = true;
				var invoice6 = (JobComInvoiceHeader)declaration6.Invoices.FirstOrDefault();
				invoice6.CA_ReadyForConsolidation = true;
				var invoice8 = (JobComInvoiceHeader)declaration8.Invoices.FirstOrDefault();
				invoice8.CA_ReadyForConsolidation = true;
				Factory.Save();

				logger = new SimpleLogger();
				processor = new LVXConsolidateProcessor(logger);
				processor.Process(new List<ZGuid> { declaration1.PK, declaration2.PK, declaration3.PK, declaration4.PK, declaration6.PK, declaration8.PK });
				AssertEquals(@"Start Courier LVS Declaration processing
B00000001 is attached to Consolidated LVS Declaration B00001000.
B00000002 is attached to Consolidated LVS Declaration B00001000.
B00000003 is attached to Consolidated LVS Declaration B00001000.
B00000004 is attached to Consolidated LVS Declaration B00001000.
B00000006 is attached to Consolidated LVS Declaration B00001001.
B00000008 is attached to Consolidated LVS Declaration B00001001.
Successfully processed 6 Courier LVS Declarations.
", logger.ToString());
			}
		}

		[TestDate(2021, 7, 7)]
		public void TestCreateIndividualConsolidateWithImporterZO_IsCreateToOneFTypePerCLVSEntry()
		{
			var imp1 = Factory.New<OrgHeader>();
			imp1.OH_Code = "Imp1";
			imp1.OH_FullName = "Imp1";
			var impAddInfo = OrgImpAddInfo.Get(imp1);
			impAddInfo.ZO_IsConsolidateToOneFTypePerCLVSEntry = true;

			var imp2 = Factory.New<OrgHeader>();
			imp2.OH_Code = "Imp2";
			imp2.OH_FullName = "Imp2";
			var imp2AddInfo = OrgImpAddInfo.Get(imp2);
			imp2AddInfo.ZO_IsConsolidateToOneFTypePerCLVSEntry = false;
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			Factory.Save();

			using (DisposableEnvironment.ForCompany("GC1"))
			{
				var dec = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000777", 2021, 4, imp1.PK, branch1.PK, "1111", "B");
				dec.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
				var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "VAR", 2021, 4, imp1.PK, branch1.PK, "PA", "A");
				Factory.Save();
				var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000111", 2015, 4, imp1.PK, branch1.PK, "1111", "A");
				declaration1.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
				var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000222", 2021, 4, imp1.PK, branch1.PK, "1111", "A");
				declaration2.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
				var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000333", 2021, 4, imp2.PK, branch1.PK, "1111", "A");
				declaration3.LVXInvoiceHeader.CA_ReadyForConsolidation = true;
				Factory.Save();

				var logger = new SimpleLogger();
				var processor = new LVXConsolidateProcessor(logger);
				processor.Process(new List<ZGuid> { dec.PK, declaration1.PK, declaration2.PK, declaration3.PK });
				AssertEquals(@"Start Courier LVS Declaration processing
B00000777 is attached to Consolidated LVS Declaration B00001000.
B00000111 is attached to Consolidated LVS Declaration B00001001.
B00000222 is attached to Consolidated LVS Declaration B00001002.
B00000333 is attached to Consolidated LVS Declaration B00008001.
Successfully processed 4 Courier LVS Declarations.
", logger.ToString());
			}
		}

		OrgHeader importer1;
		OrgHeader importer2;
		GlbBranch branch1;
		GlbBranch branch2;
		GlbBranch branch3;
	}
}
