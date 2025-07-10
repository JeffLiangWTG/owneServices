using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class DataContextTest : TestCase
	{
		public void TestEnum()
		{
			new EnumerationChecker().CheckEnums(typeof(Constants.DataContext), 35);
		}
	}
}
