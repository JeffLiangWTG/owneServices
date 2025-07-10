using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LockingMessageQueueTest : TestCase
	{
		public void TestDispose()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q1 = new LockingMessageQueue(lockMechanismFactory.Create("LQ1"));
			var q2 = new LockingMessageQueue(lockMechanismFactory.Create("LQ2"));
			var q3 = new LockingMessageQueue(lockMechanismFactory.Create("LQ3"));

			var m1 = new MessageKeySet(new[] { "K1" });
			var m2 = new MessageKeySet(new[] { "K1" });

			AssertEquals(true, q1.Enqueue(m1));
			AssertEquals(false, q2.Enqueue(m1));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K1', owner='LQ1'.
Lock rejected: key='K1', owner='LQ2', currentOwner='LQ1'.
			".Trim(), string.Join("\r\n", messages));
			messages.Clear();

			q1.Dispose();
			AssertEquals(false, q2.Enqueue(m2));
			AssertEquals(true, q3.Enqueue(m2));

			AssertMultilineASCIIEquals(@"
Lock released: key='K1', owner='LQ1'.
Lock acquired: key='K1', owner='LQ3'.
			".Trim(), string.Join("\r\n", messages));
			messages.Clear();

			AssertEquals
			(
				"Cannot enqueue messages into a LockingMessageQueue after it was disposed.",
				AssertExceptionThrown<InvalidOperationException>(() => q1.Enqueue(m2)).Message
			);

			// disposing several times is ok 
			q1.Dispose();
			q1.Dispose();
			q2.Dispose();
			q3.Dispose();

			AssertMultilineASCIIEquals(@"
Lock released: key='K1', owner='LQ3'.
			".Trim(), string.Join("\r\n", messages));
			messages.Clear();
		}

		public void TestLocking()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q1 = new LockingMessageQueue(lockMechanismFactory.Create("LQ1"));
			var q2 = new LockingMessageQueue(lockMechanismFactory.Create("LQ2"));

			var m1 = new MessageKeySet(new[] { "K1" });
			var m2 = new MessageKeySet(new[] { "K2" });
			var m3 = new MessageKeySet(new[] { "K1" });
			var m4 = new MessageKeySet(new[] { "K2" });
			var m5 = new MessageKeySet(new[] { "K1", "K2" });

			AssertEquals(true, q1.Enqueue(m1));
			AssertEquals(false, q2.Enqueue(m1));

			AssertEquals(true, q2.Enqueue(m2));
			AssertEquals(false, q1.Enqueue(m2));

			AssertEquals(true, q1.Enqueue(m3));
			AssertEquals(false, q2.Enqueue(m3));

			AssertEquals(false, q1.Enqueue(m4));
			AssertEquals(true, q2.Enqueue(m4));

			AssertEquals(false, q1.Enqueue(m5));
			AssertEquals(false, q2.Enqueue(m5));

			var q3 = new LockingMessageQueue(lockMechanismFactory.Create("LQ3"));
			AssertEquals(false, q3.Enqueue(m5));

			q1.Dispose();
			q2.Dispose();

			var q4 = new LockingMessageQueue(lockMechanismFactory.Create("LQ4"));
			AssertEquals(true, q4.Enqueue(m5));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K1', owner='LQ1'.
Lock rejected: key='K1', owner='LQ2', currentOwner='LQ1'.
Lock acquired: key='K2', owner='LQ2'.
Lock rejected: key='K2', owner='LQ1', currentOwner='LQ2'.
Lock rejected: key='K1', owner='LQ3', currentOwner='LQ1'.
Lock released: key='K1', owner='LQ1'.
Lock released: key='K2', owner='LQ2'.
Lock acquired: key='K1', owner='LQ4'.
Lock acquired: key='K2', owner='LQ4'.
			".Trim(), string.Join("\r\n", messages));
		}

		public void TestSkippedUnlocked()
		{
			var lockMechanismFactory = new LockMechanismForTestFactory();
			var q1 = new LockingMessageQueue(lockMechanismFactory.Create("LQ1"));
			var q2 = new LockingMessageQueue(lockMechanismFactory.Create("LQ2"));

			var m1 = new MessageKeySet(new[] { "K1" });
			var m2 = new MessageKeySet(new[] { "K1", "K2" });
			var m3 = new MessageKeySet(new[] { "K2" });
			var m4 = new MessageKeySet(new[] { "K3" });

			AssertEquals(true, q1.Enqueue(m1));
			AssertEquals(false, q2.Enqueue(m1));

			q1.Dispose();

			AssertEquals("cannot enqueue because 'K1' in m1 was already rejected", false, q2.Enqueue(m2));
			AssertEquals("cannot enqueue because 'K2' in m2 was already rejected", false, q2.Enqueue(m3));
			AssertEquals(true, q2.Enqueue(m4));
		}

		public void TestAffectsVsDependsOn()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q1 = new LockingMessageQueue(lockMechanismFactory.Create("LQ1"));
			var q2 = new LockingMessageQueue(lockMechanismFactory.Create("LQ2"));

			var m1 = new MessageKeySet(new[] { "K1", "UNK" }, new[] { "K1" });
			var m2 = new MessageKeySet(new[] { "K2", "UNK" }, new[] { "K2" });
			var m3 = new MessageKeySet(new[] { "UNK" }, new[] { "UNK" });
			var m4 = new MessageKeySet(new[] { "K3", "UNK" }, new[] { "K3" });

			AssertEquals(true, q1.Enqueue(m1));
			AssertEquals(false, q2.Enqueue(m1));

			AssertEquals(true, q2.Enqueue(m2));
			AssertEquals(false, q1.Enqueue(m2));

			AssertEquals(true, q2.Enqueue(m3));
			AssertEquals(false, q1.Enqueue(m3));

			AssertEquals(false, q1.Enqueue(m4));
			AssertEquals(true, q2.Enqueue(m4));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K1', owner='LQ1'.
Lock rejected: key='K1', owner='LQ2', currentOwner='LQ1'.
Lock acquired: key='K2', owner='LQ2'.
Lock rejected: key='K2', owner='LQ1', currentOwner='LQ2'.
Lock acquired: key='UNK', owner='LQ2'.
Lock rejected: key='UNK', owner='LQ1', currentOwner='LQ2'.
Lock acquired: key='K3', owner='LQ2'.
			".Trim(), string.Join("\r\n", messages));
		}

		public void TestDependsOnWithoutAffects()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q1 = new LockingMessageQueue(lockMechanismFactory.Create("LQ1"));
			var q2 = new LockingMessageQueue(lockMechanismFactory.Create("LQ2"));

			var m1 = new MessageKeySet(new[] { "KX" }, new[] { "K1" });
			var m2 = new MessageKeySet(new[] { "KX" }, new[] { "K2" });
			var m3 = new MessageKeySet(new[] { "KX" }, new[] { "K1" });

			AssertEquals(true, q1.Enqueue(m1));
			AssertEquals(false, q2.Enqueue(m1));

			AssertEquals(true, q2.Enqueue(m2));
			AssertEquals(false, q1.Enqueue(m2));

			q1.Dispose();

			AssertEquals("m1 affected K1, but there is no dependency on it", true, q2.Enqueue(m3));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K1', owner='LQ1'.
Lock rejected: key='K1', owner='LQ2', currentOwner='LQ1'.
Lock acquired: key='K2', owner='LQ2'.
Lock rejected: key='K2', owner='LQ1', currentOwner='LQ2'.
Lock released: key='K1', owner='LQ1'.
Lock acquired: key='K1', owner='LQ2'.
			".Trim(), string.Join("\r\n", messages));
		}

		public void TestLockOrderWithinMessage()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q = new LockingMessageQueue(lockMechanismFactory.Create("LQ"));

			AssertEquals(true, q.Enqueue(new MessageKeySet(new[] { "K1-1", "K1-2" })));
			AssertEquals(true, q.Enqueue(new MessageKeySet(new[] { "K2-2", "K2-1" })));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K1-1', owner='LQ'.
Lock acquired: key='K1-2', owner='LQ'.
Lock acquired: key='K2-1', owner='LQ'.
Lock acquired: key='K2-2', owner='LQ'.
			".Trim(), string.Join("\r\n", messages));
		}

		public void TestDuplicateKeys()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q = new LockingMessageQueue(lockMechanismFactory.Create("LQ"));

			AssertEquals(true, q.Enqueue(new MessageKeySet(new[] { "K11", "K12", "K12" })));
			AssertEquals(true, q.Enqueue(new MessageKeySet(new[] { "K22", "K22", "K21" })));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K11', owner='LQ'.
Lock acquired: key='K12', owner='LQ'.
Lock acquired: key='K21', owner='LQ'.
Lock acquired: key='K22', owner='LQ'.
			".Trim(), string.Join("\r\n", messages));
		}

		public void TestMiddleKeyLocked()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			var q1 = new LockingMessageQueue(lockMechanismFactory.Create("LQ1"));
			var q2 = new LockingMessageQueue(lockMechanismFactory.Create("LQ2"));

			AssertEquals(true, q1.Enqueue(new MessageKeySet(new[] { "K2" })));
			AssertEquals(false, q2.Enqueue(new MessageKeySet(new[] { "K1", "K2", "K3" })));

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K2', owner='LQ1'.
Lock acquired: key='K1', owner='LQ2'.
Lock rejected: key='K2', owner='LQ2', currentOwner='LQ1'.
Lock released: key='K1', owner='LQ2'.
			".Trim(), string.Join("\r\n", messages));
		}

		public void TestExceptionDuringLocking()
		{
			var messages = new List<string>();
			var lockMechanismFactory = new LockMechanismForTestFactory(s => messages.Add(s));
			lockMechanismFactory.OnBeforeLock += (owner, key) =>
			{
				if (key == "K2")
				{
					throw new InvalidOperationException("K2 is broken");
				}
			};

			var lockMechanism = lockMechanismFactory.Create("LQ");
			var q = new LockingMessageQueue(lockMechanism);

			AssertEquals
			(
				"K2 is broken",
				AssertExceptionThrown<InvalidOperationException>(() => q.Enqueue(new MessageKeySet(new[] { "K1", "K2", "K3" }))).Message
			);

			AssertMultilineASCIIEquals(@"
Lock acquired: key='K1', owner='LQ'.
Lock released: key='K1', owner='LQ'.
			".Trim(), string.Join("\r\n", messages));
		}
	}
}
