using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LockMechanismTest : TestCase
	{
		public void TestLockMechanismForTestFactory()
		{
			var messages = new List<string>();
			var factory = new LockMechanismForTestFactory(s => messages.Add(s));

			var m1 = factory.Create("owner1");
			var m2 = factory.Create("owner2");

			AssertEquals(true, m1.TryGetLock("Key1", out var lock1M1));
			AssertNotNull(lock1M1);
			AssertEquals(false, m2.TryGetLock("Key1", out var lock1M2));
			AssertNull(lock1M2);

			AssertEquals(true, m2.TryGetLock("Key2", out var lock2M2));
			AssertNotNull(lock2M2);

			lock1M1.Dispose();
			AssertEquals(true, m2.TryGetLock("Key1", out lock1M2));
			AssertNotNull(lock1M2);

			AssertContains(
				"Avoid locking same key",
				AssertExceptionThrown<InvalidOperationException>(() => m2.TryGetLock("Key1", out _)).Message
			);

			AssertContains(
				"Cannot release",
				AssertExceptionThrown<InvalidOperationException>(() => lock1M1.Dispose()).Message
			);

			AssertMultilineASCIIEquals(@"
Lock acquired: key='Key1', owner='owner1'.
Lock rejected: key='Key1', owner='owner2', currentOwner='owner1'.
Lock acquired: key='Key2', owner='owner2'.
Lock released: key='Key1', owner='owner1'.
Lock acquired: key='Key1', owner='owner2'.
			".Trim(), string.Join("\r\n", messages));
		}
	}
}
