using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class CodeDescriptionPairListTest : ReadOnlyCodeDescriptionPairListTest
	{
		public void TestEInvoicingBatchState()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.EInvoicingBatchState);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(Constants.EInvoicingBatchState.Ready));
			Assert(list.ContainsCode(Constants.EInvoicingBatchState.Sent));
			Assert(list.ContainsCode(Constants.EInvoicingBatchState.Discarded));
		}

		public void TestIAdditionalInformationWithSetterMembers()
		{
			IAdditionalInformationWithSetter list = new CodeDescriptionPairList();
			AssertNull(list.AdditionalInformation);
			list.SetAdditionalInformation("HELLO World");
			AssertEquals("HELLO World", list.AdditionalInformation);
		}

		public void TestPeriodApportionmentMethods()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.PeriodApportionmentMethods);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(Constants.PeriodApportionmentMethods.Codes.Default));
			Assert(list.ContainsCode(Constants.PeriodApportionmentMethods.Codes.EquallyOverPeriods));
			Assert(list.ContainsCode(Constants.PeriodApportionmentMethods.Codes.Manual));
			Assert(list.ContainsCode(Constants.PeriodApportionmentMethods.Codes.Day));
		}

		public void TestEInvoicingPivotState()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.EInvoicingPivotState);
			AssertEquals(12, list.Count);
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Queued));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Batched));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.BatchedWithError));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Sent));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Delivered));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Succeed));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Failed));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Discarded));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.Pending));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.AwaitingReview));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.InProcessing));
			Assert(list.ContainsCode(Constants.EInvoicingPivotState.NotEligible));
		}

		public void TestComplianceDocumentStatus()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.ComplianceDocumentStatus);
			AssertEquals(4, list.Count);
			Assert(list.ContainsCode(Constants.ComplianceDocumentStatus.Added));
			Assert(list.ContainsCode(Constants.ComplianceDocumentStatus.Voided));
			Assert(list.ContainsCode(Constants.ComplianceDocumentStatus.NumberSet));
			Assert(list.ContainsCode(Constants.ComplianceDocumentStatus.Finalised));
		}

		public void TestEFreightStatus()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.EFreightStatus);
			AssertPairEquals(new CodeDescriptionPair(Constants.EFreightStatus.Code.NON, Constants.EFreightStatus.Description.NON), list[0]);
			AssertPairEquals(new CodeDescriptionPair(Constants.EFreightStatus.Code.EAP, Constants.EFreightStatus.Description.EAP), list[1]);
			AssertPairEquals(new CodeDescriptionPair(Constants.EFreightStatus.Code.EAW, Constants.EFreightStatus.Description.EAW), list[2]);
		}

		public void TestAgentTypes()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.AgentType);
			AssertPairEquals(new CodeDescriptionPair(Constants.AgentType.Direct, Constants.AgentTypeDescriptions.Direct), list[0]);
			AssertPairEquals(new CodeDescriptionPair(Constants.AgentType.CoLoad, Constants.AgentTypeDescriptions.CoLoad), list[1]);
			AssertPairEquals(new CodeDescriptionPair(Constants.AgentType.Agent, Constants.AgentTypeDescriptions.Agent), list[2]);
			AssertPairEquals(new CodeDescriptionPair(Constants.AgentType.Charter, Constants.AgentTypeDescriptions.Charter), list[3]);
			AssertPairEquals(new CodeDescriptionPair(Constants.AgentType.Courier, Constants.AgentTypeDescriptions.Courier), list[4]);
			AssertPairEquals(new CodeDescriptionPair(Constants.AgentType.Other, Constants.AgentTypeDescriptions.Other), list[5]);
		}

		public void TestTemperatureTypes()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.TemperatureTypes);
			AssertPairEquals(new CodeDescriptionPair(Constants.Temperature.Centigrade, Constants.Temperature.GetDescription(Constants.Temperature.Centigrade)), list[0]);
			AssertPairEquals(new CodeDescriptionPair(Constants.Temperature.Fahrenheit, Constants.Temperature.GetDescription(Constants.Temperature.Fahrenheit)), list[1]);
		}

		public void TestCachedList()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			AssertNoExceptionThrown(() => list.AddPair("BOB", "BUILDER"));
			AssertNoExceptionThrown(() => list.RemoveCode("BOB"));
			var pair = new CodeDescriptionPair("WENDY", "DESTROYER");
			AssertNoExceptionThrown(() => list.Add(pair));
			AssertNoExceptionThrown(() => list.Clear());
			AssertNoExceptionThrown(() => list.Insert(0, pair));
			AssertNoExceptionThrown(() => list.Remove(pair));
			AssertNoExceptionThrown(() => list.AddOverwriteIfExists(pair));
			AssertNoExceptionThrown(() => list.RemoveAt(0));
			var factory = new BusinessObjectFactory();
			var list2 = factory.GetCachedValue("CodeDescriptionPairListTestCachedKey", () => list);
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot add item to a cached list", () => list2.AddPair("BOB", "BUILDER"));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot remove item from a cached list", () => list2.RemoveCode("KG"));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot clear item from a cached list", () => list2.Clear());
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot insert item to a cached list", () => list2.Insert(0, pair));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot remove item from a cached list", () => list2.Remove(pair));
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot remove item from a cached list", () => list2.RemoveAt(0));
		}

		public void TestOperatorPlus()
		{
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			list1.AddPair("foo", "bar");
			list1.AddPair("foofoo", "barbar");

			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			list2.AddPair("spam", "eggs");
			list2.AddPair("spamspam", "eggeggs");

			CodeDescriptionPairList combinedList = list1 + list2;

			AssertNotNull("combined list isn't null", combinedList);
			Assert("combined list isn't list1", combinedList != list1);
			Assert("combined list isn't list2", combinedList != list2);

			AssertEquals(2, list1.Count);
			AssertPairEquals(new CodeDescriptionPair("foo", "bar"), list1[0]);
			AssertPairEquals(new CodeDescriptionPair("foofoo", "barbar"), list1[1]);

			AssertEquals(2, list2.Count);
			AssertPairEquals(new CodeDescriptionPair("spam", "eggs"), list2[0]);
			AssertPairEquals(new CodeDescriptionPair("spamspam", "eggeggs"), list2[1]);

			AssertEquals(4, combinedList.Count);
			AssertPairEquals(new CodeDescriptionPair("foo", "bar"), combinedList[0]);
			AssertPairEquals(new CodeDescriptionPair("foofoo", "barbar"), combinedList[1]);
			AssertPairEquals(new CodeDescriptionPair("spam", "eggs"), combinedList[2]);
			AssertPairEquals(new CodeDescriptionPair("spamspam", "eggeggs"), combinedList[3]);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestOperatorPlusFirstArgNull()
		{
			CodeDescriptionPairList list1 = null;
			CodeDescriptionPairList list2 = new CodeDescriptionPairList();
			CodeDescriptionPairList result = list1 + list2;
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestOperatorPlusSecondArgNull()
		{
			CodeDescriptionPairList list1 = new CodeDescriptionPairList();
			CodeDescriptionPairList list2 = null;
			CodeDescriptionPairList result = list1 + list2;
		}

		void AssertPairEquals(ICodeDescription expectedPair, ICodeDescription actualPair)
		{
			AssertEquals(expectedPair.Code + expectedPair.Description, actualPair.Code + actualPair.Description);
		}

		public void TestRemoveCode()
		{
			List.AddPair("ABC", "Alphabet");
			List.AddPair("ZYX", "Reverse Alphabet");
			List.AddPair("ZUB", "ZUBS");

			List.RemoveCode("ABC");

			AssertEquals("2 items remaining", 2, List.Count);
			AssertEquals("ABC Removed", false, List.ContainsCode("ABC"));
		}

		public void TestInsertInSortOrder()
		{
			var testList = new CodeDescriptionPairList();
			testList.InsertInSortOrder(new CodeDescriptionPair("ZZZ", (NoResString)"Last Item"));
			testList.InsertInSortOrder(new CodeDescriptionPair("000", (NoResString)"First Item"));
			AssertEquals("First Item Appears in first index", "000", testList[0].Code);
			AssertEquals("Last Item Appears in last index", "ZZZ", testList[1].Code);
		}

		public void TestInsertInSortOrderByDescription()
		{
			var testList = new CodeDescriptionPairList();
			testList.InsertInSortOrderByDescription(new CodeDescriptionPair("A", (NoResString)"3"));
			testList.InsertInSortOrderByDescription(new CodeDescriptionPair("B", (NoResString)"1"));
			testList.InsertInSortOrderByDescription(new CodeDescriptionPair("C", (NoResString)"4"));
			testList.InsertInSortOrderByDescription(new CodeDescriptionPair("D", (NoResString)"2"));
			testList.InsertInSortOrderByDescription(new CodeDescriptionPair("A", (NoResString)"2"));

			AssertEquals("testList[0].Description", "1", testList[0].Description);
			AssertEquals("testList[1].Description", "2", testList[1].Description);
			AssertEquals("testList[2].Description", "2", testList[2].Description);
			AssertEquals("testList[3].Description", "3", testList[3].Description);
			AssertEquals("testList[4].Description", "4", testList[4].Description);

			AssertEquals("testList[0].Code", "B", testList[0].Code);
			AssertEquals("testList[1].Code", "A", testList[1].Code);
			AssertEquals("testList[2].Code", "D", testList[2].Code);
			AssertEquals("testList[3].Code", "A", testList[3].Code);
			AssertEquals("testList[4].Code", "C", testList[4].Code);
		}

		public void TestSort()
		{
			CodeDescriptionPairList testList = new CodeDescriptionPairList(OLookUpEditType.AccountOrderType);
			testList.AddPair("ZZZ", "Last Item");
			testList.AddPair("000", "First Item");
			testList.Sort();
			AssertEquals("First Item Appears in first index", "000", testList[0].Code);
			AssertEquals("Last Item Appears in last index", "ZZZ", testList[testList.Count - 1].Code);
		}

		public void TestSortByDescription()
		{
			List.AddPair("A", "3");
			List.AddPair("B", "1");
			List.AddPair("C", "4");
			List.AddPair("D", "2");
			List.AddPair("A", "2");

			List.SortByDescription();
			AssertEquals("List[0].Description", "1", List[0].Description);
			AssertEquals("List[1].Description", "2", List[1].Description);
			AssertEquals("List[2].Description", "2", List[2].Description);
			AssertEquals("List[3].Description", "3", List[3].Description);
			AssertEquals("List[4].Description", "4", List[4].Description);

			AssertEquals("List[0].Code", "B", List[0].Code);
			AssertEquals("List[1].Code", "A", List[1].Code);
			AssertEquals("List[2].Code", "D", List[2].Code);
			AssertEquals("List[3].Code", "A", List[3].Code);
			AssertEquals("List[4].Code", "C", List[4].Code);
		}

		public void TestSortByDescriptionAndCombineIfSameCode()
		{
			var actualList = new CodeDescriptionPairList();
			var expectedList = new CodeDescriptionPairList();

			var pairB1 = new CodeDescriptionPair("BBB", "Ze Desc CC"); // length 10
			var pairB2 = new CodeDescriptionPair("BBB", "Ze Desc CC"); // length 10
			var pairB3 = new CodeDescriptionPair("BBB", "Ze Desc A, length  21");
			var pairB4 = new CodeDescriptionPair("BBB", "Ze Desc B, length  21");
			var pairC1 = new CodeDescriptionPair("CCC", "Desc B, length  18");
			var pairC2 = new CodeDescriptionPair("CCC", "Desc A, length  18");
			var pairC3 = new CodeDescriptionPair("CCC ", "Desc CC "); // length 9 Description will not be trimmed.
			var pairC4 = new CodeDescriptionPair("CCC", "Desc CC"); // length 8
			var unrelatedPair1 = new CodeDescriptionPair("AAA", "Ze pair at the end of list");
			var unrelatedPair2 = new CodeDescriptionPair("ZZZ", "A pair at the start of list");

			actualList.Add(pairB1);
			actualList.Add(pairB2);
			actualList.Add(pairC1);
			actualList.Add(pairC2);
			actualList.Add(unrelatedPair1);
			actualList.Add(unrelatedPair2);
			actualList.SortByDescriptionAndCombineIfSameCode();

			expectedList.AddPair("ZZZ", "A pair at the start of list");
			expectedList.AddPair("CCC", "Desc A, length  18, Desc B, length  18");
			expectedList.AddPair("BBB", "Ze Desc CC");
			expectedList.AddPair("AAA", "Ze pair at the end of list");
			AssertContainsExactElementsInExactOrder(expectedList, actualList);

			actualList = new CodeDescriptionPairList();
			actualList.Add(pairB1);
			actualList.Add(pairB2);
			actualList.Add(pairB3);
			actualList.Add(pairC1);
			actualList.Add(pairC2);
			actualList.Add(pairC3);
			actualList.Add(unrelatedPair1);
			actualList.Add(unrelatedPair2);
			actualList.SortByDescriptionAndCombineIfSameCode();

			expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("ZZZ", "A pair at the start of list");
			expectedList.AddPair("CCC", "Desc A, length  18, Desc B, length  18, Desc CC "); // Description will not be trimmed.
			expectedList.AddPair("BBB", "Ze Desc A, length  21, Ze Desc CC");
			expectedList.AddPair("AAA", "Ze pair at the end of list");
			AssertContainsExactElementsInExactOrder(expectedList, actualList);

			actualList = new CodeDescriptionPairList();
			actualList.Add(pairB1);
			actualList.Add(pairB2);
			actualList.Add(pairB3);
			actualList.Add(pairB4);
			actualList.Add(pairC1);
			actualList.Add(pairC2);
			actualList.Add(pairC3);
			actualList.Add(pairC4);
			actualList.Add(unrelatedPair1);
			actualList.Add(unrelatedPair2);
			actualList.SortByDescriptionAndCombineIfSameCode();

			expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("ZZZ", "A pair at the start of list");
			expectedList.AddPair("CCC", "Desc A, length  18, Desc B, length  18, Desc CC, ...");
			expectedList.AddPair("BBB", "Ze Desc A, length  21, Ze Desc B, length  21, ...");
			expectedList.AddPair("AAA", "Ze pair at the end of list");
			AssertContainsExactElementsInExactOrder(expectedList, actualList);
		}

		void AssertContainsExactElementsInExactOrder(CodeDescriptionPairList expected, CodeDescriptionPairList actual)
		{
			AssertEquals("Count", expected.Count, actual.Count);
			for (int i = 0; i < expected.Count; i++)
			{
				AssertEquals(expected[i].Code, actual[i].Code);
				AssertEquals(expected[i].Description, actual[i].Description);
			}
		}

		public void TestCodeIndexer()
		{
			List.AddPair("Code1", "Description1");
			List.AddPair("Code2", "Description2");

			AssertEquals("Description1", List["Code1"].Description);
			AssertEquals("Description2", List["Code2"].Description);

			AssertNull(List["CODE1"]);
		}

		public void TestCodeIndexer_CaseInsensitive()
		{
			List.AddPair("Code1", "Description1");
			List.AddPair("Code2", "Description2");

			AssertEquals("Description1", List["CODE1", StringComparison.OrdinalIgnoreCase].Description);
			AssertEquals("Description2", List["CODE2", StringComparison.OrdinalIgnoreCase].Description);
		}

		public void TestCodeIndexer_MultilingualCode()
		{
			using (var chsMockData = Res.GetLanguageInstance(SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				chsMockData.Put("1", new ResourceStringData("1", "一"));

				var list = new CodeDescriptionPairList();
				var code = (NoResString)"One";
				list.AddPair(code);

				AssertEquals(list[code], list[code.GetUnresolvedString()]);
				AssertEquals(list[code].Code, code.GetUnresolvedString());

				using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals(list[code], list[code.GetUnresolvedString()]);
					AssertEquals(list[code].Code, code.GetUnresolvedString());
				}
			}
		}

		public void TestSettingViaIndexer()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");

			List.AddPair("", "");
			List.AddPair("", "");

			List[0] = pair1;
			List[1] = pair2;

			AssertEquals("List[0]", pair1, List[0]);
			AssertEquals("List[1]", pair2, List[1]);
		}

		public void TestAddIfNotExist()
		{
			List.AddPairIfNotExist("ABC", "Test ABC");
			List.AddPairIfNotExist("BCD", "Test BCD");
			List.AddPairIfNotExist("ABC", "Test ABC");

			AssertEquals("Count", 2, List.Count);
			Assert("Contains ABC", List.ContainsCode("ABC"));
			Assert("Contains BCD", List.ContainsCode("BCD"));
		}

		public void TestAddOverwriteIfExists()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("C1", "Old Desc");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("C1", "New Desc");

			List.Add(pair1);
			List.AddOverwriteIfExists(pair2);

			AssertEquals("Should have replaced the old element", 1, List.Count);
			AssertEquals("Should have returned the new description", List.GetDescriptionFromCode("C1"), "New Desc");
		}

		public void TestAddRangeOverwriteIfExists()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("C1", "Old Desc");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("C1", "New Desc");

			List<CodeDescriptionPair> list = new List<CodeDescriptionPair>();
			list.Add(pair2);

			List.Add(pair1);
			List.AddRangeOverwriteIfExists(list);

			AssertEquals("Should have replaced the old element", 1, List.Count);
			AssertEquals("Should have returned the new description", List.GetDescriptionFromCode("C1"), "New Desc");
		}

		public void TestOverwriteCodeDescriptionPairInPlace()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("C1", "Old Desc 1");
			list.AddPair("C2", "Old Desc 2");
			list.AddPair("C3", "Old Desc 3");
			list.OverwriteDescriptionForCodeInPlace("C2", "New Desc 2");
			AssertEquals("Should have replaced the old element", "New Desc 2", list["C2"].Description);
			AssertEquals("Order should have been same", 1, list.IndexOfCode("C2"));
		}

		public void TestDebtorTypes()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.DebtorTypes);
			AssertEquals(2, list.Count);
			Assert(list.ContainsCode(Constants.DebtorTypes.Code.DebtorGroup));
			AssertEquals(Constants.DebtorTypes.Description.DebtorGroup, list[Constants.DebtorTypes.Code.DebtorGroup].Description);
			Assert(list.ContainsCode(Constants.DebtorTypes.Code.DebtorOrganisation));
			AssertEquals(Constants.DebtorTypes.Description.DebtorOrganisation, list[Constants.DebtorTypes.Code.DebtorOrganisation].Description);
		}

		public void TestDefaultCargoReportConsigneeOption()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.DefaultCargoReportConsigneeOption);
			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("CON", "Consignee");
			expectedList.AddPair("DEL", "Deliver To");
			expectedList.AddPair("NON", "None");

			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestTaxOverrideTransactionContext()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.TaxOverrideTransactionContext);
			AssertEquals(3, list.Count);

			var expectedList = new CodeDescriptionPairList();

			expectedList.AddPair("ALL", "All Context");
			expectedList.AddPair("INT", "Intercompany Invoice Import");
			expectedList.AddPair("STD", "Standard Context");

			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestTaxOverrideDefaultingRule()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.TaxOverrideDefaultingRule);
			AssertEquals(3, list.Count);

			var expectedList = new CodeDescriptionPairList();

			expectedList.AddPair("ART", "Copy AR Transaction's Tax ID, Ex-Tax Amount, Tax Amount as AP");
			expectedList.AddPair("NON", "Not Applicable");
			expectedList.AddPair("SUM", "Sum AR Transaction's Ex-Tax Amount and Tax Amount as AP Transaction's Ex-Tax Amount");

			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestPaymentBatchStatus()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.PaymentBatchStatus);
			AssertEquals(3, list.Count);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("WRK", "Working");
			expectedList.AddPair("CMP", "Completed");
			expectedList.AddPair("CAN", "Canceled");

			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestFacilityTypes()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.FacilityTypes);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(Constants.FacilityType.Code.Terminal));
			Assert(list.ContainsCode(Constants.FacilityType.Code.ContainerYard));
			Assert(list.ContainsCode(Constants.FacilityType.Code.TransitWarehouse));
		}

		#region IList

		public override void TestIListAdd()
		{
			CodeDescriptionPair x = new CodeDescriptionPair("", "");
			int index = ListAsIList.Add(x);
			AssertEquals("List[0]", x, List[0]);
			AssertEquals("Add()", 0, index);

			CodeDescriptionPair y = new CodeDescriptionPair("", "");
			index = ListAsIList.Add(y);
			AssertEquals("List[1]", y, List[1]);
			AssertEquals("Add()", 1, index);
		}

		public override void TestIListClear()
		{
			CodeDescriptionPair pair = new CodeDescriptionPair("", "");

			ListAsIList.Add(pair);
			ListAsIList.Clear();
			AssertEquals("Count", 0, List.Count);
		}

		public override void TestIListIndexerSetter()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");

			List.AddPair("", "");
			List.AddPair("", "");

			ListAsIList[0] = pair1;
			ListAsIList[1] = pair2;

			AssertEquals("List[0]", pair1, List[0]);
			AssertEquals("List[1]", pair2, List[1]);
		}

		public override void TestIListInsert()
		{
			CodeDescriptionPair pair = new CodeDescriptionPair("", "");

			List.AddPair("", "");
			List.AddPair("", "");

			ListAsIList.Insert(1, pair);
			AssertEquals("Count", 3, List.Count);
			AssertEquals("List[1]", pair, List[1]);
		}

		public override void TestIListRemove()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");

			List.Add(pair1);
			List.Add(pair2);

			ListAsIList.Remove(pair2);
			AssertEquals("Count", 1, List.Count);
			AssertEquals("List[0]", pair1, List[0]);
		}

		public override void TestIListRemoveAt()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");

			List.Add(pair1);
			List.Add(pair2);

			ListAsIList.RemoveAt(1);
			AssertEquals("Count", 1, List.Count);
			AssertEquals("List[0]", pair1, List[0]);
		}

		#endregion

		#region Defined Lookups

		public void TestOLookupEditTypes()
		{
			foreach (OLookUpEditType value in Enum.GetValues(typeof(OLookUpEditType)))
			{
				if (value != OLookUpEditType.CustomType && value != OLookUpEditType.EquipmentGroup)
				{
					CodeDescriptionPairList testList = new CodeDescriptionPairList(value);
					Assert("CodeDescriptionPairList of OLookUpEditType " + value + " Should have at least one Item", testList.Count > 0);
				}
			}
		}

		public void TestWeightAndVolumeDisplayTypesDescriptionsAreTranslatable()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.WeightAndVolumeDisplayTypes);
			foreach (CodeDescriptionPair pair in list)
			{
				AssertNotNull(pair);
				var isMultiLingual = pair is IMultilingualDescription;
				var isResString = IsResString(((IMultilingualDescription)pair).MultilingualDescription);
				Assert(string.Format("List {0} does not properly use a Multilingual string for {1} - {2}", OLookUpEditType.WeightAndVolumeDisplayTypes, pair.Code, pair.Description), (isMultiLingual && isResString));
			}
		}

		public void TestAllOLookupEditTypesUseMultilingualStrings()
		{
			CombineAssertions(delegate
			{
				foreach (OLookUpEditType value in Enum.GetValues(typeof(OLookUpEditType)))
				{
					if (value != OLookUpEditType.CustomType && value != OLookUpEditType.Months && !value.ToString().StartsWith("US") && value != OLookUpEditType.AWBChargeCodes && !value.ToString().Contains("IncoTerms"))
					{
						CodeDescriptionPairList list = new CodeDescriptionPairList(value);
						foreach (ICodeDescription pair in list)
						{
							Assert(string.Format("List {0} does not properly use a Multilingual string for {1} - {2}", value, pair.Code, pair.Description), (pair is IMultilingualDescription && IsResString(((IMultilingualDescription)pair).MultilingualDescription)) || string.IsNullOrEmpty(pair.Description) || pair.Description == pair.Code);
						}
					}
				}
			});
		}

		bool IsResString(MultilingualString s)
		{
			return s is ResourceString || (s is ModifiedMultilingualString && ((ModifiedMultilingualString)s).Strings.All(item => IsResString(item)));
		}

		public void TestNotifyMode()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
			AssertEquals("NotifyMode number of items", 4, list.Count);
			AssertEquals(true, list.ContainsCode(Constants.ContactNotifyModes.Email));
			AssertEquals(true, list.ContainsCode(Constants.ContactNotifyModes.Fax));
			AssertEquals(true, list.ContainsCode(Constants.ContactNotifyModes.Print));
			AssertEquals(true, list.ContainsCode(Constants.ContactNotifyModes.EPrint));
		}

		public void TestRoundingRules()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.RoundingRules);
			AssertEquals("Count", 3, list.Count);
			Assert(list.ContainsCode(Constants.RoundingRules.Codes.None));
			Assert(list.ContainsCode(Constants.RoundingRules.Codes.JapanYen));
			Assert(list.ContainsCode(Constants.RoundingRules.Codes.JapanYenWithCharge));
		}

		public void TestDateFilterType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.DateFilterType);
			AssertEquals("Count", 4, list.Count);
			Assert("Contains STD", list.ContainsCode("STD"));
			Assert("Contains DEP", list.ContainsCode("DEP"));
			Assert("Contains ARR", list.ContainsCode("ARR"));
			Assert("Contains CUS", list.ContainsCode("CUS"));
		}

		public void TestComplianceRollupBehaviourType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ComplianceRollupBehaviourType);
			AssertEquals("Count", 3, list.Count);
			Assert("Contains NON", list.ContainsCode("MNL"));
			Assert("Contains ROL", list.ContainsCode("SRA"));
			Assert("Contains ROL", list.ContainsCode("SSM"));
		}

		public void TestComplianceBookAllocationLevel()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ComplianceBookAllocationLevel);
			AssertEquals("Count", 4, list.Count);
			Assert("Contains COM", list.ContainsCode("COM"));
			Assert("Contains BRN", list.ContainsCode("BRN"));
			Assert("Contains BDP", list.ContainsCode("BDP"));
			Assert("Contains CTR", list.ContainsCode("CTR"));
		}

		public void TestNationalityType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.NationalityType);
			AssertEquals("Count", 4, list.Count);
		}

		public void TestNZProcessingPort()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.NZProcessingPort);
			AssertEquals("Count", 9, list.Count);
		}

		public void TestDocumentTransportMode()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode);
			AssertEquals("Count", 7, list.Count);
		}

		public void TestVesselType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.VesselType);
			AssertEquals("Count", 23, list.Count);
		}

		public void TestConsolInvoicingStyles()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ConsolInvoicingStyles);
			AssertEquals("Count", 3, list.Count);
			Assert("Contains CON", list.ContainsCode("MAS"));
			Assert("Contains APP", list.ContainsCode("APP"));
			Assert("Contains MAB", list.ContainsCode("MAB"));
		}

		public void TestChargeTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ChargeTypes);
			AssertEquals("Count", 7, list.Count);
		}

		public void TestConversionFactor()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ConversionFactor);
			AssertEquals("Count", 2, list.Count);
			Assert("Contains MAS", list.ContainsCode("CON"));
			Assert("Contains W/M", list.ContainsCode("W/M"));
		}

		public void TestACPeriodCountTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ACPeriodCountType);
			AssertEquals("Count", 4, list.Count);
		}

		public void TestTransactionTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.TransactionTypes);
			AssertEquals("Count TransactionTypes", 20, list.Count);
		}

		public void TestARAPTransactionTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ARAPTransactionTypes);
			AssertEquals("Count TransactionTypes", 12, list.Count);
		}

		public void TestBankChargeTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.BankChargeTypes);
			AssertEquals("Count BankChargeTypes", 9, list.Count);
		}

		public void TestPaymentMethod()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, true))
			{
				var listWhenRegistriesEnabled = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
				AssertEquals("Count PaymentMethod", 19, listWhenRegistriesEnabled.Count);
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, false))
			{
				var listWhenRegistryIsDisabled = new CodeDescriptionPairList(OLookUpEditType.PaymentMethod);
				AssertEquals("Count PaymentMethod", 18, listWhenRegistryIsDisabled.Count);
			}
		}

		public void TestPaymentOrReceiptMethod()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PaymentOrReceiptMethod);
			AssertEquals("Count PaymentOrReceiptMethod", 19, list.Count);
		}

		public void TestReceiptMethod()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ReceiptMethod);
			AssertEquals("Count ReceiptMethod", 14, list.Count);
		}

		public void TestWIPAccrualTransactionTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.WIPAccrualTransactionTypes);
			AssertEquals("Count TransactionTypes", 2, list.Count);
		}

		public void TestDomesticPaymentTerms()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms);

			AssertEquals("There are only 4 Payment Terms", 4, list.Count);
			Assert(list.ContainsCode(Constants.DomesticPaymentTerms.Prepaid));
			Assert(list.ContainsCode(Constants.DomesticPaymentTerms.Collect));
			Assert(list.ContainsCode(Constants.DomesticPaymentTerms.CollectThirdParty));
			Assert(list.ContainsCode(Constants.DomesticPaymentTerms.CollectCOD));
			AssertEquals("Prepaid", list.GetDescriptionFromCode(Constants.DomesticPaymentTerms.Prepaid));
			AssertEquals("Collect", list.GetDescriptionFromCode(Constants.DomesticPaymentTerms.Collect));
			AssertEquals("Collect 3rd Party", list.GetDescriptionFromCode(Constants.DomesticPaymentTerms.CollectThirdParty));
			AssertEquals("Collect COD", list.GetDescriptionFromCode(Constants.DomesticPaymentTerms.CollectCOD));
		}

		public void TestPaymentStatus()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PaymentStatus);
			AssertEquals("Count Status Types", 2, list.Count);
		}

		public void TestGetWeightType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Weight);
			Assert("Weight Fields", list.Count > 0);
		}

		public void TestGetLengthType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Length);
			Assert("Length Fields", list.Count > 0);
		}

		public void TestGetVolumeType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.Volume);
			Assert("Volume Fields", list.Count > 0);
		}

		public void TestOrderHeaderStatus()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.OrderHeaderStatus);
			AssertEquals("List.Count", 7, list.Count);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Incomplete, "Incomplete"), list[0]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Open, "Placed"), list[1]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Confirmed, "Confirmed"), list[2]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Shipped, "Shipped"), list[3]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.PartDelivered, "Part Delivered"), list[4]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Delivered, "Delivered"), list[5]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Cancelled, "Canceled"), list[6]);
		}

		public void TestOrderLineStatus()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.OrderLineStatus);
			AssertEquals("List.Count", 5, list.Count);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Open, "Placed"), list[0]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.PartDelivered, "Part Delivered"), list[1]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Delivered, "Delivered"), list[2]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Cancelled, "Canceled"), list[3]);
			AssertPairEquals(new CodeDescriptionPair(Constants.OrderStatus.Incomplete, "Incomplete"), list[4]);
		}

		public void TestOrgHeaderCategory()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);
			AssertEquals("List.Count", 4, list.Count);
		}

		public void TestPayableOrderStage()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PayableOrderStage);
			AssertEquals("List.Count", 4, list.Count);
		}

		public void TestPayableOrderDisposition()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PayableOrderDisposition);
			AssertEquals("List.Count", 9, list.Count);
		}

		public void TestPayableOrderType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PayableOrderType);
			AssertEquals("List.Count", 4, list.Count);
		}

		public void TestPayableOrderGoodsStatus()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.PayableOrderGoodsStatus);
			AssertEquals("List.Count", 6, list.Count);
		}

		public void TestAWBChargeCodes()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.AWBChargeCodes);
			AssertEquals("List.Count", 120, list.Count);

			var anyDuplicate = list.Elements.GroupBy(x => x.Code).Any(g => g.Count() > 1);
			AssertEquals("Code in the list must be unique", expected: false, anyDuplicate);

			AssertEquals("Customs/ regulatory clearance", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.JA));
			AssertEquals("Live animals related services", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.LA));
			AssertEquals("Veterinary physical/ documentary inspection", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.LG));
			AssertEquals("Pick-up service", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.PU));
			AssertEquals("Dangerous goods physical/ documentary inspection", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.RA));
			AssertEquals("Delivery service", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.SA));
			AssertEquals("Delivery service surface charge - destination", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.SD));
			AssertEquals("Shipment stopped in transit at customer request", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.SI));
			AssertEquals("Early release of shipment", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.SP));
			AssertEquals("Pick-up service surface charge - origin", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.SU));
			AssertEquals("Transit handling", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.TR));
			AssertEquals("Adjusting of improperly loaded Unit Load Device", list.GetDescriptionFromCode(Constants.AWB.ChargeCodes.UC));

			Assert(list.ContainsCode(Constants.AWB.ChargeCodes.ZD));
			Assert(list.ContainsCode(Constants.AWB.ChargeCodes.ZE));
		}

		public void TestAWBDimensions()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.AWBDimensions);
			AssertEquals("List.Count", 5, list.Count);

			var codesInList = new Hashtable(134);

			foreach (CodeDescriptionPair pair in list)
			{
				Assert("Code in the list must be unique", !codesInList.ContainsKey(pair.Code));
				codesInList.Add(pair.Code, null);
			}
		}

		public void TestAWBAsAgreedFirstSetTypes()
		{
			var pairList = new CodeDescriptionPairList(OLookUpEditType.AWBAsAgreedFirstSetType);

			AssertEquals(Constants.AWB.AsAgreedTypes.Descriptions.All, pairList.GetDescriptionFromCode(Constants.AWB.AsAgreedTypes.Codes.All));
			AssertEquals(Constants.AWB.AsAgreedTypes.Descriptions.Collect, pairList.GetDescriptionFromCode(Constants.AWB.AsAgreedTypes.Codes.Collect));
			AssertEquals(Constants.AWB.AsAgreedTypes.Descriptions.None, pairList.GetDescriptionFromCode(Constants.AWB.AsAgreedTypes.Codes.None));

			AssertEquals(3, pairList.Count);
		}

		public void TestAWBAsAgreedSecondSetTypes()
		{
			var pairList = new CodeDescriptionPairList(OLookUpEditType.AWBAsAgreedSecondSetType);

			AssertEquals(Constants.AWB.AsAgreedTypes.Descriptions.All, pairList.GetDescriptionFromCode(Constants.AWB.AsAgreedTypes.Codes.All));
			AssertEquals(Constants.AWB.AsAgreedTypes.Descriptions.Prepaid, pairList.GetDescriptionFromCode(Constants.AWB.AsAgreedTypes.Codes.Prepaid));
			AssertEquals(Constants.AWB.AsAgreedTypes.Descriptions.None, pairList.GetDescriptionFromCode(Constants.AWB.AsAgreedTypes.Codes.None));

			AssertEquals(3, pairList.Count);
		}

		public void TestAirWaybillPaperTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.AirWaybillPaperTypes);

			Assert("Contains Iata Code", list.ContainsCode(Constants.AWB.PaperTypes.Iata));
			Assert("Contains Iata Old Code", list.ContainsCode(Constants.AWB.PaperTypes.IataOld));
			Assert("Contains Traxon", list.ContainsCode(Constants.AWB.PaperTypes.Traxon));
			Assert("Contains Letter", list.ContainsCode(Constants.AWB.PaperTypes.Letter));

			AssertEquals("List.Count", 4, list.Count);
		}

		public void TestFreightContainerMode()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode);
			AssertEquals("FreightContainerMode list count.", 16, list.Count);
		}

		public void TestTransportType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.TransportType);
			AssertEquals("TransportType list count.", 7, list.Count);
		}

		public void TestShipmentType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ShipmentType);
			AssertEquals("ShipmentType list count.", 9, list.Count);
		}

		public void TestContainerStorageClass()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ContainerStorageClass);
			AssertEquals("ContainerStorageClass list count", 8, list.Count);
		}

		public void TestShipmentScreen()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.ShipmentScreenLayout);
			Assert("ShipmentScreenLayout list count", list.Count > 0);
		}

		public void TestOSMGSecurityLevel()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.OSMGSecurityLevel);
			AssertEquals("List should have 2 elements", 2, list.Count);
			AssertEquals($"List should contain '{Constants.OSMGSecurityLevels.Standard}'", Constants.OSMGSecurityLevels.Standard, list.GetCodeFromDescription("Standard"));
			AssertEquals($"List should contain '{Constants.OSMGSecurityLevels.Enhanced}'", Constants.OSMGSecurityLevels.Enhanced, list.GetCodeFromDescription("Enhanced"));
		}

		public void TestStaffCertificateType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.StaffCertificateType);
			AssertEquals("Count", 9, list.Count);
		}

		public void TestPrintCopyType()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.PrintCopyType);

			AssertEquals("Count", 4, list.Count);
			AssertEquals("List[0].Code", "PRN", list[0].Code);
			AssertEquals("List[0].Code", "Print", list[0].Description);
			AssertEquals("List[1].Code", "FAX", list[1].Code);
			AssertEquals("List[1].Code", "Fax", list[1].Description);
			AssertEquals("List[2].Code", "EML", list[2].Code);
			AssertEquals("List[2].Code", "E-Mail", list[2].Description);
			AssertEquals("List[3].Code", "ALL", list[3].Code);
			AssertEquals("List[3].Code", "ALL", list[3].Description);
		}

		public void TestAPPaymentMethod()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, true))
			{
				var listWhenBothRegistriesEnabled = new CodeDescriptionPairList(OLookUpEditType.APPaymentMethod);
				AssertEquals("Count should be 4", 4, listWhenBothRegistriesEnabled.Count);
				AssertEquals("List should contain CHQ", ReceiptTypes.Cheque, listWhenBothRegistriesEnabled.GetCodeFromDescription("Check"));
				AssertEquals("List should contain DDR", ReceiptTypes.DirectDebit, listWhenBothRegistriesEnabled.GetCodeFromDescription("Direct Debit"));
				AssertEquals("List should contain END", ReceiptTypes.eNettDirectDebit, listWhenBothRegistriesEnabled.GetCodeFromDescription("ComPay Direct Debit"));
				AssertEquals("List should contain EPO", EPaymentMethods.EPaymentViaOFX, listWhenBothRegistriesEnabled.GetCodeFromDescription("E-Payment via OFX"));
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(EnvProxy.Instance.CurrentCompany.PK, false))
			{
				var listWhenOnlyOneRegistryIsEnabled = new CodeDescriptionPairList(OLookUpEditType.APPaymentMethod);
				AssertEquals("Count should be 3", 3, listWhenOnlyOneRegistryIsEnabled.Count);
				AssertEquals("List should contain CHQ", ReceiptTypes.Cheque, listWhenOnlyOneRegistryIsEnabled.GetCodeFromDescription("Check"));
				AssertEquals("List should contain DDR", ReceiptTypes.DirectDebit, listWhenOnlyOneRegistryIsEnabled.GetCodeFromDescription("Direct Debit"));
				AssertEquals("List should contain END", ReceiptTypes.eNettDirectDebit, listWhenOnlyOneRegistryIsEnabled.GetCodeFromDescription("ComPay Direct Debit"));
			}
		}

		public void TestAutoPrintTypes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.AutoPrintTypes);

			AssertEquals("Count should be 2", 2, list.Count);
			AssertEquals("List should contain AUT", Constants.AutoPrintTypes.AutoPrint, list.GetCodeFromDescription("Auto-Print Check Books"));
			AssertEquals("List should contain MAN", Constants.AutoPrintTypes.Manual, list.GetCodeFromDescription("Manual Check Books"));
		}

		public void TestEquipmentGroup()
		{
			CodeDescriptionPairList equipmentGroupPairList = new CodeDescriptionPairList();
			equipmentGroupPairList.AddPair("TST", "TEST");
			EnvProxy.Instance.Registry.ReferenceFiles.EquipmentGroup = equipmentGroupPairList;
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.EquipmentGroup);
			AssertEquals(1, list.Count);
			AssertEquals(true, list.ContainsCode("TST"));
		}

		public void TestJobDocumentPeriods()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.JobRequiredDocumentPeriods);
			AssertEquals("Count", 2, list.Count);
			AssertEquals("list[0].Code", Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment, list[0].Code);
			AssertEquals("list[0].Description", Constants.JobRequiredDocuments.DocumentPeriodDescriptions.OncePerShipment, list[0].Description);
			AssertEquals("list[1].Code", Constants.JobRequiredDocuments.DocumentPeriods.Periodic, list[1].Code);
			AssertEquals("list[1].Description", Constants.JobRequiredDocuments.DocumentPeriodDescriptions.Periodic, list[1].Description);
		}

		public void TestLocalCartageTransportModes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.LocalCartageTransportModes);
			AssertEquals(4, list.Count);
			AssertEquals(true, list.ContainsCode(Constants.TransportModes.Air));
			AssertEquals(true, list.ContainsCode(Constants.TransportModes.Sea));
		}

		public void TestLocalCartageContainerModes()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.LocalCartageContainerModes);
			AssertEquals(4, list.Count);
			AssertEquals(true, list.ContainsCode(Constants.ContainerModes.BreakBulk));
			AssertEquals(true, list.ContainsCode(Constants.ContainerModes.Loose));
		}

		public void TestAllocationMethod()
		{
			var allocationMethodList = new CodeDescriptionPairList(OLookUpEditType.AllocationMethod);

			AssertEquals(11, allocationMethodList.Count);
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.ChargeableUnits));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.Manual));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.Shipment));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.Revenue));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.GrossWeight));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.ContainerCount));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.OuterPackTotal));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.TwentyFootEquivalentUnit));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.CapacityPerContainer));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.FreeSpaceContribution));
			AssertEquals(true, allocationMethodList.ContainsCode(AllocationMethod.GrossVolume));
		}

		public void TestGLLanguages()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.GLLanguage);
			var standardLanguageList = new CodeDescriptionPairList(OLookUpEditType.Language);
			AssertEquals("standard language list + ZZZ", standardLanguageList.Count + 1, list.Count);
			foreach (CodeDescriptionPair pair in standardLanguageList)
			{
				AssertEquals(pair.Description, list.GetDescriptionFromCode(pair.Code));
			}
			AssertEquals("External Link to General Ledger", list.GetDescriptionFromCode("ZZZ"));
			Assert("Standard Language List should not contain 'ZZZ'", !standardLanguageList.ContainsCode("ZZZ"));
		}

		public void TestLanguagesDBAccess()
		{
			new CodeDescriptionPairList(OLookUpEditType.Language); // Populate the cache
			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				var initialFactoryCount = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length;
				for (int i = 0; i < 100; i++)
				{
					new CodeDescriptionPairList(OLookUpEditType.Language);
				}

				AssertEquals(initialFactoryCount, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Length);
			}
		}

		public void TestLocalCustomLanguages()
		{
			var factory = new BusinessObjectFactory();
			var testLanguage1 = factory.New<IRefLocalLanguage>();
			testLanguage1.RA_Code = "CC";
			testLanguage1.RA_RN_NKCountryCode = "CN";
			testLanguage1.RA_Description = "My Test Language 1";

			factory.Save();

			var list = new CodeDescriptionPairList(OLookUpEditType.Language);
			AssertEquals("Local custom language should be added to the language list", testLanguage1.FullLanguageCode, list.GetCodeFromDescription(testLanguage1.RA_Description));
		}

		public void TestCustomLabelCount()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.CustomLabels);
			AssertEquals(201, list.Count);
		}

		public void TestAWBNatureAndQtyOfGoodsType()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.AWBNatureAndQtyOfGoodsType);
			AssertEquals(9, list.Count);
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.GoodsDescription));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.Consolidation));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.Dimensions));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.Volume));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.ULDNumber));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.ShippersLoadAndCount));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.HarmonisedCommodityCode));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.CountryOfGoodsOrigin));
			Assert(list.ContainsCode(Constants.AWB.NatureAndQtyOfGoodsTypes.LithiumBattery));
		}

		public void TestAWBLithiumBatteryType()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.AWBLithiumBatteryType);
			AssertEquals(7, list.Count);
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.PI965));
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.PI966));
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.PI967));
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.PI968));
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.PI969));
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.PI970));
			Assert(list.ContainsCode(Constants.AWB.LithiumBatteryTypes.Codes.LMB));
		}

		public void TestAutoratingMode()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.FreightRateAutoratingMode);
			AssertEquals(3, list.Count);
			Assert(list.ContainsCode(Constants.FreightRateAutoratingModes.Code.StandardRate));
			Assert(list.ContainsCode(Constants.FreightRateAutoratingModes.Code.FreightPlusRate));
			Assert(list.ContainsCode(Constants.FreightRateAutoratingModes.Code.AllInRate));
		}

		public void TestChequeTransactionTypes()
		{
			var pairList = new CodeDescriptionPairList(OLookUpEditType.ChequeTransactionHeader);

			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeEntryTransaction));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeOutToCreditor));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeOutToBankForCollection));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeOutToBankAsGuarantee));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeCollectedAtBank));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.BadChequeAtBank));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeReturnFromBank));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeReturnToTheDebtor));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeCollectedInPortfolio));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.BadChequeInPortfolio));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.CollectionOfEndorsedCheque));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeReturnFromCreditor));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ReturnedBadChequeFromCreditor));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.UncollectibleCheques));
			Assert(pairList.ContainsCode(ChequeTransactionTypes.ChequeInJudicialProcess));

			AssertEquals(15, pairList.Count);
		}

		#endregion

		#region TestAddVolumePairs

		public void TestAddVolumePairs()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.Volume);

			AssertEquals(11, list.Count);
			Assert(list.ContainsCode(Constants.Volume.CubicDecimetres));
			Assert(list.ContainsCode(Constants.Volume.CubicFeet));
			Assert(list.ContainsCode(Constants.Volume.CubicInches));
			Assert(list.ContainsCode(Constants.Volume.CubicMetres));
			Assert(list.ContainsCode(Constants.Volume.CubicYards));
			Assert(list.ContainsCode(Constants.Volume.Litre));
			Assert(list.ContainsCode(Constants.Volume.MegaLitre));
			Assert(list.ContainsCode(Constants.Volume.TeaChest));
			Assert(list.ContainsCode(Constants.Volume.CubicCentimeters));
			Assert(list.ContainsCode(Constants.Volume.USGallons));
			Assert(list.ContainsCode(Constants.Volume.ImperialGallons));
		}

		#endregion

		#region TestMAWBBillingSellRate

		public void TestMAWBBillingSellRate()
		{
			var actualList = new CodeDescriptionPairList(OLookUpEditType.MAWBBillingSellRateModes);

			AssertEquals("MAWBBillingSellRateModes has 4 elements", 4, actualList.Count);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("NON", "No Sell Rate to populate");
			expectedList.AddPair("COL", "Sell Rates to populate for Collect direct consols");
			expectedList.AddPair("PPD", "Sell Rates to populate for Prepaid direct consols");
			expectedList.AddPair("BTH", "Sell Rates to populate for both prepaid and collect direct consols");

			AssertContainsExactElementsInExactOrder(expectedList, actualList);
		}

		#endregion

		#region TestSupplierBookingLineStatusPairs
		public void TestSupplierBookingLineStatusPairs()
		{
			var list = new CodeDescriptionPairList(OLookUpEditType.SupplierBookingLineStatus);
			AssertEquals(8, list.Count);

			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.Incomplete));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.AcceptedAtOriginDepot));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.ArrivedAtDestination));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.AwaitingLocalDelivery));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.Confirmed));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.CustomsClearedAtDestination));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.Delivered));
			Assert(list.ContainsCode(Constants.SupplierBookingLineStatus.Codes.DepartedFromOriginDepot));
		}
		#endregion

		#region ProductivityWise

		public void TestFilterReferenceTypes_WhenProductivityWiseModeEnabled()
		{
			string[] expectedCodes_PWDisabled = new string[]
			{
				Constants.ReferenceTypes.All,
				Constants.ReferenceTypes.ClientSupplierRelationship,
				Constants.ReferenceTypes.Accounting,
				Constants.ReferenceTypes.SupplyChainLogistics,
				Constants.ReferenceTypes.GeneralReferenceTables,
				Constants.ReferenceTypes.HumanResourcesStaffEmployment,
				Constants.ReferenceTypes.BusinessEntityProcessWorkflow,
				Constants.ReferenceTypes.ComplianceReport
			};

			string[] expectedCodes_PWEnabled = new string[]
			{
				Constants.ReferenceTypes.All,
				Constants.ReferenceTypes.Accounting,
				Constants.ReferenceTypes.GeneralReferenceTables,
				Constants.ReferenceTypes.HumanResourcesStaffEmployment,
				Constants.ReferenceTypes.BusinessEntityProcessWorkflow,
				Constants.ReferenceTypes.ComplianceReport
			};

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			var prePWActualCodesArray = new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes).GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder("Our CDP List should contain all doc reference types, but instead...", expectedCodes_PWDisabled, prePWActualCodesArray);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var postPWActualCodesArray = new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes).GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder("Our CDP List should contain all non-Freight doc reference types, but instead...", expectedCodes_PWEnabled, postPWActualCodesArray);
		}

		public void TestFilterReferenceTypesMatchTypesOfRefDocTypesInDb()
		{
			var expectedCodes = new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes).GetAllCodes();
			var notInClause = new StringBuilder();
			var queryParams = new ZSqlParameterCollection();

			for (var index = 0; index < expectedCodes.Length; index++)
			{
				var paraName = "@type" + index;
				notInClause.Append(paraName);
				queryParams.Add(paraName, expectedCodes[index], RefDocTypeSchema.RT_ReferenceType);

				if (index != expectedCodes.Length - 1)
				{
					notInClause.Append(", ");
				}
			}

			var typesInDb = new List<string>();

			using (var cmd = Db.Connection.Command($"SELECT DISTINCT RT_ReferenceType FROM dbo.RefDocType WHERE RT_ReferenceType NOT IN ({notInClause})")) // OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
			{
				cmd.AddParameters(queryParams);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						typesInDb.Add(reader.GetString(0));
					}
				}
			}

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			AssertEquals($"Some RT_ReferenceType values are not added into the CDP list but are detected in the database: {string.Join(", ", typesInDb)}", 0, typesInDb.Count);
		}

		#endregion

		#region Test GL Account Type

		public void TestGLAccountType()
		{
			var expectedCodes = new string[]
			{
				AccountTypeComboBoxConstants.BalanceSheetAccount,
				AccountTypeComboBoxConstants.ProfitAndLossAccount,
				AccountTypeComboBoxConstants.Total,
				AccountTypeComboBoxConstants.Header,
				AccountTypeComboBoxConstants.OpeningBalance,
				AccountTypeComboBoxConstants.ClosingBalance,
				AccountTypeComboBoxConstants.Consolidation,
				AccountTypeComboBoxConstants.Alternate,
				AccountTypeComboBoxConstants.Note
			};

			var glAccountTypeActualCodesArray = new CodeDescriptionPairList(OLookUpEditType.GLAccountType).GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes, glAccountTypeActualCodesArray);
		}

		public void TestGLAccountDescriptorType()
		{
			var expectedCodes = new string[]
			{
				AccountTypeComboBoxConstants.BalanceSheetAccount,
				AccountTypeComboBoxConstants.ProfitAndLossAccount,
				AccountTypeComboBoxConstants.Total,
				AccountTypeComboBoxConstants.Header,
				AccountTypeComboBoxConstants.Consolidation,
				AccountTypeComboBoxConstants.Alternate,
				AccountTypeComboBoxConstants.CarriedForwardAccount,
				AccountTypeComboBoxConstants.Note
			};

			var glAccountDescriptorTypeActualCodesArray = new CodeDescriptionPairList(OLookUpEditType.GLAccountDescriptorType).GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder(expectedCodes, glAccountDescriptorTypeActualCodesArray);
		}

		public void TestGLJournalTypes()
		{
			var expectedGLJournalTypeCodesArray = new CodeDescriptionPairList(OLookUpEditType.GLJournalTypes).GetAllCodes().ToArray();
			AssertContainsExactElementsInAnyOrder(new string[] { TransactionTypes.GLAutoJournal, TransactionTypes.GLReversingJournal, TransactionTypes.GLStandardJournal, TransactionTypes.GLNoteJournal }, expectedGLJournalTypeCodesArray);
		}

		#endregion

		#region Implementation

		protected override Type CodeDescriptionPairListType
		{
			get { return typeof(CodeDescriptionPairList); }
		}

		protected new CodeDescriptionPairList List
		{
			get { return (CodeDescriptionPairList)base.List; }
		}

		#endregion
	}
}
