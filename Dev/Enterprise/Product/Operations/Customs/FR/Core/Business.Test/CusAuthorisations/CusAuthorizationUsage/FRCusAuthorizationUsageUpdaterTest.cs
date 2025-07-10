using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	class FRCusAuthorizationUsageUpdaterTest : TestCaseWithFactory
	{
		public void TestGetValidAuthorizationType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OPO", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("21P").FirstOrDefault());

				AssertEquals("OTO", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("22P").FirstOrDefault());

				AssertEquals("TEE", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("23P").FirstOrDefault());

				AssertEquals("IPO", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("51P").FirstOrDefault());

				AssertEquals("TEA", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("53P").FirstOrDefault());

				AssertEquals("CWP", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("71P").FirstOrDefault());
				AssertEquals("CWP", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("76").FirstOrDefault());
				AssertEquals("CWP", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("77").FirstOrDefault());
				AssertEquals("CWP", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("78").FirstOrDefault());
				AssertEquals("CWP", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("78P").FirstOrDefault());

				AssertEquals("SDE", FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("I1").FirstOrDefault());

				AssertNull(FRCusAuthorizationUsageUpdater.GetValidAuthorizationTypes("10").FirstOrDefault());
			});
		}

		public void TestUpdater()
		{
			CombineAssertions(() =>
			{
				TestPopulate("21P", "OPO");
				TestPopulate("22P", "OTO");
				TestPopulate("23P", "TEE");
				TestPopulate("44", "EUS");
				TestPopulate("44P", "EUS");
				TestPopulate("51P", "IPO");
				TestPopulate("53P", "TEA");
				TestPopulate("71P", "CWP");
				TestPopulate("76", "CWP");
				TestPopulate("77", "CWP");
				TestPopulate("78", "CWP");
				TestPopulate("78P", "CWP");
				TestPopulate("I1", "SDE");
			});
		}

		void TestPopulate(string declarationType, string expectedAuthorisationType)
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = declarationType;

			var singleUseAuthorisationHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
			singleUseAuthorisationHeader.CPH_Number = "22222222";
			singleUseAuthorisationHeader.CPH_IsSingleUse = true;
			singleUseAuthorisationHeader.CPH_Type = expectedAuthorisationType;
			singleUseAuthorisationHeader.CPH_OH_PermitHolder = importer.PK;
			singleUseAuthorisationHeader.CPH_RN_NKCountryCode = "FR";
			singleUseAuthorisationHeader.CPH_IsSingleUse = true;

			var nonSingleUseAuthorisationHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
			nonSingleUseAuthorisationHeader.CPH_Number = "12345678";
			nonSingleUseAuthorisationHeader.CPH_IsSingleUse = true;
			nonSingleUseAuthorisationHeader.CPH_Type = expectedAuthorisationType;
			nonSingleUseAuthorisationHeader.CPH_OH_PermitHolder = importer.PK;
			nonSingleUseAuthorisationHeader.CPH_RN_NKCountryCode = "FR";
			nonSingleUseAuthorisationHeader.CPH_IsSingleUse = false;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, 1, instruction.CusAuthorizationUsages.Count);
				var usage = instruction.CusAuthorizationUsages[0];
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, expectedAuthorisationType, usage.AGC_Code);
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, "12345678", usage.AuthorisationHeader.CPH_Number);
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, importer.PK, usage.AGC_OH_Owner);
			}

			nonSingleUseAuthorisationHeader.CPH_OH_PermitHolder = ZGuid.Empty;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, false))
			{
				var declarant = declaration.SetupDeclarant().Header.SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE001", ZString.Empty, ZString.Empty, "B26F06FF");
				var supplier = declaration.SetupSupplier().SetupAccount(OrgCusAccountCodeList.Codes.DGI, "A", "DIE002", ZString.Empty, ZString.Empty, "B26F06FF");
				var representative = Factory.NewWithValidTestData<OrgHeader>();
				representative.OH_Code = "FR0";
				var representativeAddress = representative.Addresses.AddNew();
				representativeAddress.AddressCode = "TestMatchAddress";
				representativeAddress.Address1 = "TestMatchAddress";
				declaration.JE_OA_Representative = representativeAddress.PK;

				var authorisationHeader1 = declaration.Declarant.SetupAuthorisationHeader(expectedAuthorisationType).WithNumber("Declarant1");
				var authorisationHeader2 = declaration.Representative.SetupAuthorisationHeader(expectedAuthorisationType).WithNumber("Representative");

				declaration.JE_DeclarantType = ZString.Empty;
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				AssertEquals("No corresponding Authorisation found for ActualClient, and JE_DeclarantType is empty." + expectedAuthorisationType, 0, instruction.CusAuthorizationUsages.Count);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				var usage = instruction.CusAuthorizationUsages[0];
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, expectedAuthorisationType, usage.AGC_Code);
				AssertEquals("No corresponding Authorisation found for ActualClient, and JE_DeclarantType is IND. AGC_OH_Owner will fallback to the declarant." + expectedAuthorisationType, "Declarant1", usage.AuthorisationHeader.CPH_Number);
				AssertEquals("No corresponding Authorisation found for ActualClient, and JE_DeclarantType is IND. AGC_OH_Owner will fallback to the declarant." + expectedAuthorisationType, declarant.Header.PK, usage.AGC_OH_Owner);

				var authorisationHeader3 = declaration.Declarant.SetupAuthorisationHeader(expectedAuthorisationType).WithNumber("Declarant2");
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				AssertEquals("Multiple matching Authorisations found for declarant, returning empty." + expectedAuthorisationType, 0, instruction.CusAuthorizationUsages.Count);

				declaration.JE_DeclarantType = RepresentationTypeList.Codes.DIR;
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				usage = instruction.CusAuthorizationUsages[0];
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, expectedAuthorisationType, usage.AGC_Code);
				AssertEquals("No corresponding Authorisation found for ActualClient, and JE_DeclarantType is DIR. AGC_OH_Owner will fallback to the Representative." + expectedAuthorisationType, "Representative", usage.AuthorisationHeader.CPH_Number);
				AssertEquals("No corresponding Authorisation found for ActualClient, and JE_DeclarantType is DIR. AGC_OH_Owner will fallback to the Representative." + expectedAuthorisationType, representative.PK, usage.AGC_OH_Owner);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				var authorisationHeader4 = declaration.Importer.SetupAuthorisationHeader(expectedAuthorisationType).WithNumber("Importer1");
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				usage = instruction.CusAuthorizationUsages[0];
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, expectedAuthorisationType, usage.AGC_Code);
				AssertEquals("When the MessageType is IMP, the ActualClient is the Importer." + expectedAuthorisationType, "Importer1", usage.AuthorisationHeader.CPH_Number);
				AssertEquals("When the MessageType is IMP, the ActualClient is the Importer." + expectedAuthorisationType, declaration.Importer.PK, usage.AGC_OH_Owner);

				var authorisationHeader5 = declaration.Importer.SetupAuthorisationHeader(expectedAuthorisationType).WithNumber("Importer2");
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				AssertEquals("Multiple matching Authorisations found for ActualClient, returning empty." + expectedAuthorisationType, 0, instruction.CusAuthorizationUsages.Count);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				var authorisationHeader6 = declaration.Supplier.SetupAuthorisationHeader(expectedAuthorisationType).WithNumber("Supplier");
				instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				usage = instruction.CusAuthorizationUsages[0];
				AssertEquals("Test for declaration type " + declarationType + " and authorisation type " + expectedAuthorisationType, expectedAuthorisationType, usage.AGC_Code);
				AssertEquals("When the MessageType is EXP, the ActualClient is the Supplier." + expectedAuthorisationType, "Supplier", usage.AuthorisationHeader.CPH_Number);
				AssertEquals("When the MessageType is EXP, the ActualClient is the Supplier." + expectedAuthorisationType, declaration.Supplier.PK, usage.AGC_OH_Owner);
				authorisationHeader1.Delete();
				authorisationHeader2.Delete();
				authorisationHeader3.Delete();
				authorisationHeader4.Delete();
				authorisationHeader5.Delete();
				authorisationHeader6.Delete();
			}
		}

		public void TestIgnoreAdhocAuthorisations()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "78";

			var authorisationHeader = Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "22222222";
			authorisationHeader.CPH_IsSingleUse = false;
			authorisationHeader.CPH_Type = "CWP";
			authorisationHeader.CPH_OH_PermitHolder = importer.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";
			authorisationHeader.CPH_IsAdHoc = false;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "53P";

			var authorisationHeader_adhoc = Factory.NewWithValidTestData<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader_adhoc.CPH_Number = "11111111";
			authorisationHeader_adhoc.CPH_IsSingleUse = false;
			authorisationHeader_adhoc.CPH_Type = "TEA";
			authorisationHeader_adhoc.CPH_OH_PermitHolder = importer.PK;
			authorisationHeader_adhoc.CPH_RN_NKCountryCode = "FR";
			authorisationHeader_adhoc.CPH_IsAdHoc = true;

			Factory.Save();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();

				AssertEquals(1, entryInstruction.CusAuthorizationUsages.Count);
				AssertNotNull("non adhoc AuthorisationHeader should be imported", entryInstruction.CusAuthorizationUsages[0].AuthorisationHeader);
				AssertEquals("non adhoc AuthorisationHeader number should be imported", "22222222", entryInstruction.CusAuthorizationUsages[0].AuthorisationHeader.CPH_Number);

				AssertEquals(1, entryInstruction2.CusAuthorizationUsages.Count);
				AssertNull("adhoc AuthorisationHeader should not be imported", entryInstruction2.CusAuthorizationUsages[0].AuthorisationHeader);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = importer.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G2;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TESTACC";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			authorisationUsageUpdater = new FRCusAuthorizationUsageUpdater(declaration);
		}

		OrgHeader importer;
		JobDeclaration declaration;
		FRCusAuthorizationUsageUpdater authorisationUsageUpdater;
	}
}
