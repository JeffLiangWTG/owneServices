using System;
using Enterprise.ServiceManager.Runner;
using NUnit.Framework;

namespace CargoWise.ServiceManager.Runner.Test
{
	class ResourceManagementTest
	{
		[Test]
		public void TestWrongConstructorParamsCall()
		{
			Assert.Multiple(() =>
			{
				var result = Assert.Throws<ArgumentNullException>(() => _ = new ResourceManagement(null));
				Assert.That(result.ParamName, Is.EqualTo("runnerLogger"));
			});
		}
	}
}
