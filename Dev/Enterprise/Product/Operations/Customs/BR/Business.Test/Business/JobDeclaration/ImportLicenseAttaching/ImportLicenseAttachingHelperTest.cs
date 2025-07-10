using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportLicenseAttachingHelperTest : TestCaseWithFactory
	{
		public void TestAttachAndDetachImportLicense()
		{
			var authorizationDate = DateTime.Today;

			var destinationDeclaration = Factory.New<JobDeclaration>();
			destinationDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Description = "TEST";
			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryheader1.CH_EntryReleaseDate = authorizationDate;
			entryheader1.MovementReferenceNumberSetter("2000010001");
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Description = "TEST";
			var entryheader2 = declaration.ActiveEntryHeaders.AddNew();
			entryheader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryheader2.CH_EntryReleaseDate = authorizationDate.AddDays(-1);
			entryheader2.MovementReferenceNumberSetter("2000010002");
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV001";
			invoiceHeader.JZ_ClusterKey = 1;
			invoiceHeader.JZ_CU_RelatedHouseBill = new ZGuid();
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.JI_CL = new ZGuid();
			invoiceLine1.JI_CO = new ZGuid();
			invoiceLine1.JI_JO = new ZGuid();
			invoiceLine1.JI_ClusterKey = 1;
			invoiceLine1.JI_MatchingKey = "A";
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction1.PK;
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction2.PK;
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_CEI = entryInstruction2.PK;

			var importLicenseAttaching = new ImportLicenseAttachingObject(entryInstruction1);
			importLicenseAttaching.FeeType = "F1ND";

			AssertEquals(2, destinationDeclaration.AttachImportLicense(new[] { importLicenseAttaching }).Count());

			var licPivots = destinationDeclaration.AttachedImportLicenseEntries;
			AssertEquals("One LIC GenPivot should be created", 1, licPivots.Count);

			var licPivot1 = licPivots[0];
			AssertEquals("LIC Related1ID", destinationDeclaration.PK, licPivot1.Relation1ID);
			AssertEquals("LIC Related2ID", entryInstruction1.PK, licPivot1.Relation2ID);

			AssertEquals("One Entry Instruction should be created", 1, destinationDeclaration.CustomsEntryInstructions.Count);
			AssertEquals("One Invoice should be cloned", 1, destinationDeclaration.Invoices.Count);

			var clonedInvoice = destinationDeclaration.Invoices[0];
			AssertEquals("JZ_JE", destinationDeclaration.PK, clonedInvoice.JZ_JE);
			AssertEquals("JZ_JZ_GroupInvoiceFK", destinationDeclaration.TopGroupInvoice.PK, clonedInvoice.JZ_JZ_GroupInvoiceFK);
			AssertEquals("JZ_ClusterKey NOT be cloned", destinationDeclaration.JE_ClusterKey, clonedInvoice.JZ_ClusterKey);
			AssertEquals("JZ_CU_RelatedHouseBill NOT be cloned", ZGuid.Empty, clonedInvoice.JZ_CU_RelatedHouseBill);

			var clonedEntryInstruction = destinationDeclaration.CustomsEntryInstructions[0];
			var clonedInvoiceLines = destinationDeclaration.Invoices[0].InvoiceLines.Cast<JobComInvoiceLine>();

			AssertEquals("Two Invoice Lines should be cloned", 2, clonedInvoiceLines.Count());

			var clonedLine1 = clonedInvoiceLines.ElementAt(0);
			var clonedLine2 = clonedInvoiceLines.ElementAt(1);

			AssertEquals("ClonedLine1 JI_parentID", clonedLine1.JI_ParentID, invoiceLine1.PK);
			AssertEquals("ClonedLine2 JI_parentID", clonedLine2.JI_ParentID, invoiceLine2.PK);

			CombineAssertions(() =>
			{
				foreach (var clonedLine in clonedInvoiceLines)
				{
					AssertEquals("clonedLine JI_CEI", clonedEntryInstruction.PK, clonedLine.JI_CEI);
					AssertEquals("JI_JZ should be equal to", clonedInvoice.PK, clonedLine.JI_JZ);
					AssertEquals("JI_CEI should be equal to", clonedEntryInstruction.PK, clonedLine.JI_CEI);
					AssertEquals("JI_ParentTableCode should be set", "JI", clonedLine.JI_ParentTableCode);
					AssertEquals("JI_CL should NOT be cloned", ZGuid.Empty, clonedLine.JI_CL);
					AssertEquals("JI_CO should NOT be cloned", ZGuid.Empty, clonedLine.JI_CO);
					AssertEquals("JI_JO should NOT be cloned", ZGuid.Empty, clonedLine.JI_JO);
					AssertEquals("JI_ClusterKey should NOT be cloned", destinationDeclaration.JE_ClusterKey, clonedLine.JI_ClusterKey);
					AssertEquals("JI_MatchingKey should NOT be cloned", ZString.Empty, clonedLine.JI_MatchingKey);
					AssertEquals("clonedLine ImportLicenseNumber", "2000010001", clonedLine.ImportLicenseNumber);
					AssertEquals("ClonedLine ImportLicenseAuthorizationDate", authorizationDate, clonedLine.ImportLicenseAuthorizationDate);
					AssertEquals("clonedLine JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, clonedLine.JI_ParentTableCode);
					AssertEquals("clonedLine ImportLicenseFeeType", "F1ND", clonedLine.ImportLicenseFeeType);
					Assert("clonedLine DutyTaxRegime", clonedLine.DutyTaxRegime.IsEmpty);
					Assert("clonedLine DutyLegalBase", clonedLine.DutyLegalBase.IsEmpty);
					Assert("clonedLine JI_OA_ManufacturerAddress", clonedLine.JI_OA_ManufacturerAddress.IsEmpty);
					AssertNull("clonedLine AdditionalTariff", clonedLine.AdditionalTariffs[0]);
				}
			});

			AssertEquals(2, destinationDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction2) }).Count());

			var licPivot2 = licPivots[1];
			AssertEquals("One more LIC GenPivot should be created", 2, licPivots.Count);
			AssertEquals("LIC Related1ID", destinationDeclaration.PK, licPivot2.Relation1ID);
			AssertEquals("LIC Related2ID", entryInstruction2.PK, licPivot2.Relation2ID);

			AssertEquals("No new Entry Instruction should be created", 1, destinationDeclaration.CustomsEntryInstructions.Count);
			AssertEquals("No new Invoice should be cloned", 1, destinationDeclaration.Invoices.Count);

			AssertEquals("Two more Invoice Lines should be cloned", 4, clonedInvoiceLines.Count());

			var clonedLine3 = clonedInvoiceLines.ElementAt(2);
			var clonedLine4 = clonedInvoiceLines.ElementAt(3);
			AssertEquals("ClonedLine3 JI_parentID", invoiceLine3.PK, clonedLine3.JI_ParentID);
			AssertEquals("ClonedLine4 JI_parentID", invoiceLine4.PK, clonedLine4.JI_ParentID);

			foreach (var cloneLine in new[] { clonedLine3, clonedLine4 })
			{
				AssertEquals("ClonedLine3 JI_CEI", clonedEntryInstruction.PK, cloneLine.JI_CEI);
				AssertEquals("ClonedLine3 ImportLicenseNumber", "2000010002", cloneLine.ImportLicenseNumber);
				AssertEquals("ClonedLine3 ImportLicenseAuthorizationDate", authorizationDate.AddDays(-1), cloneLine.ImportLicenseAuthorizationDate);
				AssertEquals("ClonedLine3 JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, cloneLine.JI_ParentTableCode);
				Assert("ClonedLine3 ImportLicenseFeeType should be Empty", cloneLine.ImportLicenseFeeType.IsEmpty);
				Assert("ClonedLine3 DutyTaxRegime", cloneLine.DutyTaxRegime.IsEmpty);
				Assert("ClonedLine3 DutyLegalBase", cloneLine.DutyLegalBase.IsEmpty);
				Assert("ClonedLine3 JI_OA_ManufacturerAddress", cloneLine.JI_OA_ManufacturerAddress.IsEmpty);
				AssertNull("ClonedLine3 AdditionalTariff", cloneLine.AdditionalTariffs[0]);
			}

			destinationDeclaration.DetachImportLicense(new[] { entryInstruction1 });

			AssertEquals("One LIC GenPivot should be deleted", 1, licPivots.Count);
			AssertEquals("One LIC GenPivot should be deleted", true, licPivot1.IsDeleted);

			AssertEquals("No Entry Instruction should be deleted", 1, destinationDeclaration.CustomsEntryInstructions.Count);
			AssertEquals("No Invoice should be deleted", 1, destinationDeclaration.Invoices.Count);

			AssertEquals("Two more Invoice Lines should be deleted", 2, clonedInvoiceLines.Count());
			AssertEquals(true, clonedLine1.IsDeleted);
			AssertEquals(true, clonedLine2.IsDeleted);

			destinationDeclaration.DetachImportLicense(new[] { entryInstruction2 });

			AssertEquals("One LIC GenPivot should be deleted", 0, licPivots.Count);
			AssertEquals("One LIC GenPivot should be deleted", true, licPivot2.IsDeleted);

			AssertEquals("No Entry Instruction should be deleted", 1, destinationDeclaration.CustomsEntryInstructions.Count);
			AssertEquals("Invoice should be deleted", 0, destinationDeclaration.Invoices.Count);

			AssertEquals("Two Invoice Lines should be deleted", 0, destinationDeclaration.InvoiceLines.Count);
			AssertEquals(true, clonedLine3.IsDeleted);
			AssertEquals(true, clonedLine4.IsDeleted);
		}

		public void TestAttachAndDetachImportLicense_CloneInvoiceData_ISW()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);
			var destinationDeclaration = Factory.New<JobDeclaration>();
			destinationDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.MainAddress.CompanyName = "MANUFACTURER1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Description = "TEST";
			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryheader1.MovementReferenceNumberSetter("2000010001");
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV001";
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;
			invoiceLine1.DutyTaxRegime = "2";
			invoiceLine1.DutyLegalBase = "1";
			invoiceLine1.ManufacturerDocAddressPK = manufacturer1.MainAddress.PK;
			invoiceLine1.JI_SecondaryPreference = "ASGPC";

			var importLicenseAttaching = new ImportLicenseAttachingObject(entryInstruction1);
			importLicenseAttaching.FeeType = "F1ND";

			CombineAssertions(() =>
			{
				destinationDeclaration.AttachImportLicense(new[] { importLicenseAttaching });
				var licPivots = destinationDeclaration.AttachedImportLicenseEntries;
				AssertEquals("One LIC GenPivot should be created", 1, licPivots.Count);

				var licPivot1 = licPivots[0];
				AssertEquals("LIC Related1ID", destinationDeclaration.PK, licPivot1.Relation1ID);
				AssertEquals("LIC Related2ID", entryInstruction1.PK, licPivot1.Relation2ID);

				AssertEquals("One Entry Instruction should be created", 1, destinationDeclaration.CustomsEntryInstructions.Count);
				AssertEquals("One Invoice should be cloned", 1, destinationDeclaration.Invoices.Count);

				var clonedEntryInstruction = destinationDeclaration.CustomsEntryInstructions[0];
				var clonedInvoiceLines = destinationDeclaration.Invoices[0].InvoiceLines.Cast<JobComInvoiceLine>();
				var clonedLine1 = clonedInvoiceLines.ElementAt(0);

				AssertEquals("Invoice Lines should be cloned", 1, clonedInvoiceLines.Count());
				AssertEquals("ClonedLine1 JI_CEI", clonedEntryInstruction.PK, clonedLine1.JI_CEI);
				AssertEquals("ClonedLine1 ImportLicenseNumber", "2000010001", clonedLine1.ImportLicenseNumber);
				AssertEquals("ClonedLine1 JI_parentID", clonedLine1.JI_ParentID, invoiceLine1.PK);
				AssertEquals("ClonedLine1 JI_ParentTableCode", JobComInvoiceLineSchema.Constants.Prefix, clonedLine1.JI_ParentTableCode);
				AssertEquals("ClonedLine1 ImportLicenseFeeType", "F1ND", clonedLine1.ImportLicenseFeeType);
				AssertEquals("ClonedLine1 DutyTaxRegime", "2", clonedLine1.DutyTaxRegime);
				AssertEquals("ClonedLine1 DutyLegalBase", "1", clonedLine1.DutyLegalBase);
				AssertEquals("ClonedLine1 JI_OA_ManufacturerAddress", manufacturer1.MainAddress.PK, clonedLine1.JI_OA_ManufacturerAddress);

				var additionTariff = clonedLine1.AdditionalTariffs[0];
				AssertEquals("AdditionalTariff.LegalActSubject", AdditionalTaxTypeList.Codes.TariffAgreement, additionTariff.LegalActSubject);
				AssertEquals("AdditionalTariff.TariffType", "ASGPC", additionTariff.TariffType);
				AssertEquals("AdditionalTariff.LegalActType", "DEC", additionTariff.LegalActType);
				AssertEquals("AdditionalTariff.LegalActIssuingBody", "EXEC", additionTariff.LegalActIssuingBody);
				AssertEquals("AdditionalTariff.LegalActNumber", "5106", additionTariff.LegalActNumber);
				AssertEquals("AdditionalTariff.LegalActYear", "2004", additionTariff.LegalActYear);
			});
		}

		public void TestPossibleEntryInstructionForAttachment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Description = "TEST1";
			var invHeader1 = declaration.Invoices.AddNew();
			var invLine1 = invHeader1.InvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction1.PK;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine1 = entryHeader1.AllEntryLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;
			entryInstruction1.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);
			Factory.Save();
			AssertEquals("Must contain 1 CusEntryInstruction", 1, declaration.GetPossibleEntryInstructionForAttachment().Length);

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Description = "TEST2";
			var invHeader2 = declaration.Invoices.AddNew();
			var invLine2 = invHeader2.InvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction2.PK;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;
			entryInstruction2.EntryHeader.MovementReferenceNumberSetter("TST2", ZDateTime.Now);
			Factory.Save();
			AssertEquals("Must contain 2 CusEntryInstruction", 2, declaration.GetPossibleEntryInstructionForAttachment().Length);

			var genPivot = Factory.New<GenPivot>();
			genPivot.XX_RelationType = GenPivotTypeDecider.Types.JobDecRelatedImportLicenseEntryGenPivot;
			genPivot.Relation1ID = declaration.PK;
			genPivot.Relation2ID = entryInstruction1.PK;
			Factory.Save();
			AssertEquals("Must contain 1 CusEntryInstruction", 1, declaration.GetPossibleEntryInstructionForAttachment().Length);
			AssertEquals("Must be equal", entryInstruction2.PK, declaration.GetPossibleEntryInstructionForAttachment()[0].PK);
		}

		public void TestDetachGeneratedImportLicense()
		{
			var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			licDeclaration.JE_DeclarationReference = "LIC_DEC";

			var iswDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			iswDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			iswDeclaration.JE_DeclarationReference = "ISW_DEC";
			var iswInstruction = iswDeclaration.CustomsEntryInstructions.AddNew();
			iswInstruction.CEI_Description = "ISW_TEST";
			var iswInvHeader = iswDeclaration.Invoices.AddNew();
			var iswInvLine1 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine1.JI_CEI = iswInstruction.PK;
			iswInvLine1.ImportLicenseNumber = "1234567890";
			var iswInvLine2 = iswInvHeader.InvoiceLines.AddNew();
			iswInvLine2.JI_CEI = iswInstruction.PK;
			iswInvLine2.ImportLicenseNumber = "1234567890";
			var iswEntryHeader = iswDeclaration.ActiveEntryHeaders.AddNew();
			iswEntryHeader.CH_JE = iswDeclaration.PK;
			iswEntryHeader.CH_CEI_Instruction = iswInstruction.PK;
			var iswEntryLine = iswEntryHeader.MergedLines.AddNew();
			iswInvLine1.JI_CL = iswEntryLine.PK;
			iswInvLine2.JI_CL = iswEntryLine.PK;

			Factory.Save();

			var generator = new GenerateImportLicenseObject(iswEntryLine);
			generator.ImportLicenseDeclarationPK = licDeclaration.PK;
			generator.GenerateImportLicense();
			var licInvLine1 = licDeclaration.InvoiceLines.Cast<JobComInvoiceLine>().Single(x => x.JI_ParentID == iswInvLine1.PK);
			var licInvLine2 = licDeclaration.InvoiceLines.Cast<JobComInvoiceLine>().Single(x => x.JI_ParentID == iswInvLine2.PK);

			ImportLicenseAttachingHelper.DetachImportLicense(iswDeclaration, licDeclaration.CustomsEntryInstructions);

			CombineAssertions(() =>
			{
				void AssertISWInvoiceLineDetached(JobComInvoiceLine iswInvLine)
				{
					AssertEquals("ISW Invoice Line should not be deleted", false, iswInvLine.IsDeleted);
					AssertEquals("ISW JI_ParentID cleard", ZGuid.Empty, iswInvLine.JI_ParentID);
					AssertEquals("ISW JI_ParentTableCode cleard", ZString.Empty, iswInvLine.JI_ParentTableCode);
					AssertEquals("ISW ImportLicenseNumber cleard", ZString.Empty, iswInvLine.ImportLicenseNumber);
				}
				AssertISWInvoiceLineDetached(iswInvLine1);
				AssertISWInvoiceLineDetached(iswInvLine2);

				AssertEquals("LIC JI_ParentID cleard", ZGuid.Empty, licInvLine1.JI_ParentID);
				AssertEquals("LIC JI_ParentTableCode cleard", ZString.Empty, licInvLine1.JI_ParentTableCode);
				AssertEquals("LIC JI_ParentID cleard", ZGuid.Empty, licInvLine2.JI_ParentID);
				AssertEquals("LIC JI_ParentTableCode cleard", ZString.Empty, licInvLine2.JI_ParentTableCode);
			});
		}

		public void TestAttachWithRatePreferenceCondition()
		{
			ReferenceTestDataHelper.CreateReferenceDataForTariffAgreementCode(Factory);

			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.MainAddress.CompanyName = "MANUFACTURER1";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Description = "TEST";
			var entryheader1 = declaration.ActiveEntryHeaders.AddNew();
			entryheader1.CH_CEI_Instruction = entryInstruction1.PK;
			entryheader1.MovementReferenceNumberSetter("2000010001");
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV001";
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.DutyLegalBase = "1";
			invoiceLine.ManufacturerDocAddressPK = manufacturer1.MainAddress.PK;
			invoiceLine.JI_SecondaryPreference = "ASGPC";

			var importLicenseAttaching = new ImportLicenseAttachingObject(entryInstruction1);
			importLicenseAttaching.FeeType = "F1ND";

			var destinationDeclaration = Factory.New<JobDeclaration>();
			destinationDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			destinationDeclaration.AttachImportLicense(new[] { importLicenseAttaching });
			AssertEquals("FTA" , destinationDeclaration.InvoiceLines[0].JI_PrimaryPreference);

			destinationDeclaration.DetachImportLicense(new[] { entryInstruction1 });
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			destinationDeclaration.AttachImportLicense(new[] { importLicenseAttaching });
			AssertEquals("NORMAL", destinationDeclaration.InvoiceLines[0].JI_PrimaryPreference);

			destinationDeclaration.DetachImportLicense(new[] { entryInstruction1 });
			invoiceLine.DutyTaxRegime = "4";
			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			destinationDeclaration.AttachImportLicense(new[] { importLicenseAttaching });
			AssertEquals("REDUCED", destinationDeclaration.InvoiceLines[0].JI_PrimaryPreference);

			destinationDeclaration.DetachImportLicense(new[] { entryInstruction1 });
			invoiceLine.DutyTaxRegime = ZString.Empty;
			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			destinationDeclaration.AttachImportLicense(new[] { importLicenseAttaching });
			Assert("Should be Empty", destinationDeclaration.InvoiceLines[0].JI_PrimaryPreference.IsEmpty);
		}
	}
}
