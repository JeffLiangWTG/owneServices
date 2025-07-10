using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.NUnit;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(LVXConsolidateServiceTask))]
	sealed class LVXConsolidateServiceTaskTest : ServiceTaskTestCase<LVXConsolidateServiceTask>
	{
		[ExpectNoExceptions]
		public void TestRefreshInvoicesAfterConsolidation()
		{
			var lvx = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2023, 3, importer1.PK, branch1.PK, "1111", "B");
			var invoiceLine1 = lvx.LVXInvoiceHeader.InvoiceLines.AddNew();
			var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2023, 3, importer1.PK, branch1.PK, "PA", "A");
			using (lvx.LVXInvoiceHeader.SuspendRefreshAdditionalInvoice())
			{
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvx.LVXInvoiceHeader, lvs1);
			}
			NUnit.Framework.Assert.That(lvs1.Invoices.Count, NUnit.Framework.Is.EqualTo(0));
			NUnit.Framework.Assert.That(lvs1.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(0));

			LVXJobsConsolidateHelper.PrepareAndRunMergeForLVS(lvs1);
			NUnit.Framework.Assert.That(lvs1.Invoices.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(lvs1.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(1));

			LVXJobsConsolidateHelper.DetachFromConsolidatedLVSDeclaration(lvx.LVXInvoiceHeader, lvs1);
			NUnit.Framework.Assert.That(lvs1.Invoices.Count, NUnit.Framework.Is.EqualTo(0));
			NUnit.Framework.Assert.That(lvs1.InvoiceLines.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[TestDate(2018, 8, 8)]
		public void TestRunTask()
		{
			using (DisposableEnvironment.ForCompany("GC1"))
			{
				var declaration7 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000007", 2015, 3, importer1.PK, branch1.PK, "1111", "B");
				var lvs1 = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00008001", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
				LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration7.LVXInvoiceHeader, lvs1);
				LVXJobsConsolidateHelper.PrepareAndRunMergeForLVS(lvs1);
				Factory.Save();
				Assert(!declaration7.LVXInvoiceHeader.CA_ReadyForConsolidation);

				var logger = new TestServiceLogger();
				var task = new LVXConsolidateServiceTask();
				task.ServiceLogger = logger;
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				NUnit.Framework.Assert.That(logger.ToString(), NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

				var declaration1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var invoice1 = (JobComInvoiceHeader)declaration1.Invoices.FirstOrDefault();
				invoice1.CA_ReadyForConsolidation = true;
				var declaration2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var invoice2 = (JobComInvoiceHeader)declaration2.Invoices.FirstOrDefault();
				invoice2.CA_ReadyForConsolidation = true;
				var declaration3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000003", 2015, 4, importer2.PK, branch1.PK, "1111", "A");
				var declaration4 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000004", 2015, 4, importer1.PK, branch1.PK, "1111", "A");
				var declaration5 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000005", 2015, 4, importer1.PK, branch2.PK, "1111", "A");
				var declaration6 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000006", 2015, 3, importer1.PK, branch1.PK, "2222", "A");
				var declaration8 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000008", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
				var declaration9 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000009", 2015, 3, importer1.PK, branch3.PK, "1111", "A");
				Factory.Save();

				logger = new TestServiceLogger();
				task = new LVXConsolidateServiceTask();
				task.ServiceLogger = logger;
				InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				NUnit.Framework.Assert.That(logger.ToString(), NUnit.Framework.Is.EqualTo(@"Information|Start Courier LVS Declaration processing
Information|B00000001 is attached to Consolidated LVS Declaration B00001000.
Information|B00000002 is attached to Consolidated LVS Declaration B00001000.
Information|B00000003 is attached to Consolidated LVS Declaration B00001000.
Information|B00000004 is attached to Consolidated LVS Declaration B00001000.
Information|B00000006 is attached to Consolidated LVS Declaration B00001001.
Information|B00000008 is attached to Consolidated LVS Declaration B00001001.
Information|Successfully processed 6 Courier LVS Declarations.
Information|Start Courier LVS Declaration processing
Information|B00000005 is attached to Consolidated LVS Declaration B00001000.
Information|Successfully processed 1 Courier LVS Declarations.
Information|Start Courier LVS Declaration processing
Information|B00000009 is attached to Consolidated LVS Declaration B00001002.
Information|Successfully processed 1 Courier LVS Declarations.
"));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			if (UseSnapshotProtectionAttribute.IsProtected)
			{
				return;
			}
			importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "Importer2";
			importer2.OH_FullName = "Importer2";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			branch2 = company1.Branches.AddNew();
			branch2.GB_Code = "GBB";
			var company2 = Factory.New<GlbCompany>();
			company2.GC_Code = "GC2";
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
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
		}

		OrgHeader importer1;
		OrgHeader importer2;
		GlbBranch branch1;
		GlbBranch branch2;
		GlbBranch branch3;
	}
}
