using System;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Loader.Common.Native;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class UserAccountControlTest : TestCase
	{
		MockRepository mocker;
		MoqMockServiceContainer services;
		UserAccountControl uac;

		protected override void SetUp()
		{
			base.SetUp();
			uac = new UserAccountControl(new MockVersionHelper(6, 0));
			mocker = new MockRepository(MockBehavior.Default);
			services = new MoqMockServiceContainer(mocker);
			uac.Services = services;
		}

		public void TestCanElevateNotVista()
		{
			AssertEquals("CanElevate", expected: false, new UserAccountControl(new MockVersionHelper(5, 0)).CanElevate);
		}

		public void TestCanElevateNotSplitToken()
		{
			ExpectElevationType(TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault);
			AssertEquals("CanElevate", expected: false, uac.CanElevate);
			mocker.VerifyAll();
		}

		public void TestCanElevateSplitToken()
		{
			ExpectElevationType(TOKEN_ELEVATION_TYPE.TokenElevationTypeLimited);
			AssertEquals("CanElevate", expected: true, uac.CanElevate);
			mocker.VerifyAll();
		}

		public void TestCanElevateRegistryEnabled()
		{
			services.NativeMethodsForTest.Setup(m => m.GetCurrentProcess()).Returns(IntPtr.Zero);
			services.RegistryForTest.Setup(m => m.GetValue(UserAccountControl.RegistryKeyName, UserAccountControl.RegistryValueName, null)).Returns(null);
			AssertEquals("CanElevate", expected: false, uac.CanElevate);
			mocker.VerifyAll();
		}

		public void TestCanElevateRegistryNotEnabled()
		{
			services.NativeMethodsForTest.Setup(m => m.GetCurrentProcess()).Returns(IntPtr.Zero);
			services.RegistryForTest.Setup(m => m.GetValue(UserAccountControl.RegistryKeyName, UserAccountControl.RegistryValueName, null)).Returns(1);
			AssertEquals("CanElevate", expected: true, uac.CanElevate);
			mocker.VerifyAll();
		}

		public void TestElevationRequiredIsAdmin()
		{
			using (AdministratorChecker.OverrideForTest(true))
			{
				AssertEquals("ElevationRequired", expected: false, uac.ElevationRequired);
			}
		}

		public void TestElevationRequiredIsNotAdminAndUacEnabled()
		{
			using (AdministratorChecker.OverrideForTest(false))
			{
				ExpectElevationType(TOKEN_ELEVATION_TYPE.TokenElevationTypeLimited);
				AssertEquals("ElevationRequired", expected: true, uac.ElevationRequired);
				mocker.VerifyAll();
			}
		}

		public void TestElevationRequiredIsNotAdminAndUacDisabled()
		{
			using (AdministratorChecker.OverrideForTest(false))
			{
				ExpectElevationType(TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault);
				AssertEquals("ElevationRequired", expected: false, uac.ElevationRequired);
				mocker.VerifyAll();
			}
		}

		delegate bool OpenProcessTokenReturnsDelegate(IntPtr processHandle, uint desiredAccess, out int tokenHandle);
		delegate bool GetTokenInformationReturnsDelegate(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);

		void ExpectElevationType(TOKEN_ELEVATION_TYPE result)
		{
			int dummyInt;
			uint dummyUInt;

			services.NativeMethodsForTest.Setup(m => m.GetCurrentProcess()).Returns(new IntPtr(-1));
			services.NativeMethodsForTest.Setup(m => m.OpenProcessToken(new IntPtr(-1), NativeMethods.TOKEN_QUERY, out dummyInt))
				.Returns(new OpenProcessTokenReturnsDelegate((IntPtr processHandle, uint desiredAccess, out int tokenHandle) =>
				{
					tokenHandle = 789;
					return true;
				}));

			services.NativeMethodsForTest.Setup(m => m.GetTokenInformation(It.IsAny<IntPtr>(), It.IsAny<TOKEN_INFORMATION_CLASS>(), It.IsAny<IntPtr>(), It.IsAny<uint>(), out dummyUInt))
				.Returns(new GetTokenInformationReturnsDelegate((IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength) =>
				{
					AssertEquals("TokenHandle", 789, tokenHandle.ToInt32());
					AssertEquals("TokenInformationClass", TOKEN_INFORMATION_CLASS.TokenElevationType, tokenInformationClass);
					Assert("TokenInformation should be allocated.", tokenInformation != IntPtr.Zero);
					AssertEquals("TokenInformationLength", 4, tokenInformationLength);
					returnLength = 4;
					Marshal.WriteInt32(tokenInformation, (int)result);
					return true;
				}));

			services.NativeMethodsForTest.Setup(m => m.CloseHandle(new IntPtr(789)));
		}

		sealed class MockVersionHelper : VersionHelper
		{
			public MockVersionHelper(int majorVersion, int minorVersion, int servicePackVersion = 0, bool isWindowsServer = false)
			{
				mockMajorVersion = majorVersion;
				mockMinorVersion = minorVersion;
				mockServicePackVersion = servicePackVersion;
				mockIsWindowsServer = isWindowsServer;
			}

			readonly int mockMajorVersion;
			readonly int mockMinorVersion;
			readonly int mockServicePackVersion;
			readonly bool mockIsWindowsServer;

			public override bool IsWindowsVersionOrGreater(int majorVersion, int minorVersion, short servicePackMajor)
			{
				if (mockMajorVersion > majorVersion)
				{
					return true;
				}
				else if (mockMajorVersion < majorVersion)
				{
					return false;
				}

				if (mockMinorVersion > minorVersion)
				{
					return true;
				}
				else if (mockMinorVersion < minorVersion)
				{
					return false;
				}

				return mockServicePackVersion >= servicePackMajor;
			}

			public override bool IsWindowsServer()
			{
				return mockIsWindowsServer;
			}
		}
	}
}
