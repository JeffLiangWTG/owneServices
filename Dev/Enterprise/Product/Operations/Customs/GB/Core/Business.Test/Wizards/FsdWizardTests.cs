using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP.FSD.Testing
{
	class FsdWizardTests : TestCaseWithFactory
	{
		[TestDate(2010, 03, 03)]
		public void TestFindDeclarationsAndCreateNew()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			// Set up old dec that we will look for
			var decFromPreviousPeriodToSeek = Factory.New<JobDeclaration>();
			decFromPreviousPeriodToSeek.JE_DeclarationReference = "B69";
			var ceh = decFromPreviousPeriodToSeek.CustomsEntryHeaders.AddNew();
			ceh.CH_CEI_Instruction = decFromPreviousPeriodToSeek.CustomsEntryInstructions[0].PK;
			var gbUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedKingdom));
			OrgHeader importer = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
			importer.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			decFromPreviousPeriodToSeek.JE_OH_Importer = importer.PK;
			decFromPreviousPeriodToSeek.JE_MessageType = "IMP";
			decFromPreviousPeriodToSeek.JE_EidrType = EidrTypeList.Codes.CFS;
			ceh.EntryNumber = "071-123456A";
			ceh.CusEntryNumber.CE_IssueDate = ZDateTime.Now;
			ceh.EntryInstruction.CEI_Style = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration; // ISD

			var authorization = Factory.New<CusAuthorisationHeader>();
			authorization.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			authorization.CPH_Number = "11111";
			var permit = Factory.NewWithValidTestData<OrgHeader>();
			permit.OH_Code = "Permit";
			authorization.CPH_OH_PermitHolder = permit.PK;

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

			// New cus entry header to use with wizard
			var decFsd = Factory.New<JobDeclaration>();
			FsdWizardManager wizardManager = new FsdWizardManager(decFsd);
			wizardManager.FsdWizard.Consignee = importer.PK;
			wizardManager.FsdWizard.Procedure = EidrTypeList.Codes.CFS;
			wizardManager.FindDeclarations();
			AssertContains("Should find no decs - wrong date range", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			ceh.CusEntryNumber.CE_IssueDate = ZDateTime.Now.AddMonths(-1);
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find one ISD within range", "SDI = 1/1, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			wizardManager.FsdWizard.Authorization = authorization.PK;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find no decs - wrong date range", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			decFromPreviousPeriodToSeek.JE_OH_Importer = valueFrom1.PK;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find one ISD within range", "SDI = 1/1, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			wizardManager.FsdWizard.Authorization = ZGuid.Empty;
			wizardManager.FsdWizard.Consignee = ZGuid.Empty;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find one ISD within range", "SDI = 1/1, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			// Cancel entry		
			ceh.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find no descs at all", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			ceh.CH_EntryStatus = "ABC"; // reset
			ceh.EntryInstruction.CEI_Style = ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find one ISW", "SDI = 0/0, SDW = 1/1", wizardManager.FsdWizard.AiStatement);

			// Now change dec to be FSD, should be excluded
			ceh.EntryInstruction.CEI_Style = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			decFromPreviousPeriodToSeek.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration; // Y
			JobComInvoiceLine line = decFromPreviousPeriodToSeek.Invoices.AddNew().InvoiceLines.AddNew();
			line.JI_Procedure = JobComInvoiceLine.CfspFsdCPCCode;
			Assert("Pre-req - should be FSD", ceh.IsFSD);
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find no decs at all, as the only one is an FSD dec", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			line.JI_Procedure = "123"; //  reset to non-FSD.....
			decFromPreviousPeriodToSeek.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse; // .... specifically ISW
			ceh.EntryInstruction.CEI_Style = ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse;
			OrgHeader supplier = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "JANSEW"); // random
			supplier.OH_RL_NKClosestPort = gbUNLOCO.RL_Code;
			wizardManager.FsdWizard.Consignor = supplier.PK;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find no decs at all, wrong supplier", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			decFromPreviousPeriodToSeek.JE_OH_Supplier = supplier.PK;
			decFromPreviousPeriodToSeek.JE_EidrType = EidrTypeList.Codes.DEL;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find no decs at all, procedure does not match declaration EIDR type", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			wizardManager.FsdWizard.Procedure = EidrTypeList.Codes.DEL;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find dec, right supplier", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			decFromPreviousPeriodToSeek.JE_EidrType = ZString.Empty;
			Factory.Save(); // It's a DB-only query, remember
			wizardManager.FindDeclarations();
			AssertContains("Should find dec, right supplier", "SDI = 0/0, SDW = 0/0", wizardManager.FsdWizard.AiStatement);

			// Now press the create button and look at the resulting dec....

			string sirMixalot = "I like SMALL butts and I cannot lie";
			wizardManager.FsdWizard.AiStatement = sirMixalot;
			wizardManager.FsdWizard.Consignee = importer.PK;
			wizardManager.FsdWizard.Consignor = supplier.PK;
			wizardManager.CreateFsd();

			AssertEquals(importer.PK, decFsd.JE_OH_Importer);
			AssertEquals(supplier.PK, decFsd.JE_OH_Supplier);
			var lineCreated = decFsd.InvoiceLines[0];
			var ai = lineCreated.AdditionalInfos[0];
			AssertEquals("FINSD", ai.CSI_Code);
			AssertEquals(sirMixalot, ai.CSI_Description);
			AssertEquals("ISD", decFsd.JE_DeclarationType);
			AssertEquals("IM", decFsd.JE_EntryStyle);
			AssertEquals("Y", decFsd.JE_EntrySubStyle);
			AssertEquals(new ZDateTime(2010, 2, 1, 0, 0, 0), decFsd.JE_EntryAuthorisationDate);
			AssertEquals(1, decFsd.JE_TotalNoOfPacks);
			AssertEquals(JobComInvoiceLine.CfspFsdCPCCode, decFsd.InvoiceLines[0].JI_Procedure);
			AssertEquals(0, lineCreated.Taxes.Count);
			AssertEquals(0, lineCreated.InvoiceHeader.PreviousDocuments.Count);

			decFsd.Factory.Save();
			StmNote[] notes = Factory.Load<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, decFsd.PK).AddToFilter(StmNoteSchema.ST_Table, decFsd.TableName));
			AssertNotContains("Ref=B69;", notes[0].ST_NoteDataAsText);
		}

		public void TestValidation()
		{
			FsdWizard wizard = new FsdWizard(Factory);
			wizard.Validation.ValidateConsignee();
			AssertHasMessageErrorContaining(wizard.ConsigneeInfo, "Consignee");
			OrgHeader org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");  //in the dat DB. 
			wizard.Consignee = org.PK;
			AssertNoMessageErrorContaining(wizard.ConsigneeInfo, "Consignee");
			wizard.Validation.ValidateProcedure();
			AssertHasErrorContaining(wizard.ProcedureInfo, "Procedure");
			wizard.Procedure = EidrTypeList.Codes.DEL;
			AssertNoErrorContaining(wizard.ProcedureInfo, "Procedure");
		}

		public void TestAuthorizations()
		{
			var wizard = new FsdWizard(Factory);
			wizard.Consignee = ZGuid.BrettsGuid;

			CombineAssertions(() =>
			{
				AssertType<CusAuthorisationHeaderCollection>(wizard.Authorizations);
				AssertEquals(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, wizard.Authorizations.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType && x.PropertyName == "Property").Value);
				AssertEquals(ZGuid.BrettsGuid, wizard.Authorizations.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Single(x => x.FilterName == CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder && x.PropertyName == "Property").Value);
			});
		}
	}

	[TestedType(typeof(FsdWizard))]
	public class FsdWizardNPBOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new FsdWizard(this.Factory);
		}
	}
}
