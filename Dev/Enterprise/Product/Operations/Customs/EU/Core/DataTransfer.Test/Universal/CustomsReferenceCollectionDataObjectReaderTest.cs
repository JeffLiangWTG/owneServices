using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	public class CustomsReferenceCollectionDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestCusReferenceDeletionAndAddition()
		{
			var mainAddressPK = CustomsReferenceDataObjectReaderTest.CreateTestOrgAddress(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MasterBill = "MYMASTER";
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var fiscalReference1 = entryInstruction.FiscalReferences.AddNew();
				fiscalReference1.CFR_Code = "FR3";
				fiscalReference1.CFR_OA_Owner = mainAddressPK;
				fiscalReference1.CFR_Reference = "FR335620241";
				var fiscalReference2 = entryInstruction.FiscalReferences.AddNew();
				fiscalReference2.CFR_Code = "FR2";
				fiscalReference2.CFR_OA_Owner = mainAddressPK;
				fiscalReference2.CFR_Reference = "FR335620242";
				Factory.SaveForTesting();

				var universalEntryInstructionData = new UniversalCustoms.EntryInstruction()
				{
					CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>() { CustomsReferenceDataObjectReaderTest.CreateCustomsReferenceBOForCusReference("FIS", "FR3", "FR335620241", CustomsReferenceDataObjectReaderTest.TestOrganizationAddress) }
				};
				var resultBOs = new CustomsReferenceCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany))
					.ReadIntoDataRows(entryInstruction.PK, entryInstruction.TablePrefix, entryInstruction.IsInDatabase, universalEntryInstructionData);

				AssertNotNull("ResultBOs exists", resultBOs);
				CombineAssertions(() =>
				{
					AssertEquals("Original FiscalReference BO deleted", true, fiscalReference1.IsDeleted);
					AssertEquals("Original SupplyChainActor BO deleted", true, fiscalReference2.IsDeleted);
					var result = resultBOs.SingleOrDefault();
					AssertEquals("Type", "FIS", result.GetValue(CusReferenceSchema.CFR_Type));
					AssertEquals("Code", "FR3", result.GetValue(CusReferenceSchema.CFR_Code));
					AssertEquals("Reference", "FR335620241", result.GetValue(CusReferenceSchema.CFR_Reference));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusReferenceSchema.CFR_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusReferenceSchema.CFR_ParentTableCode));
					AssertEquals("Owner", mainAddressPK, result.GetValue(CusReferenceSchema.CFR_OA_Owner));
				});
			}
		}

		public void TestCusAuthorizationDeletionAndAddition()
		{
			var orgPK = CustomsReferenceDataObjectReaderTest.CreateTestOrg(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MasterBill = "MYMASTER";
				declaration.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorizationUsage1 = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage1.AGC_Code = "ABC";
				cusAuthorizationUsage1.AGC_OH_Owner = orgPK;
				cusAuthorizationUsage1.AGC_Number = "12345";
				var cusAuthorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorizationUsage2.AGC_Code = "DEF";
				cusAuthorizationUsage2.AGC_OH_Owner = orgPK;
				cusAuthorizationUsage2.AGC_Number = "67890";
				Factory.SaveForTesting();

				var universalEntryInstructionData = new UniversalCustoms.EntryInstruction()
				{
					CustomsReferenceCollection = new List<UniversalCustoms.CustomsReference>() { CustomsReferenceDataObjectReaderTest.CreateCustomsReferenceBOForCusReference("AUT", "GHI", "99999", CustomsReferenceDataObjectReaderTest.TestOrganizationAddress) }
				};
				var resultBOs = new CustomsReferenceCollectionDataObjectReader(logger, new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Germany))
					.ReadIntoDataRows(entryInstruction.PK, entryInstruction.TablePrefix, entryInstruction.IsInDatabase, universalEntryInstructionData);

				AssertNotNull("ResultBOs exists", resultBOs);
				CombineAssertions(() =>
				{
					AssertEquals("Original CusAuthorizationUsage BO deleted", true, cusAuthorizationUsage1.IsDeleted);
					AssertEquals("Original SupplyChainActor BO deleted", true, cusAuthorizationUsage2.IsDeleted);
					var result = resultBOs.SingleOrDefault();
					AssertEquals("Code", "GHI", result.GetValue(CusAuthorizationUsageSchema.AGC_Code));
					AssertEquals("Reference", "99999", result.GetValue(CusAuthorizationUsageSchema.AGC_Number));
					AssertEquals("Parent PK", entryInstruction.PK, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentID));
					AssertEquals("Parent TableCode", entryInstruction.TablePrefix, result.GetValue(CusAuthorizationUsageSchema.AGC_ParentTableCode));
					AssertEquals("Owner", orgPK, result.GetValue(CusAuthorizationUsageSchema.AGC_OH_Owner));
				});
			}
		}
	}
}
