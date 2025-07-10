using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CodeDescriptionOptionStorageExtensionTest : TestCaseWithFactory
	{
		public void TestGetSelectedOptionCodeAsString()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("", invoiceLine.CargoAttributes.GetSelectedOptionCodeAsString().ToString());
			var optionCollection = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes).OptionCollection;
			optionCollection.SelectedCodes = new List<ZString> { "18", "23", "25", "30", "xx" };
			optionCollection.RefreshSelectionCollection();
			AssertEquals("18,23,25,30", invoiceLine.CargoAttributes.GetSelectedOptionCodeAsString().ToString());
			AssertEquals("18|23|25|30", invoiceLine.CargoAttributes.GetSelectedOptionCodeAsString("|").ToString());
		}

		public void TestGetSelectedOptionDescAsString()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			AssertEquals("", invoiceLine.CargoAttributes.GetSelectedOptionDescAsString().ToString());
			var optionCollection = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes).OptionCollection;
			optionCollection.Load();
			optionCollection.SelectedCodes = new List<ZString> { "18", "23", "25", "30", "xx" };
			optionCollection.RefreshSelectionCollection();
			AssertEquals("首次进出口,带皮木材/板材,A级特殊物品,市场采购", invoiceLine.CargoAttributes.GetSelectedOptionDescAsString().ToString());
			AssertEquals("首次进出口|带皮木材/板材|A级特殊物品|市场采购", invoiceLine.CargoAttributes.GetSelectedOptionDescAsString("|").ToString());
		}

		[TestDate(2020, 3, 20)]
		public void TestValidateSelectedOptionAsString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = EntryTypeList.Codes.RecordListing;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.Paperless;
			AssertEquals("", instruction.OperationMatters.GetSelectedOptionDescAsString().ToString());
			var optionCollection = new CodeDescriptionOptionCollectionParent(Factory, instruction.OperationMatters).OptionCollection;
			optionCollection.Load();
			optionCollection.SelectedCodes = new List<ZString> { "PTF", "ATF", "AIC", "ABC" };
			optionCollection.RefreshSelectionCollection();
			AssertHasMessageError(instruction.OperationMattersAsStringInfo, "Only '通关无纸化' entry supports 担保验放.");
		}

		public void TestValidateMutuallyExclusiveCodes()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var instruction = testItems.EntryInstruction;
			var targetInfo = instruction.OperationMattersAsStringInfo;
			var operationMatters = instruction.OperationMatters;
			var aicOperationMatter = operationMatters.AddNew();
			aicOperationMatter.CY_Code = OperationMatterList.Codes.AssuredInspectClearance;
			var cdcOperationMatter = operationMatters.AddNew();
			cdcOperationMatter.CY_Code = OperationMatterList.Codes.ConsolidatedDutyCollection;
			var notBothAICAndCDCMessage = $"'{OperationMatterList.Descriptions.AssuredInspectClearance}' and '{OperationMatterList.Descriptions.ConsolidatedDutyCollection}' cannot be selected at the same time.";
			instruction.Validation.ValidateOperationMattersAsString();
			AssertHasMessageErrorContaining(targetInfo, notBothAICAndCDCMessage);
			aicOperationMatter.CY_Code = "";
			instruction.Validation.ValidateOperationMattersAsString();
			AssertNoMessageErrorContaining(targetInfo, notBothAICAndCDCMessage);
			aicOperationMatter.CY_Code = OperationMatterList.Codes.AssuredInspectClearance;
			var testParent = new CodeDescriptionOptionCollectionParent(Factory, operationMatters);
			var collection = testParent.OptionCollection;
			var cdcOption = collection.Cast<CodeDescriptionOption>().First(o => o.Code == OperationMatterList.Codes.ConsolidatedDutyCollection);
			var selectedInfo = cdcOption.SelectedInfo;
			AssertHasMessageErrorContaining(selectedInfo, notBothAICAndCDCMessage);
			var aicOption = collection.Cast<CodeDescriptionOption>().First(o => o.Code == OperationMatterList.Codes.AssuredInspectClearance);
			aicOption.Selected = false;
			cdcOption.Selected = false;
			cdcOption.Selected = true;
			AssertNoMessageErrorContaining(selectedInfo, notBothAICAndCDCMessage);
			aicOption.Selected = true;
			cdcOption.Selected = false;
			cdcOption.Selected = true;
			AssertHasMessageErrorContaining(selectedInfo, notBothAICAndCDCMessage);
		}
	}
}
