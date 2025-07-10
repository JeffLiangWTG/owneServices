using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GBCommonConstants;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSFinalSupplementaryDeclarationHelper))]
	public class CDSFinalSupplementaryDeclarationHelperTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			return new CDSFinalSupplementaryDeclarationHelper(dec);
		}

		[TestDate(2011, 01, 12)]
		public void TestCDSFinalSupplementaryDeclarationHelperDefaults()
		{
			var dec = Factory.New<JobDeclaration>();
			var helper = new CDSFinalSupplementaryDeclarationHelper(dec);

			CombineAssertions("CDSFinalSupplementaryDeclarationHelper defaults", () =>
			{
				AssertEquals("default to 1st day of current month", new ZDate(2011, 01, 1), helper.DueDate);
				AssertEquals("default to 1st day of previous month", new ZDate(2010, 12, 1), helper.StartOfPeriod);
				AssertEquals("default to branch org proxy", dec.Branch.OrgProxy.MainAddress.PK, helper.DeclarantAddressPK);
				AssertEquals("default to branch org proxy", dec.Branch.OrgProxy.MainAddress.PK, helper.AuthorisationHolderAddressPK);
				AssertEquals("default to SDE", CDSAuthorisationHeaderTypeList.Codes.SimplifiedDeclaration, helper.AuthorisationType);
			});
		}

		[TestDate(2010, 11, 12)]
		public void TestProperties()
		{
			var warehouse = Factory.NewWithValidTestData<OrgAddress>();
			var declarant = Factory.NewWithValidTestData<OrgAddress>();
			var importer = Factory.NewWithValidTestData<OrgAddress>();
			var dec = Factory.New<JobDeclaration>();
			var helper = new CDSFinalSupplementaryDeclarationHelper(dec);

			helper.DueDate = ZDate.Today;
			helper.DeclarantAddressPK = declarant.PK;
			helper.AuthorisationHolderAddressPK = warehouse.PK;
			helper.AuthorisationType = "AAA";
			helper.NumberOfTypeYDeclarationsDue = 1;
			helper.NumberOfTypeYDeclarationsSubmitted = 2;
			helper.NumberOfTypeZDeclarationsDue = 3;
			helper.NumberOfTypeZDeclarationsSubmitted = 4;
			helper.CDSFinalSupplementaryDeclarationHelperLateChildCollection.AddNew();
			helper.CDSFinalSupplementaryDeclarationHelperLateChildCollection.AddNew();
			helper.ImporterPK = importer.PK;
			helper.FindForImporterLinkedOrg = true;
			helper.FindOnlyForAuthHolder = true;
			helper.FindOnlyForAuthType = true;
			helper.FindOnlyForDueDate = true;

			AssertEquals(new ZDate(2010, 11, 12), helper.DueDate);
			AssertEquals(new ZDate(2010, 10, 1), helper.StartOfPeriod);
			AssertEquals(declarant.PK, helper.DeclarantAddressPK);
			AssertEquals(warehouse.PK, helper.AuthorisationHolderAddressPK);
			AssertEquals("AAA", helper.AuthorisationType);
			AssertEquals(1, helper.NumberOfTypeYDeclarationsDue);
			AssertEquals(2, helper.NumberOfTypeYDeclarationsSubmitted);
			AssertEquals(3, helper.NumberOfTypeZDeclarationsDue);
			AssertEquals(4, helper.NumberOfTypeZDeclarationsSubmitted);
			AssertEquals(2, helper.CDSFinalSupplementaryDeclarationHelperLateChildCollection.Count);
			AssertEquals(importer.PK, helper.ImporterPK);
			AssertEquals(true, helper.FindForImporterLinkedOrg);
			AssertEquals(true, helper.FindOnlyForAuthHolder);
			AssertEquals(true, helper.FindOnlyForAuthType);
			AssertEquals(true, helper.FindOnlyForDueDate);
		}

		public void TestCreateCDSFsd()
		{
			var dec = Factory.New<JobDeclaration>();
			var declarantAddress = Factory.NewWithValidTestData<OrgAddress>();
			var authorisationHolderAddress = Factory.NewWithValidTestData<OrgAddress>();
			var cdsFsdManager = new CDSFinalSupplementaryDeclarationHelperManager(dec);
			cdsFsdManager.CDSFsdWizard.DeclarantAddressPK = declarantAddress.PK;
			cdsFsdManager.CDSFsdWizard.AuthorisationHolderAddressPK = authorisationHolderAddress.PK;
			ZDateTime dueDate = new ZDateTime(2022, 06, 20);
			cdsFsdManager.CDSFsdWizard.DueDate = dueDate;
			cdsFsdManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted = 4;
			cdsFsdManager.CDSFsdWizard.NumberOfTypeYDeclarationsDue = 5;
			cdsFsdManager.CDSFsdWizard.NumberOfTypeZDeclarationsSubmitted = 6;
			cdsFsdManager.CDSFsdWizard.NumberOfTypeZDeclarationsDue = 7;

			cdsFsdManager.CreateCDSFsd();
			AssertEquals("Created with warning status", CdsFsdCreatedResult.Warning, cdsFsdManager.ExecutionResult);

			AssertEquals("Defaulted", "IMP", dec.JE_MessageType);
			AssertEquals("Defaulted", "CDS", dec.JE_CustomsProfile);
			AssertEquals("Defaulted", "CDS", dec.JE_ApplicationCode);
			AssertEquals(declarantAddress.PK, dec.JE_OA_DeclarantAddress);
			AssertEquals("Q", dec.CusEntryInstruction.CEI_SubStyle);
			AssertEquals("FS", dec.CusEntryInstruction.CEI_Style);

			AssertEquals(1, dec.Invoices.Count);
			AssertEquals(1, dec.InvoiceLines.Count);

			var invoiceHeader = dec.Invoices[0];
			AssertEquals("FSD FOR 2022-06-20", invoiceHeader.JZ_InvoiceNumber);

			var invoiceLine = dec.InvoiceLines[0];
			AssertEquals("009097F", invoiceLine.JI_Procedure);
			AssertEquals("3 AdditionalInfos created", 3, invoiceLine.AdditionalInfos.Count);

			AssertEquals("FINSL", invoiceLine.AdditionalInfos[2].CSI_Code);
			AssertEquals("0/0", invoiceLine.AdditionalInfos[2].CSI_Description);

			var child1 = new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
			child1.DueDate = new ZDateTime(2021, 01, 01);
			child1.NumberOfTypeYDeclarations = 1;
			child1.NumberOfTypeZDeclarations = 10;
			cdsFsdManager.CDSFsdWizard.CDSFinalSupplementaryDeclarationHelperLateChildCollection.Add(child1);

			var child2 = new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
			child2.DueDate = new ZDateTime(2021, 02, 04);
			child2.NumberOfTypeYDeclarations = 2;
			child2.NumberOfTypeZDeclarations = 20;
			cdsFsdManager.CDSFsdWizard.CDSFinalSupplementaryDeclarationHelperLateChildCollection.Add(child2);

			var child3 = new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
			child3.DueDate = new ZDateTime(2021, 02, 06);
			child3.NumberOfTypeYDeclarations = 3;
			child3.NumberOfTypeZDeclarations = 30;
			cdsFsdManager.CDSFsdWizard.CDSFinalSupplementaryDeclarationHelperLateChildCollection.Add(child3);

			var child4 = new CDSFinalSupplementaryDeclarationHelperLateChild(Factory);
			child4.DueDate = new ZDateTime(2021, 03, 09);
			child4.NumberOfTypeYDeclarations = 4;
			child4.NumberOfTypeZDeclarations = 40;
			cdsFsdManager.CDSFsdWizard.CDSFinalSupplementaryDeclarationHelperLateChildCollection.Add(child4);

			var authorization = Factory.New<CusAuthorisationHeader>();
			authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorization.CPH_Number = "7777777";
			authorization.CPH_StartDate = ZDate.BrettsBirthday;

			ZAddress authorisationAddress = new ZAddress(cdsFsdManager.CDSFsdWizard.AuthorisationHolderAddressPKInfo);
			authorization.CPH_OH_PermitHolder = authorisationAddress.OrgHeader.PK;
			Factory.Save();

			cdsFsdManager.CreateCDSFsd();
			invoiceLine = dec.InvoiceLines[0];

			AssertEquals("Defaulted to Authorisation Number", "7777777", dec.CusEntryInstruction.CusAuthorizationUsages[0].AGC_Number);
			AssertEquals("Authorisation Type", "SDE", dec.CusEntryInstruction.CusAuthorizationUsages[0].AGC_Code);
			AssertEquals("Successfully created", CdsFsdCreatedResult.Success, cdsFsdManager.ExecutionResult);

			AssertEquals("FINSY", invoiceLine.AdditionalInfos[0].CSI_Code);
			AssertEquals("SDY=4/5", invoiceLine.AdditionalInfos[0].CSI_Description);
			AssertEquals("FINSZ", invoiceLine.AdditionalInfos[1].CSI_Code);
			AssertEquals("SDZ=6/7", invoiceLine.AdditionalInfos[1].CSI_Description);
			AssertEquals("FINSL", invoiceLine.AdditionalInfos[2].CSI_Code);
			var description = invoiceLine.AdditionalInfos[2].CSI_Description;
			CombineAssertions("LateChild Grouping by YY/MM", () =>
			{
				AssertContains("Jan 2021 type Y, count 1 (child1)", "01/21=Y 1", description);
				AssertContains("Jan 2021 type Z, count 10 (child1)", "01/21=Z 10", description);
				AssertContains("Feb 2021 type Y, count 5 (child2 & child3)", "02/21=Y 5", description);
				AssertContains("Feb 2021 type Z, count 50 (child2 & child3)", "02/21=Z 50", description);
				AssertContains("Mar 2021 type Y, count 4 (child4)", "03/21=Y 4", description);
				AssertContains("Mar 2021 type Z, count 40 (child4)", "03/21=Z 40", description);
				AssertEquals("01/21=Y 1 01/21=Z 10 02/21=Y 5 02/21=Z 50 03/21=Y 4 03/21=Z 40", description);
			});
			AssertEquals("Tax Point", dueDate, dec.JE_EntryAuthorisationDate);

			AssertEquals("Previous Document Added", 1, dec.PreviousDocuments.Count);
			AssertEquals("Previous Document Code", "ZZZ", dec.PreviousDocuments[0].CSI_Code);
			AssertEquals("Previous Document SubType", "Z", dec.PreviousDocuments[0].CSI_SubType);
			AssertEquals("Previous Document Reference", "FSD", dec.PreviousDocuments[0].CSI_ReferenceNumber);

			AssertEquals(1, dec.CustomsEntryHeaders[0].AllEntryLines.Count);

			authorization.CPH_Number = "";
			authorization.CPH_OH_PermitHolder = ZGuid.Empty;
			cdsFsdManager.CreateCDSFsd();

			AssertEquals("Set to default authorisation number", CDSFinalSupplementaryDeclarationHelperManager.DefaultAuthorisationNumber, dec.CusEntryInstruction.CusAuthorizationUsages[0].AGC_Number);
			AssertEquals("Created with warning status", CdsFsdCreatedResult.Warning, cdsFsdManager.ExecutionResult);
		}

		public void TestCountTimelyEntries()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			var decToFind = Factory.New<JobDeclaration>();
			decToFind.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			decToFind.JE_DeclarationReference = "B68";
			var ceh = decToFind.CustomsEntryHeaders.AddNew();
			ceh.CH_CEI_Instruction = decToFind.CustomsEntryInstructions[0].PK;
			var gbUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom));
			OrgHeader importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			importer.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			var importerAddress = importer.MainAddress;
			decToFind.JE_OH_Importer = importer.PK;
			decToFind.JE_MessageType = "IMP";
			decToFind.JE_EidrType = EidrTypeList.Codes.CFS;
			decToFind.CusEntryInstruction.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			ceh.EntryNumber = "071-123456A";
			ceh.CusEntryNumber.CE_IssueDate = ZDateTime.Now;

			var decToFindWithoutEidr = Factory.New<JobDeclaration>();
			decToFindWithoutEidr.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			decToFindWithoutEidr.JE_DeclarationReference = "B69";
			var ceh2 = decToFindWithoutEidr.CustomsEntryHeaders.AddNew();
			ceh2.CH_CEI_Instruction = decToFindWithoutEidr.CustomsEntryInstructions[0].PK;
			decToFindWithoutEidr.JE_OH_Importer = importer.PK;
			decToFindWithoutEidr.JE_MessageType = "IMP";
			decToFindWithoutEidr.CusEntryInstruction.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			ceh2.EntryNumber = "071-123456B";
			ceh2.CusEntryNumber.CE_IssueDate = ZDateTime.Now;

			Factory.Save();

			var decFsd = Factory.New<JobDeclaration>();
			var wizardManager = new CDSFinalSupplementaryDeclarationHelperManager(decFsd);
			wizardManager.CDSFsdWizard.AuthorisationHolderAddressPK = importer.MainAddress.PK;
			wizardManager.CDSFsdWizard.StartOfPeriod = ZDateTime.Empty;
			wizardManager.CDSFsdWizard.DueDate = ZDateTime.Empty;
			wizardManager.CountTimelyEntries();
			AssertHasError("Mandatory validation check entered", wizardManager.CDSFsdWizard.StartOfPeriodInfo, "Please enter a Start of Period.");
			AssertHasError("Mandatory validation check entered", wizardManager.CDSFsdWizard.DueDateInfo, "Please enter a Due Date.");
			AssertEquals("Should find no decs - invalid date range", 0, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			wizardManager.CDSFsdWizard.StartOfPeriod = ZDateTime.Today.AddMonths(-3);
			wizardManager.CDSFsdWizard.DueDate = ZDateTime.Today.AddMonths(-2);
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find no decs - wrong date range", 0, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

			wizardManager.CDSFsdWizard.DueDate = ZDateTime.Today.AddMonths(1);
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find 2 dec - correct date range", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

			decToFindWithoutEidr.JE_EidrType = EidrTypeList.Codes.SCD;
			Factory.Save();
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find 1 dec", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

			var newOrgHeader = Factory.New<OrgHeader>();
			newOrgHeader.OH_Code = "ABC";
			newOrgHeader.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			Factory.Save();
			wizardManager.CDSFsdWizard.FindForImporterLinkedOrg = true;
			wizardManager.CDSFsdWizard.AuthorisationHolderAddressPK = newOrgHeader.MainAddress.PK;
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find no dec - wrong authorisation holder", 0, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

			var authorization = Factory.New<CusAuthorisationHeader>();
			authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorization.CPH_Number = "11111";
			authorization.CPH_OH_PermitHolder = newOrgHeader.PK;

			var valueFrom1 = Factory.NewWithValidTestData<OrgHeader>();
			valueFrom1.OH_Code = "Test1";
			var rule1 = authorization.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
			rule1.CPR_ValueFrom = "Test1";

			var valueFrom2 = Factory.NewWithValidTestData<OrgHeader>();
			valueFrom2.OH_Code = "Test2";
			var rule2 = authorization.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
			rule2.CPR_ValueFrom = "Test2";

			Factory.Save();
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find no dec - wrong authorisation holder", 0, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

			decToFind.JE_OH_Importer = valueFrom1.PK;
			decToFindWithoutEidr.JE_OH_Importer = valueFrom2.PK;
			Factory.Save();
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find 1 dec", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Latvia);
			decToFind = Factory.New<JobDeclaration>();
			decToFind.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			decToFind.CusEntryInstruction.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			decToFind.JE_DeclarationReference = "B70";
			ceh = decToFind.CustomsEntryHeaders.AddNew();
			ceh.CH_CEI_Instruction = decToFind.CustomsEntryInstructions[0].PK;
			decToFind.JE_OH_Importer = importer.PK;
			decToFind.JE_MessageType = "IMP";
			decToFind.JE_EidrType = EidrTypeList.Codes.CFS;
			ceh.EntryNumber = "071-123456C";
			ceh.CusEntryNumber.CE_IssueDate = ZDateTime.Now;

			decToFindWithoutEidr = Factory.New<JobDeclaration>();
			decToFindWithoutEidr.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			decToFindWithoutEidr.CusEntryInstruction.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			decToFindWithoutEidr.JE_DeclarationReference = "B71";
			ceh2 = decToFindWithoutEidr.CustomsEntryHeaders.AddNew();
			ceh2.CH_CEI_Instruction = decToFindWithoutEidr.CustomsEntryInstructions[0].PK;
			decToFindWithoutEidr.JE_OH_Importer = importer.PK;
			decToFindWithoutEidr.JE_MessageType = "IMP";
			ceh2.EntryNumber = "071-123456D";
			ceh2.CusEntryNumber.CE_IssueDate = ZDateTime.Now;

			Factory.Save();

			decFsd = Factory.New<JobDeclaration>();
			wizardManager = new CDSFinalSupplementaryDeclarationHelperManager(decFsd);
			wizardManager.CDSFsdWizard.AuthorisationHolderAddressPK = importer.MainAddress.PK;
			wizardManager.CDSFsdWizard.DueDate = ZDateTime.Today.AddMonths(1);
			wizardManager.CountTimelyEntries();
			AssertEquals("Should find 0 dec - Limited to GB declarations", 0, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
		}

		public void TestCountTimelyEntries_FindForImporter()
		{
			var gbUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom));

			var decToFind = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			var importerToFind = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			importerToFind.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			decToFind.JE_OH_Importer = importerToFind.PK;

			var importerToIgnore = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JANSEW");
			importerToIgnore.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			decToIgnore.JE_OH_Importer = importerToIgnore.PK;

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CDSFsdWizard.ImporterPK = importerToFind.PK;

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find for Importer", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				wizardManager.CDSFsdWizard.ImporterPK = ZGuid.Empty;

				wizardManager.CountTimelyEntries();
				AssertEquals("Find for Importer", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindForDeclarant()
		{
			var decToFind = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			var declarantToFind = Factory.New<OrgHeader>();
			declarantToFind.OH_Code = "declarant1";
			decToFind.JE_OA_DeclarantAddress = declarantToFind.MainAddress.PK;

			var declarantToIgnore = Factory.New<OrgHeader>();
			declarantToIgnore.OH_Code = "declarant2";
			decToIgnore.JE_OA_DeclarantAddress = declarantToIgnore.MainAddress.PK;

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CDSFsdWizard.DeclarantAddressPK = declarantToFind.MainAddress.PK;

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find for Declarant", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				wizardManager.CDSFsdWizard.DeclarantAddressPK = ZGuid.Empty;

				wizardManager.CountTimelyEntries();
				AssertEquals("Find for Declarant", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindForImporterLinkedOrg()
		{
			var decToFind = GetNewBasicMatchingDeclaration();
			var decToFindForImporter = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			var gbUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom));
			var authOwner = Factory.New<OrgHeader>();
			authOwner.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			authOwner.OH_Code = "ABC";

			var auth = Factory.New<CusAuthorisationHeader>();
			auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			auth.CPH_Number = "11111";
			auth.CPH_OH_PermitHolder = authOwner.PK;

			var linkedImporter1 = Factory.New<OrgHeader>();
			linkedImporter1.OH_Code = "Test1";
			var rule1 = auth.CusAuthorisationRules.AddNew();
			rule1.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
			rule1.CPR_ValueFrom = "Test1";

			var linkedImporter2 = Factory.New<OrgHeader>();
			linkedImporter2.OH_Code = "Test2";
			var rule2 = auth.CusAuthorisationRules.AddNew();
			rule2.CPR_RuleCode = GBCusAuthorisationRuleTypeList.Codes.ORG;
			rule2.CPR_ValueFrom = "Test2";

			decToFind.JE_OH_Importer = linkedImporter1.PK;

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			decToIgnore.JE_OH_Importer = importer.PK;

			var importerToFind = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			importerToFind.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			decToFindForImporter.JE_OH_Importer = importerToFind.PK;

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CDSFsdWizard.ImporterPK = importerToFind.PK;
			wizardManager.CDSFsdWizard.AuthorisationHolderAddressPK = authOwner.MainAddress.PK;
			wizardManager.CDSFsdWizard.AuthorisationType = AuthorisationTypeCodes.SDE;
			wizardManager.CDSFsdWizard.FindForImporterLinkedOrg = true;

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find for linked importer org", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				decToIgnore.JE_OH_Importer = linkedImporter2.PK;
				Factory.Save();

				wizardManager.CountTimelyEntries();
				AssertEquals("Find for linked importer org", 3, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				wizardManager.CDSFsdWizard.FindForImporterLinkedOrg = false;
				wizardManager.CountTimelyEntries();
				AssertEquals("Find for linked importer org", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindOnlyForAuthHolder()
		{
			var decToFind = GetNewBasicMatchingDeclaration();
			var decToFindMultipleAuth = GetNewBasicMatchingDeclaration();
			var decToFindMultiHeader = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			var authOwner = Factory.New<OrgHeader>();
			authOwner.CustomsCodes.AddNew("EOR", "456");
			authOwner.OH_Code = "456";
			var authOwner2 = Factory.New<OrgHeader>();
			authOwner2.CustomsCodes.AddNew("EOR", "789");
			authOwner2.OH_Code = "789";

			var authToFind = decToFind.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToFind.AGC_Code = AuthorisationTypeCodes.SDE;
			authToFind.AGC_Number = "12345";
			authToFind.AGC_OH_Owner = authOwner.PK;

			var authToIgnore = decToIgnore.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToIgnore.AGC_Code = AuthorisationTypeCodes.EIR;
			authToIgnore.AGC_Number = "67890";
			authToIgnore.AGC_OH_Owner = authOwner2.PK;

			var authToFind2 = decToFindMultipleAuth.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToFind2.AGC_Code = AuthorisationTypeCodes.SDE;
			authToFind2.AGC_Number = "12345";
			authToFind2.AGC_OH_Owner = authOwner.PK;

			var authToIgnore2 = decToFindMultipleAuth.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToIgnore2.AGC_Code = AuthorisationTypeCodes.EIR;
			authToIgnore2.AGC_Number = "67890";
			authToIgnore2.AGC_OH_Owner = authOwner2.PK;

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CDSFsdWizard.AuthorisationHolderAddressPK = authOwner.MainAddress.PK;
			wizardManager.CDSFsdWizard.FindOnlyForAuthHolder = true;

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Auth Holder", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				var newCei = decToFindMultiHeader.CustomsEntryInstructions.AddNew();
				newCei.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
				var newCeh = decToFindMultiHeader.CustomsEntryHeaders.AddNew();
				newCeh.CH_CEI_Instruction = newCei.PK;
				newCeh.EntryNumber = "TST2";
				newCeh.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
				newCeh.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

				authToFind = decToFindMultiHeader.CustomsEntryInstructions[0].CusAuthorizationUsages.AddNew();
				authToFind.AGC_Code = AuthorisationTypeCodes.SDE;
				authToFind.AGC_Number = "135";
				authToFind.AGC_OH_Owner = authOwner.PK;

				authToIgnore = decToFindMultiHeader.CustomsEntryInstructions[1].CusAuthorizationUsages.AddNew();
				authToIgnore.AGC_Code = AuthorisationTypeCodes.EIR;
				authToIgnore.AGC_Number = "246";
				authToIgnore.AGC_OH_Owner = authOwner2.PK;

				Factory.Save();

				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Auth Holder", 3, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				wizardManager.CDSFsdWizard.FindOnlyForAuthHolder = false;
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Auth Holder", 5, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindOnlyForAuthType()
		{
			var decToFind = GetNewBasicMatchingDeclaration();
			var decToFindMultipleAuth = GetNewBasicMatchingDeclaration();
			var decToFindMultiHeader = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			var authOwner = Factory.New<OrgHeader>();
			authOwner.CustomsCodes.AddNew("EOR", "456");
			authOwner.OH_Code = "456";
			var authToFind = decToFind.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToFind.AGC_Code = AuthorisationTypeCodes.SDE;
			authToFind.AGC_Number = "12345";
			authToFind.AGC_OH_Owner = authOwner.PK;

			var authToIgnore = decToIgnore.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToIgnore.AGC_Code = AuthorisationTypeCodes.EIR;
			authToIgnore.AGC_Number = "67890";
			authToIgnore.AGC_OH_Owner = authOwner.PK;
			var authToFind2 = decToFindMultipleAuth.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToFind2.AGC_Code = AuthorisationTypeCodes.SDE;
			authToFind2.AGC_Number = "12345";
			authToFind2.AGC_OH_Owner = authOwner.PK;

			var authOwner2 = Factory.New<OrgHeader>();
			authOwner2.CustomsCodes.AddNew("EOR", "789");
			authOwner2.OH_Code = "789";
			var authToIgnore2 = decToIgnore.CusEntryInstruction.CusAuthorizationUsages.AddNew();
			authToIgnore2.AGC_Code = AuthorisationTypeCodes.EIR;
			authToIgnore2.AGC_Number = "67890";
			authToIgnore2.AGC_OH_Owner = authOwner2.PK;

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CDSFsdWizard.AuthorisationType = AuthorisationTypeCodes.SDE;
			wizardManager.CDSFsdWizard.FindOnlyForAuthType = true;

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Authorisation Type", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				var newCei = decToFindMultiHeader.CustomsEntryInstructions.AddNew();
				newCei.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
				var newCeh = decToFindMultiHeader.CustomsEntryHeaders.AddNew();
				newCeh.CH_CEI_Instruction = newCei.PK;
				newCeh.EntryNumber = "TST2";
				newCeh.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
				newCeh.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
				var authOwner3 = Factory.New<OrgHeader>();
				authOwner3.CustomsCodes.AddNew("EOR", "123");
				authOwner3.OH_Code = "123";
				var authToFind3 = decToFindMultiHeader.CustomsEntryInstructions[0].CusAuthorizationUsages.AddNew();
				authToFind3.AGC_Code = AuthorisationTypeCodes.SDE;
				authToFind3.AGC_Number = "A12";
				authToFind3.AGC_OH_Owner = authOwner3.PK;
				var authToIgnore3 = decToFindMultiHeader.CustomsEntryInstructions[1].CusAuthorizationUsages.AddNew();
				authToIgnore3.AGC_Code = AuthorisationTypeCodes.EIR;
				authToIgnore3.AGC_Number = "B12";
				authToIgnore3.AGC_OH_Owner = authOwner3.PK;

				Factory.Save();

				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Authorisation Type", 3, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				wizardManager.CDSFsdWizard.FindOnlyForAuthType = false;
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Authorisation Type", 5, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindOnlyForDueDate()
		{
			var decToFind = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			decToFind.JE_SuppDecDueDate = ZDateTime.Now;
			decToIgnore.JE_SuppDecDueDate = ZDateTime.Now.AddDays(1);

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CDSFsdWizard.DueDate = ZDateTime.Now;
			wizardManager.CDSFsdWizard.FindOnlyForDueDate = true;

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Due Date", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				wizardManager.CDSFsdWizard.FindOnlyForDueDate = false;
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Due Date", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindForCompany()
		{
			var otherGBCompany = Factory.New<GlbCompany>();
			otherGBCompany.GC_Code = "GB2";
			otherGBCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var branchGBOther = otherGBCompany.Branches.AddNew();
			branchGBOther.GB_RL_NKHomePort = "GBLHR";
			branchGBOther.GB_Code = "GB2";

			Factory.Save();

			_ = GetNewBasicMatchingDeclaration();

			using (DisposableEnvironment.ForBranch(branchGBOther.PK.ToGuid()))
			{
				_ = GetNewBasicMatchingDeclaration();
			}

			Factory.Save();

			var wizardManager = GetWizardManager();

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Company", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);

				_ = GetNewBasicMatchingDeclaration();
				Factory.Save();

				wizardManager.CountTimelyEntries();
				AssertEquals("Find only for Company", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
			});
		}

		public void TestCountTimelyEntries_FindOnlyCDS()
		{
			_ = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			decToIgnore.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

			Factory.Save();

			var wizardManager = GetWizardManager();
			wizardManager.CountTimelyEntries();

			AssertEquals("Find only CDS declarations", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
		}

		public void TestCountTimelyEntries_FindOnlyMRN()
		{
			_ = GetNewBasicMatchingDeclaration();
			var decToIgnore = GetNewBasicMatchingDeclaration();

			decToIgnore.CustomsEntryHeaders[0].CusEntryNumber.CE_EntryType = CusEntryNumberTypes.EU.LocalReferenceNumber;

			Factory.Save();

			var wizardManager = GetWizardManager();

			wizardManager.CountTimelyEntries();

			AssertEquals("Find only for MRN", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
		}

		public void TestCountTimelyEntries_CountResultsAreSplitToYZAndOutputed()
		{
			var decToFind1 = GetNewBasicMatchingDeclaration();
			var decToFind2 = GetNewBasicMatchingDeclaration();

			Factory.Save();

			var wizardManager = GetWizardManager();

			CombineAssertions(() =>
			{
				wizardManager.CountTimelyEntries();
				AssertEquals("Number of Type Y declarations due", 2, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
				AssertEquals("Number of Type Z declarations due", 0, wizardManager.CDSFsdWizard.NumberOfTypeZDeclarationsSubmitted);

				decToFind1.CusEntryInstruction.CEI_SubStyle = "Z";

				Factory.Save();

				wizardManager.CountTimelyEntries();
				AssertEquals("Number of Type Y declarations due", 1, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
				AssertEquals("Number of Type Z declarations due", 1, wizardManager.CDSFsdWizard.NumberOfTypeZDeclarationsSubmitted);

				decToFind2.CusEntryInstruction.CEI_SubStyle = "Z";

				Factory.Save();

				wizardManager.CountTimelyEntries();
				AssertEquals("Number of Type Y declarations due", 0, wizardManager.CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted);
				AssertEquals("Number of Type Z declarations due", 2, wizardManager.CDSFsdWizard.NumberOfTypeZDeclarationsSubmitted);
			});
		}

		CDSFinalSupplementaryDeclarationHelperManager GetWizardManager(bool clearDefaults = true, ZDateTime? startOfPeriod = null, ZDateTime? dueDate = null)
		{
			var decFsd = Factory.New<JobDeclaration>();
			var wizardManager = new CDSFinalSupplementaryDeclarationHelperManager(decFsd);
			wizardManager.CDSFsdWizard.StartOfPeriod = startOfPeriod ?? ZDateTime.UtcNow.AddMonths(-1);
			wizardManager.CDSFsdWizard.DueDate = dueDate ?? ZDateTime.UtcNow.AddMonths(1);
			if (clearDefaults)
			{
				wizardManager.CDSFsdWizard.AuthorisationHolderAddressPK = ZGuid.Empty;
				wizardManager.CDSFsdWizard.DeclarantAddressPK = ZGuid.Empty;
				wizardManager.CDSFsdWizard.AuthorisationType = ZString.Empty;
				wizardManager.CDSFsdWizard.ImporterPK = ZGuid.Empty;
			}
			return wizardManager;
		}

		JobDeclaration GetNewBasicMatchingDeclaration()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_EidrType = EidrTypeList.Codes.CFS;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.CusEntryInstruction.CEI_SubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;
			var ceh = dec.CustomsEntryHeaders.AddNew();
			ceh.CH_CEI_Instruction = dec.CustomsEntryInstructions[0].PK;
			ceh.EntryNumber = "TST1";
			ceh.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
			ceh.CusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			return dec;
		}
	}
}
