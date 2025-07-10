using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class RebillFlagChangingEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			RebillFlagChangingEventArgs args = new RebillFlagChangingEventArgs(RebillFlags.IsRTS);
			AssertEquals(RebillFlags.IsRTS, args.RebillFlag);
		}
	}
}
