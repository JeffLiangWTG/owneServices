using System;
using NUnit.Framework;
using ServiceManager.Host.CW;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.StartupCommand
{
	public class RunControllerStartupCommandTests
	{
		[Test]
		public void TestWrongConstructorParams()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => new RunControllerStartupCommand(null));
			Assert.That(exception?.ParamName, Is.EqualTo("controllerService"));
		}
	}
}
