using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.CN;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusEntryInstruction))]
	class CusEntryInstructionTest : Customs.Business.Testing.CusEntryInstructionAbstractTest
	{
		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(
				Factory.New<CusEntryInstruction>(),
				"CNCusEntryInstruction",
				predicate: fieldName => !fieldName.StartsWith(CusCNEntryInstructionSchema.Constants.Prefix));
		}

		public void TestICusStorageDocPivotTypeSupporter_ReloadCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var provider = (ICusStorageDocPivotTypeSupporter)instruction;
			var attachments = instruction.Attachments;
			CombineAssertions(() =>
			{
				AssertEquals("Pre-requisite", 0, attachments.Count);
				var pivot = Factory.New<CusStorageDocPivot>();
				pivot.CSD_ParentID = instruction.PK;
				pivot.CSD_ParentTableCode = instruction.TablePrefix;
				provider.ReloadCollection();
				AssertEquals("Reloaded", 1, attachments.Count);
			});
		}

		public void TestICusStorageDocPivotTypeSupporter_HumanReadableName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var provider = (ICusStorageDocPivotTypeSupporter)instruction;
			AssertEquals("Entry Instruction with CPC 11", provider.HumanReadableName);
		}

		public void TestCanDelete()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = testDeclaration.CustomsEntryInstructions.AddNew();
			var entryHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			Assert(instruction.CanDelete);
			entryHeader.CH_CEI_Instruction = instruction.PK;
			Assert(instruction.CanDelete);
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingOriginal;
			Assert(!instruction.CanDelete);
			instruction.CEI_Style = "11";
			AssertEquals("Entry Instruction with CPC 11 is being used by an Entry Header which has been lodged at Customs waiting for response and cannot be deleted.", instruction.ReasonForNotAbleToDelete);
		}

		public void TestOperationMattersAsString()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var code1 = Factory.New<OperationMatter>();
			code1.CY_ParentTableCode = instruction.TablePrefix;
			code1.CY_Code = OperationMatterList.Codes.AssuredInspectClearance;
			code1.CY_Type = Constants.CusCodeDataTypes.Codes.OperationMatter;
			code1.CY_ParentID = instruction.PK;
			var code2 = Factory.New<OperationMatter>();
			code2.CY_ParentTableCode = instruction.TablePrefix;
			code2.CY_Code = OperationMatterList.Codes.ConsolidatedDutyCollection;
			code2.CY_Type = Constants.CusCodeDataTypes.Codes.OperationMatter;
			code2.CY_ParentID = instruction.PK;
			AssertEquals("担保验放,汇总征税", instruction.OperationMattersAsString);
		}

		public void TestBillOfLadingDateInfo()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertNotNull(instruction.BillOfLadingDateInfo);
			Assert(!(instruction.BillOfLadingDateInfo is ZWrappedPropertyInfo));
			instruction.BillOfLading = "BL001";
			Assert(instruction.BillOfLadingDateInfo is ZWrappedPropertyInfo);
		}

		public void TestOtherPackagesAsString()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var code1 = Factory.New<OtherPackage>();
			code1.CY_ParentTableCode = instruction.TablePrefix;
			code1.CY_Code = "00";
			code1.CY_Type = Constants.CusCodeDataTypes.Codes.Package;
			code1.CY_ParentID = instruction.PK;
			var code2 = Factory.New<OtherPackage>();
			code2.CY_ParentTableCode = instruction.TablePrefix;
			code2.CY_Code = "01";
			code2.CY_Type = Constants.CusCodeDataTypes.Codes.Package;
			code2.CY_ParentID = instruction.PK;
			AssertEquals("散装,裸装", instruction.OtherPackagesAsString);
		}

		public void TestGetCusCodeDataType()
		{
			var iCusCodeDataTypeSupporter = Factory.New<CusEntryInstruction>() as ICusCodeDataTypeSupporter;
			AssertEquals(typeof(EnterpriseQualification), iCusCodeDataTypeSupporter.GetCusCodeDataTypes()["EPQ"]);
			AssertEquals(typeof(SpecialBusinessIdentifier), iCusCodeDataTypeSupporter.GetCusCodeDataTypes()["SBI"]);
			AssertEquals(typeof(OtherPackage), iCusCodeDataTypeSupporter.GetCusCodeDataTypes()["PKG"]);
			AssertEquals(typeof(OperationMatter), iCusCodeDataTypeSupporter.GetCusCodeDataTypes()["OPM"]);
			AssertEquals(typeof(CusAttachment), iCusCodeDataTypeSupporter.GetCusCodeDataTypes()["ATH"]);
			AssertEquals(false, iCusCodeDataTypeSupporter.GetCusCodeDataTypes().ContainsKey("OTH"));
		}

		public void TestEnterpriseQualifications()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = testDeclaration.CustomsEntryInstructions.AddNew();
			var cusCodeData0 = instruction.EnterpriseQualifications.AddNew();
			var cusCodeData1 = Factory.New<EnterpriseQualification>();
			cusCodeData1.CY_ParentID = instruction.PK;
			cusCodeData1.CY_ParentTableCode = "CEI";
			cusCodeData1.CY_Type = "EPQ";
			var cusCodeData2 = Factory.New<EnterpriseQualification>();
			cusCodeData2.CY_ParentID = testDeclaration.PK;
			cusCodeData2.CY_ParentTableCode = "JE";
			cusCodeData2.CY_Type = "EPQ";
			var cusCodeData3 = Factory.New<EnterpriseQualification>();
			cusCodeData3.CY_ParentID = instruction.PK;
			cusCodeData3.CY_ParentTableCode = "CEI";
			cusCodeData3.CY_Type = "OTH";
			instruction.EnterpriseQualifications.Load();
			AssertEquals("Correct type", typeof(EnterpriseQualificationCollection), instruction.EnterpriseQualifications.GetType());
			AssertEquals("Value defaulted", "CEI", cusCodeData0.CY_ParentTableCode);
			AssertEquals("Value defaulted", instruction.PK, cusCodeData0.CY_ParentID);
			AssertEquals("Only 2 loaded correctly", 2, instruction.EnterpriseQualifications.Count);
			Assert("Only 2 loaded correctly", instruction.EnterpriseQualifications.Select(x => x.PK).Contains(cusCodeData0.PK));
			Assert("Only 2 loaded correctly", instruction.EnterpriseQualifications.Select(x => x.PK).Contains(cusCodeData1.PK));
			AssertEquals("Child collection registered", true, instruction.IsRegisteredEditableChildObject(instruction.EnterpriseQualifications));
		}

		public void TestPackagesAndPackageUQ()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = "BTH";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			childInstruction.CEI_Packages = 11;
			childInstruction.CEI_PackageUQ = "99";
			instruction.CEI_Packages = 10;
			instruction.CEI_PackageUQ = "00";
			AssertEquals("Original value not empty, should NOT be copied", 11, childInstruction.CEI_Packages);
			AssertEquals("Original value not empty, should NOT be copied", "99", childInstruction.CEI_PackageUQ);
			childInstruction.CEI_Packages = 0;
			childInstruction.CEI_PackageUQ = "";
			instruction.CEI_Packages = 12;
			instruction.CEI_PackageUQ = "01";
			AssertEquals("Original value empty, should be copied", 12, childInstruction.CEI_Packages);
			AssertEquals("Original value empty, should be copied", "01", childInstruction.CEI_PackageUQ);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageSubType = "BTH";
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Packages = 13;
			instruction1.CEI_PackageUQ = "99";
			var childInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Packages should have been copied when setting Parent", 13, childInstruction1.CEI_Packages);
			AssertEquals("PackagesUQ should have been copied when setting Parent", "99", childInstruction1.CEI_PackageUQ);
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			childInstruction1.CEI_CEI_Parent = instruction2.PK;
			AssertEquals("Packages should have been copied when setting Parent", 13, instruction2.CEI_Packages);
			AssertEquals("PackagesUQ should have been copied when setting Parent", "99", instruction2.CEI_PackageUQ);
		}

		public void TestCEI_RelatedManualNo()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_ManualNo = "MANUALNO001";
			instruction1.CEI_RelatedManualNo = "M";
			declaration.JE_MessageSubType = "BTH";
			Assert("WillGenerateBothEntries, should be readonly", instruction1.CEI_RelatedManualNoInfo.ReadOnly);
			declaration.JE_MessageSubType = "CUS";
			Assert("NOT WillGenerateBothEntries, should NOT be readonly", !instruction1.CEI_RelatedManualNoInfo.ReadOnly);
			declaration.JE_MessageSubType = "BTH";
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("Related Manual No. should have been copied from parent when setting Parent", "MANUALNO001", childInstruction.CEI_RelatedManualNo);
			AssertEquals("Related Manual No. should have been copied from child when setting child's Parent", "", instruction1.CEI_RelatedManualNo);
			instruction1.CEI_ManualNo = "MANUALNO002";
			childInstruction.CEI_ManualNo = "MANUALN1002";
			AssertEquals("Related Manual No. should have been copied from parent when setting parent's manual No.", "MANUALNO002", childInstruction.CEI_RelatedManualNo);
			AssertEquals("Related Manual No. should have been copied from child when setting child's manual No.", "MANUALN1002", instruction1.CEI_RelatedManualNo);
		}

		public void TestAttachmentsAndCusStorageDocPivotTypeDecider()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.CreateNewCusStorageDocPivot();
			attachment.CSD_DocType = "T1";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var reloadedInstruction = newFactory.Load<CusEntryInstruction>(instruction.PK);
			var reloadedAttachments = reloadedInstruction.Attachments;
			AssertEquals("Should load attachment from DB", 1, reloadedAttachments.Count);
			AssertEquals("Should load attachment from DB", "T1", reloadedAttachments[0].AttachmentType);
			var reloadedAttchment = newFactory.Load<BaseCusStorageDocPivot>(attachment.PK);
			AssertType("ICusStorageDocPivotTypeSupporter methof, should have returned correct Type", typeof(CusStorageDocPivot), reloadedAttchment);
		}

		public void TestOnCEI_StyleChanged()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("CN", "China");
			helper.CreateOrFindExistingRefCusProcedure("CN", "", "0110", "", "", "0110", "IMP,EXP", "");
			helper.CreateOrFindExistingRefCusProcedure("CN", "", "0214", "", "", "0214", "IMP,EXP", "");
			helper.CreateOrFindExistingRefCusProcedure("CN", "", "0245", "", "", "0245", "IMP,EXP", "");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "0110";
			AssertEquals("Default to 101", "101", instruction.CEI_LevyType);
			instruction.CEI_Style = "0214";
			AssertEquals("Default to 502", "502", instruction.CEI_LevyType);
			instruction.CEI_LevyType = "299";
			instruction.CEI_Style = "0110";
			AssertEquals("No change if selected LevyType is supported", "299", instruction.CEI_LevyType);
			instruction.CEI_LevyType = "299";
			instruction.CEI_Style = "0245";
			AssertEquals("Change to empty if LevyType is allowed empty", ZString.Empty, instruction.CEI_LevyType);
			instruction.CEI_LevyType = "414";
			instruction.CEI_Style = "0110";
			AssertEquals("Change if selected LevyType is not supported", "101", instruction.CEI_LevyType);
			instruction.CEI_LevyType = "414";
			instruction.CEI_Style = "0245";
			AssertEquals("Change to empty if LevyType is allowed empty", ZString.Empty, instruction.CEI_LevyType);
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			using (instruction2.SetterSuspender.SuspendSetting(CusEntryInstruction.Schema.CEI_Style))
			{
				instruction2.CEI_Style = "0110";
				AssertEquals("", instruction2.CEI_Style);
				AssertEquals("", instruction2.CEI_LevyType);
			}
		}

		public void TestDescriptionWithBillOfLading()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(ZString.Empty, instruction.DescriptionWithBillOfLading);
			instruction.CEI_Description = "Test Instruction";
			instruction.BillOfLading = "12345678";
			AssertEquals("Test Instruction - 12345678", instruction.DescriptionWithBillOfLading);
		}

		public void TestBillOfLading()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(ZString.Empty, instruction.BillOfLading);
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = instruction.PK;
			entryNumber.CE_EntryType = CusEntryNumberTypes.China.BillOfLading;
			entryNumber.CE_EntryNum = "AAA";
			instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.BillOfLading = "BBB";
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.China.BillOfLading);
			var newEntryNum = Factory.LoadTop1<CusEntryNumber>(query);
			AssertNotNull(newEntryNum);
			AssertEquals(CusEntryNumberTypes.China.BillOfLading, newEntryNum.CE_EntryType);
			AssertEquals("BBB", newEntryNum.CE_EntryNum);
		}

		public void TestGetCusAddInfoTypes()
		{
			var testItem = (ICusAddInfoTypeSupporter)Factory.NewWithValidTestData<JobDeclaration>().CustomsEntryInstructions.AddNew();
			AssertEquals(typeof(CIQRequiredDocument), testItem.GetCusAddInfoTypes()["RQD"]);
		}

		public void TestCustomsMessageRemarks()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var messageRemarksQuery = new ZQuery(StmNoteSchema.ST_ParentID, instruction.PK);
			messageRemarksQuery.AddToFilter(StmNoteSchema.ST_Table, instruction.TableName);
			messageRemarksQuery.AddToFilter(StmNoteSchema.ST_Description, "Customs Message Remarks");
			var testLoadOfNote = Factory.Load<StmNote>(messageRemarksQuery);
			AssertEquals(0, testLoadOfNote.Length);
			instruction.CustomsMessageRemarks = "Customs Message Remarks Text";
			testLoadOfNote = Factory.Load<StmNote>(messageRemarksQuery);
			AssertEquals(1, testLoadOfNote.Length);
			AssertEquals("Customs Message Remarks Text", testLoadOfNote[0].ST_NoteText);
			AssertEquals("Customs Message Remarks", testLoadOfNote[0].ST_Description);
			instruction.CustomsMessageRemarks = "Customs Message Remarks Text changed";
			testLoadOfNote = Factory.Load<StmNote>(messageRemarksQuery);
			AssertEquals(1, testLoadOfNote.Length);
			AssertEquals("Customs Message Remarks Text changed", testLoadOfNote[0].ST_NoteText);
			AssertEquals("Customs Message Remarks", testLoadOfNote[0].ST_Description);
			instruction.CustomsMessageRemarks = "";
			Factory.Save();
			testLoadOfNote = Factory.Load<StmNote>(messageRemarksQuery);
			AssertEquals(0, testLoadOfNote.Length);
			instruction.CustomsMessageRemarks = "Customs Message Remarks";
			testLoadOfNote = Factory.Load<StmNote>(messageRemarksQuery);
			AssertEquals(1, testLoadOfNote.Length);
			AssertEquals(false, testLoadOfNote[0].IsDeleted);
			Factory.Save();
			instruction.CustomsMessageRemarks = "";
			AssertEquals(false, testLoadOfNote[0].IsDeleted);
			Factory.Save();
			AssertEquals(true, testLoadOfNote[0].IsDeleted);
		}

		public void TestSpecialBusinessIdentifiersAsString()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CIQRequires = true;
			Factory.Save();
			var code1 = Factory.New<SpecialBusinessIdentifier>();
			code1.CY_ParentTableCode = instruction.TablePrefix;
			code1.CY_Code = "B01";
			code1.CY_Type = "SBI";
			code1.CY_ParentID = instruction.PK;
			var code2 = Factory.New<SpecialBusinessIdentifier>();
			code2.CY_ParentTableCode = instruction.TablePrefix;
			code2.CY_Code = "B04";
			code2.CY_Type = "SBI";
			code2.CY_ParentID = instruction.PK;
			Factory.Save();
			AssertEquals("国际赛事,国际会议", instruction.SpecialBusinessIdentifiersAsString);
		}

		public void TestCIQRequires()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "BTH";
			var enteringInstruction = declaration.CustomsEntryInstructions.AddNew();
			var exitingInstruction = declaration.CustomsEntryInstructions.AddNew();
			enteringInstruction.CEI_CIQRequires = true;
			exitingInstruction.CEI_CIQRequires = true;
			declaration.JE_MessageType = "EXP";
			AssertEquals("Exiting Instruction becomes CUS so CEI_CIQRequires should remain true.", true, exitingInstruction.CEI_CIQRequires);
			AssertEquals("Entering Instruction becomes REC so CEI_CIQRequires should be set false.", false, enteringInstruction.CEI_CIQRequires);
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "BTH";
			enteringInstruction = declaration.CustomsEntryInstructions.AddNew();
			exitingInstruction = declaration.CustomsEntryInstructions.AddNew();
			enteringInstruction.CEI_CIQRequires = true;
			exitingInstruction.CEI_CIQRequires = true;
			declaration.JE_MessageSubType = "CUS";
			AssertEquals("CEI_CIQRequires should remain true for CUS Job.", true, exitingInstruction.CEI_CIQRequires);
			AssertEquals("CEI_CIQRequires should remain true for CUS Job.", true, enteringInstruction.CEI_CIQRequires);
		}

		public void TestClearCIQData()
		{
			void AssertCiqDataCleared(string message, CusEntryInstruction instructionToAssert, bool shouldHaveCleared)
			{
				CombineAssertions(message, () =>
				{
					var messageShouldOrShouldNot = shouldHaveCleared ? "" : "NOT ";
					Assert("EnterpriseQualifications should " + messageShouldOrShouldNot + "be cleared", shouldHaveCleared != instructionToAssert.EnterpriseQualifications.Any());
					Assert("CEI_CIQRelatedNum should " + messageShouldOrShouldNot + "be cleared", shouldHaveCleared == instructionToAssert.CEI_CIQRelatedNum.IsEmpty);
					Assert("CEI_CIQRelatedReason should " + messageShouldOrShouldNot + "be cleared", shouldHaveCleared == instructionToAssert.CEI_CIQRelatedReason.IsEmpty);
					Assert("SpecialBusinessIdentifiers should " + messageShouldOrShouldNot + "be cleared", shouldHaveCleared == instructionToAssert.SpecialBusinessIdentifiersAsString.IsEmpty);
					Assert("CIQRequiredDocuments should " + messageShouldOrShouldNot + "be cleared", shouldHaveCleared != instructionToAssert.CIQRequiredDocuments.Any());
				});
			}

			void AddCIQDatas(CusEntryInstruction instructionToAdd)
			{
				var enterpriseQualification = instructionToAdd.EnterpriseQualifications.AddNew();
				enterpriseQualification.CY_Code = "303";
				enterpriseQualification.CY_Data = "1234";
				instructionToAdd.CEI_CIQRelatedNum = "REL00001";
				instructionToAdd.CEI_CIQRelatedReason = "1";
				var special = instructionToAdd.SpecialBusinessIdentifiers.AddNew();
				special.CY_Code = "B01";
				var requiredDocs = instructionToAdd.CIQRequiredDocuments.AddNew();
				requiredDocs.XC_DocumentType = "12";
				requiredDocs.XC_NumberOfCopies = 4;
				requiredDocs.XC_NumberOfOriginals = 5;
				AssertCiqDataCleared("To make sure CIQ Data added successfully.", instructionToAdd, false);
			}

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CIQRequires = true;
			AddCIQDatas(instruction);
			Factory.Save();
			AssertCiqDataCleared("CEI_CIQRequires & WillGenerateEnteringEntry, should not clear CIQ datas.", instruction, false);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			AssertCiqDataCleared("WillGenerateEnteringEntry false and therefore should clear CIQ datas, but not before FactorySaving.", instruction, false);

			AddCIQDatas(instruction);
			instruction.CEI_CIQRequires = false;
			Factory.Save();
			AssertCiqDataCleared("CEI_CIQRequires false, should clear CIQ datas.", instruction, true);
		}

		public void TestWillGenerateEnteringEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", instruction.WillGenerateEnteringEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", instruction.WillGenerateEnteringEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", !instruction.WillGenerateEnteringEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", !instruction.WillGenerateEnteringEntry);
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			childInstruction.CEI_CEI_Parent = instruction.PK;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("EXP+BTH - Parent", instruction.WillGenerateEnteringEntry);
			Assert("EXP+BTH - Child", !childInstruction.WillGenerateEnteringEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert("IMP+BTH - Parent", instruction.WillGenerateEnteringEntry);
			Assert("IMP+BTH - Child", !childInstruction.WillGenerateEnteringEntry);
		}

		public void TestWillGenerateExitingEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", !instruction.WillGenerateExitingEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", !instruction.WillGenerateExitingEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", instruction.WillGenerateExitingEntry);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", instruction.WillGenerateExitingEntry);
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			childInstruction.CEI_CEI_Parent = instruction.PK;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("EXP+BTH - Parent", !instruction.WillGenerateExitingEntry);
			Assert("EXP+BTH - Child", childInstruction.WillGenerateExitingEntry);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert("IMP+BTH - Parent", !instruction.WillGenerateExitingEntry);
			Assert("IMP+BTH - Child", childInstruction.WillGenerateExitingEntry);
		}

		public void TestEntryType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("IMP+CUS", EntryTypeList.Codes.CustomsEntry, instruction.EntryType);
			AssertEquals("IMP+CUS", "进口报关单", instruction.EntryTypeDescription);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals("IMP+REC", EntryTypeList.Codes.RecordListing, instruction.EntryType);
			AssertEquals("IMP+REC", "进境备案清单", instruction.EntryTypeDescription);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			AssertEquals("EXP+CUS", EntryTypeList.Codes.CustomsEntry, instruction.EntryType);
			AssertEquals("EXP+CUS", "出口报关单", instruction.EntryTypeDescription);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			AssertEquals("EXP+REC", EntryTypeList.Codes.RecordListing, instruction.EntryType);
			AssertEquals("EXP+REC", "出境备案清单", instruction.EntryTypeDescription);
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			childInstruction.CEI_CEI_Parent = instruction.PK;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			AssertEquals("EXP+BTH - Parent", EntryTypeList.Codes.RecordListing, instruction.EntryType);
			AssertEquals("EXP+BTH - Parent", "进境备案清单", instruction.EntryTypeDescription);
			AssertEquals("EXP+BTH - Child", EntryTypeList.Codes.CustomsEntry, childInstruction.EntryType);
			AssertEquals("EXP+BTH - Child", "出口报关单", childInstruction.EntryTypeDescription);
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			AssertEquals("IMP+BTH - Parent", EntryTypeList.Codes.CustomsEntry, instruction.EntryType);
			AssertEquals("IMP+BTH - Parent", "进口报关单", instruction.EntryTypeDescription);
			AssertEquals("IMP+BTH - Child", EntryTypeList.Codes.RecordListing, childInstruction.EntryType);
			AssertEquals("IMP+BTH - Child", "出境备案清单", childInstruction.EntryTypeDescription);
		}

		public void TestParentAndChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				AssertNull("IMP+CUS - ParentInstruction", instruction.ParentInstruction);
				AssertNull("IMP+CUS - ChildInstruction", instruction.ChildInstruction);
				AssertEquals("IMP+CUS - ChildInstructionPK", ZGuid.Empty, instruction.ChildInstructionPK);
				Assert("IMP+CUS - IsParent", !instruction.IsParent);
				Assert("IMP+CUS - IsChild", !instruction.IsChild);
				Assert("IMP+CUS - CEI_CEI_Parent_ReadOnly", instruction.CEI_CEI_ParentInfo.ReadOnly);
				declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				AssertNull("IMP+REC - ParentInstruction", instruction.ParentInstruction);
				AssertNull("IMP+REC - ChildInstruction", instruction.ChildInstruction);
				AssertEquals("IMP+REC - ChildInstructionPK", ZGuid.Empty, instruction.ChildInstructionPK);
				Assert("IMP+REC - IsParent", !instruction.IsParent);
				Assert("IMP+REC - IsChild", !instruction.IsChild);
				Assert("IMP+REC - CEI_CEI_Parent_ReadOnly", instruction.CEI_CEI_ParentInfo.ReadOnly);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
				AssertNull("EXP+CUS - ParentInstruction", instruction.ParentInstruction);
				AssertNull("EXP+CUS - ChildInstruction", instruction.ChildInstruction);
				AssertEquals("EXP+CUS - ChildInstructionPK", ZGuid.Empty, instruction.ChildInstructionPK);
				Assert("EXP+CUS - IsParent", !instruction.IsParent);
				Assert("EXP+CUS - IsChild", !instruction.IsChild);
				Assert("EXP+CUS - CEI_CEI_Parent_ReadOnly", instruction.CEI_CEI_ParentInfo.ReadOnly);
				declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
				AssertNull("EXP+REC - ParentInstruction", instruction.ParentInstruction);
				AssertNull("EXP+REC - ChildInstruction", instruction.ChildInstruction);
				AssertEquals("EXP+REC - ChildInstructionPK", ZGuid.Empty, instruction.ChildInstructionPK);
				Assert("EXP+REC - IsParent", !instruction.IsParent);
				Assert("EXP+REC - IsChild", !instruction.IsChild);
				Assert("EXP+REC - CEI_CEI_Parent_ReadOnly", instruction.CEI_CEI_ParentInfo.ReadOnly);
				declaration.JE_MessageSubType = DecTypeList.Codes.Both;
				AssertNull("IMP+BTH - Orphon  - ParentInstruction", instruction.ParentInstruction);
				AssertNull("IMP+BTH - Orphon - ChildInstruction", instruction.ChildInstruction);
				AssertEquals("IMP+BTH - Orphon - ChildInstructionPK", ZGuid.Empty, instruction.ChildInstructionPK);
				Assert("IMP+BTH - Orphon - IsParent", !instruction.IsParent);
				Assert("IMP+BTH - Orphon - IsChild", !instruction.IsChild);
				Assert("IMP+BTH - Orphon - CEI_CEI_Parent_ReadOnly", !instruction.CEI_CEI_ParentInfo.ReadOnly);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				AssertNull("EXP+BTH - Orphon  - ParentInstruction", instruction.ParentInstruction);
				AssertNull("EXP+BTH - Orphon - ChildInstruction", instruction.ChildInstruction);
				AssertEquals("EXP+BTH - Orphon - ChildInstructionPK", ZGuid.Empty, instruction.ChildInstructionPK);
				Assert("EXP+BTH - Orphon - IsParent", !instruction.IsParent);
				Assert("EXP+BTH - Orphon - IsChild", !instruction.IsChild);
				Assert("EXP+BTH - Orphon - CEI_CEI_Parent_ReadOnly", !instruction.CEI_CEI_ParentInfo.ReadOnly);
				var childInstruction = declaration.CustomsEntryInstructions.AddNew();
				childInstruction.CEI_CEI_Parent = instruction.PK;
				AssertNull("IMP+BTH - Parent - ParentInstruction", instruction.ParentInstruction);
				AssertEquals("IMP+BTH - Parent - ChildInstruction", childInstruction, instruction.ChildInstruction);
				AssertEquals("IMP+BTH - Parent - ChildInstructionPK", childInstruction.PK, instruction.ChildInstructionPK);
				Assert("IMP+BTH - Parent - IsParent", instruction.IsParent);
				Assert("IMP+BTH - Parent - IsChild", !instruction.IsChild);
				Assert("IMP+BTH - Parent - CEI_CEI_Parent_ReadOnly", instruction.CEI_CEI_ParentInfo.ReadOnly);
				AssertEquals("IMP+BTH - Child - ParentInstruction", instruction, childInstruction.ParentInstruction);
				AssertNull("IMP+BTH - Child - ChildInstruction", childInstruction.ChildInstruction);
				AssertEquals("IMP+BTH - Child - ChildInstructionPK", ZGuid.Empty, childInstruction.ChildInstructionPK);
				Assert("IMP+BTH - Child - IsParent", !childInstruction.IsParent);
				Assert("IMP+BTH - Child - IsChild", childInstruction.IsChild);
				Assert("IMP+BTH - Child - CEI_CEI_Parent_ReadOnly", !childInstruction.CEI_CEI_ParentInfo.ReadOnly);
				declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
				AssertNull("EXP+BTH - Parent - ParentInstruction", instruction.ParentInstruction);
				AssertEquals("EXP+BTH - Parent - ChildInstruction", childInstruction, instruction.ChildInstruction);
				AssertEquals("EXP+BTH - Parent - ChildInstructionPK", childInstruction.PK, instruction.ChildInstructionPK);
				Assert("EXP+BTH - Parent - IsParent", instruction.IsParent);
				Assert("EXP+BTH - Parent - IsChild", !instruction.IsChild);
				Assert("EXP+BTH - Parent - CEI_CEI_Parent_ReadOnly", instruction.CEI_CEI_ParentInfo.ReadOnly);
				AssertEquals("EXP+BTH - Child - ParentInstruction", instruction, childInstruction.ParentInstruction);
				AssertNull("EXP+BTH - Child - ChildInstruction", childInstruction.ChildInstruction);
				AssertEquals("EXP+BTH - Child - ChildInstructionPK", ZGuid.Empty, childInstruction.ChildInstructionPK);
				Assert("EXP+BTH - Child - IsParent", !childInstruction.IsParent);
				Assert("EXP+BTH - Child - IsChild", childInstruction.IsChild);
				Assert("EXP+BTH - Child - CEI_CEI_Parent_ReadOnly", !childInstruction.CEI_CEI_ParentInfo.ReadOnly);
			}

			);
		}

		public void TestClearChildInstructionWhenRemoving()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			childInstruction.CEI_CEI_Parent = instruction.PK;
			instruction.Delete();
			AssertEquals("ChildInstruction.CEI_CEI_Parent should be cleared", ZGuid.Empty, childInstruction.CEI_CEI_Parent);
		}

		public void TestClearOtherObjsWhenDeleting()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var attachment = instruction.Attachments.AddNew();
			attachment.AttachmentType = "T1";
			var specialBusinessIdentifier = instruction.SpecialBusinessIdentifiers.AddNew();
			specialBusinessIdentifier.CY_Code = EnterpriseQualificationList.Codes._200;
			var enterpriseQua = instruction.EnterpriseQualifications.AddNew();
			enterpriseQua.CY_Code = EnterpriseQualificationList.Codes._200;
			var ciqReq = instruction.CIQRequiredDocuments.AddNew();
			ciqReq.XC_DocumentType = CIQRequiredDocumentTypeList.Codes._11;
			instruction.Delete();
			CombineAssertions(() =>
			{
				Assert("Attachments should have been deleted.", attachment.IsDeleted);
				Assert("SpecialBusinessIdentifiers should have been deleted.", specialBusinessIdentifier.IsDeleted);
				Assert("EnterpriseQualifications should have been deleted.", enterpriseQua.IsDeleted);
				Assert("CIQRequiredDocuments should have been deleted.", ciqReq.IsDeleted);
			}

			);
		}

		public void TestSettingCEI_CEI_Parent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction1.PK;
			instruction2.CEI_CEI_Parent = instruction1.PK;
			instruction1.CEI_CEI_Parent = instruction2.PK;
			Assert("CEI_CEI_Parent of Child should be clear", instruction2.CEI_CEI_Parent.IsEmpty);
			Assert("JI_CEI of Invoice Line linked to Child should be clear", invoiceLine.JI_CEI.IsEmpty);
			instruction3.CEI_CEI_Parent = instruction2.PK;
			Assert("CEI_CEI_Parent of old Parent should be clear", instruction1.CEI_CEI_Parent.IsEmpty);
		}

		public void TestEDocCollections()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "PDF");
			var eDoc2 = ((IDocManagerSupport)entry).DocManagerInfo.AddFileOrDocument(new byte[1], "Test2.pdf", "PDF");
			var eDoc3 = shipment.DocManagerInfo.AddFileOrDocument(new byte[1], "Test3.pdf", "PDF");
			AssertNotNull(instruction.Attachments);
			AssertEquals(3, instruction.EDocCollections.Count());
			Assert("ICusStorageDocPivotParent method", instruction.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc1.UniqueKey.ToGuid()) != null));
			Assert("ICusStorageDocPivotParent method", instruction.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc2.UniqueKey.ToGuid()) != null));
			Assert("ICusStorageDocPivotParent method", instruction.EDocCollections.Any(x => x.GetFromUniqueKey(eDoc3.UniqueKey.ToGuid()) != null));
			var newPivot = instruction.Attachments.CreateNewCusStorageDocPivot();
			newPivot.CSD_DocType = CSDDocTypeList.Codes._00000001;
			Factory.Save();
			var loadedPivot = new BusinessObjectFactory().Load<BaseCusStorageDocPivot>(newPivot.PK);
			AssertType("ICusStorageDocPivotTypeSupporter method, should have returned the correct type", typeof(CusStorageDocPivot), loadedPivot);
		}

		public void TestLinkedFormalEntryHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "CUS";
			AssertNull(instruction.LinkedFormalEntryHeader);
			entryHeader.CH_CEI_Instruction = instruction.PK;
			AssertNotNull(instruction.LinkedFormalEntryHeader);
			entryHeader.CH_MessageType = "PRE";
			AssertNull(instruction.LinkedFormalEntryHeader);
			entryHeader.CH_MessageType = "REC";
			AssertNotNull(instruction.LinkedFormalEntryHeader);
		}

		public void TestCEI_CIQRequires()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "ENTA";
			Assert(instruction.CEI_CIQRequiresInfo.ReadOnly);
			entry.EntryNumber = ZString.Empty;
			Assert(!instruction.CEI_CIQRequiresInfo.ReadOnly);
		}

		public void TestTriggerRequiresCIQ()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Assert(!instruction.CEI_CIQRequiresInfo.ReadOnly);
			Assert(!instruction.CEI_CIQRequires);
			instruction.CEI_CIQRelatedNum = "123";
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			instruction.CEI_CIQRelatedNum = ZString.Empty;
			Assert(!instruction.CEI_CIQRequires);
			instruction.CEI_CIQRelatedReason = "1";
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			instruction.CEI_CIQRelatedReason = ZString.Empty;
			Assert(!instruction.CEI_CIQRequires);
			var specialBusinessIdentifier = instruction.SpecialBusinessIdentifiers.AddNew();
			specialBusinessIdentifier.CY_Code = SpecialBusinessList.Codes.B01;
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			instruction.SpecialBusinessIdentifiers.RemoveAndDelete(specialBusinessIdentifier);
			Assert(!instruction.CEI_CIQRequires);
			var enterpriseQualification = instruction.EnterpriseQualifications.AddNew();
			enterpriseQualification.CY_Code = EnterpriseQualificationList.Codes._200;
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			instruction.EnterpriseQualifications.RemoveAndDelete(enterpriseQualification);
			Assert(!instruction.CEI_CIQRequires);
			var requiredDocument = instruction.CIQRequiredDocuments.AddNew();
			requiredDocument.XC_DocumentType = CIQRequiredDocumentTypeList.Codes._11;
			Assert(instruction.CEI_CIQRequires);
			instruction.CEI_CIQRequires = false;
			instruction.CIQRequiredDocuments.RemoveAndDelete(requiredDocument);
			Assert(!instruction.CEI_CIQRequires);
		}

		public void TestCNEntryInstructionSetupCorrectly()
		{
			var instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			Factory.Save();
			var caDeclaration = instruction.AddInfoChild;
			AssertEquals(instruction.PK, caDeclaration.CNE_CEI);
			AssertEquals(true, caDeclaration.IsInDatabase);
			AssertEquals(1, caDeclaration.CNE_ClusterKey);
			var newFactory = new BusinessObjectFactory();
			instruction = newFactory.Load<CusEntryInstruction>(instruction.PK);
			caDeclaration = instruction.AddInfoChild;
			AssertEquals(instruction.PK, caDeclaration.CNE_CEI);
			AssertEquals(true, caDeclaration.IsInDatabase);
			AssertEquals(1, caDeclaration.CNE_ClusterKey);
			instruction.Delete();
			AssertEquals(true, caDeclaration.IsDeleted);
			instruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			Factory.Save();
			caDeclaration = instruction.AddInfoChild;
			AssertEquals(instruction.PK, caDeclaration.CNE_CEI);
			AssertEquals(2, caDeclaration.CNE_ClusterKey);
		}

		public void TestDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CIQRequiredDocuments.AddNew();
			instruction.EnterpriseQualifications.AddNew();
			instruction.SpecialBusinessIdentifiers.AddNew();
			instruction.OtherPackages.AddNew();
			instruction.Attachments.AddNew().AttachmentType = CSDDocTypeList.Codes._00000001;
			instruction.Attachments.AddNew().AttachmentType = CSDDocTypeList.Codes._10000002;
			instruction.BillOfLading = "BL";
			Factory.Save();
			AssertEquals(1, Factory.Load<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, instruction.PK)).Length);
			AssertEquals(4, Factory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, instruction.PK)).Length);
			AssertEquals(1, Factory.Load<CusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, instruction.PK)).Length);
			AssertEquals(1, Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK)).Length);
			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.Load<CusEntryInstruction>(instruction.PK).Delete();
			AssertEquals(0, anotherFactory.Load<CusAddInfo>(new ZQuery(CusAddInfoSchema.B7_ParentID, instruction.PK)).Length);
			AssertEquals(0, anotherFactory.Load<CusCodeData>(new ZQuery(CusCodeDataSchema.CY_ParentID, instruction.PK)).Length);
			AssertEquals(0, anotherFactory.Load<CusStorageDocPivot>(new ZQuery(CusStorageDocPivotSchema.CSD_ParentID, instruction.PK)).Length);
			AssertEquals(0, anotherFactory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, instruction.PK)).Length);
		}

		public void TestIsCIQDataAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", instruction1.IsCIQDataAllowed);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", instruction1.IsCIQDataAllowed);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", instruction1.IsCIQDataAllowed);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", instruction1.IsCIQDataAllowed);

			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			Assert("BTH+Entering", !instruction1.IsCIQDataAllowed);
			Assert("BTH+Exiting", instruction2.IsCIQDataAllowed);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			Assert("BTH+Exiting", instruction1.IsCIQDataAllowed);
			Assert("BTH+Entering", !instruction2.IsCIQDataAllowed);
		}

		public void TestTwoStageAccessApplicable()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_CIQRequires = true;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("IMP+CUS", instruction1.TwoStageAccessApplicable);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("IMP+REC", instruction1.TwoStageAccessApplicable);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			Assert("EXP+CUS", !instruction1.TwoStageAccessApplicable);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("EXP+REC", !instruction1.TwoStageAccessApplicable);
		}

		public void TestClearTwoStageAccessDataIfNotApplicable()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_CIQRequires = true;
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

			instruction.CNE_ApplyForCombinedInspections = true;
			instruction.CNE_ApplyForConditionalPickup = true;
			instruction.CNE_ApplyForTransition = true;
			instruction.CNE_TransitionSite = "TransitionSite";

			Factory.Save();

			Assert("CNE_ApplyForCombinedInspections", instruction.CNE_ApplyForCombinedInspections);
			Assert("CNE_ApplyForConditionalPickup", instruction.CNE_ApplyForConditionalPickup);
			Assert("CNE_ApplyForTransition", instruction.CNE_ApplyForTransition);
			AssertEquals("CNE_TransitionSite", "TransitionSite", instruction.CNE_TransitionSite);

			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			Assert("TwoStageAccessApplicable", !instruction.TwoStageAccessApplicable);
			Factory.Save();

			Assert("CNE_ApplyForCombinedInspections cleared", !instruction.CNE_ApplyForCombinedInspections);
			Assert("CNE_ApplyForConditionalPickup cleared", !instruction.CNE_ApplyForConditionalPickup);
			Assert("CNE_ApplyForTransition cleared", !instruction.CNE_ApplyForTransition);
			AssertEquals("CNE_TransitionSite cleared", "", instruction.CNE_TransitionSite);
		}

		public void TestCusStorageDocPivots()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var storageDoc = Factory.New<CusStorageDocPivot>();
			storageDoc.Parent = instruction;

			AssertEquals(1, instruction.CusStorageDocPivots.Count);
		}

		public void TestCusAttachments()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var attachment = Factory.New<CusAttachment>();
			attachment.Parent = instruction;

			AssertEquals(1, instruction.CusAttachments.Count);
		}

		public void TestCEI_SubStyle()
		{
			AssertEquals(1, Factory.New<CusEntryInstruction>().CEI_SubStyleInfo.MaxLength);
		}
	}
}
