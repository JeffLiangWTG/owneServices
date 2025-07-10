using System.Diagnostics;
using CargoWise.Setup.Services;
using NUnit.Framework;

namespace CargoWise.Setup.Test.Services;

internal class ProcessRunnerProxyTest
{
	[Test]
	public void TestProcessWrapperWhenProcessNotPresent()
	{
		var wrapper = new ProcessWrapper(null);

		Assert.That(wrapper.ExitCode, Is.EqualTo(-1));
		Assert.That(wrapper.Present, Is.False);
	}

	[Test]
	public void TestProcessWrapperWhenProcessPresent()
	{
		var real = new Process();
		var wrapper = new ProcessWrapper(real);

		Assert.That(wrapper.Present, Is.True);
	}
}
