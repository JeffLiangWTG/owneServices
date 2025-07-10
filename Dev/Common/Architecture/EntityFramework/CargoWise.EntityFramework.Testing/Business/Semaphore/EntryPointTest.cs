using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class EntryPointTest : TestCase
	{
		public void TestEntryState()
		{
			using (var entryState = entryPoint.GetState())
			{
				AssertEquals("entryState.IsAllowed", true, entryState.IsAllowed);
				using (var nestedEntryState = entryPoint.GetState())
				{
					AssertEquals("nestedEntryState.IsAllowed", false, nestedEntryState.IsAllowed);
					AssertEquals("entryState.IsAllowed", true, entryState.IsAllowed);
				}
				AssertEquals("entryState.IsAllowed", true, entryState.IsAllowed);
			}

			using (var entryState = entryPoint.GetState())
			{
				AssertEquals("entryState.IsAllowed", true, entryState.IsAllowed);
			}
		}

		readonly EntryPoint entryPoint = new EntryPoint();
	}
}
