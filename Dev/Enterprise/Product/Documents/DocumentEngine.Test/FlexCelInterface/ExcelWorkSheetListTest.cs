using System;
using System.Collections;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.FlexCelInterface.Testing
{
	sealed class ExcelWorkSheetListTest : TestCase
	{
		public void TestFind()
		{
			using (var excelInterface = new ExcelInterface())
			{
				var documentWorkSheet = new ExcelWorkSheet(excelInterface, "Document");

				var workSheets = new ExcelWorkSheetList() { documentWorkSheet };

				Action<ExcelWorkSheet, string> assertFind = (expected, workSheetName) =>
				{
					var message = string.Format("Could not find [{0}] among the worksheets.", workSheetName);
					AssertEquals(message, expected, workSheets.Find(workSheetName));
				};

				assertFind(documentWorkSheet, "Document");
				assertFind(null, "Translations");
				assertFind(null, "Other");

				var otherWorkSheet = new ExcelWorkSheet(excelInterface, "Other");

				workSheets.Add(otherWorkSheet);

				assertFind(documentWorkSheet, "Document");
				assertFind(otherWorkSheet, "Other");

				assertFind(documentWorkSheet, "Document");
				assertFind(null, "Translations");
				assertFind(otherWorkSheet, "Other");
			}
		}

		[ExpectNoExceptions()]
		public void TestEmptyConstructor()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			AssertNotNull(testExcelWorkSheetList);
		}

		public void TestConstructorWithExcelWorkSheetList()
		{
			ExcelWorkSheetList testExcelWorkSheetList1 = new ExcelWorkSheetList();
			testExcelWorkSheetList1.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList1.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList1.Add(GetNewExcelWorkSheet());

			ExcelWorkSheetList testExcelWorkSheetList2 = new ExcelWorkSheetList(testExcelWorkSheetList1);
			AssertEquals(3, testExcelWorkSheetList2.Count);
		}

		public void TestConstructorWithArray()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			AssertEquals(2, testExcelWorkSheetList.Count);
		}

		public void TestIndexerGet()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			AssertNotNull(testExcelWorkSheetList[0]);
		}

		public void TestIndexerSet()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			ExcelWorkSheet testExcelWorkSheet = GetNewExcelWorkSheet();
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList[0] = testExcelWorkSheet;
			AssertSame(testExcelWorkSheet, testExcelWorkSheetList[0]);
		}

		public void TestAdd()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			AssertEquals(0, testExcelWorkSheetList.Count);
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			AssertEquals(3, testExcelWorkSheetList.Count);
		}

		public void TestAddRangeExcelWorkSheetList()
		{
			ExcelWorkSheetList testExcelWorkSheetList1 = new ExcelWorkSheetList();
			testExcelWorkSheetList1.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList1.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList1.Add(GetNewExcelWorkSheet());

			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			AssertEquals(0, testExcelWorkSheetList.Count);
			testExcelWorkSheetList.AddRange(testExcelWorkSheetList1);
			AssertEquals(3, testExcelWorkSheetList.Count);
		}

		public void TestAddRangeArray()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			AssertEquals(0, testExcelWorkSheetList.Count);
			testExcelWorkSheetList.AddRange(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet(), GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			AssertEquals(4, testExcelWorkSheetList.Count);
		}

		public void TestContaines()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			ExcelWorkSheet testExcelWorkSheet = GetNewExcelWorkSheet();
			testExcelWorkSheetList.Add(testExcelWorkSheet);
			Assert(testExcelWorkSheetList.Contains(testExcelWorkSheet));
		}

		public void TestCopyTo()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			ExcelWorkSheet[] excelWorkSheetList = new ExcelWorkSheet[3] { null, null, null };
			testExcelWorkSheetList.CopyTo(excelWorkSheetList, 1);
			AssertNull(excelWorkSheetList[0]);
			AssertNotNull(excelWorkSheetList[1]);
			AssertNotNull(excelWorkSheetList[2]);
		}

		public void TestToArray()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			ExcelWorkSheet[] excelWorkSheetList = testExcelWorkSheetList.ToArray();
			AssertEquals(2, excelWorkSheetList.Length);
		}

		public void TestIndexOf()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			ExcelWorkSheet testExcelWorkSheet = GetNewExcelWorkSheet();
			testExcelWorkSheetList.Add(testExcelWorkSheet);
			AssertEquals(2, testExcelWorkSheetList.IndexOf(testExcelWorkSheet));
		}

		public void TestInsert()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			ExcelWorkSheet testExcelWorkSheet = GetNewExcelWorkSheet();
			testExcelWorkSheetList.Insert(1, testExcelWorkSheet);
			AssertEquals(1, testExcelWorkSheetList.IndexOf(testExcelWorkSheet));
		}

		public void TestRemove()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			ExcelWorkSheet testExcelWorkSheet = GetNewExcelWorkSheet();
			testExcelWorkSheetList.Add(testExcelWorkSheet);
			Assert(testExcelWorkSheetList.Contains(testExcelWorkSheet));
			testExcelWorkSheetList.Remove(testExcelWorkSheet);
			Assert(!testExcelWorkSheetList.Contains(testExcelWorkSheet));
		}

		public void TestCount()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList();
			AssertEquals(0, testExcelWorkSheetList.Count);
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			testExcelWorkSheetList.Add(GetNewExcelWorkSheet());
			AssertEquals(3, testExcelWorkSheetList.Count);
		}

		public void TestGetEnumerator()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			AssertNotNull(testExcelWorkSheetList.GetEnumerator());
		}

		public void TestCurrent()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			IEnumerator @enum = testExcelWorkSheetList.GetEnumerator();
			@enum.MoveNext();
			AssertNotNull(@enum.Current);
		}

		public void TestMoveNext()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			IEnumerator @enum = testExcelWorkSheetList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
		}

		public void TestReset()
		{
			ExcelWorkSheetList testExcelWorkSheetList = new ExcelWorkSheetList(new ExcelWorkSheet[] { GetNewExcelWorkSheet(), GetNewExcelWorkSheet() });
			IEnumerator @enum = testExcelWorkSheetList.GetEnumerator();
			Assert(@enum.MoveNext());
			Assert(@enum.MoveNext());
			Assert(!@enum.MoveNext());
			@enum.Reset();
			Assert(@enum.MoveNext());
		}

		#region Implementation

		ExcelWorkSheet GetNewExcelWorkSheet()
		{
			return new ExcelWorkSheet(null, "");
		}

		#endregion
	}
}
