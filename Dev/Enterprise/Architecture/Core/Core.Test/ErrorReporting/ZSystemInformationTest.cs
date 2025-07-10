using System;
using System.ComponentModel;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ZSystemInformationTest : TransactionedTestCase
	{
		void CompareAndMakeAssertionBetween(string typeOfMemory, UInt64 value, uint minimumValueInMb, uint maximumValueInGb)
		{
			string message = typeOfMemory + " should be greater than " + minimumValueInMb + "MB";
			Assertion.Assert(message, value > minimumValueInMb);
			message = typeOfMemory + " should be less than " + maximumValueInGb + "GB";
			Assertion.Assert(message, value / 1024 < maximumValueInGb);
		}

		public void TestRightValuesForMemory64Bit()
		{
			ZSystemInformation testSysInfo = ZSystemInformation.Instance;

			CompareAndMakeAssertionBetween("Available Physical Memory", testSysInfo.AvailablePhysicalMemory, 1, 128);
			CompareAndMakeAssertionGreaterThan("Available Virtual Memory", testSysInfo.AvailableVirtualMemory, 65536);
			CompareAndMakeAssertionGreaterThan("Total Virtual Memory", testSysInfo.TotalVirtualMemory, 65536);
			CompareAndMakeAssertionBetween("Committed System Wide Memory", testSysInfo.CommittedTotalSystemWide, 1, 256);
			CompareAndMakeAssertionBetween("Commit Limit System Wide Memory", testSysInfo.CommittLimitSystemWide, 1, 256);
			CompareAndMakeAssertionBetween("Committable Available System Wide Memory", testSysInfo.CommittableAvailableSystemWide, 1, 256);
		}

		void CompareAndMakeAssertionGreaterThan(string typeOfMemory, UInt64 value, uint minimumValueInMb)
		{
			string message = typeOfMemory + " should be greater than " + minimumValueInMb + "MB";
			Assertion.Assert(message, value > minimumValueInMb);
		}

		public void TestRightValuesForPagefileSize()
		{
			ZSystemInformation testSysInfo = ZSystemInformation.Instance;
			Assertion.Assert(" Insufficient Pagefile", testSysInfo.AvailablePageFileSize > 1);
		}

		public void WrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new ZSystemInformation(null));
				AssertEquals("systemAdapter", result.ParamName);
			});
		}

		public class AvailableDiskSpaceTest : TestCase
		{
			public void TestRightValuesForAvailableDiskSpace()
			{
				var testSysInfo = ZSystemInformation.Instance;
				Assert("Available disk space should be greater than " + (ulong)1 + "MB", testSysInfo.AvailableFreeSpace > 1);
				Assert("Available disk space should be less than " + (ulong)2000 + "GB", testSysInfo.AvailableFreeSpace / 1024 < 2000);
			}

			public void TestAvailableDiskSpace()
			{
				CombineAssertions(() =>
				{
					Test(10 * 1024 * 1024, 10);
					Test(100 * 1024 * 1024, 100);
				});

				void Test(ulong value, ulong expected)
				{
					// Arrange
					var systemAdapterMock = new Mock<ZSystemInformation.ISystemAdapter>();
					var systemInformation = new ZSystemInformation(systemAdapterMock.Object);

					var freeBytesAvailable = value;
					systemAdapterMock
						.Setup(adapter => adapter.GetDiskFreeSpace(out freeBytesAvailable))
						.Returns(true);

					// Act
					var result = systemInformation.AvailableFreeSpace;

					// Assert
					AssertEquals(expected, result);
					systemAdapterMock.Verify(adapter => adapter.GetDiskFreeSpace(out freeBytesAvailable), Times.Once);
				}
			}

			public void TestAvailableDiskSpaceReturnsFalse()
			{
				// Arrange
				var systemAdapterMock = new Mock<ZSystemInformation.ISystemAdapter>();
				var systemInformation = new ZSystemInformation(systemAdapterMock.Object);

				var freeBytesAvailable = 0UL;
				systemAdapterMock
					.Setup(adapter => adapter.GetDiskFreeSpace(out freeBytesAvailable))
					.Returns(false);

				// Act
				// Assert
				AssertExceptionThrown<Win32Exception>(() => _ = systemInformation.AvailableFreeSpace);
				systemAdapterMock.Verify(adapter => adapter.GetDiskFreeSpace(out freeBytesAvailable), Times.Once);
			}

			public void TestIsThereSufficientFreeDiskSpace_No()
			{
				CombineAssertions(() =>
				{
					Test(10);
					Test(100);
				});

				void Test(ulong value)
				{
					// Arrange
					var systemInformationMock = new Mock<ZSystemInformation> { CallBase = true };
					systemInformationMock
						.Protected()
						.Setup<ulong>("AvailableFreeSpaceCore")
						.Returns(value);

					// Act
					var result = systemInformationMock.Object.IsThereSufficientFreeDiskSpace(out var availableFreeSpace);

					// Assert
					AssertEquals(false, result);
					AssertEquals(value, availableFreeSpace);
					systemInformationMock
						.Protected()
						.Verify<ulong>("AvailableFreeSpaceCore", Times.Once());
				}
			}

			public void TestIsThereSufficientFreeDiskSpace_Yes()
			{
				CombineAssertions(() =>
				{
					Test(ZSystemInformation.MinimumRequirements.DiskFreeSpaceInMB + 100);
					Test(ZSystemInformation.MinimumRequirements.DiskFreeSpaceInMB + 200);
				});

				void Test(ulong value)
				{
					// Arrange
					var systemInformationMock = new Mock<ZSystemInformation> { CallBase = true };
					systemInformationMock
						.Protected()
						.Setup<ulong>("AvailableFreeSpaceCore")
						.Returns(value);

					// Act
					var result = systemInformationMock.Object.IsThereSufficientFreeDiskSpace(out var availableFreeSpace);

					// Assert
					AssertEquals(true, result);
					AssertEquals(value, availableFreeSpace);
					systemInformationMock
						.Protected()
						.Verify<ulong>("AvailableFreeSpaceCore", Times.Once());
				}
			}
		}
	}
}
