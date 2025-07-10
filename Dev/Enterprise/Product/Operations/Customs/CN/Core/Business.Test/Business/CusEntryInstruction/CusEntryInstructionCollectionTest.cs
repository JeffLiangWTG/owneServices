using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection))]
	class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForFirstChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TotalNoOfPacks = 10;
			declaration.JE_TotalNoOfPacksPackType = Core.Constants.PkgUnit.Bag;
			var instructionCollection = new CusEntryInstructionCollection(declaration);
			var instruction = instructionCollection.AddNew();
			AssertEquals(10, instruction.CEI_Packages);
			AssertEquals(PackageType.Codes.Bag, instruction.CEI_PackageUQ);
		}

		public void TestSetDefaultsFromPreviousEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instructionCollection = new CusEntryInstructionCollection(declaration);
			var instruction1 = instructionCollection.AddNew();
			instruction1.CEI_Packages = 10;
			instruction1.CEI_PackageUQ = PackageType.Codes.Bag;
			var instruction2 = instructionCollection.AddNew();
			AssertEquals(PackageType.Codes.Bag, instruction2.CEI_PackageUQ);
		}

		public void TestSetDefaultsForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms, instruction.CEI_DocumentSubmissionType);
			instruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.PaperlessWithList;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(EntryDocumentSubmissionTypes.Codes.PaperlessWithList, instruction.CEI_DocumentSubmissionType);
			Assert(instruction.CEI_EnterprisePromised);
			var importer = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var orgAddInfo = declaration.OrgImpAddInfo;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertNull("Not exists when ZO_IsConsolidatedDutyCollection is false", instruction.OperationMatters.GetFirstElementHaving(OperationMatterList.Codes.ConsolidatedDutyCollection));
			orgAddInfo.ZO_IsConsolidatedDutyCollection = true;
			orgAddInfo.ZO_IsAssuredInspectClearance = true;
			orgAddInfo.ZO_IntelligentDeclarationType = IntelligentDeclarationTypeList.Codes.Add;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertNotNull("Exists", instruction.OperationMatters.GetFirstElementHaving(OperationMatterList.Codes.ConsolidatedDutyCollection));
			AssertNotNull("Exists", instruction.OperationMatters.GetFirstElementHaving(OperationMatterList.Codes.AssuredInspectClearance));
			AssertEquals("CEI_SubStyle copied from ZO_IntelligentDeclarationType", IntelligentDeclarationTypeList.Codes.Add, instruction.CEI_SubStyle);
			declaration.JE_MessageSubType = EntryTypeList.Codes.RecordListing;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertNull("Not exists when WillGenerateRecordListing is true", instruction.OperationMatters.GetFirstElementHaving(OperationMatterList.Codes.ConsolidatedDutyCollection));
			AssertNotNull("Exists when WillGenerateRecordListing is true", instruction.OperationMatters.GetFirstElementHaving(OperationMatterList.Codes.AssuredInspectClearance));
			AssertEquals("CEI_SubStyle copied from ZO_IntelligentDeclarationType", IntelligentDeclarationTypeList.Codes.Add, instruction.CEI_SubStyle);
		}

		public void TestSettingParentInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("CEI_CEI_Parent should NOT be set", ZGuid.Empty, instruction2.CEI_CEI_Parent);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("CEI_CEI_Parent should be set to previous instruction", instruction2.PK, instruction3.CEI_CEI_Parent);
			var instruction4 = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("CEI_CEI_Parent should NOT be set", ZGuid.Empty, instruction4.CEI_CEI_Parent);
		}

		public void TestTriggerRequiresCIQOnAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			Assert(!instruction.CEI_CIQRequires);
			declaration.OfficeOfDestination = "1100";
			instruction = declaration.CustomsEntryInstructions.AddNew();
			Assert(instruction.CEI_CIQRequires);
		}

		public void TestSetDefaultsFromLinkAddInfoForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions("set defaults from link addinfo for new child", () =>
			{
				Assert(instruction1.CEI_ManualNo.IsEmpty);
				Assert(instruction1.CEI_Style.IsEmpty);
				Assert(instruction1.CEI_LevyType.IsEmpty);
			}

			);
			var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, declaration.CountryCode);
			var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			var otherUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			var consignee = OrgHeader.New(Factory);
			consignee.OH_RL_NKClosestPort = otherUnloco.RL_Code;
			var consignor = OrgHeader.New(Factory);
			declaration.JE_OH_Supplier = consignor.PK;
			declaration.JE_OH_Importer = consignee.PK;
			var link = consignee.SupplierLinks.AddNew(consignor);
			link.OL_RN_NKImporterCountry = otherCountry.RN_Code;
			AssertSame(link, declaration.SupplierImporterLink);
			var linkAddInfo = declaration.SupplierImporterLink.GetAddInfo();
			linkAddInfo.ZO_ManualNo = "A12345678901";
			linkAddInfo.ZO_ProcedureCode = "10";
			linkAddInfo.ZO_LevyType = "501";
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			CombineAssertions("set defaults from link addinfo for new child", () =>
			{
				AssertEquals(linkAddInfo.ZO_ManualNo, instruction2.CEI_ManualNo);
				AssertEquals(linkAddInfo.ZO_ProcedureCode, instruction2.CEI_Style);
				AssertEquals(linkAddInfo.ZO_LevyType, instruction2.CEI_LevyType);
			}

			);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new CusEntryInstructionCollection(testDeclaration);
		}
	}
}
