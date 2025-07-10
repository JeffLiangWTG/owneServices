using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class MutexIDTest : TestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestReasonNotNull()
		{
			new MutexID("Name", null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNameNotNull()
		{
			new MutexID(null, "123456789012345678901234567890");
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestNameNotEmpty()
		{
			new MutexID("", "123456789012345678901234567890");
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestReasonIsLongEnough()
		{
			new MutexID("Name", "12345678901234567890123456789");
		}

		public void TestReasonAndNameSet()
		{
			MutexID iD = new MutexID("Name", "123456789012345678901234567890");
			AssertEquals("Name set", "Name", iD.Name);
			AssertEquals("Reason set", "123456789012345678901234567890", iD.Reason);
		}
	}
}
