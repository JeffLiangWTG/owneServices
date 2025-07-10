using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	class ServiceTaskRunResultTest
	{
		[Test]
		public void TestEnvironmentCorruptedIgnoreReleaselockError()
		{
			Assert.That(ServiceTaskRunResult.EnvironmentCorrupted.HasFlag(ServiceTaskRunResult.IgnoreReleaseLockError), Is.True);
		}

		[Test]
		public void TestNotSuccessServiceTaskRunResult()
		{
			foreach (
				var result
				in Enum.GetValues(typeof(ServiceTaskRunResult)).Cast<ServiceTaskRunResult>().Except(new[] { ServiceTaskRunResult.Success }))
			{
				Assert.That(result, Is.Not.EqualTo(ServiceTaskRunResult.Success));
			}
		}

		[Test]
		public void TestExpectedValues()
		{
			Assert.Multiple(() =>
			{
				foreach (var (result, value) in ServiceTaskRunResults)
				{
					Assert.That((int)result, Is.EqualTo(value), result.ToString("G"));
				}
			});
		}

		[Test]
		public void TestAllValuesAreIncluded()
		{
			// Arrange
			var expectedValues = Enum
				.GetValues(typeof(ServiceTaskRunResult))
				.Cast<ServiceTaskRunResult>();

			// Act
			var result = expectedValues
				.Except(ServiceTaskRunResults.Select(tuple => tuple.result));

			// Assert
			Assert.That(result, Is.EquivalentTo(Enumerable.Empty<ServiceTaskRunResult>()));
		}

		static readonly IEnumerable<(ServiceTaskRunResult result, int value)> ServiceTaskRunResults = new (ServiceTaskRunResult result, int value)[]
		{
			(ServiceTaskRunResult.Success, 0x0000),
			(ServiceTaskRunResult.UnhandledException, 0x0002),
			(ServiceTaskRunResult.Cancelled, 0x0004),
			(ServiceTaskRunResult.EnvironmentCorrupted, unchecked((int)0x8000_0008)),
			(ServiceTaskRunResult.ServiceTaskLockNotAcquired, 0x0010),
			(ServiceTaskRunResult.GroupLockNotAcquired, 0x0020),
			(ServiceTaskRunResult.LockNotReleased, unchecked((int)0x8000_0040)),
			(ServiceTaskRunResult.IgnoreReleaseLockError, unchecked((int)0x8000_0000)),
		};
	}
}
