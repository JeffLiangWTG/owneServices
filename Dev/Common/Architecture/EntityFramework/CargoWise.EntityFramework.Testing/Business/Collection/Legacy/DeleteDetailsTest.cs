using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DeleteDetailsTest : TestCase
	{
		public void TestDisallow()
		{
			AssertEquals("Set message", "reason", new DeleteDetails.Disallow("reason").Reason);
		}

		public void TestCanDelete()
		{
			Assert("Can delete", !new DeleteDetails.Disallow("blah").CanDelete);
			Assert("Can't delete", new DeleteDetails.Allow().CanDelete);
		}
	}
}
