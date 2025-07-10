using System;
using System.Linq;
using CargoWise.Common;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class ExceptionSourceTest
	{
		[Test]
		public void TestCriticalExceptions()
		{
			var result = ExceptionSource.CriticalExceptions
				.Where(exception => !exception.IsCriticalException());

			Assert.That(result, Is.EquivalentTo(Enumerable.Empty<Exception>()));
		}

		[Test]
		public void TestNotCriticalExceptions()
		{
			var result = ExceptionSource.NotCriticalExceptions
				.Where(exception => exception.IsCriticalException());

			Assert.That(result, Is.EquivalentTo(Enumerable.Empty<Exception>()));
		}
	}
}
