using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class GCWrapperTest : TestCase
	{
		public void TestReclaimMemoryOnObject()
		{
			AssignObject(() => new object());
			WeakReference objRef = new WeakReference(obj);
			WaitPeriodsTillGcRun(ref obj);
			AssertNull(obj);
			Assert(!objRef.IsAlive);
		}

		public void TestReclaimMemoryOnDisposable()
		{
			disposeCalled = false;
			AssignObject(() => new DisposableObject());
			WeakReference objRef = new WeakReference(obj);
			WaitPeriodsTillGcRun(ref obj);
			AssertNull(obj);
			Assert(!objRef.IsAlive);
			Assert(disposeCalled);
		}

		public void TestReclaimMemoryOnByteArray()
		{
			AssignObject(() => new byte[100000]);
			WeakReference objRef = new WeakReference(obj);
			WaitPeriodsTillGcRun(ref obj);
			AssertNull(obj);
			Assert(!objRef.IsAlive);
		}

		[SnailTest]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI005:WeakReferenceTargetRaceConditionRule", Justification = "Testing")]
		public void TestReclaimMemoryGenerational()
		{
			AssignObject(() => new object());
			AssignObject2(() => new object());
			WeakReference obj1Ref = new WeakReference(obj);
			WeakReference obj2Ref = new WeakReference(obj2);
			while (GC.GetGeneration(obj) < 2)
			{
				GC.Collect();
			}

			AssertEquals(2, GC.GetGeneration(obj));
			WaitPeriodsTillGcRun(ref obj);
			AssertNull(obj);
			Assert(!obj1Ref.IsAlive);
			AssertEquals(2, GC.GetGeneration(obj2));
			GCWrapper.ReclaimMemory(ref obj2);
			AssertNull(obj2);
			Assert(obj2Ref.IsAlive);
			Thread.Sleep(1100);
			AssignObject3(() => new object());
			WeakReference obj3Ref = new WeakReference(obj3);
			WaitPeriodsTillGcRun(ref obj3);
			AssertNull(obj3);
			Assert(!obj3Ref.IsAlive);
			Assert(obj2Ref.IsAlive);
			AssignObject2(() => obj2Ref.Target);
			AssertEquals(2, GC.GetGeneration(obj2));
			Thread.Sleep(30100);
			WaitPeriodsTillGcRun(ref obj2);
			AssertNull(obj2);
			Assert(!obj2Ref.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void AssignObject(Func<object> func)
		{
			obj = func();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void AssignObject2(Func<object> func)
		{
			obj2 = func();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		void AssignObject3(Func<object> func)
		{
			obj3 = func();
		}

		void WaitPeriodsTillGcRun(ref object obj)
		{
			GCWrapper.ReclaimMemory(ref obj);
			if (!SpinWait.SpinUntil(() => GC.WaitForFullGCComplete(100) != GCNotificationStatus.Succeeded, TimeSpan.FromSeconds(3)))
			{
				Assert("The GC did not execute within 3 seconds after invoking GC.Collect().", true);
			}
		}

		object obj;
		object obj2;
		object obj3;
		static bool disposeCalled;
		class DisposableObject : IDisposable
		{
			public void Dispose()
			{
				GCWrapperTest.disposeCalled = true;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			GCWrapper.ResetLastCollectionTimesForTest();
		}
	}
}
