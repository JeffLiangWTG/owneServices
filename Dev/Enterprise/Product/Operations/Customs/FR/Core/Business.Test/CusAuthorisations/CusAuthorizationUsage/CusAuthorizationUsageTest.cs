using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	[TestedType(typeof(CusAuthorizationUsage))]
	sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestValueSetStrategy()
		{
			var standAloneCusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			AssertType<CusAuthorizationUsageValueSetStrategy>(standAloneCusAuthorizationUsage.ValueSetStrategy);

			var deltaIEJobDeclaration = Factory.New<JobDeclaration>();
			deltaIEJobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var deltaIEInstruction = deltaIEJobDeclaration.CustomsEntryInstructions.AddNew();
			var deltaIECusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			deltaIECusAuthorizationUsage.AGC_ParentID = deltaIEInstruction.PK;
			deltaIECusAuthorizationUsage.AGC_ParentTableCode = "CEI";
			AssertType<DeltaIECusAuthorizationUsageValueSetStrategy>(deltaIECusAuthorizationUsage.ValueSetStrategy);

			var deltaGJobDeclaration = Factory.New<JobDeclaration>();
			deltaGJobDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var deltaGInstruction = deltaGJobDeclaration.CustomsEntryInstructions.AddNew();
			var deltaGCusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			deltaGCusAuthorizationUsage.AGC_ParentID = deltaGInstruction.PK;
			deltaGCusAuthorizationUsage.AGC_ParentTableCode = "CEI";
			AssertType<CusAuthorizationUsageValueSetStrategy>(deltaGCusAuthorizationUsage.ValueSetStrategy);
		}

		class CusAuthorizationUsageForTest : CusAuthorizationUsage
		{
			public CusAuthorizationUsageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public IValueSetStrategy ValueSetStrategy => base.GetValueSetStrategy();
		}

		public void TestJobDeclaration()
		{
			var cusAuthorizationUsageNotAssingned = Factory.New<CusAuthorizationUsage>();
			AssertNull("jobDeclaration should be null  as cusAuthorizationUsage has not yet been set.", cusAuthorizationUsageNotAssingned.JobDeclaration);
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			var entryInsCusAuthorizationUsage = jobDeclaration.CustomsEntryInstructions.AddNew().CusAuthorizationUsages.AddNew();
			AssertEquals("The correct jobDeclaration from CustomsEntryInstructions should be linked", jobDeclaration, entryInsCusAuthorizationUsage.JobDeclaration);
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.MiscellaneousCustoms;
			var invoiceLineCusAuthorizationUsage = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew().CusAuthorizationUsages.AddNew();
			AssertEquals("The correct jobDeclaration from InvoiceLines should be linked", jobDeclaration, invoiceLineCusAuthorizationUsage.JobDeclaration);
		}

		public void TestAGC_AuthorizationShortCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var cusAuthorizationUsage = Factory.NewWithValidTestData<CusAuthorizationUsage>();
			cusAuthorizationUsage.AGC_Number = "12345678";
			cusAuthorizationUsage.AGC_Code = "OPO";
			cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;

			AssertEquals("Prerequisite: authorization usage related authorization must be null.", null, cusAuthorizationUsage.RelatedAuthorisationHeader);
			AssertEquals("AGC_AuthorizationShortCode should be empty when no related authorization is found for the authorization usage.", ZString.Empty, cusAuthorizationUsage.AGC_AuthorizationShortCode);

			var authorisationHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
			authorisationHeader.CPH_Number = "12345678";
			authorisationHeader.CPH_Type = "OPO";
			authorisationHeader.CPH_OH_PermitHolder = orgHeader.PK;
			authorisationHeader.CPH_RN_NKCountryCode = "FR";
			authorisationHeader.CPH_StartDate = ZDate.Today;
			authorisationHeader.CPH_EndDate = ZDate.Today;
			authorisationHeader.CPH_IsActive = true;
			Factory.Save();

			AssertEquals("Prerequisite: authorization usage must have a related authorization.", authorisationHeader, cusAuthorizationUsage.RelatedAuthorisationHeader);
			AssertEquals("AGC_AuthorizationShortCode should be empty when related authorization has no AUT rule.", ZString.Empty, cusAuthorizationUsage.AGC_AuthorizationShortCode);

			var autRule = authorisationHeader.CusAuthorisationRules.AddNew();
			autRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
			autRule.CPR_ValueFrom = "ZZZZZ";
			AssertEquals("AGC_AuthorizationShortCode should return AUT rule CPR_ValueFrom from related authorization.", "ZZZZZ", cusAuthorizationUsage.AGC_AuthorizationShortCode);
		}

		public void TestPopulateAndSendAuthorizations_EndToEnd()
		{
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGE;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = exporter.PK;
			orgCusAccount.CZ_Type = OrgCusAccountDeltaGTypeList.Codes.G1;

			var nonSingleUseAuthorisationHeader = Factory.New<Customs.Business.CusAuthorisationHeader>();
			nonSingleUseAuthorisationHeader.CPH_Number = "12345678";
			nonSingleUseAuthorisationHeader.CPH_Type = "OPO";
			nonSingleUseAuthorisationHeader.CPH_OH_PermitHolder = exporter.PK;
			nonSingleUseAuthorisationHeader.CPH_RN_NKCountryCode = "FR";
			nonSingleUseAuthorisationHeader.CPH_IsSingleUse = false;

			var autRule = nonSingleUseAuthorisationHeader.CusAuthorisationRules.AddNew();
			autRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.AUT;
			autRule.CPR_ValueFrom = "ZZZZZ";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			declaration.JE_OH_Supplier = exporter.PK;
			declaration.JE_CustomsProfile = "TESTACC";
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;

			var previousDocument = declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes.CLE;
			previousDocument.CSI_ReferenceNumber = "PRV0001";

			var instruction = declaration.CustomsEntryInstructions[0];
			instruction.CEI_Style = "21P";

			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];

			var authorisationUsageUpdater = new FRCusAuthorizationUsageUpdater(declaration);

			CombineAssertions(() =>
			{
				AssertEquals("No authorizations should exist", 0, instruction.CusAuthorizationUsages.Count);
				authorisationUsageUpdater.UpdateEntryInstructionAuthorizations();
				AssertEquals("After UpdateEntryInstructionAuthorizations 1 authorization should be created", 1, instruction.CusAuthorizationUsages.Count);
				var usage = instruction.CusAuthorizationUsages[0];
				AssertEquals("AGC_Number should be empty", ZString.Empty, usage.AGC_Number);
				AssertEquals("AGC_CPH_Authorization should contain data from nonSingleUseAuthorisationHeader", nonSingleUseAuthorisationHeader.PK, usage.AGC_CPH_Authorization);

				var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
				sendingObject.MessageType = "ANT";
				var errCollector = new EU.Business.ErrorCollector();
				var messageBuilderManager = new DeltaGMessageBuilderManager(errCollector);
				var messageBuilder = messageBuilderManager.NewMessageBuilder(sendingObject);
				var message = messageBuilder.GetMessage();

				AssertContains("Should contain element AutorisationEco", "<AutorisationEco>", message);
				AssertContains("Should contain element RegimeEco", "<RegimeEco>", message);
			});
		}

		public void TestCreateTemporaryAuthorisationFromUsage()
		{
			var authorisationUsage = Factory.New<CusAuthorizationUsage>();
			authorisationUsage.AGC_Number = "12345";
			authorisationUsage.AGC_OH_Owner = ZGuid.BrettsGuid;
			authorisationUsage.AGC_Code = "BLA";
			var temporaryAuthorisation = authorisationUsage.CreateTemporaryAuthorisationFromUsage();
			CombineAssertions("Temporary authorisation created form usage properties", () =>
			{
				AssertEquals("CPH_IsAdHoc", true, temporaryAuthorisation.CPH_IsAdHoc);
				AssertEquals("CPH_Number", "S/DECLARATION", temporaryAuthorisation.CPH_Number);
				AssertEquals("CPH_OH_PermitHolder", ZGuid.BrettsGuid, temporaryAuthorisation.CPH_OH_PermitHolder);
				AssertEquals("CPH_Type", "BLA", temporaryAuthorisation.CPH_Type);
			});
			Assert("Temporary Authorisation should be created by a dedicated Factory.", authorisationUsage.Factory != temporaryAuthorisation.Factory);
		}

		public void TestUseEffectiveReferenceNumber() => AssertEquals(true, Factory.New<CusAuthorizationUsage>().UseEffectiveReferenceNumber);
	}
}
