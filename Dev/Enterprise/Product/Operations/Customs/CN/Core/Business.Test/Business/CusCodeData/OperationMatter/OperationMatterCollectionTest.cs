using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OperationMatterCollection))]
	class OperationMatterCollectionTest : CusCodeDataCollectionTest<OperationMatter>
	{
		protected override CusCodeDataCollection<OperationMatter> GetCusCodeDataCollection()
		{
			return new OperationMatterCollection(Factory.New<CusEntryInstruction>());
		}

		public void TestGetAllOptions()
		{
			var codeList = ((GetCusCodeDataCollection() as OperationMatterCollection).GetAllOptions() as CodeDescriptionPairList).GetAllCodes();
			AssertCollectionNotContains(OperationMatterList.Codes.PaperlessTaxForm, codeList);
			AssertCollectionNotContains(OperationMatterList.Codes.AutonomousTaxFiling, codeList);
			AssertCollectionContains(OperationMatterList.Codes.ConsolidatedDutyCollection, codeList);
		}

		public void TestValidateSeletedOption()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var instruction = testItems.EntryInstruction;
			var targetInfo = instruction.OperationMattersAsStringInfo;
			var operationMatters = instruction.OperationMatters;
			var cdcOperationMatter = operationMatters.AddNew();
			cdcOperationMatter.CY_Code = OperationMatterList.Codes.ConsolidatedDutyCollection;
			var cdcNotSupportRECMessage = $"'{OperationMatterList.Descriptions.ConsolidatedDutyCollection}' does not support {DecTypeList.Descriptions.RecordListing}";
			instruction.Validation.ValidateOperationMattersAsString();
			AssertNoMessageErrorContaining(targetInfo, cdcNotSupportRECMessage);
			instruction.JobDeclaration.JE_MessageSubType = EntryTypeList.Codes.RecordListing;
			instruction.Validation.ValidateOperationMattersAsString();
			AssertHasMessageErrorContaining(targetInfo, cdcNotSupportRECMessage);
			var dutyModeMessage = $"'{OperationMatterList.Descriptions.ConsolidatedDutyCollection}' should not be selected due to some Invoice Lines with Duty Mode 3,6,7.";
			ZString[] dutyModes = { DutyModeList.Codes._3, DutyModeList.Codes._6, DutyModeList.Codes._7 };
			instruction.Validation.ValidateOperationMattersAsString();
			AssertNoMessageErrorContaining(targetInfo, dutyModeMessage);
			foreach (var dutyMode in dutyModes)
			{
				testItems.InvoiceLine.JI_DutyMode = dutyMode;
				instruction.Validation.ValidateOperationMattersAsString();
				AssertHasMessageErrorContaining(targetInfo, dutyModeMessage);
			}
		}

		public void TestValidationModeProvider()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var instruction = testItems.EntryInstruction;
			var collection = new OperationMatterCollection(instruction);
			ValidationExtensionsTest.AssertValidationModeProvider(instruction.JobDeclaration, collection.ValidationModeProvider);

			AssertNull((GetCusCodeDataCollection() as OperationMatterCollection).ValidationModeProvider);
		}
	}
}
