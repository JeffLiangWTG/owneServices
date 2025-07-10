using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryInstruction = Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusAuthorisationUsageExportUpdaterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAuthorisations_IsSDEExportOrSDEOutwardProcessing_FromDeclarant()
		{
			var declarant1 = Factory.New<OrgHeader>();
			declarant1.OH_Code = "OH1";
			var declarant2 = Factory.New<OrgHeader>();
			declarant2.OH_Code = "OH2";
			declarant1.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			declarant1.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER2", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			declarant2.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER3", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			declarant2.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER4", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001300;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0001;
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages, (CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant1.PK, "NUMBER1"));

			declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "CusAuthorizationUsages not populated when there are multiple valid authorisations");
		}

		[ExpectNoExceptions]
		public void TestAuthorisations_IsSDEExportOrSDEOutwardProcessing_FromDeclarantThenRepresentative()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "OH1";
			var representative1 = Factory.New<OrgHeader>();
			representative1.OH_Code = "OH2";
			var representative2 = Factory.New<OrgHeader>();
			representative2.OH_Code = "OH3";
			representative1.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			representative1.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER2", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			representative2.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER3", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			representative2.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER4", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure);
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OA_Representative = representative1.MainAddress.PK;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._111300;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0010;
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages, (CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, representative1.PK, "NUMBER2"));

			declaration.JE_OA_Representative = representative2.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "CusAuthorizationUsages not populated when there are multiple valid authorisations");
		}

		[ExpectNoExceptions]
		public void TestAuthorisations_IsCCLExportOrCCLOutwardProcessing_FromDeclarant()
		{
			var declarant1 = Factory.New<OrgHeader>();
			declarant1.OH_Code = "OH1";
			var declarant2 = Factory.New<OrgHeader>();
			declarant2.OH_Code = "OH2";
			declarant1.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER1");
			declarant2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER2");
			declarant2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER3");
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000410;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages, (CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, declarant1.PK, "NUMBER1"));

			declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "CusAuthorizationUsages not populated when there are multiple valid authorisations");
		}

		[ExpectNoExceptions]
		public void TestAuthorisations_IsCCLExportOrCCLOutwardProcessing_FromDeclarantThenRepresentative()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "OH1";
			var representative1 = Factory.New<OrgHeader>();
			representative1.OH_Code = "OH2";
			var representative2 = Factory.New<OrgHeader>();
			representative2.OH_Code = "OH3";
			representative1.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER1");
			representative2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER2");
			representative2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER3");
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OA_Representative = representative1.MainAddress.PK;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110410;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0011;
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages, (CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, representative1.PK, "NUMBER1"));

			declaration.JE_OA_Representative = representative2.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "CusAuthorizationUsages not populated when there are multiple valid authorisations");
		}

		[ExpectNoExceptions]
		public void TestAuthorisations_IsOPOOutwardProcessing()
		{
			var declarant1 = Factory.New<OrgHeader>();
			declarant1.OH_Code = "OH1";
			var declarant2 = Factory.New<OrgHeader>();
			declarant2.OH_Code = "OH2";
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "OH3";
			declarant1.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "NUMBER1");
			declarant2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "NUMBER2");
			declarant2.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "NUMBER3");
			representative.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, "NUMBER4");
			Factory.Save();

			declaration.JE_OA_Representative = representative.MainAddress.PK;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._110110;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "Not getting authorisations from representative");

			declaration.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages, (CusAuthorizationHeaderTypeList.Codes.OutwardProcessing, declarant1.PK, "NUMBER1"));

			declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "CusAuthorizationUsages not populated when there are multiple valid authorisations");
		}

		[ExpectNoExceptions]
		public void TestAuthorisations_IsEIRExport()
		{
			var declarant1 = Factory.New<OrgHeader>();
			declarant1.OH_Code = "OH1";
			var declarant2 = Factory.New<OrgHeader>();
			declarant2.OH_Code = "OH2";
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "OH3";
			declarant1.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "NUMBER1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			declarant2.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "NUMBER2", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			declarant2.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "NUMBER3", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			representative.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "NUMBER4", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			Factory.Save();

			declaration.JE_OA_Representative = representative.MainAddress.PK;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "Not getting authorisations from representative");

			declaration.JE_OA_DeclarantAddress = declarant1.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages, (CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant1.PK, "NUMBER1"));

			declaration.JE_OA_DeclarantAddress = declarant2.MainAddress.PK;
			updater.UpdateEntryInstructionAuthorizations();
			NUnit.Framework.Assert.That(entryInstruction.CusAuthorizationUsages.Count, Is.EqualTo(0), "CusAuthorizationUsages not populated when there are multiple valid authorisations");
		}

		[ExpectNoExceptions]
		public void TestAuthorisations_Overlapped()
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "OH1";
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "OH2";
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "NUMBER1", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			declarant.CreateAuthorisationRecord(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, "NUMBER2");
			declarant.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "NUMBER3", CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
			Factory.Save();

			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._001410;
			entryInstruction.CEI_SubStyle = ExportDeclarationTypeTimeList.Codes._20;
			entryInstruction.ZG_PartyConstellation = PartyConstellationCodeList.Codes._0101;

			updater.UpdateEntryInstructionAuthorizations();
			AssertCusAuthorisationUsages(entryInstruction.CusAuthorizationUsages,
				(CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, declarant.PK, "NUMBER1"),
				(CusAuthorizationHeaderTypeList.Codes.CentralizedClearance, declarant.PK, "NUMBER2"),
				(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, declarant.PK, "NUMBER3"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			updater = new CusAuthorizationUsageExportUpdater(declaration);
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusAuthorizationUsageExportUpdater updater;

		[ExpectNoExceptions]
		void AssertCusAuthorisationUsages(EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction> actualUsages, params (ZString code, ZGuid owner, ZString number)[] expectedUsages)
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var usages = actualUsages.ToList();

				string UsagesToString(IEnumerable<CusAuthorizationUsage> usageList)
				{
					var message = new StringBuilder();
					foreach (var usage in usageList)
					{
						message.Append($"\n  Code={usage.AGC_Code} Owner={usage.AGC_OH_Owner} Number={usage.AGC_Number} IsSystemGenerated={usage.AGC_IsSystemGenerated}");
					}
					return message.ToString();
				}
				var actualUsagesMessage = $"Actual usages:{UsagesToString(usages)}";

				NUnit.Framework.Assert.That(usages.All(a => a.AGC_IsSystemGenerated), Is.EqualTo(true), $"Is System Generated\n{actualUsagesMessage}");

				foreach (var (code, owner, number) in expectedUsages)
				{
					var numMatched = usages.RemoveAll(a => a.AGC_Code == code && a.AGC_OH_Owner == owner && a.AGC_Number == number);
					NUnit.Framework.Assert.That(numMatched, Is.EqualTo(1), $"Expected exactly one usage of: Code={code} Owner={owner} Number={number}\n{actualUsagesMessage}");
				}
				NUnit.Framework.Assert.That(usages.Count, Is.EqualTo(0), $"Superflous usages:{UsagesToString(usages)}\n{actualUsagesMessage}");
			});
		}
	}
}
