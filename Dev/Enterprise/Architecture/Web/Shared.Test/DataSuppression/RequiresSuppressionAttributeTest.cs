using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Shared.Testing
{
	sealed class RequiresSuppressionAttributeTest : TestCase
	{
		#region Test Cases

		public void TestPropertiesWithRequiresSuppressionAttribute()
		{
			foreach (var info in typeof(Dummy).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				AssertEquals(info.Name == "Test2" || info.Name == "Test4" ? 1 : 0, info.GetCustomAttributes(typeof(RequiresSuppressionAttribute), false).Length);
			}
		}

		#endregion

		#region Test Classes

		class Dummy
		{
			public int Test1 { get; set; }

			[RequiresSuppression]
			public int Test2 { get; set; }

			public string Test3 { get; set; }

			[RequiresSuppression]
			public string Test4 { get; set; }
		}

		#endregion
	}
}
