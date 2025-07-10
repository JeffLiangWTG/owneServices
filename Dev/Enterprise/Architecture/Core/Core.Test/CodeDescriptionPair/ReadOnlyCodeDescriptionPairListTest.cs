using System;
using System.Collections;
using System.Linq;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class ReadOnlyCodeDescriptionPairListTest : TransactionedTestCase
	{
		public void TestGetHumanReadableListOfElements()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("ABC", "ABC Description");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("XYZ", "XYZ Descroption");

			List.Elements.Add(pair1);
			List.Elements.Add(pair2);

			AssertEquals("list.GetHumanReadableListOfElements()", ExpectedHumanReadableListOfElements.Trim(), List.GetHumanReadableListOfElements());
			AssertEquals("list.GetHumanReadableListOfElements(', ')", ExpectedHumanReadableListOfElements.Trim().Replace(System.Environment.NewLine, ", "), List.GetHumanReadableListOfElements(", "));
		}
		const string ExpectedHumanReadableListOfElements = @"
ABC - ABC Description
XYZ - XYZ Descroption
";
		public void TestCodeMaxLength()
		{
			AssertEquals("The max. code length was reported as " + List.MaxCodeLength + ", when it's actually 0", 0, List.MaxCodeLength);

			List.Elements.Add(new CodeDescriptionPair("A", "First item"));
			List.Elements.Add(new CodeDescriptionPair("AB", "Item two"));
			List.Elements.Add(new CodeDescriptionPair("XYZ", "Item three"));
			AssertEquals("The max. code length was reported as " + List.MaxCodeLength + ", when it's actually 3", 3, List.MaxCodeLength);
		}

		public void TestCodesAsString()
		{
			List.Elements.Add(new CodeDescriptionPair("Code1", "Desc1"));
			List.Elements.Add(new CodeDescriptionPair("Code2", "Desc2"));
			List.Elements.Add(new CodeDescriptionPair("Code3", "Desc3"));
			AssertEquals("Code1, Code2, Code3", List.CodesAsString);
		}

		public void TestElementsAsString()
		{
			List.Elements.Add(new CodeDescriptionPair("Code1", "Desc1"));
			List.Elements.Add(new CodeDescriptionPair("Code2", "Desc2"));
			List.Elements.Add(new CodeDescriptionPair("Code3", "Desc3"));

			const string expectedString =
				"Code1 - Desc1\r\n" +
				"Code2 - Desc2\r\n" +
				"Code3 - Desc3" +
				"";

			AssertEquals(expectedString, List.ElementsAsString);
		}

		public void TestCloneFromConstructorUsingCodeDescriptionPair()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("AAA", "A Description");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("BBB", "B Description");
			CodeDescriptionPair pair3 = new CodeDescriptionPair("CCC", "C Description");

			List.Elements.Add(pair1);
			List.Elements.Add(pair2);
			List.Elements.Add(pair3);

			ReadOnlyCodeDescriptionPairList clone = new ReadOnlyCodeDescriptionPairList(List);

			AssertEquals("Count", 3, clone.Count);

			AssertEquals("Clone[0].GetType()", typeof(CodeDescriptionPair), clone[0].GetType());
			AssertEquals("Clone[1].GetType()", typeof(CodeDescriptionPair), clone[1].GetType());
			AssertEquals("Clone[2].GetType()", typeof(CodeDescriptionPair), clone[2].GetType());

			AssertEquals("GetDescriptionFromCode(\"AAA\")", "A Description", clone.GetDescriptionFromCode("AAA"));
			AssertEquals("GetDescriptionFromCode(\"BBB\")", "B Description", clone.GetDescriptionFromCode("BBB"));
			AssertEquals("GetDescriptionFromCode(\"CCC\")", "C Description", clone.GetDescriptionFromCode("CCC"));

			Assert("Cloned list should not be the same instance the original list.", clone != List);
			Assert("Cloned list's elements should not be the same instances as the elements in the original list.", clone[0] != pair1);
			Assert("Cloned list's elements should not be the same instances as the elements in the original list.", clone[1] != pair2);
			Assert("Cloned list's elements should not be the same instances as the elements in the original list.", clone[2] != pair3);
		}

		public void TestCloneFromConstructorUsingCodeElement()
		{
			CodeElement element1 = new CodeElement(Guid.NewGuid(), "AAA", "A Description");
			CodeElement element2 = new CodeElement(Guid.NewGuid(), "BBB", "B Description");
			CodeElement element3 = new CodeElement(Guid.NewGuid(), "CCC", "C Description");

			List.Elements.Add(element1);
			List.Elements.Add(element2);
			List.Elements.Add(element3);

			ReadOnlyCodeDescriptionPairList clone = new ReadOnlyCodeDescriptionPairList(List);

			AssertEquals("Count", 3, clone.Count);

			AssertEquals("Clone[0].GetType()", typeof(CodeElement), clone[0].GetType());
			AssertEquals("Clone[1].GetType()", typeof(CodeElement), clone[1].GetType());
			AssertEquals("Clone[2].GetType()", typeof(CodeElement), clone[2].GetType());

			AssertEquals("GetDescriptionFromCode(\"AAA\")", "A Description", clone.GetDescriptionFromCode("AAA"));
			AssertEquals("GetDescriptionFromCode(\"BBB\")", "B Description", clone.GetDescriptionFromCode("BBB"));
			AssertEquals("GetDescriptionFromCode(\"CCC\")", "C Description", clone.GetDescriptionFromCode("CCC"));

			AssertEquals("Clone[0].PK", element1.PK, clone[0].PK);
			AssertEquals("Clone[1].PK", element2.PK, clone[1].PK);
			AssertEquals("Clone[2].PK", element3.PK, clone[2].PK);

			Assert("Cloned list should not be the same instance the original list.", clone != List);
			Assert("Cloned list's elements should not be the same instances as the elements in the original list.", clone[0] != element1);
			Assert("Cloned list's elements should not be the same instances as the elements in the original list.", clone[1] != element2);
			Assert("Cloned list's elements should not be the same instances as the elements in the original list.", clone[2] != element3);
		}

		public void TestUsingXMLArray()
		{
			List.Elements.Add(new CodeDescriptionPair("AAA", "A Description"));
			List.Elements.Add(new CodeDescriptionPair("BBB", "B Description"));
			List.Elements.Add(new CodeDescriptionPair("CCC", "C Description"));

			byte[] xml = List.ToXMLByteArray();

			ReadOnlyCodeDescriptionPairList copyList = (ReadOnlyCodeDescriptionPairList)Activator.CreateInstance(CodeDescriptionPairListType, new object[] { xml });

			AssertEquals("Count", 3, copyList.Count);
			AssertEquals("GetDescriptionFromCode(\"AAA\")", "A Description", copyList.GetDescriptionFromCode("AAA"));
			AssertEquals("GetDescriptionFromCode(\"BBB\")", "B Description", copyList.GetDescriptionFromCode("BBB"));
			AssertEquals("GetDescriptionFromCode(\"CCC\")", "C Description", copyList.GetDescriptionFromCode("CCC"));
		}

		public void TestUsingXMLArrayWithEmptyBytes()
		{
			ReadOnlyCodeDescriptionPairList list = (ReadOnlyCodeDescriptionPairList)Activator.CreateInstance(CodeDescriptionPairListType, new object[] { Array.Empty<byte>() });
			AssertEquals("Should have zero elements.", 0, list.Count);
		}

		public void TestGetAllCodes()
		{
			var actual = List.GetAllCodes();
			AssertEquals("Count", 0, actual.Length);

			List.Elements.Add(new CodeDescriptionPair("ABC", ""));
			List.Elements.Add(new CodeDescriptionPair("XYZ", ""));

			actual = List.GetAllCodes();

			AssertEquals("Count", 2, actual.Length);
			AssertEquals("Contains(\"ABC\")", true, actual.Contains("ABC"));
			AssertEquals("Contains(\"XYZ\")", true, actual.Contains("XYZ"));
		}

		public void TestTrimsProperly()
		{
			List.Elements.Add(new CodeDescriptionPair("M", ""));

			AssertEquals("ContainsCode(\"M\")", true, List.ContainsCode("M"));
			AssertEquals("ContainsCode(\"M \")", true, List.ContainsCode("M "));
			AssertEquals("ContainsCode(\" M\")", false, List.ContainsCode(" M"));
		}

		public void TestContainsCode()
		{
			List.Elements.Add(new CodeDescriptionPair("ABC", ""));
			List.Elements.Add(new CodeDescriptionPair("XYZ", ""));

			AssertEquals("ContainsCode(\"ABC\")", true, List.ContainsCode("ABC"));
			AssertEquals("ContainsCode(\"XYZ\")", true, List.ContainsCode("XYZ"));
			AssertEquals("ContainsCode(\"!@#\")", false, List.ContainsCode("!@#"));
		}

		public void TestContainsOnly()
		{
			AssertEquals(false, List.ContainsOnly("ABC"));

			List.Elements.Add(new CodeDescriptionPair("ABC", ""));
			List.Elements.Add(new CodeDescriptionPair("XYZ", ""));

			AssertEquals(false, List.ContainsOnly("ABC"));
			AssertEquals(false, List.ContainsOnly("XYZ"));
			AssertEquals(true, List.ContainsOnly("ABC", "XYZ"));
			AssertEquals(false, List.ContainsOnly("ABC", "XYZ", "DEF"));
		}

		public void TestGetCodeAndDescription()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("ABC", "ABC Description");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("XYZ", "XYZ Description");

			List.Elements.Add(pair1);
			List.Elements.Add(pair2);

			AssertEquals("GetDescriptionFromCode(\"!@#\")", null, List.GetDescriptionFromCode("!@#"));
			AssertEquals("GetDescriptionFromCode(\"ABC\")", "ABC Description", List.GetDescriptionFromCode("ABC"));
			AssertEquals("GetDescriptionFromCode(\"XYZ\")", "XYZ Description", List.GetDescriptionFromCode("XYZ"));

			AssertEquals("GetCodeFromDescription(\"!@#\")", null, List.GetCodeFromDescription("!@#"));
			AssertEquals("GetCodeFromDescription(\"ABC Description\")", "ABC", List.GetCodeFromDescription("ABC Description"));
			AssertEquals("GetCodeFromDescription(\"XYZ Description\")", "XYZ", List.GetCodeFromDescription("XYZ Description"));
		}

		public void TestGetCodeInNonEnglishEnvironment()
		{
			var pair1 = new CodeDescriptionPair("ABC", (NoResString)"ABC Description");
			List.Elements.Add(pair1);
			using (var mockChs = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockChs.Put("BCB1E64A-F550-4968-8104-D700D2AE5FB0", new ResourceStringData("BCB1E64A-F550-4968-8104-D700D2AE5FB0", "测试"));
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
				{
					AssertEquals("GetCodeFromDescription(\"ABC Description\")", "ABC", List.GetCodeFromDescription("ABC Description"));
				}
			}
		}

		public void TestIndexer()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");

			List.Elements.Add(pair1);
			List.Elements.Add(pair2);

			AssertEquals("List[0]", pair1, List[0]);
			AssertEquals("List[1]", pair2, List[1]);
		}

		public void TestIndexOfCode()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("x", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("y", "");

			List.Elements.Add(pair1);
			List.Elements.Add(pair2);

			AssertEquals("IndexOfCode(\"x\")", 0, List.IndexOfCode("x"));
			AssertEquals("IndexOfCode(\"y\")", 1, List.IndexOfCode("y"));
			AssertEquals("IndexOfCode(\"z\")", -1, List.IndexOfCode("z"));
		}

		public void TestICollectionMembers()
		{
			ICollection collection = List;

			AssertEquals("IsSynchronized", false, collection.IsSynchronized);
			AssertEquals("SyncRoot", List.Elements, collection.SyncRoot);

			CodeDescriptionPair pair = new CodeDescriptionPair("", "");
			List.Elements.Add(pair);
			AssertEquals("Count", 1, List.Count);

			CodeDescriptionPair[] array = new CodeDescriptionPair[1];
			collection.CopyTo(array, 0);
			AssertEquals("Array[0]", pair, array[0]);
		}

		public void TestIEnumerableMembers()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");

			List.Elements.Add(pair1);
			List.Elements.Add(pair2);

			IEnumerator enumerator = ListAsIList.GetEnumerator();

			enumerator.MoveNext();
			AssertEquals("Enumerator.Current", pair1, enumerator.Current);

			enumerator.MoveNext();
			AssertEquals("Enumerator.Current", pair2, enumerator.Current);
		}

		public void TestDefaultCode()
		{
			List.DefaultCode = "ABC";
			AssertEquals("DefaultCode", "ABC", List.DefaultCode);
		}

		public void TestEquality()
		{
			ReadOnlyCodeDescriptionPairList list1 = new ReadOnlyCodeDescriptionPairList();
			ReadOnlyCodeDescriptionPairList list2 = new ReadOnlyCodeDescriptionPairList();
			AssertEquals(list1, list2);
			AssertEquals(list1.GetHashCode(), list2.GetHashCode());
			list1.Elements.Add(new CodeDescriptionPair("A", "aaa"));
			AssertNotEquals(list1, list2);
			list2.Elements.Add(new CodeDescriptionPair("A", "aaa"));
			AssertEquals(list1, list2);
			AssertEquals(list1.GetHashCode(), list2.GetHashCode());
			list2.Elements.Add(new CodeDescriptionPair("B", "bbb"));
			AssertNotEquals(list1, list2);
			list1.Elements.Add(new CodeDescriptionPair("C", "ccc"));
			AssertNotEquals(list1, list2);
			list1.Elements.Add(new CodeDescriptionPair("B", "bbb"));
			list2.Elements.Add(new CodeDescriptionPair("C", "ccc"));
			AssertEquals(list1, list2);
			AssertEquals(list1.GetHashCode(), list2.GetHashCode());
		}

		#region IList

		public void TestSupportedIListMembers()
		{
			CodeDescriptionPair pair1 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair2 = new CodeDescriptionPair("", "");
			CodeDescriptionPair pair3 = new CodeDescriptionPair("harbl", "yes");

			List.Elements.Add(pair1);

			AssertEquals("IsReadOnly", false, ListAsIList.IsReadOnly);
			AssertEquals("List.Contains(Pair1)", true, List.Contains(pair1));
			AssertEquals("List.Contains(Pair2)", true, List.Contains(pair2));
			AssertEquals("List.Contains(Pair3)", false, List.Contains(pair3));
			AssertEquals("ListAsIList.Contains(Pair1)", true, ListAsIList.Contains(pair1));
			AssertEquals("ListAsIList.Contains(Pair2)", true, ListAsIList.Contains(pair2));
			AssertEquals("ListAsIList.Contains(Pair3)", false, ListAsIList.Contains(pair3));
			AssertEquals("List[0]", pair1, List[0]);
			AssertEquals("ListAsIList[0]", pair1, ListAsIList[0]);
			AssertEquals("IndexOf(Pair1)", 0, ListAsIList.IndexOf(pair1));
			AssertEquals("IsFixedSize", false, ListAsIList.IsFixedSize);
		}

		// For the tests below this, we aren't using [ExpectException] because the tests need to be overriden in the subclass,
		// where they should work properly and not throw exceptions.
		public virtual void TestIListClear()
		{
			try
			{
				ListAsIList.Clear();
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Exception Message", "Cannot call IList.Clear() on a ReadOnlyCodeDescriptionPairList.", ex.Message);
			}
		}

		public virtual void TestIListIndexerSetter()
		{
			try
			{
				ListAsIList[0] = null;
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Exception Message", "Cannot set IList[value] on a ReadOnlyCodeDescriptionPairList.", ex.Message);
			}
		}

		public virtual void TestIListRemoveAt()
		{
			try
			{
				ListAsIList.RemoveAt(0);
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Exception Message", "Cannot call IList.RemoveAt(index) on a ReadOnlyCodeDescriptionPairList.", ex.Message);
			}
		}

		public virtual void TestIListInsert()
		{
			try
			{
				ListAsIList.Insert(0, null);
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Exception Message", "Cannot call IList.Insert(index, value) on a ReadOnlyCodeDescriptionPairList.", ex.Message);
			}
		}

		public virtual void TestIListRemove()
		{
			try
			{
				ListAsIList.Remove(null);
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Exception Message", "Cannot call IList.Remove(value) on a ReadOnlyCodeDescriptionPairList.", ex.Message);
			}
		}

		public virtual void TestIListAdd()
		{
			try
			{
				ListAsIList.Add(null);
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Exception Message", "Cannot call IList.Add(value) on a ReadOnlyCodeDescriptionPairList.", ex.Message);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			list = (ReadOnlyCodeDescriptionPairList)Activator.CreateInstance(CodeDescriptionPairListType, null);
		}

		protected virtual Type CodeDescriptionPairListType
		{
			get { return typeof(ReadOnlyCodeDescriptionPairList); }
		}

		protected IList ListAsIList
		{
			get { return List; }
		}

		protected ReadOnlyCodeDescriptionPairList List
		{
			get { return list; }
		}

		protected ReadOnlyCodeDescriptionPairList list;

		#endregion
	}
}
