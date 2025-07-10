using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class ValueObjectTestCoverageHelperTest : TestCaseWithFactory
	{
		public void TestCoverage()
		{
			TestValueObjectsForCoverageTest.TestValueObject value = new TestValueObjectsForCoverageTest.TestValueObject();
			value.ParentRelation.IsSpecified = false;
			value.ChildRelation.IsSpecified = false;
			value.IntValueSpecified = false;

			ValueObjectTestCoverageHelper helper = new ValueObjectTestCoverageHelper(typeof(TestValueObjectsForCoverageTest.TestValueObject));

			//CoveredNothing
			string[] coveredNothing = new string[] {
				"DateValue",
				"ChildRelation/DateValue",
				"ChildRelation/StrValue",
				"ParentRelation/DateValue",
				"ParentRelation/StrValue",
				"IntValue"
			};

			AssertPathsNotCovered("Covered nothing", coveredNothing, helper.UncoveredPaths);

			//Covered a completely empty object
			helper.NotifyCovered(value);
			string[] completelyEmptyPath = new string[] {
				"DateValue",
				"ChildRelation/DateValue",
				"ChildRelation/StrValue",
				"ParentRelation/DateValue",
				"ParentRelation/StrValue",
				"IntValue"
			};

			AssertPathsNotCovered("Covered a completely empty object", completelyEmptyPath, helper.UncoveredPaths);

			//Constructed a parent relation, but still covered nothing
			value.ParentRelation = new TestValueObjectsForCoverageTest.TestRelatedValueObject();
			helper.NotifyCovered(value);
			string[] emptyWithParentRelation = new string[] {
				"DateValue",
				"ChildRelation/DateValue",
				"ChildRelation/StrValue",
				"ParentRelation/DateValue",
				"ParentRelation/StrValue",
				"IntValue"
			};

			AssertPathsNotCovered("Constructed a parent relation, but still covered nothing", emptyWithParentRelation, helper.UncoveredPaths);

			//Covered a parent relation
			value.ParentRelation.DateValue = new DateTime(2003, 1, 1);
			value.ParentRelation.StrValue = "splaty";
			helper.NotifyCovered(value);
			string[] coveredParentRelation = new string[] {
				"DateValue",
				"ChildRelation/DateValue",
				"ChildRelation/StrValue",
				"IntValue"
			};

			AssertPathsNotCovered("Covered a parent relation", coveredParentRelation, helper.UncoveredPaths);

			//Constructed a child relation but not populated it yet
			value.ChildRelation = new TestValueObjectsForCoverageTest.TestRelatedValueObjectCollection();
			helper.NotifyCovered(value);
			string[] unpopulatedChildRelation = new string[] {
				"DateValue",
				"ChildRelation/DateValue",
				"ChildRelation/StrValue",
				"IntValue"
			};

			AssertPathsNotCovered("Constructed a child relation but not populated it yet", unpopulatedChildRelation, helper.UncoveredPaths);

			//Covered part of a child relation
			value.ChildRelation.Add(new TestValueObjectsForCoverageTest.TestRelatedValueObject());
			value.ChildRelation[0].DateValue = new DateTime(2004, 1, 1);
			helper.NotifyCovered(value);
			string[] coveredPartOfChildRelation = new string[] {
				"DateValue",
				"ChildRelation/StrValue",
				"IntValue"
			};

			AssertPathsNotCovered("Covered part of a child relation", coveredPartOfChildRelation, helper.UncoveredPaths);

			//Covered the rest of the child relation, in a different list item
			value.ChildRelation.Add(new TestValueObjectsForCoverageTest.TestRelatedValueObject());
			value.ChildRelation[1].StrValue = "blah";
			helper.NotifyCovered(value);
			string[] coveredPartOfChildRelationInDifferentItem = new string[] {
				"DateValue",
				"IntValue"
			};

			AssertPathsNotCovered("Covered the rest of the child relation, in a different list item", coveredPartOfChildRelationInDifferentItem, helper.UncoveredPaths);

			value.DateValue = new DateTime(2002, 1, 1);
			value.IntValueSpecified = true;
			helper.NotifyCovered(value);
			string[] dateTimeOffsetValuePaths = new string[] {
				"DateTimeOffsetValue",
				"ParentRelation/DateTimeOffsetValue",
				"ChildRelation/DateTimeOffsetValue"
			};
			AssertPathsNotCovered("Completely covered the value object (except for DateTimeOffsetValue paths)", dateTimeOffsetValuePaths, helper.UncoveredPaths);

			value.ChildRelation.Add(new TestValueObjectsForCoverageTest.TestRelatedValueObject());
			value.ChildRelation[2].DateTimeOffsetValue = new ZDateTimeOffset(2003, 1, 1, 2, 3, 4, 123, TimeSpan.FromHours(8));
			value.DateTimeOffsetValue = new ZDateTimeOffset(2003, 1, 1, 2, 3, 4, 123, TimeSpan.FromHours(8));
			value.DateTimeOffsetValueSpecified = true;
			value.ParentRelation.DateTimeOffsetValue = new ZDateTimeOffset(2003, 1, 1, 2, 3, 4, 123, TimeSpan.FromHours(8));
			helper.NotifyCovered(value);

			AssertEquals("Completely covered the value object", 0, helper.UncoveredPaths.Length);
		}

		void AssertPathsNotCovered(string message, string[] expectedPaths, IEnumerable<string> actualPaths)
		{
			ZStringBuilder pathsNotCovered = new ZStringBuilder();

			foreach (string expectedPath in expectedPaths)
			{
				bool found = false;

				foreach (string actualPath in actualPaths)
				{
					if (actualPath == expectedPath)
					{
						found = true;
					}
				}

				if (!found)
				{
					pathsNotCovered.Append(expectedPath + "\r\n");
				}
			}

			if (!string.IsNullOrEmpty(pathsNotCovered.ToString()))
			{
				Fail(message + ". The Following Paths were covered found: \r\n" + pathsNotCovered.ToString());
			}
		}
	}
}
