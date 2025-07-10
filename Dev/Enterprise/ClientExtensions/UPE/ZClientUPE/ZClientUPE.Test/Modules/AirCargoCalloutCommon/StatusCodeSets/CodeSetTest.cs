using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class CodeSetTest : TestCase
	{
		public void TestAddRemoveContains()
		{
			CodeSet set = new CodeSet();
			set.Add("SPL");
			AssertEquals("Code added to the set correctly", true, set.Contains("SPL"));
			set.Add("SPL");
			AssertEquals("Code re-added to the set still is in the set", true, set.Contains("SPL"));
			set.Remove("SPL");
			AssertEquals("Code removed from the set not in the set", false, set.Contains("SPL"));
		}

		public void TestAddingEmptyCodeHasNoEffect()
		{
			CodeSet set = new CodeSet();
			set.Add("");
			AssertEquals("Empty code shouldn't be added", true, set.IsEmpty);
		}

		public void TestCount()
		{
			CodeSet codeSet = new CodeSet();
			codeSet.Add("Code1");
			codeSet.Add("Code2");
			AssertEquals("There should be 2 elements in the list", 2, codeSet.Count);
		}

		public void TestClear()
		{
			CodeSet codeSet = new CodeSet();
			codeSet.Add("Code1");
			codeSet.Add("Code2");
			AssertEquals("Should not be empty initially for the test", false, codeSet.IsEmpty);
			codeSet.Clear();
			AssertEquals("Should be empty after Clear()", true, codeSet.IsEmpty);
		}

		public void TestIsEmpty()
		{
			CodeSet codeSet = new CodeSet();
			AssertEquals("The set is empty initially", true, codeSet.IsEmpty);
			codeSet.Add("Code1");
			codeSet.Add("Code2");
			AssertEquals("Should not be empty with 2 items", false, codeSet.IsEmpty);
			codeSet.Remove("Code1");
			codeSet.Remove("Code2");
			AssertEquals("Should be empty after removing all items", true, codeSet.IsEmpty);
		}

		public void TestGetMultipleCodeFilter()
		{
			CodeSet codeSet = new CodeSet();
			codeSet.Add("Code1");
			codeSet.Add("Code2");
			AssertEquals("Filter should OR all the codes together", "(OH_Code in ('Code1', 'Code2'))", codeSet.GetMultipleCodeFilter(OrgHeaderSchema.OH_Code).LiteralTextADO);
		}

		public void TestChanged()
		{
			CodeSet codeSet = new CodeSet();
			codeSet.Changed += new EventHandler(OnCodeSet_Changed);
			OnCodeSet_ChangedCalled = false;
			codeSet.Add("Code");
			AssertEquals("When adding a new item, the set has changed", true, OnCodeSet_ChangedCalled);
			OnCodeSet_ChangedCalled = false;
			codeSet.Add("Code");
			AssertEquals("When adding an item that already exists, the set has not changed", false, OnCodeSet_ChangedCalled);
			OnCodeSet_ChangedCalled = false;
			codeSet.Remove("Code");
			AssertEquals("When removing an item the set has changed", true, OnCodeSet_ChangedCalled);
			OnCodeSet_ChangedCalled = false;
			codeSet.Remove("Code");
			AssertEquals("When removing a non-existant item the set has not changed", false, OnCodeSet_ChangedCalled);
			codeSet.Add("Code");
			OnCodeSet_ChangedCalled = false;
			codeSet.Clear();
			AssertEquals("When clearing a set with an item the set has changed", true, OnCodeSet_ChangedCalled);
			OnCodeSet_ChangedCalled = false;
			codeSet.Clear();
			AssertEquals("When clearing an empty set the set has not changed", false, OnCodeSet_ChangedCalled);
		}

		void OnCodeSet_Changed(object sender, EventArgs e)
		{
			OnCodeSet_ChangedCalled = true;
		}

		bool OnCodeSet_ChangedCalled;
		public void TestClone()
		{
			CodeSet codeSet = new CodeSet();
			codeSet.Add("CD1");
			codeSet.Add("CD2");
			CodeSet clonedCodeSet = codeSet.Clone();
			AssertEquals("First code should have been cloned", true, clonedCodeSet.Contains("CD1"));
			AssertEquals("Second code should have been cloned", true, clonedCodeSet.Contains("CD2"));
		}

		public void TestEnumerator()
		{
			CodeSet set = new CodeSet();
			set.Add("Code1");
			set.Add("Code1");
			set.Add("Code2");
			StringCollectionX enumeratedCodes = new StringCollectionX();
			foreach (ZString code in set)
			{
				enumeratedCodes.Add(code);
			}

			AssertEquals("2 codes should have been enumerated from the set", 2, enumeratedCodes.Count);
			AssertCollectionContains("Code1 should have been enumerated", "Code1", enumeratedCodes);
			AssertCollectionContains("Code2 should have been enumerated", "Code2", enumeratedCodes);
		}

		public void TestOnClearComplete()
		{
			TestCodeSet codeSet = new TestCodeSet();
			codeSet.Add("Code");
			codeSet.OnClearCompleteCalled = false;
			codeSet.Clear();
			AssertEquals("OnClearComplete called after Clear()", true, codeSet.OnClearCompleteCalled);
			codeSet.OnClearCompleteCalled = false;
			codeSet.Clear();
			AssertEquals("OnClearComplete called after Clear() even though set was empty", true, codeSet.OnClearCompleteCalled);
		}

		class TestCodeSet : CodeSet
		{
			public bool OnClearCompleteCalled;
			protected override void OnClearComplete()
			{
				base.OnClearComplete();
				OnClearCompleteCalled = true;
			}
		}
	}
}
