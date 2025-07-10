using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	public class IsRedirectedChangingEventArgsTest : TestCase
	{
		public void TestConstructor()
		{
			IsRedirectedChangingEventArgs args = new IsRedirectedChangingEventArgs(true);
			AssertEquals(true, args.IsRedirected);
		}
	}
}
