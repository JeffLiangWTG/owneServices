using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class MultipleValueCriteriaTest : TestCase
	{
		public void TestSplitMyself()
		{
			var originalCriteria = new MultipleValueCriteria() { TableName = "DummyBizo", ColumnName = "Z0_PK", Values = (new object[] { 1, 2, 3 }) };

			var splittedCriterias = originalCriteria.SplitMyself(1);
			AssertSplittedCriteriasSameAsExpectedCriterias(splittedCriterias, new MultipleValueCriteria[] {
				new MultipleValueCriteria() { TableName = "DummyBizo", ColumnName = "Z0_PK", Values = (new object[] { 1 }) },
				new MultipleValueCriteria() { TableName = "DummyBizo", ColumnName = "Z0_PK", Values = (new object[] { 2 }) },
				new MultipleValueCriteria() { TableName = "DummyBizo", ColumnName = "Z0_PK", Values = (new object[] { 3 }) }
			});

			splittedCriterias = originalCriteria.SplitMyself(2);
			AssertSplittedCriteriasSameAsExpectedCriterias(splittedCriterias, new MultipleValueCriteria[] {
				new MultipleValueCriteria() { TableName = "DummyBizo", ColumnName = "Z0_PK", Values = (new object[] { 1, 2 }) },
				new MultipleValueCriteria() { TableName = "DummyBizo", ColumnName = "Z0_PK", Values = (new object[] { 3 }) }
			});

			splittedCriterias = originalCriteria.SplitMyself(3);
			AssertSplittedCriteriasContainOnlyOriginalCriteria(splittedCriterias, originalCriteria);

			splittedCriterias = originalCriteria.SplitMyself(4);
			AssertSplittedCriteriasContainOnlyOriginalCriteria(splittedCriterias, originalCriteria);

			splittedCriterias = originalCriteria.SplitMyself(0);
			AssertSplittedCriteriasContainOnlyOriginalCriteria(splittedCriterias, originalCriteria);

			splittedCriterias = originalCriteria.SplitMyself(-1);
			AssertSplittedCriteriasContainOnlyOriginalCriteria(splittedCriterias, originalCriteria);
		}

		void AssertSplittedCriteriasContainOnlyOriginalCriteria(IEnumerable<MultipleValueCriteria> splittedCriterias, MultipleValueCriteria originalCriteria)
		{
			CombineAssertions(() =>
			{
				Assert(splittedCriterias.Count() == 1);
				AssertEquals(originalCriteria, splittedCriterias.ToArray()[0]);
			}
			);
		}

		void AssertSplittedCriteriasSameAsExpectedCriterias(IEnumerable<MultipleValueCriteria> splittedCriterias, params MultipleValueCriteria[] expectedCriterias)
		{
			var splittedCriteriaArray = splittedCriterias.ToArray();

			CombineAssertions(() =>
			{
				Assert("splittedCriteriaArray.Length == expectedCriteria.Length", splittedCriteriaArray.Length == expectedCriterias.Length);

				for (int i = 0; i < splittedCriteriaArray.Length; ++i)
				{
					var splittedCriteria = splittedCriteriaArray[i];
					var expectedCriteria = expectedCriterias[i];
					AssertEquals(expectedCriteria.TableName, splittedCriteria.TableName);
					AssertEquals(expectedCriteria.ColumnName, splittedCriteria.ColumnName);

					var expectedCriteriaValuesArray = expectedCriteria.Values.ToArray();
					var splittedCriteriaValuesArray = splittedCriteria.Values.ToArray();
					AssertEquals(expectedCriteriaValuesArray.Length, splittedCriteriaValuesArray.Length);

					for (int j = 0; j < splittedCriteriaValuesArray.Length; ++j)
					{
						AssertEquals(expectedCriteriaValuesArray[j], splittedCriteriaValuesArray[j]);
					}
				}
			}
			);
		}
	}
}
