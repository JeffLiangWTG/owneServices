using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LVXJobsConsolidateHelperTest : TestCaseWithFactory
	{
		public void TestNoExceptionThrownWhenConsolidateLVXJobWithMultipleInvoiceHeader()
		{
			var associatedLVSDeclarations = new List<JobDeclaration>();
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20001", companyPk: company1.PK.ToGuid());
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20001");
			Factory.Save();
			var lvx = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B0000000X", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var invoice = lvx.Invoices.AddNew();
			invoice.JZ_OH_Buyer = importer1.PK;
			invoice.CA_PortOfClearance = "1111";
			AssertNoExceptionThrown(() =>
			{
				LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx, Factory, associatedLVSDeclarations, (dec) => dec.GetDeclarationIdLink(), Notify);
			});
			var lvs = associatedLVSDeclarations.FirstOrDefault();
			AssertEquals(string.Format("{0} should be attached to {1}", lvx.HumanReadableName, lvs.HumanReadableName), lvs.PK, lvx.LVXInvoiceHeader.FirstAdditionalDeclaration.PK);
		}

		public void TestHandleSaveException()
		{
			var associatedLVSDeclarations = new List<JobDeclaration>();
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20001", companyPk: company1.PK.ToGuid());
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20001");
			Factory.Save();
			var lvx = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B0000000X", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			Factory.Save();
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var lvx1 = newFactory.Load<JobDeclaration>(lvx.PK);
			newFactory.Saving += (f) =>
			{
				lvx.JE_CarrierCode = "001";
				Factory.Save();
				lvx1.JE_CarrierCode = "002";
			};

			AssertNoExceptionThrown(() =>
			{
				LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx1, newFactory, associatedLVSDeclarations, (dec) => dec.GetDeclarationIdLink(), Notify);
			});
			var lvs = associatedLVSDeclarations.FirstOrDefault();
			AssertEquals(string.Format("[BROKEN-HL B0000000X] is attached to Consolidated LVS Declaration [BROKEN-HL {0}].",lvs.JE_DeclarationReference), currentMessage);
			AssertEquals(string.Format("{0} should be attached to {1}", lvx.HumanReadableName, lvs.HumanReadableName), lvs.PK, lvx1.LVXInvoiceHeader.FirstAdditionalDeclaration.PK);
		}

		public void TestGetErrorMessageWhenDateIsNotValid()
		{
			List<JobDeclaration> associatedLVSDeclarations = new List<JobDeclaration>();
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20001", companyPk: company1.PK.ToGuid());
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20001");
			Factory.Save();
			var lvx3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B0000000X", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvxObj = lvx3 as BusinessObject;
			lvxObj[JobDeclarationSchema.JE_EntryAuthorisationDate] = null;
			Factory.Save();

			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx3, Factory, associatedLVSDeclarations, (dec) => dec.GetDeclarationIdLink(), Notify);
			AssertEquals("[BROKEN-HL B0000000X] has invalidate Entry Authorization Date.", currentMessage);
		}

		public void TestFindMatchingLVSDeclarationInDb()
		{
			var lvx1 = Factory.New<JobDeclaration>();
			lvx1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvx1.JE_MessageSubType = "VAR";
			lvx1.JE_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();
			var strategies = LVXJobsConsolidateHelper.GetConsolidationStrategies(lvx1);
			AssertSame(lvx1, LVXJobsConsolidateHelper.FindMatchingLVSDeclarationInDb(Factory, strategies));
			lvx1.CA_LVSCloseDate = DateTime.Now;
			Factory.Save();
			strategies = LVXJobsConsolidateHelper.GetConsolidationStrategies(lvx1);
			AssertNotSame(lvx1, LVXJobsConsolidateHelper.FindMatchingLVSDeclarationInDb(Factory, strategies));
		}

		public void TestGetLVXJobValidationMessage()
		{
			List<JobDeclaration> associatedLVSDeclarations = new List<JobDeclaration>();
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "Importer1";
			importer1.OH_FullName = "Importer1";
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "GC1";
			var branch1 = company1.Branches.AddNew();
			branch1.GB_Code = "GBA";
			TransactionNumberTestHelper.SetupCompanyASECNumberForTest("20001", companyPk: company1.PK.ToGuid());
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "20001");
			Factory.Save();
			var lvx1 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000001", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvx2 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B00000002", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			var lvx3 = LVXJobsConsolidateHelperForTest.CreateLVX(Factory, "B0000000X", 2015, 3, importer1.PK, branch1.PK, "1111", "A");
			lvx3.IsCancelled = true;
			Factory.Save();
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx3, Factory, associatedLVSDeclarations, (dec) => dec.GetDeclarationIdLink(), Notify);
			AssertEquals("[BROKEN-HL B0000000X] has already been deactivated.", currentMessage);
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx3, Factory, associatedLVSDeclarations, (dec) => dec.JE_DeclarationReference, Notify);
			AssertEquals("B0000000X has already been deactivated.", currentMessage);
			lvx2.Invoices.RemoveAll();
			var invoice = lvx2.Invoices.OfType<JobComInvoiceHeader>().FirstOrDefault();
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx2, Factory, associatedLVSDeclarations, (dec) => dec.JE_DeclarationReference, Notify);
			AssertEquals("B00000002 has no invoices.", currentMessage);
			lvx2.ActiveEntryHeaders.RemoveAndDeleteAll();
			invoice = lvx2.Invoices.AddNew();
			invoice.JZ_OH_Buyer = importer1.PK;
			invoice.CA_PortOfClearance = "1111";
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx2, Factory, associatedLVSDeclarations, (dec) => dec.JE_DeclarationReference, Notify);
			AssertEquals("B00000002 is not ready for Consolidation.", currentMessage);
			invoice.CA_ReadyForConsolidation = true;
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx2, Factory, associatedLVSDeclarations, (dec) => dec.JE_DeclarationReference, Notify);
			AssertEquals("Can't merge B00000002 and it has no entry headers, it is ignored. Merge failed reason : You can't merge this entry because there is an invoice header with no invoice lines.", currentMessage);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var lvs = LVXJobsConsolidateHelperForTest.CreateLVS(Factory, "B00000003", "MSI", 2015, 3, importer1.PK, branch1.PK, "PA", "A");
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvx2.LVXInvoiceHeader, lvs);
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx2, Factory, associatedLVSDeclarations, (dec) => dec.JE_DeclarationReference, Notify);
			AssertEquals("B00000002 is not ready for Consolidation.", currentMessage);
			invoice.CA_ReadyForConsolidation = true;
			LVXJobsConsolidateHelper.ConsolidateSingleLVXJob(lvx2, Factory, associatedLVSDeclarations, (dec) => dec.JE_DeclarationReference, Notify);
			AssertEquals("B00000002 has already been attached to Consolidated LVS Declaration B00000003, it is ignored.", currentMessage);
		}

		void Notify(LogType logType, string message, params object[] args)
		{
			currentMessage = string.Format(message, args);
		}
		ZString currentMessage;
	}
}
