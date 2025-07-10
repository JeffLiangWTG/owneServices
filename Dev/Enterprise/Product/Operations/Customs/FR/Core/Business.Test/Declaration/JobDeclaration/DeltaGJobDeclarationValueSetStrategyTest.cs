using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DeltaGJobDeclarationValueSetStrategy))]
	class DeltaGJobDeclarationValueSetStrategyTest : JobDeclarationValueSetStrategyAbstractTest
	{
		public override void TestDefaultJE_DeltaMode()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.JE_OH_Importer = importer.PK;

			declaration.JE_CustomsProfile = "DGI001";
			AssertEquals("Delta mode can be inferred from profile when there is one and only one account linked to the profile.", OrgCusAccountDeltaGTypeList.Codes.G1, declaration.JE_DeltaMode);

			declaration.JE_CustomsProfile = ZString.Empty;
			AssertEquals("Delta Mode should be empty when no profile is selected.", ZString.Empty, declaration.JE_DeltaMode);

			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G2, "DGI001", ZString.Empty, ZString.Empty, "B26F06FF");
			var declaration2 = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			declaration2.JE_OH_Importer = importer.PK;
			declaration2.JE_CustomsProfile = "DGI001";
			AssertEquals("Delta mode can't be inferred from profile when there is more than one account linked to the profile. ", ZString.Empty, declaration2.JE_DeltaMode);
		}

		public override void TestDefaultJE_CustomsProfile()
		{
			var declaration = Factory.New<JobDeclaration>().WithFlux(EU.Business.MessageTypeList.Codes.Import);
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Importer = importer.PK;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI000", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			AssertEquals("No account is available in importer setup, so the one from declarant should be used for customs profile", "DGI000", declaration.JE_CustomsProfile);

			declaration.JE_CustomsProfile = ZString.Empty;
			declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			AssertNull(declaration.Declarant);
			AssertEquals("No account is available in importer setup, and the declarant is null", "", declaration.JE_CustomsProfile);

			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, ZString.Empty, "DGI001", ZString.Empty, ZString.Empty, "B26F06FF");
			declaration.JE_OH_Importer = ZGuid.Empty;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("An authorization is available in importer setup, it should be used for customs profile", "DGI001", declaration.JE_CustomsProfile);
		}

		public override void TestDefaultJE_DeclarationLanguage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			AssertEquals("JE_DeclarationLanguage should be empty for DeltaG declarations.", ZString.Empty, declaration.JE_DeclarationLanguage);
		}

		public void TestCEI_SubStyleUpdatedWhenDateOfArrivalOrDeltaModeChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var cei = declaration.CustomsEntryInstructions.FirstOrDefault<CusEntryInstruction>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;

			cei.EntryHeader.EntryNumber = "";

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals(EntrySubstyleCodePairList.Codes.D, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.A, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.D, cei.CEI_SubStyle);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals(EntrySubstyleCodePairList.Codes.F, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(-1);
			AssertEquals(EntrySubstyleCodePairList.Codes.C, cei.CEI_SubStyle);

			declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(1);
			AssertEquals(EntrySubstyleCodePairList.Codes.F, cei.CEI_SubStyle);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			AssertEquals(EntrySubstyleCodePairList.Codes.D, cei.CEI_SubStyle);

			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			AssertEquals(EntrySubstyleCodePairList.Codes.F, cei.CEI_SubStyle);

			declaration.JE_DeltaMode = "AA";
			AssertEquals(EntrySubstyleCodePairList.Codes.F, cei.CEI_SubStyle);

			cei.EntryHeader.EntryNumber = "123";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("Entry Number has a value so no change", EntrySubstyleCodePairList.Codes.F, cei.CEI_SubStyle);

			cei.EntryHeader.EntryNumber = "";
			declaration.JE_DateOfArrival = ZDateTime.Now.AddDays(-1);
			AssertEquals("Entry Number blank so should be a change", EntrySubstyleCodePairList.Codes.C, cei.CEI_SubStyle);

			declaration.ActiveEntryHeaders.RemoveAndDeleteAll();
			declaration.JE_DateOfArrival = ZDateTime.Empty;
			AssertEquals("Entry Header is null so should be a change", EntrySubstyleCodePairList.Codes.F, cei.CEI_SubStyle);
		}
	}
}
