using System;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	abstract class EnvironmentCorruptedException<T> where T : EnvironmentCorruptedException
	{
		[Test]
		public void TestMessage()
		{
			var dbUpgraderException = (T)Activator.CreateInstance(typeof(T), new object[] { nameof(TestMessage) });
			Assert.That(dbUpgraderException.Message, Is.EqualTo(nameof(TestMessage)));
		}
	}
}
