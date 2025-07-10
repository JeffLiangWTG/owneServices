using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Helpers.Registration
{
	class ProductRegistrationPeriodicCheckerTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			memoryCacheMock = new Mock<IMemoryCache>();
			hostLoggerMock = new Mock<IHostLogger>();
			productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationPeriodicChecker = new ProductRegistrationPeriodicChecker(memoryCacheMock.Object, hostLoggerMock.Object, productRegistrationMock.Object);
		}

		protected override void TearDown()
		{
			ObjectFactory.DisposeSubstitutions();
			base.TearDown();
		}

		[ExpectNoExceptions]
		public void TestConstants()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, Is.EqualTo("ProductRegistrationPeriodicChecker_LocalVerification"));
				NUnit.Framework.Assert.That(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, Is.EqualTo("ProductRegistrationPeriodicChecker_RemoteVerification"));
				NUnit.Framework.Assert.That(ProductRegistrationPeriodicChecker.MemoryCacheUnregisteredProductLogReference, Is.EqualTo("ProductRegistrationPeriodicChecker_UnRegisteredProductLogVerification"));
				NUnit.Framework.Assert.That(ProductRegistrationPeriodicChecker.MemoryCacheLocalExpiration, Is.EqualTo(TimeSpan.FromMinutes(10)));
				NUnit.Framework.Assert.That(ProductRegistrationPeriodicChecker.MemoryCacheRemoteExpiration, Is.EqualTo(TimeSpan.FromMinutes(30)));
				NUnit.Framework.Assert.That(ProductRegistrationPeriodicChecker.MemoryCacheUnregisteredProductExpiration, Is.EqualTo(TimeSpan.FromMinutes(1)));
			});
		}

		[ExpectNoExceptions]
		public void TestReturnsFalseForWiseCloudTrial()
		{
			// Arrange
			memoryCacheMock.Reset();

			memoryCacheMock
				.Setup(cache => cache.AddOrGetExisting(It.IsAny<string>(), It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);

			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock
				.SetupGet(key => key.DatabaseType)
				.Returns(DatabaseTypes.Codes.WisecloudTrial);

			// Act
			var result = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(false));
			productRegistrationMock.VerifyGet(registration => registration.Key, Times.Once);
			productRegistrationKeyMock.VerifyGet(key => key.DatabaseType, Times.Once);
		}

		[ExpectNoExceptions]
		public void TestRegistrationResultForLocalVerificationIncludesAllValues()
		{
			// Arrange
			var allValues = Enum
				.GetValues(typeof(ProductRegistrationVerifyResult))
				.Cast<ProductRegistrationVerifyResult>();
			var usedValues = RegistrationResultForLocalVerification
				.Select(tuple => tuple.productRegistrationVerifyResult);

			// Act
			var result = allValues.Except(usedValues);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EquivalentTo(Enumerable.Empty<ProductRegistrationVerifyResult>()));
		}

		[ExpectNoExceptions]
		public void TestRegistrationResultForRemoteVerificationIncludesAllValues()
		{
			// Arrange
			var allValues = Enum
				.GetValues(typeof(ProductRegistrationVerifyResult))
				.Cast<ProductRegistrationVerifyResult>();
			var usedValues = RegistrationResultForRemoteVerification
				.Select(tuple => tuple.productRegistrationVerifyResult);

			// Act
			var result = allValues.Except(usedValues);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EquivalentTo(Enumerable.Empty<ProductRegistrationVerifyResult>()));
		}

		[ExpectNoExceptions]
		public void TestUpdatesValueInCacheForLocalCheckIfRemoteCheckExists()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var (productRegistrationVerifyResult, value) in RegistrationResultForLocalVerification)
				{
					Test(productRegistrationVerifyResult, value);
				}
			});

			void Test(ProductRegistrationVerifyResult productRegistrationVerifyResult, bool expected)
			{
				// Arrange
				memoryCacheMock.Reset();
				productRegistrationMock.Reset();

				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns(true);

				var result = false;
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
					{
						result = func.Invoke();
						_ = expiration.Invoke();
						return result;
					});

				productRegistrationMock
					.Setup(registration => registration.LocalVerify())
					.Returns(productRegistrationVerifyResult);

				// Act
				_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(expected));
				productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Once);
				productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Never);
				memoryCacheMock.Verify(
					cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()),
					Times.Once);
				memoryCacheMock.Verify(
					cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()),
					Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdatesValueInCacheForLocalCheckIfRemoteCheckExists_Exception()
		{
			// Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					_ = expiration.Invoke();
					return result;
				})
				.Verifiable();
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock
				.SetupGet(key => key.DatabaseType)
				.Returns(DatabaseTypes.Codes.Production);
			productRegistrationMock
				.Setup(registration => registration.LocalVerify())
				.Throws<Exception>();

			// Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			// Assert
			productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Once);
			productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Never);
			memoryCacheMock.Verify();
		}

		public void TestUpdatesValueInCacheForLocalCheckIfRemoteCheckExists_DbUpgradeExceptionBubblesUp()
		{
			// Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);

			var localResult = false;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					localResult = func.Invoke();
					_ = expiration();
					return localResult;
				});

			productRegistrationMock
				.Setup(registration => registration.LocalVerify())
				.Throws(new Mock<DatabaseUpgradeException>(":(").Object);

			// Act
			// Assert
			AssertExceptionThrown<DatabaseUpgradeException>(() => _ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown());
			productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Once);
			productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Never);
			NUnit.Framework.Assert.That(localResult, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestUpdatesValueInCacheForRemoteCheckIfLocalCheckExists()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var (productRegistrationVerifyResult, value) in RegistrationResultForRemoteVerification)
				{
					Test(productRegistrationVerifyResult, value);
				}
			});

			void Test(ProductRegistrationVerifyResult productRegistrationVerifyResult, bool expected)
			{
				// Arrange
				memoryCacheMock.Reset();
				productRegistrationMock.Reset();

				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns(true);
				var result = false;
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
					{
						result = func.Invoke();
						_ = expiration();
						return result;
					});
				productRegistrationMock
					.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
					.Returns(productRegistrationVerifyResult);

				// Act
				_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(expected));
				productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Never);
				productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Once);
				productRegistrationMock.Verify(registration => registration.Verify(CancellationToken.None, 20000), Times.Once);
				memoryCacheMock.Verify(
					cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()),
					Times.Once);
				memoryCacheMock.Verify(
					cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()),
					Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdatesValueInCacheForRemoteCheckIfLocalCheckExists_Exception()
		{
			// Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					_ = expiration.Invoke();
					return result;
				})
				.Verifiable();
			productRegistrationMock
				.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
				.Throws<Exception>();

			// Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			// Assert
			productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Never);
			productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Once);
			productRegistrationMock.Verify(registration => registration.Verify(CancellationToken.None, 20000), Times.Once);
			memoryCacheMock.Verify();
		}

		public void TestUpdatesValueInCacheForRemoteCheckIfLocalCheckExists_DbUpgradeExceptionBubblesUp()
		{
			// Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);

			var remoteResult = false;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					remoteResult = func.Invoke();
					_ = expiration.Invoke();
					return remoteResult;
				});
			productRegistrationMock
				.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
				.Throws(new Mock<DatabaseUpgradeException>(":(").Object);

			// Act
			// Assert
			AssertExceptionThrown<DatabaseUpgradeException>(() => _ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown());
			productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Never);
			productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Once);
			productRegistrationMock.Verify(registration => registration.Verify(CancellationToken.None, 20000), Times.Once);
			NUnit.Framework.Assert.That(remoteResult, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestUpdatesValuesInCacheForLocalCheckAndRemoteCheckIfNoneExists()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				foreach (var (localProductRegistrationVerifyResult, localValue) in RegistrationResultForLocalVerification)
					foreach (var (remoteProductRegistrationVerifyResult, remoteValue) in RegistrationResultForRemoteVerification)
					{
						Test(localProductRegistrationVerifyResult, localValue, remoteProductRegistrationVerifyResult, remoteValue);
					}
			});

			void Test(ProductRegistrationVerifyResult localProductRegistrationVerifyResult, bool expectedLocal, ProductRegistrationVerifyResult remoteProductRegistrationVerifyResult, bool expectedRemote)
			{
				// Arrange
				memoryCacheMock.Reset();
				productRegistrationMock.Reset();

				var localResult = false;
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
					{
						localResult = func.Invoke();
						_ = expiration.Invoke();
						return localResult;
					});
				var remoteResult = false;
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
					{
						remoteResult = func.Invoke();
						_ = expiration.Invoke();
						return remoteResult;
					});
				productRegistrationMock
					.Setup(registration => registration.LocalVerify())
					.Returns(localProductRegistrationVerifyResult);
				productRegistrationMock
					.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
					.Returns(remoteProductRegistrationVerifyResult);

				// Act
				_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

				// Assert
				productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Once);
				productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Once);
				productRegistrationMock.Verify(registration => registration.Verify(CancellationToken.None, 20000), Times.Once);
				NUnit.Framework.Assert.That(localResult, Is.EqualTo(expectedLocal));
				NUnit.Framework.Assert.That(remoteResult, Is.EqualTo(expectedRemote));
				memoryCacheMock.Verify(
					cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()),
					Times.Once);
				memoryCacheMock.Verify(
					cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()),
					Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdatesValuesInCacheForLocalCheckAndRemoteCheckIfNoneExists_Exception()
		{
			// Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					_ = expiration.Invoke();
					return result;
				})
				.Verifiable();
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					_ = expiration.Invoke();
					return result;
				})
				.Verifiable();
			productRegistrationMock
				.Setup(registration => registration.LocalVerify())
				.Throws<Exception>();
			productRegistrationMock
				.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
				.Throws<Exception>();

			// Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			// Assert
			productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.Once);
			productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.Once);
			productRegistrationMock.Verify(registration => registration.Verify(CancellationToken.None, 20000), Times.Once);
			memoryCacheMock.Verify();
		}

		public void TestUpdatesValuesInCacheForLocalCheckAndRemoteCheckIfNoneExists_DbUpgradeExceptionBubblesUp()
		{
			// Arrange
			var localResult = false;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					localResult = func.Invoke();
					_ = expiration.Invoke();
					return localResult;
				});
			var remoteResult = false;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					remoteResult = func.Invoke();
					_ = expiration.Invoke();
					return remoteResult;
				});
			productRegistrationMock
				.Setup(registration => registration.LocalVerify())
				.Throws(new Mock<DatabaseUpgradeException>(":(").Object);
			productRegistrationMock
				.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
				.Throws(new Mock<DatabaseUpgradeException>(":(").Object);

			// Act
			// Assert
			AssertExceptionThrown<DatabaseUpgradeException>(() => _ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown());
			productRegistrationMock.Verify(registration => registration.LocalVerify(), Times.AtMostOnce);
			productRegistrationMock.Verify(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()), Times.AtMostOnce);
			NUnit.Framework.Assert.That(localResult, Is.EqualTo(false));
			NUnit.Framework.Assert.That(remoteResult, Is.EqualTo(false));
		}

		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ProductRegistrationPeriodicChecker(null, Mock.Of<IHostLogger>(), Mock.Of<IProductRegistration>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("memoryCache"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ProductRegistrationPeriodicChecker(Mock.Of<IMemoryCache>(), null, Mock.Of<IProductRegistration>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ProductRegistrationPeriodicChecker(Mock.Of<IMemoryCache>(), Mock.Of<IHostLogger>(), null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("productRegistration"));
			});
		}

		[ExpectNoExceptions]
		public void TestMessageShownOnFailedRegistrationCheck()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(true, true, DatabaseTypes.Codes.WisecloudTrial, "System is unregistered (local registration: [True], remote registration: [True], type: [WCT])");
				Test(true, false, DatabaseTypes.Codes.Training, "System is unregistered (local registration: [True], remote registration: [False], type: [TRN])");
				Test(false, true, DatabaseTypes.Codes.Training, "System is unregistered (local registration: [False], remote registration: [True], type: [TRN])");
				Test(false, false, DatabaseTypes.Codes.Training, "System is unregistered (local registration: [False], remote registration: [False], type: [TRN])");
			});

			void Test(bool localCacheResult, bool remoteCacheValue, string trialSystem, string expectedLog)
			{
				// Arrange
				memoryCacheMock.Reset();
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns(localCacheResult);
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns(remoteCacheValue);
				memoryCacheMock
					.Setup(cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheUnregisteredProductLogReference, It.IsAny<Func<object>>(), It.IsAny<TimeSpan>()))
					.Returns<string, Func<object>, TimeSpan>((key, func, expiration) => func.Invoke());

				var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
				productRegistrationMock
					.SetupGet(registration => registration.Key)
					.Returns(productRegistrationKeyMock.Object);
				productRegistrationKeyMock
					.SetupGet(key => key.DatabaseType)
					.Returns(trialSystem);

				// Act
				var result = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(false));
				hostLoggerMock.Verify(x => x.Log(LogLevel.Warning, expectedLog), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestMessageNotShownOnRegisteredSystem()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(true, true, DatabaseTypes.Codes.Production);
				Test(true, true, DatabaseTypes.Codes.Education);
				Test(true, true, DatabaseTypes.Codes.Test);
				Test(true, true, DatabaseTypes.Codes.Training);
				Test(true, true, DatabaseTypes.Codes.Demo);
			});

			void Test(bool localCacheResult, bool remoteCacheValue, string trialSystem)
			{
				// Arrange
				memoryCacheMock.Reset();
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns(localCacheResult);
				memoryCacheMock
					.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
					.Returns(remoteCacheValue);
				var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
				productRegistrationMock
					.SetupGet(registration => registration.Key)
					.Returns(productRegistrationKeyMock.Object);
				productRegistrationKeyMock
					.SetupGet(key => key.DatabaseType)
					.Returns(trialSystem);
				// Act
				var result = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

				// Assert
				NUnit.Framework.Assert.That(result, Is.EqualTo(true));
				hostLoggerMock.VerifyNoOtherCalls();
			}
		}

		[ExpectNoExceptions]
		public void TestRegistrationIsLoggedAfterSettingUnregisteredCache()
		{
			//Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(false);
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(false);
			memoryCacheMock
				.Setup(cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheUnregisteredProductLogReference, It.IsAny<Func<object>>(), It.IsAny<TimeSpan>()))
				.Returns<string, Func<object>, TimeSpan>((key, func, expiration) => func.Invoke());
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock
				.SetupGet(key => key.DatabaseType)
				.Returns(DatabaseTypes.Codes.Production);

			hostLoggerMock.Invocations.Clear();

			//Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			// Assert
			hostLoggerMock.Verify(x => x.Log(LogLevel.Information, "System is registered (local registration: [True], remote registration: [True], type: [PRD])"),
				Times.Once);
			hostLoggerMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestRegistrationIsNotLoggedAfterUnregisteredStateAlreadyInCache()
		{
			//Arrange
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(false);
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(false);
			memoryCacheMock
				.Setup(cache => cache.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheUnregisteredProductLogReference, It.IsAny<Func<object>>(), It.IsAny<TimeSpan>()))
				.Returns(new object());
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns(true);
			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationKeyMock
				.SetupGet(key => key.DatabaseType)
				.Returns(DatabaseTypes.Codes.Production);

			hostLoggerMock.Invocations.Clear();

			//Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			// Assert
			hostLoggerMock.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestUpdatesCacheExpiryForLocalOnLocalPass()
		{
			//Arrange
			var memoryCacheLocalExpiration = TimeSpan.Zero;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					memoryCacheLocalExpiration = expiration.Invoke();
					return result;
				});

			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationMock
				.Setup(registration => registration.LocalVerify())
				.Returns(ProductRegistrationVerifyResult.OK);

			//Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			//Assert
			NUnit.Framework.Assert.That(memoryCacheLocalExpiration, Is.EqualTo(TimeSpan.FromMinutes(10)));
		}

		[ExpectNoExceptions]
		public void TestUpdatesCacheExpiryForLocalOnLocalFail()
		{
			//Arrange
			var memoryCacheLocalExpiration = TimeSpan.Zero;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheLocalReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					memoryCacheLocalExpiration = expiration.Invoke();
					return result;
				});

			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationMock
				.Setup(registration => registration.LocalVerify())
				.Returns(ProductRegistrationVerifyResult.Fail);

			//Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			//Assert
			NUnit.Framework.Assert.That(memoryCacheLocalExpiration, Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		[ExpectNoExceptions]
		public void TestUpdatesCacheExpiryForRemoteOnRemotePass()
		{
			//Arrange
			var memoryCacheRemoteExpiration = TimeSpan.Zero;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					memoryCacheRemoteExpiration = expiration.Invoke();
					return result;
				});

			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationMock
				.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
				.Returns(ProductRegistrationVerifyResult.OK);

			//Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			//Assert
			NUnit.Framework.Assert.That(memoryCacheRemoteExpiration, Is.EqualTo(TimeSpan.FromMinutes(30)));
		}

		[ExpectNoExceptions]
		public void TestUpdatesCacheExpiryForRemoteOnRemoteFail()
		{
			//Arrange
			var memoryCacheRemoteExpiration = TimeSpan.Zero;
			memoryCacheMock
				.Setup(m => m.AddOrGetExisting(ProductRegistrationPeriodicChecker.MemoryCacheRemoteReference, It.IsAny<Func<bool>>(), It.IsAny<Func<TimeSpan>>()))
				.Returns<string, Func<bool>, Func<TimeSpan>>((key, func, expiration) =>
				{
					var result = func.Invoke();
					memoryCacheRemoteExpiration = expiration.Invoke();
					return result;
				});

			var productRegistrationKeyMock = new Mock<IProductRegistrationKey>();
			productRegistrationMock
				.SetupGet(registration => registration.Key)
				.Returns(productRegistrationKeyMock.Object);
			productRegistrationMock
				.Setup(registration => registration.Verify(It.IsAny<CancellationToken>(), It.IsAny<int>()))
				.Returns(ProductRegistrationVerifyResult.Fail);

			//Act
			_ = productRegistrationPeriodicChecker.IsProductRegisteredAsNonTrialSystemOrUnknown();

			//Assert
			NUnit.Framework.Assert.That(memoryCacheRemoteExpiration, Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		static readonly IEnumerable<(ProductRegistrationVerifyResult productRegistrationVerifyResult, bool value)> RegistrationResultForLocalVerification = new[]
		{
			(ProductRegistrationVerifyResult.OK, true),
			(ProductRegistrationVerifyResult.Fail, false),
			(ProductRegistrationVerifyResult.Unregistered, false),
			(ProductRegistrationVerifyResult.NotFound, false),
			(ProductRegistrationVerifyResult.Timeout, false),
			(ProductRegistrationVerifyResult.Error, false),
		};

		static readonly IEnumerable<(ProductRegistrationVerifyResult productRegistrationVerifyResult, bool value)> RegistrationResultForRemoteVerification = new[]
		{
			(ProductRegistrationVerifyResult.OK, true),
			(ProductRegistrationVerifyResult.Fail, false),
			(ProductRegistrationVerifyResult.Unregistered, false),
			(ProductRegistrationVerifyResult.NotFound, false),
			(ProductRegistrationVerifyResult.Timeout, true),
			(ProductRegistrationVerifyResult.Error, true),
		};

		Mock<IMemoryCache> memoryCacheMock;
		Mock<IHostLogger> hostLoggerMock;
		Mock<IProductRegistration> productRegistrationMock;
		ProductRegistrationPeriodicChecker productRegistrationPeriodicChecker;
	}
}
