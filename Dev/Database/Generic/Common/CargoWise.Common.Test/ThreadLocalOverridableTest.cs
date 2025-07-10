using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class ThreadLocalOverridableTest : TestCase
	{
		public void TestThreadLocalOverridable()
		{
			var overridable = new ThreadLocalOverridable<int>();
			overridable.Value = 1;

			var t1 = Task.Run(() =>
			{
				AssertEquals(0, overridable.Value);
				overridable.Value = 10;
			});
			Task.WaitAll(t1);

			AssertEquals(1, overridable.Value);
		}

		public void TestResetDereferencesAllThreadLocals()
		{
			var overridable = new ThreadLocalOverridable<int>();
			overridable.Value = 1;

			var t1 = Task.Run(() =>
			{
				Overridable.ResetAll();
			});
			Task.WaitAll(t1);

			AssertEquals("Value should be reset on every thread.", 0, overridable.Value);
		}
	}
}
