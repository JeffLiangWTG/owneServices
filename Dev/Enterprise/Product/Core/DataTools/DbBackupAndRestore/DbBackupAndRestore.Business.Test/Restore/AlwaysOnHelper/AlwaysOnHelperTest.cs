using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	internal class AlwaysOnHelperTest : TestCase
	{
		readonly AlwaysOnHelper helper = new AlwaysOnHelper();

		public void TestAddDomainToServerNamesWithNoInstanceName()
		{
			List<string> serverNames = new List<string> { "server1", "server2" };
			var domainName = "test.domain";
			List<string> expected = new List<string> { "server1.test.domain", "server2.test.domain" };
			List<string> actual = helper.AddDomainToServerNames(serverNames, domainName);

			Assert(expected.SequenceEqual(actual));
		}

		public void TestAddDomainToServerNamesWithInstanceNames()
		{
			List<string> serverNames = new List<string> { "server1\\INSTANCE1", "server2\\INSTANCE2" };
			var domainName = "test.domain";
			List<string> expected = new List<string> { "server1.test.domain\\INSTANCE1", "server2.test.domain\\INSTANCE2" };
			List<string> actual = helper.AddDomainToServerNames(serverNames, domainName);

			Assert(expected.SequenceEqual(actual));
		}
	}
}
