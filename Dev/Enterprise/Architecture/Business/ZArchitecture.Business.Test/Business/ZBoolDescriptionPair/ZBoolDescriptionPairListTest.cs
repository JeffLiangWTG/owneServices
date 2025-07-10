using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class ZBoolDescriptionPairListTest : TestCase
	{
		public void TestIndexerByInt()
		{
			ZBoolDescriptionPair result = List[0];
			AssertEquals("Indexing by int", Pair, result);
			AssertEquals("Indexing by int", Pair.Description, result.Description);
			AssertEquals("Indexing by int", Pair.Value, result.Value);
		}

		public void TestIndexerByDescription()
		{
			ZBoolDescriptionPair badResult = List["Crap Description"];
			AssertNull("Indexing by description", badResult);

			ZBoolDescriptionPair goodResult = List["Bebop"];
			AssertEquals("Indexing by description", Pair, goodResult);
			AssertEquals("Indexing by description", Pair.Description, goodResult.Description);
			AssertEquals("Indexing by description", Pair.Value, goodResult.Value);
		}

		public void TestAddNew()
		{
			AssertEquals("Precondition", 2, List.Count);

			List.AddNew("Rocksteady", false);
			AssertEquals("Should be 3 pairs in the list", 3, List.Count);
			AssertEquals("Rocksteady", List["Rocksteady"].Description);
			AssertEquals(false, List["Rocksteady"].Value);
		}

		public void TestAddNewWithPK()
		{
			AssertEquals("Precondition", 2, List.Count);
			AssertNotNull("Precondition - Krang Exists", List["Krang"]);

			List.AddNew("April", false);
			AssertEquals("Should be 3 pairs in the list", 3, List.Count);
			AssertEquals("April", List["April"].Description);
			AssertEquals(false, List["April"].Value);
		}

		public void TestAdd()
		{
			ZBoolDescriptionPair newPair = new ZBoolDescriptionPair("Shredder", true);
			AssertEquals("Precondition", 2, List.Count);

			List.Add(newPair);
			AssertEquals("Should be 3 pairs in the list", 3, List.Count);
			AssertEquals("Shredder", List["Shredder"].Description);
			AssertEquals(true, List["Shredder"].Value);
		}

		public void TestAddNewFiresOnChanged()
		{
			Assert("Precondition - OnListChangedFired should be false", !OnListChangedFired);

			List.AddNew("Leonardo", true);
			Assert("List.OnListChanged should have fired", OnListChangedFired);
		}

		public void TestAddFiresOnChanged()
		{
			ZBoolDescriptionPair newPair = new ZBoolDescriptionPair("Michaelangelo", true);
			Assert("Precondition - OnListChangedFired should be false", !OnListChangedFired);

			List.Add(newPair);
			Assert("List.OnListChanged should have fired", OnListChangedFired);
		}

		public void TestSettingPairDescriptionFiresParentListOnPairChanged()
		{
			Assert("Precondition - List.OnPairChangedFired should be false", !OnPairChangedFired);

			Pair.Description = "Raphael";
			Assert("List.OnPairChangedFired should have fired", OnPairChangedFired);
		}

		public void TestSettingPairDescriptionFiresParentListOnPairChangedForBinding()
		{
			Assert("Precondition - List.OnPairChangedFired should be false", !OnPairChangedForBindingFired);

			Pair.Description = "Splinter";
			Assert("List.OnPairChangedFired should have fired", OnPairChangedForBindingFired);
		}

		public void TestSettingPairValueFiresParentListOnChanged()
		{
			Assert("Precondition - List.OnPairChangedFired should be false", !OnPairChangedFired);

			Pair.Value = false;
			Assert("List.OnPairChangedFired should have fired", OnPairChangedFired);
		}

		public void TestCount()
		{
			AssertEquals("Precondition", 2, List.Count);

			List.AddNew("Donatello", true);
			AssertEquals("List.Count should be 3", 3, List.Count);
		}

		public void TestClear()
		{
			AssertEquals("Precondition", 2, List.Count);

			List.Clear();
			AssertEquals("List.Count should be 0", 0, List.Count);
		}

		public void TestIsOnChangedSuspended()
		{
			AssertEquals("Precondition", false, List.IsOnChangedSuspended);

			List.SuspendOnChanged();
			AssertEquals("List.IsOnChangedSuspended should be true", true, List.IsOnChangedSuspended);

			List.ResumeOnChanged();
			AssertEquals("List.IsOnChangedSuspended should be false", false, List.IsOnChangedSuspended);
		}

		public void TestSuspendPairChanged()
		{
			AssertEquals("Precondition", false, List.IsOnChangedSuspended);

			List.SuspendOnChanged();
			AssertEquals("List.IsOnChangedSuspended should be true", true, List.IsOnChangedSuspended);
		}

		public void TestResumePairChanged()
		{
			List.SuspendOnChanged();
			AssertEquals("Precondition", true, List.IsOnChangedSuspended);

			List.ResumeOnChanged();
			AssertEquals("List.IsOnChangedSuspended should be false", false, List.IsOnChangedSuspended);
		}

		public void TestResumeOnChangedWithoutSuspendOnChangedReportsDevError()
		{
			AssertEquals("Precondition - Last Developer Error Should be blank", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			try
			{
				List.ResumeOnChanged();
				AssertEquals("Calling List.ResumeOnChanged() without first calling List.SuspendOnChanged() should report a developer error", false, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			TestGuid = ZGuid.NewZGuid();
			Pair = new ZBoolDescriptionPair("Bebop", true);
			PairWithPK = new ZBoolDescriptionPair(TestGuid, "Krang", true);

			List = new ZBoolDescriptionPairList();
			List.Add(Pair);
			List.Add(PairWithPK);
			List.OnListChanged += new ZBoolDescriptionPairChangedEventHandler(List_OnListChanged);
			List.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler(List_OnPairChanged);
			List.OnPairChangedForBinding += new ZBoolDescriptionPairChangedEventHandler(List_OnPairChangedForBinding);

			OnListChangedFired = false;
			OnPairChangedFired = false;
			OnPairChangedForBindingFired = false;
			ErrorReporter.Clear();
		}

		void List_OnListChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			OnListChangedFired = true;
		}

		void List_OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			OnPairChangedFired = true;
		}

		void List_OnPairChangedForBinding(ZBoolDescriptionPairChangedEventArgs e)
		{
			OnPairChangedForBindingFired = true;
		}

		ZBoolDescriptionPairList List;
		ZBoolDescriptionPair Pair;
		ZBoolDescriptionPair PairWithPK;

		ZGuid TestGuid;

		bool OnListChangedFired;
		bool OnPairChangedFired;
		bool OnPairChangedForBindingFired;

		#endregion
	}
}
