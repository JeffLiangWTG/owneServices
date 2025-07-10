using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	partial class CustomsReferenceCollectionCreatorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestCustomsReferenceMappingsFromCusReference()
		{
			var mainAddressPK = CustomsReferenceDataObjectReaderTest.CreateTestOrgAddress(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var fiscalReference = entryInstruction.FiscalReferences.AddNew();
				fiscalReference.CFR_Code = "FR3";
				fiscalReference.CFR_OA_Owner = mainAddressPK;
				fiscalReference.CFR_Reference = "FR335620241";
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (UniversalDataBuss.DataObjects.Universal.Shipment)writer.GetDataObject(declaration);
					var customsReference = shipmentData.EntryInstructionCollection[0].CustomsReferenceCollection.SingleOrDefault(cr => cr.Type.Code.GetValueOrDefault() == "FIS");
					CombineAssertions(() =>
					{
						AssertNotNull(customsReference.Type.Code);
						AssertEquals("Code", "FR3", customsReference.SubType.Code);
						AssertEquals("FR335620241", customsReference.Reference);
						AssertEquals("TESTORG", customsReference.Owner.OrganizationCode);
					});
				}
			}
		}

		public void TestCustomsReferenceMappingsFromCusAuthorizationUsage()
		{
			var mainAddressPK = CustomsReferenceDataObjectReaderTest.CreateTestOrg(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorisationUsage.AGC_Code = "ABC";
				cusAuthorisationUsage.AGC_OH_Owner = mainAddressPK;
				cusAuthorisationUsage.EffectiveReferenceNumber = "12345";
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					var shipmentData = (UniversalDataBuss.DataObjects.Universal.Shipment)writer.GetDataObject(declaration);
					var customsReference = shipmentData.EntryInstructionCollection[0].CustomsReferenceCollection.SingleOrDefault(cr => cr.Type.Code.GetValueOrDefault() == "AUT");
					CombineAssertions(() =>
					{
						AssertNotNull(customsReference);
						AssertEquals("Code", "ABC", customsReference.SubType.Code);
						AssertEquals("12345", customsReference.Reference);
						AssertEquals("TESTORG", customsReference.Owner.OrganizationCode);
					});
				}
			}
		}

		public void TestCustomsReferenceMappingsFromCusAuthorizationUsageWithNullCustomsReferenceOwner()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorisationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				cusAuthorisationUsage.AGC_Code = "ABC";
				cusAuthorisationUsage.EffectiveReferenceNumber = "12345";
				Factory.SaveForTesting();

				var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
				using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
				{
					AssertNoExceptionThrown(() =>
					{
						var shipmentData = (UniversalDataBuss.DataObjects.Universal.Shipment)writer.GetDataObject(declaration);
						var customsReference = shipmentData.EntryInstructionCollection[0].CustomsReferenceCollection.SingleOrDefault();
						AssertNull("customsReference.Owner", customsReference.Owner);
					});
				}
			}
		}
	}
}
