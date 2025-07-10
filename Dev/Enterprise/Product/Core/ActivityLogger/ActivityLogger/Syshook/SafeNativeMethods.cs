using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ActivityLogger
{
	#region SyshookInitWrapper

	[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "It IS instantiated, in the static cstr for the LibraryInitialisation variable. Overridable hides this from the analyser.")]
	internal class InitWrapper
	{
		public InitWrapper()
		{
			SafeNativeMethods.LoadLibrary();
		}

		~InitWrapper()
		{
			SafeNativeMethods.FreeLibrary();
		}
	}

	#endregion

	#region SafeNativeMethods

	/// <summary>
	/// Any C#/ActivityLogger code hoping to interact with native Win32 or Syshook methods should write a safe wrapper here and call it.
	/// </summary>
	internal static class SafeNativeMethods
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "This is a hack to call FreeLibrary. When the application is closed the GC will run, calling the InitWrapper destructor and destroying syshook")]
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "It is used - we need it to stay in scope while application is open")]
		[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "It is used - we need it to stay in scope while application is open")]
		static readonly InitWrapper LibraryInitialisation;

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We must load the syshook library before we access it, and there is no better time to do so than when creating the class. Creating it inline failed because the field was never accessed (and thus never initialised).")]
		static SafeNativeMethods()
		{
			LibraryInitialisation = new InitWrapper();
		}

		[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "The design has ensured there are no thread issues with this. Additionally, alternatives to this such as Overridable do not work.")]
		static IntPtr SyshookDllHandle;

		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "It IS called, in the cstr for the Overridable<InitWrapper> but the analyser doesn't know this.")]
		internal static void LoadLibrary()
		{
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var libPath = Path.Combine(binFolder ?? string.Empty, SyshookInterop.DllName);

			var handle = UnsafeNativeMethods.LoadLibraryW(libPath);
			var lastError = Marshal.GetLastWin32Error();
			if (handle == IntPtr.Zero)
			{
				throw new FileLoadException($"LoadLibrary failed: can't load DLL: {libPath}", new Win32Exception(lastError));
			}

			SyshookDllHandle = handle;
		}

		internal static void FreeLibrary()
		{
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			var result = UnsafeNativeMethods.FreeLibrary(SyshookDllHandle);
			var lastError = Marshal.GetLastWin32Error();
			if (!result)
			{
				var lastEx = new Win32Exception(lastError);
				var libPath = Path.Combine(binFolder ?? string.Empty, SyshookInterop.DllName);
				throw new FileLoadException($"FreeLibrary failed: can't free DLL {libPath}", lastEx);
			}

			SyshookDllHandle = IntPtr.Zero;
		}

		public static SyshookInterop.ReturnCode AttachHooks()
		{
			var result = (SyshookInterop.ReturnCode)UnsafeNativeMethods.AttachHooks();
			if (result != SyshookInterop.ReturnCode.ERROR_SUCCESS)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), $"AttachHooks failed: {result}");
			}

			return result;
		}

		public static SyshookInterop.ReturnCode DetachHooks()
		{
			var result = (SyshookInterop.ReturnCode)UnsafeNativeMethods.DetachHooks();
			if (result != SyshookInterop.ReturnCode.ERROR_SUCCESS)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), $"DetachHooks failed: {result}");
			}

			return result;
		}

		public static string GetSyshookPath()
		{
			var maxPath = UnsafeNativeMethods.GetMaxPath();
			var sb = new StringBuilder(maxPath);

			var result = UnsafeNativeMethods.GetSyshookPath(sb, maxPath);
			var lastError = Marshal.GetLastWin32Error();
			if (result < 0)
			{
				throw new Win32Exception(lastError, $"GetSyshookPath failed: {(SyshookInterop.ReturnCode)result}");
			}
			else if (result > 0 && lastError != SyshookInterop.ERROR_SUCCESS)
			{
				throw new Win32Exception(lastError, $"GetSyshookPath failed: {result}");
			}

			return sb.ToString();
		}

		public static string GetWindowText(IntPtr hWnd)
		{
			var length = UnsafeNativeMethods.GetWindowTextLengthW(hWnd);
			var sb = new StringBuilder(length + 1);

			var result = UnsafeNativeMethods.GetWindowTextW(hWnd, sb, sb.Capacity);
			var lastError = Marshal.GetLastWin32Error();
			if (result == 0 && lastError != SyshookInterop.ERROR_SUCCESS)
			{
				throw new Win32Exception(lastError, $"GetWindowText failed: {result}");
			}

			return sb.ToString();
		}

		public static HookerEvent WaitForEvent(Int32 milliseconds)
		{
			var message = new HookMessage();
			var dwResult = UnsafeNativeMethods.WaitForBlock(milliseconds <= 0 ? SyshookInterop.INFINITE : milliseconds, ref message);

			switch (dwResult)
			{
				case SyshookInterop.WAIT_OBJECT_0:
					return new HookerEvent(message);
				case SyshookInterop.WAIT_TIMEOUT:
					return HookerEvent.Timeout;
				default:
					return HookerEvent.Unknown;
			}
		}
	}

	#endregion

	#region SyshookInterop

	internal static class SyshookInterop
	{
		public const string DllName = "Enterprise.ActivityLogger.Native.Syshook.dll";

		#region Windows

		#region Hook Types

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 WH_MOUSE = 7;

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 WH_KEYBOARD = 2;

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 WH_CBT = 5;

		#endregion

		#region MouseHook

		[CodeAlive("Broken reflection test thinks this is unused")]
		[SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
		public enum WM_MouseMessage : Int32
		{
			MouseMove				= 0x200, // WM_MOUSEMOVE
			LeftButtonDown			= 0x201, // WM_LBUTTONDOWN
			LeftButtonUp			= 0x202, // WM_LBUTTONUP
			LeftButtonDoubleClick	= 0x203, // WM_LBUTTONDBLCLK
			RightButtonDown			= 0x204, // WM_RBUTTONDOWN
			RightButtonUp			= 0x205, // WM_RBUTTONUP
			RightButtonDoubleClick	= 0x206, // WM_RBUTTONDBLCLK
			MiddleButtonDown		= 0x207, // WM_MBUTTONDOWN
			MiddleButtonUp			= 0x208, // WM_MBUTTONUP
			MiddleButtonDoubleClick = 0x209, // WM_MBUTTONDBLCLK
			MouseWheel				= 0x20A, // WM_MOUSEWHEEL
			MouseHWheel				= 0x20E, // WM_MOUSEHWHEEL
		}

		#endregion

		#region CBTHook

		[CodeAlive("Broken reflection test thinks this is unused")]
		public enum HCBT
		{
			MoveSize = 0,
			MinMax = 1,
			QueueSync = 2,
			CreateWnd = 3,
			DestroyWnd = 4,
			Activate = 5,
			ClickSkipped = 6,
			KeySkipped = 7,
			SysCommand = 8,
			SetFocus = 9
		}

		#endregion

		#region Windows Message Codes

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 WAIT_OBJECT_0 = 0;

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 WAIT_TIMEOUT = 258;

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 MAX_PATH = 260;

		public const Int32 INFINITE = unchecked((Int32)0xFFFFFFFF); // this is meant to 'overflow'

		[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
		public const Int32 ERROR_SUCCESS = 0;

		#endregion

		#endregion

		#region Syshook

		public enum ReturnCode : Int32
		{
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERROR_SUCCESS = 0,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERROR_NULL_MUTEX_HANDLE = -1,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERROR_FILE_ALREADY_EXISTS = -2,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERROR_COULD_NOT_UNHOOK_A_HOOK = -3,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERROR_FAILED_TO_CLOSE_MUTEX_HANDLE = -4,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERR_CANT_GET_MODULE_NAME = -5,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERROR_NULL_SYSHOOK_DLL_HANDLE = -6,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERR_COULD_NOT_OPEN_SHM_CHANNEL = -7,
			[SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
			ERR_COULD_NOT_CLOSE_SHM_CHANNEL = -8
		}

		public enum HookerEventType
		{
			TimeOut,
			Keyboard,
			Mouse,
			Cbt,
			Unknown
		}

		#endregion
	}

	#endregion
}
