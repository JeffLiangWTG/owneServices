using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	internal class WeakHashtableTest : TestCase
	{
		public void TestGC()
		{
			object o = new object();
			TestWeakHashtable hash = new TestWeakHashtable();
			hash[o] = 5;
			Thread.Sleep(0);
			AssertEquals("Entry should exist while a reference to the key exists", 5, hash[o]);
			o = null;
			GC.Collect();
			Thread.Sleep(0);
			AssertNull("Reference to the key is null, should be no entry", hash[o]);
		}

		public void TestEnumerator()
		{
			TestKey key1;
			TestKey key2;
			TestKey key3;
			TestKey key4;
			TestWeakHashtable hash = SetupTestEnumerator(out key1, out key2, out key3, out key4);
			int index = 1;
			foreach (KeyValuePair<object, object> entry in hash)
			{
				hash.RemoveGCdRefs();
				if (index == 1)
				{
					AssertEquals(key1, entry.Key);
					key2 = null;
					GC.Collect();
					Thread.Sleep(0);
				}
				else if (index == 2)
				{
					AssertEquals(key3, entry.Key);
					key4 = null;
					GC.Collect();
					Thread.Sleep(0);
				}

				index++;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		TestWeakHashtable SetupTestEnumerator(out TestKey key1, out TestKey key2, out TestKey key3, out TestKey key4)
		{
			TestWeakHashtable hash = new TestWeakHashtable();
			key1 = new TestKey("key1", 4);
			key2 = new TestKey("key2", 3);
			key3 = new TestKey("key3", 2);
			key4 = new TestKey("key4", 1);
			hash[key1] = 1;
			hash[key2] = 2;
			hash[key3] = 3;
			hash[key4] = 4;
			// make sure we iterate in the order of the keys for the purpose of this test
			int index = 1;
			foreach (KeyValuePair<object, object> entry in hash)
			{
				if (index == 1)
				{
					AssertEquals("Keys not suitable", key1, entry.Key);
				}
				else if (index == 2)
				{
					AssertEquals("Keys not suitable", key2, entry.Key);
				}
				else if (index == 3)
				{
					AssertEquals("Keys not suitable", key3, entry.Key);
				}
				else if (index == 4)
				{
					AssertEquals("Keys not suitable", key4, entry.Key);
				}

				index++;
			}

			return hash;
		}

		public void TestRemove()
		{
			WeakReferencedKeyDictionary<object, object> hash = new WeakReferencedKeyDictionary<object, object>();
			AssertEquals("Should have no items initially", 0, hash.Count);
			object key1 = new object();
			object key2 = new object();
			hash[key1] = null;
			hash[key2] = null;
			AssertEquals("Should have 2 items now", 2, hash.Count);
			hash.Remove(key2);
			AssertEquals(true, hash.ContainsKey(key1));
			AssertEquals(false, hash.ContainsKey(key2));
		}

		#region TestGCHappeningDuringEqualsAndGetHashCode
		[ExpectNoExceptions]
		public void TestWhenGCDuringEqualsAndGetHashCode()
		{
			var t = new Thread(new ThreadStart(DoWhenGCDuringEqualsAndGetHashCode_ThreadStart));
			t.Name = "WeakHashtable.TestWhenGCDuringEqualsAndGetHashCode";
			t.Start();
			for (int i = 0; i < 100 && !t.IsAlive; i++)
			{
				Thread.Sleep(10);
			}

			while (t.IsAlive)
			{
				GC.Collect();
				Thread.Sleep(1);
			}
		}

		void DoWhenGCDuringEqualsAndGetHashCode_ThreadStart()
		{
			for (int i = 0; i < 256; i++)
			{
				WeakReferencedKeyDictionary<object, object> hash = new WeakReferencedKeyDictionary<object, object>();
				hash[new TestKey("", 1)] = new object();
				hash[new TestKey("", 1)] = new object();
				_ = hash.ContainsKey(new TestKey("", 1));
				foreach (KeyValuePair<object, object> entryNotUsed in hash)
				{
					object x = entryNotUsed;
				}

				Thread.Sleep(1);
			}
		}

		#endregion
		#region Test Classes
		class TestKey
		{
			public TestKey(string str, int i)
			{
				this.str = str;
				this.index = i;
			}

			public override int GetHashCode()
			{
				return this.index;
			}

			public override bool Equals(object obj)
			{
				return this.index == ((TestKey)obj).index;
			}

			public override string ToString()
			{
				return this.str;
			}

			readonly string str;
			readonly int index;
		}

		class TestWeakHashtable : WeakReferencedKeyDictionary<object, object>
		{
			public new void RemoveGCdRefs()
			{
				base.RemoveGCdRefs();
			}
		}
		#endregion
	}
}