using System;
using System.Threading;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class HostApplicationLockAcquirerTest : TestCase
	{
		public void TestWrongConstructorParams()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HostApplicationLockAcquirer(null, productRegistrationMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("applicationEmergencyExit"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HostApplicationLockAcquirer(applicationEmergencyExitMock.Object, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("productRegistration"));
			});
		}

		public void TestDoesNotExitOnLockAcquire()
		{
			// Arrange
			using (var hostMutexAcquirer = new HostApplicationLockAcquirer(applicationEmergencyExitMock.Object, productRegistrationMock.Object))
			{
				// Act
				hostMutexAcquirer.AcquireHostApplicationLock();

				// Assert
				AssertNoExceptionThrown(() =>
				{
					using (TryAcquireHostApplicationLock(out var createdNew))
					{
						NUnit.Framework.Assert.That(createdNew, Is.EqualTo(false), $"Failed to acquire the lock: {MutexKey} because it has been acquired by the application");
					}

					applicationEmergencyExitMock.Verify(proxy => proxy
						.ExitApplicationUnsafe(It.IsAny<string>()), Times.Never);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
				});
			}
		}

		public void TestExitsOnLockAcquireFailure()
		{
			// Arrange
			using (TryAcquireHostApplicationLock(out var createdNew))
			using (var hostMutexAcquirer = new HostApplicationLockAcquirer(applicationEmergencyExitMock.Object, productRegistrationMock.Object))
			{
				NUnit.Framework.Assert.That(createdNew, Is.True);

				// Act
				hostMutexAcquirer.AcquireHostApplicationLock();

				// Assert
				AssertNoExceptionThrown(() =>
				{
					applicationEmergencyExitMock.Verify(proxy => proxy
						.ExitApplicationUnsafe(It.Is<string>(message => message.StartsWith("Failed to acquire a lock on"))), Times.Once);
					applicationEmergencyExitMock.VerifyNoOtherCalls();
				});
			}
		}

		[ExpectNoExceptions]
		public void TestAcquiredLockReleasedOnDispose()
		{
			// Arrange
			using (var hostMutexAcquirer = new HostApplicationLockAcquirer(applicationEmergencyExitMock.Object, productRegistrationMock.Object))
			{
				// Act
				hostMutexAcquirer.AcquireHostApplicationLock();
			}

			// Assert
			using (TryAcquireHostApplicationLock(out var createdNew))
			{
				NUnit.Framework.Assert.That(createdNew, Is.EqualTo(true), $"HostApplicationLock can be acquired on key: {MutexKey} after acquired lock is released on dispose");
			}
		}

		[ExpectNoExceptions]
		public void TestAcquiresLock()
		{
			// Arrange
			using (var hostMutexAcquirer = new HostApplicationLockAcquirer(applicationEmergencyExitMock.Object, productRegistrationMock.Object))
			{
				// Act
				hostMutexAcquirer.AcquireHostApplicationLock();

				// Assert
				using (TryAcquireHostApplicationLock(out var createdNew))
				{
					NUnit.Framework.Assert.That(createdNew, Is.EqualTo(false), $"HostApplicationLock: {MutexKey} has been acquired by another instance");
				}
			}
		}

		public void TestNotAcquiredLockCanBeDisposed()
		{
			AssertNoExceptionThrown(() =>
			{
				// Arrange
				using (TryAcquireHostApplicationLock(out var createdNew))
				using (new HostApplicationLockAcquirer(applicationEmergencyExitMock.Object, productRegistrationMock.Object))
				{
					NUnit.Framework.Assert.That(createdNew, Is.EqualTo(true), $"HostApplicationLock: {MutexKey} has been acquired by another instance");

					// Act
				}

				// Assert
			});
		}

		static IDisposable TryAcquireHostApplicationLock(out bool createdNew)
		{
			var semaphore = new Semaphore(1, 1, MutexKey, out createdNew);
			if (createdNew)
			{
				semaphore.WaitOne();
			}

			return semaphore;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKeyForTest>();
			productRegistrationKeyMock
				.SetupGet(proxy => proxy.EnterpriseCode)
				.Returns(TestEnterpriseCode);
			productRegistrationKeyMock
				.SetupGet(proxy => proxy.ServerCode)
				.Returns(TestServerCode);
			productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock
				.SetupGet(proxy => proxy.Key)
				.Returns(productRegistrationKeyMock.Object);
			applicationEmergencyExitMock = new Mock<IApplicationEmergencyExit>();
		}

		Mock<IApplicationEmergencyExit> applicationEmergencyExitMock;
		Mock<IProductRegistration> productRegistrationMock;

		const string TestEnterpriseCode = "TST";
		const string TestServerCode = "SVR";
		const string MutexKey = "ediEnterpriseProcessController_" + TestServerCode + "_" + TestEnterpriseCode;
	}
}
