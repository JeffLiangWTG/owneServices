using System.Reflection;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DriveMapperTest : TestCase
	{
		public void TestConnectNetworkPath_Success()
		{
			Mapper.ConnectReturnCode = DriveMapper.NO_ERROR;
			Assert("Connected Successfully", Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertMapperState(TestNetPath, false, "", DriveMapper.NO_ERROR);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_ReUsing()
		{
			Mapper.ConnectReturnCode = DriveMapper.ERROR_SESSION_CREDENTIAL_CONFLICT;
			Assert("Connected Successfully", Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertMapperState(TestNetPath, true, "", DriveMapper.ERROR_SESSION_CREDENTIAL_CONFLICT);
			AssertNull("No Exception", Mapper.CaughtException);

			Mapper.ConnectReturnCode = DriveMapper.NO_ERROR;
			Mapper.DisconnectReturnCode = DriveMapper.NO_ERROR;
			Assert("Connected Successfully", Mapper.ConnectNetworkPath(TestNetPath, "", "", true));

			AssertMapperState(TestNetPath, false, "", DriveMapper.NO_ERROR);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_CredentialsConflict()
		{
			Mapper.ConnectReturnCode = DriveMapper.ERROR_SESSION_CREDENTIAL_CONFLICT;
			Mapper.DisconnectReturnCode = DriveMapper.NO_ERROR;
			AssertEquals("Unable to connected", false, Mapper.ConnectNetworkPath(TestNetPath, "", "", true));

			AssertMapperState(TestNetPath, false, string.Format(DriveMapper.SessionCredentialConflictMessage, TestNetPath, Constants.ProductName), DriveMapper.ERROR_SESSION_CREDENTIAL_CONFLICT);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_BadNetPath()
		{
			Mapper.ConnectReturnCode = DriveMapper.ERROR_BAD_NETPATH;
			AssertEquals("Unable to connected", false, Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertMapperState(TestNetPath, false, string.Format(DriveMapper.BadNetPathMessage, TestNetPath, Constants.ProductName), DriveMapper.ERROR_BAD_NETPATH);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_AccessDenied()
		{
			Mapper.ConnectReturnCode = DriveMapper.ERROR_ACCESS_DENIED;
			AssertEquals("Unable to connected", false, Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertMapperState(TestNetPath, false, string.Format(DriveMapper.AccessDeniedMessage, TestNetPath, Constants.ProductName), DriveMapper.ERROR_ACCESS_DENIED);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_InvalidPassword()
		{
			Mapper.ConnectReturnCode = DriveMapper.ERROR_INVALID_PASSWORD;
			AssertEquals("Unable to connected", false, Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertMapperState(TestNetPath, false, DriveMapper.InvalidPasswordMessage, DriveMapper.ERROR_INVALID_PASSWORD);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_LogonFailure()
		{
			Mapper.ConnectReturnCode = DriveMapper.ERROR_LOGON_FAILURE;
			AssertEquals("Unable to connect", false, Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertMapperState(TestNetPath, false, string.Format(DriveMapper.LogonFailureMessage, TestNetPath), DriveMapper.ERROR_LOGON_FAILURE);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_OtherReturnCode()
		{
			Mapper.ConnectReturnCode = 100;
			AssertEquals("Unable to connected", false, Mapper.ConnectNetworkPath(TestNetPath, "", "", false));

			AssertNotNull("Should be Exception", Mapper.CaughtException);
			Assert("Win32 Exception", Mapper.CaughtException is System.ComponentModel.Win32Exception);
			string expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(100).Message);
			AssertMapperState(TestNetPath, false, expectedMessage, 100);
		}

		public void TestConnectNetworkPath_InvalidUserName()
		{
			AssertEquals("Unable to connected", false, Mapper.ConnectNetworkPath(TestNetPath, @"CORP\#Br0ken#", "", false));

			AssertMapperState(TestNetPath, false, string.Format(DriveMapper.BadUserNameMessage, @"CORP\#Br0ken#", Constants.ProductName), DriveMapper.ERROR_NONE_MAPPED);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestConnectNetworkPath_ExistingUserName()
		{
			Assert("Connected Successfully", Mapper.ConnectNetworkPath(TestNetPath, "richard.white", "", false));

			AssertMapperState(TestNetPath, false, "", DriveMapper.NO_ERROR);
			AssertNull("No Exception", Mapper.CaughtException);
		}

		public void TestWin32ErrorHandler()
		{
			AssertMapperMessageForWin32Error("", DriveMapper.NO_ERROR);
			AssertMapperMessageForWin32Error(string.Format(DriveMapper.InUseMessage, Mapper.ShareName, Constants.ProductName), DriveMapper.ERROR_DEVICE_IN_USE, DriveMapper.ERROR_OPEN_FILES);

			AssertMapperMessageForWin32Error(string.Format(DriveMapper.SessionCredentialConflictMessage, Mapper.ShareName, Constants.ProductName), delegate
				 {
					 FieldInfo forceInfo = typeof(DriveMapper).GetField("fForce", BindingFlags.Instance | BindingFlags.NonPublic);
					 AssertNotNull("Force field info", forceInfo);
					 forceInfo.SetValue(Mapper, true);
					 AssertEquals("Force should be set", true, Mapper.Force);
				 }, DriveMapper.ERROR_SESSION_CREDENTIAL_CONFLICT);

			AssertMapperMessageForWin32Error(string.Format(DriveMapper.BadNetPathMessage, Mapper.ShareName, Constants.ProductName), DriveMapper.ERROR_BAD_NET_NAME, DriveMapper.ERROR_BAD_PROVIDER, DriveMapper.ERROR_NO_NET_OR_BAD_PATH, DriveMapper.ERROR_BAD_NETPATH);
			AssertMapperMessageForWin32Error(string.Format(DriveMapper.AccessDeniedMessage, Mapper.ShareName, Constants.ProductName), DriveMapper.ERROR_ACCESS_DENIED);
			AssertMapperMessageForWin32Error(DriveMapper.InvalidPasswordMessage, DriveMapper.ERROR_INVALID_PASSWORD);
			AssertMapperMessageForWin32Error(string.Format(DriveMapper.LogonFailureMessage, Mapper.ShareName), DriveMapper.ERROR_LOGON_FAILURE);
			AssertMapperMessageForWin32Error(string.Format(DriveMapper.BadUserNameMessage, Mapper.UserName, Constants.ProductName), DriveMapper.ERROR_BAD_PROFILE, DriveMapper.ERROR_BAD_USERNAME, DriveMapper.ERROR_CANNOT_OPEN_PROFILE, DriveMapper.ERROR_NONE_MAPPED);

			string expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_ALREADY_ASSIGNED).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_ALREADY_ASSIGNED);
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_BAD_DEVICE).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_BAD_DEVICE);
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_CANCELLED).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_CANCELLED);
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_DEVICE_ALREADY_REMEMBERED).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_DEVICE_ALREADY_REMEMBERED);
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_EXTENDED_ERROR).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_EXTENDED_ERROR);
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_NO_NETWORK).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_NO_NETWORK);
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(DriveMapper.ERROR_BUSY).Message);
			AssertMapperMessageForWin32Error(expectedMessage, DriveMapper.ERROR_BUSY);

			// Using any error code to test default
			expectedMessage = string.Format(DriveMapper.OtherErrorMessage, "connecting", Mapper.ShareName, Constants.ProductName, new System.ComponentModel.Win32Exception(100).Message);
			AssertMapperMessageForWin32Error(expectedMessage, 100);
		}

		#region Implementation

		void AssertMapperState(string psNetPath, bool pfReusing, string psMessage, int piReturnCode)
		{
			AssertEquals(string.Format("ShareName should be ''{0}''", psNetPath), psNetPath, Mapper.ShareName);
			AssertEquals("Reusing should be " + pfReusing.ToString(), pfReusing, Mapper.ReUsing);
			AssertEquals(string.Format("Message should be ''{0}''", psMessage), psMessage, Mapper.Message);
			AssertEquals("ReturnCode should be " + piReturnCode.ToString(), piReturnCode, Mapper.ReturnCode);
		}

		void AssertMapperMessageForWin32Error(string message, params int[] errorCodes)
		{
			AssertMapperMessageForWin32Error(message, null, errorCodes);
		}

		delegate void SetupDelegate();

		void AssertMapperMessageForWin32Error(string message, SetupDelegate mapperSetup, params int[] errorCodes)
		{
			foreach (int errorCode in errorCodes)
			{
				Mapper.ClearStateForTest();
				if (mapperSetup != null)
				{
					mapperSetup();
				}

				Mapper.HandleReturnCode(errorCode);
				AssertEquals("Message should be as expected", message, Mapper.Message);
			}
		}

		DriveMapperTestHelper Mapper
		{
			get
			{
				if (fMapper == null)
				{
					fMapper = new DriveMapperTestHelper();
				}
				return fMapper;
			}
		}

		DriveMapperTestHelper fMapper;

		protected override void TearDown()
		{
			base.TearDown();
			fMapper = null;
		}

		string TestNetPath
		{
			get { return "SomeNetworkPath"; }
		}

		#region TestHelperClass

		public class DriveMapperTestHelper : DriveMapper
		{
			protected override int WNetAddConnectionWrapper(ref structNetResource pstNetRes, string psPassword, string psUsername, int piFlags)
			{
				Mode = MapperMode.Connect;
				return ConnectReturnCode;
			}

			protected override int WNetCancelConnectionWrapper(string psName, int piFlags, int pfForce)
			{
				Mode = MapperMode.Disconnect;
				return DisconnectReturnCode;
			}

			public int ConnectReturnCode
			{
				get { return fConnectReturnCode; }
				set { fConnectReturnCode = value; }
			}

			int fConnectReturnCode;

			public int DisconnectReturnCode
			{
				get { return fDisconnectReturnCode; }
				set { fDisconnectReturnCode = value; }
			}

			int fDisconnectReturnCode;

			public void ClearStateForTest()
			{
				ClearState();
			}

			public void HandleReturnCode(int errorCode)
			{
				Win32ErrorHandler(errorCode);
			}
		}

		#endregion

		#endregion

	}
}
