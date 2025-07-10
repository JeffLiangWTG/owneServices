using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(CodeDescriptionOptionCollection))]
	class CodeDescriptionOptionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CodeDescriptionOptionCollection>
	{
		protected override CodeDescriptionOptionCollection GetCollectionToTest()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			return new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes).OptionCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionOption(new CodeDescriptionOptionCollectionParent(Factory, Factory.New<JobComInvoiceLine>().CargoAttributes), new CodeDescriptionPair(CargoAttributeList.Codes._11, CargoAttributeList.Descriptions._11));
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(CodeDescriptionOptionCollection);
		}

		public void TestSelectedCodes()
		{
			var testItem = testParent.OptionCollection;
			var selectedCodes = testItem.SelectedCodes;
			AssertContainsExactElementsInAnyOrder(new[] { "11", "13" }, selectedCodes);
			testItem.SelectedCodes = new List<ZString> { "18", "23", "25", "30" };
			var selectedOptions = testItem.Cast<CodeDescriptionOption>().Where(option => option.Selected);
			AssertEquals(4, selectedOptions.Count());
			Assert(selectedOptions.Any(option => option.Code == "18"));
			Assert(selectedOptions.Any(option => option.Code == "23"));
			Assert(selectedOptions.Any(option => option.Code == "25"));
			Assert(selectedOptions.Any(option => option.Code == "30"));
		}

		public void TestLoad()
		{
			var allOptions = new CargoAttributeList().Cast<ICodeDescription>();
			AssertEquals(allOptions.Count(), testOptions.Count());
			foreach (var option in allOptions)
			{
				Assert(testOptions.Any(o => o.Code == option.Code));
			}
		}

		public void TestGroupedCodeDescriptionPairList()
		{
			AssertOptionSelectedChanged("11", "12", "13");
			AssertOptionSelectedChanged("12", "11", "13");
			AssertOptionSelectedChanged("13", "11", "12");
			AssertOptionSelectedChanged("18", "19", "20", "21", "22");
			AssertOptionSelectedChanged("21", "18", "19", "20", "22");
		}

		void AssertOptionSelectedChanged(string selectedChanged, params string[] expectedUnselected)
		{
			foreach (var s in expectedUnselected)
			{
				testOptions.First(option => option.Code == s).Selected = true;
			}

			testOptions.First(option => option.Code == selectedChanged).Selected = true;
			foreach (var expected in expectedUnselected)
			{
				Assert(selectedChanged + " selected, others of the group should be unselected, but found " + expected + " selected", !testOptions.FirstOrDefault(option => option.Code == expected).Selected);
			}
		}

		public void TestUnselectAll()
		{
			testParent.OptionCollection.UnselectAll();
			foreach (var option in testOptions)
			{
				Assert(!option.Selected);
			}
		}

		public void TestRefreshSelectionCollection()
		{
			invoiceLine.CargoAttributes.AddNew("XX");
			AssertContainsExactElementsInAnyOrder(new ZString[] { "11", "13", "XX" }, invoiceLine.CargoAttributes.Select(x => x.CY_Code));
			testParent.OptionCollection.SelectedCodes = new ZString[] { "11", "23", "25", "30" };
			testParent.OptionCollection.RefreshSelectionCollection();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "11", "23", "25", "30" }, invoiceLine.CargoAttributes.Select(x => x.CY_Code));
		}

		[TestDate(2020, 3, 20)]
		public void TestRefreshSelectionCollectionForValidation()
		{
			testItems.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			testItems.JobDeclaration.JE_MessageSubType = EntryTypeList.Codes.RecordListing;
			testItems.EntryInstruction.CEI_DocumentSubmissionType = EntryDocumentSubmissionTypes.Codes.Paperless;
			AssertEquals("", testItems.EntryInstruction.OperationMatters.GetSelectedOptionDescAsString().ToString());
			var optionCollection = new CodeDescriptionOptionCollection(new CodeDescriptionOptionCollectionParent(Factory, testItems.EntryInstruction.OperationMatters));
			optionCollection.Load();
			optionCollection.SelectedCodes = new List<ZString> { "PTF", "ATF", "AIC", "ABC" };
			optionCollection.RefreshSelectionCollection();
			AssertHasMessageError(testItems.EntryInstruction.OperationMattersAsStringInfo, "Only '通关无纸化' entry supports 担保验放.");
		}

		public void TestValidationOnLoadAndOptionSelected()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem).PK;
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "TESTUMF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CommodityType, "DGC", tariff);
			Factory.Save();
			var invoiceLine = (JobComInvoiceLine)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "TESTUMF";
			var testCollection = new CodeDescriptionOptionCollectionParent(Factory, invoiceLine.CargoAttributes).OptionCollection;
			testCollection.Load();
			var codeDescriptionList = testCollection.Cast<CodeDescriptionOption>();
			var messageText = "Tariff 'TESTUMF' seem to be dangerous chemical";
			AssertNoMessageErrorContaining("Only 31,32,33 should have the message error.", codeDescriptionList.FirstOrDefault(o => o.Code == "11").SelectedInfo, messageText);
			var item31 = codeDescriptionList.FirstOrDefault(o => o.Code == "31");
			var item32 = codeDescriptionList.FirstOrDefault(o => o.Code == "32");
			var item33 = codeDescriptionList.FirstOrDefault(o => o.Code == "33");
			AssertHasMessageErrorContaining("31,32,33 should have the message error.", item31.SelectedInfo, messageText);
			AssertHasMessageErrorContaining("31,32,33 should have the message error.", item32.SelectedInfo, messageText);
			AssertHasMessageErrorContaining("31,32,33 should have the message error.", item33.SelectedInfo, messageText);
			item32.Selected = true;
			AssertNoMessageErrorContaining("32 selected, 31&32&33 message errors should be cleared", item31.SelectedInfo, messageText);
			AssertNoMessageErrorContaining("32 selected, 31&32&33 message errors should be cleared", item32.SelectedInfo, messageText);
			AssertNoMessageErrorContaining("32 selected, 31&32&33 message errors should be cleared", item33.SelectedInfo, messageText);
		}

		JobComInvoiceLine invoiceLine;
		IEnumerable<CodeDescriptionOption> testOptions;
		CodeDescriptionOptionCollectionParent testParent;
		CNEntryHeaderTestData testItems;

		protected override void SetUp()
		{
			base.SetUp();
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var invoiceLine = testItems.InvoiceLine;
			var cusCodeData1 = Factory.New<CargoAttribute>();
			cusCodeData1.CY_Code = "11";
			cusCodeData1.CY_ParentTableCode = invoiceLine.TablePrefix;
			cusCodeData1.CY_Type = "CAT";
			cusCodeData1.CY_ParentID = invoiceLine.PK;
			var cusCodeData2 = Factory.New<CargoAttribute>();
			cusCodeData2.CY_Code = "13";
			cusCodeData2.CY_ParentTableCode = invoiceLine.TablePrefix;
			cusCodeData2.CY_Type = "CAT";
			cusCodeData2.CY_ParentID = invoiceLine.PK;
			Factory.Save();
			this.testItems = testItems;
			this.invoiceLine = Factory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.CargoAttributes.Load();
			testParent = new CodeDescriptionOptionCollectionParent(Factory, this.invoiceLine.CargoAttributes);
			testOptions = testParent.OptionCollection.Cast<CodeDescriptionOption>();
		}
	}
}
