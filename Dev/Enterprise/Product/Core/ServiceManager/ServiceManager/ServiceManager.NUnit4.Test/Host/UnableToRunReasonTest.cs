using System;
using System.Linq;
using Enterprise.ServiceManager.Host.Testing.Helpers;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host.Testing
{
	class UnableToRunReasonTest
	{
		[Test]
		public void TestReasons()
		{
			Assert.Multiple(() =>
			{
				foreach (var (reason, logLevel, message) in UnableToRunReasonHelper.Source)
				{
					Assert.That(reason.LogInfo().LogLevel, Is.EqualTo(logLevel));
					Assert.That(reason.LogInfo().Message, Is.EqualTo(message));
				}
			});
		}

		[Test]
		public void TestAllReasonsAreTested()
		{
			// Arrange
			var testedValues = UnableToRunReasonHelper.Source.Select(tuple => tuple.reason);
			var allValues = Enum.GetValues(typeof(UnableToRunReason)).Cast<UnableToRunReason>();

			// Act
			var result = allValues.Except(testedValues);

			// Assert
			Assert.That(result, Is.EquivalentTo(Enumerable.Empty<UnableToRunReason>()));
		}

		[Test]
		public void TestConversionToFailureReasonType()
		{
			Assert.Multiple(() =>
			{
				AssertConversionToFailureReasonType(
					UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock,
					FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock);
				AssertConversionToFailureReasonType(
					UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock,
					FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock);
				AssertConversionToFailureReasonType(
					UnableToRunReason.RunnerWasCancelled,
					FailureReasonType.RunnerWasCancelled);
				Assert.Throws<ArgumentOutOfRangeException>(() => _ = UnableToRunReasonHelper.ConvertToFailureReasonType(UnableToRunReason.ConfigurationError));
				Assert.Throws<ArgumentOutOfRangeException>(() => _ = UnableToRunReasonHelper.ConvertToFailureReasonType(UnableToRunReason.LockToUpdateNextRunTimeFailed));
				Assert.Throws<ArgumentOutOfRangeException>(() => _ = UnableToRunReasonHelper.ConvertToFailureReasonType(UnableToRunReason.RunnerProcessExited));
				Assert.Throws<ArgumentOutOfRangeException>(() => _ = UnableToRunReasonHelper.ConvertToFailureReasonType(null));
			});
		}

		void AssertConversionToFailureReasonType(UnableToRunReason unableToRunReason, FailureReasonType expectedFailureReasonType)
		{
			var failureReasonType = UnableToRunReasonHelper.ConvertToFailureReasonType(unableToRunReason);
			Assert.That(failureReasonType, Is.EqualTo(expectedFailureReasonType));
		}
	}
}
