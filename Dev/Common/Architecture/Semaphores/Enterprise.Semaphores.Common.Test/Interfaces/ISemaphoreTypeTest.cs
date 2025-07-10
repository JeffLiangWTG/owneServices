using NUnit.Framework;

namespace Enterprise.Semaphores.Common.Testing
{
	public interface ISemaphoreTypeTest
	{
		[TestedType(typeof(CommonSemaphoreType))]
		internal class Test : SemaphoreTypeTestCase
		{
			protected override ISemaphoreType TestSemaphore
			{
				get { return new CommonSemaphoreType("LockInfo", "ABC", 0); }
			}
		}
	}
}
