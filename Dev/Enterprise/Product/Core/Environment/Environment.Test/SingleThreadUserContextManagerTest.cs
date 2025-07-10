using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	[TestedType(typeof(SessionUserContextManager))]
	sealed class SingleThreadUserContextManagerTest : UserContextManagerTest<SessionUserContextManager>
	{
		protected override SessionUserContextManager GetNewUserContextManager()
		{
			return new SessionUserContextManager();
		}
	}
}
