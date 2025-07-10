#region Test
#if DEBUG

namespace CargoWise.Bi.Common.Testing
{
	using NUnit.Framework;

	public class BiConstantsTest : TestCase
	{
		public void TestPrefixesHaveFixedLength()
		{
			CombineAssertions("EDW table prefixes should have a fixed length of 5. When changing the length, [Transform].[InitialLoadTable] should be changed as well.", () =>
			{
				AssertEquals("EdwBaseTablePrefix", 5, BiConstants.EdwBaseTablePrefix.Length);
				AssertEquals("EdwAggregateTablePrefix", 5, BiConstants.EdwAggregateTablePrefix.Length);
				AssertEquals("EdwModelViewPrefix", 5, BiConstants.EdwModelViewPrefix.Length);
			});
		}
	}
}

#endif
#endregion