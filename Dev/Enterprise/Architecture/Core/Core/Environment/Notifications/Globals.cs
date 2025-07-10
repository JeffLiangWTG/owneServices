using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using CargoWise.Common;
using CargoWise.Common.Testing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Environment
{
	public static class Globals
	{
		#region Debug / Test / Web Flags

		#region IsDebug

		public static bool IsDebugMode
		{
			get
			{
				#region Test
#if DEBUG
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					return IsDebugMode_ForTest.Value;
				}
#endif
				#endregion

#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}

#if DEBUG
		public static readonly Overridable<bool> IsDebugMode_ForTest = new Overridable<bool>(true);

		public static IDisposable TemporaryOverrideForIsDebugMode(bool value)
		{
			var oldValue = IsDebugMode_ForTest.Value;
			IsDebugMode_ForTest.Value = value;
			return new DisposableAction(() => IsDebugMode_ForTest.Value = oldValue);
		}
#endif

		#endregion

		#region IsTest

		/// <summary>
		/// Are we currently running the unit tests?
		/// </summary>
		public static bool IsTest
		{
			get
			{
				#region Test
#if DEBUG
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					return IsTest_ForTest.Value;
				}
#endif
				#endregion

				return false;
			}
		}

		#region InTransactionedTestCase

		/// <summary>
		/// Are we currently in a transaction test case?
		/// </summary>
		/// 

		public static readonly Overridable<bool> InTransactionedTestCase_ForTest = new Overridable<bool>(true);

		public static bool InTransactionedTestCase
		{
			get
			{
#if DEBUG
				if (NUnit.Framework.TransactionedTestCase.InTransactionedTestCase)
				{
					return InTransactionedTestCase_ForTest.Value;
				}
#endif

				return false;
			}
		}

		public static IDisposable TemporaryOverrideForInTransactionedTestCase(bool value)
		{
			var oldValue = InTransactionedTestCase_ForTest.Value;
			InTransactionedTestCase_ForTest.Value = value;
			return new DisposableAction(() => InTransactionedTestCase_ForTest.Value = oldValue);
		}

		#endregion

#if DEBUG
		public static readonly Overridable<bool> IsTest_ForTest = new Overridable<bool>(true);

		public static IDisposable TemporaryOverrideForIsTest(bool value)
		{
			var oldValue = IsTest_ForTest.Value;
			IsTest_ForTest.Value = value;
			return new DisposableAction(() => IsTest_ForTest.Value = oldValue);
		}
#endif

		#endregion

		#region IsWeb

		public static bool IsWeb
		{
			get
			{
				#region Test
#if DEBUG
				if (IsTest)
				{
					return isWeb.Value;
				}
#endif
				#endregion
				return EnvProxy.Instance.IsWeb;
			}
			#region Test
#if DEBUG
			set
			{
				if (IsTest)
				{
					isWeb.Value = value;
				}
			}
#endif
			#endregion
		}

		#endregion

		#region IsWebService

		public static bool IsWebService
		{
			get
			{
				#region Test
#if DEBUG
				if (IsTest)
				{
					return isWebService.Value;
				}
#endif
				#endregion
				return EnvProxy.Instance.IsWebService;
			}
			#region Test
#if DEBUG
			set
			{
				if (IsTest)
				{
					isWebService.Value = value;
				}
			}
#endif
			#endregion
		}

		#endregion

		#region IsWebServiceOrWeb

		public static bool IsWebServiceOrWeb => IsWebService || IsWeb;

		#endregion

		#endregion

		#region Message / Notifications
		/// <summary>
		/// Specifies whether the current session is Console mode
		/// </summary>
		[ThreadSafe] // Only ever set once, on startup, before other threads are made. (well, its supposed to be)
		static bool? isConsoleSession = null;
		public static bool IsConsoleSession
		{
			get
			{
#if DEBUG
				if (isConsoleSession_forTest.Value.HasValue)
				{
					return isConsoleSession_forTest.Value.Value;
				}
#endif
				if (isConsoleSession == null)
				{
					isConsoleSession = false;
				}
				return isConsoleSession.Value;
			}

			set
			{
#if DEBUG
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					isConsoleSession_forTest.Value = value;
				}
				else
#endif
				{
					isConsoleSession = value;
				}
			}
		}

		[ThreadSafe] // Only ever set once, on startup, before other threads are made. (well, its supposed to be)
		internal static bool? isUserInteractive;
		/// <summary>
		/// Specifies whether the current session has a GUI attached - ie the main application,
		/// not a Service Task / Background App Domain Worker etc.
		/// </summary>
		public static bool IsUserInteractive
		{
			get
			{
#if DEBUG
				if (isUserInteractive_forTest.Value.HasValue)
				{
					return isUserInteractive_forTest.Value.Value;
				}
#endif
				if (!isUserInteractive.HasValue)
				{
					isUserInteractive = true;
					ErrorReporter.ReportOnce("IsUserInteractiveProblem", "IsUserInteractive is being used before being set");
				}

				return isUserInteractive.Value;
			}

			set
			{
#if DEBUG
				if (NUnit.Framework.TestingState.IsRunningTests)
				{
					isUserInteractive_forTest.Value = value;
				}
				else
#endif
				{
					if ((!isUserInteractive.HasValue || isUserInteractive.Value) && !value)
					{
						isUserInteractiveStack = new StackTrace();
					}
					isUserInteractive = value;
				}
			}
		}

#if DEBUG
		static readonly Overridable<bool?> isConsoleSession_forTest = new Overridable<bool?>(null);
		static readonly Overridable<bool?> isUserInteractive_forTest = new Overridable<bool?>(null);

		public static IDisposable SetIsUserInteractiveForTest(bool value)
		{
			bool originalValue = IsUserInteractive;
			IsUserInteractive = value;
			return new DisposableAction(() => IsUserInteractive = originalValue);
		}

		public static IDisposable SetIsWebForTest(bool value)
		{
			bool originalValue = IsWeb;
			IsWeb = value;
			return new DisposableAction(() => IsWeb = originalValue);
		}

		public static IDisposable SetIsWinzorForTest(bool value)
		{
			bool originalValue = IsWinzor;
			IsWinzor = value;
			return new DisposableAction(() => IsWinzor = originalValue);
		}

		public static IDisposable SetClientIdentifierForTest(string value)
		{
			var originalValue = ClientIdentifier;
			ClientIdentifier = value;
			return new DisposableAction(() => ClientIdentifier = originalValue);
		}

		public static IDisposable SetIsConsoleSessionForTest(bool value)
		{
			bool originalValue = IsConsoleSession;
			IsConsoleSession = value;
			return new DisposableAction(() => IsConsoleSession = originalValue);
		}

		public static IDisposable SetIsUnitTestingProductionFunctionality()
		{
			Globals.SetIsUnitTestingProductionFunctionality(true);
			return new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(false));
		}

#endif

		public static bool CanShowDialogs => IsUserInteractive && ((!IsWeb && !IsConsoleSession && System.Environment.UserInteractive) || IsWinzor);

		public static StackTrace IsUserInteractiveStack => isUserInteractiveStack;
		[ThreadSafe]
		static StackTrace isUserInteractiveStack;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool IsWinzor { get; set; }

		public static string ClientIdentifier { get; set; }

#nullable enable
		/// <summary>
		/// If using Winzor, and if a client IP address is available, this will be set to the IP address of the client.
		/// It is not guaranteed to be set, and may be null. It is not guaranteed to always be up to date if the client changes IP address during a session.
		/// This will be the address as seen by the server's hosting environment. The client may have additional IP addresses, including behind a VPN or NAT, which will not be shown here.
		/// </summary>
		public static IPAddress? WinzorClientIpAddress { get; set; }
#nullable disable

		/// <summary>
		/// Create a message box at run time, else simply remember the last message during unit tests.
		/// </summary>
		public static IUserNotification Message
		{
			get
			{
				#region Testing
#if DEBUG
				if ((IsTest || !IsTest_ForTest.Value) && !isUnitTestingProductionFunctionality.Value) // we reset the overridable after checking if this is valid for testing.
				{
					return UnitTestUserNotification.Instance;
				}
				else
#endif
				#endregion
				{
					if (IsConsoleSession)
					{
						return CommandLineUserNotification.Instance;
					}
					else if (CanShowDialogs)
					{
						if (InteractiveNotification == null)
						{
							throw new ApplicationException("Please setup notification instance for interactive mode"); // exception message
						}

						return InteractiveNotification;
					}
					else
					{
						return UnattendedUserNotification.Instance;
					}
				}
			}
		}

		#region IsUnitTestingProductionFunctionality
#if DEBUG
		static readonly Overridable<bool> isUnitTestingProductionFunctionality = new Overridable<bool>(false);

		public static void SetIsUnitTestingProductionFunctionality(bool value)
		{
			isUnitTestingProductionFunctionality.Value = value;
		}
#endif
		public static bool GetIsUnitTestingProductionFunctionality()
		{
#if DEBUG
			return isUnitTestingProductionFunctionality.Value;
#else
			return false;
#endif
		}

		#endregion

		[SuppressThreadStaticFieldMessage]
		public static IUserNotification InteractiveNotification;

		#endregion

		#region IsDBUpgSkipped
		public static bool fIsDBUpgSkipped;
		public static bool IsDBUpgSkipped
		{
			get { return fIsDBUpgSkipped; }
			set { fIsDBUpgSkipped = value; }
		}

		#endregion

		#region Test
#if DEBUG

		static readonly Overridable<bool> isWeb = new Overridable<bool>(false);

		static readonly Overridable<bool> isWebService = new Overridable<bool>(false);

#endif
		#endregion
	}
}
