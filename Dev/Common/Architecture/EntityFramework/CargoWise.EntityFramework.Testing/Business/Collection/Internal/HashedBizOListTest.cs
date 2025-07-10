using System;
using System.Collections;
using CargoWise.Common.Collections;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class HashedBizOListTest : TestCaseWithFactory
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestAddNull()
		{
			HashedBizOList list = new HashedBizOList();
			list.Add(null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestInsertNull()
		{
			HashedBizOList list = new HashedBizOList();
			list.Insert(0, null);
		}

		public void TestIndexOfOptimisedForHashByPK()
		{
			HashedBizOList list = new HashedBizOList();
			AssertEquals(-1, list.IndexOfOptimisedForHashByPK(null));
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			list.Add(dummy1);
			list.Add(dummy2);
			list.Add(dummy3);
			AssertEquals(0, list.IndexOfOptimisedForHashByPK(dummy1));
			AssertEquals(1, list.IndexOfOptimisedForHashByPK(dummy2));
			AssertEquals(2, list.IndexOfOptimisedForHashByPK(dummy3));
		}

		public void TestIsDeletedTypeContained()
		{
			var list = new HashedBizOList();
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy1_2 = Factory.Load<DummyBaseBusinessObject>(dummy1.PK);

			AssertEquals(false, dummy1 == dummy1_2);

			list.Add(dummy1);
			list.Add(dummy2);

			AssertEquals(true, list.Contains(dummy1));
			AssertEquals(true, list.Contains(dummy2));
			AssertEquals(true, list.Contains(dummy1_2));

			dummy1.Delete();

			AssertEquals(true, list.Contains(dummy1));
			AssertEquals(true, list.Contains(dummy2));
			AssertEquals(true, list.Contains(dummy1_2));
		}

		public void TestIndexOfWithNull()
		{
			HashedBizOList list = new HashedBizOList();
			AssertEquals(-1, list.IndexOf(null));
		}

		public void TestIndexOfWithStartIndex()
		{
			HashedBizOList list = new HashedBizOList();
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			list.Add(dummy1);
			list.Add(dummy2);
			list.Add(dummy3);

			AssertEquals(-1, list.IndexOf(dummy3, 0, 1));
			AssertEquals(2, list.IndexOf(dummy3, 0, 100));
			AssertEquals(2, list.IndexOf(dummy3, 2, 1));
			AssertEquals(-1, list.IndexOf(dummy1, 2, 1));

			list.Remove(dummy2);
			AssertEquals(1, list.IndexOf(dummy3, 1, 1));
		}

		public void TestGeneralBusinessObjectComparer()
		{
			var comparer = new HashedBizOList.GeneralBusinessObjectComparer(null);
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Delete();

			AssertEquals(0, comparer.Compare(null, null));
			AssertEquals(1, comparer.Compare(dummy1, null));
			AssertEquals(-1, comparer.Compare(null, dummy1));

			AssertEquals(0, comparer.Compare(null, dummy2));
			AssertEquals(0, comparer.Compare(dummy2, null));
			AssertEquals(1, comparer.Compare(dummy1, dummy2));
			AssertEquals(-1, comparer.Compare(dummy2, dummy1));

			dummy1.Delete();

			AssertEquals(0, comparer.Compare(dummy1, dummy2));
		}

		public void TestNonPersistentBizoContains()
		{
			var list = new HashedBizOList();
			var dummy1 = new NonPersistentWithOverriddenEquality(1);
			list.Add(dummy1);

			AssertEquals(true, list.Contains(dummy1));
			AssertEquals(true, list.Contains(new NonPersistentWithOverriddenEquality(1)));
			AssertEquals(false, list.Contains(new NonPersistentWithOverriddenEquality(2)));
		}

		public void TestSort_DoesNotImplyListChanged()
		{
			var list = new HashedBizOList();
			var dummy1 = new NonPersistentWithOverriddenEquality(1);
			var dummy2 = new NonPersistentWithOverriddenEquality(2);
			var dummy3 = new NonPersistentWithOverriddenEquality(3);
			list.Add(dummy2);
			list.Add(dummy3);
			list.Add(dummy1);

			list.ApplySort((IComparer)new ZComparer<NonPersistentWithOverriddenEquality>((x, y) => x.Number.CompareTo(y.Number)));
			AssertArrayEqualsByElements(new[] { dummy1, dummy2, dummy3 }, list.ToArray());
			list.GetByPK(dummy1.PK);
			Assert(list.HasHash);

			list.ApplySort((IComparer)new ZComparer<NonPersistentWithOverriddenEquality>((x, y) => x.Number.CompareTo(y.Number)));
			Assert("Order didn't change so keep the hash.", list.HasHash);
		}

		#region NonPersistentWithOverriddenEquality

		class NonPersistentWithOverriddenEquality : NonPersistentBusinessObject
		{
			public NonPersistentWithOverriddenEquality(int number)
			{
				this.Number = number;
			}

			public int Number { get; }
			public override bool Equals(object obj) => obj is NonPersistentWithOverriddenEquality non && non.Number == Number;
			public override int GetHashCode() => Number;
		}

		#endregion
	}
}
