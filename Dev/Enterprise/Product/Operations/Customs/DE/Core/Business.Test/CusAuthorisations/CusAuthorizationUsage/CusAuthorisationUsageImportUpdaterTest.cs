using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusAuthorisationUsageImportUpdaterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAddLocalClearanceProcedureAuthorisationNumber_Declarant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			Factory.Save();

			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT1");
		}

		[ExpectNoExceptions]
		public void TestAddLocalClearanceProcedureAuthorisationNumber_RepresentativeFallback()
		{
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REPRESENTATIVE1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			Factory.Save();

			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, representative.PK, "REPRESENTATIVE1");
		}

		[ExpectNoExceptions]
		public void TestAddLocalClearanceProcedureAuthorisationNumber_BuyingAgentFallback()
		{
			var buyingAgent = Factory.NewWithValidTestData<OrgHeader>();
			buyingAgent.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "BUYINGAGENT1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			Factory.Save();

			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			jobDeclaration.JE_OA_BuyingAgentAddress = buyingAgent.MainAddress.PK;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, buyingAgent.PK, "BUYINGAGENT1");
		}

		[ExpectNoExceptions]
		public void TestAddLocalClearanceProcedureAuthorisationNumber_AZ_Style()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "REPRESENTATIVE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			Factory.Save();

			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			entryInstruction.CEI_LocalClearanceDate = ZDate.Today;

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, representative.PK, "REPRESENTATIVE");
		}

		[ExpectNoExceptions]
		public void TestAddLocalClearanceProcedureAuthorisationNumber_MultipleValidAuthorizations()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REPRESENTATIVE1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REPRESENTATIVE2", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
			Factory.Save();

			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZA;

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Single valid authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT");

			jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Multiple valid authorizations from Representative");
		}

		[ExpectNoExceptions]
		public void TestAddEndOfUseAuthorisation()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var supportDocument = invoiceLine.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "DECLARANT");
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EndUse, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddEndOfUseAuthorisation_MultipleValidAuthorizations()
		{
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var supportDocument = invoiceLine.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "FirstEUS");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.EndUse, "SecondEUS");
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddEndOfUseAuthorisation_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var supportDocument = invoiceLine.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N990;

			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "No EUS authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EndUse;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = ZDate.Today.AddDays(-1);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				supportDocument.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.C034;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Supporting document doesn't require EUS");
			});
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AZL_CWP()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECLARANT");
			auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AZL_CW1()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECLARANT");
			auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AZL_MultipleValidAuthorizations()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "FirstEIR");
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "SecondEIR");
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AZL_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				var rule = auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "No EIR authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_SEL()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECLARANT");
			auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_SEL_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var auth1 = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "FirstEIR");
			auth1.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			var auth2 = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "SecondEIR");
			auth2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_SEL_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;

			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				var rule = auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "No EIR authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_DIR()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			var auth = CreateAuthorizationWithLocalClearanceDate(representative, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "REPRESENTATIVE");
			auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Representative", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, representative.PK, "REPRESENTATIVE");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var auth2 = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECLARANT");
			auth2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_DIR_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			var auth1 = CreateAuthorizationWithLocalClearanceDate(representative, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "FirstEIR");
			auth1.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			var auth2 = CreateAuthorizationWithLocalClearanceDate(representative, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "SecondEIR");
			auth2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_DIR_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				var rule = auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No EIR authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_IND()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			var auth = CreateAuthorizationWithLocalClearanceDate(representedParty, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "REPRESENTEDPARTY");
			auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from RepresentedParty", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, representedParty.PK, "REPRESENTEDPARTY");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			var auth2 = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DECLARANT");
			auth2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_IND_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			var auth1 = CreateAuthorizationWithLocalClearanceDate(representedParty, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "FirstEIR");
			auth1.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			var auth2 = CreateAuthorizationWithLocalClearanceDate(representedParty, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "SecondEIR");
			auth2.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddEIRAuthorization_AAV_IND_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				var rule = auth.CreateAuthorisationRule(CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No EIR authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_SEL()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_SEL_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "FirstIPO");
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "SecondIPO");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_SEL_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No IPO authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_DIR()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_DIR_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "FirstIPO");
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "SecondIPO");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_DIR_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No IPO authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_IND()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(representedParty, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "REPRESENTEDPARTY");
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from RepresentedParty", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, representedParty.PK, "REPRESENTEDPARTY");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DECLARANT");
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_IND_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(representedParty, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "FirstIPO");
			CreateAuthorizationWithLocalClearanceDate(representedParty, CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "SecondIPO");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_AAV_IND_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No IPO authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_SEL()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_SEL_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "FirstIPO");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "SecondIPO");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_SEL_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No IPO authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_DIR()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_DIR_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "FirstIPO");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "SecondIPO");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_DIR_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No IPO authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_IND()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			representedParty.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "REPRESENTEDPARTY");
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from RepresentedParty", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, representedParty.PK, "REPRESENTEDPARTY");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "DECLARANT");
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.InwardProcessing, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_IND_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			representedParty.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "FirstIPO");
			representedParty.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.InwardProcessing, "SecondIPO");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddIPOAuthorization_VAV_IND_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No IPO authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.InwardProcessing;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_SEL()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_SEL_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "FirstIPO", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "SecondIPO", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_SEL_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var (auth, rule) = declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No SDE authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = ZDate.Today.AddDays(-1);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_DIR()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REPRESENTATIVE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Representative", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, representative.PK, "REPRESENTATIVE");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_DIR_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_Representative = representative.MainAddress.PK;
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "FirstSDE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "SecondSDE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_DIR_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var (auth, rule) = declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No SDE authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = ZDate.Today.AddDays(-1);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_IND()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			representedParty.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REPRESENTEDPARTY", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from RepresentedParty", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, representedParty.PK, "REPRESENTEDPARTY");

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage("Authorization from Declarant", entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_IND_MultipleValidAuthorizations()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			var representedParty = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_BuyingAgentAddress = representedParty.MainAddress.PK;
			representedParty.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "FirstSDE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			representedParty.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "SecondSDE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VAV_IND_EdgeCases()
		{
			jobDeclaration.JE_DeclarantType = RepresentationTypeList.Codes._3Indirect;
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VAV;
			NUnit.Framework.Assert.Multiple(() =>
			{
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var (auth, rule) = declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REPRESENTEDPARTY", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No SDE authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = ZDate.Today.AddDays(-1);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VZL_CWP()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VZL_CW1()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VZL_MultipleValidAuthorizations()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "FirstSDE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "SecondSDE", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1);
			Factory.Save();

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddSDEAuthorization_VZL_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var (auth, rule) = declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.CustomsWarehousing);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No SDE authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");

				auth.CPH_StartDate = ZDate.Today.AddDays(-1);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				rule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				Factory.Save();
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid rule");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_AZL()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_AZL_MultipleValidAuthorizations()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "FirstCWP");
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "SecondCWP");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_AZL_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No CW1 authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_VZL()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_VZL_MultipleValidAuthorizations()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "FirstCWP");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "SecondCWP");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_VZL_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No CWP authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_EZL()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_EZL_MultipleValidAuthorizations()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "FirstCWP");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, "SecondCWP");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddCWPAuthorization_EZL_EdgeCases()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 10, 26);
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No CWP authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_AZL()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_AZL_MultipleValidAuthorizations()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "FirstCW1");
			CreateAuthorizationWithLocalClearanceDate(declarant, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "SecondCW1");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_AZL_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZL;
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No CW1 authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				auth.CPH_StartDate = new ZDate(2021, 10, 30);
				auth.CPH_EndDate = ZDate.Today.AddDays(1);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_VZL()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_VZL_MultipleValidAuthorizations()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "FirstCW1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "SecondCW1");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_VZL_EdgeCases()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.VZL;
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No CW1 authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_EZL()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "DECLARANT");

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsage(entryInstruction.CusAuthorizationUsages[0], CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, declarant.PK, "DECLARANT");
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_EZL_MultipleValidAuthorizations()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "FirstCW1");
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, "SecondCW1");

			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestAddCW1Authorization_EZL_EdgeCases()
		{
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 10, 26);
			NUnit.Framework.Assert.Multiple(() =>
			{
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Declarant null");

				var declarant = Factory.NewWithValidTestData<OrgHeader>();
				jobDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
				var auth = declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, "DECLARANT");
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "No CW1 authorization");

				auth.CPH_Type = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
				auth.CPH_StartDate = new ZDate(2021, 10, 01);
				auth.CPH_EndDate = new ZDate(2021, 10, 31);
				updater.UpdateEntryInstructionAuthorizations();
				NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Any(), NUnit.Framework.Is.EqualTo(false), "Invalid authorization date");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 10, 26);
			updater = new CusAuthorizationUsageImportUpdater(jobDeclaration);
		}
		JobDeclaration jobDeclaration;
		CusEntryInstruction entryInstruction;
		CusAuthorizationUsageUpdater updater;

		[ExpectNoExceptions]
		void AssertCusAuthorisationUsage(CusAuthorizationUsage authorizationUsage, ZString expectedCode, ZGuid expectedOwner, ZString expectedNumber)
		{
			AssertCusAuthorisationUsage("", authorizationUsage, expectedCode, expectedOwner, expectedNumber);
		}

		[ExpectNoExceptions]
		void AssertCusAuthorisationUsage(string message, CusAuthorizationUsage authorizationUsage, ZString expectedCode, ZGuid expectedOwner, ZString expectedNumber)
		{
			CombineAssertions(message, () =>
			{
				NUnit.Framework.Assert.That(authorizationUsage.AGC_Code, NUnit.Framework.Is.EqualTo(expectedCode), "Code");
				NUnit.Framework.Assert.That(authorizationUsage.AGC_OH_Owner, NUnit.Framework.Is.EqualTo(expectedOwner), "Owner");
				NUnit.Framework.Assert.That(authorizationUsage.AGC_IsSystemGenerated, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Is System Generated");
				NUnit.Framework.Assert.That(authorizationUsage.AGC_Number, NUnit.Framework.Is.EqualTo(expectedNumber), "Number");
			});
		}

		CusAuthorisationHeader CreateAuthorizationWithLocalClearanceDate(OrgHeader authorizationHolder, string authType, string authNumber)
		{
			var authorization = authorizationHolder.CreateAuthorisationRecord(authType, authNumber);
			authorization.CPH_StartDate = new ZDate(2021, 10, 01);
			authorization.CPH_EndDate = new ZDate(2021, 10, 31);
			return authorization;
		}
	}
}
