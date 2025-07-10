using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.RunnableTask
{
	class RunnableServiceTaskHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDisabledTasks()
		{
			const string testTaskCode = "ABC";
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(testTaskCode, true);
				Test($"{testTaskCode}|DEF", true);
				Test(string.Empty, false);
			});

			void Test(string value, bool expectedResult)
			{
				var realResult = false;
				realResult = RunnableServiceTaskHelper.IsTaskDisabled(testTaskCode, value);

				NUnit.Framework.Assert.That(realResult, Is.EqualTo(expectedResult), $"{value} should disable task [{testTaskCode}]");
			}
		}
	}
}
